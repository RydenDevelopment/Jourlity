using Jourlity.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.Data.Context
{
    public class JourlityContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        
        public JourlityContext()
        {
            
        }
        
        public JourlityContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
