using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public class Method
    {
        public Instruction[] Instructions;
        public Dictionary<string, int> Labels = new Dictionary<string, int>();
        public int Arity;

        internal void PrepareLabels()
        {
            for (int i = 0; i < Instructions.Length; ++i)
            {
                Instruction ins = Instructions[i];
                if (ins is Label)
                {
                    Labels[((Label)ins).label] = i + 1;
                }
            }
        }
        public override string ToString()
        {
            return Instructions.Select(i => i.ToString()).Combine("\n");
        }
    }
}
