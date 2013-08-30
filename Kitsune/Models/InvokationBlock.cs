using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public delegate void InvokationBlockArgChangeEvent(object sender, int arg, IBlock _old, IBlock _new);
    [Serializable]
    public class InvokationBlock : IBlock
    {
        [field: NonSerialized] public event InvokationBlockArgChangeEvent OnArgChanged;
        public string Text { get; private set;  }
        public BlockAttributes Attributes { get; private set; }
        public ArgList Args { get; private set; }
        public List<DataType> ArgTypes { get; private set; }
        
        public InvokationBlock(string text, BlockAttributes Attributes, IEnumerable<DataType> ArgTypes)
        {
            this.Text = text;
            this.Attributes = Attributes;
            this.ArgTypes = new List<DataType>();
            this.ArgTypes.AddRange(ArgTypes);
            this.Args = new ArgList(this);

            this.ParentRelationship = new ParentRelationship();
            this.OnArgChanged += delegate(object sender, int arg, IBlock _old, IBlock _new) { };
        }
        
        public ParentRelationship ParentRelationship { get; set; }

        public void SetArg(int i, IBlock arg)
        {
            IBlock oldArg = Args[i];
            Detach(oldArg);
            Args[i] = arg;
            Attach(arg, i);
            OnArgChanged(this, i, oldArg, arg);
        }

        private void Attach(IBlock arg, int i)
        {
            arg.ParentRelationship = new ParentRelationship(ParentRelationshipType.Arg, this, i);
        }

        private void Detach(IBlock arg)
        {
            arg.ParentRelationship = new ParentRelationship();
        }

        public IBlock DeepClone()
        {
            InvokationBlock ret = new InvokationBlock(Text, Attributes, ArgTypes);
            int i = 0;
            foreach (IBlock arg in Args)
            {
                ret.SetArg(i, arg.DeepClone());
            }
            return ret;
        }
    }
}
