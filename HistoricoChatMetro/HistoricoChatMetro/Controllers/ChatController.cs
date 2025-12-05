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
    /// Controlador para el chat
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICreateLogger _createLogger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="chatService"></param>
        /// <param name="mapper"></param>
        /// <param name="createLogger"></param>
        public ChatController(IChatService chatService, IMapper mapper, ICreateLogger createLogger)
        {
            _chatService = chatService;
            _mapper = mapper;
            _createLogger = createLogger;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para consultar las conversaciones por usuario con filtro
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>Result</returns>

        //[Authorize(Roles = "Administrador")]
        [HttpGet("GetConversationByUser")]
        public Result GetConversationByUser([FromQuery] string userId,
            [FromQuery] string? name = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                if ( User.GetRole() != "Administrador")
                {
                    userId = User.GetEmail();
                }
                var result = _chatService.GetConversationByUser(userId, name, from, to);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        [HttpGet("MeConversations")]
        public Result MeConversations(
            [FromQuery] string? name = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.GetConversationByUser(email, name, from, to);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para consultar las conversaciones por ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>Result</returns>
        [HttpGet("GetConversationById")]
        public Result GetConversationById([FromQuery] string conversationId)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.GetConversationById(email, conversationId);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para crear y actualizar una conversación
        /// </summary>
        /// <param name="consult"></param>
        /// <returns>Result</returns>
        [HttpPost]
        [Route("SaveConversation")]
        public Result SaveConversation([FromBody] ConsultDto consult)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.SaveConversation(email, consult.Conversation!);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para eliminar mensajes de conversaciones
        /// </summary>
        /// <param name="consult"></param>
        /// <returns>Result</returns>
        [HttpPut("UpdateMessages")]
        public Result UpdateMessages([FromBody] ConsultDto consult)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.UpdateMessages(email, consult);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para eliminar logicamente una conversación
        /// </summary>
        /// <param name="consult"></param>
        /// <returns>Result</returns>
        [HttpPut("DeleteConversation")]
        public Result DeleteConversation([FromBody] ConsultDto consult)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.DeleteConversation(email, consult.ConversationId, consult.Status??true);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        [HttpPut("DeleteConversationsFromIds")]
        public Result DeleteConversationsFromIds([FromBody] QueryConversationsByIds query)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.DeleteConversations(email, query.ConversationIds, query.Status ?? true);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        [HttpPut("DeleteConversationsFromSearch")]
        public Result DeleteConversationsFromSearch([FromBody] QueryConversationBySearch query)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.DeleteConversations(email, query.Name, query.From, query.To);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para actualizar favoritos en mensajes
        /// </summary>
        /// <param name="consult"></param>
        /// <returns>Result</returns>
        [HttpPut("UpdateFieldMessages")]
        public Result UpdateFieldMessages([FromBody] ConsultDto consult)
        {
            try
            {
                var email = User.GetEmail();
                var result = _chatService.UpdateFieldMessages(email, consult.ConversationId, consult.Messages!.FirstOrDefault()!);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }

        [HttpPut("UpdateFieldConversation")]
        public Result UpdateFieldsConversation([FromBody] UpdateFieldConversation consult)
        {
            try
            {
                if (consult.ModeloDocumento == null)
                {
                    return Result.CreateMessage(false, "No hay campos para actualizar", null);
                }
                var email = User.GetEmail();
                var result = _chatService.UpdateFieldConversation(email, consult.ConversationId, consult.ModeloDocumento);
                return Result.CreateMessage(true, result.Result.MessageHttp, result.Result.Data);
            }
            catch (Exception ex)
            {
                return Result.CreateMessage(false, ex.Message, null);
            }
        }
    }
}
