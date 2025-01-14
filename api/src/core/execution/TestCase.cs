namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Extensions;

internal sealed class TestCase
{
    public TestCase(Guid id, MethodInfo methodInfo, int lineNumber, int attributeIndex)
    {
        Id = id;
        MethodInfo = methodInfo;
        Line = lineNumber;
        Parameters = InitialParameters();
        TestCaseAttribute = TestCaseAttributes[attributeIndex];
    }

    public string Name => MethodInfo.Name;

    public Guid Id { get; }

    public int Line
    {
        get;
        private set;
    }

    public List<TestCaseAttribute> TestCaseAttributes
        => MethodInfo.GetCustomAttributes<TestCaseAttribute>().Where(TestParametersFilter).ToList();

    private Func<TestCaseAttribute, bool> TestParametersFilter { get; } = _ => true;

    public TestCaseAttribute TestCaseAttribute { get; init; }

    internal bool IsParameterized => TestCaseAttributes.Any(p => p.Arguments.Length > 0);

    internal bool HasDataPoint => DataPoint != null;

    internal DataPointAttribute? DataPoint => MethodInfo.GetCustomAttribute<DataPointAttribute>();

    public bool IsSkipped => Attribute.IsDefined(MethodInfo, typeof(IgnoreUntilAttribute));

    private IEnumerable<object> Parameters
    {
        get;
    }

    public MethodInfo MethodInfo
    {
        get;
        set;
    }

    public object?[] Arguments => IsParameterized ? TestCaseAttribute.Arguments : Parameters.SelectMany(ResolveParam).ToArray();

    private IEnumerable<object> ResolveParam(object input)
    {
        if (input is IValueProvider provider) return provider.GetValues();
        return new[] { input };
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

    internal static string BuildDisplayName(string testName, TestCaseAttribute attribute, int attributeIndex = 0, bool withAttributeIndex = false)
    {
        var name = attribute.TestName ?? testName;
        if (withAttributeIndex) return $"{name} #{attributeIndex}";

        if (attribute.Arguments.Length <= 0) return name;
        var parameters = string.Join(", ", attribute.Arguments.Select(GdUnitExtensions.Formatted));
        return $"{name} ({parameters})";
    }

    internal static string BuildDisplayName(string testName)
        => testName;

    internal static string BuildFullyQualifiedName(string classNameSpace, string testName, TestCaseAttribute attr)
    {
        if (attr.Arguments.Length == 0)
            return $"{classNameSpace}.{attr.TestName ?? testName}";
        var parameterizedTestName = BuildDisplayName(testName, attr, -1);
        return $"{classNameSpace}.{testName}.{parameterizedTestName}";
    }
}
