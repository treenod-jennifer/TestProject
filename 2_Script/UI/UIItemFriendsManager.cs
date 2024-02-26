using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIItemFriendsManager : MonoBehaviour
{
    private Dictionary<int, List<UserFriend>> friendsData = new Dictionary<int, List<UserFriend>>();

    public IEnumerator Load()
    {
        this.friendsData.Clear();
        
        yield return LoadFriendsData();

        List<UserBase> friendsData = null;
        friendsData = SDKGameProfileManager._instance.GetAllPlayingFriendList();
        friendsData.Sort((a, b) => { return (int)(a.Flower - b.Flower); });

        foreach (var friend in friendsData)
        {
            var f = friend as UserFriend;
            if (f != null)
                AddFriendData(friend.stage, f);
            //Debug.Log($"friend : {friend._profile.name}, stage : {friend.stage}, flower : {friend._profile.scoreData.scoreValue}\nURL : {friend._profile.pictureUrl}");
        }

        UpdateObserver();
    }

    public List<UserFriend> GetFriends(int stageIndex)
    {
        if(friendsData.TryGetValue(stageIndex, out List<UserFriend> friends))
        {
            return friends;
        }

        return null;
    }

    private void OnDestroy()
    {
    }

    private IEnumerator LoadFriendsData()
    {
        List<string> userKeys = SDKGameProfileManager._instance.GetPlayingFriendsKeys();
        yield return SDKGameProfileManager._instance.GetAllProfileList
        (
            (List<Profile_PION> profileData) =>
            {
                int length = profileData.Count;
                for (int i = 0; i < length; i++)
                {
                    if (SDKGameProfileManager._instance.TryGetPlayingFriend(profileData[i].userKey, out UserFriend userData))
                    {
                        userData.SetPionProfile(profileData[i]);
                        profileData[i].profile.SetSDKGameProfileRequestData(userData);
                    }
                }
            },
            userKeys.ToArray()
        );
    }

    private void AddFriendData(int stage, UserFriend friendsData)
    {
        if (friendsData == null) return;

        if (this.friendsData.ContainsKey(stage))
        {
            this.friendsData[stage].Add(friendsData);
        }
        else
        {
            this.friendsData.Add(stage, new List<UserFriend>() { friendsData });
        }
    }

    #region 옵저버 관련

    public interface FriendsManagerObserver
    {
        void UpdateFriend();
    }

    private HashSet<FriendsManagerObserver> observers = new HashSet<FriendsManagerObserver>();

    public void AddObserver(FriendsManagerObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void DeleteObserver(FriendsManagerObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    private void UpdateObserver()
    {
        foreach(var observer in observers)
        {
            observer.UpdateFriend();
        }
    }
    #endregion
}
