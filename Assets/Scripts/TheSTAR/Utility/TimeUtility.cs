using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class TimeUtility
    {
        public static void Wait(float timeSeconds, Action action) => Wait((int)(timeSeconds * 1000), action);

        public async static void Wait(int timeMilliseconds, Action action)
        {
            await Task.Run(() => Task.Delay(timeMilliseconds));
            action?.Invoke();
        }

        public static TimeCycleControl DoWhile(WaitWhileCondition condition, float timeSeconds, Action action) => DoWhile(condition, (int)(timeSeconds * 1000), action);

        public static TimeCycleControl DoWhile(WaitWhileCondition condition, int timeMilliseconds, Action action)
        {
            action?.Invoke();
            return While(condition, timeMilliseconds, action);
        }

        public static TimeCycleControl While(WaitWhileCondition condition, int timeMilliseconds, Action action)
        {
            TimeCycleControl control = new ();
            WaitWhile(condition, timeMilliseconds, action, control);
            return control;
        }

        private static void WaitWhile(WaitWhileCondition condition, int timeMilliseconds, Action action, TimeCycleControl control)
        {
            if (control.IsBreak) return;

            Wait(timeMilliseconds, () =>
            {
                if (control.IsBreak) return;

                action?.Invoke();

                if (!condition.Invoke()) return;

                WaitWhile(condition, timeMilliseconds, action, control);
            });
        }

        private enum CycleStatus
        {
            Alive,
            Breaked
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