using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace Kitsune
{
    public class ContentView : IBlockView 
    {
        public event ViewChangedEvent Changed;
        List<IBlockView> argViews = new List<IBlockView>();
        BitArray trueArgs;
        List<int> argIndexes = new List<int>();
        List<Bitmap> parts = new List<Bitmap>();
        Bitmap _cached;
        
        bool fresh = false;
        int _width, _height;
        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }
        NineContent abc;
        List<DataType> ArgTypes = new List<DataType>();

        public ContentView(IBlockView[] argViews, DataType[] argTypes, BitArray trueArgs, NineContent abc)
        {
            this.trueArgs = trueArgs;
            this.ArgTypes.AddRange(argTypes);
            this.abc = abc;
            Changed += delegate(object sender) {};
            int i = 0;
            foreach (IBlockView v in argViews)
            {
                AddSubView(v, argTypes[i], i);
                i++;
            }
            Reassemble();
        }

        public void SetArgView(int i, IBlockView v)
        {
            SetSubView(argIndexes[i], v);
        }

        public void SetSubView(int i, IBlockView v)
        {
            IBlockView oldSubView = argViews[i];
            Detach(oldSubView);

            if (v.Parent != null)
            {
                ((ContentView)v.Parent).Detach(v);
            }

            Attach(v);
            argViews[i] = v;
            parts[i] = v.Assemble();
            Reassemble();
            Changed(this);
        }
        public void AddSubView(IBlockView v, DataType argType)
        {
            AddSubView(v, argType, argViews.Count);
        }
        public void AddSubView(IBlockView v, DataType argType, int i)
        {
            if (!(v is LabelView))
                argIndexes.Add(i);

            if (v.Parent != null)
            {
                ((ContentView)v.Parent).Detach(v);
            }
            Attach(v);
            parts.Add(v.Assemble());
            Reassemble();
            Changed(this);
        }

        public void RemoveSubView(int index)
        {
            IBlockView v = argViews[index];
            Detach(v);
            argViews.RemoveAt(index);
            parts.RemoveAt(index);
            Reassemble();
            Changed(this);
        }

        private void Attach(IBlockView v)
        {
            v.Changed += new ViewChangedEvent(ArgView_Changed);
            v.Parent = this;
        }

        private void Detach(IBlockView v)
        {
            if (!(v.Parent == this))
            {
                throw new InvalidOperationException("How did the parent of my arg not be me??");
            }
            v.Parent = null;
            v.Changed -= ArgView_Changed;
        }

        public IBlock Model 
        {
            get
            {
                return Parent.Model;
            }
        }

        internal void NotifyChanged()
        {
            Changed(this);
        }

        void ArgView_Changed(object source)
        {
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
            // Why do we recompute the parts array instead of just using it?
            // I remember this was to fix a bug that occurred when just using 'parts'
            // need to remember & document the exact reason
            this.parts.Clear();
            this.parts.AddRange(argViews.Select(v => v.Assemble()));

            int firstTextWidth = Math.Max(parts[0].Width, abc.MinTextWidth);
            int bmpWidth = abc.TextStart.X
                + firstTextWidth
                + abc.TextArgDist
                + parts.After(1).Sum(b => b.Width)
                + 3;
                

            bmpWidth = Math.Max(bmpWidth, abc.MinWidth);

            int height = parts.Max(b => b.Height);
            height = Math.Max(height, abc.WMid.Height);

            int width = bmpWidth - (abc.NW.Width + abc.NE.Width);
            int bmpHeight = height + abc.NW.Height + abc.SW.Height;

            Bitmap ret = new Bitmap(bmpWidth, bmpHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            _cached = ret;

  
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.Clear(Color.Transparent);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                abc.RenderToFit(g, width, height);
                Point p = abc.TextStart;
                int yC = (height - this.parts[0].Height) / 2;
                g.DrawImageUnscaled(parts[0], p.Offseted(0, yC));
                argViews[0].RelativePos = p.Offseted(0, yC);

                p.Offset(firstTextWidth + abc.TextArgDist, 0);

                for (int i = 1; i < parts.Count; ++i)
                {
                    yC = (height - this.parts[i].Height) / 2;
                    g.DrawImageUnscaled(this.parts[i], p.Offseted(0, yC));
                    Point relativePos = p.Offseted(0, yC);
                    argViews[i].RelativePos = relativePos;
                    
                    p.Offset(this.parts[i].Width, 0);
                }
            }
            _width = bmpWidth;
            _height = bmpHeight;
            fresh = true;

        }

        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            foreach (DropRegion dr in ChildDropRegions(origin))
                yield return dr;

            int a = 0;
            
            for(int i=0; i<argViews.Count; ++i)
            {
                IBlockView v = argViews[i];
                if (!trueArgs[i])
                {
                    continue;
                }
                Point rp = v.RelativePos;
                yield return new DropRegion(DropType.AsArgument, new Rectangle(rp, parts[i].Size).Offseted(origin.X, origin.Y), this, a, ArgTypes[a]);
                a++;
            }
        }

        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            int i = 0;
            foreach (IBlockView v in argViews)
            {
                Point rp = v.RelativePos;
                foreach (DropRegion dr in v.DropRegions(rp))
                    yield return new DropRegion(dr.DropType, dr.Rectangle.Offseted(origin.X, origin.Y), dr.Destination, dr.ExtraInfo, dr.ArgType);
                i++;
            }
        }

        public int Width
        {
            get
            {
                if (!fresh)
                    throw new InvalidOperationException("Cannot call ContentView.Width when control is out of date");
                return _width;
            }
        }

        public int Height
        {
            get
            {
                if (!fresh)
                    throw new InvalidOperationException("Cannot call ContentView.Height when control is out of date");
                return _height;
            }
        }
        public Size Size { get { return _cached.Size; } }

        public static Bitmap[] TextBitmaps(string[] text, Graphics textMetrics, Font textFont)
        {
            return text.Select(t => BitmapFromText(t, textMetrics, textFont)).ToArray();
        }

        public static Bitmap BitmapFromText(string text, Graphics textMetrics, Font textFont)
        {
            SizeF s = textMetrics.MeasureString(text, textFont);
            Bitmap b = new Bitmap((int) s.Width, (int) s.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Color.Transparent);
                g.DrawString(text, textFont, Brushes.Beige, 0, 0);
            }
            return b;

        }
        
        public bool HasPoint(Point p, Point origin)
        {
            int x = p.X - origin.X;
            int y = p.Y - origin.Y;
            if (x < 0 || x >= _cached.Width || y < 0 || y >= _cached.Height)
                return false;
            return _cached.GetPixel(x, y).ToArgb() != Color.Transparent.ToArgb();
        }

        public IBlockView ChildHasPoint(Point p, Point origin)
        {
            for (int i = 0; i < argViews.Count; ++i)
            {
                IBlockView v = argViews[i];
                Point rp = v.RelativePos;
                if (!trueArgs[i])
                    continue;
                if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y)))
                    return v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
            }
            return this;
        }
    }
}
