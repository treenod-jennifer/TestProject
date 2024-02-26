using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ProfileTextureManager
{
    private static Dictionary<string, List<IProfileTexture>> observerDic = new Dictionary<string, List<IProfileTexture>>();

    public interface IProfileTexture
    {
        string userKey { get; }
        void SetProfile(IUserProfileData pictureUrl);
    }
    public interface IUserProfileData
    {
        string _userKey { get; }
        string _pictureUrl { get; }
        string _alterPicture { get; }
    }

    public static void AddObserverDic(IProfileTexture observer)
    {
        if (observer == null || string.IsNullOrEmpty(observer.userKey)) return;

        if (observerDic.ContainsKey(observer.userKey))
        {
            if (observerDic[observer.userKey].Contains(observer))
                return;
            else
                observerDic[observer.userKey].Add(observer);
        }
        else
            observerDic.Add(observer.userKey, new List<IProfileTexture> { observer });
    }

    public static void RemoveObserverList(IProfileTexture observer)
    {
        if (observer == null || string.IsNullOrEmpty(observer.userKey)) return;

        if (observerDic.ContainsKey(observer.userKey))
        {
            observerDic[observer.userKey].Remove(observer);

            if (observerDic[observer.userKey].Count == 0)
            {
                observerDic.Remove(observer.userKey);
            }
        }
    }

    public static void SetProfileTextureUrl(IUserProfileData user)
    {
        foreach(var observer in observerDic[user._userKey])
        {
            observer.SetProfile(user);
        }
    }


    public static bool IsAlterPicture(string alterPicture)
    {
        return !string.IsNullOrEmpty(alterPicture) && !alterPicture.Equals("0");
    }
}
