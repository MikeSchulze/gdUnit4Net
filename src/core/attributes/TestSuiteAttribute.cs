using System;

namespace GdUnit3
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestSuiteAttribute : TestStageAttribute
    {
        public TestSuiteAttribute([System.Runtime.CompilerServices.CallerLineNumber] int line = 0, [System.Runtime.CompilerServices.CallerMemberName] string name = "") : base(name, line)
        { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class BeforeAttribute : TestStageAttribute
    {
        public BeforeAttribute([System.Runtime.CompilerServices.CallerLineNumber] int line = 0, [System.Runtime.CompilerServices.CallerMemberName] string name = "") : base(name, line)
        { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AfterAttribute : TestStageAttribute
    {
        public AfterAttribute([System.Runtime.CompilerServices.CallerLineNumber] int line = 0, [System.Runtime.CompilerServices.CallerMemberName] string name = "") : base(name, line)
        { }
    }
}
