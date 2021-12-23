using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace osu_map_parser.lexer
{
    class Token
    {
        public readonly int Position;
        public readonly TokenType Type;
        public readonly string Lexeme;

        public Token(int position, TokenType type, string lexeme)
        {
            CheckToken(type, lexeme);
            Position = position;
            Type = type;
            Lexeme = lexeme;
        }

        [Conditional("DEBUG")]
        private void CheckToken(TokenType type, string lexeme)
        {
            var regex = Regexes.TokenRegexes[type];
            if (!regex.IsMatch(lexeme))
            {
                throw new Exception($"Lexeme \"{lexeme}\" doesn't match regex {regex}");
            }
        }
    }
}
