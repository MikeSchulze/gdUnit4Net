using System.Reflection;
using GdUnit4.Api;

namespace GdUnit4.Tests.Core.TestExtensions;

internal class ContextParameterResolver : IParameterResolver
{
    public bool SupportsParameter(ParameterInfo parameter, IExtensionContext context) =>
        parameter.ParameterType == typeof(IExtensionContext);

    public object? ResolveParameter(ParameterInfo parameter, IExtensionContext context) => context;
}
