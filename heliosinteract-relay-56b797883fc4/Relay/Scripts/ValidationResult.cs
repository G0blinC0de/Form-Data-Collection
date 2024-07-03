namespace Helios.Relay
{
    public class ValidationResult
    {
        public ValidationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public readonly bool IsSuccess;
        public readonly string Message;
    }
}
