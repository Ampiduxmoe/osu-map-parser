using System;
using System.Collections.Generic;
using System.Text;

namespace osu_map_parser.lexer
{
    public enum TokenType
    {
        WhitespacesOrComments,
        WordOrName,
        Number,
        Punctuator
    }
}
