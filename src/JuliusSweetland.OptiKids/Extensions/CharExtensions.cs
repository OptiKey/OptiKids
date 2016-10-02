using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.OptiKids.Enums;

namespace JuliusSweetland.OptiKids.Extensions
{
    public static class CharExtensions
    {
        public static string ToPrintableString(this char c)
        {
            var escapedLiteralString = c.ToString(CultureInfo.InvariantCulture)
                    .Replace("\0", @"\0")
                    .Replace("\a", @"\a")
                    .Replace("\b", @"\b")
                    .Replace("\t", @"\t")
                    .Replace("\f", @"\f")
                    .Replace("\n", @"\n")
                    .Replace("\r", @"\r");

            return string.Format(@"[Char:{0}|Unicode:U+{1:x4}]", escapedLiteralString, (int)c);
        }
    }
}
