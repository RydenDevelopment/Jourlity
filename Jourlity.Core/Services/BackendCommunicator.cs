using System.Text;
using Newtonsoft.Json;

namespace Jourlity.Core.Services;

public class BackendCommunicator : IBackendCommunicator
{
    private readonly HttpClient _client;


    public BackendCommunicator(HttpClient client)
    {
        _client = client;
    }

    public async Task LoggErrorToBackend(Exception ex)
    {
        // Never serialize the exception itself, it can carry client data. Send the sanitized report only.
        var report = ErrorReportBuilder.Build(ex);

        var json = JsonConvert.SerializeObject(report);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        // Relative path (no leading slash) so it resolves under the BaseAddress, e.g. .../Api/Logg
        var result = await _client.PostAsync("Logg", data);
        result.EnsureSuccessStatusCode();
    }
}