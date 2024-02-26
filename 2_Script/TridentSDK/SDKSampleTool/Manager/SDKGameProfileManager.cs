using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Trident;
using JsonFx.Json;
using System;
using System.Linq;

public class SDKGameProfileManager : MonoBehaviour 
{
    public static SDKGameProfileManager _instance = null;

    public static void Initialized()
    { 
        if (_instance == null)
        {
            GameObject go = new GameObject("SDKGameProfileManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SDKGameProfileManager>();
        }
    }

    public static void DestroySelf()
    {
        Destroy(_instance.gameObject);
        _instance = null;
    }
    
    public enum FriendFilter
    {
        LINE_FRIEND = 1 << 1,
        GAME_FRIEND = 1 << 2,
        ALL_FRIEND = LINE_FRIEND | GAME_FRIEND
    }

    public int GameFriendMaximumCount { get { return _friendSetting == null ? -1 : _friendSetting.friendMax; } }

    // 내 프로필
    private UserSelf userData = new UserSelf();
    //친구 정보 리스트(게임하는 친구)
    private Dictionary<string, UserFriend> _playingLineFriendList = new Dictionary<string, UserFriend>();

    //친구 정보 리스트(게임하는 친구)
    private Dictionary<string, UserFriend> _playingGameFriendList = new Dictionary<string, UserFriend>();

    //친구 정보 리스트(게임을하지 않는 친구)
    private Dictionary<string, UserFriend> _nonPlayLineFriendList = new Dictionary<string, UserFriend>();
    //유저 프로필 데이터
    private Dictionary<string, Profile_PION> _dicUserProfileInfo = new Dictionary<string, Profile_PION>();
    //게임친구 리스트
    private Dictionary<string, Profile_PION> _gameFriendsData = new Dictionary<string, Profile_PION>();
    //게임친구 설정 정보
    private PION_FriendSetting _friendSetting = null;
    
    
    public void InitData ()
    {
        // 에디터 환경용 더미 초기화 
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (NetworkSettings.Instance.useDebugAuthMode)
            {
                userData.InitDummyData();
                userData.token = NetworkSettings.Instance.debugToken;
                
                var p = new ServiceSDK.UserProfileData();
                p.userKey = NetworkSettings.Instance.debugUserKey;
                p.name = NetworkSettings.Instance.debugUserName;
                userData.SetTridentProfile(p);
            }
            else
            {
                userData.InitDummyData();
                
                var p = new ServiceSDK.UserProfileData();
                p.InitProfileDummyData();
                userData.SetTridentProfile(p);
            }
        }
    }

