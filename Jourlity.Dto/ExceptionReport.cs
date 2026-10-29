namespace Jourlity.Dto;

public class ExceptionReport
{
    /// <summary>Full type name of the exception, e.g. System.InvalidOperationException.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Method frames as Namespace.Type.Method, without file paths or line numbers.</summary>
    public List<string> StackFrames { get; set; } = [];
}
