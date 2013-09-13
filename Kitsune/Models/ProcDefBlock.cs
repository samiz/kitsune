using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void ProcDefBitChangedEvent(object sender, int index, IProcDefBit newBit);
    public delegate void ProcDefBitAddedEvent(object sender, IProcDefBit newBit);
    public delegate void ProcDefBitRemovedEvent(object sender, IProcDefBit bit);
    public delegate void ProcDefBodyChangedEvent(object sender, IBlock newBody);
    [Serializable]
    public class ProcDefBlock : IBlock
    {
        [field: NonSerialized] public event ProcDefBitChangedEvent FormalParamChanged;
        [field: NonSerialized] public event ProcDefBitAddedEvent FormalParamAdded;
        [field: NonSerialized] public event ProcDefBitRemovedEvent FormalParamRemoved;

        public List<IProcDefBit> Bits = new List<IProcDefBit>();

        public ProcDefBlock()
        {
            this.ParentRelationship = new ParentRelationship();

            this.FormalParamChanged += delegate(object sender, int index, IProcDefBit newBit) { };
            this.FormalParamAdded += delegate(object sender, IProcDefBit newBit) { };
            this.FormalParamRemoved += delegate(object sender, IProcDefBit bit) { };
        }

        internal string GetMethodString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IProcDefBit bit in Bits)
                sb.Append(bit.ArgBitString());
            return sb.ToString();
        }


        internal DataType[] GetArgTypes()
        {
            List<DataType> ret = new List<DataType>();
            foreach (IProcDefBit bit in Bits)
            {
                if (bit is VarDefBlock)
                {
                    VarDefBlock vdb = bit as VarDefBlock;
                    ret.Add(vdb.Type);
                }
            }
            return ret.ToArray();
        }

        internal string[] GetArgNames()
        {
            List<string> ret = new List<string>();
            foreach (IProcDefBit bit in Bits)
            {
                if (bit is VarDefBlock)
                {
                    VarDefBlock vdb = bit as VarDefBlock;
                    ret.Add(vdb.Name);
                }
            }
            return ret.ToArray();
        }

        internal VarDefBlock GetArg(string varName)
        {
            foreach (IProcDefBit bit in Bits)
            {
                if (bit is VarDefBlock)
                {
                    VarDefBlock vdb = bit as VarDefBlock;
                    if (vdb.Name == varName)
                        return vdb;
                }
            }
            throw new ArgumentException(string.Format("ProcDefBlock.GetArg(): Variable {0} not an argument", varName));
        }


        public void AddBit(IProcDefBit bit)
        {
            Bits.Add(bit);
            FormalParamAdded(this, bit);
        }

        internal void RemoveBit(IProcDefBit bit)
        {
            if (Bits.Contains(bit))
            {
                Bits.Remove(bit);
            }
            FormalParamRemoved(this, bit);
        }

        [NonSerialized] ParentRelationship _parentRelationship;
        public ParentRelationship ParentRelationship 
        { 
            get { return _parentRelationship; } 
            set { _parentRelationship = value; } 
        }
        public bool ShouldSerializeParentRelationship() { return false; }

        public IBlock DeepClone()
        {
            ProcDefBlock ret = new ProcDefBlock();
            Bits.ForEach(b=>ret.AddBit(b.DeepClone()));
            return ret;
        }
        public string ToJson()
        {
            List<string> lst = new List<string>();
            lst.Add("\"define\"");

            List<string> lst2 = new List<string>();
            lst2.Add(this.GetMethodString());
            lst2.AddRange(this.GetArgNames().InterleavedWith(this.GetArgTypes().Select(at=>DataTypeNames.NameOf(at))));
            lst.Add(string.Format("[{0}]", lst2.Select(a=>string.Format("\"{0}\"",a)).Combine(", ")));
                        
            return string.Format("[{0}]", lst.Combine(", "));
        }
    }
}
