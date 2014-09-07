using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            this.Expressions = new NlBlockExpression(exprs);
        }

        public NlForExpression(YacqExpression identifier, YacqExpression source, YacqExpression expr)
            : base(null)
        {
            this.VarName = (identifier as IdentifierExpression).Name;
            this.Source = source;
            this.Expressions = new NlBlockExpression(expr);
        }

        public string VarName { get; private set; }
        public YacqExpression Source { get; private set; }
        public NlBlockExpression Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var sourceType = this.Source.Type(symbols);
            var enumerator = Expression.Variable(typeof(IEnumerator), "$enumerator");
            var elmVar = Expression.Variable(typeof(object), this.VarName);

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            var blockSymbols = new SymbolTable(symbols);
            blockSymbols[DispatchTypes.Method, "$break"] = (e, s, t) => Expression.Break(breakLabel);
            blockSymbols[DispatchTypes.Method, "$continue"] = (e, s, t) => Expression.Continue(continueLabel);
            blockSymbols[this.VarName] = YacqExpression.Contextful(elmVar, ContextType.Dynamic);

            return Expression.Block(
                new[] { enumerator },
                Expression.Assign(
                    enumerator,
                    sourceType != null && sourceType.GetInterfaces().Any(t => t == typeof(IEnumerable))
                        ? Expression.Call(this.Source.Reduce(symbols), typeof(IEnumerable).GetMethod("GetEnumerator", Type.EmptyTypes))
                        : YacqExpression.Contextful(this.Source, ContextType.Dynamic).Method("GetEnumerator").Reduce(symbols)
                ),
                Expression.TryFinally(
                    Expression.Loop(
                        Expression.Block(
                            new[] { elmVar },
                            Expression.IfThen(
                                Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext", Type.EmptyTypes))),
                                Expression.Break(breakLabel)
                            ),
                            Expression.Assign(elmVar, Expression.Property(enumerator, "Current")),
                            this.Expressions.Reduce(blockSymbols)
                        ),
                        breakLabel,
                        continueLabel
                    ),
                    Expression.IfThen(
                        Expression.TypeIs(enumerator, typeof(IDisposable)),
                        Expression.Call(Expression.Convert(enumerator, typeof(IDisposable)), typeof(IDisposable).GetMethod("Dispose", Type.EmptyTypes))
                    )
                )
            );
        }

        public override string ToString()
        {
            return "for " + this.VarName + " in " + this.Source.ToString() + ":" + this.Expressions.ToString() + "\nrof";
        }
    }
}
