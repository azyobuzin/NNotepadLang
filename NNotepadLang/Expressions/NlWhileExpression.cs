using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlWhileExpression : YacqExpression
    {
        public NlWhileExpression(YacqExpression cond, IEnumerable<YacqExpression> exprs)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = exprs.ToArray();
        }

        public NlWhileExpression(YacqExpression cond, YacqExpression expr)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = new[] { expr };
        }

        public YacqExpression Cond { get; private set; }
        public IReadOnlyList<YacqExpression> Expressions { get; private set; }
        
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            var blockSymbols = new SymbolTable(symbols);
            blockSymbols.Add(DispatchTypes.Method, "$break", (e, s, t) => Expression.Break(breakLabel));
            blockSymbols.Add(DispatchTypes.Method, "$continue", (e, s, t) => Expression.Continue(continueLabel));

            return Expression.Loop(
                Expression.Block(
                    Expression.IfThen(
                        Expression.IsFalse(this.Cond.Reduce(symbols)),
                        Expression.Break(breakLabel)
                    ),
                    Expression.Invoke(YacqExpression.AmbiguousLambda(this.Expressions).Reduce(symbols))
                ),
                breakLabel,
                continueLabel
            );
        }
    }
}
