using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Parseq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlIfExpression : YacqExpression
    {
        public NlIfExpression(YacqExpression cond, IEnumerable<YacqExpression> trueExpr, IEnumerable<Tuple<YacqExpression, IEnumerable<YacqExpression>>> elif, IOption<IEnumerable<YacqExpression>> falseExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = trueExpr.ToArray();
            this.Elif = elif.Select(t => Tuple.Create(t.Item1, t.Item2.ToArray() as IReadOnlyList<YacqExpression>)).ToArray();
            this.False = falseExpr.Select(x => x.ToArray()).Otherwise(() => new YacqExpression[0]);
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new[] { trueExpr };
            this.Elif = new Tuple<YacqExpression, IReadOnlyList<YacqExpression>>[0];
            this.False = new YacqExpression[0];
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr, YacqExpression falseExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new[] { trueExpr };
            this.Elif = new Tuple<YacqExpression, IReadOnlyList<YacqExpression>>[0];
            this.False = new[] { falseExpr };
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr, IEnumerable<Tuple<YacqExpression, YacqExpression>> elif, YacqExpression falseExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new[] { trueExpr };
            this.Elif = elif.Select(t => Tuple.Create(t.Item1, new[] { t.Item2 } as IReadOnlyList<YacqExpression>)).ToArray();
            this.False = new[] { falseExpr };
        }

        public YacqExpression Cond { get; private set; }
        public IReadOnlyList<YacqExpression> True { get; private set; }
        public IReadOnlyList<Tuple<YacqExpression, IReadOnlyList<YacqExpression>>> Elif { get; private set; }
        public IReadOnlyList<YacqExpression> False { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var cond = this.Cond.Reduce(symbols);
            var trueExpr = this.True.Any()
                ? Expression.Invoke(YacqExpression.AmbiguousLambda(this.True).Reduce(symbols))
                : Expression.Empty() as Expression;
            var elif = this.Elif
                .Select(t => Tuple.Create(
                    t.Item1.Reduce(symbols),
                    t.Item2.Any()
                        ? Expression.Invoke(YacqExpression.AmbiguousLambda(t.Item2).Reduce(symbols))
                        : Expression.Empty() as Expression
                ))
                .ToArray();
            Array.Reverse(elif);
            var falseExpr = this.False.Any()
                ? Expression.Invoke(YacqExpression.AmbiguousLambda(this.False).Reduce(symbols))
                : Expression.Empty() as Expression;

            if (trueExpr.Type != typeof(void) && falseExpr.Type != typeof(void) && elif.All(t => t.Item2.Type != typeof(void)))
            {
                return elif.Aggregate(
                    falseExpr,
                    (expr, t) => Expression.Condition(t.Item1, t.Item2, expr),
                    expr => Expression.Condition(cond, trueExpr, expr)
                );
            }

            return elif.Aggregate(
                falseExpr,
                (expr, t) => Expression.IfThenElse(t.Item1, t.Item2, expr),
                expr => Expression.IfThenElse(cond, trueExpr, expr)
            );
        }
    }
}
