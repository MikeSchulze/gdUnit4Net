namespace NUnit.Extension.GdUnit4.Driver;

using System.Reflection;

using NUnit.Engine;
using NUnit.Engine.Extensibility;

[Extension(Enabled = true)]
public class GdUnit4DriverService : IDriverService
{
    public IFrameworkDriver GetDriver(AppDomain domain, string assemblyPath, string targetFramework, bool skipNonTestAssemblies)
    {
        // Only create our driver if the assembly contains GodotTest attributes
        var assembly = Assembly.LoadFrom(assemblyPath);
        if (!HasGodotTests(assembly) && skipNonTestAssemblies)
            return null;

        return new GdUnit4Driver(assembly);
    }

    private bool HasGodotTests(Assembly assembly)
    {
        try
        {
            // Check if assembly contains any methods with GodotTest attribute
            return assembly.GetTypes()
                .Any(t => t.GetMethods()
                    .Any(m => m.GetCustomAttributes(typeof(GodotTestAttribute), false).Any()));
        }
        catch
        {
            return false;
        }
    }
}

public class GdUnit4Driver : IFrameworkDriver
{
    private readonly Assembly _assembly;

    public GdUnit4Driver(Assembly assembly) => _assembly = assembly;

    public string ID => "GdUnit4";
    string IFrameworkDriver.ID { get; set; }

    public string Load(string testAssemblyPath, IDictionary<string, object> settings)
    {
        // Load assembly and discover tests
        var assembly = Assembly.LoadFrom(testAssemblyPath);
        return DiscoverTests(assembly);
    }

    public int CountTestCases(string filter) => throw new NotImplementedException();

    public string Explore(string filter) => Load(_assembly.Location, new Dictionary<string, object>());

    public string Run(ITestEventListener listener, string filter) =>
        // Implement test execution logic
        "<test-run/>";

    public void StopRun(bool force) { }


    private string DiscoverTests(Assembly assembly) =>
        // Create XML representation of discovered tests
        @"<test-suite id='1' name='GodotTests' fullname='GodotTests' type='Assembly'>
            <test-case id='2' name='ExampleTest' fullname='ExampleTest' methodname='ExampleTest'/>
        </test-suite>";
}
