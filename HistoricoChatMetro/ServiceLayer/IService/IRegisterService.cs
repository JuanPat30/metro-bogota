using DomainLayer.Models;

namespace ServiceLayer.IService
{
    public interface IRegisterService
    {
        Task<Result> GetUsers();
        Task<Result> GetAll(int page, int pageSize, string? name = null, DateTime? from = null, DateTime? to = null, bool? status = null, bool? isDescending = null);
     
    }
}
