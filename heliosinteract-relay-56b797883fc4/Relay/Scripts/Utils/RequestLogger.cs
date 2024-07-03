using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using YamlDotNet.Serialization;

namespace Helios.Relay
{
    public class RequestLogger
    {
        public RequestLogger(string path)
        {
            _path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
        }

        private string _path;

        public async Task<RestResponse> SendAndLogRequest(RestClient restClient, RestRequest request, RFile fileInfo = null)
        {
            var sendTimestamp = DateTime.Now;
            var response = await restClient.ExecuteAsync(request);
            var requestToLog = new
            {
                timestamp = sendTimestamp,
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = restClient.BuildUri(request)?.ToString(),
                file = fileInfo?.path,
                mimeType = fileInfo?.mimeType,
            };

            var responseToLog = new
            {
                timestamp = DateTime.Now,
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri?.ToString(),
                errorMessage = response.ErrorMessage,
            };

            var output = new { Request = requestToLog, Response = responseToLog };
            var yaml = new Serializer().Serialize(output);
            await File.AppendAllTextAsync(_path, "---\n" + yaml);
            
            return response;
        }
    }
}
