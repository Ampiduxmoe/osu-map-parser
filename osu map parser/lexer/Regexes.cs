using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace osu_map_parser.lexer
{
    static class Regexes
    {
        static readonly IReadOnlyDictionary<TokenType, string> tokenPatterns = new Dictionary<TokenType, string>
        {
            { TokenType.WhitespacesOrComments, @"([ \t]+)|(//.*)" },
            { TokenType.WordOrName, @"(\p{L}|[0-9_=-])+" },
            { TokenType.Number, @"-?[0-9]+(\.[0-9]+)?" },
            { TokenType.Punctuator, @"[:[\],|""]" },
        };

        public static Dictionary<TokenType, string> groupNames;
        public static IReadOnlyDictionary<TokenType, string> GroupNames
        {
            get
            {
                if (groupNames != null)
                {
                    return groupNames;
                }

                groupNames = new Dictionary<TokenType, string>();
                foreach (var kv in tokenPatterns)
                {
                    var tokenType = kv.Key;
                    var pattern = kv.Value;
                    var groupName = tokenType.ToString();
                    groupNames.Add(tokenType, groupName);
                }
                return groupNames;
            }
        }

        private static Dictionary<TokenType, Regex> tokenRegexes;
        public static IReadOnlyDictionary<TokenType, Regex> TokenRegexes
        {
            get
            {
                if (tokenRegexes != null)
                {
                    return tokenRegexes;
                }

                tokenRegexes = new Dictionary<TokenType, Regex>();
                foreach (var kv in tokenPatterns)
                {
                    var tokenType = kv.Key;
                    var pattern = kv.Value;
                    var groupName = tokenType.ToString();
                    var regex = RegexUtils.CreateRegex($"(?<{groupName}>{pattern})");
                    tokenRegexes.Add(tokenType, regex);
                }
                return tokenRegexes;
            }
        }

        private static Regex combinedRegex;
        public static Regex CombinedRegex
        {
            get
            {
                if (combinedRegex != null)
                {
                    return combinedRegex;
                }

                var regexPatterns = new List<string>();
                foreach (var kv in tokenPatterns)
                {
                    var tokenType = kv.Key;
                    var pattern = kv.Value;
                    var groupName = tokenType.ToString();
                    regexPatterns.Add($"(?<{groupName}>{pattern})");
                }
                var combinedPattern = string.Join("\n|\n", regexPatterns);
                combinedRegex = RegexUtils.CreateRegex(combinedPattern);
                return combinedRegex;
            }
        }
    }
}
