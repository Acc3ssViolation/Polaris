using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Storage
{
    public class DbServer
    {
        public string Name { get; set; }
        public ulong Id { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityBuilder = modelBuilder.Entity<DbServer>();
            entityBuilder.HasKey(_ => _.Id);
        }
    }
}
