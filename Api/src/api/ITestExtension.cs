// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

#pragma warning disable CA1040
/// <summary>
///     Marker interface that all test extensions implement.
///     Extensions add lifecycle callbacks and parameter injection to test suites.
/// </summary>
/// <remarks>
///     Register extensions via <see cref="ExtendWithAttribute{T}" /> or <see cref="RegisterExtensionAttribute" />.
/// </remarks>
public interface ITestExtension;
#pragma warning restore CA1040
