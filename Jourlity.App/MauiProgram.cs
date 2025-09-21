using Microsoft.Extensions.Logging;
using Jourlity.Data.Context;
using Jourlity.Data.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jourlity.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        SettingUpDatabase(builder);

        SettingUpServices(builder);

        var app = builder.Build();
        
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JourlityContext>();
        dbContext.Database.MigrateAsync();

        return app;
    }

    private static void SettingUpServices(MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();
        
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    private static void SettingUpDatabase(MauiAppBuilder builder)
    {
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appSpecificFolder = Path.Combine(appDataDir, "Jourlity");
        
        Directory.CreateDirectory(appSpecificFolder);
        
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(Path.Combine(appSpecificFolder), "database.sqlite"),
            Mode = SqliteOpenMode.ReadWriteCreate, 
        }.ToString();
        
        builder.Services.AddDbContext<JourlityContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
    }
}
