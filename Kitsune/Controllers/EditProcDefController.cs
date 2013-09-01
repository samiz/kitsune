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
        Rectangle debugRect = new Rectangle(), debugRect2 = new Rectangle();
        Button eraseButton;

        Dictionary<string, int> defaultArgNames = new Dictionary<string, int>();
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
        }

        public void Done()
        {
            view.Changed -= view_Changed;
        }
        void view_Changed(object source)
        {
            Changed(this);
        }

        public void Redraw(Graphics g, Size size)
        {
            g.Clear(Color.WhiteSmoke);
            Bitmap b = view.Assemble();
            
            // I wish it were the center, but there's some mismatch
            // between a label's textbox and the place where the text is drawn
            // for now we hide it by not constantly moving the view
            // strangely, the mistmatch disappears (text is drawn in the right place)
            // when view.RelativePos is not moved anymore. hmm...
            // Point origin = size.Center(b.Size);
            Point origin = new Point(15, 15);
            view.RelativePos = origin;
            g.DrawImageUnscaled(b, origin);
            //g.DrawRectangle(Pens.Teal, debugRect);
            //g.DrawRectangle(Pens.Fuchsia, debugRect2);
        }

        public void AddArg(DataType type)
        {
            string argName = MakeArgName(type);
            model.AddBit(new VarDefBlock(argName, type));
        }

        public void AddText()
        {
            string text = GenerateNewName("label");
            ProcDefTextBit t = new ProcDefTextBit(text);
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
            eraseButton.Hide();
        }

        private void ShowEraseButton(IBlockView hit)
        {
            eraseButton.Location = new Point(hit.AbsolutePos().X, 2);
            eraseButton.Show();
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
            tb.Focus();
            ShowEraseButton(v);
            State = EditState.TextEditing;
        }

        private void ResetTextEditState()
        {
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

            // since the view is always centered, changed could move the view
            // so we move the textbox accordingly
            // PositionTextBox(tb, editedTextView.AbsoluteBounds());

            using (Graphics g = tb.Parent.CreateGraphics())
            {
                debugRect = tb.Bounds.Offseted(0, 50);
                debugRect2 = editedTextView.AbsoluteBounds().Offseted(0, 60);
            }
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
    }
}
