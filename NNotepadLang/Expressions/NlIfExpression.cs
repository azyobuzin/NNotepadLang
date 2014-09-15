using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
            this.True = new NlBlockExpression(trueExpr);
            this.Elif = elif.Select(t => Tuple.Create(t.Item1, new NlBlockExpression(t.Item2))).ToArray();
            this.False = falseExpr.Select(x => new NlBlockExpression(x)).Otherwise(() => null);
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new NlBlockExpression(trueExpr);
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr, YacqExpression falseExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new NlBlockExpression(trueExpr);
            this.False = new NlBlockExpression(falseExpr);
        }

        public NlIfExpression(YacqExpression cond, YacqExpression trueExpr, IEnumerable<Tuple<YacqExpression, YacqExpression>> elif, YacqExpression falseExpr)
            : base(null)
        {
            this.Cond = cond;
            this.True = new NlBlockExpression(trueExpr);
            this.Elif = elif.Select(t => Tuple.Create(t.Item1, new NlBlockExpression(t.Item2))).ToArray();
            this.False = new NlBlockExpression(falseExpr);
        }

        public YacqExpression Cond { get; private set; }
        public NlBlockExpression True { get; private set; }
        public IReadOnlyList<Tuple<YacqExpression, NlBlockExpression>> Elif { get; private set; }
        public NlBlockExpression False { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var cond = Expression.Convert(this.Cond.Reduce(symbols), typeof(bool));
            var trueExpr = this.True.Reduce(symbols);
            var elif = (this.Elif ?? Enumerable.Empty<Tuple<YacqExpression, NlBlockExpression>>())
                .Select(t => Tuple.Create(
                    Expression.Convert(t.Item1.Reduce(symbols), typeof(bool)),
                    t.Item2.Reduce(symbols)
                ))
                .ToArray();
            Array.Reverse(elif);
            var falseExpr = this.False != null ? this.False.Reduce(symbols) : Expression.Empty();

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

        public override string ToString()
        {
            var sb = new StringBuilder("if ");
            sb.Append(this.Cond.ToString());
            sb.Append(":");
            sb.AppendLine(this.True.ToString());
            if (this.Elif != null)
            {
                foreach (var t in this.Elif)
                {
                    sb.Append("elif ");
                    sb.Append(t.Item1.ToString());
                    sb.Append(":");
                    sb.AppendLine(t.Item2.ToString());
                }
            }
            if (this.False != null)
            {
                sb.Append("else:");
                sb.AppendLine(this.False.ToString());
            }
            sb.Append("fi");
            return sb.ToString();
        }
    }
}
