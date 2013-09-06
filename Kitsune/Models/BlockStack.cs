using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public delegate void BlockStackSplitEvent(int nLeft);
    public delegate void BlockStackInsertEvent(object sender, int i, IBlock b);
    [Serializable]
    public class BlockStack : IBlock, IEnumerable<IBlock>
    {
        List<IBlock> Blocks = new List<IBlock>();

        [field: NonSerialized] public event BlockStackSplitEvent OnSplit;
        [field: NonSerialized] public event BlockStackInsertEvent OnInsert;
        public BlockStack()
        {
            this.ParentRelationship = new ParentRelationship();
            this.OnSplit += delegate(int nLeft) { };
            this.OnInsert += delegate(object sender, int i, IBlock b) { };
        }
        public IBlock DeepClone()
        {
            BlockStack ret = new BlockStack();

            foreach (IBlock block in Blocks)
            {
                ret.Add(block);
            }
            return ret;
        }
        public ParentRelationship ParentRelationship { get; set; }

        public IBlock this[int index]
        {
            get
            {
                return Blocks[index]; 
            }
            set
            {
                IBlock old = Blocks[index];
                Detach(old);
                Blocks[index] = value;
                Attach(value, index);
            }
        }
        public int Count { get { return Blocks.Count; } }
        private void Detach(IBlock old)
        {
            old.ParentRelationship = new ParentRelationship();
        }

        private void Attach(IBlock b, int index)
        {
            b.ParentRelationship = new ParentRelationship(ParentRelationshipType.Stack, this, index);
        }

        public void Add(IBlock b)
        {
            Blocks.Add(b);
            Attach(b, Blocks.Count - 1);
        }

        public void AddRange(IEnumerable<IBlock> values)
        {
            foreach (IBlock b in values)
                Add(b);
        }

        internal void Clear()
        {
            foreach (IBlock b in Blocks)
            {
                Detach(b);
            }
            Blocks.Clear();
        }

        internal void Insert(int index, IBlock block)
        {
            Blocks.Insert(index, block);
            for (int i = index; i < Blocks.Count; ++i)
            {
                Attach(Blocks[i], i);
            }
            OnInsert(this, index, block);
        }


        public IBlock Split(int nKeep)
        {
            if(nKeep <1 || nKeep >= Blocks.Count)
                throw new InvalidOperationException(string.Format("Cannot Split {0} blocks from a {1}-block stack", Blocks.Count - nKeep, Blocks.Count));

            if ((Blocks.Count - nKeep) == 1)
            {
                IBlock ret = Blocks.Last();
                Detach(ret);
                Blocks.RemoveAt(nKeep);
                OnSplit(nKeep);
                return ret;
            }
            else
            {
                BlockStack ret = new BlockStack();
                for (int i = nKeep; i < Blocks.Count; ++i)
                {
                    // Detach(Blocks[i], i); // Not really needed since the new stack will override the attachment
                    ret.Blocks.Add(Blocks[i]);
                }
                
                Blocks.RemoveRange(nKeep, Blocks.Count - nKeep);
                OnSplit(nKeep);
                return ret;
            }
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Blocks.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)Blocks).GetEnumerator();
        }

        public bool Empty { get { return Blocks.Count == 0; } }
    }
}
