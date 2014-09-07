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
            this.Expressions = new NlBlockExpression(exprs);
        }

        public NlUnlessExpression(YacqExpression cond, YacqExpression expr)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = new NlBlockExpression(expr);
        }

        public YacqExpression Cond { get; private set; }
        public NlBlockExpression Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Expression.IfThen(
                Expression.IsFalse(Expression.Convert(this.Cond.Reduce(symbols), typeof(bool))),
                this.Expressions.Reduce(symbols)
            );
        }

        public override string ToString()
        {
            return "unless " + this.Cond.ToString() + ":" + this.Expressions.ToString() + "\nsselnu";
        }
    }
}
