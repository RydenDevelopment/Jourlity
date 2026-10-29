using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Jourlity.Dto;

namespace Jourlity.Core.Services;

/// <summary>
/// Turns an exception into an <see cref="ErrorReport"/> that is safe to send off the device.
/// Only type names and method frames are kept. Message, Data, Source, HelpLink and file paths are
/// dropped, since they can contain client names, entry content or case folder paths.
/// </summary>
public static class ErrorReportBuilder
{
    private const int MaxExceptions = 10;
    private const int MaxFrames = 64;

    public static ErrorReport Build(Exception ex)
    {
        return Build(ex, GetAppVersion(), RuntimeInformation.OSDescription, DateTimeOffset.UtcNow);
    }

    public static ErrorReport Build(Exception ex, string? appVersion, string? operatingSystem, DateTimeOffset timestamp)
    {
        var report = new ErrorReport
        {
            Timestamp = timestamp,
            AppVersion = appVersion,
            OperatingSystem = operatingSystem,
        };

        var seen = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        var queue = new Queue<Exception>();
        queue.Enqueue(ex);

        while (queue.Count > 0 && report.Exceptions.Count < MaxExceptions)
        {
            var current = queue.Dequeue();
            if (!seen.Add(current))
                continue;

            report.Exceptions.Add(new ExceptionReport
            {
                Type = current.GetType().FullName ?? current.GetType().Name,
                StackFrames = GetFrames(current),
            });

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    queue.Enqueue(inner);
            }
            else if (current.InnerException != null)
            {
                queue.Enqueue(current.InnerException);
            }
        }

        return report;
    }

    private static List<string> GetFrames(Exception ex)
    {
        // fNeedFileInfo: false so no file names or line numbers are read
        var frames = new StackTrace(ex, false).GetFrames();

        return frames
            .Take(MaxFrames)
            .Select(FormatFrame)
            .ToList();
    }

    private static string FormatFrame(StackFrame frame)
    {
        var method = frame.GetMethod();
        if (method == null)
            return "<unknown>";

        var type = method.DeclaringType?.ToString();
        return type == null ? method.Name : $"{type}.{method.Name}";
    }

    private static string? GetAppVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    }
}
