namespace Helios.Raven
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Microsoft.Extensions.Configuration;

    public class Raven : TextWriter
    {
        private TextWriter _consoleOut = Console.Out;
        private Socket _socket;
        private IPEndPoint _endPoint;
        private string _applicationName;
        private string _path;

        public Raven(IConfiguration configuration)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var serverAddress = IPAddress.Parse(configuration.GetSection("Raven")["Address"]);
            _endPoint = new IPEndPoint(serverAddress, int.Parse(configuration.GetSection("Raven")["Endpoint"]));
            _applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "relay.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
        }

        public override void WriteLine(string text)
        {
            var message = new WoodpeckerMessage
            {
                application = _applicationName,
                message = text,
                level = "info"
            };

            var encodedData = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            _socket.SendTo(Encoding.UTF8.GetBytes(encodedData), _endPoint);
            var log = $"{DateTime.UtcNow.ToString("o")} - {message.level}: {message.message}";
            _consoleOut.WriteLine(log);
            File.AppendAllText(_path, log + "\n");
        }

        protected override void Dispose(bool disposing)
        {
            _socket.Close();
            _socket.Dispose();
            base.Dispose(disposing);
        }

        public override Encoding Encoding => Encoding.Default;

        [Serializable]
        private class WoodpeckerMessage
        {
            public string message;
            public string application;
            public string level;
        }
    }
}