using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.Services.Commanders
{
    public interface IProgramExecutioner
    {
        void ExecuteProgram(string program, string dir);
    }
}
