using DomainLayer.Dtos;
using DomainLayer.Models;

namespace ServiceLayer.IService
{
    public interface IChatService
    {
        Task<Result> GetConversationByUser(string userId, string? name = null, DateTime? from = null, DateTime? to = null);
        Task<Result> GetConversationById(string userId, string conversationId);
        Task<Result> SaveConversation(string userId, ConversationDto conversation);
        Task<Result> UpdateMessages(string userId, string conversationId);
        Task<Result> UpdateMessages(string userId, ConsultDto consult);
        Task<Result> DeleteConversation(string userId, string conversationId, bool newStatus);
        Task<Result> DeleteConversations(string userId, string[] conversations, bool newStatus);
        Task<Result> DeleteConversations(string userId, string? name = null, DateTime? from = null, DateTime? to = null);
        Task<Result> UpdateFieldMessages(string userId, string conversationId, MessageChatBotDto messagesDto);

        Task<Result> UpdateFieldConversation(string userId, string conversationId, bool? modeloDocumento);
    }
}
