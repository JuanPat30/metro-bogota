using Google.Cloud.Firestore;
using RepositoryLayer.IRepository;

namespace RepositoryLayer.Repository
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase wrapper de DocumentSnapshot
    /// </summary>
    public class DocumentSnapshotWrapper : IDocumentSnapshotWrapper
    {
        private readonly DocumentSnapshot _documentSnapshot;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="documentSnapshot"></param>
        public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        {
            _documentSnapshot = documentSnapshot;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para convertir a modelo respuesta de snapshot
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ConvertTo<T>()
        {
            return _documentSnapshot.ConvertTo<T>();
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que retorna existencia de data
        /// </summary>
        public bool Exists => _documentSnapshot.Exists;
    }
}