using System.Collections.Generic;
using System.Linq;
using XSpect.Yacq.Expressions;
using Parseq;

namespace NNotepadLang.Expressions
{
    public class NlDefMethodExpression : NlEmptyExpression
    {
        public NlDefMethodExpression(IOption<YacqExpression> access, IOption<YacqExpression> isOverride, IOption<YacqExpression> isNative, YacqExpression name, IOption<YacqExpression> args, IEnumerable<YacqExpression> body)
        {
            this.Access = access.Select(x => (x as NlAccessModifierExpression).Access).Otherwise(() => AccessModifier.Public);
            this.IsOverride = isOverride.HasValue;
            this.IsNative = isNative.HasValue;
            this.Name = (name as IdentifierExpression).Name;
            this.Args = args
                .Select(expr => (expr as NlListExpression).Expressions.Cast<IdentifierExpression>().Select(x => x.Name).ToArray())
                .Otherwise(() => new string[0]);
            this.Body = body.ToArray();
        }

        public AccessModifier Access { get; private set; }
        public bool IsOverride { get; private set; }
        public bool IsNative { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<string> Args { get; private set; }
        public IReadOnlyList<YacqExpression> Body { get; private set; }
    }
}