    public void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }


    #region 내 프로필

    public void LoadMyProfile(Action callbackHandler)
    {
        ServiceSDK.ServiceSDKManager.instance.GetMyProfile((bool result, ServiceSDK.UserProfileData profile) =>
        {
            Extension.PokoLog.Log("============OnLoadMyProfile ");
            ServiceSDK.UserProfileData myProfile = null;
            if (result)
            {
                myProfile = profile;
            }
            else
            {
                myProfile = new ServiceSDK.UserProfileData();
                myProfile.userKey = ServiceSDK.ServiceSDKManager.instance.GetUserKey();
            }
            userData.SetTridentProfile(myProfile);
            userData.SetUserData();
            PrintProfile(myProfile);
            Extension.PokoLog.Log("토큰 " + ServiceSDK.ServiceSDKManager.instance.GetAccessToken());
            
            userData.token = ServiceSDK.ServiceSDKManager.instance.GetAccessToken();
            DelegateHelper.SafeCall(callbackHandler);
        });
    }

    public void SetGuestUserProfile()
    {
        var myProfile = new ServiceSDK.UserProfileData
        {
            name = "GUEST USER",
            pictureUrl = "",
            userKey = ServiceSDK.ServiceSDKManager.instance.GetUserKey()
        };
        
        userData.SetTridentProfile(myProfile);
        userData.token = ServiceSDK.ServiceSDKManager.instance.GetAccessToken();
    }

    public UserSelf GetMyProfile()
    {
        return this.userData;
    }

    #endregion


    #region 게임 하는 친구

    public IEnumerator LoadPlayingLineFriendList(Action callbackHandler)
    {
        Extension.PokoLog.Log("============LoadGameFriendList ");
        int currentCount = 0;
        int total = 0;
        Dictionary<string, ServiceSDK.UserProfileData> friendDic = new Dictionary<string, ServiceSDK.UserProfileData>();
        bool err = false;
        do
        {
            bool ret = false;
            
            ServiceSDK.ServiceSDKManager.instance.GetGameFriends(currentCount, (bool result, int t, List<ServiceSDK.UserProfileData> profileList) =>
            {
                if (result == true)
                {
                    for (int i = 0; i < profileList.Count; i++)
                    {
                        friendDic[profileList[i].userKey] = profileList[i];
                    }
                    currentCount = friendDic.Count;
                }
                else
                {
                    err = true;
                }
                total = t;
                ret = true;
            });

            yield return new WaitUntil(() => ret == true);
        }
        while (currentCount < total && err == false);

        Extension.PokoLog.Log("Loaded Game Friend Profile");
        List<ServiceSDK.UserProfileData> userDataList = new List<ServiceSDK.UserProfileData>(friendDic.Values);

        if (_playingLineFriendList.Count != 0)
        {
            _playingLineFriendList.Clear();
        }

        foreach (var item in userDataList)
        {
            UserFriend data = new UserFriend();
            data.SetTridentProfile(item);
            data.SetUserData();
            _playingLineFriendList.Add(data._userKey, data);
        }

        this.PrintProfile(userDataList);

        DelegateHelper.SafeCall(callbackHandler);
    }

    public List<string> GetPlayingLineFriendKeys()
    {
        return _playingLineFriendList.Keys.ToList();
    }

    public List<string> GetPlayingFriendsKeys()
    {
        var keys = new List<string> (_playingLineFriendList.Keys.ToList());
        keys.AddRange(_playingGameFriendList.Keys.ToList());
        return keys;
    }

    public List<UserFriend> GetPlayingLineFriendList()
    {
        return _playingLineFriendList.Values.ToList();
    }

    public bool TryGetPlayingLineFriend(string key, out UserFriend user)
    {
        return _playingLineFriendList.TryGetValue(key, out user);
    }

    public int GetPlayingLineFriendsCount()
    {
        return _playingLineFriendList.Count;
    }

    #endregion


    #region 게임을 하지 않는 친구

    public IEnumerator LoadNonPlayLineFriendList(Action callbackHandler)
    {
        Extension.PokoLog.Log("============LoadGameInviteFriendList ");
        int currentCount = 0;
        int total = 0;
        Dictionary<string, ServiceSDK.UserProfileData> nonGameFriendDic = new Dictionary<string, ServiceSDK.UserProfileData>();

        bool err = false;
        do {
            bool ret = false;
            
            ServiceSDK.ServiceSDKManager.instance.GetNonGameFriends(currentCount, (bool result, int t, List<ServiceSDK.UserProfileData> profileList) => 
            {
                if (result)
                {
                    for (int i = 0; i < profileList.Count; i++)
                    {
                        nonGameFriendDic[profileList[i].providerKey] = profileList[i];
                    }

                    currentCount = nonGameFriendDic.Count;
                }
                else
                {
                    err = true;
                }
                ret = true;
                total = t;
            });
            yield return new WaitUntil(() => ret == true);
        }
        while (currentCount < total && err == false);

        Extension.PokoLog.Log("============LoadGameInviteFriendList - GetRecommendedNonGameFriends ");
        bool retRecommend = false;
        //추천친구 정렬을위해 추천친구 받아오기
        ServiceSDK.ServiceSDKManager.instance.GetRecommendedNonGameFriends((bool result, List<string> providerKeyList) => 
        {
            retRecommend = true;

            if (providerKeyList == null || nonGameFriendDic == null)
            {
            }
            else
            {
                string log = "======recommend : ";
                int count = providerKeyList.Count;
                int orderNo = count;
                for (int i = 0; i < count; i++)
                {
                    if (nonGameFriendDic.ContainsKey(providerKeyList[i]))
                    {
                        nonGameFriendDic[providerKeyList[i]].recommendOrderNo = orderNo;
                        log += providerKeyList[i] + ",";
                    }

                    orderNo--;
                }
            }
            
        });

        yield return new WaitUntil(() => retRecommend == true);

        Extension.PokoLog.Log("Loaded NonGame Friend Profile");
        List<ServiceSDK.UserProfileData> userDataList = new List<ServiceSDK.UserProfileData>(nonGameFriendDic.Values);

        if (_nonPlayLineFriendList.Count != 0)
        {
            _nonPlayLineFriendList.Clear();
        }

        foreach (var item in userDataList)
        {
            UserFriend data = new UserFriend();
            data.SetTridentProfile(item);
            data.SetUserData();
            _nonPlayLineFriendList.Add(item.providerKey, data);
        }

        this.PrintProfile(userDataList);
        DelegateHelper.SafeCall(callbackHandler);
    }

    public List<string> GetNonPlayLineFriendsKeys()
    {
        return _nonPlayLineFriendList.Keys.ToList();
    }

    public List<UserFriend> GetNonPlayLineFriendList()
    {
        return _nonPlayLineFriendList.Values.ToList();
    }

    public bool TryGetNonPlayLineFriend(string key, out UserFriend user)
    {
        return _nonPlayLineFriendList.TryGetValue(key, out user);
    }

    public int GetNonPlayLineFriendsCount()
    {
        return _nonPlayLineFriendList.Count;
    }

    #endregion


    #region 유저 프로필

    public IEnumerator GetAllProfileList(System.Action<List<Profile_PION>> callbackHandler, params string[] userKeys)
    {
        yield return GetProfileList(callbackHandler, userKeys);
    }

    public void UpdateGameProfileData(Profile_PIONCustom data, System.Action<Profile_PIONCustom> callbackHandler)
    {
        StartCoroutine(UpsertProfile(data, callbackHandler));
    }

    private static IEnumerator GetProfileList(System.Action<List<Profile_PION>> callbackHandler, string[] userKeys)
    {
        const int PROFILE_QUERY_LIMIT = 200;
        string    key                 = "";
        int       keyCount            = 0;

        List<Profile_PION> respList = new List<Profile_PION>();

        // string[] userKey값 세팅해줌
        for (int i = 0; i < userKeys.Length; i++)
        {
            key += userKeys[i];
            keyCount++;

            if (keyCount >= PROFILE_QUERY_LIMIT)
            {
                yield return GetProfileList_Partial(respList, key);

                key      = "";
                keyCount = 0;
                continue;
            }

            if (i != userKeys.Length - 1)
            {
                key += ",";
            }
            else
            {
                yield return GetProfileList_Partial(respList, key);
                break;
            }
        }

        DelegateHelper.SafeCall(callbackHandler, respList);
    }

    /// <summary>
    /// 전체 프로필 리스트를 받아온다.
    /// </summary>
    private static IEnumerator GetProfileList_Partial(List<Profile_PION> respList, string key)
    {
        bool isComplete = false;

        ServerAPI.GetProfileList(ServiceSDK.ServiceSDKManager.instance.GetAccessToken(), key, resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                respList.AddRange(resp.data);
                isComplete = true;
            }
            else if(resp.IsFailTridentAPI)
            {
                // 에러 발생 후 그대로 진행할 경우 스테이지 데이터 설정 부분에서 NullReferenceException이 발생하기 때문에 Reboot 처리 추가.
                TridentAPIErrorController.OpenErrorPopupAndReboot(resp.lineStatusCode, resp.error);
            }
        });

        yield return new WaitUntil(() => isComplete);
    }

    // gte(이상) lte(이하)
    public static IEnumerator UpsertProfile(Profile_PIONCustom profileData, System.Action<Profile_PIONCustom> callbackHandler)
    {
        var profile = new SDKGameProfileRequestJsonData(profileData);
        var keyProfileString = JsonWriter.Serialize(profile);

        bool isComplete = false;
        ServerAPI.UpsertProfile(keyProfileString, resp =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.data.profile);
            }
            else if(resp.IsFailTridentAPI)
            {
                // API 통신 중 에러 발생 시 경고 팝업만 출력.
                TridentAPIErrorController.OpenErrorPopup(resp.lineStatusCode, resp.error);
                DelegateHelper.SafeCall(callbackHandler, null);
            }
            isComplete = true;
        });

        yield return new WaitUntil(() => isComplete);
    }

    public void AddUsersProfile(string key, Profile_PION profileInfo)
    {
        if (SDKGameProfileManager._instance.TryGetPlayingLineFriend(key, out UserFriend f))
        {
            _dicUserProfileInfo.Remove(key);
            _dicUserProfileInfo.Add(key, profileInfo);
            f.SetPionProfile(profileInfo);
        }

        if (SDKGameProfileManager._instance.TryGetNonPlayLineFriend(key, out UserFriend f2))
        {
            _dicUserProfileInfo.Remove(key);
            _dicUserProfileInfo.Add(key, profileInfo);
            f2.SetPionProfile(profileInfo);
        }

        if (SDKGameProfileManager._instance.TryGetPlayingGameFriendList(key, out UserFriend f3))
        {
            _gameFriendsData.Remove(key);
            _gameFriendsData.Add(key, profileInfo);
            f3.SetPionProfile(profileInfo);
        }

        if (userData._userKey == key)
        {
            userData.SetPionProfile(profileInfo);
        }
    }

    bool TryGetValueToUsersProfile(string key, out Profile_PION user)
    {
        return _dicUserProfileInfo.TryGetValue(key, out user);
    }

    #endregion


    #region 게임친구

    public class PION_FriendSetting
    {
        public int friendMax;
        public int receiveRequestMax;
        public int sendRequestMax;
    }

    public class PION_GameFriendSendReq : Profile_PION
    {
        public string friendRequestId;
        public long time;
    }

    public class PION_SendGameFriendReq
    {
        public string status;
        public string friendRequestId;
    }

    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class PION_FailInfo
    {
        private class PION_ErrorInfo
        {
            public string code;
            public string message;
        }

        [Newtonsoft.Json.JsonProperty]
        private PION_ErrorInfo error;

        public PION_FailInfo(string code, string message)
        {
            error         = new PION_ErrorInfo();
            error.code    = code;
            error.message = message;
        }

        public string Code    { get { return error.code; } }
        public string Message { get { return error.message; } }
    }

    public static bool IsGameFriendActive 
    { 
        get 
        {
            if (ServerRepos.LoginCdn == null) return true;

            return ServerRepos.LoginCdn.GameFriendVer != 0; 
        } 
    }

    /// <summary>
    /// 친구 설정 확인
    /// </summary>
    private static IEnumerator GetGameFriendSystemSetting(Action<PION_FriendSetting> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.GetGameFriendsSystemSetting(ServiceSDK.ServiceSDKManager.instance.GetAccessToken(), resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke(resp.data);
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            var testData = new PION_FriendSetting();
            testData.friendMax = 15;
            testData.receiveRequestMax = 15;
            testData.sendRequestMax = 15;

            completeHandler?.Invoke(testData);
            yield break;
        }
    }

    /// <summary>
    /// 게임친구 리스트 가져오기
    /// </summary>
    private static IEnumerator GetGameFriendList(Action<List<Profile_PION>> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.GetGameFriendList(ServiceSDK.ServiceSDKManager.instance.GetAccessToken(),resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke(resp.data.list);
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            List<Profile_PION> list = new List<Profile_PION>();

            const int testFriendCount = 7;
            for (int i = 0; i < testFriendCount; i++)
            {
                Profile_PION testFriend = new Profile_PION
                {
                    userKey = $"T0CG0000000{i}",
                    profile =
                    {
                        name = $"Test[{i + 1}]"
                    }
                };
                list.Add(testFriend);
            }

            completeHandler?.Invoke(list);
        }
    }

    /// <summary>
    /// (포코친구) 추천친구 리스트
    /// </summary>
    public static IEnumerator GetGameFriendSuggestion(Action<List<Profile_PION>> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.GetGameFriendSuggestion(resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke(resp.data.list);
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            var list = new List<Profile_PION>();

            const int testFriendCount = 15;
            for (int i = 0; i < testFriendCount; i++)
            {
                Profile_PION testFriend = new Profile_PION
                {
                    userKey = $"Test({i + 1})",
                    profile =
                    {
                        name = $"Test({i + 1}) : {UnityEngine.Random.Range(0, 100)}"
                    }
                };
                list.Add(testFriend);
            }

            completeHandler?.Invoke(list);
        }
    }

    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public static IEnumerator SendGameFriendRequest(string userKey, Action<PION_SendGameFriendReq> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            
            Protocol.SendGameFriendRequestResp requestResp = null;
            ServerAPI.SendGameFriendRequest(userKey, resp =>
            {
                requestResp = resp;
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);

            if (requestResp.IsSuccessTridentAPI)
            {
                completeHandler?.Invoke(requestResp.data);
                SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.REQ_GAME_FRIEND, userKey);
                
                yield return CheckAndAddSendGameFriendList(requestResp.data);
            }
            else
            {
                failHandler?.Invoke(new PION_FailInfo(requestResp.error, requestResp.message));
            }
        }
        else
        {
            failHandler?.Invoke(null);
            yield break;
        }
    }

    // 보냈던 친구신청 목록 조회
    private static IEnumerator GetSentGameFriendRequests(Action<List<PION_GameFriendSendReq>> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.GetSentGameFriendRequests(resp =>
            {
                if (resp.IsSuccessTridentAPI)
                { 
                    completeHandler?.Invoke(resp.data.list);
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            var list = new List<PION_GameFriendSendReq>();

            const int testFriendCount = 3;
            for (int i = 0; i < testFriendCount; i++)
            {
                PION_GameFriendSendReq testFriend = new PION_GameFriendSendReq
                {
                    userKey = $"Test[{i + 1}]",
                    profile =
                    {
                        name = $"Test[{i + 1}]"
                    }
                };

                list.Add(testFriend);
            }
            completeHandler?.Invoke(list);
        }
    }

    /// <summary>
    /// 최대 친구 신청 수
    /// </summary>
    private const int MAX_SEND_COUNT = 60;

    /// <summary>
    /// 친구 신청 목록에 대한 캐시처리
    /// </summary>
    private static List<PION_GameFriendSendReq> sendGameFriendList = null;

    /// <summary>
    /// 신청목록에 신청내역을 추가하고, 최대 신청수를 넘기면 오래된 신청부터 삭제 처리한다.
    /// </summary>
    /// <param name="addFriend">추가할 신청내역</param>
    /// <returns></returns>
    private static IEnumerator CheckAndAddSendGameFriendList(PION_SendGameFriendReq addFriend = null)
    {
        if (sendGameFriendList == null)
        {
            yield return GetSentGameFriendRequests
            (
                (request) =>
                {
                    sendGameFriendList = request;
                },
                OpenPopupFailInfo
            );

            if (sendGameFriendList == null) yield break;
        }
        else
        {
            if (addFriend != null)
            {
                var data = new PION_GameFriendSendReq
                {
                    friendRequestId = addFriend.friendRequestId,
                    time            = Global.GetTime()
                };
                sendGameFriendList.Add(data);
            }
        }

        int deleteIndex = 0;
        int deleteCount = sendGameFriendList.Count - MAX_SEND_COUNT;
        for (int i = 0; i < deleteCount; i++)
        {

            yield return CancelSentGameFriendRequest(sendGameFriendList[deleteIndex].friendRequestId, 
                () => 
                {
                    sendGameFriendList.RemoveAt(deleteIndex);
                },
                (failInfo) =>
                {
                    deleteIndex++;
                }
            );
        }
    }

    // 보냈던 친구신청 취소
    private static IEnumerator CancelSentGameFriendRequest(string friendRequestId, Action completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.CancelSentGameFriendRequest(friendRequestId, resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke();
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            completeHandler?.Invoke();
            yield break;
        }
    }

    /// <summary>
    /// 나에게 친구 요청을 한 리스트
    /// </summary>
    public static IEnumerator GetIncomingGameFriendRequests(Action<List<PION_GameFriendSendReq>> completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.GetIncomingGameFriendRequests(resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke(resp.data.list);
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            var list = new List<PION_GameFriendSendReq>();

            const int testFriendCount = 3;
            for (int i = 0; i < testFriendCount; i++)
            {
                PION_GameFriendSendReq testFriend = new PION_GameFriendSendReq
                {
                    userKey = $"Test[{i + 1}]",
                    profile =
                    {
                        name = $"Test[{i + 1}]"
                    }
                };

                list.Add(testFriend);
            }

            completeHandler?.Invoke(list);
        }
    }

    public static IEnumerator CheckIncomingFriendExist(Action<bool> completeHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.CheckIncomingFriendExist(resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke(resp.data.total > 0);
                }
                else if(resp.IsFailTridentAPI)
                {
                    completeHandler?.Invoke(false);
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            completeHandler?.Invoke(true);

            yield break;
        }
    }

    /// <summary>
    /// 친구 요청 거부
    /// </summary>
    public static IEnumerator DeclineGameFriendRequest(string friendRequestId, Action completeHandler = null, Action<PION_FailInfo> failHandler = null)
    {
        if (NetworkSettings.Instance.IsRealDevice() || (NetworkSettings.Instance.authPlaform == NetworkSettings.AuthPlatforms.Auth_Line && NetworkSettings.Instance.useDebugAuthMode))
        {
            bool isComplete = false;
            ServerAPI.DeclineGameFriendRequest(friendRequestId, resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    completeHandler?.Invoke();
                }
                else if(resp.IsFailTridentAPI)
                {
                    failHandler?.Invoke(new PION_FailInfo(resp.error, resp.message));
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        else
        {
            completeHandler?.Invoke();
        }
    }

    public static void SavePokoFriendCount()
    {
        if (NetworkSettings.Instance.IsRealDevice() == false)
            return;

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
        Profile_PION profileData = myProfile.GetPionProfile();
        if (profileData != null)
        {
            if ( profileData.profile.pokoFriendCount != SDKGameProfileManager._instance.GetGameFriendsCount() )
            {
                profileData.profile.pokoFriendCount = SDKGameProfileManager._instance.GetGameFriendsCount();
                SDKGameProfileManager._instance.UpdateGameProfileData(profileData.profile, (r) => { });
            }
        }
    }

    /// <summary>
    /// 게임 친구 설정 초기화(login.php 통신 전 호출)
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitGameFriendData()
    {
        yield return LoadGameFriendSettingInfo();
        yield return LoadGameFriendList();
    }
    
    /// <summary>
    /// 게임친구 설정 정보 로드
    /// </summary>
    private IEnumerator LoadGameFriendSettingInfo()
    {
        if (!IsGameFriendActive) yield break;

        yield return GetGameFriendSystemSetting
        (
            (settingRequest) =>
            {
                _instance._friendSetting = settingRequest;
            },
            OpenPopupFailInfo
        );
    }

    /// <summary>
    /// 게임친구 리스트 로드
    /// </summary>
    public IEnumerator LoadGameFriendList()
    {
        if (!IsGameFriendActive) yield break;

        yield return GetGameFriendList(OnLoadGameFriendListComplete, OpenPopupFailInfo);
    }
    
    private void OnLoadGameFriendListComplete(List<Profile_PION> request)
    { 
        _gameFriendsData.Clear();
        _playingGameFriendList.Clear();

        foreach (var item in request)
        {
            _gameFriendsData.Add(item.userKey, item);
            
            var data = new UserFriend();
            data.SetPionProfile(item);
            data.SetUserData();
            _playingGameFriendList.Add(data._userKey, data);
        }
    }
    
    public List<string> GetGameFriendsKey()
    {
        return _gameFriendsData.Keys.ToList();
    }

    public List<Profile_PION> GetGameFriendsValue()
    {
        return _gameFriendsData.Values.ToList();
    }

    bool TryGetPlayingGameFriendList(string key, out UserFriend u)
    {
        return _playingGameFriendList.TryGetValue(key, out u);
    }

    public bool TryGetPlayingFriend(string key, out UserFriend u)
    {
        if (SDKGameProfileManager._instance.TryGetPlayingLineFriend(key, out UserFriend f))
        {
            u = f;
            return true;
        }        

        if (IsGameFriendActive && SDKGameProfileManager._instance.TryGetPlayingGameFriendList(key, out UserFriend f3))
        {
            u = f3;
            return true;
        }
        
        u = null;
        return false;
    }

    bool TryGetValueToGameFriends(string key, out Profile_PION user)
    {
        return _gameFriendsData.TryGetValue(key, out user);
    }

    public int GetGameFriendsCount()
    {
        return _gameFriendsData.Count;
    }

    public int GetPlayingFriendsCount()
    {
        return GetPlayingLineFriendsCount() + GetGameFriendsCount();
    }

    public List<string> GetAllPlayingFriendKeys()
    {
        List<string> keys = new List<string>(_playingLineFriendList.Keys.ToList());

        if (IsGameFriendActive)
        {
            keys.AddRange(_playingGameFriendList.Keys.ToList());
        }

        return keys;
    }

    public List<UserBase> GetAllPlayingFriendList()
    {
        List<UserBase> userList = new List<UserBase>(_playingLineFriendList.Values.ToList());

        if (IsGameFriendActive)
        {
            foreach (var f in _playingGameFriendList)
            {
                if (!_playingLineFriendList.ContainsKey(f.Key))
                {
                    userList.Add(f.Value);
                }
            }
        }

        //if( !userList.Exists( x => x._userKey != this.userData._userKey) )
        //{
        //    userList.Add(userData);
        //}
        return userList;
    }

    public bool TryGetPIONProfile(string key, out Profile_PION pionProfile )
    {
        if( this.userData._userKey == key )
        {
            pionProfile = this.userData.GetPionProfile();
            return true;
        }

        if (TryGetValueToUsersProfile(key, out pionProfile))
            return true;

        if (TryGetValueToGameFriends(key, out pionProfile))
            return true;

        return false;
    }

    /// <summary>
    /// 삭제 유저관리
    /// </summary>
    public static class DeleteUser
    {
        private const string SAVE_KEY = "DELETE_USER_QUEUE";
        private const int DELETE_USER_MAX_COUNT = 30;

        private static Queue<string> userQueue;

        private static Queue<string> UserQueue
        {
            get
            {
                if (userQueue == null)
                {
                    if (PlayerPrefs.HasKey(SAVE_KEY))
                    {
                        string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                        userQueue = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<string>>(jsonData);
                    }
                    else
                    {
                        userQueue = new Queue<string>(DELETE_USER_MAX_COUNT);
                    }
                }

                return userQueue;
            }
        }

        private static void Save()
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(UserQueue);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
        }

        private static void Update()
        {
            int deleteCount = UserQueue.Count - DELETE_USER_MAX_COUNT;

            for (int i = 0; i < deleteCount; i++)
            {
                UserQueue.Dequeue();
            }
        }

        public static void Add(string userKey)
        {
            UserQueue.Enqueue(userKey);

            Update();
            Save();
        }

        public static string[] Array
        {
            get
            {
                Update();

                return UserQueue.ToArray();
            }
        }
    }
    #endregion


    //---------------------------------------------------------------------------
    public static void SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN rsn, string frnMid, string detail = "")
    {
        if (SDKGameProfileManager._instance == null)
            return;

        //그로씨
        var socialLog = new ServiceSDK.GrowthyCustomLog_Social (
            SDKGameProfileManager._instance.userData.stage.ToString(),
            rsn,
            frnMid, 
            detail);
        var doc = Newtonsoft.Json.JsonConvert.SerializeObject(socialLog);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
    }

    //---------------------------------------------------------------------------
    private void PrintProfile(List<ServiceSDK.UserProfileData> userDataList)
    {
        int count = userDataList.Count;

        for (int i = 0; i < count; i++)
        {
            PrintProfile(userDataList[i]);
        }
    }

    private void PrintProfile(ServiceSDK.UserProfileData userData)
    {
        Extension.PokoLog.Log("=========================");
        Extension.PokoLog.Log("userKey : " + userData.userKey + ", name : " + userData.name + ", pictureUrl : " + userData.pictureUrl + ", provederkey : " + userData.providerKey + ", providerId: " + userData.providerId.ToString());
    }
    
    public static void OpenPopupFailInfo(PION_FailInfo failInfo)
    {
        string failText;

        if (failInfo == null)
        {
            failText = "Unknown Error";
        }
        else
        {
            failText = $"{failInfo.Code}";

            if (!string.IsNullOrEmpty(failInfo.Message) && !string.IsNullOrWhiteSpace(failInfo.Message))
            {
                failText += $"\n{failInfo.Message}";
            }
        }

        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) =>
            {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: failText,
                    useButtons: false
                );
            }
        );
    }
}
