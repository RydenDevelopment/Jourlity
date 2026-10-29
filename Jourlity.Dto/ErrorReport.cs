namespace Jourlity.Dto;

/// <summary>
/// Error report sent from the app to the backend. It only carries metadata: exception type names and
/// method frames. It has no message field on purpose, since exception messages, Data and stack trace
/// file paths can contain client data.
/// </summary>
public class ErrorReport
{
    public DateTimeOffset Timestamp { get; set; }
    public string? AppVersion { get; set; }
    public string? OperatingSystem { get; set; }

    /// <summary>The reported exception first, followed by its inner exceptions.</summary>
    public List<ExceptionReport> Exceptions { get; set; } = [];
}
