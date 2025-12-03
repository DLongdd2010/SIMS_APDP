using Microsoft.EntityFrameworkCore;
namespace SIMS_APDP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //public DbSet<User> Users { get; set; }
        //// Add other DbSets here for other entities
    }
}
