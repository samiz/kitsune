using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    /*
     Bits that are part of a proc's definition, like a formal param or a text bit
     send the value [x:int] to [y:channel]
      ^ textbit       ^ formal
     */
    public interface IProcDefBit
    {
        IProcDefBit DeepClone();
        string ArgBitString();
    }
}
