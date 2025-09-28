using Microsoft.Extensions.Logging;
using Jourlity.Data.Context;
using Jourlity.Data.Entities;
using Jourlity.Data.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Entry = Jourlity.Data.Entities.Entry;

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

        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("********** OMG! FirstChanceException **********");
            System.Diagnostics.Debug.WriteLine(e.Exception);
        };
        
        return app;
    }

    private static void SettingUpServices(MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();
        
        builder.Services.AddScoped<IRepository<Client>, Repository<Client, JourlityContext>>();
        builder.Services.AddScoped<IRepository<Entry>, Repository<Entry, CaseContext>>();
    }

    private static void SettingUpDatabase(MauiAppBuilder builder)
    {
        builder.Services.AddDbContext<JourlityContext>();
        builder.Services.AddDbContext<CaseContext>();
    }
}
