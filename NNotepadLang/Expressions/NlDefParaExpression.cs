using System.Collections.Generic;
using System.Linq;
using Parseq;
using XSpect.Yacq.Expressions;

namespace NNotepadLang.Expressions
{
    public class NlDefParaExpression : NlEmptyExpression
    {
        public NlDefParaExpression(IOption<YacqExpression> access, YacqExpression name, IOption<IOption<YacqExpression>> args, IEnumerable<YacqExpression> body)
        {
            this.Access = access.Select(x => (x as NlAccessModifierExpression).Access).Otherwise(() => AccessModifier.Public);
            this.Name = (name as IdentifierExpression).Name;
            this.Args = args.HasValue && args.Value.HasValue
                ? (args.Value.Value as NlListExpression).Expressions.Cast<IdentifierExpression>().Select(x => x.Name).ToArray()
                : new string[0];
            this.Body = body.ToArray();
        }

        public AccessModifier Access { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<string> Args { get; private set; }
        public IReadOnlyList<YacqExpression> Body { get; private set; }
    }
}
