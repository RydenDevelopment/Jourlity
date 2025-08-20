using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Jourlity.App;
using Jourlity.App.Data;
using Jourlity.App.Data.Interface;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOidcAuthentication(async options =>
{
    var secrets = new ClientSecrets()
    {
        ClientId = "CLIENT_ID",
        ClientSecret = "CLIENT_SECRET"
    };

    var token = new TokenResponse { RefreshToken = "REFRESH_TOKEN" }; 
    var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer 
            {
                ClientSecrets = secrets
            }), 
        "user", 
        token);

    var service = new DriveService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credentials,
        ApplicationName = "TestProject"
    });
    
    builder.Services.AddSingleton(service);
    builder.Services.AddScoped(typeof(IRepository<>), typeof(GoogleDriveRepository<>));
    builder.Configuration.Bind("Local", options.ProviderOptions);
});

await builder.Build().RunAsync();