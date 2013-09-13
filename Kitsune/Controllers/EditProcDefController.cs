using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Kitsune
{
    public delegate void EditProcDefControllerChangedEvent(object sender);
    public class EditProcDefController
    {
        public event EditProcDefControllerChangedEvent Changed;
        
        enum EditState { Ready, TextEditing };
        private EditState State;

        ProcDefView view;
        ProcDefBlock model;
        BlockViewFactory factory;

        ITextualBlock editedTextModel;
        ITextualView editedTextView;
        string originalEditedText;
        TextBox editedTextBox;
        Func<TextBox> textBoxMaker;
        //Rectangle debugRect = new Rectangle(), debugRect2 = new Rectangle();
        Button eraseButton;

        Dictionary<string, int> defaultArgNames = new Dictionary<string, int>();
        private Graphics graphics;
        public EditProcDefController(ProcDefView view, ProcDefBlock model, BlockViewFactory factory,
            Func<TextBox> textBoxMaker, Button eraseButton)
        {
            this.view = view;
            this.view.Changed += new ViewChangedEvent(view_Changed);
            this.model = model;
            this.factory = factory;
            this.textBoxMaker = textBoxMaker;
            this.eraseButton = eraseButton;
            this.eraseButton.Hide();
            this.eraseButton.Click += new EventHandler(eraseButton_Click);
            this.Changed += delegate(object sender) { };
            State = EditState.Ready;

            // I wish the view was dynamically centered, but there's some mismatch
            // between a label's textbox and the place where the text is drawn
            // for now we hide it by not constantly moving the view
            // strangely, the mistmatch disappears (text is drawn in the right place)
            // when view.RelativePos is not moved anymore. hmm...
            // Point origin = size.Center(b.Size);
            Point origin = new Point(15, 15);
            view.RelativePos = origin;
        }

        public void Done()
        {
            view.Changed -= view_Changed;
        }
        void view_Changed(object source)
        {
            //Redraw(graphics);
            Changed(this);
        }

        public void Redraw(Graphics g)
        {
            g.Clear(Color.WhiteSmoke);
            Bitmap b = view.Assemble();
            
            g.DrawImageUnscaled(b, view.RelativePos);
            //g.DrawRectangle(Pens.Teal, debugRect);
            //g.DrawRectangle(Pens.Fuchsia, debugRect2);
        }

        internal void Redraw()
        {
            Redraw(graphics);
        }
        public Image FinalImage { get { return view.Assemble(); } }
        public void AddArg(DataType type)
        {
            string argName = MakeArgName(type);
            VarDefBlock v = new VarDefBlock(argName, type);
            v.ParentRelationship = new ParentRelationship(ParentRelationshipType.FormalParameter, model, model.Bits.Count);
            model.AddBit(v);
        }

        public void TestAddArgPerformance()
        {
            for (int i = 0; i < 1500; ++i)
            {
                AddArg(DataType.Number);
                IProcDefBit b = model.Bits.Last();
                model.RemoveBit(b);
            }
        }

        public void AddText()
        {
            //string text = GenerateNewName("label");
            string text = "";
            ProcDefTextBit t = new ProcDefTextBit(text);
            t.ParentRelationship = new ParentRelationship(ParentRelationshipType.None, model, -1);
            model.AddBit(t);
            ITextualView v = (ITextualView) factory.ViewFromBlock(t);
            SetEditState(v);
        }

        internal void MouseDown(Point p)
        {
            if (State == EditState.TextEditing)
            {
                IBlockView hit = HitTest(p);
                if (hit != editedTextView)
                {
                    ResetTextEditState();
                }
            }
            // Note that we can enter the following if block
            // after the preceding one
            if (State == EditState.Ready)
            {
                IBlockView hit = HitTest(p);
                
                if (hit is ITextualView)
                {
                    SetEditState(hit as ITextualView);
                }
            }
        }

        void eraseButton_Click(object sender, EventArgs e)
        {
            IProcDefBit b = (IProcDefBit)editedTextModel;
            ResetTextEditState();
            model.RemoveBit(b);
        }

        private void ShowEraseButton(IBlockView hit)
        {
            eraseButton.Location = new Point(hit.AbsolutePos().X, 2);
            eraseButton.Show();
        }

        private void HideEraseButton()
        {
            eraseButton.Hide();
        }

        private void SetEditState(ITextualView v)
        {
            if (State == EditState.TextEditing)
            {
                // We're done with any old editing operation
                ResetTextEditState();
            }
            editedTextView = v;
            ITextualBlock model = (ITextualBlock)v.Model;
            editedTextModel = model;
            originalEditedText = model.Text;
            TextBox tb = textBoxMaker();
            editedTextBox = tb;

            tb.Text = model.Text;
            PositionTextBox(tb, v.AbsoluteBounds());
            
            tb.TextChanged += new EventHandler(argTextBox_TextChanged);
            tb.KeyDown += new KeyEventHandler(argTextBox_KeyDown);
            
            tb.Show();
            tb.Select();
            ShowEraseButton(v);
            State = EditState.TextEditing;
        }

        private void ResetTextEditState()
        {
            HideEraseButton();
            editedTextBox.Parent.Controls.Remove(editedTextBox);
            editedTextBox = null;
            editedTextView = null;
            State = EditState.Ready;
        }

        void argTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Changed(this);
                ResetTextEditState();
                e.SuppressKeyPress = true; // prevent the beep when pressing enter in a single line TextBox
            }
            else if (e.KeyCode == Keys.Escape)
            {
                editedTextBox.Text = originalEditedText;
                ResetTextEditState();
                e.SuppressKeyPress = true;
            }
        }

        void argTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string newStr = tb.Text;
            editedTextModel.SetText(newStr);
             
            Changed(this);

            // Since the view is always centered, changed could move the view
            // so we move the textbox accordingly
            // PositionTextBox(tb, editedTextView.AbsoluteBounds());

            tb.Size = editedTextView.Assemble().Size;
            
            /*
            using (Graphics g = tb.Parent.CreateGraphics())
            {
                debugRect = tb.Bounds.Offseted(0, 50);
                debugRect2 = editedTextView.AbsoluteBounds().Offseted(0, 60);
            }
             //*/
        }

        private void PositionTextBox(TextBox tb, Rectangle r)
        {
            tb.Location = r.Location;
            tb.Size = r.Size;
        }

        private string MakeArgName(DataType type)
        {
            string baseName = "undefined";
            switch (type)
            {
                case DataType.Text:
                    baseName = "text";
                    break;
                case DataType.Number:
                    baseName = "number";
                    break;
                case DataType.Boolean:
                    baseName = "boolean";
                    break;
                case DataType.Object:
                    baseName = "object";
                    break;
            }
            return GenerateNewName(baseName);
        }

        private string GenerateNewName(string baseName)
        {
            if (!defaultArgNames.ContainsKey(baseName))
                defaultArgNames[baseName] = 0;
            return baseName + (++defaultArgNames[baseName]);
        }

        private IBlockView HitTest(Point p)
        {
            IBlockView hit = null;
            Point location = view.RelativePos;
            if (view.HasPoint(p, location))
            {
                hit = view.ChildHasPoint(p, location);
            }
            
            return hit;
        }
        public ProcDefBlock Model { get { return model; } }

        internal void SetGrapics(Graphics graphics)
        {
            this.graphics = graphics;
        }


    }
}
