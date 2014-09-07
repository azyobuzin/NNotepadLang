using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlListExpression : YacqExpression
    {
        public NlListExpression(IEnumerable<YacqExpression> exprs)
            : base(null)
        {
            this.Expressions = exprs.ToArray();
        }

        public NlListExpression(params YacqExpression[] exprs)
            : base(null)
        {
            this.Expressions = exprs;
        }

        public IReadOnlyList<YacqExpression> Expressions { get; private set; }

        public override string ToString()
        {
            return string.Join(", ", this.Expressions);
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            switch (this.Expressions.Count)
            {
                case 0:
                    return Expression.Empty();
                case 1:
                    return this.Expressions[0];
                default:
                    return Expression.Block(this.Expressions.Select(expr => expr.Reduce(symbols)));
            }
        }
    }
}
