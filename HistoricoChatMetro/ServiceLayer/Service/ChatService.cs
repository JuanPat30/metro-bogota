using AutoMapper;
using Commun;
using DomainLayer.Dtos;
using DomainLayer.Models;
using RepositoryLayer.IRepository;
using ServiceLayer.IService;
using System.Reflection;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para el servicio del Chat
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="chatRepository"></param>
        /// <param name="mapper"></param>
        public ChatService(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que retorna las conversaciones por usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>Result</returns>
        public async Task<Result> GetConversationByUser(string userId, string? name = null, DateTime? from = null, DateTime? to = null)
        {
            Result result = new Result();
            TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

            List<ConversationDto> conversations = await _chatRepository.GetConversationByUser(userId, name, from, to);

            if (conversations == null)
            {
                return ValidationConversation();
            }
            conversations.ForEach(x =>
            {
                x.Date = TimeZoneInfo.ConvertTimeFromUtc(x.Date!.Value, zonaHoraria);
                x.Messages?.ForEach(m =>
                {
                    m.Fecha = TimeZoneInfo.ConvertTimeFromUtc(m.Fecha!.Value, zonaHoraria);
                });
            });


            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = conversations;


            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que retorna una conversacion por Uuid
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>Result</returns>
        public async Task<Result> GetConversationById(string userId, string conversationId)
        {
            Result result = new Result();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(conversationId))
            {
                result.Success = true;
                result.MessageHttp = Constants.msjMs200;
                result.Data = $"{Constants.PARAMS_REQUIRED} userId, conversationId";

                return result;
            }
            TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

            ConversationDto? conversation = await _chatRepository.GetConversationById(userId, conversationId);
            if (conversation != null)
            {
                conversation.Date = TimeZoneInfo.ConvertTimeFromUtc(conversation.Date!.Value, zonaHoraria);
                conversation.Messages?.ForEach(x =>
                {
                    x.Fecha = TimeZoneInfo.ConvertTimeFromUtc(x.Fecha!.Value, zonaHoraria);
                });
                result.Success = true;
                result.MessageHttp = Constants.msjMs200;
                result.Data = conversation;
            }
            else
            {
                result.Success = true;
                result.MessageHttp = Constants.msjMs204;
                result.Data = conversation;
            }


                return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que almacena una conversación y actualiza una conversación
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationDto"></param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Result> SaveConversation(string userId, ConversationDto conversationDto)
        {
            Result result = new Result();

            if (conversationDto == null)
            {
                throw new ArgumentNullException("entity");
            }

            var conversationRef = _chatRepository.GetConversationByIdReference(userId, conversationDto.UuidConversation);
            IDocumentSnapshotWrapper snapshot = await conversationRef.GetSnapshotAsync();


            Conversation conversation = snapshot.ConvertTo<Conversation>();
            ConversationDto existConversation = _mapper.Map<ConversationDto>(conversation);

            if (existConversation == null)
            {
                await InserConversationAsync(userId, conversationDto);
            }
            else
            {
                await UpdateConversation(userId, conversationDto, existConversation, conversationRef);

            }

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método interno para insertar conversación
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationDto"></param>
        /// <returns></returns>
        private async Task InserConversationAsync(string userId, ConversationDto conversationDto)
        {
            conversationDto.Date = DateTime.UtcNow;
            conversationDto.Estado = true;
            conversationDto.Messages ??= new List<MessageChatBotDto>();
            await _chatRepository.Insert(userId, conversationDto);
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método interno que actualiza una conversacion en base a los campos de llegada
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationDto"></param>
        /// <param name="existConversation"></param>
        /// <param name="conversationRef"></param>
        /// <returns></returns>
        private async Task UpdateConversation(string userId, ConversationDto conversationDto, ConversationDto existConversation, IDocumentReferenceWrapper conversationRef)
        {
            foreach (var prop in typeof(ConversationDto).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name == "UuidConversation" || prop.Name == "Messages" || prop.Name == "Estado")
                    continue;
                var newValue = prop.GetValue(conversationDto);
                if (newValue != null && !string.IsNullOrEmpty(newValue.ToString()) && !(prop.PropertyType == typeof(DateTime?) && newValue == null) && !(prop.PropertyType == typeof(bool?) && newValue == null))
                {
                    if (prop.PropertyType.IsAssignableFrom(newValue.GetType()))
                    {
                        prop.SetValue(existConversation, newValue);
                    }
                }
            }
            if (conversationDto.Messages != null)
            {
                existConversation.Messages ??= new List<MessageChatBotDto>();
                conversationDto.Messages?.ForEach(m =>
                {
                    m.Uuid = Guid.NewGuid().ToString();
                    m.Fecha = DateTime.UtcNow;
                });

                existConversation.Messages.AddRange(conversationDto.Messages);
            }
            await _chatRepository.Update(userId, conversationDto.UuidConversation, conversationRef, existConversation);
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que edita los mensajes eliminandolos los mensajes antigüos
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>Result</returns>
        public async Task<Result> UpdateMessages(string userId, string conversationId)
        {
            Result result = new Result();
            var conversationRef = _chatRepository.GetConversationByIdReference(userId, conversationId);
            IDocumentSnapshotWrapper snapshot = await conversationRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return ValidationConversation();
            }

            Conversation existConversation = snapshot.ConvertTo<Conversation>();
            if (existConversation.Messages == null || existConversation.Messages.Count < 0)
            {
                return ValidationMessages();
            }
            ConversationDto conversationDto = _mapper.Map<ConversationDto>(existConversation);

            conversationDto.Messages ??= new List<MessageChatBotDto>();
            conversationDto.Messages.Clear();

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = await _chatRepository.UpdateMessages(conversationRef, conversationDto.Messages);


            return result;
        }

        public async Task<Result> UpdateMessages(string userId, ConsultDto consult)
        {
            Result result = new Result();
            var conversationRef = _chatRepository.GetConversationByIdReference(userId, consult.ConversationId);
            IDocumentSnapshotWrapper snapshot = await conversationRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return ValidationConversation();
            }

            Conversation existConversation = snapshot.ConvertTo<Conversation>();
            if (existConversation.Messages == null || existConversation.Messages.Count < 0)
            {
                return ValidationMessages();
            }

            ConversationDto conversationDto = _mapper.Map<ConversationDto>(existConversation);

            if (consult.Messages == null || consult.Messages.Count < 0)
            {
                return ValidationMessages();
            }

            if (conversationDto.Messages == null || conversationDto.Messages.Count == 0)
            {
                if (conversationDto.Messages == null || !conversationDto.Messages.Any())
                {
                    conversationDto.Messages = new List<MessageChatBotDto>(consult.Messages);
                }
            } else
            {
                consult.Messages?.ForEach(message =>
                {
                    var messageToUpdate = conversationDto.Messages.FirstOrDefault(m => m.Uuid == message.Uuid);
                    if (messageToUpdate != null)
                    {
                        messageToUpdate.Message = message.Message;
                    }
                });
            }

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = await _chatRepository.UpdateMessages(conversationRef, conversationDto.Messages);


            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que elimina logicamente una conversacion por usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="newStatus"></param>
        /// <returns>Result</returns>
        public async Task<Result> DeleteConversation(string userId, string conversationId, bool newStatus)
        {
            Result result = new Result();

            var existConversation = await _chatRepository.GetConversationById(userId, conversationId);

            if (existConversation != null)
            {
                result.Success = true;
                result.MessageHttp = Constants.msjMs200;
                result.Data = await _chatRepository.DeleteConversation(userId, conversationId, newStatus);
            }
            else
            {
                result.Success = true;
                result.MessageHttp = Constants.msjMs204;
                result.Data = Constants.CONV_NO_EXIST;
            }

            return result;
        }

        public async Task<Result> DeleteConversations(string userId, string[] conversations, bool newStatus)
        {
            var operationResult = await _chatRepository.DeleteConversations(userId, conversations, newStatus);

            return new Result
            {
                Success = true,
                MessageHttp = Constants.msjMs200,
                Data = operationResult
            };
        }
                
        public async Task<Result> DeleteConversations(string userId, string? name = null, DateTime? from = null, DateTime? to = null)
        {
            var operationResult = await _chatRepository.DeleteConversations(userId, name, from, to);

            return new Result
            {
                Success = true,
                MessageHttp = Constants.msjMs200,
                Data = operationResult
            };
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para colocar un mensaje como favorito de una conversación por usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="messagesDto"></param>
        /// <returns>Result</returns>
        public async Task<Result> UpdateFieldMessages(string userId, string conversationId, MessageChatBotDto messagesDto)
        {
            Result result = new Result();

            var conversationRef = _chatRepository.GetConversationByIdReference(userId, conversationId);
            IDocumentSnapshotWrapper snapshot = await conversationRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return ValidationConversation();
            }

            Conversation existingConversation = snapshot.ConvertTo<Conversation>();
            if (existingConversation.Messages == null || !existingConversation.Messages.Any())
            {
                return ValidationMessages();
            }

            List<MessageChatBotDto> messageToUpdateDto = _mapper.Map<List<MessageChatBotDto>>(existingConversation.Messages);
            var messageToUpdate = messageToUpdateDto.FirstOrDefault(m => m!.Uuid == messagesDto.Uuid);

            if (messageToUpdate == null)
            {
                return ValidationMessageExist();
            }

            messageToUpdateDto = NewMessages(messagesDto, messageToUpdate, messageToUpdateDto);

            bool updateSuccess = await _chatRepository.UpdateFieldMessages(conversationRef, messageToUpdateDto);

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = updateSuccess ? messageToUpdate : null;

            return result;
        }

        public async Task<Result> UpdateFieldConversation(string userId, string conversationId, bool? modeloDocumento)
        {
            Result result = new Result();

            var conversationRef = _chatRepository.GetConversationByIdReference(userId, conversationId);
            IDocumentSnapshotWrapper snapshot = await conversationRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return ValidationConversation();
            }

            Conversation existingConversation = snapshot.ConvertTo<Conversation>();
            existingConversation.ModeloDocumento = modeloDocumento;

            await _chatRepository.UpdateFieldConversation(conversationRef, modeloDocumento);

            result.Success = true;
            result.MessageHttp = Constants.msjMs200;
            result.Data = existingConversation;

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Validación privada cuando no existe una conversación
        /// </summary>
        /// <returns>Result</returns>
        private Result ValidationConversation()
        {
            Result result = new();

            result.Success = true;
            result.MessageHttp = Constants.msjMs204;
            result.Data = Constants.CONV_NO_EXIST;

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Validación privada cuando no existen los mensajes
        /// </summary>
        /// <returns>Result</returns>
        private Result ValidationMessages()
        {
            Result result = new();

            result.Success = true;
            result.MessageHttp = Constants.msjMs204;
            result.Data = Constants.MSJ_NO_EXIST;

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Validación privada cuando no existe el mensaje a actualizar
        /// </summary>
        /// <returns>Result</returns>
        private Result ValidationMessageExist()
        {
            Result result = new();

            result.Success = true;
            result.MessageHttp = Constants.msjMs204;
            result.Data = Constants.MSJ_NO_EXIST_UPDATE;

            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método privado para actualizar a los nuevos mensajes
        /// </summary>
        /// <param name="messagesDto"></param>
        /// <param name="messageToUpdate"></param>
        /// <param name="messageToUpdateDto"></param>
        /// <returns></returns>
        private List<MessageChatBotDto> NewMessages(MessageChatBotDto messagesDto, MessageChatBotDto messageToUpdate, List<MessageChatBotDto> messageToUpdateDto)
        {
            foreach (var prop in typeof(MessageChatBotDto).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name == "Uuid")
                    continue;
                var newValue = prop.GetValue(messagesDto);
                if (newValue != null && !string.IsNullOrEmpty(newValue.ToString()))
                {
                    if (prop.PropertyType.IsAssignableFrom(newValue.GetType()))
                    {
                        prop.SetValue(messageToUpdate, newValue);
                    }
                }
            }

            int index = messageToUpdateDto.IndexOf(messageToUpdate);

            messageToUpdateDto.RemoveAt(index);
            messageToUpdateDto.Insert(index, messageToUpdate);

            return messageToUpdateDto;
        }

    }
}