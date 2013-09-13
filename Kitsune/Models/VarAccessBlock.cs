using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public class VarAccessBlock : IVarBlock
    {
        VarDefBlock _declaration;

        public VarAccessBlock(VarDefBlock declaration)
        {
            this._declaration = declaration;
        }

        public string Name { get { return _declaration.Name; } }
        public VarDefBlock Declaration { get { return _declaration; } }
        
        [NonSerialized] ParentRelationship _parentRelationship;
        public ParentRelationship ParentRelationship
        {
            get { return _parentRelationship; }
            set { _parentRelationship = value; }
        }
        public bool ShouldSerializeParentRelationship() { return false; }

        public IBlock DeepClone()
        {
            return new VarAccessBlock(Declaration);
        }

        public string ToJson()
        {
            return string.Format("[\"var\", \"{0}\"]", _declaration.Name);
        }
    }
}
