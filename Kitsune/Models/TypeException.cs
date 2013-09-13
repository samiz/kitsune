using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.Models
{
    class TypeException : Exception
    {
        public TypeException(string msg) : base(msg)
        {
            
        }
    }
}
