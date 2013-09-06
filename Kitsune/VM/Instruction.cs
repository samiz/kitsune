using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public abstract class Instruction
    {
        public VM vm;
        public Instruction(VM vm)
        {
            this.vm = vm;
        }
        public abstract void Run(Process p);
        public override abstract string ToString();
    }
}
