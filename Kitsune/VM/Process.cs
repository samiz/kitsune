using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public enum ProcessState
    {
        Running, Sleeping, Exited
    }
    public class Process
    {
        public ProcessState State;
        public Stack<object> operandStack = new Stack<object>();
        public Stack<Frame> callStack = new Stack<Frame>();
        public int timeSlice;
        public long timeToWake;

        public Process()
        {
            State = ProcessState.Running;
        }
        public void Call(Method method)
        {
            callStack.Push(new Frame(method));
        }

        internal void RunTimeslice()
        {
            while (timeSlice > 0)
            {
                timeSlice--;
                Frame f = callStack.Peek();
                Instruction i = f.NextInstruction();
                i.Run(this);
            }
        }
    }
}
