using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    class Push : Instruction
    {
        private object value;
        public Push(VM vm, object value)
            :base(vm)
        {
            this.value = value;
        }
        public override void Run(Process p)
        {
            p.operandStack.Push(value);
        }
    }
    class PushLocal : Instruction
    {
        private string name;
        public PushLocal(VM vm, string name)
            : base(vm)
        {
            this.name= name;
        }
        public override void Run(Process p)
        {
            p.operandStack.Push(p.callStack.Peek().Locals[name]);
        }
    }
    class Discard : Instruction
    {
        // Just pop a value from the operand stack
        public Discard(VM vm)
            : base(vm)
        {
        }
        public override void Run(Process p)
        {
            p.operandStack.Pop();
        }
    }
    class PopLocal: Instruction
    {
        string name;
        public PopLocal(VM vm, string name)
            : base(vm)
        {
            this.name = name;
        }
        public override void Run(Process p)
        {
            object v = p.operandStack.Pop();
            p.callStack.Peek().Locals[name ] = v;
        }
    }
    class ApplyPrim : Instruction
    {
        private int arity;
        private Func<object[], object> primitive;
        public ApplyPrim(VM vm, int arity, Func<object[], object> primitive)
            : base(vm)
        {
            this.arity = arity;
            this.primitive = primitive;
        }
        public override void Run(Process p)
        {
            object[] args = new object[arity];
            for (int i = 0; i < arity; ++i)
            {
                args[i] = p.operandStack.Pop();
            }
            p.operandStack.Push(primitive(args));
        }
    }
    class Stop : Instruction
    {
        public Stop(VM vm) : base(vm) { }


        public override void Run(Process p)
        {
            p.timeSlice = 0;
            vm.Stop();
        }
    }
    class JumpIfNot : Instruction
    {
        private string label;

        public JumpIfNot(VM vm, string label) : base(vm)
        {
            this.label = label;
        }
        public override void Run(Process p)
        {
            int value = (int) (double) p.operandStack.Pop();
            if (value == 0)
            {
                Frame f = p.callStack.Peek();
                f.IP = f.Method.Labels[label];
            }
        }
    }
    class Jump : Instruction
    {
        private string label;

        public Jump(VM vm, string label)
            : base(vm)
        {
            this.label = label;
        }
        public override void Run(Process p)
        {
            Frame f = p.callStack.Peek();
            f.IP = f.Method.Labels[label];
        }
    }
    class Label : Instruction
    {
        public string label;

        public Label(VM vm, string label)
            : base(vm)
        {
            this.label = label;
        }
        public override void Run(Process p)
        {
            
        }
    }
    class Call : Instruction
    {
        public Method callee;

        public Call(VM vm, Method callee)
            : base(vm)
        {
            this.callee = callee;
        }
        public override void Run(Process p)
        {
             p.Call(callee);
        }
    }
    class Ret : Instruction
    {
        public Ret(VM vm)
            : base(vm)
        {

        }
        public override void Run(Process p)
        {
            p.callStack.Pop();
            if (p.callStack.Count == 0)
            {
                p.timeSlice = 0;
                p.State = ProcessState.Exited;
            }
        }
    }
    class Wait : Instruction
    {
        public Wait(VM vm)
            : base(vm)
        {

        }
        public override void Run(Process p)
        {
            double duration = (double) p.operandStack.Pop();
            vm.Sleepify(p, (long) duration);
            p.timeSlice = 0;    
        }
    }
}
