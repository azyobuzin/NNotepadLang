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
            this.Expression = expr;
        }

        public VariableType VariableType { get; private set; }
        public YacqExpression Expression { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.VariableType.ToString().ToLowerInvariant() + " " + this.Expression.ToString();
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
