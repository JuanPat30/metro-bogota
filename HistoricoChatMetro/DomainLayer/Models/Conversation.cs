using Google.Cloud.Firestore;

namespace DomainLayer.Models
{
    [FirestoreData]
    public class Conversation
    {
        [FirestoreProperty]
        public string UuidConversation { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? Date { get; set; }

        [FirestoreProperty]
        public bool Estado { get; set; }

        [FirestoreProperty]
        public List<MessageChatBot>? Messages { get; set; }

        [FirestoreProperty]
        public bool? ModeloDocumento { get; set; } = false;
    }
}
