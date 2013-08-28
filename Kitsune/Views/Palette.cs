using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Kitsune
{
    public class ToolSpec
    {
        public Bitmap bmpFile;
        public Rectangle rectangle;
        public string funcName;
        public string category;
        public IBlock[] defaultArgs;
        public ToolSpec(Bitmap bmpFile,
                       string funcName,
                       string category,
            IBlock[] defaultArgs)
        {
            this.bmpFile = bmpFile;
            this.funcName = funcName;
            this.category = category;
            this.defaultArgs = defaultArgs;
        }
        
    }
    public delegate void PaletteModifiedEvent(object sender);
    public class Palette
    {
        public event PaletteModifiedEvent Modified;
        const int tabSpacing = 5;
        const int tabPadding = 2;
        Bitmap bitmap;
        Size toolSize;
        Size initialSize;
        List<ToolSpec> tools = new List<ToolSpec>();
        List<Rectangle> tabRects = new List<Rectangle>();
        List<string> tabTexts = new List<string>();
        IEnumerable<ToolPrototype> toolSpecs;
        HashSet<String> categories = new HashSet<string>();
        string filePrefix;
        Graphics textMetrics;
        Font textFont;
        private string currentCategory;
        public Palette(Size size, Graphics textMetrics, Font textFont)
        {
            initialSize = size;
            this.textMetrics = textMetrics;
            this.textFont = new Font(textFont.FontFamily, textFont.Size+2, textFont.Style);
        }
        internal void Resize(Size size, Graphics g)
        {
            initialSize = size;
            textMetrics = g;
        }
        public Bitmap Bitmap { get { return bitmap; } }
        public void Init(IEnumerable<ToolPrototype> toolSpecs, string filePrefix, string initialCategory)
        {
            this.currentCategory = initialCategory;
            this.toolSpecs = toolSpecs;
            this.filePrefix = filePrefix;
            tools.Clear();
            foreach (ToolPrototype spec in toolSpecs)
            {
                string[] parts = spec.tool.Split("|".ToCharArray());
                string fileName = Path.Combine(filePrefix, parts[0]) + ".bmp";
                string funcName = parts[1];
                Bitmap tbBmp = (Bitmap)Bitmap.FromFile(fileName);
                tbBmp.MakeTransparent(tbBmp.GetPixel(0, 0));
                tools.Add(new ToolSpec(tbBmp,funcName, spec.category, spec.defaultArgs));
                categories.Add(spec.category);
            }
        }
        public void LayoutTools(string category)
        {
            bool first = true;
            int x=0, y=0;
            
            int tabHeight = categories.Max(s => (int)textMetrics.MeasureString(s, textFont).Height) + tabPadding *2;
            int tabWidth = categories.Sum(s => (int)textMetrics.MeasureString(s, textFont).Width) 
                + (tabPadding*2 + tabSpacing ) * categories.Count;
            foreach (ToolSpec spec in tools)
            {
                if (!spec.category.Contains(category))
                    continue;
                if(first)
                {
                    toolSize = spec.bmpFile.Size;
                    first = false;
                }
                if(x + toolSize.Width >= initialSize.Width)
                {
                    x = 0;
                    y+= toolSize.Height;
                }
                spec.rectangle = new Rectangle(x, y, toolSize.Width, toolSize.Height);
                x+= toolSize.Width;
            }
            bitmap = new Bitmap(initialSize.Width, y + toolSize.Height + 10 + tabHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            UpdateBitmap(category, tabWidth, tabHeight);
            
        }
        public string HitTest(Point p, out IBlock[] defaultArgs)
        {
            string ret = "";
            int i=0;
            foreach (Rectangle r in tabRects)
            {
                if (r.Contains(p))
                {
                    currentCategory = tabTexts[i];
                    LayoutTools(currentCategory);
                    defaultArgs = null;
                    if (Modified != null)
                    {
                        Modified(this);
                    }
                    return "";
                }
                i++;
            }
            foreach (ToolSpec ts in tools)
            {
                if (ts.category == currentCategory && ts.rectangle.Contains(p))
                {
                    defaultArgs = ts.defaultArgs;
                    return ts.funcName;
                }
            }
            defaultArgs = new IBlock[] { };
            return ret;
        }

        public void UpdateBitmap(string category, int tabWidth, int tabHeight)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                
                g.Clear(Color.Transparent);
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150)), 0, tabHeight, bitmap.Width, bitmap.Height - tabHeight);
                int catX = 10;
                Brush text, bg;
                tabRects.Clear();
                tabTexts.Clear();
                int pullY; // pull the tab up a bit so as not to intersect with the toolbox
                foreach (string cat in categories)
                {
                    tabTexts.Add(cat);
                    if(cat == category)
                    {
                        text = Brushes.White;
                        bg = Brushes.Black;
                        pullY = 0;
                    }
                    else
                    {
                        text = Brushes.Black;
                        bg = Brushes.DarkGray;
                        pullY = 1;
                    }
                    Size sz = g.MeasureString(cat, textFont).ToSize();
                    Rectangle r = new Rectangle(catX, 0, sz.Width+tabPadding*2, sz.Height+tabPadding *2-pullY);
                    g.FillRectangle(bg, r);
                    tabRects.Add(r);
                    g.DrawString(cat, textFont, text, catX+tabPadding, tabPadding);
                    g.DrawRectangle(Pens.Black, r);
                    catX += tabSpacing + r.Width;

                }
                foreach (ToolSpec ts in tools)
                {
                    if (!ts.category.Contains(category))
                        continue;
                    g.DrawImageUnscaled(ts.bmpFile, ts.rectangle.Location.Offseted(5,5+ tabHeight));
                }
                Color c1 = Color.FromArgb(20, 20, 20);
                g.FillRectangle(new SolidBrush(c1), 0, tabHeight, bitmap.Width,5);
                g.FillRectangle(new SolidBrush(c1), 0, tabHeight, 5, bitmap.Height-tabHeight);

                //Color c2 = Color.FromArgb(220, 220, 220);
                Color c2 = Color.FromArgb(20, 20, 20);
                g.FillRectangle(new SolidBrush(c2), bitmap.Width-5, tabHeight, 5, bitmap.Height-tabHeight);
                g.FillRectangle(new SolidBrush(c2), 0, bitmap.Height-5, bitmap.Width, 5);
            }
        }

        internal void Reloadtools()
        {
            LayoutTools(currentCategory);
        }
    }
}
