using Commun.Helpers;
using Commun.Logger;
using DomainLayer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.IService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para el servicio de token
    /// </summary>
    
    internal class AuthTokenData
    {
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? user.FindFirst("email")?.Value;

            return email ?? throw new InvalidOperationException("No se encontró un email válido en los claims.");
        }

        public static string? GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value;
        }
    }

    public class AuthToken : IAuthToken
    {
        private readonly IConfiguration configuration;
        private readonly ICreateLogger createLogger;
        private readonly RsaKeyService rsaKeyService;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="_configuration"></param>
        /// <param name="_createLogger"></param>
        public AuthToken(
            IConfiguration _configuration, 
            ICreateLogger _createLogger,
            RsaKeyService _rsaKeyService)
        {
            configuration = _configuration;
            createLogger = _createLogger;
            rsaKeyService = _rsaKeyService; 
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para generar el token
        /// </summary>
        /// <param name="authModel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public Tuple<string, DateTime> GenerarToken(AuthModel authModel)
        {
            try
            {
                if (authModel == null)
                {
                    createLogger.LogWriteExcepcion($"El modelo de autenticación no puede ser nulo. {authModel}");
                    throw new ArgumentNullException(nameof(authModel), "El modelo de autenticación no puede ser nulo.");
                }

                if (string.IsNullOrWhiteSpace(authModel.Credential))
                {
                    createLogger.LogWriteExcepcion("La credencial no puede ser nula o vacía.");
                    throw new ArgumentException("La credencial no puede ser nula o vacía.", nameof(authModel.Credential));
                }

                string SecretPassword = JwtSettings.Jwt_SecretPassword;
                string Issuer = JwtSettings.Jwt_Issuer;
                string Audience = JwtSettings.Jwt_Audience;
                string Expire = JwtSettings.Jwt_Expire;

                if (string.IsNullOrWhiteSpace(SecretPassword))
                {
                    createLogger.LogWriteExcepcion("La contraseña secreta del token no está configurada.");
                    throw new InvalidOperationException("La contraseña secreta del token no está configurada.");
                }

                var tokenData = GetTokenData(authModel.Credential);

                if (tokenData == null || string.IsNullOrWhiteSpace(tokenData.email))
                {
                    createLogger.LogWriteExcepcion("Los datos del token no son válidos o el email está vacío.");
                    throw new UnauthorizedAccessException("Los datos del token no son válidos o el email está vacío.");
                }

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretPassword));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(signingCredentials);
                var expires = DateTime.Now.AddMinutes(Convert.ToInt32(Expire));

                var payload = new JwtPayload(
                    issuer: Issuer,
                    audience: Audience,
                    claims: [
                        new Claim(JwtRegisteredClaimNames.Email, tokenData.email),
                        new Claim(ClaimTypes.Email, tokenData.email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        // 🔐 Add roles here
                        new Claim(ClaimTypes.Role, tokenData.role),
                        // new Claim(ClaimTypes.Role, "User"), // for multiple roles
                    ],
                    notBefore: DateTime.Now,
                    expires: expires
                );

                var token = new JwtSecurityToken(header, payload);

                return Tuple.Create(new JwtSecurityTokenHandler().WriteToken(token), expires);
            }
            catch (Exception ex)
            {
                createLogger.LogWriteExcepcion($"Ocurrió un error al generar el token de autenticación. {ex}");
                throw new Exception("Ocurrió un error al generar el token de autenticación.", ex);
            }
        }

        private AuthTokenData GetTokenData(String credential)
        {
            // 1. Decode from Base64
            byte[] encryptedBytes = Convert.FromBase64String(credential);

            // 2. Decrypt using RSA
            byte[] decryptedBytes = this.rsaKeyService.RsaKey.Decrypt(
                encryptedBytes,
                RSAEncryptionPadding.OaepSHA256
            );

            // 3. Convert bytes to JSON string
            string json = Encoding.UTF8.GetString(decryptedBytes);

            // 4. Deserialize to object
            var data = JsonSerializer.Deserialize<AuthTokenData>(json);

            // 5. Trim email (just in case)
            if (data != null)
            {
                data.email = data.email?.Trim() ?? string.Empty;
                data.role = data.role?.Trim() ?? string.Empty;
            }

            return data!;
        }
    }
}
