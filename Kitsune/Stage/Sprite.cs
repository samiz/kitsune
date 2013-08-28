using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public class Sprite
    {
        Bitmap bitmap;
        Bitmap sourceBitmap;
        float direction;
        public bool Visible;
        
        public Sprite(Bitmap b, Point location, bool transparent)
            : this(b, location, transparent, 0.0f)
        {
        }
    
        public Sprite(Bitmap b, Point location, bool transparent, float initialDirection)
        {
            sourceBitmap  = b;
            if(transparent)
                b.Transparent();
            int size = Math.Max(b.Width, b.Height);
            bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawImageUnscaled(sourceBitmap, (bitmap.Width - b.Width) / 2,
                    (bitmap.Height- b.Height) / 2);
            }
            
            Move(location);
            direction = initialDirection;
            UpdateBitmapDirection();
            Visible = true;
        }

        internal void Reset(Bitmap b)
        {
            sourceBitmap = b;
            int size = Math.Max(b.Width, b.Height);
            bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawImageUnscaled(sourceBitmap, (bitmap.Width - b.Width) / 2,
                    (bitmap.Height - b.Height) / 2);
            }
            UpdateBitmapDirection();
        }
        internal void Reset(Bitmap b, Point p)
        {
            Reset(b);
            Move(p);
        }

        public void Move(PointF newLocation)
        {
            Location = newLocation;
        }

        public void Move(float x, float y)
        {
            Move(new PointF(x, y));
        }

        public void Offset(float dx, float dy)
        {
            Move(Location.X + dx, Location.Y + dy);
        }

        
        internal void DrawOn(System.Drawing.Graphics g)
        {
            g.DrawImageUnscaled(bitmap, Bounds.Location);
        }

        public void fd(double steps)
        {
            PointF location = Location;
            double directionRad = (direction + 90) * Math.PI / 180;
            double newX = location.X + steps * Math.Cos(directionRad);
            double newY = location.Y - steps * Math.Sin(directionRad);
            Move((float)newX, (float)newY);
        }

        public void rt(float angle)
        {
            direction -= angle;
            UpdateBitmapDirection();
        }

        public void lt(float angle)
        {
            direction += angle;
            UpdateBitmapDirection();
        }

        private void UpdateBitmapDirection()
        {
            int size = bitmap.Size.Width; // width assumed == height
            int half = size / 2;
            
            using(Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.TranslateTransform(half, half);
                g.RotateTransform(-direction);
                g.TranslateTransform(-half, -half);
                g.DrawImageUnscaled(sourceBitmap, 0 , 0);
            }
        }

        public PointF Location { get; set;}
        public Rectangle Bounds
        {
            get
            {
                float x = Location.X - bitmap.Width / 2;
                float y = Location.Y - bitmap.Height / 2;
                Point p = new Point((int)x, (int)y);
                return new Rectangle(p, bitmap.Size);
            }
        }
    }
}
