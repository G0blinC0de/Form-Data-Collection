using System;
using System.Text;

namespace Helios.Relay
{
    public static class ConsoleLogger
    {
        public static void WriteLine(object value, bool addErrorTag = false, ConsoleColor color = ConsoleColor.White)
        {
            WriteLine(value.ToString(), RelayService.None, "", addErrorTag, color);
        }

        public static void WriteLine(string message, RelayService relayService, string loggingKey, bool addErrorTag = false, ConsoleColor color = ConsoleColor.White)
        {
            var consoleMessage = new StringBuilder();
            if (addErrorTag) consoleMessage.Append("[ERROR] ");
            if (relayService != RelayService.None) consoleMessage.Append($"[{relayService}] ");
            if (!string.IsNullOrWhiteSpace(loggingKey)) consoleMessage.Append($"[Id: {loggingKey}] ");
            if (!string.IsNullOrEmpty(message)) consoleMessage.Append(message);

            Console.ForegroundColor = color;
            Console.WriteLine(consoleMessage.ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}