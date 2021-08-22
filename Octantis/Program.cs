using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Octantis
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(configBuilder =>
                {
                    configBuilder
                        .AddEnvironmentVariables("DOTNET_")
                        .AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder
                        .AddIniFile("octantis.ini")
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    loggingBuilder
                        .AddConfiguration(context.Configuration.GetSection("Logging"))
                        .AddDebug()
                        .AddConsole();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                })
                .ConfigureServices((context, serviceCollection) =>
                {
                    serviceCollection
                        .Configure<DiscordSettings>(context.Configuration.GetSection("Discord"))
                        .AddHostedService<DiscordService>();
                });

            using var host = hostBuilder.Build();

            await host.RunAsync();
        }

    }
}
