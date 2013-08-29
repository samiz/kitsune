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
        public ParentRelationship ParentRelationship {get ; set; }

        public IBlock DeepClone()
        {
            return new VarAccessBlock(Declaration);
        }
    }
}
