namespace GdUnit4.Api;

using System;

public record TestNode
{
    /// <summary>
    ///     Gets the unique identifier for this test case.
    /// </summary>
    public required Guid Id { get; init; }


    public required Guid ParentId { get; set; }
}
