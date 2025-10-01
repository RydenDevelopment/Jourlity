using Jourlity.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.Data.Context;

public class CaseContext : DbContext
{
    private readonly string _folderPath;

    public DbSet<Entry> Entry { get; set; }
    
    public CaseContext()
    {
        this._folderPath = "";
    }
    
    public CaseContext(string folderPath)
    {
        this._folderPath = folderPath;
    }
        
    public CaseContext(DbContextOptions options) : base(options)
    {
        this._folderPath = "";
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) 
            return;
        
        var caseFolder = GetBasePathForContext(_folderPath);
        
        Directory.CreateDirectory(caseFolder);
            
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(caseFolder, "database.sqlite"),
            Mode = SqliteOpenMode.ReadWriteCreate, 
        }.ToString();

        optionsBuilder.UseSqlite(connectionString);
    }

    private static string GetBasePathForContext(string folderPath)
    {
        var applicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jourlity");

        applicationFolder = Path.Combine(applicationFolder, folderPath);

        return applicationFolder;
    }
}