// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

using System;
using System.Collections.Generic;

/// <summary>
///     Represents an interface for a test report, providing details about the outcome of a test execution.
/// </summary>
public interface ITestReport
{
    /// <summary>
    ///     Enum to categorize the type of test report, supporting multiple flags for combined states.
    /// </summary>
    [Flags]
    enum ReportType
    {
        /// <summary>
        ///     Indicates that the test was executed successfully without any issues.
        /// </summary>
        SUCCESS,

        /// <summary>
        ///     Indicates that the test finished with warnings, but no failures occurred.
        /// </summary>
        WARNING,

        /// <summary>
        ///     Indicates that the test failed due to some issues or errors.
        /// </summary>
        FAILURE,

        /// <summary>
        ///     Indicates that the test found orphan nodes.
        /// </summary>
        ORPHAN,

        /// <summary>
        ///     Denotes that the test was forcibly terminated before it completed execution.
        /// </summary>
        TERMINATED,

        /// <summary>
        ///     Indicates that the test execution was interrupted, possibly timeout or due to runtime conditions.
        /// </summary>
        INTERRUPTED,

        /// <summary>
        ///     Indicates that the test was aborted, typically due to unrecoverable errors.
        /// </summary>
        ABORT,

        /// <summary>
        ///     Marks the test as skipped and not executed during the test run.
        /// </summary>
        SKIPPED,

        /// <summary>
        ///     Represents standard output logs produced during the test execution.
        /// </summary>
        STDOUT
    }

    /// <summary>
    ///     Gets the type of the test report, indicating the outcome or status of the test.
    /// </summary>
    ReportType Type { get; }

    /// <summary>
    ///     Gets the line number in the test script or code where the report originated.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    ///     Gets the message associated with the test report, providing additional details.
    /// </summary>
    string Message { get; }

    /// <summary>
    ///     Gets the stack trace information in case of test failure or error, providing contextual details.
    /// </summary>
    string? StackTrace { get; }

    /// <summary>
    ///     Gets a value indicating whether the test report denotes an error occurred during execution.
    /// </summary>
    bool IsError { get; }

    /// <summary>
    ///     Gets a value indicating whether the test report denotes a failure.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    ///     Gets a value indicating whether the test report contains warnings.
    /// </summary>
    bool IsWarning { get; }

    /// <summary>
    ///     Serializes the test report into a dictionary representation containing its properties and values.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, object> Serialize();
}
