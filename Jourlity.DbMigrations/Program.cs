using Jourlity.Data.Context;
using Jourlity.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.DbMigrations;
using Jourlity.Data;

/// <summary>
/// Use the following command at the solution level in command prompt:
/// dotnet ef migrations add InitialCreate --startup-project Jourlity.DbMigrations --project Jourlity.Data
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        var dbPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jourlity.db"));
        
        var options = new DbContextOptionsBuilder<JourlityContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using (var context = new JourlityContext(options))
        {
            context.Database.EnsureCreated();
            CreateTable(context);
            InsertUsers(context);
            DisplayAllUsers(context);
            DeleteUserByName(context, "Otto");
            DisplayAllUsers(context);
        }

        File.Delete(dbPath);
        Console.WriteLine("Database file deleted!");
    }

    private static void DeleteUserByName(JourlityContext context, string name)
    {
        var user = context.Clients.SingleOrDefault(u => u.Name == name);
        if (user != null)
        {
            context.Clients.Remove(user);
            context.SaveChanges();
            Console.WriteLine($"User with name '{name}' deleted.");
        }
    }

    private static void DisplayAllUsers(JourlityContext context)
    {
        var clients = context.Clients;
        Console.WriteLine("Current users in the database:");
        foreach (var client in clients)
        {
            Console.WriteLine($"ID: {client.ClientId}, Name: {client.Name}, Email {client.Email}");
        }
    }

    private static void InsertUsers(JourlityContext context)
    {
        var clients = new[]
        {
            new Client { ClientId = Guid.Empty, Name = "Otto", Email = "Otto@test.se"},
            new Client { ClientId = Guid.Empty, Name = "Tim", Email = "Tim@test.se" },
            new Client { ClientId = Guid.Empty, Name = "Steve", Email = "Steve@test.se"},
            new Client { ClientId = Guid.Empty, Name = "Robert", Email = "Robert@test.se"}
        };

        context.Clients.AddRange(clients);
        context.SaveChanges();
        Console.WriteLine("Users inserted.");
    }

    private static void CreateTable(JourlityContext context)
    {
        // Table creation is handled by EnsureCreated method
        Console.WriteLine("Table created.");
    }
}
