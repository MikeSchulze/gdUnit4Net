using System;

namespace GdUnit3.Tests.Resources
{
    using static Assertions;
    // will be ignored because of missing `[TestSuite]` anotation
    // used by executor integration test
    public class TestSuiteParameterizedTests
    {
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void ParameterizedBoolValue(int a, bool expected)
        {
            AssertThat(Convert.ToBoolean(a)).IsEqual(expected);
        }

        [TestCase(1, 2, 3, 6)]
        [TestCase(3, 4, 5, 12)]
        [TestCase(6, 7, 8, 21)]
        public void ParameterizedIntValues(int a, int b, int c, int expected)
        {
            AssertThat(a + b + c).IsEqual(expected);
        }

        [TestCase(1, 2, 3, 6)]
        [TestCase(3, 4, 5, 11)]
        [TestCase(6, 7, 8, 22)]
        public void ParameterizedIntValuesFail(int a, int b, int c, int expected)
        {
            AssertThat(a + b + c).IsEqual(expected);
        }
    }
}
