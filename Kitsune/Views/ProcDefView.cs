using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class ProcDefView : IStackableBlockView
    {
        public event ViewChangedEvent Changed;
        ProcDefBlock _model;

        ContentView surroundingContent;
        ContentView invokationContent;

        public ProcDefView(ProcDefBlock model, ContentView surroundingContent,
        ContentView invokationContent)
        {
            this._model = model;
            this.surroundingContent = surroundingContent;
            this.invokationContent = invokationContent;

            Changed += delegate(object sender) { };
                        
            surroundingContent.Changed += new ViewChangedEvent(content_Changed);
            surroundingContent.Parent = this;
            invokationContent.RelativePos = new Point(0, 0);

            Reassemble();
        }

        void content_Changed(object source)
        {
            Reassemble();
            Changed(this);
        }

        public void AddFormalBit(IBlockView bit, DataType type)
        {
            invokationContent.AddSubView(bit, type);
        }

        public void RemoveFormalBit(IBlockView bit)
        {
            invokationContent.RemoveSubView(bit);
        }
        public void RemoveFormalBit(int index)
        {
            invokationContent.RemoveSubView(index);
        }

        public void SetFormalBit(int index, IBlockView v)
        {
            invokationContent.SetSubView(index, v);
        }

        public Bitmap Assemble()
        {
            return surroundingContent.Assemble();
            /*
             if (_cached == null)
                Reassemble();

            return _cached;
             */
        }

        public void Reassemble()
        {
          //  surroundingContent.Reassemble();
            /*
            Bitmap contentBmp = surroundingContent.Assemble();
            Bitmap bodyBmp = body.Assemble();
            int width = Math.Max(contentBmp.Width, bodyBmp.Width);
            //int height = contentBmp.Height + bodyBmp.Height - BlockStackView.NotchHeight;
            int height = contentBmp.Height + bodyBmp.Height;
            _cached = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(_cached))
            {
                g.FastSettings();
                g.Clear(Color.Transparent);
                g.DrawImageUnscaled(contentBmp, 0, 0);
                //g.DrawImageUnscaled(bodyBmp, 0, contentBmp.Height - BlockStackView.NotchHeight);
            }
             */ 
        }

        public IBlock Model { get {return _model; } }

        public IBlockView Parent { get; set; }

        public Point RelativePos { get; set; }

        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            
                Rectangle r = new Rectangle(origin, this.Assemble().Size).BottomSlice(5);
                yield return new DropRegion(DropType.Below,
                    r,
                    this);
        }

        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            /*
              foreach (DropRegion dr in content.ChildDropRegions(origin))
                yield return dr;
             //*/
            yield break;

        }

        public bool HasPoint(System.Drawing.Point p, System.Drawing.Point origin)
        {
            /*
            int x = p.X - origin.X;
            int y = p.Y - origin.Y;
            if (x < 0 || x >= _cached.Width || y < 0 || y >= _cached.Height)
                return false;
            return _cached.GetPixel(x, y).ToArgb() != Color.Transparent.ToArgb();
             */
            return surroundingContent.HasPoint(p, origin);
        }

        public IBlockView ChildHasPoint(System.Drawing.Point p, System.Drawing.Point origin)
        {
            
            IBlockView v = invokationContent;
            Point rp = v.RelativePos;

            if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y)))
            {
                IBlockView cv = v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
                if (cv != v)
                    return cv;
                else
                    return this;
            }
            
            return this;
            
        }

        public BlockAttributes EffectiveAttribute()
        {
            return BlockAttributes.Hat;
        }
    }
}
