using DomainLayer.Models;

namespace ServiceLayer.IService
{
    public interface IAuthToken
    {
        Tuple<string, DateTime> GenerarToken(AuthModel authModel);
    }
}
