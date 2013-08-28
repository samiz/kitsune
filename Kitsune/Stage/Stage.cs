using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune.Stage
{
    public class Stage
    {
        Size size;
        Bitmap background;
        public Bitmap Buffer;
        List<Sprite> sprites = new List<Sprite>();
        Graphics graphics;
        Font textFont;
        Dictionary<Sprite, Sprite> speechBalloons = new Dictionary<Sprite, Sprite>();

        public Stage(Graphics graphics, Font textFont, Size size)
        {
            this.graphics = graphics;
            this.textFont = textFont;
            this.size = size;
            background = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(background))
            {
                g.Clear(Color.White);
            }
            Buffer = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(background))
            {
                g.DrawImageUnscaled(background, 0, 0);
            }
        }
        public void AddSprite(Sprite s) { sprites.Add(s); Update(s.Bounds); }
        public void Update(Rectangle invalidated)
        {
            using (Graphics g = Graphics.FromImage(Buffer))
            {
                g.DrawImage(background, invalidated, invalidated, GraphicsUnit.Pixel);
                foreach (Sprite s in sprites)
                {
                    if (s.Bounds.IntersectsWith(invalidated))
                        s.DrawOn(g);
                }
            }
        }

        public void RedrawAll()
        {
            Redraw(new Rectangle(0,0,size.Width, size.Height));
        }
        public void Redraw(Rectangle invalidated)
        {
            Redraw(graphics, invalidated);
        }

        public void Redraw(Graphics g, Rectangle invalidated)
        {
            Update(invalidated);
            g.DrawImage(Buffer, invalidated, invalidated, GraphicsUnit.Pixel);
        }

        public void Fwd(Sprite s, double steps)
        {
            Point p1 = s.Location.ToPoint();
            Rectangle r1 = s.Bounds;
            s.fd(steps);
            Point p2 = s.Location.ToPoint();
            Rectangle r2 = s.Bounds;
            using (Graphics g = Graphics.FromImage(background))
            {
                g.DrawLine(Pens.Black, p1, p2);
            }
            Redraw(Rectangle.Union(r1, r2));
            //RedrawAll();
        }

        public Bitmap MakeSpeechBalloon(Sprite s, string text, out Point drawFrom)
        {
            Rectangle r1 = AreaRight(Bounds, s.Bounds);
            int margin = 2;
            Rectangle r2 = r1;
            r1.Inflate(- margin * 2, - margin * 2);
            Size textSize = graphics.MeasureString(text, textFont, r1.Size.ToSizeF()).ToSize();
            textSize = new Size(textSize.Width + 1, textSize.Height + 1);
            Bitmap ret = new Bitmap(textSize.Width + margin *2, textSize.Height+ margin *2, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Point head = new Point(ret.Width / 2, ret.Height - 1);
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.Clear(Color.Transparent);
                Point[] points = 
                {
                    head, head.Offseted(3,-2), head.Offseted(6, -2)
                };
                
                Rectangle r3 = new Rectangle(1, 1, ret.Width - 2, ret.Height - 4);
                g.FillRectangle(Brushes.White, r3);
                g.DrawRectangle(Pens.Black, r3);
                g.FillPolygon(Brushes.White, points);
                g.DrawPolygon(Pens.Black, points);
                g.DrawLine(Pens.White, points[1], points[2].Offseted(0,-1));
                g.DrawString(text, textFont, Brushes.Black, new Rectangle(new Point(margin, margin), textSize));
            }
            drawFrom = s.Bounds.TopRight().Offseted(2, -10);
            return ret;

        }

        internal void Say(Sprite s, string str)
        {
            Point p;
            Bitmap bubble = MakeSpeechBalloon(s, str, out p);
            Sprite bubbleSprite;
            if (!speechBalloons.ContainsKey(s))
            {
                bubbleSprite = new Sprite(bubble, p, false);
                speechBalloons[s] = bubbleSprite;
                sprites.Add(bubbleSprite);
            }
            else
            {
                bubbleSprite = speechBalloons[s];
                bubbleSprite.Reset(bubble, p);
            }
            //Redraw(bubbleSprite.Bounds);
            RedrawAll();
        }

        private Rectangle AreaAbove(Rectangle large, Rectangle small)
        {
            return new Rectangle(large.Left, large.Top, large.Width, small.Top - large.Top);
        }

        private Rectangle AreaBelow(Rectangle large, Rectangle small)
        {
            return new Rectangle(small.Bottom, large.Left, large.Width, large.Height - small.Bottom);
        }

        private Rectangle AreaLeft(Rectangle large, Rectangle small)
        {
            return new Rectangle(large.Left, large.Top, small.Left - large.Left, large.Height);
        }

        private Rectangle AreaRight(Rectangle large, Rectangle small)
        {
            return new Rectangle(small.Right, large.Top, large.Width - small.Right, large.Height);
        }

        public void WithSprite(Sprite s, Action<Sprite> func)
        {
            Rectangle r1 = s.Bounds;
            func(s);
            Rectangle r2 = s.Bounds;
            //Redraw(Rectangle.Union(r1, r2));
            Update(Rectangle.Union(r1, r2));
        }

        public Rectangle Bounds 
        {
            get { return new Rectangle(0, 0, size.Width, size.Height); }
        }

  
    }
}
