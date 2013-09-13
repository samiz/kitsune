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
        public DataType ReturnType { get; private set; }
        
        public InvokationBlock(string text, BlockAttributes Attributes, IEnumerable<DataType> ArgTypes, DataType returnType)
        {
            this.Text = text;
            this.Attributes = Attributes;
            this.ArgTypes = new List<DataType>();
            this.ArgTypes.AddRange(ArgTypes);
            this.ReturnType = returnType;
            this.Args = new ArgList(this);

            this.ParentRelationship = new ParentRelationship();
            this.OnArgChanged += delegate(object sender, int arg, IBlock _old, IBlock _new) { };
        }

        [NonSerialized]
        ParentRelationship _parentRelationship;
        public ParentRelationship ParentRelationship
        {
            get { return _parentRelationship; }
            set { _parentRelationship = value; }
        }
        public bool ShouldSerializeParentRelationship() { return false; }

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
            InvokationBlock ret = new InvokationBlock(Text, Attributes, ArgTypes, ReturnType);
            int i = 0;
            foreach (IBlock arg in Args)
            {
                ret.SetArg(i, arg.DeepClone());
            }
            return ret;
        }
        public string ToJson()
        {
            List<string> lst = new List<string>();
            lst.AddRange(this.Args.Select(b => b.ToJson()));
            return string.Format("[\"{0}\",\"{1}\",\"{2}\", {3}]", 
                this.Text,
                DataTypeNames.TypeFingerprint(this.ArgTypes),
                this.ReturnType,
                lst.Combine(", "));
        }
    }
}
