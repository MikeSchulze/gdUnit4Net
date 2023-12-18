using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GdUnit4.TestAdapter.Discovery;

public class CodeNavigationDataProvider : IDisposable
{

    public struct CodeNavigation
    {
        public int Line { get; set; }
        public string? Source { get; set; }
        public readonly bool IsValid => Source != null;
    }

    internal readonly Assembly? assembly;
    internal readonly DiaSession diaSession;

    public CodeNavigationDataProvider(string assemblyPath, IMessageLogger logger)
    {
        try
        {
            assembly = Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception e)
        {
            logger.SendMessage(TestMessageLevel.Error, e.Message);
        }
        diaSession = new(assemblyPath);
    }

    public Assembly? GetAssembly() => assembly;

    public CodeNavigation GetNavigationData(string className, string methodName)
    {
        var navigationData = TryGetNavigationDataForMethod(className, methodName)
            ?? TryGetNavigationDataForAsyncMethod(className, methodName);
        return new CodeNavigation()
        {
            Line = navigationData?.MinLineNumber ?? -1,
            Source = navigationData?.FileName
        };
    }

    private DiaNavigationData? TryGetNavigationDataForMethod(string className, string methodName)
    {
        var navigationData = diaSession.GetNavigationData(className, methodName);
        return string.IsNullOrEmpty(navigationData?.FileName) ? null : navigationData;
    }

    private DiaNavigationData? TryGetNavigationDataForAsyncMethod(string className, string methodName)
    {
        var methodInfo = GetMethod(className, methodName);
        if (methodInfo == null) return null;

        var stateMachineAttribute = GetStateMachineAttribute(methodInfo);
        if (stateMachineAttribute == null) return null;

        var stateMachineType = GetStateMachineType(stateMachineAttribute);
        return diaSession.GetNavigationData(stateMachineType?.FullName ?? "", "MoveNext");
    }

    MethodInfo? GetMethod(string className, string methodName) =>
        assembly!.GetType(className)?
           .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
           .Where(info => info.Name == methodName)
           .OrderBy(info => info.GetParameters().Length)
           .FirstOrDefault();

    static Attribute? GetStateMachineAttribute(MethodInfo method) =>
            method.GetCustomAttributes(false)
                .Cast<Attribute>()
                .FirstOrDefault(attribute => attribute.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");

    static Type? GetStateMachineType(Attribute stateMachineAttribute) =>
        stateMachineAttribute.GetType()
            .GetProperty("StateMachineType")?
            .GetValue(stateMachineAttribute) as Type ?? null;

    public void Dispose()
    {
        diaSession?.Dispose();
    }
}
