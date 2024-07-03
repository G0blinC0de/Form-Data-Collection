using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using Helios.Encryption;

namespace Helios.Relay.Twilio
{
    public class TwilioWebRequest
    {
        private readonly string accountSid;
        private readonly string authToken;
        private readonly string fromPhoneNumber;
        private readonly string message;

        public TwilioWebRequest(IConfiguration configuration)
        {
            accountSid = StringEncryption.Decrypt(configuration.GetSection("Twilio")["AccountSid"]);
            authToken = StringEncryption.Decrypt(configuration.GetSection("Twilio")["AuthToken"]);
            fromPhoneNumber = configuration.GetSection("Twilio")["PhoneNumber"];
            message = configuration.GetSection("Twilio")["Message"];
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendTwilioMessageAsync(string fileUri, string phoneNumber)
        {
            ConsoleLogger.WriteLine(fileUri);

            var twilioMessage = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromPhoneNumber),
                to: new PhoneNumber(phoneNumber),
                mediaUrl: new[] {
                    new Uri(fileUri)
                }.ToList()
            ).ConfigureAwait(false);

            if (twilioMessage.ErrorCode != null)
            {
                if (message.Contains("Connection Error")) throw new RequestFailureException(twilioMessage.ErrorMessage);
                throw new RelayException($"Twilio Error for #{phoneNumber}: {twilioMessage.ErrorMessage}");
            }
        }
    }
}