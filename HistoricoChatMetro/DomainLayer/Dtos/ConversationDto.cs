namespace DomainLayer.Dtos
{
    public class ConversationDto
    {
        public string UuidConversation { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public bool Estado { get; set; }
        public List<MessageChatBotDto>? Messages { get; set; }
        public bool? ModeloDocumento { get; set; } = false;
    }
}
