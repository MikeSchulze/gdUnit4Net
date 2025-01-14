namespace GdUnit4.Core.Discovery;

using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

/// <summary>
///     Provides source code navigation information for test methods by using debug information from PDB files.
/// </summary>
internal sealed class CodeNavigationDataProvider : IDisposable
{
    private readonly DiaSession diaSession;
    private bool disposed;

    /// <summary>
    ///     Initializes a new instance of the CodeNavigationDataProvider.
    /// </summary>
    /// <param name="assemblyPath">Path to the test assembly containing PDB debug information.</param>
    public CodeNavigationDataProvider(string assemblyPath) =>
        diaSession = new DiaSession(assemblyPath);

    public void Dispose()
    {
        if (disposed)
            return;
        diaSession.Dispose();
        disposed = true;
        // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Gets source code navigation information for a test method.
    /// </summary>
    /// <param name="mi">The method to get navigation data for.</param>
    /// <returns>Navigation information including source file and line number.</returns>
    public CodeNavigation GetNavigationData(MethodInfo mi)
    {
        var navigationData = TryGetNavigationDataForMethod(mi.DeclaringType!.FullName!, mi)
                             ?? TryGetNavigationDataForAsyncMethod(mi);
        return new CodeNavigation
        {
            LineNumber = navigationData?.MinLineNumber ?? -1,
            CodeFilePath = navigationData?.FileName,
            Method = mi
        };
    }

    private DiaNavigationData? TryGetNavigationDataForMethod(string className, MethodInfo methodInfo)
    {
        var navigationData = diaSession.GetNavigationData(className, methodInfo.Name);
        return string.IsNullOrEmpty(navigationData?.FileName) ? null : navigationData;
    }

    private DiaNavigationData? TryGetNavigationDataForAsyncMethod(MethodInfo methodInfo)
    {
        var stateMachineAttribute = GetStateMachineAttribute(methodInfo);
        if (stateMachineAttribute == null)
            return null;

        var stateMachineType = GetStateMachineType(stateMachineAttribute);
        return diaSession.GetNavigationData(stateMachineType?.FullName ?? "", "MoveNext");
    }

    private static Attribute? GetStateMachineAttribute(MethodInfo method) =>
        method.GetCustomAttributes(false)
            .Cast<Attribute>()
            .FirstOrDefault(attribute => attribute.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");

    private static Type? GetStateMachineType(Attribute stateMachineAttribute) =>
        stateMachineAttribute.GetType()
            .GetProperty("StateMachineType")?
            .GetValue(stateMachineAttribute) as Type ?? null;

    /// <summary>
    ///     Value type representing source code navigation information for a test method.
    /// </summary>
    public readonly struct CodeNavigation
    {
        /// <summary>
        ///     The method this navigation data refers to.
        /// </summary>
        public required MethodInfo Method { get; init; }

        /// <summary>
        ///     The line number in the source file where the method is defined.
        /// </summary>
        public required int LineNumber { get; init; }

        /// <summary>
        ///     The source code file path containing the method.
        /// </summary>
        public required string? CodeFilePath { get; init; }

        /// <summary>
        ///     Indicates if this navigation data contains valid source information.
        /// </summary>
        public readonly bool IsValid => CodeFilePath != null;

        public override string ToString()
            => $"""
                CodeNavigation:
                  Name: '{Method.Name}'
                  Line: {LineNumber}
                  CodeFilePath: '{CodeFilePath}';
                """;
    }
}
