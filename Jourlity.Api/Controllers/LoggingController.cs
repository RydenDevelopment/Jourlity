using System.Net;
using Jourlity.Dto;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class LoggController : ControllerBase
{
    private readonly ILogger<LoggController> _logger;

    public LoggController(ILogger<LoggController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    public HttpMessage<object> Post(ErrorReport report)
    {
        var exceptions = report.Exceptions
            .Select(e => string.Join($"{Environment.NewLine}   at ", e.StackFrames.Prepend(e.Type)));

        _logger.LogCritical("Client error report from {AppVersion} on {OperatingSystem} at {Timestamp}{NewLine}{Exceptions}",
            report.AppVersion, report.OperatingSystem, report.Timestamp, Environment.NewLine,
            string.Join($"{Environment.NewLine} ---> ", exceptions));

        return new HttpMessage<object>
        {
            HttpStatusCode = HttpStatusCode.Created,
            Message = "Success",
            Object = null
        };
    }
}