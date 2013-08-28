using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class LabelView : IBlockView
    {
        public event ViewChangedEvent Changed;
        public Bitmap bitmap;
        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }
        public LabelView(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            Changed += delegate(object sender) { };
        }

        public IBlock Model
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public System.Drawing.Bitmap Assemble()
        {
            return bitmap;
        }

        public void Reassemble()
        {
        }

        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            return new DropRegion[] { };
        }

        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            return new DropRegion[] { };
        }

        public bool HasPoint(Point p, Point origin)
        {
            return Bounds(origin).Contains(p);
        }

        public IBlockView ChildHasPoint(Point p, Point origin)
        {
            return null;
        }

        Rectangle Bounds(Point p)
        {
            return new Rectangle(p, bitmap.Size);
        }
    }
}
