using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabboEnvironment_R3.Kernel.IO
{
    public class ConsoleSystem
    {
        public void Serialize()
        {
            Console.Title += " based on [HE2]";
            Console.WriteLine();
        }

        public DynamicConsoleLine GetDynamicConsoleLine(string Type, string Line, params object[] Parameters)
        {
            PrintLine(Type, Line, Parameters);
            return new DynamicConsoleLine(Console.CursorTop - 1);
        }

        public void PrintLine(string Type, string Line, params object[] Parameters)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(" <{0}> ", Type);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Line, Parameters);
        }

        public void PrintLine(ConsoleColor Color, string Type, string Line, params object[] Parameters)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(" <{0}> ", Type);
            Console.ForegroundColor = Color;
            Console.WriteLine(Line, Parameters);
        }

        public void PrintLine(string Line, params object[] Parameters)
        {
            PrintLine("GLOBAL", Line, Parameters);
        }
    }
}