using System;

public static class DelegateHelper
{
    public static void SafeCall(Action action)
    {
        if (action != null)
            action();
    }

    public static void SafeCall<T>(Action<T> action, T data)
    {
        if (action != null)
            action(data);
    }

    public static void SafeCall<T1, T2>(Action<T1, T2> action, T1 data1, T2 data2)
    {
        if (action != null)
            action(data1, data2);
    }

    public static void SafeCall<T1, T2, T3>(Action<T1, T2, T3> action, T1 data1, T2 data2, T3 data3)
    {
        if (action != null)
            action(data1, data2, data3);
    }

    public static void SafeCall<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 data1, T2 data2, T3 data3, T4 data4)
    {
        if (action != null)
            action(data1, data2, data3, data4);
    }

	 
}


