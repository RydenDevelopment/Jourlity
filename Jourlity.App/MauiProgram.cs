using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using Jourlity.Core.Services;
using Microsoft.Extensions.Logging;
using Jourlity.Data.Context;
using Jourlity.Data.Entities;
using Jourlity.Data.Repository;
using Jourlity.Dto;
using Newtonsoft.Json;
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
        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("********** OMG! FirstChanceException **********");
            System.Diagnostics.Debug.WriteLine(e.Exception);
        };
#endif

        SettingUpDatabase(builder);
        SettingUpServices(builder);

        var app = builder.Build();
        
        return app;
    }

    private static void SettingUpServices(MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();
        
        builder.Services.AddHttpClient("Backend", client =>
        {
            var backendUrl = Environment.GetEnvironmentVariable("BackendUrl");
            if (backendUrl == null)
                throw new KeyNotFoundException("No backend url has been configured");
            
            client.BaseAddress = new Uri(backendUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        
        builder.Services.AddScoped<IRepository<Client>, Repository<Client, JourlityContext>>();
        builder.Services.AddScoped<IRepository<Entry>, Repository<Entry, CaseContext>>();

        builder.Services.AddScoped<IBackendCommunicator, BackendCommunicator>();
    }

    private static void SettingUpDatabase(MauiAppBuilder builder)
    {
        builder.Services.AddDbContext<JourlityContext>();
        builder.Services.AddDbContext<CaseContext>();
    }
    
}
