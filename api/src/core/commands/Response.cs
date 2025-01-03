namespace GdUnit4.Core.Commands;

using System.Net;

public record Response
{
    public HttpStatusCode StatusCode { get; init; }
    public string Payload { get; init; } = "";
}
