namespace GdUnit4;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Godot;

public static class Utils
{
    private const string GD_UNIT_TEMP = "user://tmp";

    public static async Task<long> DoWait(long timeout)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        using (var tokenSource = new CancellationTokenSource()) await Task.Delay(TimeSpan.FromMilliseconds(timeout), tokenSource.Token);

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    internal static string GodotTempDir() => Path.Combine(Path.GetTempPath(), "gdUnit4Net");

    /// <summary>
    ///     Creates a temporary folder under Godot managed user directory
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
    ///     Deletes the GdUnit temp directory recursively
    /// </summary>
    public static void ClearTempDir()
    {
        var tempFolder = GodotTempDir();
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);
    }

    /// <summary>
    ///     Maps Godot error number to a readable error message. See at ERROR
    ///     https://docs.godotengine.org/de/stable/classes/class_@globalscope.html#enum-globalscope-error
    /// </summary>
    /// <param name="error">The Godot error</param>
    /// <returns>The error as string presentation if available</returns>
    public static string ErrorAsString(Error error) => error.ToString();

    public static string ErrorAsString(int error) => !Enum.IsDefined(typeof(Error), Convert.ToInt64(error))
        ? $"The error: {error} is not defined in Godot."
        : ErrorAsString((Error)Enum.ToObject(typeof(Error), error));
}
