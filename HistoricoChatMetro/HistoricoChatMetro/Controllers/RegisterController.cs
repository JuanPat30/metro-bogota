using DomainLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.IService;

namespace HistoricoChatMetro.Controllers
{
    /// <summary>
    /// Gabriela Muñoz
    /// Controlador para la bitacora
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterService _registerService;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="registerService"></param>
        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que devuelve la lista de usuarios
        /// </summary>
        /// <returns>Result</returns>
        [Authorize(Roles = "Administrador")]
        [HttpGet("GetUsers")]
        public Result GetUsers()
        {
            try
            {
                var result = _registerService.GetUsers();
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que retorna todas las conversaciones de todos los usuarios con paginacíón
        /// y aplica filtros por usuario, paginacion, fechas, estado y ordenamiento
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="status"></param>
        /// <param name="isDescending"></param>
        /// <returns>Result</returns>
        [Authorize(Roles = "Administrador")]
        [HttpGet("GetAll")]
        public Result GetAll([FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? name = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null, 
            [FromQuery] bool? status = null, 
            [FromQuery] bool? isDescending = null)
        {
            try
            {
                var result = _registerService.GetAll(page, pageSize, name, from, to, status, isDescending);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }
    }
}
