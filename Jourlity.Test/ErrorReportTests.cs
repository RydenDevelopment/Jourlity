using System.Net;
using Jourlity.Core.Services;
using Newtonsoft.Json;

namespace Jourlity.Test;

public class ErrorReportTests
{
    private const string ClientName = "Anna Andersson";
    private const string EntryContent = "Diagnosed with something confidential";
    private static readonly string CasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jourlity", "Anna Andersson case", "case.sqlite");

    private static readonly string[] Secrets = [ClientName, EntryContent, CasePath, "Andersson", "Jourlity\\Anna", "case.sqlite"];

    [Test]
    public void Report_DoesNotContainMessage()
    {
        var ex = Capture(() => throw new InvalidOperationException($"Could not save entry for {ClientName}: {EntryContent}"));

        AssertContainsNoSecrets(ErrorReportBuilder.Build(ex));
    }

    [Test]
    public void Report_DoesNotContainData()
    {
        var ex = Capture(() =>
        {
            var inner = new InvalidOperationException("Failed");
            inner.Data["Client"] = ClientName;
            inner.Data[ClientName] = CasePath;
            throw inner;
        });

        AssertContainsNoSecrets(ErrorReportBuilder.Build(ex));
    }

    [Test]
    public void Report_DoesNotContainFilePaths()
    {
        var ex = Capture(() => throw new FileNotFoundException($"Could not find {CasePath}", CasePath));

        var report = ErrorReportBuilder.Build(ex);

        AssertContainsNoSecrets(report);
        Assert.That(report.Exceptions[0].Type, Is.EqualTo(typeof(FileNotFoundException).FullName));
    }

    [Test]
    public void Report_DoesNotContainInnerExceptionDetails()
    {
        var ex = Capture(() =>
        {
            try
            {
                ThrowWithClientData();
            }
            catch (Exception inner)
            {
                throw new ApplicationException("Saving failed", inner);
            }
        });

        var report = ErrorReportBuilder.Build(ex);

        AssertContainsNoSecrets(report);
        Assert.That(report.Exceptions.Select(e => e.Type), Is.EqualTo(new[]
        {
            typeof(ApplicationException).FullName,
            typeof(IOException).FullName,
        }));
    }

    [Test]
    public void Report_DoesNotContainAggregatedExceptionDetails()
    {
        var ex = Capture(() => throw new AggregateException(
            Capture(ThrowWithClientData),
            new ArgumentException(ClientName, CasePath)));

        var report = ErrorReportBuilder.Build(ex);

        AssertContainsNoSecrets(report);
        Assert.That(report.Exceptions, Has.Count.EqualTo(3));
    }

    [Test]
    public void Report_KeepsTypesAndMethodFrames()
    {
        var ex = Capture(ThrowWithClientData);

        var report = ErrorReportBuilder.Build(ex);

        Assert.That(report.Exceptions[0].Type, Is.EqualTo(typeof(IOException).FullName));
        Assert.That(report.Exceptions[0].StackFrames,
            Has.Some.EqualTo($"{typeof(ErrorReportTests).FullName}.{nameof(ThrowWithClientData)}"));
        Assert.That(report.Exceptions[0].StackFrames, Has.None.Contains(".cs"));
    }

    [Test]
    public async Task BackendCommunicator_PostsOnlySanitizedReport()
    {
        var handler = new CapturingHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost/Api/") };
        var communicator = new BackendCommunicator(client);

        var ex = Capture(() =>
        {
            var e = new InvalidOperationException($"{ClientName}: {EntryContent}", Capture(ThrowWithClientData));
            e.Data["Path"] = CasePath;
            throw e;
        });

        await communicator.LoggErrorToBackend(ex);

        Assert.That(handler.RequestUri, Is.EqualTo(new Uri("https://localhost/Api/Logg")));
        Assert.That(handler.Body, Is.Not.Null.And.Not.Empty);
        foreach (var secret in Secrets)
            Assert.That(handler.Body, Does.Not.Contain(secret).IgnoreCase);
        Assert.That(handler.Body, Does.Contain(typeof(InvalidOperationException).FullName));
    }

    private static void ThrowWithClientData()
    {
        var e = new IOException($"Could not open {CasePath} for {ClientName}");
        e.Data["Entry"] = EntryContent;
        throw e;
    }

    private static Exception Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            return e;
        }

        throw new AssertionException("Expected the action to throw");
    }

    private static void AssertContainsNoSecrets(object report)
    {
        var json = JsonConvert.SerializeObject(report);

        foreach (var secret in Secrets)
            Assert.That(json, Does.Not.Contain(secret).IgnoreCase);
    }

    private class CapturingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
