
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;
using System;

namespace Jourlity.Data.Context
{
    public class JourlityContextFactory : IDesignTimeDbContextFactory<JourlityContext>
    {
        public JourlityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<JourlityContext>();
            
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jourlity/database.sqlite")),
                Mode = SqliteOpenMode.ReadWriteCreate, 
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);

            return new JourlityContext(optionsBuilder.Options);
        }
    }
}
