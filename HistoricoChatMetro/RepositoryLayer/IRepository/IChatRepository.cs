using DomainLayer.Dtos;

namespace RepositoryLayer.IRepository
{
    public interface IChatRepository
    {
        Task<List<ConversationDto>> GetConversationByUser(string userId, string? name = null, DateTime? from = null, DateTime? to = null);
        Task<ConversationDto?> GetConversationById(string userId, string conversationId);
        IDocumentReferenceWrapper GetConversationByIdReference(string userId, string conversationId);
        Task<bool> Insert(string userId, ConversationDto conversation);
        Task<ConversationDto> Update(string userId, string conversationId, IDocumentReferenceWrapper chatsRef, ConversationDto conversation);
        Task<ConversationDto> UpdateMessages(IDocumentReferenceWrapper conversationRef, List<MessageChatBotDto> updatedMessage);
        Task<bool> DeleteConversation(string userId, string conversationId, bool newStatus);
        Task<bool> DeleteConversations(string userId, string[] conversations, bool newStatus);
        Task<bool> DeleteConversations(string userId, string? name = null, DateTime? from = null, DateTime? to = null);
        Task<bool> UpdateFieldMessages(IDocumentReferenceWrapper conversationRef, List<MessageChatBotDto> messagesDto);
        Task<bool> UpdateFieldConversation(IDocumentReferenceWrapper conversationRef, bool? modeloDocumento);
    }
}
