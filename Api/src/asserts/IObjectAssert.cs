// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Asserts;

using Constraints;

/// <summary>
///     An Assertion Tool to verify object values.
/// </summary>
public interface IObjectAssert : IObjectConstraint, IAssertMessage<IObjectConstraint>
{
}
