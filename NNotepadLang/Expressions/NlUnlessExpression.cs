using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlUnlessExpression : YacqExpression
    {
        public NlUnlessExpression(YacqExpression cond, IEnumerable<YacqExpression> exprs)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = exprs.ToArray();
        }

        public NlUnlessExpression(YacqExpression cond, YacqExpression expr)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = new[] { expr };
        }

        public YacqExpression Cond { get; private set; }
        public IReadOnlyList<YacqExpression> Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Expression.IfThen(
                this.Cond.Reduce(symbols),
                Expression.Invoke(YacqExpression.AmbiguousLambda(this.Expressions).Reduce(symbols))
            );
        }
    }
}
