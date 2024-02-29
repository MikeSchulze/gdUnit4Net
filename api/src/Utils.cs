
namespace GdUnit4;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public sealed class Utils
{
    private static readonly Dictionary<Godot.Error, string> GodotErrors = new() {
        {Godot.Error.Failed, "Generic error."},
        {Godot.Error.Unavailable, "Unavailable error."},
        {Godot.Error.Unconfigured, "Unconfigured error."},
        {Godot.Error.Unauthorized, "Unauthorized error."},
        {Godot.Error.ParameterRangeError, "Parameter range error."},
        {Godot.Error.OutOfMemory, "Out of memory (OOM) error."},
        {Godot.Error.FileNotFound,"File: Not found error."},
        {Godot.Error.FileBadDrive, "File: Bad drive error."},
        {Godot.Error.FileBadPath, "File: Bad path error."},
        {Godot.Error.FileNoPermission, "File: No permission error."},
        {Godot.Error.FileAlreadyInUse, "File: Already in use error."},
        {Godot.Error.FileCantOpen,"File: Can't open error."},
        {Godot.Error.FileCantWrite, "File: Can't write error."},
        {Godot.Error.FileCantRead, "File: Can't read error."},
        {Godot.Error.FileUnrecognized, "File: Unrecognized error."},
        {Godot.Error.FileCorrupt, "File: Corrupt error."},
        {Godot.Error.FileMissingDependencies, "File: Missing dependencies error."},
        {Godot.Error.FileEof, "File: End of file (EOF) error."},
        {Godot.Error.CantOpen, "Can't open error."},
        {Godot.Error.CantCreate,"Can't create error."},
        {Godot.Error.QueryFailed, "Query failed error."},
        {Godot.Error.AlreadyInUse, "Already in use error."},
        {Godot.Error.Locked, "Locked error."},
        {Godot.Error.Timeout, "Timeout error."},
        {Godot.Error.CantConnect, "Can't connect error."},
        {Godot.Error.CantResolve, "Can't resolve error."},
        {Godot.Error.ConnectionError, "Connection error."},
        {Godot.Error.CantAcquireResource, "Can't acquire resource error."},
        {Godot.Error.CantFork, "Can't fork process error."},
        {Godot.Error.InvalidData, "Invalid data error."},
        {Godot.Error.InvalidParameter, "Invalid parameter error."},
        {Godot.Error.AlreadyExists, "Already exists error."},
        {Godot.Error.DoesNotExist, "Does not exist error."},
        {Godot.Error.DatabaseCantRead, "Database: Read error."},
        {Godot.Error.DatabaseCantWrite, "Database: Write error."},
        {Godot.Error.CompilationFailed, "Compilation failed error."},
        {Godot.Error.MethodNotFound, "Method not found error."},
        {Godot.Error.LinkFailed, "Linking failed error."},
        {Godot.Error.ScriptFailed, "Script failed error."},
        {Godot.Error.CyclicLink, "Cycling link (import cycle) error."},
        {Godot.Error.InvalidDeclaration, "Invalid declaration error."},
        {Godot.Error.DuplicateSymbol, " Duplicate symbol error."},
        {Godot.Error.ParseError, "Parse error."},
        {Godot.Error.Busy, "Busy error."},
        {Godot.Error.Skip, "Skip error."},
        {Godot.Error.Help, "Help error."},
        {Godot.Error.Bug, "Bug error."},
        {Godot.Error.PrinterOnFire, "Printer on fire error. (This is an easter egg, no engine methods return this error code.)"}
    };

    public static async Task<long> DoWait(long timeout)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        using (var tokenSource = new CancellationTokenSource())
        {
            await Task.Delay(TimeSpan.FromMilliseconds(timeout), tokenSource.Token);
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }


    private const string GDUNIT_TEMP = "user://tmp";

    internal static string GodotTempDir() =>
        Path.GetFullPath(Godot.ProjectSettings.GlobalizePath(GDUNIT_TEMP));

    /// <summary>
    /// Creates a temporary folder under Godot managed user directory
    /// </summary>
    /// <param name="path">a relative path</param>
    /// <returns>the full path to the created temp directory</returns>
    public static string CreateTempDir(string path)
    {
        var tempFolder = Path.Combine(GodotTempDir(), path);
        if (!new FileInfo(tempFolder).Exists)
            Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }

    /// <summary>
    /// Deletes the GdUnit temp directory recursively
    /// </summary>
    public static void ClearTempDir()
    {
        var tempFolder = GodotTempDir();
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);
    }

    /// <summary>
    /// Maps Godot error number to a readable error message. See at ERROR
    /// https://docs.godotengine.org/de/stable/classes/class_@globalscope.html#enum-globalscope-error
    /// </summary>
    /// <param name="errorNumber"></param>
    /// <returns></returns>
    public static string ErrorAsString(Godot.Error errorNumber) => GodotErrors.GetValueOrDefault(errorNumber, $"The error: {errorNumber} is not defined in Godot.");
    public static string ErrorAsString(int errorNumber) => ErrorAsString((Godot.Error)Enum.ToObject(typeof(Godot.Error), errorNumber));
}
