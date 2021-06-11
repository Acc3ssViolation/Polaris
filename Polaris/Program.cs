using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polaris.Authorization;
using Polaris.Storage;
using System;
using System.IO;
using System.Reflection;
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
            var databaseStorage = host.Services.GetRequiredService<DatabaseStorage>();
            await databaseStorage.InitializeAsync();
            await host.RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext hbc, IServiceCollection serviceCollection)
        {
            var discordSettings = hbc.Configuration.GetSection("discord").Get<DiscordSettings>();
            serviceCollection
                .AddSingleton(discordSettings)
                .AddSingleton(sp => {
                    var config = new CommandServiceConfig();
                    config.CaseSensitiveCommands = false;
                    config.DefaultRunMode = RunMode.Sync;
                    config.IgnoreExtraArgs = false;
                    config.LogLevel = Discord.LogSeverity.Debug;
                    config.ThrowOnError = true;
                    var service = new CommandService(config);
                    return service;
                })
                .AddSingleton<Func<PolarisDbContext>>(CreateDatabase)
                .AddTransient<DatabaseStorage>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<IClaimManager, ServerClaimManager>()
                .AddSingleton<IClaimProvider, ServerClaimProvider>()
                .AddHostedService<PluginService>()
                .AddHostedService<DiscordService>()
                .AddHostedService<GuildService>();
        }

        private static Func<PolarisDbContext> CreateDatabase(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<DiscordSettings>();
            var storagePath = settings.StoragePath ?? new FileInfo(new Uri(Assembly.GetEntryAssembly()!.GetName()!.CodeBase!).AbsolutePath).Directory!.FullName;
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(storagePath, "polaris.db")
            };
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<PolarisDbContext>();
            dbContextOptionsBuilder.UseSqlite(builder.ToString());
#if DEBUG
            dbContextOptionsBuilder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            dbContextOptionsBuilder.EnableSensitiveDataLogging(true);
#endif
            var options = dbContextOptionsBuilder.Options;

            return () => new PolarisDbContext(options);
        }
    }
}
