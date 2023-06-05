namespace GdUnit4.Core
{
    public sealed class CoreUtils
    {

        public static string NormalizedFailureMessage(string input)
        {
            using (var rtl = new Godot.RichTextLabel())
            {
                rtl.BbcodeEnabled = true;
                rtl.AppendText(input);
                var text = rtl.GetParsedText();
                rtl.Free();
                return text.Replace("\r", "");
            }
        }
    }
}
