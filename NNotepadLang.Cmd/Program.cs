using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNotepadLang;
using XSpect.Yacq;
using XSpect.Yacq.LanguageServices;

namespace NNotepadLang.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var exprs = YacqServices.ReadAll(new Reader(NlGrammar.Default), File.ReadAllText(@"C:\Users\azyobuzin\Desktop\test.np"));
            Debugger.Break();
        }
    }
}
