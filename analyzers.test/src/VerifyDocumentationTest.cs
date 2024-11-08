namespace GdUnit4.Analyzers.Test;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DiagnosticsDocumentationTest
{
    private static readonly string ProjectRoot = Directory.GetCurrentDirectory()
        .Split(Path.DirectorySeparatorChar)
        .TakeWhile(dir => dir != "analyzers.test")
        .Concat(new[] { "analyzers" })
        .Aggregate(Path.Combine);

    [TestMethod]
    [DynamicData(nameof(GetDiagnosticDescriptors), DynamicDataSourceType.Method, DynamicDataDisplayName = "GetCustomDynamicDataDisplayName")]
    public void CheckDataPointAttributeDocumentation(DiagnosticDescriptor descriptor, string name)
    {
        var docLink = descriptor.HelpLinkUri.Replace("https://github.com/MikeSchulze/gdUnit4Net/tree/master/analyzers/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
        var docPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), ProjectRoot, docLink);

        Assert.IsTrue(File.Exists(docPath), $"Documentation file not found: {docPath}\nExpected documentation for rule {descriptor.Id}");

        var docContent = File.ReadAllText(docPath);

        // Check if the first line of the document contains the required header
        var lines = docContent.Split('\r', '\n');
        var firstLine = lines.FirstOrDefault();
        var expectedHeader = $"# {descriptor.Id}";

        Assert.IsTrue(firstLine == expectedHeader, $"required header '{expectedHeader}' on the first line.");
        Assert.IsTrue(lines.Length > 2 && string.IsNullOrEmpty(lines[1]), "Expected an empty line after the header.");
        StringAssert.Contains(docContent, "# Problem Description", "Documentation should contain ` Problem Description` section");
        StringAssert.Contains(docContent, $"## {descriptor.Title.ToString()}", "Documentation should contain the diagnostic title");
        StringAssert.Contains(docContent, descriptor.Description.ToString(), "Documentation should contain the diagnostic description");
    }

    // ReSharper disable once UnusedMember.Global
    public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data) => $"Attribute-{data[1]}";

    private static IEnumerable<object[]> GetDiagnosticDescriptors()
    {
        var type = typeof(DiagnosticRules.DataPoint);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            if (field.FieldType == typeof(DiagnosticDescriptor) && field.IsInitOnly) // IsInitOnly checks if it's readonly
                yield return new[] { field.GetValue(null)!, field.Name.Replace("Attribute", "") }; // Wrap in an object array as required by DynamicData
    }
}
