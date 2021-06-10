using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polaris.Common;
using Polaris.Storage;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class ServerClaimManager : IClaimManager
    {
        private readonly SubjectType[] AllowedSubjectTypes = new[] { SubjectType.User, SubjectType.Role };

        private readonly Func<PolarisDbContext> _dbFactory;
        private readonly ILogger<ServerClaimManager> _logger;

        public ServerClaimManager(Func<PolarisDbContext> dbFactory, ILogger<ServerClaimManager> logger)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IClaimCollection?> GetClaimCollectionAsync(GuildSubject subject, CancellationToken cancellationToken)
        {
            if (!AllowedSubjectTypes.Contains(subject.Type))
                throw new ArgumentException("Invalid subject type for claims", nameof(subject));

            using var dbContext = _dbFactory();
            var dbPermissions = dbContext.Permissions.AsNoTracking().Where(p => p.ServerId == subject.GuildId && p.SubjectId == p.SubjectId && p.SubjectType == p.SubjectType);

            var claims = await dbPermissions.Select(p => p.Identifier).ToListAsync(cancellationToken);

            return new ClaimCollection(subject, claims);
        }

        public async Task SetPermissionClaimAsync(GuildSubject subject, string claim, CancellationToken cancellationToken)
        {
            if (!AllowedSubjectTypes.Contains(subject.Type))
                throw new ArgumentException("Invalid subject type for claims", nameof(subject));

            using var dbContext = _dbFactory();
            var dbPermissions = dbContext.Permissions.AsNoTracking().Where(p => p.ServerId == subject.GuildId && p.SubjectId == p.SubjectId && p.SubjectType == p.SubjectType);
            var dbPermission = await dbPermissions.FirstOrDefaultAsync(p => p.Identifier == claim, cancellationToken);
            if (dbPermission is not null)
                return;

            dbPermission = new DbPermission(subject.Type, subject.Id, subject.GuildId, claim);
            dbContext.Permissions.Add(dbPermission);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletePermissionClaimAsync(GuildSubject subject, string claim, CancellationToken cancellationToken)
        {
            if (!AllowedSubjectTypes.Contains(subject.Type))
                throw new ArgumentException("Invalid subject type for claims", nameof(subject));

            using var dbContext = _dbFactory();
            var dbPermissions = dbContext.Permissions.AsNoTracking().Where(p => p.ServerId == subject.GuildId && p.SubjectId == p.SubjectId && p.SubjectType == p.SubjectType);
            var dbPermission = await dbPermissions.FirstOrDefaultAsync(p => p.Identifier == claim, cancellationToken);
            if (dbPermission is null)
                return;

            dbContext.Permissions.Remove(dbPermission);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
