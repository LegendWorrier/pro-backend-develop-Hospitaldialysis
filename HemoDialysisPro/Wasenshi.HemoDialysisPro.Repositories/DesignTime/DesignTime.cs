using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class DesignTime : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Server=127.0.0.1;Port=5432;Database=HemoDialysisPro;Integrated Security=true;")
                .Options);
        }
    }
}
