using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public class ArgList : IEnumerable<IBlock>
    {
        List<DataType> ArgTypes = new List<DataType>();
        List<IBlock> Args = new List<IBlock>();
        InvokationBlock owner;
        public ArgList(InvokationBlock owner)
        {
            this.owner = owner;
            this.ArgTypes = new List<DataType>();
        }

        public ArgList(InvokationBlock owner, IEnumerable<DataType> ArgTypes)
        {
            this.owner = owner;
            this.ArgTypes.AddRange(ArgTypes);
        }

        public IBlock this[int index]
        {
            get
            {
                return Args[index];
            }
            set
            {
                IBlock old = Args[index];
                Args[index] = value;
                value.ParentRelationship = new ParentRelationship(ParentRelationshipType.Arg, owner, index);
            }
        }

        public void Add(IBlock b)
        {
            b.ParentRelationship = new ParentRelationship(ParentRelationshipType.Arg, owner, Args.Count);
            Args.Add(b);
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Args.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)Args).GetEnumerator();
        }

        internal void AddRange(IBlock[] values)
        {
            foreach (IBlock b in values)
                Add(b);
            
        }
    }
}
