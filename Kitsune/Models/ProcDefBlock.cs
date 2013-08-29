using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void ProcDefBitChangedEvent(object sender, int index, IProcDefBit newBit);
    public delegate void ProcDefBitAddedEvent(object sender, IProcDefBit newBit);
    public delegate void ProcDefBodyChangedEvent(object sender, IBlock newBody);
    [Serializable]
    public class ProcDefBlock : IBlock
    {
        public event ProcDefBitChangedEvent FormalParamChanged;
        public event ProcDefBitAddedEvent FormalParamAdded;
        public event ProcDefBodyChangedEvent BodyChanged;

        public List<IProcDefBit> Bits = new List<IProcDefBit>();
        List<int> formalParamPositions = new List<int>();
        IBlock body;

        public ProcDefBlock()
        {
            this.FormalParamChanged += delegate(object sender, int index, IProcDefBit newBit) { };
            this.FormalParamAdded += delegate(object sender, IProcDefBit newBit) { };
            this.BodyChanged += delegate(object sender, IBlock newBody) { };
        }

        public IBlock Body { get { return body; } }

        public void SetBody(IBlock body)
        {
            AttachBody(body);
            this.body = body;
            BodyChanged(this, body);
        }

        public void AddBit(IProcDefBit bit)
        {
            Bits.Add(bit);
            if (bit is VarDefBlock)
            {
                formalParamPositions.Add(Bits.Count - 1);
                Attach((VarDefBlock) bit, formalParamPositions.Count-1);
            }
            FormalParamAdded(this, bit);
        }

        public VarDefBlock GetFormalParam(int i)
        {
            return (VarDefBlock)Bits[formalParamPositions[i]];
        }

        public void SetFormalParam(int i, VarDefBlock p)
        {
            VarDefBlock old = GetFormalParam(i);
            Detach(old);
            this.Bits[formalParamPositions[i]] = p;
            Attach(p, i);
            FormalParamChanged(this, i, p);
        }

        public ParentRelationship ParentRelationship { get; set; }

        private void Attach(IBlock arg, int i)
        {
            arg.ParentRelationship = new ParentRelationship(ParentRelationshipType.Arg, this, i);
        }

        private void AttachBody(IBlock body)
        {
            body.ParentRelationship = new ParentRelationship(ParentRelationshipType.Stack, this, 0);
        }

        private void Detach(IBlock b)
        {
            b.ParentRelationship = new ParentRelationship();
        }

        public IBlock DeepClone()
        {
            ProcDefBlock ret = new ProcDefBlock();
            Bits.ForEach(b=>ret.AddBit(b.DeepClone()));
            ret.body = body.DeepClone();
            return ret;
        }
    }
}
