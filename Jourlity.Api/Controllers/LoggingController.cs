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
    public HttpMessage<object> Post(HttpMessage<Exception> content)
    {
        _logger.LogCritical(content.Object, content.Object?.Message);

        return new HttpMessage<object>
        {
            HttpStatusCode = HttpStatusCode.Created,
            Message = "Success",
            Object = null
        };
    }
}