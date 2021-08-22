using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octantis.Discord.Api;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                    var jsonOptions = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        PropertyNamingPolicy = new LowerCamelCaseNamingPolicy(),
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };
                    serviceCollection
                        .Configure<DiscordSettings>(context.Configuration.GetSection("Discord"))
                        .AddSingleton<JsonSerializerOptions>((sp) => jsonOptions)
                        .AddSingleton<RestApi>()
                        .AddSingleton<HttpClient>()
                        .AddHostedService<DiscordService>();
                });

            using var host = hostBuilder.Build();

            await host.RunAsync();
        }

    }
}
