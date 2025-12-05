using AutoMapper;
using Commun;
using Commun.Helpers;
using Commun.Logger;
using DomainLayer.Dtos;
using DomainLayer.Models;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ServiceLayer.IService;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para enviar un correo
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IChatService _chatService;
        private readonly IConfiguration _config;
        private readonly ICreateLogger _createLogger;
        private readonly IMapper _mapper;
        private readonly IFileWrapper _fileWrapper;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="chatService"></param>
        /// <param name="config"></param>
        /// <param name="createLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="fileWrapper"></param>
        public EmailService(IChatService chatService, IConfiguration config, ICreateLogger createLogger, IMapper mapper, IFileWrapper fileWrapper)
        {
            _chatService = chatService;
            _config = config;
            _createLogger = createLogger;
            _mapper = mapper;
            _fileWrapper = fileWrapper;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para enviar el correo al destinatario con los detalles de la conversacion
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>Result</returns>
        public async Task<Result> SendEmail(string userId, string conversationId)
        {
            Result result = new Result();
            try
            {
                TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
                var conversation = await _chatService.GetConversationById(userId, conversationId);

                ConversationDto conversationDto = _mapper.Map<ConversationDto>(conversation.Data);

                if (conversationDto == null)
                {
                    return ValidationConversation();
                }

                var date = conversationDto.Date != null ? TimeZoneInfo.ConvertTimeFromUtc(conversationDto.Date!.Value, zonaHoraria).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : "";

                string projectRoot = Directory.GetCurrentDirectory();
                string fullPath = Path.Combine(projectRoot, "MailTemplates", "TemplateMail.html");

                string htmlTemplate = _fileWrapper.ReadAllText(fullPath);

                StringBuilder messagesHtml = BuildMessages(conversationDto);

                string statusClass = conversationDto.Estado ? "status-activo" : "status-inactivo";

                htmlTemplate = htmlTemplate
                    .Replace("{{status}}", conversationDto.Estado ? "Activo" : "Inactivo")
                    .Replace("{{statusClass}}", statusClass)
                    .Replace("{{date}}", date)
                    .Replace("{{userName}}", userId)
                    .Replace("{{messages}}", messagesHtml.ToString());

                // userId = "einer@servinformacion.com"; // For testing purposes, replace with actual userId if needed.

                var email = BuildEmail(userId, date, htmlTemplate);

                using var smtp = new SmtpClient();

                var host = _config.GetSection("Email:Host").Value;

                smtp.Connect(
                    _config.GetSection("Email:Host").Value,
                    Convert.ToInt32(_config.GetSection("Email:Port").Value),
                    SecureSocketOptions.StartTls
                    );
                smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
                smtp.Send(email);
                smtp.Disconnect(true);

                result.Success = true;
                result.MessageHttp = Constants.msjMs200;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                result.Success = false;
                result.MessageHttp = "Error al enviar a uno o más destinatarios.";
                foreach (SmtpFailedRecipientException recEx in ex.InnerExceptions)
                {
                    _createLogger.LogWriteExcepcion($"Error al enviar al destinatario {recEx.FailedRecipient}: {recEx.Message}");
                }
            }
            catch (SmtpException ex)
            {
                _createLogger.LogWriteExcepcion($"Error SMTP: {ex.StatusCode} - {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _createLogger.LogWriteExcepcion($"Operación inválida: {ex.Message}");
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion($"Ocurrió un error inesperado: {ex.Message}");
            }
            return result;

        }

        /// <summary>
        /// Gabriela Muñoz
        /// Validación interna cuando no existe la conversación
        /// </summary>
        /// <returns></returns>
        private Result ValidationConversation()
        {
            Result result = new();

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = Constants.CONV_NO_EXIST;

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Construcción del mensaje
        /// </summary>
        /// <param name="conversationDto"></param>
        /// <returns></returns>
        private StringBuilder BuildMessages(ConversationDto conversationDto)
        {
            StringBuilder messagesHtml = new StringBuilder();
            conversationDto.Messages?.ForEach(message =>
            {
                string messageAsHtml = MarkdownUtils.ToHtml(message.Message);
                if (!message.IdPersona.ToLower().Equals("chatbot"))
                {
                    messagesHtml.Append($@"
                    <div class='section'>
                        <h3>Consulta:</h3>
                        <p>{messageAsHtml}</p>
                    </div>");
                }
                else
                {
                    messagesHtml.Append($@"
                    <div class='section response'>
                        <h3>Respuesta:</h3>
                        <p>{messageAsHtml}</p>
                    </div>");
                }
            });
            return messagesHtml;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Construcción del correo
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="date"></param>
        /// <param name="htmlTemplate"></param>
        /// <returns></returns>
        private MimeMessage BuildEmail(string userId, string date, string htmlTemplate)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(userId));
            email.Subject = $"Conversación chatbot {date}";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlTemplate
            };

            return email;
        }
    }
}