using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public enum ArgViewType { Content, Script, Unknown }
    public class ArgumentPartition
    {
        public int N;
        public ArgViewType Type;

        public ArgumentPartition(int n, ArgViewType argViewType)
        {
            this.N = n;
            this.Type = argViewType;
        }
    }
}
