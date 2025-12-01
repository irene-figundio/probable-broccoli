using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Xml.Linq;
using WorkbotAI.Models.Abstractions;

namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public class ApplicationDbContext : DbContext
    {

        private readonly ICurrentUserService? _currentUser;
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService? currentUser = null)
            : base(options)
        {
            _currentUser = currentUser;
        }

        // All DbSets go here

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost;Database=workbot_ai;User Id=sa;Password=sa1;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    // e => EF.Property<bool>(e, "IsDeleted") == false
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProp = Expression.Call(
                        typeof(EF),
                        nameof(EF.Property),
                        new[] { typeof(bool) },
                        parameter,
                        Expression.Constant(nameof(ISoftDeletable.IsDeleted))
                    );
                    var body = Expression.Equal(isDeletedProp, Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ApplyAuditRules();
            return base.SaveChangesAsync(ct);
        }

        private void ApplyAuditRules()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var userId = _currentUser?.UserId;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable sd)
                {
                    entry.State = EntityState.Modified;
                    sd.IsDeleted = true;
                    sd.DeletionTime = utcNow;
                    sd.DeletionUserId = userId;
                    continue;
                }

                if (entry.State == EntityState.Modified && entry.Entity is IHasModificationAudit ma)
                {
                    if (HasRealChanges(entry, nameof(IHasModificationAudit.LastModificationTime),
                                             nameof(IHasModificationAudit.LastModificationUserId)))
                    {
                        ma.LastModificationTime = utcNow;
                        ma.LastModificationUserId = userId;
                    }
                }
            }
        }


        private static bool HasRealChanges(EntityEntry entry, params string[] auditPropertyNames)
        {
            // True se esiste almeno una proprietà modificata che NON è uno dei campi di audit
            var auditSet = new HashSet<string>(auditPropertyNames, StringComparer.Ordinal);
            return entry.Properties.Any(p => p.IsModified && !auditSet.Contains(p.Metadata.Name));
        }

        private void ApplySoftDeleteRules()
        {
            var utcNow = DateTimeOffset.Now;
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
            {
                if (entry.Entity is ISoftDeletable sd)
                {
                    // Converte la delete in update, settando i campi di soft delete
                    entry.State = EntityState.Modified;
                    sd.IsDeleted = true;
                    sd.DeletionTime = utcNow;
                    sd.DeletionUserId = _currentUser?.UserId;
                }
            }
        }

    }
}
