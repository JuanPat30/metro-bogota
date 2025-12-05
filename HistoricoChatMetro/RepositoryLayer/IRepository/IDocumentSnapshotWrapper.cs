
namespace RepositoryLayer.IRepository
{
    public interface IDocumentSnapshotWrapper
    {
        T ConvertTo<T>();
        bool Exists { get; }
    }
}
