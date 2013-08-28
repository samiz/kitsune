using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public enum ParentRelationshipType
    {
        None, Stack, Arg
    }

    [Serializable]
    public class ParentRelationship
    {
        public ParentRelationshipType Type;
        public IBlock Parent;
        public int Index;
        public ParentRelationship()
        {
            this.Type = ParentRelationshipType.None;
            this.Parent = null;
            this.Index = -1;
        }
        public ParentRelationship(ParentRelationshipType Type, IBlock parent, int Index)
        {
            this.Type = Type;
            this.Parent = parent;
            this.Index = Index;
        }

        internal void Detach(BlockSpace blockSpace)
        {
            if (Parent is InvokationBlock)
            {
                InvokationBlock ib = Parent as InvokationBlock;
                ib.SetArg(Index, blockSpace.Default(ib.ArgTypes[Index]));
            }
            else if(Parent is BlockStack)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
