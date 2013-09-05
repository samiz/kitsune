using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class CanvasView
    {
        Bitmap canvas;
        //Bitmap background;
        Brush backgroundBrush;
        Graphics graphics;
        Font textFont;
        Dictionary<IBlockView, Point> subViews;
        List<DropRegion> dropRegions;
        
        DropRegion CurrentDropRegion;
        bool activeDropRegion = false;
        // long lastRefreshTimeStamp = 0;
        // long ticks = 0;
        public string status = "";
        private Palette palette;
        public IBlockView Marked { get; set; }

        public CanvasView(Graphics graphics, Size canvasSize, 
            Dictionary<IBlockView, Point> subViews, List<DropRegion> dropRegions,
            Font textFont, Palette palette)
        {
            this.graphics = graphics;
            this.subViews = subViews;
            this.dropRegions = dropRegions;
            this.canvas = new Bitmap(canvasSize.Width, canvasSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            this.textFont = textFont;
            Bitmap bg = BitmapExtensions.LoadBmp("bg.bmp");
            Bitmap bg2 = new Bitmap(bg.Width, bg.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bg2))
            {
                g.DrawImageUnscaled(bg, 0, 0);
            }
            bg.Dispose();
            this.backgroundBrush = new TextureBrush(bg2);
            this.palette = palette;
            palette.Location = new Point(10, 10);
            // InitBackGround(canvasSize);
        }
        internal void Resize(Size canvasSize, Graphics g)
        {
            this.graphics.Dispose();
            this.graphics = g;
            this.canvas = new Bitmap(canvasSize.Width, canvasSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        }
        public void SetDropRegion(DropRegion dr)
        {
            CurrentDropRegion = dr;
            activeDropRegion = true;
        }

        public void ResetDropRegion()
        {
            activeDropRegion = false;
            Update(CurrentDropRegion.Rectangle);
        }
        private void InitBackGround(Size canvasSize)
        {
            /*
            this.background = new Bitmap(canvasSize.Width, canvasSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Random r = new Random();
            using (Graphics g = Graphics.FromImage(background))
            {
                g.Clear(Color.Beige);
                for (int i = 0; i < 10; ++i)
                {
                    int cx = r.Next(background.Width);
                    int cy = r.Next(background.Height);
                    int rd = 50 + r.Next(350);
                    Color clr = BitmapExtensions.RandomPastel(r);
                    g.FillEllipse(new SolidBrush(clr), cx - rd, cy - rd, rd * 2, rd * 2);
                }
            }
             */
        }

        public void Redraw(Graphics g, Rectangle invalidated)
        {
            g.DrawImage(canvas, invalidated, invalidated, GraphicsUnit.Pixel);
        }

        public void Update(Rectangle invalidated)
        {
            
            /*
            const long MinRefreshDelay = 5;
            long t = System.DateTime.Now.Ticks;
            ticks += t - lastRefreshTimeStamp;
            lastRefreshTimeStamp = t;
            if (ticks > MinRefreshDelay)
            {
                //UpdateGraphics(invalidated);
                UpdateGraphics(new Rectangle(0,0, canvas.Width, canvas.Height));
                ticks = 0;
            }
            //*/
            UpdateGraphics(invalidated);
        }

        private void UpdateGraphics(Rectangle invalidated)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                //g.DrawImage(background, invalidated, invalidated, GraphicsUnit.Pixel);
                g.FillRectangle(backgroundBrush, invalidated);

                if (PaletteRect.IntersectsWith(invalidated))
                {
                    g.DrawImageUnscaled(palette.Bitmap, PaletteRect.Location);
                }
                foreach (KeyValuePair<IBlockView, Point> kv in subViews)
                {
                    Bitmap b = kv.Key.Assemble();
                    Point pos = kv.Value;
                    Rectangle bounds = new Rectangle(pos, b.Size);
                    if (!bounds.IntersectsWith(invalidated))
                        continue;
                    g.DrawImageUnscaled(b, kv.Value);
                    if (kv.Key == Marked)
                        g.DrawRectangle(Pens.Red, new Rectangle(kv.Value, b.Size));

                    /*
                    foreach (DropRegion dr in kv.Key.DropRegions(kv.Value))
                    {
                        g.DrawRectangle(Pens.Magenta, dr.Rectangle);
                    }
                     //*/
                }
                if (activeDropRegion)
                {
                    if (CurrentDropRegion.DropType == DropType.AsArgument)
                    {
                        Rectangle r2 = CurrentDropRegion.Rectangle;
                        r2.Inflate(4, 4);
                        //g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Silver)), r2);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.BlanchedAlmond)), r2);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.BlanchedAlmond, CurrentDropRegion.Rectangle);
                    }
                }
                /*
                foreach (DropRegion dr in dropRegions)
                {
                    g.FillRectangle(Brushes.Bisque, dr.Rectangle);
                }
                 //*/
                //g.DrawString(status, textFont, Brushes.Black, 5, 600);
            }
            //ScheduleRedraw();
            Redraw(graphics, invalidated);
        }

        public bool ActiveDropRegion { get { return activeDropRegion; } }

        public DropRegion DropRegion { get { return CurrentDropRegion; } }


        public Rectangle PaletteRect 
        {
            get
            {
                return new Rectangle(palette.Location, palette.Size);
            }
        }
    }
}
