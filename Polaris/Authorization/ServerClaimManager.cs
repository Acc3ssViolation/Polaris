using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polaris.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class ServerClaimManager : IClaimManager
    {
        private readonly Func<PolarisDbContext> _dbFactory;
        private readonly ILogger<ServerClaimManager> _logger;

        public ServerClaimManager(Func<PolarisDbContext> dbFactory, ILogger<ServerClaimManager> logger)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IRoleClaimCollection?> GetRoleClaimCollectionAsync(ulong guildId, ulong roleId, CancellationToken cancellationToken)
        {
            using var dbContext = _dbFactory();

            var dbPermissions = await dbContext.Permissions.AsNoTracking()
                .Where(p => p.ServerId == guildId && p.SubjectType == ClaimType.Role && p.SubjectId == roleId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (dbPermissions.Count > 0)
            {
                return new RoleClaimCollection(guildId, roleId, dbPermissions.Select(p => new PermissionClaim(p.Identifier, p.AllowedOperations)));
            }

            return null;
        }

        public async Task<IUserClaimCollection?> GetUserClaimCollectionAsync(ulong guildId, ulong userId, CancellationToken cancellationToken)
        {
            using var dbContext = _dbFactory();

            var dbPermissions = await dbContext.Permissions.AsNoTracking()
                .Where(p => p.ServerId == guildId && p.SubjectType == ClaimType.User && p.SubjectId == userId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (dbPermissions.Count > 0)
            {
                return new UserClaimCollection(guildId, userId, dbPermissions.Select(p => new PermissionClaim(p.Identifier, p.AllowedOperations)));
            }

            return null;
        }

        public async Task<bool> UpdatePermissionClaimAsync(ulong guildId, ClaimType type, ulong subjectId, IPermissionClaim claim, CancellationToken cancellationToken)
        {
            using var dbContext = _dbFactory();

            var dbPermission = await dbContext.Permissions.AsQueryable()
                .FirstOrDefaultAsync(p => 
                    p.ServerId == guildId && 
                    p.SubjectType == type && 
                    p.SubjectId == subjectId &&
                    string.Equals(p.Identifier, claim.Identifier)
                ,cancellationToken)
                .ConfigureAwait(false);
            if (dbPermission is null)
            {
                dbPermission = new DbPermission(type, subjectId, guildId, claim.Identifier, claim.ClaimedOperations);
                dbContext.Permissions.Add(dbPermission);
            }

            dbPermission.AllowedOperations = claim.ClaimedOperations;

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> DeletePermissionClaimAsync(ulong guildId, ClaimType type, ulong subjectId, string claimIdentifier, CancellationToken cancellationToken)
        {
            using var dbContext = _dbFactory();
            var dbPermission = await dbContext.Permissions.AsQueryable()
                .FirstOrDefaultAsync(p =>
                    p.ServerId == guildId &&
                    p.SubjectType == type &&
                    p.SubjectId == subjectId &&
                    string.Equals(p.Identifier, claimIdentifier)
                , cancellationToken)
                .ConfigureAwait(false);
            if (dbPermission is null)
            {
                return false;
            }
            dbContext.Permissions.Remove(dbPermission);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> CreateClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken)
        {
            ulong subjectId;
            ClaimType claimType;

            if (collection is IUserClaimCollection userClaimCollection)
            {
                subjectId = userClaimCollection.UserId;
                claimType = ClaimType.User;
            }
            else if (collection is IRoleClaimCollection roleClaimCollection)
            {
                subjectId = roleClaimCollection.RoleId;
                claimType = ClaimType.Role;
            }
            else
            {
                return false;
            }

            using var dbContext = _dbFactory();

            var dbPermissions = dbContext.Permissions.AsNoTracking().Where(p => p.ServerId == collection.GuildId && p.SubjectType == claimType && p.SubjectId == subjectId);

            if (dbPermissions.Any())
            {
                return false;
            }

            var newPermissions = collection.Claims.Select(c => new DbPermission(claimType, subjectId, collection.GuildId, c.Identifier, c.ClaimedOperations));
            dbContext.Permissions.AddRange(newPermissions);
            return (await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false)) > 0;
        }

        public async Task<bool> DeleteClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken)
        {
            Func<DbPermission, bool> predicate;

            if (collection is IUserClaimCollection userClaimCollection)
            {
                predicate = p => p.ServerId == collection.GuildId && p.SubjectType == ClaimType.User && p.SubjectId == userClaimCollection.UserId;
            }
            else if (collection is IRoleClaimCollection roleClaimCollection)
            {
                predicate = p => p.ServerId == collection .GuildId && p.SubjectType == ClaimType.Role && p.SubjectId == roleClaimCollection.RoleId;
            }
            else
            {
                return false;
            }

            using var dbContext = _dbFactory();

            var dbPermissions = dbContext.Permissions.AsNoTracking().Where(predicate);
            dbContext.Permissions.RemoveRange(dbPermissions);
            return (await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false)) > 0;
        }

        public Task<bool> UpdateClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
