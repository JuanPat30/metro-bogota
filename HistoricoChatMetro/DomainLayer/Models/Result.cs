namespace DomainLayer.Models
{
    public class Result
    {
        public bool Success { get; set; } = false;
        public string MessageHttp { get; set; } = string.Empty;
        public object? Data { get; set; } = null;

        public Result()
        {
            this.Success = false;
        }

        public Result(bool success, string messageHttp, object? data)
        {
            Success = success;
            MessageHttp = messageHttp;
            Data = data;
        }

        public static Result CreateMessage(bool success, string MessageHttp, object? Data)
        {
            return new Result { Success = success, MessageHttp = MessageHttp, Data = Data };
        }
    }
}
