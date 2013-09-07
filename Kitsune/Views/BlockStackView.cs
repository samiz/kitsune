using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class BlockStackView : IStackableBlockView
    {
        public event ViewChangedEvent Changed;
        BlockStack model;
        List<IBlockView> elements = new List<IBlockView>();
        Bitmap[] elementBitmaps;
        public const int NotchHeight = 4;
        Bitmap _cached;

        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }

        public BlockStackView(BlockStack model, IEnumerable<IBlockView> elements)
        {
            this.model = model;
            this.model.OnSplit += new BlockStackSplitEvent(model_OnSplit);
            this.elements.AddRange(elements);
            
            Changed += delegate(object sender) { };
            foreach (IBlockView v in this.elements)
            {
                Attach(v);
            }
            Reassemble();
        }

        private void Attach(IBlockView v)
        {
            v.Parent = this;
            v.Changed += new ViewChangedEvent(subView_Changed);
        }

        public IBlock Model { get { return model; } }

        void subView_Changed(object source)
        {
            Reassemble();
            Changed(this);
        }

        void model_OnSplit(int nLeft)
        {
            for (int i = nLeft; i < elements.Count; ++i)
            {
                elements[i].Changed -= subView_Changed;
                elements[i].Parent = null;
            }
            elements.RemoveRange(nLeft, elements.Count - nLeft);
            Reassemble();
            Changed(this);
        }

        internal void InsertView(int i, IBlockView v)
        {
            if (v.Parent != null)
            {
                // We don't want to deal here with deataching v from its parent
                // it should be already detached
                throw new InvalidOperationException();
            }
            Attach(v);
            elements.Insert(i, v);
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
            if (elements.Count == 0)
            {
                _cached = new Bitmap(1, 1);
                _cached.SetPixel(0, 0, Color.Transparent);
                return;
            }

            elementBitmaps = elements.Select(e => e.Assemble()).ToArray();
            int width = elementBitmaps.Max(e => e.Width);
            int height = elementBitmaps.Sum(e => e.Height) - (elements.Count - 1) * BlockStackView.NotchHeight;
            _cached = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(_cached))
            {
                g.FastSettings();
                g.Clear(Color.Transparent);
                int y = 0;
                for (int i = 0; i < elements.Count; ++i)
                {
                    g.DrawImageUnscaled(elementBitmaps[i], 0, y);
                    elements[i].RelativePos = new Point(0, y);
                    y += elementBitmaps[i].Height - BlockStackView.NotchHeight;
                }
            }
        }
        public BlockAttributes EffectiveAttribute()
        {
            if (elements.Count == 0)
            {
                return BlockAttributes.Stack;
            }
            IInvokationBlockView first = (IInvokationBlockView)elements[0];
            IInvokationBlockView last = (IInvokationBlockView)elements.Last();

            bool connectAbove = first.Attribute != BlockAttributes.Hat;
            bool connectBelow = last.Attribute != BlockAttributes.Cap;
            if (connectAbove && connectBelow)
                return BlockAttributes.Stack;
            else if (connectBelow)
                return BlockAttributes.Hat;
            else if (connectAbove)
                return BlockAttributes.Cap;
            else
                throw new InvalidOperationException("What's a reporter block doing in a BlockStack?");
        }
        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            if (elements.Count == 0)
            {
                yield break;
            }
            else
            {
                bool connectAbove = false, connectBelow = false;
                IStackableBlockView first = (IStackableBlockView)elements[0];
                IStackableBlockView last = (IStackableBlockView)elements.Last();

                connectAbove = first.EffectiveAttribute()!= BlockAttributes.Hat;
                connectBelow = last.EffectiveAttribute()!= BlockAttributes.Cap;

                if(connectAbove)
                    yield return new DropRegion(DropType.Above, new Rectangle(origin.Offseted(0, -2), new Size(_cached.Width, 5)), this);

                int y = origin.Y + elementBitmaps[0].Height - BlockStackView.NotchHeight;
                for (int i = 1; i < elements.Count; ++i)
                {
                    yield return new DropRegion(DropType.Between,
                        new Rectangle(origin.X, y - 2, elementBitmaps[i].Width, 5),
                        this,
                        i);

                    y += elementBitmaps[i].Height - BlockStackView.NotchHeight;
                }

                if(connectBelow)
                    yield return new DropRegion(DropType.Below, new Rectangle(origin.Offseted(0, _cached.Height - 2), new Size(_cached.Width, 5)), this);

                foreach (DropRegion r in ChildDropRegions(origin))
                    yield return r;
            }
        }
        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            int y = 0;
            for (int i = 0; i < elements.Count; ++i)
            {
                foreach (DropRegion dr in elements[i].ChildDropRegions(origin.Offseted(0, y)))
                {
                    yield return dr;
                }
                y += elementBitmaps[i].Height - BlockStackView.NotchHeight;
            }
        }

        public bool HasPoint(Point p, Point origin)
        {
            if (_cached == null)
                return false;
            int x = p.X - origin.X;
            int y = p.Y - origin.Y;
            if (x < 0 || x >= _cached.Width || y < 0 || y >= _cached.Height)
                return false;
            return _cached.GetPixel(x, y).ToArgb() != Color.Transparent.ToArgb();
        }

        public IBlockView ChildHasPoint(Point p, Point origin)
        {
            int y = 0;

            for (int i = 0; i < elements.Count; ++i)
            {
                IBlockView v = elements[i];
                Point rp = new Point(0, y);

                if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y)))
                {
                    IBlockView c = v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
                    if (i == 0 && c == v)
                    {
                        // selecting the first block-> select whole stack
                        // but selecting a proper child of the first block should
                        // go normally
                        return this;
                    }
                    else
                    {
                        return c;
                    }
                }
                y += elementBitmaps[i].Height - BlockStackView.NotchHeight;
            }
            return this;
        }
    }
}
