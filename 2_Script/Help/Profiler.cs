using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace GameFramework
{
    public class Profiler
    {
        static readonly Stopwatch ProfileTimer = new Stopwatch();
        static string ProfileName;
        public static void TraceBegin(string profileName = "")
        {
            ProfileTimer.Reset();
            ProfileTimer.Start();
            ProfileName = profileName;
        }

        public static long TraceEnd()
        {
            ProfileTimer.Stop();
            return ProfileTimer.ElapsedMilliseconds;
        }

        public static void TraceEndAndReport()
        {
            ProfileTimer.Stop();
            Log.Trace("{0} ({1} ms)", ProfileName, ProfileTimer.ElapsedMilliseconds);
        }
    }
}
