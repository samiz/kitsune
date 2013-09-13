using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void ProcDefTextBitTextChangedEvent(object sender, string newText);
    [Serializable]
    public class ProcDefTextBit : IProcDefBit, ITextualBlock
    {
        [field: NonSerialized] public event ProcDefTextBitTextChangedEvent TextChanged;
        public string _Text;
        public ProcDefTextBit(string text)
        {
            this._Text = text;
            this.TextChanged += delegate(object sender, string newText) { };
        }

        public string ArgBitString()
        {
            return _Text;
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

        [NonSerialized] ParentRelationship _parentRelationship;
        public ParentRelationship ParentRelationship
        {
            get { return _parentRelationship; }
            set { _parentRelationship = value; }
        }
        public bool ShouldSerializeParentRelationship() { return false; }

        IBlock IBlock.DeepClone()
        {
            throw new NotImplementedException();
        }

        public string ToJson()
        {
            return Text;
        }
    }
}
