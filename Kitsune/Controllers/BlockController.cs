using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kitsune
{
    enum CanvasState
    {
        Ready, Dragging,
        TextEditing
    }
    public class ToolPrototype
    {
        public string tool;
        public string category;
        public IBlock[] defaultArgs;
    }
    public delegate void ControllerModified(object sender);
    public class BlockController
    {
        public event ControllerModified Modified;
        BlockSpace blockSpace;
        CanvasView canvasView;
        CanvasState state;
        Size canvasSize;

        Dictionary<IBlockView, Point> allViews = new Dictionary<IBlockView, Point>();
        Dictionary<IBlock, IBlockView> blockViews = new Dictionary<IBlock, IBlockView>();

        IBlockView dragged;
        TopLevelScript draggedModel;
        Point draggingOrigin;
        ITextualBlock editedTextModel;
        ITextualView editedTextView;
        string originalEditedText;
        private TextBox editedTextBox;
        string status = "";
        
        BlockViewFactory viewFactory;

        List<DropRegion> DropRegions = new List<DropRegion>();
        
        Palette palette;
        Func<TextBox> textBoxMaker;
        
        public BlockController(Graphics textMetrics, Font textFont, Size canvasSize, Func<TextBox> textBoxMaker)
        {
            this.Modified += delegate(object sender) { };
            blockSpace = new BlockSpace();
            blockSpace.OnTopLevelAdded += new TopLevelEvent(blockSpace_OnTopLevelAdded);
            blockSpace.OnTopLevelDeleted += new TopLevelEvent(blockSpace_OnTopLevelDeleted);
            blockSpace.OnTopLevelMoved += new TopLevelEvent(blockSpace_OnTopLevelMoved);

            palette = new Palette(new Size(canvasSize.Width - 20, 60), textMetrics, textFont);
            palette.Modified += new PaletteModifiedEvent(palette_Modified);

            canvasView = new CanvasView(textMetrics, canvasSize, allViews, DropRegions, textFont, palette);
            this.canvasSize = canvasSize;
            state = CanvasState.Ready;
            viewFactory = new BlockViewFactory(textMetrics, textFont, blockSpace, blockViews,
                ()=>Modified(this));

            this.textBoxMaker = textBoxMaker;
        }

        public void InitPalette()
        {
            palette.Init(new ToolPrototype[] { 
                tool("if|if % then % else %", "control", new TextBlock("0"), new BlockStack(), new BlockStack()),
                tool("repeat|repeat % times %", "control", new TextBlock("10"), new BlockStack()),
                tool("forever|forever %", "control", new BlockStack()),
                tool("wait|wait % milliseconds", "control", new TextBlock("500")),
                tool("move|move % steps", "motion", new TextBlock("10")),
                tool("right|turn % degrees right", "motion", new TextBlock("10")),
                tool("left|turn % degrees left", "motion", new TextBlock("10")),
                tool("gotoxy|move to x: % y: %", "motion", new TextBlock("100"),new TextBlock("100")),
                tool("say|say %", "looks", new TextBlock("hello world")),
                tool("sayForATime|say % for % seconds", "looks", new TextBlock("hello world"), new TextBlock("2")),
                tool("plus|% + %", "math", new TextBlock("5"), new TextBlock("5")),
                tool("minus|% - %", "math", new TextBlock("5"), new TextBlock("5")),
                tool("times|% * %", "math", new TextBlock("5"), new TextBlock("5")),
                tool("divide|% / %", "math", new TextBlock("5"), new TextBlock("5")),
                tool("random|random from % to %", "math", new TextBlock("1"), new TextBlock("10")),
                tool("sin|sin %", "math", new TextBlock("60")),
                tool("cos|cos%", "math", new TextBlock("60")),
                tool("tan|tan %", "math", new TextBlock("60")),
                tool("sqrt|sqrt %", "math", new TextBlock("100")),
                tool("asin|asin %", "math", new TextBlock("1")),
                tool("acos|acos%", "math", new TextBlock("1")),
                tool("atan|atan %", "math", new TextBlock("1")),
                tool("flag|when _flag_ clicked", "control"), 
                tool("stopScript|stop script", "control")}
                , Path.Combine(Application.StartupPath, "./Assets/Tools"), "motion", blockSpace);

            palette.LayoutTools("motion");


        }
        void palette_Modified(object sender, Rectangle oldPaletteRect)
        {
            Update(Rectangle.Union(canvasView.PaletteRect, oldPaletteRect));
        }
        private void ResetBlockSpace()
        {
            blockSpace.OnTopLevelAdded += new TopLevelEvent(blockSpace_OnTopLevelAdded);
            blockSpace.OnTopLevelDeleted += new TopLevelEvent(blockSpace_OnTopLevelDeleted);
            blockSpace.OnTopLevelMoved += new TopLevelEvent(blockSpace_OnTopLevelMoved);
            viewFactory.ResetBlockSpace(blockSpace);
            allViews.Clear();
            blockSpace.NotifyReloaded();
        }
        internal void Clear()
        {
            blockSpace.Clear();
        }
        ToolPrototype tool(string tool, string category, params IBlock[] defaultArgs)
        {
            ToolPrototype tp = new ToolPrototype();
            tp.tool = tool;
            tp.category = category;
            tp.defaultArgs = defaultArgs;
            return tp;
        }
        public void Resize(Size size, Graphics g)
        {
            if (size.Width <= 20)
            {
                // can't resize palette
                return;
            }
            this.canvasSize = size;
            palette.Resize(new Size(canvasSize.Width - 20, 80), g);
            //palette.Resize(new Size(60, canvasSize.Height - 20));
            palette.Reloadtools();
            canvasView.Resize(size, g);
            viewFactory.SetTextMetricsGraphics(g);
            Update();
        }

        public void Save(Stream s)
        {
            IFormatter f = new BinaryFormatter();
            f.Serialize(s, blockSpace);
        }

        public void Load(Stream s)
        {
            IFormatter f = new BinaryFormatter();
            blockSpace = (BlockSpace)f.Deserialize(s);
            blockSpace.AddDummyEvents();
            ResetBlockSpace();
        }

        public void RegisterSystemMethod(string methodName, BlockAttributes attribute, DataType returnType, DataType[] argTypes)
        {
            blockSpace.RegisterMethod(methodName, attribute, returnType, argTypes, true);
        }

        public void DefineNewProc(ProcDefBlock proc, string category)
        {
            string methodName = proc.GetMethodString();
            DataType[] argTypes = proc.GetArgTypes();
            blockSpace.RegisterMethod(methodName, BlockAttributes.Stack, DataType.Script, argTypes, false);
            palette.AddTool(blockSpace,
                tool("nobmp|"+methodName, category, argTypes.Select(t=>blockSpace.Default(t)).ToArray()));

            Random r = new Random();
            int thirdX = canvasSize.Width / 3;
            int thirdY = canvasSize.Height / 3;
            int x = thirdX + r.Next(thirdX);
            int y = thirdY + r.Next(thirdY);
            AddTopLevel(proc, new Point(x, y));
        }

        public TopLevelScript AddTopLevel(IBlock block, Point location)
        {
            return blockSpace.AddScript(new TopLevelScript(location, block, blockSpace));
        }

        public TopLevelScript SplitBlockStack(BlockStack block, int nKeep, Point newStacklocation)
        {
            IBlock newStack = block.Split(nKeep);
            return AddTopLevel(newStack, newStacklocation);
        }

        private TopLevelScript TakeoutBlockArgument(InvokationBlock parent, int i, Point newArgLocation)
        {
            IBlock newBlock = blockSpace.TakeoutArg(parent, i);
            return AddTopLevel(newBlock, newArgLocation);
        }

        void blockSpace_OnTopLevelAdded(object sender, TopLevelScript tl)
        {
            IBlockView v = viewFactory.ViewFromBlock(tl.Block);

            allViews[v] = tl.Location;
            v.RelativePos = tl.Location;
            v.Parent = null;
            
            Update(ViewBounds(v));
            Modified(this);
        }

        void blockSpace_OnTopLevelDeleted(object sender, TopLevelScript tl)
        {
            IBlockView v = blockViews[tl.Block];
            Rectangle bounds = ViewBounds(v);
            allViews.Remove(v);
            Modified(this);            
            Update(bounds);
        }

        void blockSpace_OnTopLevelMoved(object sender, TopLevelScript tl)
        {
            IBlockView v = blockViews[tl.Block];
            Rectangle r1 = ViewBounds(v);
            v.RelativePos = tl.Location;
            allViews[v] = tl.Location;
            Rectangle r2 = ViewBounds(v);
            Modified(this);
            Update(Rectangle.Union(r1, r2));
        }
 
        public void Mark(IBlockView v)
        {
            canvasView.Marked = v;
        }
       
        public void MarkSelectedView(Point p)
        {
            IBlockView oldMarked = canvasView.Marked;
            string oldStatus = status;

            status = "";
            IBlockView marked = null;
            foreach (KeyValuePair<IBlockView, Point> kv in allViews)
            {
                IBlockView v = kv.Key;
                Point location = kv.Value;

                if (v.HasPoint(p, location))
                {
                    marked = v;
                    status = string.Format("({0},{1}) | Subview under mouse: {2}", p.X, p.Y, v.ChildHasPoint(p, location));
                    break;
                }
            }
            canvasView.Marked = marked;
            if (status != oldStatus || marked != oldMarked)
            {
                Rectangle invalidated = Rectangle.Empty;
                if (marked != null)
                    invalidated = Rectangle.Union(invalidated, ViewBounds(marked));

                if (oldMarked != null)
                    invalidated = Rectangle.Union(invalidated, ViewBounds(oldMarked));
                Update(invalidated);
            }
        }

        private Rectangle ViewBounds(IBlockView v)
        {
            if (!allViews.ContainsKey(v))
                return Rectangle.Empty;
            return new Rectangle(allViews[v], v.Assemble().Size);
        }

        public void Update()
        {
            Update(new Rectangle(new Point(0, 0), canvasSize));
        }

        public void Update(Rectangle invalidated)
        {
            canvasView.Update(invalidated);
        }

        public void Redraw(Graphics graphics, Rectangle rectangle)
        {
            canvasView.Redraw(graphics, rectangle);
        }

        internal void MouseDown(Point p)
        {
            if (state == CanvasState.TextEditing)
            {
                // Since the mousedown registered, we've clicked outside the textbox
                ResetTextEditState();
            }
            else if (state == CanvasState.Ready)
            {
                if (canvasView.PaletteRect.Contains(p))
                {
                    int x = canvasView.PaletteRect.Left;
                    int y = canvasView.PaletteRect.Top;
                    IBlock[] defaultArgs;
                    string funcName = palette.HitTest(p.Offseted(-x, -y), out defaultArgs);
                    if (funcName != "")
                    {
                        IBlock b = blockSpace.makeNewBlock(funcName,defaultArgs);
                        TopLevelScript s = AddTopLevel(b, p.Offseted(-5, -5));

                        dragged = blockViews[b];
                        draggingOrigin = p;
                        draggedModel = s;
                        state = CanvasState.Dragging;
                        PrepareDropRegions(b);
                        Update(ViewBounds(dragged));
                        return;
                    }
                }
                IBlockView hit = HitTest(p);
                if (hit == null)
                    return;
                if (!allViews.ContainsKey(hit))
                {
                    if (hit.Model.ParentRelationship.Type == ParentRelationshipType.Stack)
                    {

                        int i = hit.Model.ParentRelationship.Index;

                        Point np = hit.AbsolutePos();
                        Rectangle bounds = ViewBounds(hit.AbsoluteAncestor());
                        BlockStack parent = (BlockStack)hit.Model.ParentRelationship.Parent;
                        TopLevelScript splitted = SplitBlockStack(parent, i, np);
                        Update(bounds);
                        draggedModel = splitted;
                        hit = blockViews[splitted.Block];
                    }
                    else if (hit.Model.ParentRelationship.Type == ParentRelationshipType.Arg)
                    {
                        if (hit is ITextualView)
                        {
                            // We shouldn't detach e.g a number argument from its block
                            // but we should enable the user to edit it
                            
                            SetEditState((ITextualView) hit);
                            return;
                        }
                        int i = hit.Model.ParentRelationship.Index;

                        Point np = hit.AbsolutePos();
                        Rectangle bounds = ViewBounds(hit.AbsoluteAncestor());
                        InvokationBlock parent = (InvokationBlock)hit.Model.ParentRelationship.Parent;
                        TopLevelScript splitted = TakeoutBlockArgument(parent, i, np);
                        Update(bounds);
                        draggedModel = splitted;
                        hit = blockViews[splitted.Block];
                    }
                    else if (hit.Model.ParentRelationship.Type == ParentRelationshipType.FormalParameter)
                    {
                        ProcDefBlock pd = (ProcDefBlock ) hit.Model.ParentRelationship.Parent;
                        VarAccessBlock va = new VarAccessBlock((VarDefBlock)pd.Bits[hit.Model.ParentRelationship.Index]);
                        TopLevelScript tls = AddTopLevel(va, p);
                        hit = ViewFromBlock(va);
                        draggedModel = tls;
                    }
                    else if (hit.Model.ParentRelationship.Type == ParentRelationshipType.None)
                    {
                        hit = null;
                        draggedModel = null;
                    }
                }
                else
                {
                    draggedModel = blockSpace.FindScript(hit.Model);
                }
                if (hit != null)
                {
                    dragged = hit;
                    draggingOrigin = p;
                    state = CanvasState.Dragging;
                    PrepareDropRegions(hit.Model);
                }
                Update();
            }

        }

       internal void MouseMove(Point point)
        {
            if (state == CanvasState.Dragging)
            {
                Point d = point.Minus(draggingOrigin);
                Point p1 = allViews[dragged];
                Point p2 = p1.Offseted(d);
                Size sz = dragged.Assemble().Size;
                Rectangle inva = Rectangle.Union(new Rectangle(p1, sz), new Rectangle(p2, sz));
                inva.Inflate(5, 5);
                draggedModel.Location = p2;
                draggingOrigin = point;
                DropRegion active;
                if (FindActiveDropRegion(ViewBounds(dragged), out active))
                {
                    Rectangle topLevelDest = ViewBounds(active.Destination.AbsoluteAncestor());
                    inva = Rectangle.Union(inva, topLevelDest);
                }
                Update(inva);
            }
            if (state == CanvasState.Ready)
            {
                /*
                IBlockView hit = HitTest(point);
                if (hit != null)
                    canvasView.status = hit.ToString();
                else
                    canvasView.status = "<None>";
                 //*/
            }
        }

        internal void MouseUp(Point point)
        {
            if (state == CanvasState.Dragging)
            {
                if (canvasView.ActiveDropRegion && EffectiveDropRegion(ViewBounds(dragged), canvasView.DropRegion))
                {
                    ConnectBlocks(canvasView.DropRegion, draggedModel);
                    Update();
                }

                state = CanvasState.Ready;
                DropRegions.Clear();
                canvasView.ResetDropRegion();
                Update(new Rectangle(new Point(0, 0), canvasSize));
            }
        }
        
        private void SetEditState(ITextualView v)
        {
            editedTextView = v;
            ITextualBlock model = (ITextualBlock)v.Model;
            editedTextModel = model;
            originalEditedText = model.Text;
            TextBox tb = textBoxMaker();
            editedTextBox = tb;

            tb.Text = model.Text;
            tb.Location = v.AbsolutePos();
            tb.Size = v.Assemble().Size;
            tb.TextChanged += new EventHandler(argTextBox_TextChanged);
            tb.KeyDown += new KeyEventHandler(argTextBox_KeyDown);
            tb.Show();
            tb.Select();
            state = CanvasState.TextEditing;
            
        }

        private void ResetTextEditState()
        {
            editedTextBox.Parent.Controls.Remove(editedTextBox);
            editedTextBox = null;
            editedTextView = null;
            state = CanvasState.Ready;
        }

        void argTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Modified(this);
                ResetTextEditState();
                e.SuppressKeyPress = true; // prevent the beep when pressing enter in a single line TextBox
            }
            else if (e.KeyCode == Keys.Escape)
            {
                editedTextBox.Text = originalEditedText;
                e.SuppressKeyPress = true;
                ResetTextEditState();
            }
        }

        void argTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Rectangle r1 = ViewBounds(editedTextView.AbsoluteAncestor());
            string newStr = tb.Text;
            editedTextModel.SetText(newStr);
            Rectangle r2 = ViewBounds(editedTextView.AbsoluteAncestor());
            Rectangle r3 = Rectangle.Union(r1, r2);
            Update(r3);
            tb.Size = editedTextView.Assemble().Size;
        }

        private void PrepareDropRegions(IBlock block)
        {
            this.DropRegions.Clear();
            foreach (KeyValuePair<IBlockView, Point> kv in allViews)
            {
                IBlockView v = kv.Key;
                if (v.Model == block)
                    continue;
                Point p = kv.Value;
                DropRegions.AddRange(v.DropRegions(p).Where(dr=>dr.IsCompatible(blockSpace.Typeof(block), blockSpace.AttributeOf(block))));
            }
        }

        private bool FindActiveDropRegion(Rectangle rect, out DropRegion  ret)
        {
            foreach (DropRegion dr in DropRegions)
            {
                if (EffectiveDropRegion(rect, dr))
                {
                    canvasView.SetDropRegion(dr);
                    ret = dr;
                    return true;
                }
                
            }
            canvasView.ResetDropRegion();
            ret = new DropRegion();
            return false;
        }

        private static bool EffectiveDropRegion(Rectangle rect, DropRegion dr)
        {
            switch (dr.DropType)
            {
                case DropType.Above:
                    if (rect.BottomSlice(5).IntersectsWith(dr.Rectangle))
                    {
                        return true;
                    }
                    break;
                case DropType.Below:
                case DropType.Between:
                    if (rect.TopSlice(5).IntersectsWith(dr.Rectangle))
                    {
                        return true; ;
                    }
                    break;
                case DropType.AsArgument:
                    if (rect.IntersectsWith(dr.Rectangle))
                    {
                        return true; ;
                    }
                    break;
            }
            return false;
        }

        private void ConnectBlocks(DropRegion dr, TopLevelScript block)
        {
            Point destinationLoc = new Point();
            if(allViews.ContainsKey(dr.Destination))
                destinationLoc = allViews[dr.Destination];
            int sourceHeight = blockViews[block.Block].Assemble().Height;
            switch (dr.DropType)
            {
                case DropType.Above:
                    
                    IBlock finalStack = blockSpace.StackAbove(block, dr.Destination.Model);
                    if (finalStack.ParentRelationship.Type == ParentRelationshipType.None)
                    {
                        // We've stacked A above B, we need to reposition the result so that
                        // B's old location stays the same
                        TopLevelScript ts = blockSpace.FindScript(finalStack);
                        ts.Location = new Point(destinationLoc.X, destinationLoc.Y - sourceHeight + BlockStackView.NotchHeight);
                    }
                    break;
                case DropType.Below:
                    blockSpace.StackBelow(block, dr.Destination.Model);
                    break;
                case DropType.Between:
                    blockSpace.RemoveScript(draggedModel);
                    (dr.Destination.Model as BlockStack).Insert((int)dr.ExtraInfo, block.Block);
                    break;
                case DropType.AsArgument:
                    blockSpace.RemoveScript(draggedModel);
                    (dr.Destination.Model as InvokationBlock).SetArg((int)dr.ExtraInfo, block.Block);
                    break;
            }
        }

        private IBlockView HitTest(Point p)
        {
            IBlockView hit = null;
            foreach (KeyValuePair<IBlockView, Point> kv in allViews)
            {
                IBlockView v = kv.Key;
                Point location = kv.Value;

                if (v.HasPoint(p, location))
                {
                    hit = v.ChildHasPoint(p, location);
                    break;
                }
            }
            return hit;
        }

        public EditProcDefController NewProcDef(Func<TextBox> textBoxMaker, Button eraseButton)
        {
            ProcDefBlock block = new ProcDefBlock();
            block.SetBody(new BlockStack());
            ProcDefView view = (ProcDefView) viewFactory.ViewFromBlock(block);

            EditProcDefController controller = new EditProcDefController(view, block, viewFactory, textBoxMaker, eraseButton);
            return controller;
        }

        internal IBlockView ViewFromBlock(IBlock b)
        {
            return viewFactory.ViewFromBlock(b);
        }

        internal IEnumerable<IBlock> GetTopLevelBlocks()
        {
            return blockSpace.Scripts.Select(s => s.Block);
        }

        internal BlockSpace GetBlockSpace()
        {
            return blockSpace;
        }
    }
}
