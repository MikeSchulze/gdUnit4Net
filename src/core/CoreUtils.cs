using System.Text.RegularExpressions;


namespace GdUnit4.Core
{
    public sealed class CoreUtils
    {
        public static string NormalizedFailureMessage(string input) => Regex.Replace(input, "\\[?\\/?color.*?\\]", string.Empty);
    }
}
