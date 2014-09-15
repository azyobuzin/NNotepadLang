using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Parseq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlSwitchExpression : YacqExpression
    {
        public NlSwitchExpression(IOption<YacqExpression> cond, IEnumerable<Tuple<YacqExpression, IEnumerable<YacqExpression>>> cases, IOption<IEnumerable<YacqExpression>> defaultExprs)
            : base(null)
        {
            this.Cond = cond.Otherwise(() => YacqExpression.Identifier("true"));
            this.Cases = cases
                .Select(t => Tuple.Create(
                    (t.Item1 as NlListExpression).Expressions,
                    new NlBlockExpression(t.Item2)
                ))
                .ToArray();
            this.DefaultExpressions = defaultExprs.Select(x => new NlBlockExpression(x)).Otherwise(() => null);
        }

        public YacqExpression Cond { get; private set; }
        public IReadOnlyList<Tuple<IReadOnlyList<YacqExpression>, NlBlockExpression>> Cases { get; private set; }
        public NlBlockExpression DefaultExpressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var cond = this.Cond.Reduce(symbols);
            var defaultExpr = this.DefaultExpressions != null
                ? this.DefaultExpressions.Reduce(symbols) : null;

            if (this.Cases.Count == 0)
            {
                return Expression.Block(cond,
                    defaultExpr != null
                        ? defaultExpr
                        : Expression.Empty()
                );
            }

            var condVar = Expression.Variable(cond.Type() ?? typeof(object));
            var comparerType = typeof(EqualityComparer<>).MakeGenericType(new[] { condVar.Type });
            var comparer = Expression.Property(null, comparerType.GetProperty("Default"));
            var equalsMethod = comparerType.GetMethod("Equals", new[] { condVar.Type, condVar.Type });

            var cases = this.Cases
                .Select(t => Tuple.Create(MakeEquals(comparer, equalsMethod, condVar, t.Item1, symbols), t.Item2.Reduce(symbols)))
                .ToArray();
            Array.Reverse(cases);

            var casesExpr = cases.All(t => t.Item2.Type != typeof(void)) && defaultExpr != null && defaultExpr.Type != typeof(void)
                ? cases.Aggregate(
                    defaultExpr,
                    (expr, t) => Expression.Condition(t.Item1, t.Item2, expr)
                )
                : cases.Aggregate(
                    defaultExpr ?? Expression.Empty(),
                    (expr, t) => Expression.IfThenElse(t.Item1, t.Item2, expr)
                );

            return Expression.Block(
                new[] { condVar },
                Expression.Assign(condVar, cond),
                casesExpr
            );
        }

        private static Expression MakeEquals(MemberExpression comparer, MethodInfo equalsMethod, ParameterExpression condVar, IReadOnlyList<YacqExpression> values, SymbolTable symbols)
        {
            var equalsExprs = values
                .Select(e => Expression.Call(comparer, equalsMethod, Expression.Convert(e.Reduce(symbols), condVar.Type), condVar) as Expression)
                .ToArray();
            Array.Reverse(equalsExprs);
            if (equalsExprs.Length == 1) return equalsExprs[0];
            return equalsExprs.Aggregate((e0, e1) => Expression.OrElse(e1, e0));
        }

        public override string ToString()
        {
            var sb = new StringBuilder("switch ");
            sb.Append(this.Cond.ToString());
            sb.AppendLine(":");
            foreach (var t in this.Cases)
            {
                sb.Append("case ");
                sb.Append(string.Join(", ", t.Item1));
                sb.Append(":");
                sb.AppendLine(t.Item2.ToString());
            }
            if (this.DefaultExpressions != null)
            {
                sb.Append("default:");
                sb.AppendLine(this.DefaultExpressions.ToString());
            }
            sb.Append("hctiws");
            return sb.ToString();
        }
    }
}
