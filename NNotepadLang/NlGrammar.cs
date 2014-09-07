using System;
using System.Linq;
using System.Linq.Expressions;
using NNotepadLang.Expressions;
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;

namespace NNotepadLang
{
    public class NlGrammar : Grammar
    {
        public NlGrammar()
        {
            this.Add("root", "space", g => g["root", "comment"].Ignore()
                .Or(Chars.OneOf(' ', '\t', '\v', '\f', '\r', '\n').Ignore())
                .Many(1).Select(Ignore)
            );
            this.Add("root", "space?", g => g["root", "space"].Maybe().Select(Ignore));
            this.Add("root", "lparen", g => '('.Satisfy().Right(g["root", "space?"]));
            this.Add("root", "rparen", g => ')'.Satisfy().Right(g["root", "space?"]));
            this.Add("root", "lbracket", g => '['.Satisfy().Right(g["root", "space?"]));
            this.Add("root", "rbracket", g => ']'.Satisfy().Right(g["root", "space?"]));
            this.Add("root", "newline", g => ':'.Satisfy().Right(g["root", "space?"]));
            this.Add("root", "endline", g => ';'.Satisfy().Right(g["root", "space?"]));

            this.Add("root", "pnl", g =>
                Combinator.Choice("\r\n".Sequence(), "\r".Sequence(), "\n".Sequence())
                    .Select(Ignore));

            this.Add("root", "comment", g => g["comment", "line"].Or(g["comment", "multi"]));

            this.Add("comment", "line", g =>
                Combinator.Sequence("//".Sequence(), Chars.NoneOf('\r', '\n').Many()).Select(Ignore)
            );

            var commentStart = "/*".Sequence();
            var commentEnd = "*/".Sequence();
            this.Add("comment", "multi", g => commentEnd.Not().Right(Chars.Any()).Many()
                .Between(commentStart, commentEnd).Select(Ignore));

            this.AddSymbols(
                assign_right_shift => ">>=",
                assign_left_shift => "<<=",
                bool_equ_s => "===",
                bool_neq_s => "!==",
                assign_plus => "+=",
                assign_minus => "-=",
                assign_multiple => "*=",
                assign_divide => "/=",
                assign_xor => "^=",
                assign_modulus => "%=",
                assign_or => "|=",
                assign_and => "&=",
                bool_equ => "==",
                bool_neq => "!=",
                bool_gre => ">=",
                bool_lee => "<=",
                module_access => "::",
                bool_andalso => "&&",
                bool_orelse => "||",
                right_shift => ">>",
                left_shift => "<<",
                increment => "++",
                decrement => "--",
                bool_grt => ">",
                bool_les => "<",
                plus => "+",
                minus => "-",
                multiple => "*",
                divide => "/",
                bin_xor => "^",
                modulus => "%",
                bin_or => "|",
                bin_and => "&",
                member => ".",
                invert => "!",
                assign => "=",
                comma => ","
            );

            this.AddKeywords(
               "attr", "class", "def", "elif", "else", "false", "fed",
               "fi", "global", "if", "instance", "local", "nil", "porp",
               "prop", "rtta", "ssalc", "true", "iface", "ecafi", "module", "eludom",
               "for", "in", "continue", "rof", "while", "elihw",
               "switch", "case", "break", "default", "hctiws", "return",
               "include", "mix", "extends", "implements", "override", "native", "self",
               "public", "protected", "private", "unless", "sselnu",
               "scr", "rcs", "lambda", "require"
            );

            var digit = Chars.Number();
            var alpha = Chars.Satisfy(c => ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z') || c == '_');

            this.Add("term", "number", g => digit.Many(1).Select(string.Concat).Pipe(
                '.'.Satisfy().Right(digit.Many(1)).Select(string.Concat).Maybe().Left(g["root", "space?"]),
                (x, y) => YacqExpression.Number(
                    y.Select(c => string.Concat(x, ".", c)).Otherwise(() => x))
            ));
            this.Add("term", "identifier", g => alpha.Pipe(
                alpha.Or(digit).Many().Left(g["root", "space?"]),
                (x, y) => YacqExpression.Identifier(x + string.Concat(y))
            ));

            this.Add("term", "string", g => Chars.NoneOf('\\', '"')
                .Or('\\'.Satisfy().Right(Chars.Any()))
                .Many().Between('"'.Satisfy(), '"'.Satisfy().Right(g["root", "space?"]))
                .Select(c => YacqExpression.Text('"', string.Concat(c)))
            );

            this.Add("deco", "access", g =>
                Combinator.Choice(
                    g["keyword", "protected"].Select(_ => AccessModifier.Protected),
                    g["keyword", "private"].Select(_ => AccessModifier.Private),
                    g["keyword", "public"].Select(_ => AccessModifier.Public)
                )
                .Select(a => new NlAccessModifierExpression(a))
            );

            this.Add("op", "prefix", SymbolChoice("plus", "minus", "invert"));

            this.Add("op", "postfix", SymbolChoice("increment", "decrement"));

            this.Add("op", "assign", SymbolChoice(
                "assign_right_shift",
                "assign_left_shift",
                "assign_plus",
                "assign_minus",
                "assign_multiple",
                "assign_divide",
                "assign_xor",
                "assign_modulus",
                "assign_or",
                "assign_and",
                "assign"
            ));

            this.Add("op", "eq", SymbolChoice(
                "bool_equ_s",
                "bool_neq_s",
                "bool_equ",
                "bool_neq"
            ));

            this.Add("op", "rel", SymbolChoice(
                "bool_gre",
                "bool_lee",
                "bool_grt",
                "bool_les"
            ));

            this.Add("op", "shift", SymbolChoice("right_shift", "left_shift"));

            this.Add("op", "add", SymbolChoice("plus", "minus"));

            this.Add("op", "mul", SymbolChoice("multiple", "divide", "modulus"));

            this.Add("root", "factor", g => Combinator.Choice(
                g["term", "string"],
                g["term", "number"],
                g["term", "identifier"]/*.Pipe(
                    g["list", "args"].Between(g["root", "lparen"], g["root", "rparen"]).Maybe(),
                    (x, y) => YacqExpression.List(x) as YacqExpression //TODO
                )*/ /* Ruby 版の更新 */,
                KeywordToIdentifier(g, "true"),
                KeywordToIdentifier(g, "false"),
                KeywordToIdentifier(g, "nil"),
                g["root", "expr"].Between(g["root", "lparen"], g["root", "rparen"]),
                g["value", "block"].Between(g["root", "lparen"], g["root", "rparen"])                    
            ));

            this.Add("name", "class", g => g["term", "identifier"]
                .SepBy(1, g["symbol", "module_access"])
                .Select(exprs => new NlClassNameExpression(exprs.Select(expr => (expr as IdentifierExpression).Name)))
            );

            this.Add("expr", "post", g => g["root", "factor"]
                .Pipe(
                    Combinator.Choice(
                        g["list", "args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"])
                            .Select(expr => Tuple.Create(0, expr.Otherwise(() => new NlListExpression()), YacqExpression.Ignore() as YacqExpression)),
                        g["root", "expr"].Between(g["root", "lbracket"], g["root", "rbracket"])
                            .Select(expr => Tuple.Create(1, expr, YacqExpression.Ignore() as YacqExpression)),
                        g["symbol", "member"].Pipe(g["term", "identifier"],
                            (x, y) => Tuple.Create(2, x, y)),
                        g["symbol", "increment"].Or(g["symbol", "decrement"])
                            .Select(x => Tuple.Create(3, x, YacqExpression.Ignore() as YacqExpression))
                    ).Many(1),
                    (factor, list) => list.Aggregate(factor, (p, t) =>
                    {
                        switch (t.Item1)
                        {
                            case 0:
                                //TODO: メソッド
                                return YacqExpression.Ignore();
                            case 1:
                                //TODO: インデクサ
                                return YacqExpression.Ignore();
                            case 2:
                                return YacqExpression.List(t.Item2, factor, t.Item3);
                            default:
                                return YacqExpression.List(t.Item2, factor); //TODO: hoge++ と ++hoge の挙動を変える
                        }
                    })
                )
                .Or(g["root", "factor"])
            );

            this.Add("expr", "unary", g => Combinator.Choice(
                g["expr", "post"],
                g["op", "prefix"].Pipe(g["root", "factor"], (x, y) => YacqExpression.List(x, y) as YacqExpression),
                g["symbol", "increment"].Or(g["symbol", "decrement"])
                    .Pipe(g["expr", "unary"], (x, y) => YacqExpression.List(x, y) as YacqExpression)
            ));

            this.AddBinaryOperator("mul", "op", "mul", "unary");
            this.AddBinaryOperator("add", "op", "add", "mul");
            this.AddBinaryOperator("shift", "op", "shift", "add");
            this.AddBinaryOperator("rel", "op", "rel", "shift");
            this.AddBinaryOperator("eq", "op", "eq", "rel");
            this.AddBinaryOperator("and", "symbol", "bin_and", "eq");
            this.AddBinaryOperator("xor", "symbol", "bin_xor", "and");
            this.AddBinaryOperator("or", "symbol", "bin_or", "xor");
            this.AddBinaryOperator("bool_and", "symbol", "bool_andalso", "or");
            this.AddBinaryOperator("bool_or", "symbol", "bool_orelse", "bool_and");

            this.Add("expr", "assign", g =>
                g["expr", "bool_or"].Pipe(
                    g["op", "assign"],
                    Tuple.Create
                )
                .Many()
                .Pipe(
                    g["expr", "bool_or"],
                    (left, right) => left.Reverse().Aggregate(right, (expr, t) =>
                        YacqExpression.List(t.Item2, t.Item1, expr))
                )
            );

            this.Add("list", "args", g => g["expr"].Last().SepBy(1, g["symbol", "comma"])
                .Select(exprs => new NlListExpression(exprs)));

            this.Add("list", "method_args", g => g["term", "identifier"].SepBy(1, g["symbol", "comma"])
                .Select(exprs => new NlListExpression(exprs)));

            this.Add("list", "classes", g => g["name", "class"].SepBy(1, g["symbol", "comma"])
                .Select(exprs => new NlListExpression(exprs)));

            this.Add("root", "expr", g => g["expr"].Last().SepBy(1, g["symbol", "comma"])
                .Select(exprs => new NlListExpression(exprs))); //TODO: もしかして最後だけが戻り値になる的な？

            this.Add("method", "calling", g => g["list", "args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"])
                .Select(o => o.Otherwise(() => new NlListExpression())));

