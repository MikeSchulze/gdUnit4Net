using System.Collections.Generic;
using System.Threading.Tasks;
using GdUnit4.Api;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<ClassLevelExtension0>]
[ExtendWith<ClassLevelExtension1>]
public class ExtensionExecutionOrderTest
{
    private static readonly List<string?> executionOrder = [];

    [RegisterExtension]
    private static readonly FieldExtension0 FieldExtension0Instance = new();

    [RegisterExtension]
    private static readonly FieldExtension1 FieldExtension1Instance = new();

    [Before]
    public void ResetExecutionOrder()
    {
        // Belt-and-suspenders clear in case the suite is re-run in the same process
        // without a fresh BeforeAll firing from ClassLevelExtension0.
        executionOrder.Clear();
    }

    [TestCase]
    [ExtendWith<MethodExtension0>]
    [ExtendWith<MethodExtension1>]
    public void TestExtensionExecutionOrder()
    {
        Assertions.AssertThat(executionOrder.Count)
            .IsEqual(6);

        // Execution order should be:
        // 1. ClassLevelExtension0
        // 2. ClassLevelExtension1
        // 3. FieldExtension0
        // 4. FieldExtension1
        // 5. MethodExtension0
        // 6. MethodExtension1
        for (var i = 0; i < executionOrder.Count; i++)
        {
            var expectedExtensionType = i switch
            {
                0 => typeof(ClassLevelExtension0).FullName,
                1 => typeof(ClassLevelExtension1).FullName,
                2 => typeof(FieldExtension0).FullName,
                3 => typeof(FieldExtension1).FullName,
                4 => typeof(MethodExtension0).FullName,
                5 => typeof(MethodExtension1).FullName,
                _ => null
            };

            Assertions.AssertThat(executionOrder[i])
                .IsEqual(expectedExtensionType!);
        }
    }

    private static void PushExecution(ITestExtension extension)
    {
        executionOrder.Add(extension.GetType().FullName);
    }

    private abstract class ExecutionPushExtension : IBeforeEachCallback
    {
        public Task BeforeEach(IExtensionContext context)
        {
            PushExecution(this);
            return Task.CompletedTask;
        }
    }

    private class ClassLevelExtension0 : ExecutionPushExtension, IBeforeAllCallback
    {
        public Task BeforeAll(IExtensionContext context)
        {
            executionOrder.Clear();
            return Task.CompletedTask;
        }
    }

    private class ClassLevelExtension1 : ExecutionPushExtension;

    private class FieldExtension0 : ExecutionPushExtension;

    private class FieldExtension1 : ExecutionPushExtension;

    private class MethodExtension0 : ExecutionPushExtension;

    private class MethodExtension1 : ExecutionPushExtension;
}
