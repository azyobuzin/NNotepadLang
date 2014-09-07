using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Parseq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace NNotepadLang.Expressions
{
    public class NlTreeExpression : YacqExpression
    {
        public NlTreeExpression(IOption<IEnumerable<YacqExpression>> pairs)
            : base(null)
        {
            this.Pairs = pairs
                .Select(x => x.Cast<NlListExpression>().Select(expr => Tuple.Create(
                    (expr.Expressions[0] as IdentifierExpression).Name,
                    expr.Expressions[1]
                )).ToArray())
                .Otherwise(() => new Tuple<string, YacqExpression>[0]);
        }

        public IReadOnlyList<Tuple<string, YacqExpression>> Pairs { get; private set; }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            var dicType = typeof(Dictionary<string, object>);
            var dicVar = Expression.Variable(dicType, "$tree");
            
            var body = new List<Expression>();
            body.Add(Expression.Assign(dicVar, Expression.New(dicType)));

            var addMethod = dicType.GetMethod("Add");
            foreach (var t in this.Pairs)
            {
                body.Add(Expression.Call(dicVar, addMethod, Expression.Constant(t.Item1), t.Item2.Reduce(symbols)));
            }

            body.Add(dicVar);
            return Expression.Block(new[] { dicVar }, body);
        }
    }
}
