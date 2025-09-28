using Jourlity.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.Data.Context
{
    public class JourlityContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        
        public JourlityContext()
        {
            
        }
        
        public JourlityContext(DbContextOptions options) : base(SetOptions(options))
        {
            
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) 
                return;
        
            var applicationFolder = GetBasePathForContext();
            
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(applicationFolder, "database.sqlite"),
                Mode = SqliteOpenMode.ReadWriteCreate, 
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);
        }

        private static DbContextOptions<JourlityContext> SetOptions(DbContextOptions options)
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appSpecificFolder = Path.Combine(appDataDir, "Jourlity");
        
            Directory.CreateDirectory(appSpecificFolder);
        
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(Path.Combine(appSpecificFolder), "database.sqlite"),
                Mode = SqliteOpenMode.ReadWriteCreate, 
            }.ToString();
            
            return new DbContextOptionsBuilder<JourlityContext>()
                .UseSqlite(connectionString)
                .Options;
        }
        
        private static string GetBasePathForContext()
        {
            var applicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jourlity");

            return applicationFolder;
        }
    }
}
