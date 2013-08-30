using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class ProcDefView : IBlockView
    {
        public event ViewChangedEvent Changed;
        ProcDefBlock _model;

        ContentView surroundingContent;
        ContentView invokationContent;
        IStackableBlockView body;

        Bitmap _cached;

        public ProcDefView(ProcDefBlock model, ContentView surroundingContent,
        ContentView invokationContent, IStackableBlockView body)
        {
            this._model = model;
            this.surroundingContent = surroundingContent;
            this.invokationContent = invokationContent;

            Changed += delegate(object sender) { };
            this.SetBody(body);
                        
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

        void body_Changed(object source)
        {
            throw new NotImplementedException();
        }

        public void AddFormalBit(IBlockView bit, DataType type)
        {
            invokationContent.AddSubView(bit, type);
        }

        public void RemoveFormalBit(int index)
        {
            invokationContent.RemoveSubView(index);
        }

        public void SetFormalBit(int index, IBlockView v)
        {
            invokationContent.SetSubView(index, v);
        }

        public void SetBody(IStackableBlockView body)
        {
            // assume 'body' is already detached from its parent
            // since we have no clean way to detach it from here
            // (this is the same as block stacks or args really, 
            // they are made top-level before being attached to something else)

            // ...howerver we need to detach the old body
            IStackableBlockView oldBody = this.body;
            DetachBody(oldBody);

            AttachBody(body);
            this.body = body;
            Reassemble();
            Changed(this);

        }

        private void AttachBody(IStackableBlockView v)
        {
            v.Changed += new ViewChangedEvent(body_Changed);
            v.Parent = this;
        }

        private void DetachBody(IStackableBlockView v)
        {
            if (v == null)
                return;
            if (!(v.Parent == this))
            {
                throw new InvalidOperationException("How did the parent of my body not be me??");
            }
            v.Parent = null;
            v.Changed -= body_Changed;
        }

        public Bitmap Assemble()
        {
            if (_cached == null)
                Reassemble();

            return _cached;
        }

        public void Reassemble()
        {
            Bitmap contentBmp = surroundingContent.Assemble();
            Bitmap bodyBmp = body.Assemble();
            int width = Math.Max(contentBmp.Width, bodyBmp.Width);
            int height = contentBmp.Height + bodyBmp.Height - BlockStackView.NotchHeight;
            _cached = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(_cached))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                
                g.DrawImageUnscaled(contentBmp, 0, 0);
                g.DrawImageUnscaled(bodyBmp, 0, contentBmp.Height - BlockStackView.NotchHeight);
            }
        }

        public IBlock Model { get {return _model; } }

        public IBlockView Parent { get; set; }

        public Point RelativePos { get; set; }

        public IEnumerable<DropRegion> DropRegions(System.Drawing.Point origin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DropRegion> ChildDropRegions(System.Drawing.Point origin)
        {
            throw new NotImplementedException();
        }

        public bool HasPoint(System.Drawing.Point p, System.Drawing.Point origin)
        {
            int x = p.X - origin.X;
            int y = p.Y - origin.Y;
            if (x < 0 || x >= _cached.Width || y < 0 || y >= _cached.Height)
                return false;
            return _cached.GetPixel(x, y).ToArgb() != Color.Transparent.ToArgb();
        }

        public IBlockView ChildHasPoint(System.Drawing.Point p, System.Drawing.Point origin)
        {
            IBlockView v = invokationContent;
            Point rp = v.RelativePos;
            
            if (v.HasPoint(p, origin.Offseted(rp.X, rp.Y)))
                    return v.ChildHasPoint(p, origin.Offseted(rp.X, rp.Y));
            
            return this;
        }
    }
}
