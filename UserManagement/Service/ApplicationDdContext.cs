using Microsoft.EntityFrameworkCore;
using System.Collections;
using UserManagement.Model;

namespace UserManagement.Service
{
    public class ApplicationDdContext : DbContext
    {
        public DbSet<User> user { get; set; }
        public DbSet<Role> role { get; set; }

        public ApplicationDdContext(DbContextOptions<ApplicationDdContext> dbContext) : base(dbContext)
        {

        }
    }
}
