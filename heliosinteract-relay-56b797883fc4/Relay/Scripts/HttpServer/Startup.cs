using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Helios.Relay.Reach;
using Helios.Relay.Twilio;
using Helios.Relay.Dispatch;
using Helios.Relay.GoogleUa;
using Helios.Relay.CsvExport;
using Helios.Relay.Keen;
using Helios.Relay.Polygon;
using Helios.Relay.Patron;
using Helios.Relay.Eshots;
using Helios.Relay.Dummy;

namespace Helios.Relay
{
    /// <summary>
    /// A class which provides the configurations and services for the Relay server.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("Configuration/relay.json", true, true);

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "twilio.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/twilio.json", true, true);
                services.AddSingleton<IRelay, TwilioRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "dispatch.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/dispatch.json", true, true);
                services.AddSingleton<IRelay, DispatchRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "google-ua.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/google-ua.json", true, true);
                services.AddSingleton<IRelay, GoogleUaRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "csv-export.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/csv-export.json", true, true);
                services.AddSingleton<IRelay, CsvExportRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "keen.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/keen.json", true, true);
                services.AddSingleton<IRelay, KeenRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "polygon.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/polygon.json", true, true);
                services.AddSingleton<IRelay, PolygonRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "patron.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/patron.json", true, true);
                services.AddSingleton<IRelay, PatronRelay>();
            }

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "eshots.json")))
            {
                configurationBuilder.AddJsonFile("Configuration/eshots.json", true, true);
                services.AddSingleton<IRelay, EshotsRelay>();
            }

            var configuration = configurationBuilder.Build();
            if (bool.Parse(configuration.GetSection("Relay")["EnableDummyRelay"])) services.AddSingleton<IRelay, DummyRelay>();
            if (bool.Parse(configuration.GetSection("Relay")["EnableReachRelay"])) services.AddSingleton<IRelay, ReachRelay>();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ICache, Cache>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.ApplicationServices.GetService<ICache>();
            app.ApplicationServices.GetServices<IRelay>().ForEach(x => x.StartRelay());
            app.UseHttpsRedirection();
            app.UseCors(builder => builder.WithOrigins("*"));
            app.UseMvc();
        }
    }
}