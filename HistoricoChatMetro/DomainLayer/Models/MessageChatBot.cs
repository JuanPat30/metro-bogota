using Google.Cloud.Firestore;

namespace DomainLayer.Models
{
    [FirestoreData]
    public class MessageChatBot
    {
        [FirestoreProperty]
        public string IdPersona { get; set; } = string.Empty;

        //[FirestoreProperty]
        //public string Role { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Message { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? Fecha { get; set; }

        [FirestoreProperty]
        public string Uuid { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool IsFavorite { get; set; }

        [FirestoreProperty]
        public string Query { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Type { get; set; } = string.Empty;

        [FirestoreProperty("documentosUrl")]
        public string? DocumentosUrl { get; set; } = string.Empty;
    }
}
