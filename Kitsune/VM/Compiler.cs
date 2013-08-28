using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public class Compiler
    {
        int labelCount;
        int varCount;
        private BlockSpace blockSpace;
        private VM vm;
        public Dictionary<string, string> PrimitiveAliases = new Dictionary<string, string>();
        public Compiler(VM vm, BlockSpace blockSpace)
        {
            this.blockSpace = blockSpace;
            this.vm = vm;
            labelCount = varCount = 0;
        }
        public Method Compile(IBlock b, bool mainProgram)
        {
            List<Instruction> instructions = new List<Instruction>();
            Dictionary<string, int> labels = new Dictionary<string,int>();
            CompileExpression(b, DataType.Script, instructions, labels);

            if (mainProgram)
                instructions.Add(new Stop(vm));
            Method ret = new Method();
            ret.Instructions = instructions.ToArray();
            ret.Arity = 0;
            ret.PrepareLabels();
            return ret;
        }

        private void CompileExpression(IBlock b, DataType type, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            if (b is BlockStack)
            {
                CompileBlockStack(b as BlockStack, instructions, labels);
            }
            else if (b is InvokationBlock)
            {
                CompileInvokationBlock(b as InvokationBlock, type, instructions, labels);
            }
            else if (b is TextBlock)
            {
                CompileTextBlock(b as TextBlock, type, instructions, labels);
            }
        }

        private void CompileTextBlock(TextBlock b, DataType type, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            switch (type)
            {
                case DataType.Text:
                    instructions.Add(new Push(vm, b.Text));
                    break;
                case DataType.Number:
                    instructions.Add(new Push(vm, Double.Parse(b.Text)));
                    break;
                case DataType.Object:
                    instructions.Add(new Push(vm, b.Text));
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void CompileInvokationBlock(InvokationBlock b, DataType type, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            if(b.Text == "if % then % else %")
            {
                CompileIfBlock(b, instructions, labels);
                return;
            }
            else if (b.Text == "forever %")
            {
                CompileForeverBlock(b, instructions, labels);
                return;
            }
            else if (b.Text == "repeat % times %")
            {
                CompileRepeatBlock(b, instructions, labels);
                return;
            }
            else if (b.Text == "wait % milliseconds")
            {
                CompileWaitBlock(b, instructions, labels);
                return;
            }
            
            DataType[] argTypes = blockSpace.blockInfos[b.Text].ArgTypes;
            for (int i = argTypes.Length-1; i >=0 ; --i)
            {
                CompileExpression(b.Args[i], argTypes[i], instructions, labels);
            }
            instructions.Add(GenerateInvokation(b.Text, argTypes.Length));
            if (type == DataType.Script)
                instructions.Add(new Discard(vm));
        }

        private void CompileWaitBlock(InvokationBlock b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            CompileExpression(b.Args[0], DataType.Number, instructions, labels);
            instructions.Add(new Wait(vm));
        }

        private void CompileRepeatBlock(InvokationBlock b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            string counter = MakeVar();
            string doLabel = MakeLabel();
            string exitLabel = MakeLabel();
            
            CompileExpression(b.Args[0], DataType.Number, instructions, labels);
            instructions.Add(new PopLocal(vm, counter));
            instructions.Add(new Label(vm, doLabel));
            instructions.Add(new PushLocal(vm, counter));
            instructions.Add(new JumpIfNot(vm, exitLabel));
            CompileExpression(b.Args[1], DataType.Script, instructions, labels);
            instructions.Add(new PushLocal(vm, counter));
            instructions.Add(new ApplyPrim(vm, 1, args=>(double) args[0] - 1.0));
            instructions.Add(new PopLocal(vm, counter));
            instructions.Add(new Jump(vm, doLabel));
            instructions.Add(new Label(vm, exitLabel));
        }

        private void CompileIfBlock(InvokationBlock b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            string label1 = MakeLabel();
            string label2 = MakeLabel();
            
            CompileExpression(b.Args[0], DataType.Number, instructions, labels);
            instructions.Add(new JumpIfNot(vm, label1));
            
            CompileExpression(b.Args[1], DataType.Script, instructions, labels);
            instructions.Add(new Jump(vm, label2));
            instructions.Add(new Label(vm, label1));
            CompileExpression(b.Args[2], DataType.Script, instructions, labels);
            instructions.Add(new Label(vm, label2));
        }

        private void CompileForeverBlock(InvokationBlock b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            string label1 = MakeLabel();

            instructions.Add(new Label(vm, label1));
            CompileExpression(b.Args[0], DataType.Script, instructions, labels);
            instructions.Add(new Jump(vm, label1));
        }

        private Instruction GenerateInvokation(string p, int arity)
        {
            if (PrimitiveAliases.ContainsKey(p))
                return new ApplyPrim(vm, arity,(Func<object[], object>) vm.GetPrimitive(PrimitiveAliases[p]));
            throw new ArgumentException("Can't find function " + p);
        }

        private void CompileBlockStack(BlockStack b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            foreach (IBlock s in b)
            {
                CompileExpression(s, DataType.Script, instructions, labels);
            }
        }

        string MakeLabel()
        {
            return string.Format("Label{0}", labelCount++);
        }

        string MakeVar()
        {
            return string.Format("Var{0}", varCount++);
        }
    }
}
