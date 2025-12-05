using DomainLayer.Models;

namespace ServiceLayer.IService
{
    public interface IEmailService
    {
        Task<Result> SendEmail(string userId, string conversationId);
     
    }
}
