namespace Helios.Relay
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Raven;

    public class Program
    {
        private static void Main(string[] args)
        {
            SetupConsoleOutput();
            DisableQuickEditMode();
            VersionInfo();
            CreateWebHostBuilder(args).Build().Run();
        }

        private static void SetupConsoleOutput()
        {
            Console.SetOut(new Raven(new ConfigurationBuilder().AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "raven.json"), true, true).Build()));
        }

        private static void DisableQuickEditMode()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !QuickEditModeOptions.DisableQuickEdit())
            {
                ConsoleLogger.WriteLine("Failed to disable \"Quick Edit Mode\". Contact Ops or consult readme to disable manually.", true);
            }
        }

        private static void VersionInfo()
        {
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            ConsoleLogger.WriteLine($"Relay v{version}");
            Console.Title = $"Relay v{version}";
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(9696, listenOptions =>
                {
                    listenOptions.UseHttps(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "encryption.pfx"), "password");
                });
            });
        }
    }
}