using System.Collections.Generic;
using System.Linq;

namespace NNotepadLang.Expressions
{
    public class NlClassNameExpression : NlEmptyExpression
    {
        public NlClassNameExpression(IEnumerable<string> names)
        {
            this.Names = names.ToArray();
        }

        public IReadOnlyList<string> Names { get; private set; }

        public override string ToString()
        {
            return string.Join("::", this.Names);
        }
    }
}
