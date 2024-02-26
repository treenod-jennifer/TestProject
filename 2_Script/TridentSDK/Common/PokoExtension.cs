using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public static class Poko
    {
        // Action 클래스에 대하여 new를 하지 않으며 사용하는 기법이 필요.
        // new에 대한 GC 부담이 커짐.
        public static void ForeachFast<T>(this IList<T> datas, Action<T> a)
        {
            int count = datas.Count;
            for (int i = 0; i < count; ++i)
            {
                a(datas[i]);
            }
        }

        public static void ForeachChild(this Transform form, Action<Transform> a)
        {
            int count = form.childCount;
            for (int i = 0; i < count; ++i)
            {
                a(form.GetChild(i));
            }
        }

        
    }

    public static class PokoLog
    {
        private const bool WRITE_LOG_FLAG = false;

        public static void Log(string log)
        {
            if (WRITE_LOG_FLAG)
            {
                Debug.Log(log);
            }
        }
    }
}

