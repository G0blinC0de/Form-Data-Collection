using System;
using System.Text;
using System.Net;
using System.Net.Http;
using RestSharp;

namespace Helios.Relay
{
    /// <summary>
    /// An exception that is thrown if request to a relay endpoint fails.
    /// </summary>
    public class RequestFailureException : Exception
    {
        public RequestFailureException() { }
        public RequestFailureException(string message) : base(message) { }
        public RequestFailureException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// The status code from the failed request, if one was provided.
        /// </summary>
        public HttpStatusCode StatusCode;

        public RequestFailureException(HttpStatusCode httpStatusCode) :
            base(HttpStatusCodeLog(httpStatusCode))
        {
            StatusCode = httpStatusCode;
        }

        public RequestFailureException(string message, HttpStatusCode httpStatusCode) :
            base($"{message} {HttpStatusCodeLog(httpStatusCode)}")
        {
            StatusCode = httpStatusCode;
        }

        public RequestFailureException(RestResponse restResponse) :
            base(RestResponseLog(restResponse), restResponse.ErrorException)
        {
            StatusCode = restResponse?.StatusCode ?? 0;
        }

        public RequestFailureException(string message, RestResponse restResponse) :
            base($"{message} {RestResponseLog(restResponse)}", restResponse.ErrorException)
        {
            StatusCode = restResponse?.StatusCode ?? 0;
        }

        public RequestFailureException(HttpResponseMessage httpResponse) :
            base(HttpResponseLog(httpResponse))
        {
            StatusCode = httpResponse?.StatusCode ?? 0;
        }

        public RequestFailureException(string message, HttpResponseMessage httpResponse) :
            base($"{message} {HttpResponseLog(httpResponse)}")
        {
            StatusCode = httpResponse?.StatusCode ?? 0;
        }

        private static string HttpStatusCodeLog(HttpStatusCode httpStatusCode) => $"({(int)httpStatusCode:000} {httpStatusCode})";

        private static string RestResponseLog(RestResponse restResponse)
        {
            if (restResponse == null) return string.Empty;

            var builder = new StringBuilder();
            builder.Append("(");
            builder.Append(((int)restResponse.StatusCode).ToString());
            builder.Append(restResponse.StatusDescription ?? restResponse.StatusCode.ToString());
            builder.Append(")");

            if (!string.IsNullOrEmpty(restResponse.Content))
            {
                builder.AppendLine();
                builder.Append(restResponse.Content);
            }

            return builder.ToString();
        }

        private static string HttpResponseLog(HttpResponseMessage httpResponse)
        {
            if (httpResponse == null) return string.Empty;

            var builder = new StringBuilder();
            builder.Append("(");
            builder.Append(((int)httpResponse.StatusCode).ToString());
            builder.Append(httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString());
            builder.Append(")");

            if (httpResponse.Content != null)
            {
                builder.AppendLine();
                builder.Append(httpResponse.Content);
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// An exception that is thrown if data is received from an endpoint in an unexpected format.
    /// </summary>
    public class UnexpectedDataFormatException : Exception
    {
        public UnexpectedDataFormatException() { }
        public UnexpectedDataFormatException(string message) : base(message) { }
        public UnexpectedDataFormatException(string message, Exception innerException) : base(message, innerException) { }

        public static UnexpectedDataFormatException MissingProperty(string name) => new UnexpectedDataFormatException($"The expected property \"{name}\" was not found.");
    }

    /// <summary>
    /// An exception that is thrown if an error occurs that is specific to a particular relay.
    /// </summary>
    public class RelayException : Exception
    {
        public RelayException() { }
        public RelayException(string message) : base(message) { }
        public RelayException(string message, Exception innerException) : base(message, innerException) { }
    }
}
