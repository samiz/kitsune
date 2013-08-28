using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    
    public class CBlockView : IInvokationBlockView
    {
        public event ViewChangedEvent Changed;
        public List<ContentView> Contents = new List<ContentView>();
        public List<IStackableBlockView> Scripts = new List<IStackableBlockView>();
        public List<ArgumentPartition> ArgPartitions;
        List<Point> scriptOffsets = new List<Point>();
        List<int> scriptIndexes = new List<int>();
        List<int> layoutOffsets = new List<int>();
        CBlockImgParts parts;
        Bitmap _cached;
        private InvokationBlock model;
        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }
        BlockAttributes attribute;

        public CBlockView(InvokationBlock model,
            BlockAttributes attribute,
            List<ContentView> contents,
            IEnumerable<BlockStackView> scripts, 
            CBlockImgParts parts, 
            List<ArgumentPartition> argPartitions)
        {
            this.model = model;
            this.attribute = attribute;
            Contents.AddRange(contents);
            Scripts.AddRange(scripts);
            this.ArgPartitions = argPartitions;
            int si = 0;
            foreach (ArgumentPartition p in argPartitions)
            {
                if (p.Type == ArgViewType.Script)
                    scriptIndexes.Add(si);
                si += p.N;
            }
            this.parts = parts;
            Changed += delegate(object sender) { };

            foreach (ContentView cv in contents)
            {
                cv.Parent = this;
                cv.Changed += new ViewChangedEvent(subView_Changed);
            }
            foreach (BlockStackView sv in scripts)
            {
                sv.Parent = this;
                sv.Changed += new ViewChangedEvent(subView_Changed);
            }
            Reassemble();
        }

        public IBlock Model { get { return model; } }
        public BlockAttributes Attribute { get { return attribute; } }

        void subView_Changed(object source)
        {
            Reassemble();
            Changed(this);
        }

        public void SetArgView(int i, IBlockView v)
        {
            int iContent = 0, iScript = 0;
            ArgViewType resultViewType = ArgViewType.Unknown;
            for (int j = 0; j < ArgPartitions.Count; ++j)
            {
                ArgumentPartition p = ArgPartitions[j];
                if (p.Type == ArgViewType.Content)
                {
                    if (i < p.N)
                    {
                        resultViewType = ArgViewType.Content;
                        break;
                    }
                    else
                    {
                        i -= p.N;
                        iContent++;
                    }
                }
                else if (p.Type == ArgViewType.Script)
                {
                    if (i < p.N)
                    {
                        resultViewType = ArgViewType.Script;
                        break;
                    }
                    else
                    {
                        i -= p.N;
                        iScript++;
                    }
                }
            }
            // Now we can use resultViewType and i to determine the arg
            if (resultViewType == ArgViewType.Content)
            {
                Contents[iContent].SetArgView(i, v);
            }
            else if (resultViewType == ArgViewType.Script)
            {
                SetScriptView(iScript, (IStackableBlockView) v);
            }
            else
            {
                throw new InvalidOperationException("Error in view search algorithm in CBlockView.SetArgView(...)");
            }
        }

        private void SetScriptView(int i, IStackableBlockView v)
        {
            IBlockView oldView = Scripts[i];
            if (!(oldView.Parent == this))
            {
                throw new InvalidOperationException("How did the parent of my arg not be me??");
            }
            oldView.Parent = null;
            oldView.Changed -= subView_Changed;

            if (v.Parent != null)
            {
                v.Changed -= ((CBlockView)v.Parent).subView_Changed;
            }
            v.Changed += new ViewChangedEvent(subView_Changed);
            v.Parent = this;
            Scripts[i] = v;
            
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
            int width = 0, height = 0;
            int sideWidth;
            List<Bitmap> layoutBitmaps = new List<Bitmap>();
            List<Bitmap> scriptBitmaps = new List<Bitmap>();
            layoutOffsets.Clear();
            scriptOffsets.Clear();

            Bitmap cb = Contents[0].Assemble(); // parts.Head
            AddLayoutBitmap(layoutBitmaps, cb, ref width, ref height);

            
            Bitmap script = Scripts[0].Assemble();
            scriptBitmaps.Add(script);
            int sideHeight = Math.Max(parts.Side.MinHeight, script.Height - BlockStackView.NotchHeight *2);
            Bitmap side = AssembleA1A2A3(parts.Side, sideHeight);
            sideWidth = side.Width;
            scriptOffsets.Add(new Point(sideWidth, height - BlockStackView.NotchHeight));
            AddLayoutBitmap(layoutBitmaps, side, ref width, ref height);
            if (Scripts[0].HasBottomNotch())
            {
                height -= BlockStackView.NotchHeight;
            }
            width = Math.Max(width, script.Width + sideWidth);
            for (int i = 1; i < Contents.Count; ++i)
            {
                cb = Contents[i].Assemble(); // parts.Waist
                AddLayoutBitmap(layoutBitmaps, cb, ref width, ref height);
                
                script = Scripts[i].Assemble();
                scriptBitmaps.Add(script);
                sideHeight = Math.Max(parts.Side.MinHeight, script.Height - BlockStackView.NotchHeight* 2);
                if (i + 1 == Contents.Count)
                    sideHeight -= BlockStackView.NotchHeight;
                side = AssembleA1A2A3(parts.Side, sideHeight);
                scriptOffsets.Add(new Point(sideWidth, height - BlockStackView.NotchHeight));
                
                AddLayoutBitmap(layoutBitmaps, side, ref width, ref height);
                if (Scripts[i].HasBottomNotch())
                {
                    height -= BlockStackView.NotchHeight;
                }
                width = Math.Max(width, script.Width + sideWidth);
            }
            
            Bitmap bottom = AssembleCBlockFoot(width, 
                attribute == BlockAttributes.Cap? parts.FootCap : parts.FootStack);

            AddLayoutBitmap(layoutBitmaps, bottom, ref width, ref height);
            
            Bitmap ret = new Bitmap(width, height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            _cached = ret;
            
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.Transparent);
                int offSetIndex = 0;
                
                int index = 0;
                int y = 0;
                foreach (Bitmap b in layoutBitmaps)
                {
                    y = layoutOffsets[index];
                    g.DrawImageUnscaled(b, 0, y);
                    
                    if ((index % 2) == 0 && ((index / 2) < Contents.Count)) // it's content, not side
                        Contents[index / 2].RelativePos = new Point(0, y);
                    
                    index++;
                }
                index = 0;
                foreach (Bitmap b in scriptBitmaps)
                {
                    y = scriptOffsets[offSetIndex++].Y;
                    Scripts[index].RelativePos = new Point(sideWidth - 1, y);
                    g.DrawImageUnscaled(b, sideWidth-1, y);
                    index++;
                }
            }
        }

        public BlockAttributes EffectiveAttribute()
        {
            return Attribute;
        }

        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            foreach (DropRegion dr in this.EdgeDropRegions(origin, _cached.Size))
                yield return dr;
            foreach (DropRegion dr in ChildDropRegions(origin))
                yield return dr;
        }

        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            int i = 0;
            foreach (IBlockView c in Contents)
            {
                foreach (DropRegion dr in c.DropRegions(origin.Offseted(0, layoutOffsets[i*2])))
                    yield return dr;
                i++;
            }
            i = 0;
            foreach (IBlockView s in Scripts)
            {
                Point p = scriptOffsets[i];

                if ((s.Model is BlockStack) && ((BlockStack)s.Model).Empty)
                {
                    // It's just a placeholder, no 'real' BlockStackView here
                    // We assume here that each script is preceded by a content and that they strictly
                    // alternate, and thus we can set the width of the placeholder to be that of 
                    // the previous content (Contents[i].Width)
                    yield return new DropRegion(DropType.AsArgument,
                        new Rectangle(origin.Offseted(p), new Size(Contents[i].Width, 5)), 
                        this, scriptIndexes[i], DataType.Script);
                }
                else
                {
                    foreach (DropRegion dr in s.DropRegions(origin.Offseted(p.X, p.Y)))
                        yield return dr;
                }
                i++;
            }
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
            for (int i = 0; i < Contents.Count; ++i)
            {
                IBlockView v = Contents[i];
                Point rp = new Point(0, layoutOffsets[i*2]); // notice that contentOffsets also holds the offset
                if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y))) // of the "side" graphics, not just the content views
                {
                    IBlockView chp = v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
                    if (chp == v)
                        return this;
                    else
                        return chp;
                }
            }

            for (int i = 0; i < Scripts.Count; ++i)
            {
                IBlockView v = Scripts[i];
                Point rp = scriptOffsets[i];
                if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y)))
                    return v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
            }
            return this;
        }

        Bitmap AssembleA1A2A3(A1A2A3 a, int height)
        {
            height = Math.Max(a.MinHeight, height);
            Bitmap ret = new Bitmap(a.A1.Width, height);
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImageUnscaled(a.A1, 0, 0);
                g.DrawImage(a.A2, new Rectangle(0, a.A1.Height, a.A1.Width, height - (a.A1.Height+a.A3.Height)));
                g.DrawImageUnscaled(a.A3, 0, height - a.A3.Height);
            }
            return ret;            
        }
        private Bitmap AssembleCBlockFoot(int width, ABC abc)
        {
            Bitmap ret = new Bitmap(width, abc.A.Height);
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.Clear(Color.Transparent);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                g.DrawImageUnscaled(abc.A, 0, 0);

                Rectangle r = new Rectangle(abc.A.Width, 0, width - (abc.A.Width + abc.C.Width), abc.A.Height);
                g.DrawImage(abc.B, r);

                g.DrawImageUnscaled(abc.C, width - abc.C.Width, 0);
            }
            return ret;
        }

        void AddLayoutBitmap(List<Bitmap> layoutBitmaps, Bitmap cb, ref int width, ref int height)
        {
            width = Math.Max(width, cb.Width);
            layoutOffsets.Add(height);
            height += cb.Height;
            layoutBitmaps.Add(cb);
        }
    }
}
