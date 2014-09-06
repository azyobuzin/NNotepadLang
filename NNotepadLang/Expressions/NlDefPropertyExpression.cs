using System;
using Parseq;
using XSpect.Yacq.Expressions;

namespace NNotepadLang.Expressions
{
    public class NlDefPropertyExpression : NlEmptyExpression
    {
        public NlDefPropertyExpression(IOption<YacqExpression> access, YacqExpression name, IOption<Tuple<YacqExpression, YacqExpression>> accessorAccess, IOption<YacqExpression> value)
        {
            this.Access = access.Select(x => (x as NlAccessModifierExpression).Access).Otherwise(() => AccessModifier.Public);
            this.Name = (name as IdentifierExpression).Name;
            if (accessorAccess.HasValue)
            {
                this.GetAccess = (accessorAccess.Value.Item1 as NlAccessModifierExpression).Access;
                this.SetAccess = (accessorAccess.Value.Item2 as NlAccessModifierExpression).Access;
            }
            else
            {
                this.GetAccess = AccessModifier.Public;
                this.SetAccess = AccessModifier.Public;
            }
            this.Value = value;
        }

        public AccessModifier Access { get; private set; }
        public string Name { get; private set; }
        public AccessModifier GetAccess { get; private set; }
        public AccessModifier SetAccess { get; private set; }
        public IOption<YacqExpression> Value { get; private set; }
    }
}
