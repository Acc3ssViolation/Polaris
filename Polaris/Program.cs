using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Polaris
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.ConfigureServices(ConfigureServices);

            using var host = hostBuilder.Build();
            await host.RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext hbc, IServiceCollection serviceCollection)
        {
            var discordSettings = hbc.Configuration.GetSection("discord").Get<DiscordSettings>();
            serviceCollection
                .AddSingleton(discordSettings)
                .AddHostedService<PluginService>()
                .AddHostedService<DiscordService>();
        }
    }
}
