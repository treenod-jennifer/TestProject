using System;
using System.Collections.Generic;
using System.Linq;
using Trident;
using UnityEngine;

public class LocalNotificationInfo
{
    public int index;
    public LOCAL_NOTIFICATION_TYPE type;
    public long expiredTime;

    public int GetPushSec()
    {
        return (int)(expiredTime - GameData.GetTime());
    }
}

public static class LocalNotificationRepos
{
    private static int notificationIndex;
    private static Dictionary<string, LocalNotificationInfo> notificationDictionary;

    private static string CreateKey(LOCAL_NOTIFICATION_TYPE type, long index)
    {
        return Enum.GetName(typeof(LOCAL_NOTIFICATION_TYPE), (int) type) + "_" + index;
    }

    static LocalNotificationRepos()
    {
        Reset();
    }

    public static void Reset()
    {
        notificationIndex = 0;
        notificationDictionary = new Dictionary<string, LocalNotificationInfo>();
    }

    public static void ClearExpired()
    {
        List<string> removes = new List<string>();
        for (var i = 0; i < notificationDictionary.Count; i++)
        {
            var key = notificationDictionary.Keys.ToList()[i];
            var info = notificationDictionary[key];

            if (info.expiredTime <= GameData.GetTime())
            {
                removes.Add(key);
            }
        }

        for (var i = 0; i < removes.Count; i++)
        {
            notificationDictionary.Remove(removes[i]);
        }
    }

    public static Dictionary<string, LocalNotificationInfo> LocalNotificationInfos
    {
        get { return notificationDictionary; }
    }

    public static void AddLocalNotification(LOCAL_NOTIFICATION_TYPE type, long index, int time)
    {
        var key = CreateKey(type, index);
        //TODO expire 된 목록 삭제
        ClearExpired();

        LocalNotificationInfo info = new LocalNotificationInfo
        {
            index = ++notificationIndex, 
            type = type, 
            expiredTime = GameData.GetTime() + time
        };

        //TODO 중복 입력 처리
        notificationDictionary[key] = info;
        
//        Debug.Log("SetNotificationIndex : " + notificationDictionary[key].index);
    }

    public static void RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE type, long index)
    {
        var key = CreateKey(type, index);
        notificationDictionary.Remove(key);
    }

}
