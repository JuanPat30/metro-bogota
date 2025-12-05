namespace DomainLayer.Dtos
{
    public class ConsultDto
    {   
        public string ConversationId { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public ConversationDto? Conversation { get; set; }
        public List<MessageChatBotDto>? Messages { get; set; }
    }

    public class UpdateFieldConversation
    {
        public string ConversationId { get; set; } = string.Empty;
        public bool? ModeloDocumento { get; set; }
    }


    public class QueryConversationsByIds
    {
        public string[] ConversationIds { get; set; } = [];

        public bool? Status { get; set; }
    }

    public class QueryConversationBySearch
    {
        public bool? Status { get; set; }
        public string? Name { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
