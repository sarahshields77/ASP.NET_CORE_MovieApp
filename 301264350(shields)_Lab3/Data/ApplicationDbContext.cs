using _301264350_shields__Lab3.Models;
using Microsoft.EntityFrameworkCore;

namespace _301264350_shields__Lab3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public new DbSet<User> Users { get; set; } // my custom User class
    }
}
