using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Storage
{
    internal class DatabaseStorage
    {
        private string _connectionString;
        private readonly ILogger<DatabaseStorage> _logger;

        public DatabaseStorage(DiscordSettings discordSettings, ILogger<DatabaseStorage> logger)
        {
            var storagePath = discordSettings.StoragePath ?? new FileInfo(new Uri(Assembly.GetEntryAssembly()!.GetName()!.CodeBase!).AbsolutePath).Directory!.FullName;
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(storagePath, "polaris.db")
            };
            _connectionString = builder.ToString();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitializeAsync()
        {
            try
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<PolarisDbContext>();
                dbContextOptionsBuilder.UseSqlite(_connectionString);

                await using var dbContext = new PolarisDbContext(dbContextOptionsBuilder.Options);

                var dbDatabase = dbContext.Database;
                foreach (var migration in dbDatabase.GetPendingMigrations())
                    _logger.LogInformation("Migration {Name} will be applied", migration);

                await dbDatabase.MigrateAsync().ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error initializing database");
            }
        }
    }
}
