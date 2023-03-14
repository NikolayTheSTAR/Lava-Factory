using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class TimeUtility
    {
        public static void Wait(float time, Action action) => Wait((int)(time * 1000), action);

        public async static void Wait(int time, Action action)
        {
            await Task.Run(() => Thread.Sleep(time));
            action?.Invoke();
        }

        public static TimeCycleControl DoWhile(WaitWhileCondition condition, float time, Action action) => DoWhile(condition, (int)(time * 1000), action);

        public static TimeCycleControl DoWhile(WaitWhileCondition condition, int time, Action action)
        {
            action?.Invoke();
            return While(condition, time, action);
        }

        public static TimeCycleControl While(WaitWhileCondition condition, int time, Action action)
        {
            TimeCycleControl control = new ();
            WaitWhile(condition, time, action, control);
            return control;
        }

        private async static void WaitWhile(WaitWhileCondition condition, int time, Action action, TimeCycleControl control)
        {
            if (control.IsBreak) return;

            while (condition.Invoke())
            {
                await Task.Run(() => Thread.Sleep(time));
                if (control.IsBreak) return;
                action?.Invoke();
            }
        }

        public delegate bool WaitWhileCondition();
    }

    public class TimeCycleControl
    {
        public bool IsBreak
        {
            get;
            private set;
        }

        public void Stop() => IsBreak = true;
    }
}