using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public delegate void TopLevelEvent (object sender, TopLevelScript tl);
    
    [Serializable]
    public class BlockSpace
    {
        [field: NonSerialized]
        public event TopLevelEvent OnTopLevelDeleted;
        [field: NonSerialized]
        public event TopLevelEvent OnTopLevelAdded;
        [field: NonSerialized]
        public event TopLevelEvent OnTopLevelMoved;
        
        public List<TopLevelScript> Scripts = new List<TopLevelScript>();
        public Dictionary<string, BlockInfo> blockInfos = new Dictionary<string, BlockInfo>();
        public Dictionary<string, BlockInfo> systemBlockInfos = new Dictionary<string, BlockInfo>();
        
        public BlockSpace()
        {
            AddDummyEvents();
        }

        internal void Clear()
        {
            Scripts.ForEach(s => OnTopLevelDeleted(this, s));
            Scripts.Clear();
            blockInfos.Clear();
            foreach (KeyValuePair<string, BlockInfo> kv in systemBlockInfos)
            {
                blockInfos[kv.Key] = kv.Value;
            }
        }

        public void AddDummyEvents()
        {
            OnTopLevelDeleted += delegate(object a, TopLevelScript b) { };
            OnTopLevelAdded += delegate(object a, TopLevelScript b) { };
            OnTopLevelMoved += delegate(object a, TopLevelScript b) { };
        }

        public TopLevelScript AddScript(TopLevelScript s)
        {
            Scripts.Add(s);
            OnTopLevelAdded(this, s);
            return s;
        }

        internal void NotifyReloaded()
        {
            Scripts.ForEach(s=>OnTopLevelAdded(this,s));
        }

        public void DetachArgument(InvokationBlock b, int i, Point newLocation)
        {
            IBlock oldArg = b.Args[i];
            b.SetArg(i, Default(b.ArgTypes[i]));

            oldArg.ParentRelationship = new ParentRelationship();
            AddScript(new TopLevelScript(newLocation, oldArg, this));
        }

        public void RegisterMethod(string methodName, BlockAttributes attribute, DataType returnType, DataType[] argTypes, bool system)
        {
            BlockInfo bi = new BlockInfo(methodName, attribute, returnType, argTypes);
            blockInfos[methodName] = bi;

            if (system)
            {
                systemBlockInfos[methodName] = bi;
            }
        }

        public bool IsMethodAFunction(string methodName)
        {
            return blockInfos[methodName].ReturnType  != DataType.Script;
        }
        public IBlock makeNewBlock(string text)
        {
            BlockInfo bi = blockInfos[text];
            IBlock[] args = new IBlock[bi.ArgTypes.Length];
            for (int i = 0; i < bi.ArgTypes.Length; ++i)
            {
                args[i] = Default(bi.ArgTypes[i]);
            }

            InvokationBlock block = new InvokationBlock(text, BlockAttributes.Hat, bi.ArgTypes);
            block.Args.AddRange(args, bi.ArgTypes);
            return block;
        }
        public IBlock makeNewBlock(string text, IBlock[] args)
        {
            BlockInfo bi = blockInfos[text];
            
            InvokationBlock block = new InvokationBlock(text, BlockAttributes.Hat, bi.ArgTypes);
            block.Args.AddRange(args.Select(a=>a.DeepClone()).ToArray(), bi.ArgTypes);
            
            return block;
        }

        public IBlock Default(DataType argType)
        {
            switch (argType)
            {
                case DataType.Number:
                case DataType.Text:
                    return new TextBlock("");
                case DataType.Script:
                    return new BlockStack();
            }
            throw new InvalidOperationException(string.Format("BlockSpace.Default() : Didn't handle argument type {0}", argType));
        }

        public void RemoveScript(TopLevelScript s)
        {
            Scripts.Remove(s);
            OnTopLevelDeleted(this, s);
        }

        internal TopLevelScript FindScript(IBlock b)
        {
            return Scripts.Find(s => s.Block == b);
        }

        internal BlockAttributes AttributeOf(IBlock block)
        {
            if (block is InvokationBlock)
            {
                InvokationBlock invokation = block as InvokationBlock;
                return blockInfos[invokation.Text].Attribute;
            }
            else if(block is BlockStack)
            {
                BlockStack stack = (BlockStack)block;
                if(stack.Empty)
                    return BlockAttributes.Stack;
                if (AttributeOf(stack[0]) == BlockAttributes.Hat)
                    return BlockAttributes.Hat;
                if (AttributeOf(stack.Last()) == BlockAttributes.Cap)
                    return BlockAttributes.Cap;
                return BlockAttributes.Stack;
            }
            else if (block is ProcDefBlock)
            {
                return BlockAttributes.Hat;
            }
            throw new NotImplementedException();
        }
        internal DataType Typeof(IBlock block)
        {
            if (block is InvokationBlock)
            {
                InvokationBlock invokation = block as InvokationBlock;
                return blockInfos[invokation.Text].ReturnType;
            }
            return DataType.Script;
        }
        internal IBlock StackAbove(TopLevelScript b1, IBlock b2)
        {
            ParentRelationship b2_oldRelationship = b2.ParentRelationship;
            BlockStack b3 = MergeStacks(b1.Block, b2);
            RemoveScript(b1);
            Become(b2_oldRelationship, b2, b3);
            return b3;
        }

        internal IBlock StackBelow(TopLevelScript b1, IBlock b2)
        {
            ParentRelationship b2_oldRelationship = b2.ParentRelationship;
            BlockStack b3 = MergeStacks(b2, b1.Block);
            RemoveScript(b1);
            Become(b2_oldRelationship, b2, b3);
            return b3;
        }

        // 'a' shall become 'b', how?
        // - If a is toplevel, remove it and add b in its place
        // - If it isn't the parent has to replace a with b
        void Become(ParentRelationship pr, IBlock a, IBlock b)
        {
            if (pr.Type == ParentRelationshipType.None)
            {
                TopLevelScript tl = FindScript(a);
                RemoveScript(tl);
                AddScript(new TopLevelScript(tl.Location, b, this));
            }
            else
            {
                switch (pr.Type)
                {
                    case ParentRelationshipType.Arg:
                        ((InvokationBlock)pr.Parent).SetArg(pr.Index, b);
                        break;
                    case ParentRelationshipType.Stack:
                        //
                        break;
                }
            }
        }

        BlockStack MergeStacks(IBlock b1, IBlock b2)
        {
            BlockStack ret = new BlockStack();
            if ((b1 is BlockStack))
            {
                BlockStack bs1 = b1 as BlockStack;
                ret.AddRange(bs1);
            }
            else
            {
                // We need to remove b1 from its parent before adding it to the newly merged stack
                // otherwise consider such a scenario:
                // 1- We drag a stack block into an existing stackBlock in a C block
                // 2- 'b1' is the existing stackBlock, it is not added to ret, but is still an arg of the C block
                // 3- after merge is called, we'll try to set ret as the arg of the C block; the C will try
                // to remove the old arg (b1)...but it doesn't have C as a parent! exception thrown
                b1.ParentRelationship.Detach(this);
                ret.Add(b1);
            }
            if ((b2 is BlockStack))
            {
                BlockStack bs2 = b2 as BlockStack;
                ret.AddRange(bs2);
            }
            else
            {
                b2.ParentRelationship.Detach(this);
                ret.Add(b2);
            }
            return ret;
        }



        internal void NotifyTopLevelMoved(TopLevelScript topLevelScript)
        {
            OnTopLevelMoved(this, topLevelScript);
        }

        internal IBlock TakeoutArg(InvokationBlock parent, int i)
        {
            IBlock arg = parent.Args[i];
            parent.SetArg(i, Default(parent.ArgTypes[i]));
            return arg;
        }
    }
}
