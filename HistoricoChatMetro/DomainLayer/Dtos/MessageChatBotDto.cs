namespace DomainLayer.Dtos
{
    public class MessageChatBotDto
    {
        public string IdPersona { get; set; } = string.Empty;
       // public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public bool IsFavorite { get; set; } = false;
        // public string Query { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? DocumentosUrl { get; set; } = string.Empty;
    }
}
