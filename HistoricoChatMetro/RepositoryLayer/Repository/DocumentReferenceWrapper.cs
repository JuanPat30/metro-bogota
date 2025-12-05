using Google.Cloud.Firestore;
using RepositoryLayer.Repository;

namespace RepositoryLayer.IRepository
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase wrapper para DocumentReference
    /// </summary>
    public class DocumentReferenceWrapper: IDocumentReferenceWrapper
    {
        private readonly DocumentReference _documentReference;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="documentReference"></param>
        public DocumentReferenceWrapper(DocumentReference documentReference)
        {
            _documentReference = documentReference;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para devolver snapshot
        /// </summary>
        /// <returns></returns>
        public async Task<IDocumentSnapshotWrapper> GetSnapshotAsync()
        {
            var snapshot = await _documentReference.GetSnapshotAsync();
            return new DocumentSnapshotWrapper(snapshot);
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para devolver document reference
        /// </summary>
        /// <returns></returns>
        public DocumentReference GetDocumentReference()
        {
            return _documentReference;
        }
    }
}
