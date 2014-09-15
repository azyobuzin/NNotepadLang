using System;
using System.Linq;
using System.Linq.Expressions;
using NNotepadLang.Expressions;
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Dynamic;
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
                "//".Sequence().IgnoreSeq(Chars.NoneOf('\r', '\n').Many())
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
                bool_in => "~=",
                bool_neq => "!=",
                bool_gre => ">=",
                bool_lee => "<=",
                pair_let => "=>",
                module_access => "::",
                range => "..",
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
               "scr", "rcs", "lambda", "require", "as", "with", "to", "tree", "eert", "yield",
               "para", "arap"
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
                "bool_neq",
                "bool_in"
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
                KeywordToIdentifier(g, "true"),
                KeywordToIdentifier(g, "false"),
                KeywordToIdentifier(g, "nil"),
                g["term", "string"],
                g["term", "number"],
                g["term", "tree"],
                g["term", "identifier"],
                g["expr", "assign"].Between(g["root", "lparen"], g["root", "rparen"]),
                g["value", "block"].Between(g["root", "lparen"], g["root", "rparen"]),
                g["list", "expr"].Between(g["root", "lbracket"], g["root", "rbracket"]),
                g["expr", "assign"].Between(g["root", "lbracket"], g["keyword", "to"]).Pipe(
                    g["expr", "assign"].Left(g["root", "rbracket"]),
                    (start, end) => YacqExpression.TypeCandidate(typeof(Enumerable))
                        .Method("Range", start, YacqExpression.Function("++", YacqExpression.Function("-", end, start)))
                )
            ));

            this.Add("term", "tree", g => g["term", "pair"].SepBy(1, g["symbol", "comma"]).Maybe()
                .Between(g["keyword", "tree"].IgnoreSeq(g["root", "newline"]), g["keyword", "eert"])
                .Select(pairs => new NlTreeExpression(pairs))
            );

            this.Add("term", "pair", g => g["term", "identifier"].Pipe(
                g["symbol", "pair_let"].Right(g["expr", "bool_or"]),
                (key, value) => new NlListExpression(key, value)
            ));

            this.Add("name", "class", g => g["term", "identifier"]
                .SepBy(1, g["symbol", "module_access"])
                .Select(exprs => new NlClassNameExpression(exprs.Select(expr => (expr as IdentifierExpression).Name)))
            );

            this.Add("expr", "post", g => g["root", "factor"]
                .Pipe(
                    Combinator.Choice(
                        g["list", "args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"])
                            .Select(expr => Tuple.Create(0, expr.Otherwise(() => new NlListExpression()))),
                        g["expr", "assign"].Between(g["root", "lbracket"], g["root", "rbracket"])
                            .Select(expr => Tuple.Create(1, expr)),
                        g["symbol", "member"].Right(g["term", "identifier"])
                            .Select(x => Tuple.Create(2, x)),
                        g["symbol", "increment"]
                            .Select(x => Tuple.Create(3, x)),
                        g["symbol", "decrement"]
                            .Select(x => Tuple.Create(4, x))
                    ).Many(1),
                    (factor, list) => list.Aggregate(factor, (p, t) =>
                    {
                        switch (t.Item1)
                        {
                            case 0:
                                return YacqExpression.List(new[] { factor }.Concat((t.Item2 as NlListExpression).Expressions));
                            case 1:
                                return YacqExpression.Function(".", factor, YacqExpression.Vector(t.Item2));
                            case 2:
                                return YacqExpression.Function(".", factor, t.Item2);
                            case 3:
                                return YacqExpression.Function("=++", factor);
                            default:
                                return YacqExpression.Function("=--", factor);
                        }
                    })
                )
                .Or(g["root", "factor"])
            );

            this.Add("expr", "unary", g => Combinator.Choice(
                g["expr", "post"],
                g["op", "prefix"].Pipe(g["expr", "post"], (x, y) => YacqExpression.List(x, y) as YacqExpression),
                g["symbol", "increment"].Right(g["expr", "post"]).Select(x => YacqExpression.Function("++=", x)),
                g["symbol", "decrement"].Right(g["expr", "post"]).Select(x => YacqExpression.Function("--=", x))
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

            this.Add("list", "expr", g => g["expr"].Last().SepBy(1, g["symbol", "comma"])
                .Select(exprs => new NlListExpression(exprs)));

            this.Add("stmt", "method", g => Combinator.Choice(
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["expr", "assign"].Between(g["keyword", "return"], g["root", "endline"]), //TODO
                g["list", "expr"].Left(g["root", "endline"])
            ));

            this.Add("stmt", "class", g => Combinator.Choice(
                g["def", "method"],
                g["def", "para"],
                g["def", "class_var"],
                g["def", "instance_var"],
                g["def", "global_var"],
                g["def", "property"]
            ));

            this.Add("stmt", "module", g => Combinator.Choice(
                g["def", "module"],
                g["def", "class"],
                g["def", "native_class"],
                g["name", "class"].Between(g["keyword", "include"], g["root", "endline"]) //TODO
            ));

            this.Add("stmt", "global", g => Combinator.Choice(
                g["def", "method"],
                g["def", "para"],
                g["def", "class"],
                g["def", "native_class"],
                g["def", "module"],
                g["def", "global_var"],
                g["block", "script"],
                g["line", "require"]
            ));

            this.Add("stmt", "script", g => Combinator.Choice(
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["expr", "assign"].Between(g["keyword", "return"], g["root", "endline"]),
                g["list", "expr"].Left(g["root", "endline"])
            ));

            this.Add("stmt", "block", g => Combinator.Choice(
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["keyword", "continue"].Left(g["root", "endline"]).Select(_ => YacqExpression.Function("$continue")),
                g["keyword", "break"].Left(g["root", "endline"]).Select(_ => YacqExpression.Function("$break")),
                g["expr", "assign"].Between(g["keyword", "return"], g["root", "endline"]),
                g["expr", "assign"].Between(g["keyword", "yield"], g["root", "endline"]),
                g["list", "expr"].Left(g["root", "endline"])
            ));

            this.Add("stmt", "para", g => Combinator.Choice(
                g["list", "expr"].Left(g["root", "endline"]),
                g["def", "local_var"],
                g["def", "instance_var"],
                g["def", "class_var"],
                g["def", "global_var"],
                g["block", "if"], g["block", "for"], g["line", "if"], g["later", "if"],
                g["block", "while"], g["block", "unless"], g["block", "switch"],
                g["line", "for"], g["line", "while"], g["line", "unless"],
                g["expr", "assign"].Between(g["keyword", "return"], g["root", "endline"]),
                g["expr", "assign"].Between(g["keyword", "yield"], g["root", "endline"])
            ));

            this.Add("root", "program", g =>
                g["root", "space?"].Right(g["stmt", "global"]).Many(1)
                    .Select(x => new NlBlockExpression(x))
            );

            this.Add("def", "method", g => g["deco", "access"].Maybe().Pipe(
               g["keyword", "override"].Maybe(),
               g["keyword", "native"].Maybe(),
               g["keyword", "def"].Right(g["term", "identifier"]),
               g["list", "method_args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"]).Maybe().Left(g["root", "newline"]),
               g["stmt", "method"].Many().Left(g["keyword", "fed"]).Left(g["root", "endline"].Maybe()),
               (access, @override, native, name, args, body) =>
                   new NlDefMethodExpression(access, @override, native, name, args, body)
            ));

            this.Add("def", "para", g => g["deco", "access"].Maybe().Pipe(
                g["keyword", "para"].Right(g["term", "identifier"]),
                g["list", "method_args"].Maybe().Between(g["root", "lparen"], g["root", "rparen"]).Maybe().Left(g["root", "newline"]),
                g["stmt", "para"].Many().Left(g["keyword", "arap"]).Left(g["root", "endline"].Maybe()),
                (access, name, args, body) => new NlDefParaExpression(access, name, args, body)
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
                g["list", "expr"].Between(g["keyword", "local"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Local));

            this.Add("def", "instance_var", g =>
                g["list", "expr"].Between(g["keyword", "instance"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Instance));

            this.Add("def", "class_var", g =>
                g["list", "expr"].Between(g["keyword", "class"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Class));

            this.Add("def", "global_var", g =>
                g["list", "expr"].Between(g["keyword", "global"], g["root", "endline"])
                    .Select(NlDefVariableExpression.Global));

            this.Add("block", "if", g => g["expr", "assign"].Between(g["keyword", "if"], g["root", "newline"])
                .Pipe(
                    g["stmt", "block"].Many(),
                    g["expr", "assign"].Between(g["keyword", "elif"], g["root", "newline"])
                        .Both(g["stmt", "block"].Many()).Many(),
                    Combinator.Sequence(g["keyword", "else"], g["root", "newline"])
                        .Right(g["stmt", "block"].Many()).Maybe(),
                    (cond, trueExpr, elif, falseExpr) =>
                        new NlIfExpression(cond, trueExpr, elif, falseExpr)
                )
                .Left(g["keyword", "fi"]).Left(g["root", "endline"].Maybe())
            );

            this.Add("line", "if", g => g["keyword", "if"].Right(g["expr", "assign"]).Pipe(
                g["list", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, trueExpr) => new NlIfExpression(cond, trueExpr)
            ));

            this.Add("block", "for", g => g["keyword", "for"].Right(g["term", "identifier"]).Pipe(
                g["expr", "assign"].Between(g["keyword", "in"], g["root", "newline"]),
                g["stmt", "block"].Many().Left(g["keyword", "rof"]).Left(g["root", "endline"].Maybe()),
                (identifier, source, exprs) => new NlForExpression(identifier, source, exprs)
            ));

            this.Add("line", "for", g => g["keyword", "for"].Right(g["term", "identifier"]).Pipe(
                g["expr", "assign"].Between(g["keyword", "in"], g["root", "newline"]),
                g["list", "expr"].Left(g["root", "endline"]),
                (identifier, source, expr) => new NlForExpression(identifier, source, expr)
            ));

            this.Add("block", "while", g => g["keyword", "while"].Right(g["expr", "assign"]).Pipe(
                g["stmt", "block"].Many().Between(g["root", "newline"], g["keyword", "elihw"]).Left(g["root", "endline"].Maybe()),
                (cond, exprs) => new NlWhileExpression(cond, exprs)
            ));

            this.Add("line", "while", g => g["keyword", "while"].Right(g["expr", "assign"]).Pipe(
                g["list", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, expr) => new NlWhileExpression(cond, expr)
            ));

            this.Add("block", "unless", g => g["keyword", "unless"].Right(g["expr", "assign"]).Pipe(
                g["stmt", "block"].Many().Between(g["root", "newline"], g["keyword", "sselnu"]).Left(g["root", "endline"].Maybe()),
                (cond, exprs) => new NlUnlessExpression(cond, exprs)
            ));

            this.Add("line", "unless", g => g["keyword", "unless"].Right(g["expr", "assign"]).Pipe(
                g["list", "expr"].Between(g["root", "newline"], g["root", "endline"]),
                (cond, expr) => new NlUnlessExpression(cond, expr)
            ));

            this.Add("later", "if", g => g["list", "expr"].Pipe(
                g["expr", "assign"].Between(g["keyword", "if"], g["root", "endline"]),
                (desc, cond) => new NlIfExpression(cond, desc)
            ));

            this.Add("block", "switch", g => g["expr", "assign"].Maybe().Between(g["keyword", "switch"], g["root", "newline"])
                .Pipe(
                    g["keyword", "case"].Right(g["list", "expr"])
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
                    g["symbol", "assign"].Right(g["expr", "assign"]).Maybe(),
                    (access, name, t, value) => new NlDefPropertyExpression(access, name, t, value)
                )
                .Left(g["root", "endline"])
            );

            this.Add("value", "block", g => g["val", "line_if"].Or(g["val", "block_if"]));

            this.Add("val", "line_if", g => g["expr", "assign"].Pipe(
                g["expr", "assign"].Between(g["keyword", "else"], g["keyword", "in"]),
                g["expr", "assign"],
                (trueExpr, falseExpr, cond) => new NlIfExpression(cond, trueExpr, falseExpr)
            ));

            this.Add("val", "block_if", g => g["expr", "assign"].Between(g["keyword", "if"], g["root", "newline"]).Pipe(
                g["expr", "assign"].Left(g["root", "endline"]),
                g["expr", "assign"].Between(g["keyword", "elif"], g["root", "newline"])
                    .Both(g["expr", "assign"]).Left(g["root", "endline"]).Many(),
                g["expr", "assign"].Between(
                    Combinator.Sequence(g["keyword", "else"], g["root", "newline"]),
                    Combinator.Sequence(g["root", "endline"], g["keyword", "fi"])
                ),
                (cond, trueExpr, elif, falseExpr) => new NlIfExpression(cond, trueExpr, elif, falseExpr)
            ));

            this.Add("line", "require", g => g["term", "string"].Between(g["keyword", "require"], g["root", "endline"])); //TODO: Select

            this.Add("def", "native_method", g => g["keyword", "def"].Right(g["term", "identifier"]).Pipe(
                g["keyword", "as"].Right(g["term", "string"]),
                (name, alternative) => YacqExpression.Ignore() //TODO
            ));

            this.Add("def", "native_class", g => g["keyword", "native"].IgnoreSeq(g["keyword", "class"]).Right(g["term", "identifier"]).Pipe(
                g["term", "string"].Between(g["keyword", "with"], g["root", "newline"]),
                g["def", "native_method"].Left(g["root", "endline"]).Many()
                    .Left(g["keyword", "ssalc"].IgnoreSeq(g["root", "endline"].Maybe())),
                (name, libraryName, members) => YacqExpression.Ignore() //TODO                
            ));

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
