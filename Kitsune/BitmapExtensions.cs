using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Kitsune
{
    static class BitmapExtensions
    {
        public static Point Offseted(this Point p, Point p2)
        {
            return p.Offseted(p2.X, p2.Y);
        }
        public static Point ToPoint(this PointF p)
        {
            return new Point((int)p.X, (int)p.Y);
        }

        public static Point Offseted(this Point p, int dx, int dy)
        {
            Point p2 = p;
            p2.Offset(dx, dy);
            return p2;
        }

        public static Rectangle Offseted(this Rectangle r, int dx, int dy)
        {
            Rectangle r2 = r;
            r2.Offset(dx, dy);
            return r2;
        }

        public static Point Center(this Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Bottom / 2);
        }

        public static RectangleF ToRectangleF(this Rectangle r)
        {
            return new RectangleF(r.Left, r.Top, r.Width, r.Height);
        }

        public static Rectangle BottomSlice(this Rectangle r, int pixels)
        {
            int b = r.Bottom;
            return new Rectangle(r.Left, b - pixels, r.Width, pixels);
        }

        public static Rectangle TopSlice(this Rectangle r, int pixels)
        {
            int t = r.Top;
            return new Rectangle(r.Left, t, r.Width, pixels);
        }

        public static Point TopRight(this Rectangle r)
        {
            return new Point(r.Right, r.Top);
        }

        public static Point Minus(this Point p1, Point p2)
        {
            Point p3 = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return p3;
        }

        public static SizeF ToSizeF(this Size size)
        {
            return new SizeF(size.Width, size.Height);
        }
        public static Point ScanPix(this Bitmap bmp, int x, int y, Color clr)
        {
            for (int xx = x; xx < bmp.Width; ++xx)
            {
                for (int yy = y; yy < bmp.Height; ++yy)
                {
                    Color c = bmp.GetPixel(xx, yy);
                    if (c.ToArgb() == clr.ToArgb())
                    {
                        return new Point(xx, yy);
                    }
                }
            }
            throw new ArgumentException("ScanPix: must supply a bitmap with the required color", "bmp");
        }

        public static Bitmap LoadBmp(string p)
        {
            string imgRoot = Path.Combine(Application.StartupPath, "Assets");
            return (Bitmap)Bitmap.FromFile(Path.Combine(imgRoot, p));
        }

        public static Bitmap Transparent(this Bitmap bmp)
        {
            bmp.MakeTransparent(bmp.GetPixel(0, 0));
            return bmp;
        }

        public static int ScanRow0(this Bitmap bmp, Color clr)
        {
            return bmp.ScanRow(0, clr);
        }

        public static int ScanRow(this Bitmap bmp, int row, Color clr)
        {
            return bmp.ScanRow(row, 0, clr);
        }

        public static int ScanRow(this Bitmap bmp, int row, int xStart, Color clr)
        {
            for (int xx = xStart; xx < bmp.Width; ++xx)
            {
                Color c = bmp.GetPixel(xx, row);
                if (c.ToArgb() == clr.ToArgb())
                {
                    return xx;
                }
            }
            throw new ArgumentException("ScanRow0: must supply a bitmap with the required color", "bmp");
        }

        public static int ScanCol0(this Bitmap bmp, Color clr)
        {
            return bmp.ScanCol(0, clr);
        }
        public static int ScanCol(this Bitmap bmp, int col, Color clr)
        {
            return bmp.ScanCol(col, 0, clr);
        }
        public static int ScanCol(this Bitmap bmp, int col, int yStart, Color clr)
        {
            for (int yy = yStart; yy < bmp.Height; ++yy)
            {
                Color c = bmp.GetPixel(col, yy);
                if (c.ToArgb() == clr.ToArgb())
                {
                    return yy;
                }
            }
            throw new ArgumentException("ScanCol0: must supply a bitmap with the required color", "bmp");
        }

        public static Point ScanPix(this Bitmap bmp, int x, int y, Color clr, Color replace)
        {
            for (int xx = x; xx < bmp.Width; ++xx)
            {
                for (int yy = y; yy < bmp.Height; ++yy)
                {
                    Color c = bmp.GetPixel(xx, yy);
                    if (c.ToArgb() == clr.ToArgb())
                    {
                        bmp.SetPixel(xx, yy, replace);
                        return new Point(xx, yy);
                    }
                }
            }
            throw new ArgumentException("ScanPix: must supply a bitmap with the required color", "bmp");
        }

        public static Bitmap TrimTrailingRight(this Bitmap bmp, Color c)
        {
            int col = bmp.Width - 1;
            while (col >=0)
            {
                for (int i = 0; i < bmp.Height; ++i)
                {
                    if (!HackyEquals(bmp.GetPixel(col, i),c))
                        goto found;
                }
                col--;
            }
        found: 
            Bitmap ret = new Bitmap(col+1, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using (Graphics g = Graphics.FromImage(ret))
        {
            g.DrawImageUnscaled(bmp, 0, 0);
        }
        return ret;

        }

        private static bool HackyEquals(Color c1, Color c2)
        {
            return (c1.A == 0 && c2.A == 0) || (c1.ToArgb() == c2.ToArgb());
        }

        public static Bitmap[][] SplitGrid(this Bitmap bmp, Color pixelClr)
        {
            Bitmap[] rows = bmp.SplitVerticalByCol0(pixelClr);
            List<int> colSeps = bmp.ExtractRow0Separators(pixelClr);
            bmp = bmp.SliceY(1);
            Bitmap[][] ret = new Bitmap[rows.Length][];
            for (int i = 0; i < rows.Length; ++i)
            {
                ret[i] = rows[i].SplitXAt(colSeps);
            }
            return ret;
        }

        public static Bitmap[] SplitHorizontaByRow0(this Bitmap bmp, Color pixelClr)
        {
            List<int> parts = ExtractRow0Separators(bmp, pixelClr);
            return bmp.SplitXAt(parts);
        }

        private static List<int> ExtractRow0Separators(this Bitmap bmp, Color pixelClr)
        {
            List<int> parts = new List<int>();

            for (int i = 0; i < bmp.Width; ++i)
            {
                if (bmp.GetPixel(i, 0).ToArgb() == pixelClr.ToArgb())
                {
                    parts.Add(i);
                }
            }
            return parts;
        }

        public static Bitmap[] SplitVerticalByCol0(this Bitmap bmp, Color pixelClr)
        {
            List<int> parts = ExtractCol0Separators(bmp, pixelClr);
            return bmp.SplitYAt(parts);
        }

        private static List<int> ExtractCol0Separators(Bitmap bmp, Color pixelClr)
        {
            List<int> parts = new List<int>();

            for (int i = 0; i < bmp.Height; ++i)
            {
                if (bmp.GetPixel(0, i).ToArgb() == pixelClr.ToArgb())
                {
                    parts.Add(i);
                }
            }
            return parts;
        }
        public static Bitmap[] SplitXAt(this Bitmap bmp, List<int> parts)
        {
            List<int> myParts = new List<int>();

            myParts.Add(0);
            myParts.AddRange(parts);
            myParts.Add(bmp.Width);

            Bitmap[] ret = new Bitmap[myParts.Count - 1];
            Bitmap other = bmp.SliceY(1);
            for (int i = 0; i < myParts.Count - 1; ++i)
            {
                ret[i] = other.SliceX(myParts[i], myParts[i + 1]);
            }
            return ret;
        }

        public static Bitmap[] SplitYAt(this Bitmap bmp, List<int> parts)
        {
            List<int> myParts = new List<int>();

            myParts.Add(0);
            myParts.AddRange(parts);
            myParts.Add(bmp.Height);

            Bitmap [] ret = new Bitmap[myParts.Count - 1];
            Bitmap other = bmp.SliceX(1);
            for (int i = 0; i < myParts.Count - 1; ++i)
            {
                ret[i] = other.SliceY(myParts[i], myParts[i + 1]);
            }
            return ret;
        }

        public static Bitmap Slice(this Bitmap bmp, int x, int y, int w, int h)
        {
            Bitmap b2 = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(b2);
            g.DrawImage(bmp, new Rectangle(0, 0, w, h), new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
            g.Dispose();
            return b2;
        }
        public static Bitmap SliceX(this Bitmap bmp, int x1, int x2)
        {
            return bmp.Slice(x1, 0, x2-x1, bmp.Height);
        }

        public static Bitmap SliceX(this Bitmap bmp, int x1)
        {
            return bmp.SliceX(x1, bmp.Width);
        }

        
        public static Bitmap SliceY(this Bitmap bmp, int y1, int y2)
        {
            return bmp.Slice(0, y1, bmp.Width, y2-y1);
        }

        public static Bitmap SliceY(this Bitmap bmp, int y1)
        {
            return bmp.SliceY(y1, bmp.Height);
        }

        public static Bitmap StitchHorizontal(this Bitmap b1, Bitmap b2)
        {
            Bitmap ret = new Bitmap(b1.Width + b2.Width, Math.Max(b1.Height, b2.Height));
            using (Graphics g = Graphics.FromImage(ret))
            {
                g.Clear(Color.Transparent);
                g.DrawImageUnscaled(b1, 0, 0);
                g.DrawImageUnscaled(b2, b1.Width, 0);
            }
            return ret;
        }
        public static double Clamp(this double v, double m)
        {
            if (v > m)
                return m;
            return v;
        }

        public static Color RandomPastel(Random r)
        {
            Color ret = ColorFromHSV(r.NextDouble(), r.NextDouble() / 2, Clamp(r.NextDouble() * 2.0, 1.0));
            return ret;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }

    
}
