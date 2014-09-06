using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Dynamic;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlForExpression : YacqExpression
    {
        public NlForExpression(YacqExpression identifier, YacqExpression source, IEnumerable<YacqExpression> exprs)
            : base(null)
        {
            this.VarName = (identifier as IdentifierExpression).Name;
            this.Source = source;
            this.Expressions = exprs.ToArray();
        }

        public NlForExpression(YacqExpression identifier, YacqExpression source, YacqExpression expr)
            : base(null)
        {
            this.VarName = (identifier as IdentifierExpression).Name;
            this.Source = source;
            this.Expressions = new[] { expr };
        }

        public string VarName { get; private set; }
        public YacqExpression Source { get; private set; }
        public IReadOnlyList<YacqExpression> Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var source = this.Source.Reduce(symbols);
            var enumerator = Expression.Variable(typeof(IEnumerator), "$enumerator");

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            var blockSymbols = new SymbolTable(symbols);
            blockSymbols.Add(DispatchTypes.Method, "$break", (e, s, t) => Expression.Break(breakLabel));
            blockSymbols.Add(DispatchTypes.Method, "$continue", (e, s, t) => Expression.Continue(continueLabel));

            return Expression.Block(
                new[] { enumerator },
                Expression.Assign(
                    enumerator,
                    source.Type.GetInterfaces().Any(t => t == typeof(IEnumerable))
                        ? Expression.Call(source, typeof(IEnumerable).GetMethod("GetEnumerator", Type.EmptyTypes))
                        : Expression.Dynamic(YacqBinder.InvokeMember("GetEnumerable"), typeof(IEnumerator)) as Expression
                ),
                Expression.TryFinally(
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThen(
                                Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext", Type.EmptyTypes))),
                                Expression.Break(breakLabel)
                            ),
                            Expression.Invoke(
                                YacqExpression.AmbiguousLambda(this.Expressions, YacqExpression.AmbiguousParameter(this.VarName))
                                    .Reduce(blockSymbols),
                                Expression.Property(enumerator, "Current")
                            )
                        ),
                        breakLabel,
                        continueLabel
                    ),
                    Expression.IfThen(
                        Expression.TypeIs(enumerator, typeof(IDisposable)),
                        Expression.Call(enumerator, typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes))
                    )
                )
            );
        }
    }
}
