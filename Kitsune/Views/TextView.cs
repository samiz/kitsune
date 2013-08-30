using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class TextView : ITextualView
    {
        public event ViewChangedEvent Changed;
        public Bitmap _cached;
        private TextBlock model;
        private IBlockView _parent;
        Nine abc;
        Graphics textMetrics;
        Font textFont;

        
        public Point RelativePos { get; set; }
        public TextView(TextBlock model, Nine abc, Graphics textMetrics, Font textFont)
        {
            this.model = model;
            this.model.TextChanged += new TextBlockTextChangedEvent(model_TextChanged);
            
            this.abc = abc;
            this.textMetrics = textMetrics;
            this.textFont = textFont;

            Changed += delegate(object sender) { };
        }

        void model_TextChanged(object sender, string newStr)
        {
            Reassemble();
            Changed(this);
        }

        public IBlock Model
        {
            get
            {
                return model;
            }
        }
        public bool Editable { get { return true; } }
        public IBlockView Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
            }
        }

        public Bitmap Assemble()
        {
            if (_cached == null)
                Reassemble();
            return _cached;
        }

        public void Reassemble()
        {
            Size sz = textMetrics.MeasureString(model.Text, textFont).ToSize();
            
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
                g.DrawString(model.Text, textFont, Brushes.Black, 2, 2);
            }
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
            return new Rectangle(p, _cached.Size);
        }
    }
}
