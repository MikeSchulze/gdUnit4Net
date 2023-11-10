using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GdUnit4.TestAdapter;

[ExtensionUri(ExecutorUri)]
public class GdUnit4TestExecutor : ITestExecutor
{

    ///<summary>
    /// The Uri used to identify the NUnitExecutor
    ///</summary>
    public const string ExecutorUri = "executor://GdUnit4TestAdapter";

    /// <summary>
    /// Runs only the tests specified by parameter 'tests'. 
    /// </summary>
    /// <param name="tests">Tests to be run.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle) { }

    /// <summary>
    /// Runs 'all' the tests present in the specified 'containers'. 
    /// </summary>
    /// <param name="containers">Path to test container files to look for tests in.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<string>? containers, IRunContext? runContext, IFrameworkHandle? frameworkHandle) { }

    /// <summary>
    /// Cancel the execution of the tests.
    /// </summary>
    public void Cancel() { }

}
