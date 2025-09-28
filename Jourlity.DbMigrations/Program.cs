using Jourlity.Data.Context;
using Jourlity.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.DbMigrations;
using Jourlity.Data;

/// <summary>
/// Use the following command at the solution level in command prompt:
/// dotnet ef migrations add [migration name] --startup-project Jourlity.DbMigrations --project Jourlity.Data --JourlityContext
/// dotnet ef migrations add [migration name] --startup-project Jourlity.DbMigrations --project Jourlity.Data --CaseContext
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        using var context = new JourlityContext();

        if (context.Database.EnsureCreated())
        {
            context.Database.Migrate();
        }
        
        CreateTable(context);
        InsertUsers(context);
        CreateCaseDb(context);
        DisplayAllUsers(context);
        //DeleteUserByName(context, "Otto");
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
            Console.WriteLine($"ID: {client.Id}, Name: {client.Name}, Email {client.Email}");
        }
    }

    private static void InsertUsers(JourlityContext context)
    {
        var clients = new[]
        {
            new Client { Id = Guid.Empty, Name = "Kim", Email = "kim@ryden.dev", DbPath = "Kim", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Client { Id = Guid.Empty, Name = "Otto", Email = "Otto@test.se", DbPath = "Otto", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            new Client { Id = Guid.Empty, Name = "Tim", Email = "Tim@test.se", DbPath = "Tim", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now  },
            new Client { Id = Guid.Empty, Name = "Steve", Email = "Steve@test.se", DbPath = "Steve", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now  },
            new Client { Id = Guid.Empty, Name = "Robert", Email = "Robert@test.se", DbPath = "Robert", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now  }
        };

        context.Clients.AddRange(clients);
        context.SaveChanges();
        Console.WriteLine("Users inserted.");
    }
    
    private static void CreateCaseDb(JourlityContext context)
    {
        var dbPaths = context.Clients.Select(x => x.DbPath).ToList();
        foreach (var dbPath in dbPaths)
        {
            using var caseContext = new CaseContext(dbPath);
            caseContext.Database.EnsureCreated();
        }
    }

    private static void CreateTable(JourlityContext context)
    {
        // Table creation is handled by EnsureCreated method
        Console.WriteLine("Table created.");
    }
}
