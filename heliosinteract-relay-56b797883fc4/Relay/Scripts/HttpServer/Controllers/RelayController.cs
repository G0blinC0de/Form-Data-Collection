namespace Helios.Relay
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/v1/[controller]")]
    [ApiController]
    public class RelayController : ControllerBase
    {
        public RelayController(IEnumerable<IRelay> relayServices)
        {
            _relays = relayServices;
        }

        private readonly IEnumerable<IRelay> _relays;
        private readonly Lazy<string> _version = new Lazy<string>(() => Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
        
        [HttpGet("Version")]
        public ActionResult<string> Version()
        {
            return _version.Value;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [DisableRequestSizeLimit]
        public IActionResult Post([FromForm] PostData postReceived)
        {
            string responseMessage = string.Empty;
            try
            {
                ConsoleLogger.WriteLine("POST received at /api/v1/relay");

                // Handle no relays online
                if (_relays == null)
                {
                    responseMessage = $"500 Internal Server Error: No relays online.";
                    ConsoleLogger.WriteLine(responseMessage, true, ConsoleColor.Red);
                    return StatusCode(StatusCodes.Status500InternalServerError, responseMessage);
                }

                // Handle post matches no relay
                if (_relays.All(service => !postReceived.RelayService.HasFlag(service.ServiceType)))
                {
                    responseMessage = $"400 Bad Request: The received post does not have a Relay Service assigned.";
                    ConsoleLogger.WriteLine(responseMessage, true, ConsoleColor.Red);
                    return BadRequest(responseMessage);
                }

                // Cache the file locally and update the file data
                CacheFileData(postReceived);

                // Find matching relays and validate data against each of them
                var validatedEntries = new Dictionary<IRelay, CacheEntry>();
                foreach (var service in _relays.Where(service => postReceived.RelayService.HasFlag(service.ServiceType)))
                {
                    var validation = service.TryValidate(postReceived, out var cacheEntry);
                    if (validation.IsSuccess)
                    {
                        validatedEntries.Add(service, cacheEntry);
                    }
                    else
                    {
                        responseMessage = $"400 Bad Request: {validation.Message}";
                        ConsoleLogger.WriteLine($"[{service.ServiceType}] {responseMessage}", true, ConsoleColor.Red);
                        return BadRequest(responseMessage);
                    }
                };

                // Success
                validatedEntries.ForEach(entry => entry.Key.Add(entry.Value));
                responseMessage = $"200 Ok: Object cached.";
                ConsoleLogger.WriteLine(responseMessage, color: ConsoleColor.Green);
                return Ok();
            }
            catch (Exception ex)
            {
                responseMessage = $"500 Internal Server Error: {ex}";
                ConsoleLogger.WriteLine(responseMessage, true, ConsoleColor.Red);
                return StatusCode(StatusCodes.Status500InternalServerError, responseMessage);
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var filesList = new List<CacheEntry>();
                _relays.ForEach(service => filesList.AddRange(service.GetAllEntries()));
                return new JsonResult(filesList);
            }
            catch (Exception ex)
            {
                return ExceptionCatch(ex);
            }
        }

        [HttpGet("GetLogs")]
        public IActionResult GetAllLogsAsync()
        {
            try
            {
                var logs = new List<string>();
                _relays.ForEach(service => logs.AddRange(service.GetLogs()));
                return new JsonResult(logs);
            }
            catch (Exception ex)
            {
                return ExceptionCatch(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                var isSuccess = false;
                _relays.ForEach(service => isSuccess = isSuccess || service.Remove(id));
                return isSuccess ? StatusCode(StatusCodes.Status200OK, $"File Id: {id} has been deleted") : StatusCode(StatusCodes.Status404NotFound, "File cannot be found");
            }
            catch (Exception ex)
            {
                return ExceptionCatch(ex);
            }
        }

        [HttpDelete("DeleteCount/{failureCount}")]
        public ActionResult DeleteFailedCountAsync(int failureCount)
        {
            try
            {
                var count = 0;
                _relays.ForEach(service => count += service.RemoveFailedEntries(failureCount));
                return count > 0
                    ? StatusCode(StatusCodes.Status200OK, $"Removed {count} entries with a failed send count at or above {failureCount}")
                    : StatusCode(StatusCodes.Status404NotFound, "No entries meet the criteria for deletion");
            }
            catch (Exception ex)
            {
                return ExceptionCatch(ex);
            }
        }

        private ActionResult ExceptionCatch(Exception ex)
        {
            var responseMessage = $"Exception: {ex}";
            ConsoleLogger.WriteLine(responseMessage, true, ConsoleColor.Red);
            return StatusCode(StatusCodes.Status500InternalServerError, responseMessage);
        }

        private void CacheFileData(IPostData postData)
        {
            if (postData.File == null || postData.File.Length == 0)
            {
                postData.FileInfo = null;
                postData.File = null;
                return;
            }

            // Get the path where the file will be written
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!string.IsNullOrEmpty(postData.Experience?.activationId)) directoryPath = Path.Combine(directoryPath, postData.Experience.activationId);
            Directory.CreateDirectory(directoryPath);
            var extension = MimeType.GetExtension(postData.FileInfo.mimeType);
            var path = Path.Combine(directoryPath, $"{Guid.NewGuid()}{extension}");

            // Cache the file
            using var fileStream = new FileStream(path, FileMode.Create);
            postData.File.CopyTo(fileStream);

            // Update the path
            postData.FileInfo.path = path;
        }
    }
}