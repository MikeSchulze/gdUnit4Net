namespace GdUnit4.Executions;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

internal sealed class TestCase
{
    public TestCase(MethodInfo methodInfo, int lineNumber)
    {
        MethodInfo = methodInfo;
        Line = lineNumber;
        Parameters = InitialParameters();
    }

    public string Name => MethodInfo.Name;

    public int Line
    { get; private set; }

    public IEnumerable<TestCaseAttribute> TestCaseAttributes
        => MethodInfo.GetCustomAttributes<TestCaseAttribute>().Where(TestParametersFilter);

    public Func<TestCaseAttribute, bool> TestParametersFilter { get; set; } = _ => true;

    public TestCaseAttribute TestCaseAttribute => MethodInfo.GetCustomAttribute<TestCaseAttribute>()!;

    internal bool IsParameterized => TestCaseAttributes.Any(p => p.Arguments.Length > 0);

    public bool IsSkipped => Attribute.IsDefined(MethodInfo, typeof(IgnoreUntilAttribute));

    private IEnumerable<object> Parameters
    { get; set; }

    public MethodInfo MethodInfo
    { get; set; }

    private IEnumerable<object> ResolveParam(object input)
    {
        if (input is IValueProvider provider)
        {
            return provider.GetValues();
        }
        return new object[] { input };
    }

    private List<object> InitialParameters()
        => MethodInfo.GetParameters()
            .SelectMany(pi => pi.GetCustomAttributesData()
                .Where(attr => attr.AttributeType == typeof(FuzzerAttribute))
                .Select(attr =>
                {
                    var arguments = attr.ConstructorArguments.Select(arg => arg.Value).ToArray();
                    return attr.Constructor.Invoke(arguments);
                }
            )
         ).ToList();

    public object[] Arguments => Parameters.SelectMany(ResolveParam).ToArray();

    internal static string BuildDisplayName(string testName, TestCaseAttribute attribute)
    {
        var name = attribute.TestName ?? testName;
        if (attribute.Arguments.Length > 0)
        {
            var parameters = string.Join(", ", attribute!.Arguments.Select(GdUnitExtensions.Formatted));
            return $"{name}({parameters})";
        }
        return name;
    }

    internal static string BuildDisplayName(string testName)
        => testName;

    internal static string BuildDisplayName(string testName, params object[] arguments)
    {
        if (arguments.Length > 0)
        {
            var parameters = string.Join(", ", arguments.Select(GdUnitExtensions.Formatted));
            return $"{testName}({parameters})";
        }
        return testName;
    }

    internal static string BuildFullyQualifiedName(string classNameSpace, string testName, TestCaseAttribute? attr)
    {
        if (attr == null || attr.Arguments.Length == 0)
            return $"{classNameSpace}.{testName}";
        var parameterizedTestName = BuildDisplayName(testName, attr);
        return $"{classNameSpace}.{testName}.{parameterizedTestName}";
    }
}
