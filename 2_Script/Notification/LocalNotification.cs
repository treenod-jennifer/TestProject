using System;
using System.Collections.Generic;
using UnityEngine;


public class LocalNotification
{
    public const string SMALL_ICON_NAME = "ic_notify";
    public const string UNITY_MAINACTIVITY = "com.linecorp.trident.unity.TridentUnityPlayerActivity";

    public static string notificationCustomFile = "hehe";
    public static long customNotificationEndTs = 0;

    static LocalNotification()
    {
        Reset();
    }

    public static void Reset()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        Treenod.iOS.LocalNotificationHelper.Registration();
        #endif

        LocalNotificationRepos.Reset();
    }
    
    private static int GetCloverFullChargeTime()
    {
        if (GameData.User.AllClover >= GameData.MaxClover)
        {
            return 0;
        }
        
        //TODO Clover가 MaxClover가 되는 시간 계산
        //TODO ts 가 갱신이 안됬을 경우에 - 값 나오는 부분 처리 
        long diff = GameData.GetTime() - GameData.User.CloverTs;
        int time = (GameData.MaxClover - GameData.User.AllClover - 1) * GameData.CloverChargeSec + GameData.CloverChargeSec - (int)diff;
        
//        Debug.Log("Clover Charge Time : " + time);

        return time;
    }

    public static void CloverNotification()
    {
        try {
            int time = GetCloverFullChargeTime();
            if (time <= 0) {
                LocalNotificationRepos.RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE.CLOVER,
                    (int) LOCAL_NOTIFICATION_TYPE.CLOVER);
                return;
            }

            LocalNotificationRepos.AddLocalNotification(LOCAL_NOTIFICATION_TYPE.CLOVER,
                (int) LOCAL_NOTIFICATION_TYPE.CLOVER, time);
        }
        catch (System.Exception e) {
//            Debug.LogWarning("CloverNotification Ex: " + e.ToString());
        }
    }

    public static void TimeMissionNotification(long mission, int time)
    {
        try
        {
            LocalNotificationRepos.AddLocalNotification(LOCAL_NOTIFICATION_TYPE.TIME_MISSION, mission, time);
        }
        catch (Exception e)
        {
//            Debug.LogWarning("TimeMissionNotification Ex: " + e.ToString());
        }
    }

    public static void GiftBoxNotification(long index, int time)
    {
        try
        {
            LocalNotificationRepos.AddLocalNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, index, time);
        }
        catch (Exception e)
        {
//            Debug.LogWarning("GiftBoxNotification Ex: " + e.ToString());
        }
    }

    public static void FriendGiftNotification(long index, int time)
    {
        try
        {
            LocalNotificationRepos.AddLocalNotification(LOCAL_NOTIFICATION_TYPE.FRIEND_GIFT, index, time);
        }
        catch (Exception e)
        {
            //            Debug.LogWarning("GiftBoxNotification Ex: " + e.ToString());
        }
    }

    public static void RegisterNotifications()
    {
        try
        {
            LocalNotificationRepos.ClearExpired();
            CloverNotification();

            if (!CheckUseNotification())
            {
                return;
            }

            foreach (var info in LocalNotificationRepos.LocalNotificationInfos)
            {
                if (CheckUseNotificationType(info.Value.type))
                {
                    if( Global._instance.HasString(GetPushMessageKey(info.Value.type)) )
                    {
                        RegisterNotification(info.Value.type, info.Value.index, info.Value.GetPushSec(),
                        Global._instance.GetString(GetPushMessageKey(info.Value.type)), true, GetNotificationSoundName());
                    }
                }
            }
        }
        catch (Exception e)
        {
//            Debug.LogWarning("RegisterNotifications Ex: " + e.ToString());
        }
    }

    public static string GetNotificationSoundName()
    {
        if( LocalNotification.customNotificationEndTs != 0  &&
            Global.LeftTime(LocalNotification.customNotificationEndTs) < 0)
        {   // 타임아웃이 세팅되어있는 경우에만 타임아웃 체크
            return "hehe";
        }

        if ( LocalNotification.notificationCustomFile.Length != 0)
            return LocalNotification.notificationCustomFile;

        return "hehe";
    }

    public static void RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE type, long index)
    {
        try
        {
            LocalNotificationRepos.RemoveLocalNotification(type, index);
        }
        catch (Exception e)
        {
//            Debug.LogWarning("RemoveLocalNotification Ex: " + e.ToString());
        }
    }

    private static string GetPushMessageKey(LOCAL_NOTIFICATION_TYPE type)
    {
        int index = (int) type + 1;
        return "l_p_" + index;
    }

    private static bool CheckUseNotification()
    {
        //0 = true, 1 = false
        if (PlayerPrefs.GetInt("_optionLocalPush") == 1)
        {
            return false;
        }

        if (PlayerPrefs.GetInt("_optionNotPushNightTime") == 0)
        {
            DateTime today = DateTime.Today;
            long localOffset = DateTimeOffset.Now.Offset.Ticks / 10000000;
            long now = GameData.GetTime() + localOffset;
            
            
            long hour22 = ConvertToUnixTimestamp(today.AddHours(22));
            long hour24 = ConvertToUnixTimestamp(today.AddDays(1));
            if (now > hour22 && now <= hour24)
            {
                return false;
            }
            
            long hour0 = ConvertToUnixTimestamp(today);
            long hour8 = ConvertToUnixTimestamp(today.AddHours(8));
            if (now >= hour0 && now < hour8)
            {
                return false;
            }
        }

        return true;
    }
    
    static long ConvertToUnixTimestamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan diff = date - origin;
        return (long)Math.Floor(diff.TotalSeconds);
    }

    private static bool CheckUseNotificationType(LOCAL_NOTIFICATION_TYPE type)
    {
        switch (type)
        {
            case LOCAL_NOTIFICATION_TYPE.CLOVER:
                return PlayerPrefs.GetInt("_optionPushFullClover") == 0;
            case LOCAL_NOTIFICATION_TYPE.TIME_MISSION:
                return PlayerPrefs.GetInt("_optionPushMissionComplete") == 0;
            case LOCAL_NOTIFICATION_TYPE.GIFT_BOX:
                return PlayerPrefs.GetInt("_optionPushGiftBox") == 0;
            case LOCAL_NOTIFICATION_TYPE.EVENT_INFO:
                return PlayerPrefs.GetInt("_optionPushEventInfo") == 0;
            case LOCAL_NOTIFICATION_TYPE.FRIEND_GIFT:
                return PlayerPrefs.GetInt("_optionPushFriendGift") == 0;
            default:
                return false;
        }
    }
    
    //iOS Type 처리에 대해서 확인이 필요함
    public static void RegisterNotification(LOCAL_NOTIFICATION_TYPE type, int index, int time, string contents, bool AOS_vibrate = true, string soundName = null)
    {
        //        Debug.Log("LOCAL_NOTIFICATION_TYPE : " + Enum.GetName(typeof(LOCAL_NOTIFICATION_TYPE), (int)type));
        //        Debug.Log("NotificationIndex : " + index);
        //        Debug.Log("Time : " + time);

#if UNITY_IOS && !UNITY_EDITOR
        if (soundName != null)
        {
            soundName += ".wav";
        }
        Treenod.iOS.LocalNotificationHelper.Add(Enum.GetName(typeof(LOCAL_NOTIFICATION_TYPE), (int)type), index, DateTime.Now.AddSeconds(time), contents, soundName);
#elif UNITY_ANDROID && !UNITY_EDITOR
        Treenod.Android.LocalNotificationHelper.Add(index, Enum.GetName(typeof(LOCAL_NOTIFICATION_TYPE), (int)type), TimeSpan.FromSeconds(time), Global._instance.GetString("l_p_4"), contents, soundName, AOS_vibrate, SMALL_ICON_NAME, UNITY_MAINACTIVITY);
#endif
    }

    public static void CancelAllNotification()
    {
        try
        {
//            Debug.Log("Cancel All Notification");
            #if UNITY_IOS && !UNITY_EDITOR
            Treenod.iOS.LocalNotificationHelper.CancelAll();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            Treenod.Android.LocalNotificationHelper.CancelAll();
            #endif
        }
        catch (Exception e)
        {
//            Debug.LogWarning("CancelAllNotification Ex: " + e.ToString());
        }

    }
}
