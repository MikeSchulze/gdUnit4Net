// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
// Need to be placed in the root namespace to be accessible by the test runner.
namespace GdUnit4;

/// <summary>
/// Registers the annotated field or property as a test extension, allowing it to be used for lifecycle callbacks and
/// parameter injection in the test suite. Using this method allows for the registration of extensions with constructor
/// arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RegisterExtensionAttribute : Attribute;
