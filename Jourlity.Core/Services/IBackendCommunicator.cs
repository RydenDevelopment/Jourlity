namespace Jourlity.Core.Services;

public interface IBackendCommunicator
{
    public Task LoggErrorToBackend(Exception ex, string message);
}