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
        public Bitmap bmp;
        public Rectangle rectangle;
        public string funcName;
        public string category;
        public IBlock[] defaultArgs;
        public ToolSpec(Bitmap bmpFile,
                       string funcName,
                       string category,
            IBlock[] defaultArgs)
        {
            this.bmp = bmpFile;
            this.funcName = funcName;
            this.category = category;
            this.defaultArgs = defaultArgs;
        }

    }
    public delegate void PaletteModifiedEvent(object sender, Rectangle oldRect);
    public class Palette
    {
        public event PaletteModifiedEvent Modified;
        const int tabSpacing = 5;
        const int tabPadding = 2;

        const int toolSpacing = 2;
        const int scrollArrowWidth = 10;

        Bitmap bitmap;
        Size initialSize;
        List<ToolSpec> tools = new List<ToolSpec>();
        List<Rectangle> tabRects = new List<Rectangle>();
        List<string> tabTexts = new List<string>();
        IEnumerable<ToolPrototype> toolSpecs;
        HashSet<String> categories = new HashSet<string>();
        string filePrefix;
        Graphics textMetrics;
        Font textFont, tabFont;
        private string currentCategory;

        bool needScroll = false;
        Rectangle leftScrollRect = new Rectangle(), rightScrollRect = new Rectangle();
        List<int> scrollPositions = new List<int>();
        List<ToolSpec> currentTab = new List<ToolSpec>();
        int currentScrollPosition;
        int tabWidth, tabHeight;
        BlockViewFactory dummyFactory;
        public Palette(Size size, Graphics textMetrics, Font textFont)
        {
            initialSize = size;
            this.textMetrics = textMetrics;
            this.textFont = textFont;
            this.tabFont = new Font(textFont.FontFamily, textFont.Size + 2, textFont.Style);
        }
        internal void Resize(Size size, Graphics g)
        {
            initialSize = size;
            textMetrics = g;
        }
        public Bitmap Bitmap { get { return bitmap; } }
        public void Init(IEnumerable<ToolPrototype> toolSpecs, string filePrefix, string initialCategory,
            BlockSpace blockSpace)
        {
            this.currentCategory = initialCategory;
            this.toolSpecs = toolSpecs;
            this.filePrefix = filePrefix;
            tools.Clear();


            dummyFactory = new BlockViewFactory(textMetrics, textFont,
                blockSpace, new Dictionary<IBlock, IBlockView>(), delegate() { });
            foreach (ToolPrototype spec in toolSpecs)
            {
                AddNewTool(blockSpace, spec);
            }
        }

        private void AddNewTool(BlockSpace blockSpace, ToolPrototype spec)
        {
            string[] parts = spec.tool.Split("|".ToCharArray());
            //string fileName = Path.Combine(filePrefix, parts[0]) + ".bmp";
            string funcName = parts[1];
            // Bitmap tbBmp = (Bitmap)Bitmap.FromFile(fileName);
            Bitmap tbBmp = dummyFactory.ViewFromBlock(blockSpace.makeNewBlock(funcName, spec.defaultArgs))
                .Assemble();
            //tbBmp.MakeTransparent(tbBmp.GetPixel(0, 0));
            tools.Add(new ToolSpec(tbBmp, funcName, spec.category, spec.defaultArgs));
            categories.Add(spec.category);
        }
        internal void AddTool(BlockSpace blockSpace, ToolPrototype spec)
        {
            AddNewTool(blockSpace, spec);
            LayoutTools(currentCategory);
            if (currentCategory == spec.category)
                UpdateBitmap(currentCategory, tabWidth, tabHeight);
        }
        public void LayoutTools(string category)
        {
            needScroll = false;
            currentScrollPosition = 0;
            scrollPositions.Clear();
            scrollPositions.Add(0);
            currentTab.Clear();
            tabHeight = categories.Max(s => (int)textMetrics.MeasureString(s, textFont).Height) + tabPadding * 2;
            tabWidth = categories.Sum(s => (int)textMetrics.MeasureString(s, textFont).Width)
                + (tabPadding * 2 + tabSpacing) * categories.Count;
            int maxRowHeight = 0;

            const int initialX = 5 + scrollArrowWidth;

            int x = initialX;
            int y = y = 5 + tabHeight + 5;
            int toolAreaWidth = initialSize.Width - scrollArrowWidth * 2;
            int i = 0;
            foreach (ToolSpec spec in tools)
            {
                if (!spec.category.Contains(category))
                    continue;
                Size toolSize = spec.bmp.Size;
                currentTab.Add(spec);
                if (x + toolSize.Width >= toolAreaWidth)
                {
                    x = initialX;
                    //y += maxRowHeight;
                    needScroll = true;
                    scrollPositions.Add(i);
                }
                spec.rectangle = new Rectangle(x, y, toolSize.Width, toolSize.Height);
                x += toolSize.Width + toolSpacing;
                maxRowHeight = Math.Max(maxRowHeight, toolSize.Height);
                i++;
            }
            bitmap = new Bitmap(initialSize.Width, y + maxRowHeight + 10 + tabHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            UpdateBitmap(category, tabWidth, tabHeight);

        }
        public string HitTest(Point p, out IBlock[] defaultArgs)
        {
            string ret = "";
            int i = 0;

            foreach (Rectangle r in tabRects)
            {
                if (r.Contains(p))
                {
                    currentCategory = tabTexts[i];
                    Rectangle oldRect = new Rectangle(Location, Size);
                    LayoutTools(currentCategory);
                    defaultArgs = null;
                    if (Modified != null)
                    {
                        Modified(this, oldRect);
                    }
                    return "";
                }
                i++;
            }
            if (needScroll)
            {

                //textMetrics.FillRectangle(Brushes.Fuchsia, leftScrollRect.Offseted(Location.X, Location.Y));
                if (leftScrollRect.Contains(p))
                {
                    ScrollBig(-1);
                    defaultArgs = null;
                    return "";
                }
                // textMetrics.FillRectangle(Brushes.Fuchsia, rightScrollRect.Offseted(Location.X, Location.Y));

                if (rightScrollRect.Contains(p))
                {
                    ScrollBig(1);
                    defaultArgs = null;
                    return "";
                }
            }
            int startPos = ScrollPositionStart();
            int endPos = ScrollPositionEnd();
            for (i = startPos; i < endPos; ++i)
            {
                ToolSpec ts = currentTab[i];
                if (ts.rectangle.Contains(p))
                {
                    defaultArgs = ts.defaultArgs;
                    return ts.funcName;
                }
            }
            defaultArgs = new IBlock[] { };
            return ret;
        }

        private void ScrollBig(int p)
        {
            int oldScrollPosition = currentScrollPosition;

            currentScrollPosition += p;
            if (currentScrollPosition < 0 || currentScrollPosition == scrollPositions.Count)
                currentScrollPosition = oldScrollPosition;

            if (currentScrollPosition != oldScrollPosition)
            {
                UpdateBitmap(currentCategory, tabWidth, tabHeight);
                Modified(this, new Rectangle(this.Location, this.Size));
            }
        }

        public void UpdateBitmap(string category, int tabWidth, int tabHeight)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FastSettings();
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
                    if (cat == category)
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
                    Size sz = g.MeasureString(cat, tabFont).ToSize();
                    Rectangle r = new Rectangle(catX, 0, sz.Width + tabPadding * 2, sz.Height + tabPadding * 2 - pullY);
                    g.FillRectangle(bg, r);
                    tabRects.Add(r);
                    g.DrawString(cat, tabFont, text, catX + tabPadding, tabPadding);
                    g.DrawRectangle(Pens.Black, r);
                    catX += tabSpacing + r.Width;

                }
                int startPos = ScrollPositionStart();
                int endPos = ScrollPositionEnd();
                for (int i = startPos; i < endPos; ++i)
                {
                    ToolSpec ts = currentTab[i];
                    Rectangle r = ts.rectangle;
                    g.DrawImageUnscaled(ts.bmp, r.Location);
                }
                Color c1 = Color.FromArgb(20, 20, 20);
                g.FillRectangle(new SolidBrush(c1), 0, tabHeight, bitmap.Width, 5);
                g.FillRectangle(new SolidBrush(c1), 0, tabHeight, 5, bitmap.Height - tabHeight);

                //Color c2 = Color.FromArgb(220, 220, 220);
                Color c2 = Color.FromArgb(20, 20, 20);
                g.FillRectangle(new SolidBrush(c2), bitmap.Width - 5, tabHeight, 5, bitmap.Height - tabHeight);
                g.FillRectangle(new SolidBrush(c2), 0, bitmap.Height - 5, bitmap.Width, 5);

                if (needScroll)
                {
                    using (Pen p = new Pen(Color.Black, 1.5f))
                    {
                        Point[] scroll1 = new Point[] { 
                    new Point(5 + scrollArrowWidth/2, tabHeight + 5),
                    new Point(5 + scrollArrowWidth/2, tabHeight + initialSize.Height-5),
                    new Point(5, tabHeight + initialSize.Height/2 -5),};
                        leftScrollRect = new Rectangle(5, tabHeight + 5, scrollArrowWidth, initialSize.Height - 10);

                        g.FillPolygon(Brushes.LightGray, scroll1);
                        g.DrawPolygon(p, scroll1);

                        Point[] scroll2 = new Point[] { 
                    new Point(initialSize.Width-5 - scrollArrowWidth/2, tabHeight + 5),
                    new Point(initialSize.Width -5 - scrollArrowWidth/2, tabHeight + initialSize.Height-5),
                    new Point(initialSize.Width-5, tabHeight + initialSize.Height/2 -5),};

                        rightScrollRect = new Rectangle(initialSize.Width - 5 - scrollArrowWidth,
                            tabHeight + 5, scrollArrowWidth, initialSize.Height - 10);
                        g.FillPolygon(Brushes.LightGray, scroll2);
                        g.DrawPolygon(p, scroll2);
                    }
                }

            }
        }

        private int ScrollPositionEnd()
        {
            return currentScrollPosition + 1 < scrollPositions.Count ? scrollPositions[currentScrollPosition + 1] : currentTab.Count;
        }

        private int ScrollPositionStart()
        {
            return scrollPositions[currentScrollPosition];
        }

        internal void Reloadtools()
        {
            LayoutTools(currentCategory);
        }

        public Point Location { get; set; }
        public Size Size
        {
            get
            {
                if (bitmap == null)
                    return new Size(10, 10);
                else
                    return bitmap.Size;
            }
        }
    }
}
