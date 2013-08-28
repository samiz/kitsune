using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public static class ViewExtensions
    {
        public static Point AbsolutePos(this IBlockView v)
        {
            Point p = v.RelativePos;
            while (v.Parent != null)
            {
                p = p.Offseted(v.Parent.RelativePos);
                v = v.Parent;
            }
            return p;
        }

        public static IBlockView AbsoluteAncestor(this IBlockView v)
        {
            while (v.Parent != null)
                v = v.Parent;
            return v;
        }
        public static bool HasBottomNotch(this IStackableBlockView v)
        {
            return v.EffectiveAttribute() == BlockAttributes.Hat
                || v.EffectiveAttribute() == BlockAttributes.Stack;
        }

        public static IEnumerable<DropRegion> EdgeDropRegions(
            this IInvokationBlockView view,
            Point origin, Size size)
        {
            BlockAttributes Attribute = view.Attribute;
            int Width = size.Width;
            int Height = size.Height;
            if (Attribute == BlockAttributes.Stack)
            {
                yield return new DropRegion(DropType.Above, new Rectangle(origin.Offseted(0, -2), new Size(Width, 5)), view);
                yield return new DropRegion(DropType.Below, new Rectangle(origin.Offseted(0, Height - 2), new Size(Width, 5)), view);
            }
            else if (Attribute == BlockAttributes.Hat)
            {
                yield return new DropRegion(DropType.Below, new Rectangle(origin.Offseted(0, Height - 2), new Size(Width, 5)), view);
            }
            else if (Attribute == BlockAttributes.Cap)
            {
                yield return new DropRegion(DropType.Above, new Rectangle(origin.Offseted(0, -2), new Size(Width, 5)), view);
            }
        }
    }
}
