namespace GdUnit4.Analyzers.Test;

using System.Runtime.CompilerServices;

using Gu.Roslyn.Asserts;

using Microsoft.CodeAnalysis;

internal static class ModuleInitializer
{
    internal static readonly MetadataReference[] References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(typeof(TestSuiteAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(CompilerGeneratedAttribute).Assembly.Location)
    ];

    [ModuleInitializer]
    internal static void Initialize() => Settings.Default = Settings.Default
        .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS0281", "CS1701", "CS1702", "CS8019"))
        .WithMetadataReferences(MetadataReferences.Transitive(typeof(ModuleInitializer)));
}
