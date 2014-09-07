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
            var code = @"scr:
for i in [1 to 10]:
    print(i);
rof
rcs";
            Console.WriteLine(NotepadLang.ReadAll(code).Last());
            Console.WriteLine();

            NotepadLang.ParseAction(code).Compile()();

            Console.WriteLine();
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
