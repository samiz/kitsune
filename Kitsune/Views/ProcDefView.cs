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

        ContentView content;
        IStackableBlockView body;

        Bitmap _cached;

        public ProcDefView(ProcDefBlock model, ContentView content)
        {
            this._model = model;
            this.content = content;

            Changed += delegate(object sender) { };
            content.Changed += new ViewChangedEvent(content_Changed);
            content.Parent = this;
            content.RelativePos = new Point(0, 0);

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
            content.AddSubView(bit, type);
        }

        public void RemoveFormalBit(int index)
        {
            content.RemoveSubView(index);
        }

        public void SetFormalBit(int index, IBlockView v)
        {
            content.SetSubView(index, v);
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
            Bitmap contentBmp = content.Assemble();
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
            throw new NotImplementedException();
        }

        public IBlockView ChildHasPoint(System.Drawing.Point p, System.Drawing.Point origin)
        {
            throw new NotImplementedException();
        }
    }
}