            this.Add("stmt", "method", g => Combinator.Choice(
                g["root", "expr"].Left(g["root", "endline"]),
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["root", "expr"].Between(g["keyword", "return"], g["root", "endline"]) //TODO
            ));

            this.Add("stmt", "class", g => Combinator.Choice(
                g["def", "method"],
                g["def", "class_var"],
                g["def", "instance_var"],
                g["def", "global_var"],
                g["def", "property"]
            ));

            this.Add("stmt", "module", g => Combinator.Choice(
                g["def", "module"],
                g["def", "class"],
                g["name", "class"].Between(g["keyword", "include"], g["root", "endline"]) //TODO
            ));

            this.Add("stmt", "global", g => Combinator.Choice(
                g["def", "method"],
                g["def", "class"],
                g["def", "module"],
                g["def", "global_var"],
                g["block", "script"],
                g["line", "require"]
            ));

            this.Add("stmt", "script", g => Combinator.Choice(
                g["root", "expr"].Left(g["root", "endline"]),
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["keyword", "continue"].Left(g["root", "endline"]),
                g["keyword", "continue"].Left(g["root", "endline"]),
                g["root", "expr"].Between(g["keyword", "return"], g["root", "endline"])
            ));

            this.Add("stmt", "block", g => Combinator.Choice(
                g["root", "expr"].Left(g["root", "endline"]),
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["keyword", "continue"].Left(g["root", "endline"]),
                g["keyword", "continue"].Left(g["root", "endline"]),
                g["root", "expr"].Between(g["keyword", "return"], g["root", "endline"])
            ));

