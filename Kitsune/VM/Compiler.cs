﻿using System;
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

        
        public Method DefineMethod(ProcDefBlock b, BlockStack stack)
        {
            // 'stack' is a BlockStack whose first element (#0) is a ProcDefBlock, the same as 'b'

            Method ret = new Method();
            List<Instruction> instructions = new List<Instruction>();
            string[] argNames = b.GetArgNames();
            for (int i = 0; i < argNames.Length; ++i)
            {
                instructions.Add(new PopLocal(vm, argNames[i]));
            }
            for(int i=1; i<stack.Count; ++i)
                instructions.AddRange(CompileBlockToInstructions(stack[i], false));
            instructions.Add(new Push(vm, null));
            instructions.Add(new Ret(vm));
            ret.Instructions = instructions.ToArray();
            ret.Arity = argNames.Length;
            ret.PrepareLabels();
            return ret;
        }
        public Method Compile(IBlock b, bool mainProgram)
        {
            List<Instruction> instructions = CompileBlockToInstructions(b, mainProgram);
            Method ret = new Method();
            ret.Instructions = instructions.ToArray();
            ret.Arity = 0;
            ret.PrepareLabels();
            return ret;
        }

        private List<Instruction> CompileBlockToInstructions(IBlock b, bool mainProgram)
        {
            List<Instruction> instructions = new List<Instruction>();
            Dictionary<string, int> labels = new Dictionary<string, int>();
            CompileExpression(b, DataType.Script, instructions, labels);

            if (mainProgram)
                instructions.Add(new Stop(vm));
            return instructions;
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
            else if (b is VarAccessBlock)
            {
                CompileVarAccessBlock(b as VarAccessBlock, type, instructions, labels);
            }
        }

        private void CompileVarAccessBlock(VarAccessBlock var, DataType type, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            // todo: add type checking?
            instructions.Add(new PushLocal(vm, var.Name));
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
                case DataType.Boolean:
                    if (b.Text == "")
                        instructions.Add(new Push(vm, false));
                    else
                        compilerError(b, string.Format("Unexpected value for boolean: {0}", b.Text));
                    break;
                case DataType.Object:
                    instructions.Add(new Push(vm, b.Text));
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void compilerError(TextBlock b, string p)
        {
            
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
            else if (b.Text == "stop script")
            {
                CompileStopScriptBlock(b, instructions, labels);
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
            
            CompileExpression(b.Args[0], DataType.Boolean, instructions, labels);
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

        private void CompileStopScriptBlock(InvokationBlock b, List<Instruction> instructions, Dictionary<string, int> labels)
        {
            instructions.Add(new Stop(vm));
        }

        private Instruction GenerateInvokation(string p, int arity)
        {
            if (PrimitiveAliases.ContainsKey(p))
                return new ApplyPrim(vm, arity, (Func<object[], object>)vm.GetPrimitive(PrimitiveAliases[p]));
            else if (vm.HasMethod(p))
                return new Call(vm, vm.GetMethod(p));
            else
                return new CallNamed(vm, p);
            //throw new ArgumentException("Can't find function " + p);
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
