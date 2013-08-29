using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    class ProcDefTextBit : IProcDefBit
    {
        public string Text;
        public ProcDefTextBit(string text)
        {
            this.Text = text;
        }

        public IProcDefBit DeepClone()
        {
            return new ProcDefTextBit(Text);
        }
    }
}
