namespace NNotepadLang.Expressions
{
    public class NlAccessModifierExpression : NlEmptyExpression
    {
        public NlAccessModifierExpression(AccessModifier access)
        {
            this.Access = access;
        }

        public AccessModifier Access { get; private set; }

        public override string ToString()
        {
            return this.Access.ToString();
        }
    }
}
