using RepositoryLayer.Data;
using ServiceLayer.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service
{
    public class FileStoreService : IFileStoreService
    {
        private readonly CloudStorageClient _cloudStorageClient;

        public FileStoreService(CloudStorageClient cloudStorageClient)
        {
            _cloudStorageClient = cloudStorageClient;
        }

        public async Task<string> UploadFileAsync(string bucket, string localPath, string objectName)
        {
            var storage = _cloudStorageClient.GetStorageClient();

            // Verifica si el archivo local existe
            if (!File.Exists(localPath))
            {
                throw new FileNotFoundException($"El archivo no fue encontrado en la ruta: {localPath}");
            }

            // Abre un stream para leer el archivo local
            using (var fileStream = File.OpenRead(localPath))
            {
                try
                {
                    // Sube el archivo
                    // Puedes agregar el tipo de contenido si lo conoces para mejorar la gestión en GCS.
                    // var contentType = "application/octet-stream"; // O un tipo más específico
                    await storage.UploadObjectAsync(
                        bucket,
                        objectName,
                        null, // contentType - null para que GCS intente detectarlo o use el default
                        fileStream);

                    // Construye y retorna la URI de GCS
                    return $"gs://{bucket}/{objectName}";
                }
                catch (Google.GoogleApiException ex)
                {
                    // Maneja errores específicos de la API de Google
                    Console.WriteLine($"Error de Google API al subir '{objectName}' al bucket '{bucket}': {ex.Message}");
                    throw; // O maneja de forma más específica
                }
                catch (Exception ex)
                {
                    // Maneja otros errores
                    Console.WriteLine($"Error inesperado al subir '{objectName}': {ex.Message}");
                    throw;
                }
            }
        }
    }
}
