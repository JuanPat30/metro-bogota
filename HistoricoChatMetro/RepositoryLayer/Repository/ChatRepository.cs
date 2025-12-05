using AutoMapper;
using Commun.Helpers;
using Commun.Logger;
using DomainLayer.Dtos;
using DomainLayer.Models;
using Google.Cloud.Firestore;
using RepositoryLayer.Data;
using RepositoryLayer.IRepository;

namespace RepositoryLayer.Repository
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase del repositorio para el almacenamiento de historico
    /// </summary>
    public class ChatRepository : IChatRepository
    {
        private readonly FirestoreDb _db;

        private readonly ICreateLogger _createLogger;
        private readonly IMapper _mapper;

        private readonly string collectionChats = "UsersChats";
        private readonly string collectionConv = "Conversations";

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="objContext"></param>
        /// <param name="mapper"></param>
        /// <param name="createLogger"></param>
        public ChatRepository(FirestoreDbContext objContext, IMapper mapper, ICreateLogger createLogger)
        {
            _db = objContext.GetDb();
            _mapper = mapper;
            _createLogger = createLogger;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para consultar las conversaciones por usuario
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>List<ConversationDto></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ConversationDto>> GetConversationByUser(string userId, string? name = null, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var query = _db.Collection(collectionChats)
                               .Document(userId)
                               .Collection(collectionConv)
                               .WhereEqualTo("Estado", true);

                if (from.HasValue) query = query.WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(from.Value.ToUniversalTime()));
                if (to.HasValue) query = query.WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(to.Value.ToUniversalTime()));

                var snapshot = await query.GetSnapshotAsync();
                List<Conversation> allConversations = snapshot.Documents.Select(d => d.ConvertTo<Conversation>()).ToList();

                if (!string.IsNullOrEmpty(name))
                {
                    string normalizedSearchName = StringUtils.RemoveAccents(name);

                    allConversations = allConversations
                        .Where(c => c.Name != null &&
                                   StringUtils.RemoveAccents(c.Name).Contains(normalizedSearchName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return _mapper.Map<List<ConversationDto>>(allConversations);
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para consultar una conversación por ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns>ConversationDto</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ConversationDto?> GetConversationById(string userId, string conversationId)
        {
            try
            {
                var docRef = _db.Collection(collectionChats).Document(userId).Collection(collectionConv).Document(conversationId);
                var snapshot = await docRef.GetSnapshotAsync();
                Conversation? conv = snapshot.Exists ? snapshot.ConvertTo<Conversation>() : null;
                return _mapper.Map<ConversationDto>(conv);
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para insertar una conversación
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationDto"></param>
        /// <returns>true si inserta o false si no inserta</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Insert(string userId, ConversationDto conversationDto)
        {
            try
            {
                Conversation conversation = _mapper.Map<Conversation>(conversationDto);
                var userDocRef = _db.Collection(collectionChats).Document(userId);
                var chatsRef = userDocRef.Collection(collectionConv);
                var docRef = chatsRef.Document(conversation.UuidConversation ?? Guid.NewGuid().ToString());
                await _db.RunTransactionAsync(async transaction =>
                {
                    var userSnapshot = await transaction.GetSnapshotAsync(userDocRef);
                    transaction.Set(docRef, conversation);
                });
                return true;
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para actualizar una conversación
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="chatsRef"></param>
        /// <param name="conversationDto"></param>
        /// <returns>ConversationDto actualizando</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ConversationDto> Update(string userId, string conversationId, IDocumentReferenceWrapper chatsRef, ConversationDto conversationDto)
        {
            try
            {
                Conversation conversation = _mapper.Map<Conversation>(conversationDto);
                await _db.RunTransactionAsync(async transaction =>
                {
                    transaction.Set(chatsRef.GetDocumentReference(), conversation, SetOptions.Overwrite);

                });

                var updatedSnapshot = await chatsRef.GetSnapshotAsync();
                return updatedSnapshot.Exists ? _mapper.Map<ConversationDto>(updatedSnapshot.ConvertTo<Conversation>()) : new ConversationDto();
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para eliminar los mensajes 
        /// </summary>
        /// <param name="conversationRef"></param>
        /// <param name="messagesDto"></param>
        /// <returns>ConversationDto con lista de mensajes vacios</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ConversationDto> UpdateMessages(IDocumentReferenceWrapper conversationRef, List<MessageChatBotDto> messagesDto)
        {
            try
            {
                List<MessageChatBot> messagesC = _mapper.Map<List<MessageChatBot>>(messagesDto);
                await _db.RunTransactionAsync(async transaction =>
                {
                    transaction.Update(conversationRef.GetDocumentReference(), "Messages", messagesC);
                    return true;
                });

                var updatedSnapshot = await conversationRef.GetSnapshotAsync();
                return updatedSnapshot.Exists ? _mapper.Map<ConversationDto>(updatedSnapshot.ConvertTo<Conversation>()) : new ConversationDto();
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para eliminar logicamente una conversacion actualizando el estado en false
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="newStatus"></param>
        /// <returns>true si elimino o false si no elimino</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> DeleteConversation(string userId, string conversationId, bool newStatus)
        {
            try
            {
                var query = _db.Collection(collectionChats)
                                  .Document(userId)
                                  .Collection(collectionConv)
                                  .Document(conversationId);

                await _db.RunTransactionAsync(async transaction =>
                {
                    var snapshot = await transaction.GetSnapshotAsync(query);
                    if (!snapshot.Exists) throw new Exception("La conversación no existe.");
                    transaction.Update(query, "Estado", newStatus);
                    return true;
                });
                return true;

            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteConversations(string userId, string[] conversationsIds, bool newStatus)
        {
            try
            {
                var query = _db.Collection(collectionChats)
                                  .Document(userId)
                                  .Collection(collectionConv);

                await _db.RunTransactionAsync(async transaction =>
                {
                    foreach (var conversationId in conversationsIds)
                    {
                        var snapshot = await transaction.GetSnapshotAsync(query.Document(conversationId));
                        if (!snapshot.Exists) throw new Exception("La conversación no existe.");
                        transaction.Update(query.Document(conversationId), "Estado", newStatus);
                    }
                    return true;
                });
                return true;

            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteConversations(string userId, string? name = null, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                CollectionReference collection = _db.Collection(collectionChats);
                QuerySnapshot snapshotS = await collection.GetSnapshotAsync();

                var query = _db.Collection(collectionChats).Document(userId).Collection(collectionConv).WhereEqualTo("Estado", true);
                if (!string.IsNullOrEmpty(name)) query = query.WhereGreaterThanOrEqualTo("Name", name).WhereLessThan("Name", name + "\uf8ff");
                if (from.HasValue) query = query.WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(from.Value.ToUniversalTime()));
                if (to.HasValue) query = query.WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(to.Value.ToUniversalTime()));

                await _db.RunTransactionAsync(async transaction =>
                {
                    var snapshot = await query.GetSnapshotAsync();
                    foreach (var document in snapshot.Documents)
                    {
                        transaction.Update(document.Reference, "Estado", false);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para obtener snapshot de conversacion
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IDocumentReferenceWrapper GetConversationByIdReference(string userId, string conversationId)
        {
            try
            {
                var documentReference = _db.Collection(collectionChats).Document(userId).Collection(collectionConv).Document(conversationId);
                return new DocumentReferenceWrapper(documentReference);
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Actualizar una conversación
        /// </summary>
        /// <param name="conversationRef"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UpdateFieldMessages(IDocumentReferenceWrapper conversationRef, List<MessageChatBotDto> messages)
        {
            try
            {
                List<MessageChatBot> messagesC = _mapper.Map<List<MessageChatBot>>(messages);
                return await _db.RunTransactionAsync(async transaction =>
                {
                    transaction.Update(conversationRef.GetDocumentReference(), "Messages", messagesC);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateFieldConversation(IDocumentReferenceWrapper conversationRef, bool? modeloDocumento)
        {
            try
            {
                return await _db.RunTransactionAsync(async transaction =>
                {
                    transaction.Update(conversationRef.GetDocumentReference(), "ModeloDocumento", modeloDocumento);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}