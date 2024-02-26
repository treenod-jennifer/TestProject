using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameFramework
{
    public class Profiler
    {
        static readonly Stopwatch ProfileTimer = new Stopwatch();
        static string ProfileName;
        static List<KeyValuePair<string, long> > lapTimes = new List<KeyValuePair<string, long>>();
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

        public static long TraceLap(string pos, 
            [CallerFilePath] string callerFile = "",  
            [CallerMemberName] string memberName = "", 
            [CallerLineNumber] int srcLineNumber = 0 )
        {
            var lastLap = lapTimes.Count > 0 ? lapTimes[lapTimes.Count - 1].Value : 0;            
            var now = ProfileTimer.ElapsedMilliseconds;
            lapTimes.Add(new KeyValuePair<string, long>(pos, now));

            
            var orgLogType = UnityEngine.Application.GetStackTraceLogType(UnityEngine.LogType.Log);
            UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Log, UnityEngine.StackTraceLogType.None);
            Log.Trace("{0} {1}: \tLap {2} \tTotal ({3} ms)\t - {4} {5}:{6}", ProfileName, pos, now - lastLap, now, callerFile, memberName, srcLineNumber);
            UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Log, orgLogType);
            return now;
        }

        public static void TraceEndAndReport()
        {
            ProfileTimer.Stop();
            Log.Trace("{0} ({1} ms)", ProfileName, ProfileTimer.ElapsedMilliseconds);
        }
    }
}
