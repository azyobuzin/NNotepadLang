using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Parseq;
using XSpect.Yacq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlDefVariableExpression : YacqExpression
    {
        public NlDefVariableExpression(VariableType type, YacqExpression expr)
            : base(null)
        {
            this.VariableType = type;
            this.Expressions = expr as NlListExpression;
        }

        public VariableType VariableType { get; private set; }
        public NlListExpression Expressions { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var names = this.Expressions.Expressions.Cast<ListExpression>()
                .Select(expr => (expr.Elements[1] as IdentifierExpression).Name);

            switch (this.VariableType)
            {
                case VariableType.Local:
                    foreach (var name in names)
                    {
                        var v = Expression.Variable(typeof(object), name);
                        symbols["$local"] =
                            (symbols["$local"] as NlListExpression<ParameterExpression>).Add(v);
                        var expr = YacqExpression.Contextful(v, ContextType.Dynamic);
                        symbols[name] = expr;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return this.Expressions;
        }

        public override string ToString()
        {
            return this.VariableType.ToString().ToLowerInvariant() + " " + this.Expressions.ToString();
        }

        public static NlDefVariableExpression Local(YacqExpression expr)
        {
            return new NlDefVariableExpression(VariableType.Local, expr);
        }

        public static NlDefVariableExpression Instance(YacqExpression expr)
        {
            return new NlDefVariableExpression(VariableType.Instance, expr);
        }

        public static NlDefVariableExpression Class(YacqExpression expr)
        {
            return new NlDefVariableExpression(VariableType.Class, expr);
        }

        public static NlDefVariableExpression Global(YacqExpression expr)
        {
            return new NlDefVariableExpression(VariableType.Global, expr);
        }
    }
}
