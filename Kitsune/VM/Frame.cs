using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public class Frame
    {
        public Method Method;
        public int IP;
        public Dictionary<string, object> Locals = new Dictionary<string, object>();
        public Frame(Method method)
        {
            this.Method = method;
            IP = 0;
        }
        internal Instruction NextInstruction()
        {
            return Method.Instructions[IP++];
        }
    }
}
