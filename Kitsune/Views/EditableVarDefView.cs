using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class EditableVarDefView : ITextualView
    {
        public event ViewChangedEvent Changed;
        VarDefBlock _model;
        Bitmap _cached;

        Nine abc;
        Bitmap bitmap;
        Graphics textMetrics;
        Font textFont;

        public EditableVarDefView(VarDefBlock model, Bitmap bitmap, Nine abc, Graphics textMetrics, Font textFont)
        {
            this._model = model;
            this.Changed += delegate(object source) { };

            // todo: So much in common with TextView; consider abstracting into common implementation
            this.abc = abc;
            this.bitmap = bitmap;
            this.textMetrics = textMetrics;
            this.textFont = textFont;
        }

        public void SetBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            Reassemble();
            Changed(this);
        }

        public Bitmap Assemble()
        {
            if (_cached == null)
                Reassemble();
            return _cached;
        }

        public void Reassemble()
        {
            Size sz = textMetrics.MeasureString(_model.Name, textFont).ToSize();

            int w = Math.Max(sz.Width, abc.MinWidth) + 2;
            int h = Math.Max(sz.Height, abc.MinHeight) + 2;
            int middleWidth = w - (abc.NW.Width + abc.NE.Width);
            int middleHeight = h - (abc.NW.Height + abc.SW.Height);
            _cached = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(_cached))
            {
                g.Clear(Color.Transparent);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                abc.RenderToFit(g, middleWidth, middleHeight);
                g.DrawImageUnscaled(bitmap, 2, 2);
            }
        }

        public IBlock Model { get { return _model; }
        }

        public IBlockView Parent { get; set; }
        public System.Drawing.Point RelativePos { get; set; }

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
            return new Rectangle(p, _cached.Size);
        }
    }
}
