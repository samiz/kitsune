using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.VM
{
    public class VM
    {
        public bool Done = false;
        Dictionary<string, Method> methods = new Dictionary<string, Method>();
        Dictionary<string, Func<object[], object>> primitives = new Dictionary<string, Func<object[], object>>();
        
        Queue<Process> running = new Queue<Process>();
        List<Process> timerWaiting = new List<Process>();
        Process runningNow;

        public void RegisterPrimitve(string name, Func<object[], object> primitive)
        {
            primitives[name] = primitive;
        }

        internal Func<object[], object> GetPrimitive(string p)
        {
            return primitives[p];
        }

        internal void Stop()
        {
            Done = true;
        }
        public void Sleepify(Process p, long duration)
        {
            p.timeToWake = DateTime.Now.AddMilliseconds(duration).Ticks;
            p.State = ProcessState.Sleeping;
            // Put them in makeshift queue, where last element = front of the queue
            timerWaiting.InsertSorted(p, (p1, p2) => (int)(p2.timeToWake - p1.timeToWake));
        }

        internal Process LaunchProcess(Method m)
        {
            Process p = new Process();
            p.Call(m);
            running.Enqueue(p);
            
            return p;
        }

        internal void RunStep()
        {
            if (!schedule())
                return;
            runningNow.timeSlice = 8;
            runningNow.RunTimeslice();

            if (stillRunning(runningNow))
                running.Enqueue(runningNow);

        }

        private bool stillRunning(Process p)
        {
            return p.State == ProcessState.Running;
        }

        private bool schedule()
        {
            activateElapsedTimers();
            if (running.Count == 0)
                return false;
            runningNow = running.Dequeue();
            return true;
        }

        private void activateElapsedTimers()
        {
            int ntimer = timerWaiting.Count;
            
            if (ntimer != 0)
            {
                long qt = System.DateTime.Now.Ticks;
                int i = ntimer-1;
                while(i >= 0)
                {
                    Process proc = timerWaiting[i];
                    if (proc.timeToWake >= qt)
                    {
                        break; // If the smallest timeToWake is not to awaken yet, then the rest aren't ready either
                    }
                    proc.State = ProcessState.Running;
                    running.Enqueue(proc);
                    timerWaiting.RemoveAt(i);
                    i--;
                }
                
            }
            
        }
    }
}
