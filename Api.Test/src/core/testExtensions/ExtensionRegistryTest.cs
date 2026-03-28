using System;
using System.Collections.Generic;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

using System.Threading.Tasks;

using Api;

using GdUnit4.Core.TestExtensions;

[TestSuite]
public class ExtensionRegistryTest
{
    #region Mock extension classes for testing

    private abstract class MockExtension(List<Type> invocationLog) 
        : IBeforeAllCallback, IBeforeEachCallback, IAfterEachCallback, IAfterAllCallback
    {
        public Task BeforeEach(IExtensionContext context)
        {
            invocationLog.Add(GetType());
            return Task.CompletedTask;
        }

        public Task BeforeAll(IExtensionContext context)
        {
            invocationLog.Add(GetType());
            return Task.CompletedTask;
        }

        public Task AfterEach(IExtensionContext context)
        {
            invocationLog.Add(GetType());
            return Task.CompletedTask;
        }

        public Task AfterAll(IExtensionContext context)
        {
            invocationLog.Add(GetType());
            return Task.CompletedTask;
        }
    }

    private class ExtensionA() : MockExtension(InvokedExtensions);

    private class ExtensionB() : MockExtension(InvokedExtensions);

    private class ExtensionC() : MockExtension(InvokedExtensions);

    private class ExtensionD() : MockExtension(InvokedExtensions);

    #endregion

    #region Mock suite classes for testing

    [ExtendWith<ExtensionA>]
    private class SuiteWithExtension;

    private class SuiteWithRegisterExtension
    {
        // ReSharper disable once UnusedMember.Local
        [RegisterExtension] private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    private class SuiteWithExtensionsAndRegistration
    {
        // ReSharper disable once UnusedMember.Local
        [RegisterExtension] private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    [ExtendWith<ExtensionB>]
    private class AbstractSuite;

    [ExtendWith<ExtensionC>]
    [ExtendWith<ExtensionD>]
    private class SuiteWithExtensionsInherits : AbstractSuite;

    private class SuiteWithNoExtensions;

    #endregion

    #region Test setup

    private static readonly List<Type> InvokedExtensions = [];

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private ExtensionRegistry extensionRegistry = null!;

    [BeforeTest]
    public void SetUpExtensions()
    {
        extensionRegistry = new ExtensionRegistry();
        InvokedExtensions.Clear();
    }

    #endregion

    #region Test FindTestExtensions

    [TestCase]
    public void FindTestExtensions_OnSuiteWithNoExtensions()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithNoExtensions));

        AssertThat(result).IsEmpty();
    }


    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtension));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtensionsAndRegistration()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtensionsAndRegistration));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtensionsInherits()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtensionsInherits));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB), typeof(ExtensionC), typeof(ExtensionD));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithRegisterExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithRegisterExtension));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionB));
    }

    #endregion
    
    #region Test run methods

    [TestCase]
    public async Task RunBeforeAll_ShouldExecuteExtensionsInCorrectOrder()
    {
        extensionRegistry.FindTestExtensions(typeof(SuiteWithExtensionsInherits));
        
        var context = new ExtensionContext(
            typeof(SuiteWithExtensionsInherits),
            new TestSuiteNode
            {
                ManagedType = "null",
                Tests = [],
                AssemblyPath = "null",
                SourceFile = "null",
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            });
        
        await extensionRegistry.RunBeforeAll(context);
        
        AssertThat(InvokedExtensions)
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB), typeof(ExtensionC), typeof(ExtensionD));
    }

    [TestCase]
    public async Task RunBeforeEach_ShouldExecuteExtensionsInCorrectOrder()
    {
        extensionRegistry.FindTestExtensions(typeof(SuiteWithExtensionsInherits));
        
        var context = new ExtensionContext(
            typeof(SuiteWithExtensionsInherits),
            new TestSuiteNode
            {
                ManagedType = "null",
                Tests = [],
                AssemblyPath = "null",
                SourceFile = "null",
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            });
        
        await extensionRegistry.RunBeforeEach(context);
        
        AssertThat(InvokedExtensions)
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB), typeof(ExtensionC), typeof(ExtensionD));
    }

    [TestCase]
    public async Task RunAfterEach_ShouldExecuteExtensionsInReverseOrder()
    {
        extensionRegistry.FindTestExtensions(typeof(SuiteWithExtensionsInherits));
        
        var context = new ExtensionContext(
            typeof(SuiteWithExtensionsInherits),
            new TestSuiteNode
            {
                ManagedType = "null",
                Tests = [],
                AssemblyPath = "null",
                SourceFile = "null",
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            });
        
        await extensionRegistry.RunAfterEach(context);
        
        AssertThat(InvokedExtensions)
            .ContainsExactly(typeof(ExtensionD), typeof(ExtensionC), typeof(ExtensionB), typeof(ExtensionA));
    }

    [TestCase]
    public async Task RunAfterAll_ShouldExecuteExtensionsInReverseOrder()
    {
        extensionRegistry.FindTestExtensions(typeof(SuiteWithExtensionsInherits));
        
        var context = new ExtensionContext(
            typeof(SuiteWithExtensionsInherits),
            new TestSuiteNode
            {
                ManagedType = "null",
                Tests = [],
                AssemblyPath = "null",
                SourceFile = "null",
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            });
        
        await extensionRegistry.RunAfterAll(context);
        
        AssertThat(InvokedExtensions)
            .ContainsExactly(typeof(ExtensionD), typeof(ExtensionC), typeof(ExtensionB), typeof(ExtensionA));
    }
    
    #endregion
}
