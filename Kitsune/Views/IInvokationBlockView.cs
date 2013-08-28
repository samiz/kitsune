using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public interface IInvokationBlockView : IStackableBlockView
    {
        void SetArgView(int i, IBlockView v);
        BlockAttributes Attribute { get; }
    }
}
