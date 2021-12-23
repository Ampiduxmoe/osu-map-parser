using System;
using System.Collections.Generic;
using System.Text;

namespace osu_map_parser.lexer
{
    class Lexer
    {
        public SourceFile SourceFile;
        public Lexer(SourceFile sourceFile)
        {
            SourceFile = sourceFile;
        }

        public IEnumerable<Token> GetTokens(int lineNo)
        {
            var regex = Regexes.CombinedRegex;
            var groupNames = Regexes.GroupNames;
            var lastPos = 0;
            var line = SourceFile.Lines[lineNo];
            for (var m = regex.Match(line); m.Success; m = m.NextMatch())
            {
                if (lastPos < m.Index)
                {
                    throw MakeException("Could not match any of token types", lineNo, lastPos);
                }
                bool found = false;
                foreach (var kv in groupNames)
                {
                    if (m.Groups[kv.Value].Success)
                    {
                        if (found)
                        {
                            throw MakeException($"\"{m.Value}\" matched multiple token groups", lineNo, m.Index);
                        }
                        found = true;
                        yield return new Token(m.Index, kv.Key, kv.Value);
                    }
                }
                if (!found)
                {
                    throw MakeException($"\"{m.Value}\" did not match any of token groups", lineNo, m.Index);
                }
                lastPos = m.Index + m.Length;
            }
        }

        private Exception MakeException(string message, int lineNo, int errorPos)
        {
            return new Exception(SourceFile.CounstructErrorMessage("Lexer error: " + message, lineNo, errorPos));
        }
    }
}
