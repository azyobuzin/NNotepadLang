using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XSpect.Yacq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using XSpect.Yacq.Symbols;
using XSpect.Yacq.SystemObjects;

namespace NNotepadLang
{
    public static class NotepadLang
    {
        public static IReadOnlyList<YacqExpression> ReadAll(IEnumerable<char> code)
        {
            return YacqServices.ReadAll(new Reader(NlGrammar.Default), code);
        }

        private static SymbolTable CreateSymbolTable(SymbolTable symbols)
        {
            symbols = new SymbolTable(symbols, typeof(RootSymbols));
            symbols["$global"] = Expression.Constant(symbols);
            symbols["*reader*"] = Expression.Constant(new Reader(NlGrammar.Default));
            if (!symbols.ExistsKey("*assembly*"))
                Expression.Constant(new YacqAssembly("NNotepadLangAssembly"));
            return symbols;
        }

        public static IReadOnlyList<Expression> ParseAll(SymbolTable symbols, IEnumerable<char> code)
        {
            return YacqServices.ParseAll(CreateSymbolTable(symbols), code);
        }

        public static Expression<Action> ParseAction(SymbolTable symbols, IEnumerable<char> code)
        {
            return YacqServices.ParseAction(CreateSymbolTable(symbols), code);
        }

        public static Expression<Action> ParseAction(IEnumerable<char> code)
        {
            return ParseAction(null, code);
        }
    }
}
