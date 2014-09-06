using System;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public abstract class NlEmptyExpression : YacqExpression
    {
        public NlEmptyExpression() : base(null) { }

        public override bool CanReduce
        {
            get
            {
                return false;
            }
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return null;
        }
    }
}
