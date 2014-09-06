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
    public class NlDefClassExpression : YacqExpression
    {
        public NlDefClassExpression(IOption<YacqExpression> access, YacqExpression name, IOption<YacqExpression> extends, IOption<YacqExpression> implements, IEnumerable<YacqExpression> members)
            : base(null)
        {
            this.Access = access.Select(x => (x as NlAccessModifierExpression).Access).Otherwise(() => AccessModifier.Public);
            this.Name = (name as IdentifierExpression).Name;
            this.Extends = extends
                .Select(expr => (expr as NlListExpression).Expressions.Cast<NlClassNameExpression>().ToArray())
                .Otherwise(() => new NlClassNameExpression[0]);
            this.Implements = implements
                .Select(expr => (expr as NlListExpression).Expressions.Cast<NlClassNameExpression>().ToArray())
                .Otherwise(() => new NlClassNameExpression[0]);
            this.Members = members.ToArray();
        }

        public AccessModifier Access { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<NlClassNameExpression> Extends { get; private set; }
        public IReadOnlyList<NlClassNameExpression> Implements { get; private set; }
        public IReadOnlyList<YacqExpression> Members { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            throw new NotImplementedException();
        }
    }
}
