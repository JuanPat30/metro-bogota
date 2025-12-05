using Google.Cloud.Firestore;

namespace RepositoryLayer.IRepository
{
    public interface IDocumentReferenceWrapper
    {
        Task<IDocumentSnapshotWrapper> GetSnapshotAsync();
        DocumentReference GetDocumentReference();
    }
}
