using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class Nine
    {
        public Bitmap NW, NE, SE, SW, NMid, SMid, WMid, EMid, Center; // 9 !!
        public Bitmap[][] asArray;
        public virtual void FromBitmapCutting(Bitmap fullBmp, Color mark)
        {

            Bitmap[][] bmps = fullBmp.SplitGrid(Color.Red);

            this.asArray = bmps;
            this.NW = bmps[0][0];
            this.NE = bmps[0][2];
            this.SW = bmps[2][0];
            this.SE = bmps[2][2];
            this.NMid = bmps[0][1];
            this.SMid = bmps[2][1];
            this.WMid = bmps[1][0];
            this.EMid = bmps[1][2];
            this.Center = bmps[1][1];
        }

        public void TestRender(Graphics g, Point p, Font f)
        {
            int x=0, y=0;
            for (int i = 0; i < asArray.Length; ++i)
            {
                x = 0;
                for (int j = 0; j < asArray[i].Length; ++j)
                {
                    
                    g.DrawImageUnscaled(asArray[i][j], p.Offseted(x,y));
                    //g.DrawString(string.Format("[{0}][{1}]", i, j), f, Brushes.Black, p.Offseted(x, y));
                    x += asArray[i][j].Width + 5;
                }
                y += asArray[i][0].Height + 5;                
            }
        }

        public virtual void RenderToFit(Graphics g, int middleWidth, int middleHeight)
        {
            /*
               +----+------+-----+
               |    |      |     |
               |----+      +---- |
               |                 |
               |----+      +-----|
               |    |      |     |
               +----+------+-----+
            */
            Rectangle r1 = new Rectangle(this.NW.Width, 0, middleWidth, this.NW.Height);
            g.DrawImage(this.NMid, r1);

            Rectangle r2 = new Rectangle(this.SW.Width, this.NW.Height + middleHeight, middleWidth, this.SW.Height);
            g.DrawImage(this.SMid, r2);

            Rectangle r3 = new Rectangle(0, this.NW.Height, this.NW.Width, middleHeight);
            g.DrawImage(this.WMid, r3);

            Rectangle r4 = new Rectangle(this.NW.Width + middleWidth, this.NW.Height, this.NE.Width, middleHeight);
            g.DrawImage(this.EMid, r4);

            Rectangle r5 = new Rectangle(this.NW.Width, this.NW.Height, middleWidth, middleHeight);
            g.DrawImage(this.Center, r5);

            //g.DrawRectangle(Pens.Yellow, r1);
            // g.DrawRectangle(Pens.Green, r2);
            // g.DrawRectangle(Pens.Brown, r3);
            //g.DrawRectangle(Pens.Fuchsia, r4);
            // g.DrawRectangle(Pens.Pink, r5);

            g.DrawImageUnscaled(this.NW, 0, 0);
            //g.DrawRectangle(Pens.Yellow, 0, 0, this.NW.Width, this.NW.Height);

            g.DrawImageUnscaled(this.NE, this.NW.Width + middleWidth, 0);
            //g.DrawRectangle(Pens.Yellow, this.NW.Width + middleWidth, 0, this.NE.Width, this.NE.Height);

            g.DrawImageUnscaled(this.SW, 0, this.NW.Height + middleHeight);
            //g.DrawRectangle(Pens.Yellow, 0, abc.NW.Height + height, abc.SW.Width, abc.SW.Height);

            g.DrawImageUnscaled(this.SE, this.NW.Width + middleWidth, this.NW.Height + middleHeight);
            //g.DrawRectangle(Pens.Yellow, abc.NW.Width + width, abc.NW.Height + height, abc.SE.Width, abc.SE.Height);

        }

        public int MinWidth { get { return NW.Width + NMid.Width + NE.Width; } }
        public int MinHeight { get { return NW.Height + WMid.Height + SW.Height; } }
    }

    public class NineContent : Nine
    {
        public Point TextStart;
        public int MinTextWidth;
        public int TextArgDist;

        public override void FromBitmapCutting(Bitmap fullBmp, Color mark)
        {
            int xText, yText;
            yText = fullBmp.ScanCol(0, Color.Red);
            fullBmp = fullBmp.SliceX(1);

            xText = fullBmp.ScanRow(0, Color.Red);
            int x2 = fullBmp.ScanRow(0, xText + 1, Color.Red);
            int x3 = fullBmp.ScanRow(0, x2 + 1, Color.Red);
            fullBmp = fullBmp.SliceY(1);

            this.TextStart = new Point(xText, yText);       
            this.MinTextWidth = x2 - this.TextStart.X;
            this.TextArgDist = x3 - x2;

            base.FromBitmapCutting(fullBmp, mark);
            
        }
    }

    public class StackBlockImgParts : NineContent
    {
        public static StackBlockImgParts FromBitmap(Bitmap fullBmp)
        {
            StackBlockImgParts ret = new StackBlockImgParts();
            fullBmp.MakeTransparent(fullBmp.GetPixel(0, 0));
            ret.FromBitmapCutting(fullBmp, Color.Red);
            return ret;
        }
    }
    
    public class ABC
    {
        public Bitmap A, B, C;
        public void FromBitmapCutting(Bitmap fullBmp, Color mark)
        {
            Bitmap[] pieces = fullBmp.SplitHorizontaByRow0(Color.Red);
            A = pieces[0];
            B = pieces[1];
            C = pieces[2];
        }
        public void TestRender(Graphics g, Point p, Font f)
        {
            g.DrawImageUnscaled(A, p);
            g.DrawImageUnscaled(B, p.Offseted(A.Width+5, 0));
            g.DrawImageUnscaled(C, p.Offseted(A.Width + B.Width + 10, 0));
        }
    }

    public class A1A2A3
    {
        public Bitmap A1, A2, A3;
        public int MinHeight;
        public void FromBitmapCutting(Bitmap fullBmp, Color mark)
        {
            Bitmap[] pieces = fullBmp.SplitVerticalByCol0(Color.Red);
            A1 = pieces[0];
            A2 = pieces[1];
            A3 = pieces[2];
            MinHeight = fullBmp.Height;
        }
        public void TestRender(Graphics g, Point p, Font f)
        {
            g.DrawImageUnscaled(A1, p);
            g.DrawImageUnscaled(A2, p.Offseted(0, A1.Height + 5));
            g.DrawImageUnscaled(A3, p.Offseted(0, A1.Height + A2.Height+ 10));
        }
    }

    public class CBlockImgParts
    {
        public NineContent Head;
        public A1A2A3 Side;
        public NineContent Waist;
        public ABC FootCap;
        public ABC FootStack;


        public void FromBitmapCutting(Bitmap fullBmp, Color mark)
        {
            fullBmp.MakeTransparent(fullBmp.GetPixel(0, 0));
            Bitmap[] vertPieces = fullBmp.SplitVerticalByCol0(mark);
            vertPieces = vertPieces.Select(b => b.TrimTrailingRight(Color.Transparent)).ToArray();

            /*
            for (int i = 0; i < vertPieces.Length; ++i)
                vertPieces[i].Save("vertPiece" + i + ".png");
             */

            Head = new NineContent();
            Head.FromBitmapCutting(vertPieces[0], mark);

            Side = new A1A2A3();
            Side.FromBitmapCutting(vertPieces[1].SliceX(1).SplitHorizontaByRow0(Color.Red)[0], mark);
            
            Waist = new NineContent();
            
            Waist.FromBitmapCutting(vertPieces[2], mark);

            FootCap = new ABC();
            FootCap.FromBitmapCutting(vertPieces[3].SliceX(2), mark);

            FootStack = new ABC();
            FootStack.FromBitmapCutting(vertPieces[4].SliceX(2), mark);
        }

        public void TestRender(Graphics g, Point p, Font f)
        {
            Head.TestRender(g, p, f);
            Side.TestRender(g, p.Offseted(0,100), f);
            Waist.TestRender(g, p.Offseted(0, 150), f);
            FootCap.TestRender(g, p.Offseted(0, 200), f);
            FootStack.TestRender(g, p.Offseted(0, 250), f);
        }
    }
    public class InputImageParts
    {
        public Bitmap InputNumber;
        public static InputImageParts FromBitmap(Bitmap fullBmp)
        {
            InputImageParts ret = new InputImageParts();
            Bitmap[] elements = fullBmp.SplitVerticalByCol0(Color.Red);
            ret.InputNumber = elements[0];
            return ret;
        }
    }
}
