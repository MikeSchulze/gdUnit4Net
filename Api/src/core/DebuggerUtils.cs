// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core;

using System.Reflection;

using Godot.NativeInterop;

internal static class DebuggerUtils
{
    private static readonly MethodInfo? DebuggerIsActiveMethod;

    static DebuggerUtils()
    {
        var nativeFuncType = typeof(NativeFuncs);
        DebuggerIsActiveMethod = nativeFuncType.GetMethod(
            "godotsharp_internal_script_debugger_is_active",
            BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static bool IsDebuggerActive()
    {
        if (DebuggerIsActiveMethod == null)
            return false;

        var isDebuggerActive = (godot_bool)DebuggerIsActiveMethod.Invoke(null, null)!;
        return isDebuggerActive.ToBool();
    }
}
