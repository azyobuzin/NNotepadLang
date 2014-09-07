using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlScriptExpression : YacqExpression
    {
        public NlScriptExpression(IEnumerable<YacqExpression> body)
            : base(null)
        {
            this.Body = new NlBlockExpression(body);
        }

        public NlBlockExpression Body { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            symbols = new SymbolTable(symbols);
            symbols["$local"] = new NlListExpression<ParameterExpression>();

            var body = this.Body.Reduce(symbols);
            var variables = (symbols["$local"] as NlListExpression<ParameterExpression>).Items;

            return Expression.Block(variables, body);
        }

        public override string ToString()
        {
            return "scr:" + this.Body.ToString() + "\nrcs";
        }
    }
}
