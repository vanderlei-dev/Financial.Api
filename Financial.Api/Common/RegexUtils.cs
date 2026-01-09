using System.Text.RegularExpressions;

namespace Financial.Api.Common
{
    public static partial class RegexUtils
    {
        [GeneratedRegex(@"^CY\d{4}$", RegexOptions.None, 1000)]
        public static partial Regex EdgarYearFormatRegex();

        [GeneratedRegex("^[aeiou]", RegexOptions.IgnoreCase)]
        public static partial Regex StartWithVowelRegex();
    }
}
