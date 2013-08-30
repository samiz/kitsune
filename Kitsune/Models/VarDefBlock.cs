using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void VarDefBlockTextChangedEvent(object sender, string newText);
    [Serializable]
    public class VarDefBlock : IVarBlock, IProcDefBit, ITextualBlock
    {
        [field: NonSerialized] public event VarDefBlockTextChangedEvent TextChanged;
        string _name;
        DataType _type;

        public VarDefBlock(string name, DataType type)
        {
            this._name = name;
            this._type = type;
            TextChanged += delegate(object sender, string newText) { };
        }
        public string Name { get { return _name; } }
        public string Text { get { return _name; } }

        public DataType Type { get { return _type; } }
        public ParentRelationship ParentRelationship { get; set; }

        public void SetText(string text)
        {
            _name = text;
            TextChanged(this, text);
        }

        public IBlock DeepClone()
        {
            return new VarDefBlock(_name, _type);
        }

        IProcDefBit IProcDefBit.DeepClone()
        {
            return new VarDefBlock(_name, _type);
        }
    }
}
