using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public delegate void EditProcDefControllerChangedEvent(object sender);
    public class EditProcDefController
    {
        public event EditProcDefControllerChangedEvent Changed;
        ProcDefView view;
        ProcDefBlock model;
        BlockViewFactory factory;

        Dictionary<string, int> defaultArgNames = new Dictionary<string, int>();
        public EditProcDefController(ProcDefView view, ProcDefBlock model, BlockViewFactory factory)
        {
            this.view = view;
            this.view.Changed += new ViewChangedEvent(view_Changed);
            this.model = model;
            this.factory = factory;
            this.Changed += delegate(object sender) { };
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
            g.Clear(Color.CornflowerBlue);
            g.DrawImageUnscaled(view.Assemble(), 0, 0);
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


    }
}
