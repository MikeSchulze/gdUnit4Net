// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionRegistryTest
{
    #region Helpers

    // Used to create ExtensionContext instances for testing without needing a full test execution environment.
    private static ExtensionContext CreateSuiteContext(Type suiteType) =>
        new(suiteType, new TestSuiteNode
        {
            ManagedType = "null",
            Tests = [],
            AssemblyPath = "null",
            SourceFile = "null",
            Id = Guid.NewGuid(),
            ParentId = Guid.NewGuid()
        });


    private static Tuple<ExtensionContext, ExtensionContext> CreateTestContext(Type suiteType, string methodName,
        List<object?>? testCaseArguments = null)
    {
        var testMethod = suiteType.GetMethod(methodName);
        if (testMethod is null)
            throw new InvalidOperationException($"Method '{methodName}' not found on '{suiteType.Name}'");

        var suiteContext = CreateSuiteContext(suiteType);
        var testContext = new ExtensionContext(suiteContext, testMethod, methodName, testCaseArguments ?? []);
        return new Tuple<ExtensionContext, ExtensionContext>(suiteContext, testContext);
    }


    // Used to collect extension execution data
    private static readonly List<string> Log = [];

    #endregion

    #region ExtensionImplementations

    private class LogExtension(string prefix)
        : IBeforeAllCallback, IAfterAllCallback, IBeforeEachCallback, IAfterEachCallback
    {
        public Task BeforeAll(IExtensionContext context)
        {
            Log.Add($"{prefix}.BeforeAll");
            return Task.CompletedTask;
        }


        public Task AfterAll(IExtensionContext context)
        {
            Log.Add($"{prefix}.AfterAll");
            return Task.CompletedTask;
        }


        public Task BeforeEach(IExtensionContext context)
        {
            Log.Add($"{prefix}.BeforeEach");
            return Task.CompletedTask;
        }


        public Task AfterEach(IExtensionContext context)
        {
            Log.Add($"{prefix}.AfterEach");
            return Task.CompletedTask;
        }
    }

    // Extension with all callbacks implemented, used to verify callback invocation and ordering via log entries.
    private class LogA() : LogExtension("A");

    private class LogB() : LogExtension("B");

    private class LogC() : LogExtension("C");

    private class LogD() : LogExtension("D");
    
    // Extension with only test-level (before/after each) callbacks implemented
    private class LogTestLevelOnly : IBeforeEachCallback, IAfterEachCallback
    {
        public Task BeforeEach(IExtensionContext ctx)
        {
            Log.Add("Method.BeforeEach");
            return Task.CompletedTask;
        }


        public Task AfterEach(IExtensionContext ctx)
        {
            Log.Add("Method.AfterEach");
            return Task.CompletedTask;
        }
    }


    // Extension that takes a callback to invoke for all lifecycle methods, used to verify that [RegisterExtension]
    // fields/properties are properly registered and invoked.
    private class CallbackExtension(Action callback)
        : IBeforeAllCallback, IAfterAllCallback, IBeforeEachCallback, IAfterEachCallback
    {
        public Task BeforeAll(IExtensionContext ctx)
        {
            callback();
            return Task.CompletedTask;
        }


        public Task AfterAll(IExtensionContext ctx)
        {
            callback();
            return Task.CompletedTask;
        }


        public Task BeforeEach(IExtensionContext ctx)
        {
            callback();
            return Task.CompletedTask;
        }


        public Task AfterEach(IExtensionContext ctx)
        {
            callback();
            return Task.CompletedTask;
        }
    }

    // Constructed parameter resolver that always resolves string parameters to a fixed value,
    // used to verify parameter resolution logic and precedence over test case arguments.
    private class AlwaysStringResolver(string value) : IParameterResolver
    {
        public bool SupportsParameter(IParameterContext p, IExtensionContext ctx)
            => p.GetParameterInfo().ParameterType == typeof(string);


        public object ResolveParameter(IParameterContext p, IExtensionContext ctx) => value;
    }

    #endregion

    #region TestSuites

    // Extension-less test suite
    private class NoExtensionSuite;

    [ExtendWith<LogA>]
    private class AllExtensionsSuite
    {
        [RegisterExtension] public static readonly ITestExtension StaticFieldExt = new LogExtension("StaticField");

        [RegisterExtension]
        public static ITestExtension StaticPropertyExt { get; } = new LogExtension("StaticProperty");


        [TestCase]
        public void TestCase()
        {
        }
    }

    // Test suite with a single class-level extension
    [ExtendWith<LogA>] private class SingleClassLevelSuite;

    // Test suite with two class-level extensions, used to verify that multiple extensions are supported and
    // invoked in the correct order.
    [ExtendWith<LogA>] [ExtendWith<LogB>] private class TwoClassLevelSuite;

    // Test suite with an extension registered via [RegisterExtension] on an instance field.
    private class InstanceFieldSuite(ITestExtension ext)
    {
        [RegisterExtension] public readonly ITestExtension Ext = ext;
    }

    // Test suite with an extension registered via [RegisterExtension] on a static field.
    private class StaticFieldSuite
    {
        [RegisterExtension] public static ITestExtension? Ext;
    }

    // Test suite with an extension registered via [RegisterExtension] on an instance property,
    private class InstancePropertySuite(ITestExtension ext)
    {
        [RegisterExtension] public ITestExtension Ext { get; } = ext;
    }

    // Test suite with an extension registered via [RegisterExtension] on a static property,
    private class StaticPropertySuite
    {
        [RegisterExtension] public static ITestExtension? Ext { get; set; }
    }

    // Test suite with a method level extension, and another extension registered via [RegisterExtension] on an
    // instance field, used to verify extension execution and ordering.
    private class MethodRegistrationSuite(ITestExtension suiteExt)
    {
        [RegisterExtension] public readonly ITestExtension SuiteExt = suiteExt;


        [TestCase]
        public void MethodWithExtension()
        {
        }


        [TestCase]
        public void PlainMethod()
        {
        }
    }

    // Test suite with no parameter resolvers registered, used to verify that test case arguments are passed forward.
    private class NoResolverSuite
    {
        [TestCase]
        public void NoParams()
        {
        }


        [TestCase]
        public void StringParam(string s)
        {
        }
    }

    // Test suite with a constructed parameter resolver registered via [RegisterExtension], used to verify parameter
    // resolution logic,
    private class ResolverSuite(IParameterResolver resolver)
    {
        [RegisterExtension] public readonly IParameterResolver Resolver = resolver;


        [TestCase]
        public void StringParam(string s)
        {
        }


        [TestCase]
        public void StringAndIntParams(string s, int n)
        {
        }


        [TestCase]
        public void UnresolvableParam(UnresolvableType p)
        {
        }
    }

    // Dummy type used to verify that an exception is thrown when a parameter cannot be resolved by any means.
    private sealed class UnresolvableType;

    [ExtendWith<LogA>] [ExtendWith<LogB>] private abstract class AbstractSuite;
    
    [ExtendWith<LogC>] [ExtendWith<LogD>] private class ConcreteSuite : AbstractSuite;
    
    #endregion

    #region Hooks

    [BeforeTest] public void BeforeTest() => Log.Clear();

    #endregion

    #region TestRunBeforeAll

    [TestCase]
    public async Task RunBeforeAll_SuiteWithNoExtensions_IsNoOp()
    {
        var suite = new NoExtensionSuite();
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(NoExtensionSuite)));

        AssertThat(Log.Count).IsEqual(0);
    }


    [TestCase]
    public async Task RunBeforeAll_SingleClassLevelExtendWith_ExtensionIsInvoked()
    {
        var suite = new SingleClassLevelSuite();
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(SingleClassLevelSuite)));

        AssertThat(Log.Count).IsEqual(1);
        AssertThat(Log[0]).IsEqual("A.BeforeAll");
    }


    [TestCase]
    public async Task RunBeforeAll_TwoClassLevelExtendWith_BothInvokedInDeclarationOrder()
    {
        var suite = new TwoClassLevelSuite();
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(TwoClassLevelSuite)));

        AssertThat(Log.Count).IsEqual(2);
        AssertThat(Log[0]).IsEqual("A.BeforeAll");
        AssertThat(Log[1]).IsEqual("B.BeforeAll");
    }


    [TestCase]
    public async Task RunBeforeAll_RegisteredInstanceField_ExtensionIsInvoked()
    {
        var invoked = false;
        var suite = new InstanceFieldSuite(new CallbackExtension(() => invoked = true));
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(InstanceFieldSuite)));

        AssertBool(invoked).IsTrue();
    }


    [TestCase]
    public async Task RunBeforeAll_RegisteredStaticField_ExtensionIsInvoked()
    {
        var invoked = false;
        StaticFieldSuite.Ext = new CallbackExtension(() => invoked = true);
        var suite = new StaticFieldSuite();
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(StaticFieldSuite)));

        AssertBool(invoked).IsTrue();
    }


    [TestCase]
    public async Task RunBeforeAll_RegisteredInstanceProperty_ExtensionIsInvoked()
    {
        var invoked = false;
        var suite = new InstancePropertySuite(new CallbackExtension(() => invoked = true));
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(InstancePropertySuite)));

        AssertBool(invoked).IsTrue();
    }


    [TestCase]
    public async Task RunBeforeAll_RegisteredStaticProperty_ExtensionIsInvoked()
    {
        var invoked = false;
        StaticPropertySuite.Ext = new CallbackExtension(() => invoked = true);
        var suite = new StaticPropertySuite();
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(StaticPropertySuite)));

        AssertBool(invoked).IsTrue();
    }


    [TestCase]
    public async Task RunBeforeAll_ExtensionWithoutBeforeAllCallback_IsNotInvoked()
    {
        var suite = new InstanceFieldSuite(new LogTestLevelOnly());
        var registry = new ExtensionRegistry();

        await registry.RunBeforeAll(CreateSuiteContext(typeof(InstanceFieldSuite)));

        AssertThat(Log.Count).IsEqual(0);
    }

    #endregion

    #region TestRunBeforeEach

    [TestCase]
    public async Task RunBeforeEach_VerifyExtensionExecutionOrder_AllExtensionTypes()
    {
        var registry = new ExtensionRegistry();

        var (_, testContext) = CreateTestContext(typeof(AllExtensionsSuite), nameof(AllExtensionsSuite.TestCase));
        await registry.RunBeforeEach(testContext);

        AssertThat(Log.Count).IsEqual(3);
        AssertThat(Log[0]).IsEqual("A.BeforeEach"); // Class-level extension from [ExtendWith]
        AssertThat(Log[1]).IsEqual("StaticField.BeforeEach"); // Static field extension from [RegisterExtension]
        AssertThat(Log[2]).IsEqual("StaticProperty.BeforeEach"); // Static property extension from [RegisterExtension]
    }

    #endregion

    #region TestRunAfterAll

    [TestCase]
    public async Task RunAfterAll_TwoClassLevelExtendWith_InvokedInReverseDeclarationOrder()
    {
        var registry = new ExtensionRegistry();

        await registry.RunAfterAll(CreateSuiteContext(typeof(TwoClassLevelSuite)));

        AssertThat(Log.Count).IsEqual(2);
        AssertThat(Log[0]).IsEqual("B.AfterAll");
        AssertThat(Log[1]).IsEqual("A.AfterAll");
    }

    #endregion
    
    #region TestAttributeInheritance

    [TestCase]
    public async Task TestInheritedExtensionAttributes_ShouldExecuteBeforeEachInCorrectOrder()
    {
        var registry = new ExtensionRegistry();
        
        await registry.RunBeforeEach(CreateSuiteContext(typeof(ConcreteSuite)));
        
        AssertThat(Log.Count).IsEqual(4);
        AssertThat(Log[0]).IsEqual("A.BeforeEach");
        AssertThat(Log[1]).IsEqual("B.BeforeEach");
        AssertThat(Log[2]).IsEqual("C.BeforeEach");
        AssertThat(Log[3]).IsEqual("D.BeforeEach");
    }

    [TestCase]
    public async Task TestInheritedExtensionAttributes_ShouldExecuteAfterEachInCorrectOrder()
    {
        var registry = new ExtensionRegistry();
        
        await registry.RunAfterEach(CreateSuiteContext(typeof(ConcreteSuite)));
        
        AssertThat(Log.Count).IsEqual(4);
        AssertThat(Log[0]).IsEqual("D.AfterEach");
        AssertThat(Log[1]).IsEqual("C.AfterEach");
        AssertThat(Log[2]).IsEqual("B.AfterEach");
        AssertThat(Log[3]).IsEqual("A.AfterEach");
    }
    
    #endregion
}
