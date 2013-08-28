using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public delegate void ViewChangedEvent (object source);
    public interface IBlockView
    {
        event ViewChangedEvent Changed;
        Bitmap Assemble();
        void Reassemble();

        IBlock Model { get; }
        IBlockView Parent { get; set; }
        Point RelativePos { get; set; }
        IEnumerable<DropRegion> DropRegions(Point origin);
        IEnumerable<DropRegion> ChildDropRegions(Point origin);
        
        bool HasPoint(Point p, Point origin);
        IBlockView ChildHasPoint(Point p, Point origin);
    }
}
