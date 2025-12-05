
namespace ServiceLayer.IService
{
    public interface IFileWrapper
    {
        string ReadAllText(string path);
        void Delete(string path);
    }
}