            this.Add("root", "program", g =>
                g["root", "space?"].Right(g["stmt", "global"]).Many(1)
                    .Select(x => YacqExpression.AmbiguousLambda(x)));

            this.Add("def", "method", g => g["deco", "access"].Maybe().Pipe(
               g["keyword", "override"].Maybe(),
               g["keyword", "native"].Maybe(),
               g["keyword", "def"].Right(g["term", "identifier"]),
               g["list", "method_args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"]).Maybe().Left(g["root", "newline"]),
               g["stmt", "method"].Many().Left(g["keyword", "fed"]).Left(g["root", "endline"].Maybe()),
               (access, @override, native, name, args, body) =>
                   new NlDefMethodExpression(access, @override, native, name, args, body)
            ));

            this.Add("def", "class", g => g["deco", "access"].Maybe().Pipe(
                g["keyword", "class"].Right(g["term", "identifier"]),
                g["keyword", "extends"].Right(g["list", "classes"]).Maybe(),
                g["keyword", "implements"].Right(g["list", "classes"]).Maybe(),
                g["root", "newline"].Right(g["stmt", "class"].Many())
                    .Left(g["keyword", "ssalc"]).Left(g["root", "endline"].Maybe()),
                (access, name, extends, implements, members) =>
                    new NlDefClassExpression(access, name, extends, implements, members)
            ));

