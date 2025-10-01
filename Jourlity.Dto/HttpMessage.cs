using System.Net;

namespace Jourlity.Dto;

public class HttpMessage<T> where T : class
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public string? Message { get; set; }
    public T? Object { get; set; }
}