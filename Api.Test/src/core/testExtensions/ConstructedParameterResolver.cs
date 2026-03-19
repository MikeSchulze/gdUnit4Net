using System.Reflection;
using GdUnit4.Api;

namespace GdUnit4.Tests.Core.TestExtensions;

internal class ConstructedParameterResolver(string constructorParam) : IParameterResolver
{
    public bool SupportsParameter(ParameterInfo parameter, IExtensionContext context) => 
        parameter.ParameterType == typeof(string) && parameter.Name == "constructorParam";


    public object? ResolveParameter(ParameterInfo parameter, IExtensionContext context) => constructorParam;
}
