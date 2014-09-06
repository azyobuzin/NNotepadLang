using System.Collections.Generic;
using Parseq;
using Parseq.Combinators;

namespace NNotepadLang
{
    internal static class ParserExtensions
    {
        internal static Parser<char, IEnumerable<char>> Sequence(this string s)
        {
            return Chars.Sequence(s);
        }
    }
}
