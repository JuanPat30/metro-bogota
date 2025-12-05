using AutoMapper;
using Commun.Logger;
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
    /// Controlador para la generación de reportes
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ICreateLogger createLogger;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="reportService"></param>
        /// <param name="_createLogger"></param>
        public ReportController(IReportService reportService,
             ICreateLogger _createLogger)
        {
            _reportService = reportService;
            createLogger = _createLogger;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que genera el reporte en excel de la bitacora por pagina
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        [HttpPost("GenerateExcel")]
        public Result GenerateExcel([FromBody] PaginatedResponseDto<ConversationsUserDto> report)
        {
            try
            {
                var base64FileData = _reportService.GetReportExcel(report);
                return Result.CreateMessage(true, base64FileData.MessageHttp, base64FileData.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que genera el reporte en Pdf para los detalles de la conversación
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GeneratePdf")]
        public Result GeneratePdf([FromQuery] string conversationId)
        {
            try
            {
                var userId = User.GetEmail();
                var result = _reportService.GeneratePdf(userId, conversationId);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

    }
}
        