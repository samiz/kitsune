using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class InvokationBlockView : IInvokationBlockView
    {
        public event ViewChangedEvent Changed;
        ContentView content;

        private InvokationBlock model;
        public BlockAttributes attribute;
        public IBlockView Parent { get; set; }
        public Point RelativePos { get; set; }

        public InvokationBlockView(InvokationBlock model, BlockAttributes attribute, ContentView content)
        {
            this.model = model;
            this.content = content;
            this.attribute = attribute;

            Changed += delegate(object sender) { };
            content.Changed += new ViewChangedEvent(content_Changed);
            content.Parent = this;
            content.RelativePos = new Point(0, 0);

            Reassemble();
        }
        public BlockAttributes Attribute { get { return attribute; } }
        public IBlock Model { get { return model; } }

        void content_Changed(object source)
        {
            Changed(this);
        }

        public void SetArgView(int i, IBlockView v)
        {
            content.SetArgView(i, v);
        }

        internal void AddArgView(IBlockView v, DataType type)
        {
            content.AddSubView(v, type);
        }

        public void AddLabelView(LabelView v)
        {
            content.AddSubView(v, DataType.Invalid);
        }

        public Bitmap Assemble()
        {
            return content.Assemble();
        }

        public void Reassemble()
        {
            content.Reassemble();
        }

        public int Width { get { return content.Width;  } }

        public int Height { get { return content.Height; } }


        public BlockAttributes EffectiveAttribute()
        {
            return attribute;
        }

        public IEnumerable<DropRegion> DropRegions(Point origin)
        {
            foreach (DropRegion dr in this.EdgeDropRegions(origin, content.Size))
                yield return dr;

            foreach(DropRegion r in content.DropRegions(origin))
                yield return r;
        }

        public IEnumerable<DropRegion> ChildDropRegions(Point origin)
        {
            /*
              foreach (DropRegion dr in content.ChildDropRegions(origin))
                yield return dr;
             //*/
            foreach (DropRegion dr in content.DropRegions(origin))
                yield return dr;
        }


        public bool HasPoint(Point p, Point origin)
        {
            return content.HasPoint(p, origin);
        }

        public IBlockView ChildHasPoint(Point p, Point origin)
        {
            IBlockView c = content.ChildHasPoint(p, origin);
            if (c != content)
                return c;
            else
                return this;
        }
    }
}
