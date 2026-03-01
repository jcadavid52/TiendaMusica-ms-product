namespace TiendaMusica.Domain.Models.Result
{
    public class TiendaMusicaError
    {
        public string Message { get; private set; } = string.Empty;
        public ErrorCode ErrorCode { get; private set; }

        public TiendaMusicaError(ErrorCode errorCode,string message)
        {
            Message = message;
            ErrorCode = errorCode;
        }
    }
}
