using Microsoft.EntityFrameworkCore;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repository.Core;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    internal class ContextModelSetup : ContextModelSetup<User, Role>
    {
        public override ModelBuilder SetupModel(ModelBuilder builder)
        {
            base.SetupModel(builder);

            builder.Entity<User>(c =>
            {
                c.OwnsMany(x => x.RefreshTokens, m =>
                {
                    m.ToTable(nameof(RefreshToken), c => c.ExcludeFromMigrations());
                });
            });

            return builder;
        }
    }
}
