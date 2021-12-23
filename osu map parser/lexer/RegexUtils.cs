using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace osu_map_parser.lexer
{
    class RegexUtils
    {
        const RegexOptions MyRegexOptions =
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.ExplicitCapture |
            RegexOptions.IgnorePatternWhitespace;

        public static Regex CreateRegex(string pattern) => new Regex(pattern, MyRegexOptions);
    }
}
