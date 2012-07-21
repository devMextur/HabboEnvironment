using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabboEnvironment_R3.Kernel.IO
{
    public class DynamicConsoleLine
    {
        public int LogLineId
        {
            get;
            private set;
        }

        public DynamicConsoleLine(int LogLineId)
        {
            this.LogLineId = LogLineId;
        }

        public void Update(ConsoleSystem ConsoleSystem, string Type, string Line, params object[] Params)
        {
            Console.SetCursorPosition(0, LogLineId);

            ConsoleSystem.PrintLine(Type, Line, Params);
        }
    }
}
