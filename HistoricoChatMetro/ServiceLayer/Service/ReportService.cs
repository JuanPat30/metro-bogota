using AutoMapper;
using Commun;
using Commun.Logger;
using DomainLayer.Dtos;
using DomainLayer.Models;
using RepositoryLayer.IRepository;
using ServiceLayer.IService;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para generar los reportes en excel y PDF
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly ICreateLogger _createLogger;
        private readonly IFileWrapper _fileWrapper;
        private readonly ReportExcel _reportExcel;
        private readonly PDFGenerator _pdfGenerator;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="chatRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="createLogger"></param>
        /// <param name="fileWrapper"></param>
        /// <param name="reportExcel"></param>
        /// <param name="pdfGenerator"></param>
        public ReportService(IChatRepository chatRepository, IMapper mapper, ICreateLogger createLogger, IFileWrapper fileWrapper, ReportExcel reportExcel, PDFGenerator pdfGenerator)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
            _createLogger = createLogger;
            _fileWrapper = fileWrapper;
            _reportExcel = reportExcel;
            _pdfGenerator = pdfGenerator;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para generar reporte en excel
        /// </summary>
        /// <param name="reportDto"></param>
        /// <returns>Result</returns>
        /// <exception cref="Exception"></exception>
        public Result GetReportExcel(PaginatedResponseDto<ConversationsUserDto> reportDto)
        {
            Result result = new Result();
            try
            {
                
                string base64 = _reportExcel.GenereteExcel(reportDto.Items);

                result.Success = true;
                result.MessageHttp = Constants.msjMs200;
                result.Data = base64;

                string outputPathDesktop = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
                string outputPath = Path.Combine(outputPathDesktop, $"Reporte Conversaciones.xlsx");
                _fileWrapper.Delete(outputPath);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.MessageHttp = "Exception was found" + ex.Message;
                result.Data = null;

                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para generar reporte en PDF
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>Result</returns>
        public async Task<Result> GeneratePdf(string userId, string conversationId)
        {
            Result results = new Result();

            try
            {
                ConversationDto? conversation = await _chatRepository.GetConversationById(userId, conversationId);

                string base64 = _pdfGenerator.GeneratePdf(conversation!, userId);

                if (conversation == null)
                {
                    results.Success = true;
                    results.MessageHttp = Constants.msjMs204;
                    results.Data = Constants.CONV_NO_EXIST;
                    return results;
                }

                results.Success = true;
                results.MessageHttp = "200 - Ok";
                results.Data = base64;

                string outputPathDesktop = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
                string outputPath = Path.Combine(outputPathDesktop, $"Conversation {conversation!.UuidConversation}.pdf");
                _fileWrapper.Delete(outputPath);
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.MessageHttp = "Exception was found" + ex.Message;
                results.Data = null;

                _createLogger.LogWriteExcepcion(ex.Message);
                throw;
            }

            return results;
        }
    }
}