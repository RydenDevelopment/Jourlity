namespace Jourlity.Core.Services;

public interface IBackendCommunicator
{
    /// <summary>
    /// Sends a sanitized report of the exception to the backend. Only type names and method frames
    /// leave the device, never the message, Data or file paths.
    /// </summary>
    public Task LoggErrorToBackend(Exception ex);
}