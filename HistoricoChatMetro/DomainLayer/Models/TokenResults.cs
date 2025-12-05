namespace DomainLayer.Models
{
    public class TokenResults
    {
        public bool isSuccess { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string expiresAt { get; set; } = string.Empty;
        public dynamic? extraInfo { get; set; } = null;
    }
}
