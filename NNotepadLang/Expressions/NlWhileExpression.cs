using System;
using System.Collections.Generic;
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
            this.Expressions = new NlBlockExpression(exprs);
        }

        public NlWhileExpression(YacqExpression cond, YacqExpression expr)
            : base(null)
        {
            this.Cond = cond;
            this.Expressions = new NlBlockExpression(expr);
        }

        public YacqExpression Cond { get; private set; }
        public NlBlockExpression Expressions { get; private set; }

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
                        Expression.IsFalse(Expression.Convert(this.Cond.Reduce(symbols), typeof(bool))),
                        Expression.Break(breakLabel)
                    ),
                    this.Expressions.Reduce(blockSymbols)
                ),
                breakLabel,
                continueLabel
            );
        }

        public override string ToString()
        {
            return "while " + this.Cond.ToString() + ":" + this.Expressions.ToString() + "\nelihw";
        }
    }
}
