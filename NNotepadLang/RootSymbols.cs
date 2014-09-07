using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NNotepadLang.Expressions;
using XSpect.Yacq;
using XSpect.Yacq.Dynamic;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using XSpect.Yacq.Symbols;

namespace NNotepadLang
{
    internal static class RootSymbols
    {
        [YacqSymbol(DispatchTypes.Method, "print")]
        public static Expression Print(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.TypeCandidate(typeof(Console)).Method(s, "WriteLine", e.Arguments);
        }

        [YacqSymbol(DispatchTypes.Method, "++=")]
        public static Expression PreIncrementAssign(DispatchExpression e, SymbolTable s, Type t)
        {
            try
            {
                return SymbolTable.Root.Match(DispatchTypes.Method, "++=")(e, s, t);
            }
            catch { }

            var target = e.Arguments[0];
            return YacqExpression.Contextful(
                YacqExpression.Function("=", target, YacqExpression.Function("++", YacqExpression.Contextful(target, ContextType.Dynamic))),
                ContextType.Dynamic
            );
        }

        [YacqSymbol(DispatchTypes.Method, "=++")]
        public static Expression PostIncrementAssign(DispatchExpression e, SymbolTable s, Type t)
        {
            try
            {
                return SymbolTable.Root.Match(DispatchTypes.Method, "=++")(e, s, t);
            }
            catch { }

            var target = e.Arguments[0];
            var firstResult = Expression.Variable(typeof(object));
            var secondResult = Expression.Variable(typeof(object));
            return new NlBlockExpression(
                new[] { firstResult, secondResult },
                YacqExpression.Function(s, "=", firstResult, target),
                YacqExpression.Function(s, "=", secondResult,
                    YacqExpression.Function(s, "++", YacqExpression.Contextful(firstResult, ContextType.Dynamic))),
                YacqExpression.Function(s, "=", target, YacqExpression.Contextful(secondResult, ContextType.Dynamic)),
                YacqExpression.Contextful(firstResult, ContextType.Dynamic)
            );
        }

        [YacqSymbol(DispatchTypes.Method, "--=")]
        public static Expression PreDecrementAssign(DispatchExpression e, SymbolTable s, Type t)
        {
            try
            {
                return SymbolTable.Root.Match(DispatchTypes.Method, "--=")(e, s, t);
            }
            catch { }

            var target = e.Arguments[0];
            return YacqExpression.Contextful(
                YacqExpression.Function("=", target, YacqExpression.Function("--", YacqExpression.Contextful(target, ContextType.Dynamic))),
                ContextType.Dynamic
            );
        }

        [YacqSymbol(DispatchTypes.Method, "=--")]
        public static Expression PostDecrementAssign(DispatchExpression e, SymbolTable s, Type t)
        {
            try
            {
                return SymbolTable.Root.Match(DispatchTypes.Method, "=--")(e, s, t);
            }
            catch { }

            var target = e.Arguments[0];
            var firstResult = Expression.Variable(typeof(object));
            var secondResult = Expression.Variable(typeof(object));
            return new NlBlockExpression(
                new[] { firstResult, secondResult },
                YacqExpression.Function(s, "=", firstResult, target),
                YacqExpression.Function(s, "=", secondResult,
                    YacqExpression.Function(s, "--", YacqExpression.Contextful(firstResult, ContextType.Dynamic))),
                YacqExpression.Function(s, "=", target, YacqExpression.Contextful(secondResult, ContextType.Dynamic)),
                YacqExpression.Contextful(firstResult, ContextType.Dynamic)
            );
        }
    }
}
