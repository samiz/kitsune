using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void ArgListArgAddedEvent(object sender, IBlock newArg, DataType newArgType);
    [Serializable]
    public class ArgList : IEnumerable<IBlock>
    {
        [field: NonSerialized] public event ArgListArgAddedEvent ArgAdded;
        List<DataType> ArgTypes = new List<DataType>();
        List<IBlock> Args = new List<IBlock>();
        InvokationBlock owner;
        public ArgList(InvokationBlock owner)
        {
            this.owner = owner;
            this.ArgTypes = new List<DataType>();
            this.ArgAdded += delegate(object sender, IBlock newArg, DataType newArgType) { };
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

        public void Add(IBlock b, DataType type)
        {
            b.ParentRelationship = new ParentRelationship(ParentRelationshipType.Arg, owner, Args.Count);
            Args.Add(b);
            ArgTypes.Add(type);
            ArgAdded(this, b, type);
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Args.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)Args).GetEnumerator();
        }

        internal void AddRange(IBlock[] values, DataType[] types)
        {
            if (values.Length != types.Length)
                throw new ArgumentException();
            for (int i = 0; i < values.Length; ++i)
                Add(values[i], types[i]);
            
        }
    }
}
