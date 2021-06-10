using Microsoft.EntityFrameworkCore;
using Polaris.Authorization;
using Polaris.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Storage
{
    public class DbPermission
    {
        public SubjectType SubjectType { get; set; }
        public ulong SubjectId { get; set; }
        public ulong ServerId { get; set; }
        public string Identifier { get; set; } = string.Empty;

        public DbServer? Server { get; set; }

        public DbPermission(SubjectType subjectType, ulong subjectId, ulong serverId, string identifier)
        {
            SubjectType = subjectType;
            SubjectId = subjectId;
            ServerId = serverId;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityBuilder = modelBuilder.Entity<DbPermission>();
            entityBuilder.HasKey(_ => new { _.Identifier, _.SubjectType, _.SubjectId });
            entityBuilder.HasIndex(_ => _.ServerId);
            entityBuilder.HasOne(_ => _.Server)
                .WithMany(_ => _.Permissions)
                .HasForeignKey(_ => _.ServerId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
