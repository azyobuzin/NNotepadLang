using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                    t.Item2.ToArray() as IReadOnlyList<YacqExpression>)
                )
                .ToArray();
            this.DefaultExpressions = defaultExprs.Select(x => x.ToArray()).Otherwise(() => new YacqExpression[0]);
        }

        public YacqExpression Cond { get; private set; }
        public IReadOnlyList<Tuple<IReadOnlyList<YacqExpression>, IReadOnlyList<YacqExpression>>> Cases { get; private set; }
        public IReadOnlyList<YacqExpression> DefaultExpressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var cond = this.Cond.Reduce(symbols);
            var cases = this.Cases
                .Select(t => Expression.SwitchCase(
                    Expression.Invoke(YacqExpression.AmbiguousLambda(t.Item2).Reduce(symbols)),
                    t.Item1.Select(expr => expr.Reduce(symbols))
                ))
                .ToArray();

            return this.DefaultExpressions.Any()
                ? Expression.Switch(
                    cond,
                    Expression.Invoke(YacqExpression.AmbiguousLambda(this.DefaultExpressions).Reduce(symbols)),
                    cases
                )
                : Expression.Switch(cond, cases);
        }
    }
}
