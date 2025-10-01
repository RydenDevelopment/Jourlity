using System.Text;
using Jourlity.Dto;
using Newtonsoft.Json;

namespace Jourlity.Core.Services;

public class BackendCommunicator : IBackendCommunicator
{
    private readonly HttpClient _client;


    public BackendCommunicator(HttpClient client)
    {
        _client = client;
    }

    public async Task LoggErrorToBackend(Exception ex, string message)
    {
        var package = new HttpMessage<Exception>
        {
            Message = message,
            Object = ex,
        };
        
        var json = JsonConvert.SerializeObject(package);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var result = await _client.PostAsync("/Logging", data);
        string resultContent = await result.Content.ReadAsStringAsync();
    }
}