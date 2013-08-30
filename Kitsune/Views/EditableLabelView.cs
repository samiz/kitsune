using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class EditableLabelView : ITextualView
    {
        public event ViewChangedEvent Changed;
        public Bitmap bitmap;
        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }
        public ITextualBlock model;
        public EditableLabelView(Bitmap bitmap, ITextualBlock model)
        {
            this.model = model;
            this.bitmap = bitmap;
            Changed += delegate(object sender) { };
        }

        public void SetBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            Changed(this);
        }

        public IBlock Model
        {
            get
            {
                return model;
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
            return this;
        }

        Rectangle Bounds(Point p)
        {
            return new Rectangle(p, bitmap.Size);
        }
    }
}
