using Commun.Helpers;
using Commun.Logger;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.IService;
using ServiceLayer.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HistoricoChatMetro.Controllers
{
    /// <summary>
    /// Gabriela Muñoz
    /// Controlador para el token
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IAuthToken _authToken;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="authToken"></param>
        public TokenController(IAuthToken authToken)
        {
            _authToken = authToken;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que genera el token para acceder a los demás metodos
        /// </summary>
        /// <param name="authModel"></param>
        /// <returns>TokenResult</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateToken")]
        public ActionResult<TokenResults> GenerateWebToken(AuthModel authModel)
        {
            try
            {
                var (token, expiresAt) = _authToken.GenerarToken(authModel);
                /*
                Response.Cookies.Append(JwtSettings.Jwt_AuthCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiresAt,
                    Domain = host,
                    Secure = Request.IsHttps,
                    Path = "/",
                });
                */

                return Ok(new TokenResults
                {
                    isSuccess = true,
                    Token = token,
                    expiresAt = expiresAt.ToString(),
                    Type = "Bearer",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Error al generar el token. Verifique sus credenciales.",
                    error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("ValidateToken")]
        public ActionResult ValidateToken()
        {
            var email = User.GetEmail();
            var role = User.GetRole();

            return Ok(new
            {
                isValid = true,
                message = "Token is valid.",
                email,
                role,
            });
        }

        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(JwtSettings.Jwt_AuthCookieName, new CookieOptions
            {
                Domain = null,
                Path = null,
                Secure = Request.IsHttps,
            });
            return Ok(new { message = "Logged out" });
        }
    }
}
