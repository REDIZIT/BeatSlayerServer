using System;
using System.IO;
using BeatSlayerServer.Models.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BeatSlayerServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = ">=< Working >=<";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{environment}.json")
                .Build();

            ServerSettings settings = new ServerSettings();
            config.Bind(settings);

            Payload.TracksFolder = settings.TracksFolder;

            CreateHostBuilder(args, settings.StartingUrl).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args, string url) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("serversettings.json", optional: false, reloadOnChange: true);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls(url);
                });
    }
}
