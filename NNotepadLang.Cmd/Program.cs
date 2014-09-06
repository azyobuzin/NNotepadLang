using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNotepadLang;
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq;
using XSpect.Yacq.LanguageServices;

namespace NNotepadLang.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //var exprs = YacqServices.ReadAll(new Reader(NlGrammar.Default), File.ReadAllText(@"C:\Users\azyobuzin\Desktop\test.np"));
            //Debugger.Break();
            var g = NlGrammar.Default.Get;

            var test = g["name", "class"];
            var ret = test("test::a".AsStream());

            Console.Write(ret.Status);
            Console.ReadKey();
        }
    }
}
