using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GdUnit4.TestAdapter;

[DefaultExecutorUri(GdUnit4TestExecutor.ExecutorUri)]
[FileExtension(".dll")]
[FileExtension(".cs")]
public class GdUnit4TestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        Console.WriteLine("GdUnit4TestDiscoverer:DiscoverTests");
        Console.WriteLine(sources);
        // Logic to get the tests from the containers passed in.

        IEnumerable<TestCase> testsFound = new LinkedList<TestCase>();
        //Notify the test platform of the list of test cases found.
        foreach (string source in sources)
        {
            var assembly = Assembly.LoadFrom(source);
            Console.WriteLine(assembly);
            Console.WriteLine(assembly.GetTypes());
            assembly.GetTypes().Select(file =>
            {
                Console.WriteLine(file);
                return file;
            });

            Console.WriteLine(source);
            var test = new TestCase("UtilsTest", new Uri(GdUnit4TestExecutor.ExecutorUri), source);
            discoverySink.SendTestCase(test);
        }

    }
}