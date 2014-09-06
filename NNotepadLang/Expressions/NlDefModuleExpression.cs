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
    public class NlDefModuleExpression : YacqExpression
    {
        public NlDefModuleExpression(YacqExpression name, IEnumerable<YacqExpression> members)
            : base(null)
        {
            this.Name = (name as IdentifierExpression).Name;
            this.Members = members.ToArray();
        }

        public string Name { get; private set; }
        public IReadOnlyList<YacqExpression> Members { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            throw new NotImplementedException();
        }
    }
}
