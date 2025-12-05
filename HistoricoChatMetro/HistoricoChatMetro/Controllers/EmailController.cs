using DomainLayer.Dtos;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.IService;
using ServiceLayer.Service;

namespace HistoricoChatMetro.Controllers
{
    /// <summary>
    /// Gabriela Muñoz
    /// Controlador para el envio del correo
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="emailService"></param>
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que envia el email con los detalles de la conversación
        /// </summary>
        /// <param name="consult"></param>
        /// <returns>Result</returns>
        [HttpPost("SendEmail")]
        public Result SendEmail([FromBody] ConsultDto consult)
        {
            try
            {
                var email = User.GetEmail();
                var result = _emailService.SendEmail(email, consult.ConversationId);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }
    }
}
