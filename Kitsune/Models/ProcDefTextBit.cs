using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void ProcDefTextBitTextChangedEvent(object sender, string newText);
    public class ProcDefTextBit : IProcDefBit, ITextualBlock
    {
        [field: NonSerialized] public event ProcDefTextBitTextChangedEvent TextChanged;
        public string _Text;
        public ProcDefTextBit(string text)
        {
            this._Text = text;
            this.TextChanged += delegate(object sender, string newText) { };
        }

        public IProcDefBit DeepClone()
        {
            return new ProcDefTextBit(Text);
        }

        public string Text
        {
            get { return _Text; }
        }

        public void SetText(string text)
        {
            _Text = text;
            TextChanged(this, text);
        }

        public ParentRelationship ParentRelationship
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IBlock IBlock.DeepClone()
        {
            throw new NotImplementedException();
        }
    }
}
