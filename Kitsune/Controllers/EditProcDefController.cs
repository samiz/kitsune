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

        Dictionary<string, int> defaultArgNames = new Dictionary<string, int>();
        public EditProcDefController(ProcDefView view, ProcDefBlock model, BlockViewFactory factory,
            Func<TextBox> textBoxMaker)
        {
            this.view = view;
            this.view.Changed += new ViewChangedEvent(view_Changed);
            this.model = model;
            this.factory = factory;
            this.textBoxMaker = textBoxMaker;
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
            Point center = size.Center(b.Size);
            view.RelativePos = center;
            g.DrawImageUnscaled(b, center);
        }

        public void AddArg(DataType type)
        {
            string argName = MakeArgName(type);
            model.AddBit(new VarDefBlock(argName, type));
        }

        public void AddText()
        {
            string text = GenerateNewName("label");
            model.AddBit(new ProcDefTextBit(text));
        }

        internal void MouseDown(Point p)
        {
            if (State == EditState.Ready)
            {
                IBlockView hit = HitTest(p);
                if (hit is ITextualView)
                {
                    SetEditState(hit as ITextualView);
                }
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
            tb.Focus();
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
            }
        }

        void argTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string newStr = tb.Text;
            editedTextModel.SetText(newStr);
            tb.Size = editedTextView.Assemble().Size;
            Changed(this);

            // since the view is always centered, changed could move the view
            // so we move the textbox accordingly
            tb.Location = editedTextView.AbsolutePos();
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
