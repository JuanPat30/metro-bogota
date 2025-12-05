using DomainLayer.Dtos;

namespace RepositoryLayer.IRepository
{
    public interface IRegisterRepository
    {
        Task<List<string>> GetUsers();
        Task<List<ConversationsUserDto>> GetAllConversations(string? name = null, DateTime? from = null, DateTime? to = null, bool? status = null, bool? isDescending = null);
    }
}
