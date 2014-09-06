using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Parseq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlScriptExpression : YacqExpression
    {
        public NlScriptExpression(IEnumerable<YacqExpression> body)
            : base(null)
        {
            this.Body = body.ToArray();
        }

        public IReadOnlyList<YacqExpression> Body { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            throw new NotImplementedException();
        }
    }
}
