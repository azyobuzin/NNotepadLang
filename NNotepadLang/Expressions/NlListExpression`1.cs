namespace NNotepadLang.Expressions
{
    public class NlListExpression<T> : NlEmptyExpression
    {
        public NlListExpression(NlListExpression<T> parent, params T[] items)
        {
            this.Items = new T[parent.Items.Length + items.Length];
            parent.Items.CopyTo(this.Items, 0);
            items.CopyTo(this.Items, parent.Items.Length);
        }

        public NlListExpression()
        {
            this.Items = new T[0];
        }

        public T[] Items { get; private set; }

        public NlListExpression<T> Add(params T[] items)
        {
            return new NlListExpression<T>(this, items);
        }
    }
}
