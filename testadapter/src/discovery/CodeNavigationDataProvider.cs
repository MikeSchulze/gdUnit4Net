namespace GdUnit4.TestAdapter.Discovery;

using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

internal sealed class CodeNavigationDataProvider : IDisposable
{
    private readonly Assembly assembly;
    private readonly DiaSession diaSession;
    private bool disposed;

    public CodeNavigationDataProvider(string assemblyPath)
    {
        assembly = Assembly.LoadFrom(assemblyPath);
        diaSession = new DiaSession(assemblyPath);
    }

    public void Dispose()
    {
        if (disposed)
            return;
        diaSession.Dispose();
        disposed = true;
        // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
        GC.SuppressFinalize(this);
    }

    public CodeNavigation GetNavigationData(string managedType, string managedMethod)
    {
        var mi = GetMethodInfo(managedType, managedMethod);
        var navigationData = TryGetNavigationDataForMethod(managedType, mi)
                             ?? TryGetNavigationDataForAsyncMethod(mi);
        return new CodeNavigation
        {
            Line = navigationData?.MinLineNumber ?? -1,
            Source = navigationData?.FileName,
            Method = mi
        };
    }

    private MethodInfo GetMethodInfo(string managedType, string managedMethod) =>
        assembly.GetType(managedType)!.GetMethod(managedMethod)!;

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


    public readonly struct CodeNavigation
    {
        public required MethodInfo Method { get; init; }
        public required int Line { get; init; }
        public required string? Source { get; init; }
        public readonly bool IsValid => Source != null;

        public override string ToString()
            => $"""
                CodeNavigation:
                  Name: {Method.Name}
                  Line: {Line}
                  Source: {Source}"";
                """;
    }
}
