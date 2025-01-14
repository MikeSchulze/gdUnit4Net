namespace GdUnit4.Core.Commands;

using System.Net;

/// <summary>
///     Represents a response from a command execution.
///     Contains status code and optional payload data.
/// </summary>
public record Response
{
    /// <summary>
    ///     HTTP status code indicating the result of the command execution.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }


    /// <summary>
    ///     Optional payload containing command-specific response data.
    /// </summary>
    public string Payload { get; init; } = "";
}
