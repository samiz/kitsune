﻿using System;
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
        [field: NonSerialized] public event ProcDefBodyChangedEvent BodyChanged;

        public List<IProcDefBit> Bits = new List<IProcDefBit>();
        IBlock body;

        public ProcDefBlock()
        {
            this.ParentRelationship = new ParentRelationship();

            this.FormalParamChanged += delegate(object sender, int index, IProcDefBit newBit) { };
            this.FormalParamAdded += delegate(object sender, IProcDefBit newBit) { };
            this.FormalParamRemoved += delegate(object sender, IProcDefBit bit) { };
            this.BodyChanged += delegate(object sender, IBlock newBody) { };
        }

        public IBlock Body { get { return body; } }
        
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
        public void SetBody(IBlock body)
        {
            this.body = body;
            BodyChanged(this, body);
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

        public ParentRelationship ParentRelationship { get; set; }

        public IBlock DeepClone()
        {
            ProcDefBlock ret = new ProcDefBlock();
            Bits.ForEach(b=>ret.AddBit(b.DeepClone()));
            ret.body = body.DeepClone();
            return ret;
        }

    }
}
