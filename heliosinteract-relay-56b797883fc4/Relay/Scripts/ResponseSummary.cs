namespace Helios.Relay
{
    using RestSharp;

    public class ResponseSummary
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        public ResponseSummary(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public ResponseSummary(RestResponse response)
        {
            IsSuccess = response.IsSuccessful;
            Message = $"StatusCode: {response.StatusCode} StatusDescription: {response.StatusDescription}";
        }
    }
}
