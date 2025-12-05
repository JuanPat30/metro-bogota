using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ServiceLayer.Service
{
    public class RsaKeyService
    {
        public RSA RsaKey { get; }

        public RsaKeyService(IConfiguration configuration)
        {
            // Intenta primero leer la clave desde una variable o valor directo
            var privateKeyPem = configuration["RsaKeys:PrivatePem"];

            if (!string.IsNullOrWhiteSpace(privateKeyPem))
            {
                RsaKey = LoadFromPemString(privateKeyPem);
                return;
            }

            // Si no se encuentra en texto, intenta leer desde archivo
            var privateKeyPath = configuration["RsaKeys:PrivatePath"];

            if (string.IsNullOrWhiteSpace(privateKeyPath))
            {
                throw new ArgumentException("No se ha configurado ni la clave PEM ni la ruta al archivo.");
            }

            if (privateKeyPath.StartsWith("Resources:", StringComparison.OrdinalIgnoreCase))
            {
                var resourcesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                var relativePath = privateKeyPath["Resources:".Length..].TrimStart('/', '\\');
                privateKeyPath = Path.Combine(resourcesDirectory, relativePath);
            }

            if (!File.Exists(privateKeyPath))
            {
                throw new FileNotFoundException($"No se encontró el archivo de clave privada en: {privateKeyPath}");
            }

            var fileContent = File.ReadAllText(privateKeyPath);
            RsaKey = LoadFromPemString(fileContent);
        }

        private static RSA LoadFromPemString(string pem)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(pem.ToCharArray());
                return rsa;
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Error al importar la clave RSA desde texto PEM.", ex);
            }
        }
    }

}
