namespace GdUnit4.core.runners;

internal class GdUnitTestCase
{
    public GdUnitTestCase(string codeFilePath, string testName)
    {
        CodeFilePath = codeFilePath;
        TestName = testName;
    }

    public string CodeFilePath { get; }

    public string TestName { get; set; }
}
