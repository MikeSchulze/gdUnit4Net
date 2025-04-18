﻿namespace GdUnit4.Api;

using System;
using System.Collections.Generic;

/// <summary>
///     Defines the core identification properties of a test event.
/// </summary>
public interface ITestEvent
{
    /// <summary>
    ///     Defines the different types of test events.
    /// </summary>
    public enum EventType
    {
        Init,
        Stop,
        SuiteBefore,
        SuiteAfter,
        TestBefore,
        TestAfter,
        DiscoverStart,
        DiscoverEnd
    }

    /// <summary>
    ///     Gets the type of test event.
    /// </summary>
    EventType Type { get; }

    /// <summary>
    ///     Gets the unique identifier for this test event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     The full qualified test name, used for console logging.
    /// </summary>
    string FullyQualifiedName { get; }

    /// <summary>
    ///     The test display name. Used for data driven test e.g. DataPointAttribute
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    ///     Gets whether the test failed.
    /// </summary>
    bool IsFailed { get; }

    /// <summary>
    ///     Gets whether the test encountered an error.
    /// </summary>
    bool IsError { get; }

    /// <summary>
    ///     Gets whether the test completed successfully.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    ///     Gets whether the test produced warnings.
    /// </summary>
    bool IsWarning { get; }

    /// <summary>
    ///     Gets whether the test was skipped.
    /// </summary>
    bool IsSkipped { get; }

    /// <summary>
    ///     Gets the elapsed time of the test execution.
    /// </summary>
    TimeSpan ElapsedInMs { get; }

    /// <summary>
    ///     Gets the collection of reports associated with the test event.
    ///     Each report provides details about various aspects of the test's execution.
    /// </summary>
    List<ITestReport> Reports { get; }
}