            this.Add("def", "module", g => g["keyword", "module"].Right(g["term", "identifier"]).Pipe(
                g["root", "newline"].Right(g["stmt", "module"].Many())
                    .Left(g["keyword", "eludom"]).Left(g["root", "endline"].Maybe()),
                (name, members) => new NlDefModuleExpression(name, members)
            ));

            this.Add("def", "local_var", g =>
                g["root", "expr"].Between(g["keyword", "local"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Local));

            this.Add("def", "instance_var", g =>
                g["root", "expr"].Between(g["keyword", "instance"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Instance));

            this.Add("def", "class_var", g =>
                g["root", "expr"].Between(g["keyword", "class"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Class));

            this.Add("def", "global_var", g =>
                g["root", "expr"].Between(g["keyword", "global"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Global));

            this.Add("block", "if", g => g["root", "expr"].Between(g["keyword", "if"], g["root", "newline"])
                .Pipe(
                    g["stmt", "block"].Many(),
                    g["root", "expr"].Between(g["keyword", "elif"], g["root", "newline"])
                        .Both(g["stmt", "block"].Many()).Many(),
                    Combinator.Sequence(g["keyword", "else"], g["root", "newline"])
                        .Right(g["stmt", "block"].Many()).Maybe(),
                    (cond, trueExpr, elif, falseExpr) =>
                        new NlIfExpression(cond, trueExpr, elif, falseExpr)
                )
                .Left(g["keyword", "fi"]).Left(g["root", "endline"].Maybe())
            );

            this.Add("line", "if", g => g["keyword", "if"].Right(g["root", "expr"]).Pipe(
                g["root", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, trueExpr) => new NlIfExpression(cond, trueExpr)
            ));

            this.Add("block", "for", g => g["keyword", "for"].Right(g["term", "identifier"]).Pipe(
                g["root", "expr"].Between(g["keyword", "in"], g["root", "newline"]),
                g["stmt", "block"].Many().Left(g["keyword", "rof"]).Left(g["root", "endline"].Maybe()),
                (identifier, source, exprs) => new NlForExpression(identifier, source, exprs)
            ));

            this.Add("line", "for", g => g["keyword", "if"].Right(g["root", "expr"]).Pipe(
                g["root", "expr"].Between(g["keyword", "in"], g["root", "newline"]),
                g["root", "expr"].Left(g["root", "endline"]),
                (identifier, source, expr) => new NlForExpression(identifier, source, expr)
            ));

            this.Add("block", "while", g => g["keyword", "while"].Right(g["root", "expr"]).Pipe(
                g["stmt", "block"].Many().Between(g["root", "newline"], g["keyword", "elihw"]).Left(g["root", "endline"].Maybe()),
                (cond, exprs) => new NlWhileExpression(cond, exprs)
            ));

            this.Add("line", "while", g => g["keyword", "while"].Right(g["root", "expr"]).Pipe(
                g["root", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, expr) => new NlWhileExpression(cond, expr)
            ));

            this.Add("block", "unless", g => g["keyword", "unless"].Right(g["root", "expr"]).Pipe(
                g["stmt", "block"].Many().Between(g["root", "newline"], g["keyword", "sselnu"]).Left(g["root", "endline"].Maybe()),
                (cond, exprs) => new NlUnlessExpression(cond, exprs)
            ));

            this.Add("line", "unless", g => g["keyword", "unless"].Right(g["root", "expr"]).Pipe(
                g["root", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, expr) => new NlUnlessExpression(cond, expr)
            ));

            this.Add("later", "if", g => g["root", "expr"].Pipe(
                g["root", "expr"].Between(g["keyword", "if"], g["root", "endline"]),
                (desc, cond) => new NlIfExpression(cond, desc)
            ));

            this.Add("block", "switch", g => g["root", "expr"].Maybe().Between(g["keyword", "switch"], g["root", "newline"])
                .Pipe(
                    g["keyword", "case"].Right(g["root", "expr"])
                        .Both(g["root", "newline"].Right(g["stmt", "block"].Many())).Many(),
                    Combinator.Sequence(g["keyword", "default"], g["root", "newline"])
                        .Right(g["stmt", "block"].Many()).Maybe(),
                    (cond, cases, defaultExprs) => new NlSwitchExpression(cond, cases, defaultExprs)
                )
                .Left(g["keyword", "hctiws"]).Left(g["root", "endline"].Maybe())
            );

            this.Add("block", "script", g => g["stmt", "script"].Many()
                .Between(
                    Combinator.Sequence(g["keyword", "scr"], g["root", "newline"]),
                    g["keyword", "rcs"].Right(g["root", "endline"].Maybe())
                )
                .Select(exprs => new NlScriptExpression(exprs))
            );

            this.Add("def", "property", g => g["deco", "access"].Maybe()
                .Pipe(
                    g["keyword", "prop"].Right(g["term", "identifier"]),
                    g["deco", "access"].Between(g["root", "lparen"], g["symbol", "comma"])
                        .Both(g["deco", "access"].Left(g["root", "rparen"])).Maybe(),
                    g["symbol", "assign"].Right(g["root", "expr"]).Maybe(),
                    (access, name, t, value) => new NlDefPropertyExpression(access, name, t, value)
                )
                .Left(g["root", "endline"])
            );

            this.Add("value", "block", g => g["val", "line_if"].Or(g["val", "block_if"]));

            this.Add("val", "line_if", g => g["root", "expr"].Pipe(
                g["root", "expr"].Between(g["keyword", "else"], g["keyword", "in"]),
                g["root", "expr"],
                (trueExpr, falseExpr, cond) => new NlIfExpression(cond, trueExpr, falseExpr)
            ));

            this.Add("val", "block_if", g => g["root", "expr"].Between(g["keyword", "if"], g["root", "newline"]).Pipe(
                g["root", "expr"].Left(g["root", "endline"]),
                g["root", "expr"].Between(g["keyword", "elif"], g["root", "newline"])
                    .Both(g["root", "expr"]).Left(g["root", "endline"]).Many(),
                g["root", "expr"].Between(
                    Combinator.Sequence(g["keyword", "else"], g["root", "newline"]),
                    Combinator.Sequence(g["root", "endline"], g["keyword", "fi"])
                ),
                (cond, trueExpr, elif, falseExpr) => new NlIfExpression(cond, trueExpr, elif, falseExpr)
            ));

            this.Add("line", "require", g => g["term", "string"].Between(g["keyword", "require"], g["root", "endline"])); //TODO: Select

            this.Add("root", "ignore", g => g["root", "space?"]);
            this.Set.Default = g => g["root", "program"];
        }

        private static YacqExpression Ignore<T>(T _)
        {
            return YacqExpression.Ignore();
        }

        private void Add<T>(string category, string id, Func<RuleGetter, Parser<char, T>> rule)
            where T : YacqExpression
        {
            base.Add(category, id, g => Combinator.Lazy(() => rule(g).Select(x => x as YacqExpression)));
        }

        private void AddSymbols(params Expression<Func<object, string>>[] syms)
        {
            foreach (var expr in syms)
            {
                this.Add("symbol", expr.Parameters[0].Name,
                    g => ((expr.Body as ConstantExpression).Value as string).Sequence()
                        .Select(c => YacqExpression.Identifier(string.Concat(c)))
                        .Left(g["root", "space?"]));
            }
        }

        private void AddKeywords(params string[] names)
        {
            foreach (var name in names)
            {
                this.Add("keyword", name, g => name.Sequence().Right(g["root", "space?"]));
            }
        }

        private static Func<RuleGetter, Parser<char, YacqExpression>> SymbolChoice(params string[] syms)
        {
            return g => syms.Select(sym => g["symbol", sym]).Choice();
        }

        private static Parser<char, YacqExpression> KeywordToIdentifier(RuleGetter g, string keyword)
        {
            return g["keyword", keyword].Select(_ => YacqExpression.Identifier(keyword) as YacqExpression);
        }

        private void AddBinaryOperator(string name, string category, string id, string next, string prev = null)
        {
            this.Add("expr", name, g => g["expr", prev ?? next].Pipe(
                g[category, id].Pipe(
                    g["expr", next],
                    Tuple.Create
                )
                .Many(),
                (left, right) => right.Aggregate(left, (l, t) => YacqExpression.List(t.Item1, left, t.Item2))
            ));
        }

        private bool isReadOnly = false;
        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        private static volatile NlGrammar _default;
        public static NlGrammar Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new NlGrammar();
                    _default.isReadOnly = true;
                }
                return _default;
            }
        }
    }
}
