using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlBlockExpression : YacqExpression
    {
        public NlBlockExpression(IEnumerable<YacqExpression> exprs)
            : base(null)
        {
            this.Expressions = exprs.ToArray();
        }

        public NlBlockExpression(params YacqExpression[] exprs)
            : base(null)
        {
            this.Expressions = exprs;
        }

        public NlBlockExpression(IEnumerable<ParameterExpression> variables, params YacqExpression[] exprs)
            : base(null)
        {
            this.Variables = variables.ToArray();
            this.Expressions = exprs;
        }

        public IReadOnlyList<ParameterExpression> Variables { get; private set; }
        public IReadOnlyList<YacqExpression> Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Expression.Block(
                this.Variables ?? new ParameterExpression[0],
                this.Expressions
                    .Where(expr => expr != null && !(expr is IgnoredExpression))
                    .Select(expr => expr.Reduce(symbols))
                    .Where(expr => expr != null && !(expr is IgnoredExpression))
                    .ToArray()
            );
        }

        public override string ToString()
        {
            return "{\n" + string.Join("\n", this.Expressions.Select(expr => expr.ToString() + ";")) + "\n}";
        }
    }
}
