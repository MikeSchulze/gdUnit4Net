// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution.Monitoring;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using Exceptions;

using Extensions;

using Godot;

using FileAccess = System.IO.FileAccess;

public class GodotExceptionMonitor
{
    // Types of exceptions that should be ignored during test execution
    private static readonly HashSet<Type> IgnoredExceptionTypes = new()
    {
        typeof(TestFailedException)

        // typeof(AssertFailedException),
        // typeof(UnitTestAssertException)
    };

    private static readonly List<Exception> CaughtExceptions = new();

    private readonly string godotLogFile;
    private long eof;

    public GodotExceptionMonitor()
    {
        godotLogFile = ProjectSettings.GlobalizePath((string)ProjectSettings.GetSetting("debug/file_logging/log_path"));
        if (!(IsLogFileAvailable = File.Exists(godotLogFile)))
            Console.WriteLine($"The Godot logfile is not available: {godotLogFile}");
    }

    private bool IsLogFileAvailable { get; }

    public void Start()
    {
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        if (!IsLogFileAvailable)
            return;

        try
        {
            using var fileStream = new FileStream(godotLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);
            fileStream.Seek(0, SeekOrigin.End);
            eof = fileStream.Length;
        }
        catch (IOException ex)
        {
            GD.PrintErr($"Failed to open log file: {ex.Message}");
        }
    }

    public async Task StopThrow()
    {
        AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;

        if (!IsLogFileAvailable)
            return;

        // we need to wait the current Godot main tread has processed all nodes
        await GodotObjectExtensions.SyncProcessFrame;

        try
        {
            foreach (var logEntry in ScanGodotLogFile())
            {
                switch (logEntry.EntryType)
                {
                    case ErrorLogEntry.ErrorType.Exception:
                        var exception = CaughtExceptions.FirstOrDefault(e => e.GetType() == logEntry.ExceptionType && e.Message == logEntry.Message);
                        if (exception != null)
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        break;
                    case ErrorLogEntry.ErrorType.PushError:
                        throw TestFailedException.FromPushError(logEntry.Message, logEntry.Details);
                    case ErrorLogEntry.ErrorType.PushWarning:
                        break;
                    default:
                        break;
                }
            }
        }
        finally
        {
            CaughtExceptions.Clear();
        }
    }

    private void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        if (ShouldIgnoreException(e.Exception))
            return;
        if (IsSceneProcessing())
            CaughtExceptions.Add(e.Exception);
    }

    private static bool ShouldIgnoreException(Exception ex)
    {
        if (ex is TargetInvocationException)
        {
            var ei = ExceptionDispatchInfo.Capture(ex.InnerException ?? ex);
            return ShouldIgnoreException(ei.SourceException);
        }

        return IgnoredExceptionTypes.Contains(ex.GetType());
    }

    private static bool IsSceneProcessing()
    {
        // Look for scene processing methods in the stack trace
        var stackTrace = new StackTrace(true);
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            if (method == null)
                continue;

            var declaringType = method.DeclaringType;
            if (declaringType == null)
                continue;

            // Check for scene processing methods
            if (IsSceneProcessingMethod(method.Name, declaringType))
                return true;
        }

        return false;
    }

    private static bool IsSceneProcessingMethod(string methodName, Type declaringType)
    {
        // Check for common Godot processing methods
        if (methodName is "_Process" or "_PhysicsProcess" or "_Input" or "_UnhandledInput" or "_Ready" or "InvokeGodotClassMethod")

            // Check if the declaring type is or inherits from Node
            return typeof(Node).IsAssignableFrom(declaringType) || typeof(RefCounted).IsAssignableFrom(declaringType);
        return false;
    }

    private List<ErrorLogEntry> ScanGodotLogFile()
    {
        var logEntries = new List<ErrorLogEntry?>();
        try
        {
            using var fileStream = new FileStream(godotLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);

            // Seek to the last known position
            fileStream.Seek(eof, SeekOrigin.Begin);

            var records = new List<string>();
            while (reader.ReadLine() is { } line)
                records.Add(line);

            // Update the EOF position
            eof = fileStream.Position;

            for (var index = 0; index < records.Count; index++)
            {
                var extractException = ErrorLogEntry.ExtractException(records.ToArray(), index);
                if (extractException != null)
                {
                    logEntries.Add(extractException);
                    continue;
                }

                var extractPushError = ErrorLogEntry.ExtractPushError(records.ToArray(), index);
                if (extractPushError != null)
                {
                    logEntries.Add(extractPushError);
                    continue;
                }

                logEntries.Add(ErrorLogEntry.ExtractPushWarning(records.ToArray(), index));
            }
        }
        catch (IOException ex)
        {
            GD.PrintErr($"Failed to read log file: {ex.Message}");
        }

        return logEntries.Where(e => e != null).ToList()!;
    }
}
