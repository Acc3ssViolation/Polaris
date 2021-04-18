using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Storage
{
    public class PolarisDbContext : DbContext
    {
        public DbSet<DbServer> Servers { get; set; }
        public DbSet<DbPermission> Permissions { get; set; }

        public PolarisDbContext() : base(DefaultOptions)
        {
        }

        public PolarisDbContext(DbContextOptions<PolarisDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DbServer.OnModelCreating(modelBuilder);
            DbPermission.OnModelCreating(modelBuilder);
        }

        public static DbContextOptions<PolarisDbContext> DefaultOptions
        {
            get
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<PolarisDbContext>();
                dbContextOptionsBuilder.UseSqlite("Data Source=polaris.db");
                return dbContextOptionsBuilder.Options;
            }
        }
    }
}
