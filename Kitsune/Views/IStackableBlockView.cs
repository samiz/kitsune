using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public interface IStackableBlockView : IBlockView
    {
        BlockAttributes EffectiveAttribute();
    }
}
