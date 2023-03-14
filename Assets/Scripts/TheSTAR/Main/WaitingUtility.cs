using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class WaitingUtility
{
    public static void Wait(float time, Action action) => Wait((int)(time * 1000), action);

    public async static void Wait(int time, Action action)
    {
        await Task.Run(() => Thread.Sleep(time));
        action?.Invoke();
    }
}