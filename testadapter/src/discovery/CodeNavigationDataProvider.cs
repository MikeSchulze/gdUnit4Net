namespace GdUnit4.TestAdapter.Discovery;

using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

public class CodeNavigationDataProvider : IDisposable
{

    public struct CodeNavigation
    {
        public int Line { get; set; }
        public string? Source { get; set; }
        public readonly bool IsValid => Source != null;
    }

    private readonly Assembly? assembly;
    private readonly DiaSession diaSession;

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

    public CodeNavigation GetNavigationData(string className, MethodInfo methodInfo)
    {
        var navigationData = TryGetNavigationDataForMethod(className, methodInfo)
            ?? TryGetNavigationDataForAsyncMethod(methodInfo);
        return new CodeNavigation()
        {
            Line = navigationData?.MinLineNumber ?? -1,
            Source = navigationData?.FileName
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

    public void Dispose()
    {
        diaSession?.Dispose();
        GC.SuppressFinalize(this);
    }
}
