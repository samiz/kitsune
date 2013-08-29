using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public class VarDefBlock : IVarBlock, IProcDefBit
    {
        string _name;
        DataType _type;

        public VarDefBlock(string name, DataType type)
        {
            this._name = name;
            this._type = type;
        }
        public string Name { get { return _name; } }
        public DataType Type { get { return _type; } }
        public ParentRelationship ParentRelationship { get; set; }

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
