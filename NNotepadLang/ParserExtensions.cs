using System.Collections.Generic;
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Expressions;

namespace NNotepadLang
{
    internal static class ParserExtensions
    {
        internal static Parser<char, IEnumerable<char>> Sequence(this string s)
        {
            return Chars.Sequence(s);
        }

        internal static Parser<TToken, YacqExpression> IgnoreSeq<TToken, T0, T1>(this Parser<TToken, T0> p0, Parser<TToken, T1> p1)
        {
            return p0.SelectMany(p => p1.Select(_ => YacqExpression.Ignore()));
        }

        internal static Parser<TToken, YacqExpression> IgnoreSeq<TToken, T0, T1, T2>(this Parser<TToken, T0> p0, Parser<TToken, T1> p1, Parser<TToken, T2> p2)
        {
            return p0.SelectMany(p => p1.IgnoreSeq(p2));
        }
    }
}
