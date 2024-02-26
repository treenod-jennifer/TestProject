using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GameFramework
{
    public static class Log
    {
        [Conditional("DEBUG")]
        public static void MethodName()
        {
            var stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            Log.Debug("[{0}::{1}]", methodBase.ReflectedType.Name, methodBase.Name);
        }

        [Conditional("DEBUG")]
        public static void MethodName(UnityEngine.Object context)
        {
            var stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            Log.Debug("[{0}::{1}] {2}", methodBase.ReflectedType.Name, methodBase.Name, context.name);
        }

        public static void Trace(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        public static void Trace(UnityEngine.Object context, string message)
        {
            UnityEngine.Debug.Log(message, context);
        }
        public static void Trace(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
        public static void Trace(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, format, args);
        }

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(UnityEngine.Object context, string message)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        [Conditional("DEBUG")]
        public static void Debug(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, format, args);
        }

        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        public static void Warning(UnityEngine.Object context, string message)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }
        public static void Warning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }
        public static void Warning(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
        public static void Error(UnityEngine.Object context, string message)
        {
            UnityEngine.Debug.LogError(message, context);
        }
        public static void Error(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }
        public static void Error(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, UnityEngine.Object context, string message)
        {
            UnityEngine.Debug.Assert(condition, message, context);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string format, params object[] args)
        {
            UnityEngine.Debug.AssertFormat(condition, format, args);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.AssertFormat(condition, context, format, args);
        }

        public static void Exception(System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
        public static void Exception(UnityEngine.Object context, System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex, context);
        }
    }
}
