namespace Examples;

using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class ExampleTest
{
    [TestCase]
    public void success()
    {
        AssertBool(true).IsTrue();
    }


    [TestCase]
    public void failed()
    {
        AssertBool(false).IsTrue();
    }

}
