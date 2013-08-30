using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void TextBlockTextChangedEvent(object sender, string newStr);
    [Serializable]
    public class TextBlock : ITextualBlock
    {
        [field: NonSerialized] public event TextBlockTextChangedEvent TextChanged;
        string text;
        public TextBlock(string text)
        {
            ParentRelationship = new ParentRelationship();
            this.text = text;
            this.TextChanged += delegate(object sender, string newStr) { };
        }
        public IBlock DeepClone()
        {
            return new TextBlock(text);
        }
        public ParentRelationship ParentRelationship { get; set;}

        public void SetText(string newStr)
        {
            this.text = newStr;
            TextChanged(this, newStr);
        }

        public string Text { get { return text; } }
    }
}
