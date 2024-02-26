using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InviteTimeComparer : IComparer<UserFriend>
{
    public int Compare(UserFriend a, UserFriend b)
    {
        if ( a.canInvite && b.canInvite ) ////a, b 둘다 초대 x 인 상태 : 그대로.
        {
            if( a.GetTridentProfile() != null && b.GetTridentProfile() != null)
            {
                var tProfile_a= a.GetTridentProfile();
                var tProfile_b = b.GetTridentProfile();
                if ((tProfile_a.recommendOrderNo == 0 && tProfile_b.recommendOrderNo == 0) || tProfile_a.recommendOrderNo == tProfile_b.recommendOrderNo)
                {
                    return string.Compare(a.GameName, b.GameName);
                }

                return tProfile_a.recommendOrderNo > tProfile_b.recommendOrderNo ? -1 : 1;
            }
            else
            {
                return string.Compare(a.GameName, b.GameName);
            }
        }
        else if (a.canInvite && !b.canInvite ) //b 만 초대인 상태 : a 가 위(b는 아래로).
            return -1;
        else if (!a.canInvite && b.canInvite ) //a 만 초대인 상태 : a 가 아래(b는 위로).
            return 1;
        else if (!a.canInvite && !b.canInvite )  //a, b 둘다 초대인 상태 : 그대로.
            return 0;
        else
            return 0;
    }
}

public class WakeupFriendJsonData
{
    public long checkTime = 0;
    public int eventIdx;
    public List<string> listUserKey = new List<string>();

    public WakeupFriendJsonData(long time, int eventIdx, List<string> listKey)
    {
        this.checkTime = time;
        this.eventIdx = eventIdx;
        this.listUserKey = listKey;
    }
}

public class UIPopupInvite : UIPopupBase
{
    public static UIPopupInvite _instance = null;

    [SerializeField]
    private GameObject eventIcon;
    [SerializeField]
    private UIItemInviteTap[] tabs;

    private InviteTimeComparer inviteComparer = new InviteTimeComparer();


    private UIInviteFriendsTap inviteFriendsTap = null;
    private UIInviteFriendsTap InviteFriendsTap
    {
        get
        {
            if(inviteFriendsTap == null)
            {
                var obj = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIInviteFriendsTap", mainSprite.gameObject);
                inviteFriendsTap = obj.GetComponent<UIInviteFriendsTap>();
            }

            return inviteFriendsTap;
        }
    }
    
    private UIWakeupFriendsTap wakeupFriendsTap = null;
    private UIWakeupFriendsTap WakeupFriendsTap
    {
        get
        {
            if(wakeupFriendsTap == null)
            {
                var obj = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIWakeupFriendsTap", mainSprite.gameObject);
                wakeupFriendsTap = obj.GetComponent<UIWakeupFriendsTap>();
            }

            return wakeupFriendsTap;
        }
    }
    
    private UIGameFriendsTap gameFriendTab = null;
    private UIGameFriendsTap GameFriendTab 
    {
        get
        {
            if(gameFriendTab == null)
            {
                var obj = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIGameFriendsTap", mainSprite.gameObject);
                gameFriendTab = obj.GetComponent<UIGameFriendsTap>();
                gameFriendTab.SetDepth(uiPanel);

            }

            return gameFriendTab;
        }
    }

    private GameObject currentTapObj = null;

    //데이터
    private List<UserFriend> inviteFriendList = new List<UserFriend>();
    private List<UserFriend> wakeupFriendList = new List<UserFriend>();

    private bool isInviteRewardEvent = false;
    private bool isActiveWakeupEvent = false;

    //playerPrefs 키
    string wakeupKey = "WakeupFreinds";

    //하루 깨우기 가능한 최대 카운트
    private const int MAX_WAKEUP_COUNT = 5;
    public int MaxWakeupCount
    {
        get { return MAX_WAKEUP_COUNT; }
    }

    private struct WakeupData
    {
        long checkTime;
        List<UserBase> listWakeupFriends;
    }

    public enum TypePopupInvite
    {
        eInvite,
        eWakeup,
        eGame
    }
    TypePopupInvite inviteTapType = TypePopupInvite.eGame;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUpInvite()
    {
        bool hasFriendRequest = LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.friend] > 0;
        isInviteRewardEvent = ServerRepos.LoginCdn.PerInviteRewardEvent == 1;
        isActiveWakeupEvent = (ServerContents.WakeupEvent == null || ServerContents.WakeupEvent.event_index == 0) ? false : true;

        if (hasFriendRequest)
        {
            inviteTapType = TypePopupInvite.eGame;
            _callbackOpen += () => ManagerUI._instance.OpenPopup<UIPopupGameFriend>();
        }
        else if (isActiveWakeupEvent)
        {
            inviteTapType = TypePopupInvite.eWakeup;
        }
        else if(isInviteRewardEvent)
        {
            inviteTapType = TypePopupInvite.eInvite;
        }
        else
        {
            inviteTapType = TypePopupInvite.eGame;
        }

        tabs[2].gameObject.SetActive(SDKGameProfileManager.IsGameFriendActive);//게임친구 탭 On/Off
        InitInviteData();
        InitWakeupData();
        InitEvent();
        InitTap();
        SetInviteTap();
    }

    private void InitInviteData()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeInviteFriends();
        }
        else
        {
			inviteFriendList = SDKGameProfileManager._instance.GetNonPlayLineFriendList();
		}

        if (inviteFriendList.Count > 0)
            inviteFriendList.Sort(inviteComparer);
    }

    private void InitWakeupData()
    {
        if (isActiveWakeupEvent == false)
            return;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeWakeupFriends();
        }
        else
        {
            CreateWakeupList();
        }
    }


    void CreateWakeupList()
    {
        //유저 데이터 갱신이 필요한지 확인(키가 등록되어 있지 않거나, 친구 목록 갱신 시간이 지났는지 검사)
        if (PlayerPrefs.HasKey(wakeupKey) == true)
        {
            string jsonStr = PlayerPrefs.GetString(wakeupKey);
            WakeupFriendJsonData readData = JsonUtility.FromJson<WakeupFriendJsonData>(jsonStr);
            int eventIdx = ServerContents.WakeupEvent?.event_index ?? 0;


            //유저 데이터 갱신이 필요없는 경우에는, 현재 저장된 친구들 중 깨우기 불가능한 친구가 있는지 검사.
            if (readData.eventIdx == eventIdx && readData.checkTime == ServerRepos.UserWakeupEvent.timeStamp)
            {
                UpdateWakeupFriendsData(readData);
                return;
            }
        }

        //유저 데이터 갱신이 필요한 경우, 현재 친구 중, 깨우기 가능한 친구 가져옴.
        List<string> listTempkey = new List<string>();
        foreach(var user in SDKGameProfileManager._instance.GetPlayingLineFriendList())
        {
            if (user.CanWakeup)
            {
                listTempkey.Add(user._userKey);
            }
        }

        //CBU 대상 친구들 중 랜덤으로 몇명만 추려줌.
        int wakeupCnt = MaxWakeupCount - ServerRepos.UserWakeupEvent.sent_today;
        bool isRand = (listTempkey.Count > wakeupCnt) ? true : false;
        int checkCnt = (isRand == true) ? wakeupCnt : listTempkey.Count;

        //랜덤 사용할 떄, 리스트 셔플해줌.
        if (isRand == true)
        {
            GenericHelper.Shuffle(listTempkey);
        }

        wakeupFriendList.Clear();
        List<string> listSaveKey = new List<string>();
        for (int i = 0; i < checkCnt; i++)
        {
            if(SDKGameProfileManager._instance.TryGetPlayingLineFriend(listTempkey[i], out UserFriend user))
            {
                wakeupFriendList.Add(user);
                listSaveKey.Add(listTempkey[i]);
            }
        }
        UpdatePlayerPrefs(ServerRepos.UserWakeupEvent.timeStamp, listSaveKey);
    }

    public void RefreshWakeupList(List<UserBase> listUserData)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeWakeupFriends();
            return;
        }

        //유저 데이터 갱신이 필요한 경우, 현재 친구 중, 깨우기 가능한 친구 가져옴.
        List<string> listTempkey = new List<string>();
        List<string> listTempkey_back = new List<string>();

        foreach(var user in SDKGameProfileManager._instance.GetPlayingLineFriendList())
        {
            if (user.CanWakeup == true)
            {
                if (listUserData.Exists(x => x._userKey == user._userKey))
                {
                    listTempkey_back.Add(user._userKey);
                }
                else
                {
                    listTempkey.Add(user._userKey);
                }
            }
        }

        //CBU 대상 친구들 중 랜덤으로 몇명만 추려줌.
        int wakeupCnt = MaxWakeupCount - ServerRepos.UserWakeupEvent.sent_today;
        bool isRand = ((listTempkey.Count + listTempkey_back.Count) > wakeupCnt) ? true : false;
        int checkCnt = (isRand == true) ? wakeupCnt : (listTempkey.Count + listTempkey_back.Count);

        //랜덤 사용할 떄, 리스트 셔플해줌.
        if (isRand == true)
        {
            GenericHelper.Shuffle(listTempkey);
        }

        for(int i = 0; i < listTempkey_back.Count; i++)
        {
            listTempkey.Add(listTempkey_back[i]);
        }

        wakeupFriendList.Clear();
        List<string> listSaveKey = new List<string>();
        for (int i = 0; i < checkCnt; i++)
        {
            if(SDKGameProfileManager._instance.TryGetPlayingLineFriend(listTempkey[i], out UserFriend user))
            {
                wakeupFriendList.Add(user);
                listSaveKey.Add(listTempkey[i]);
            }
        }
        UpdatePlayerPrefs(ServerRepos.UserWakeupEvent.timeStamp, listSaveKey);

        return;
    }

    //저장되어 있는 친구들 중 가능한 친구들(깨우기 아직 안 보낸)의 데이터만 새로 갱신해줌.
    private void UpdateWakeupFriendsData(WakeupFriendJsonData readData)
    {
        wakeupFriendList.Clear();
        bool isChangeData = false;
        List<string> listSaveKey = new List<string>();
        for (int i = 0; i < readData.listUserKey.Count; i++)
        {
            string tempUserKey = readData.listUserKey[i];

            if(SDKGameProfileManager._instance.TryGetPlayingLineFriend(tempUserKey, out UserFriend user))
            {
                if (user.IsAlreadyWakeupSent == false)
                {
                    wakeupFriendList.Add(user);
                    listSaveKey.Add(readData.listUserKey[i]);
                }
                else
                {
                    isChangeData = true;
                }
            }
        }
        if(isChangeData == true)
            UpdatePlayerPrefs(readData.checkTime, listSaveKey);
    }

    private void UpdatePlayerPrefs(long time, List<string> listSaveKey)
    {
        int eventIdx = ServerContents.WakeupEvent?.event_index ?? 0;

        //PlayerPrefs에 현재 시간, 친구리스트 저장
        WakeupFriendJsonData saveData = new WakeupFriendJsonData(time, eventIdx, listSaveKey);
        string jsonString = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(wakeupKey, jsonString);
    }

    static public IEnumerator CoInitFriendProfile()
    {
        List<string> userKeys = SDKGameProfileManager._instance.GetPlayingLineFriendKeys();
        yield return SDKGameProfileManager._instance.GetAllProfileList
        (
            (List<Profile_PION> profileData) =>
            {
               int length = profileData.Count;
               for (int i = 0; i < length; i++)
               {
                   if(SDKGameProfileManager._instance.TryGetPlayingLineFriend(profileData[i].userKey, out UserFriend userData))
                   {
                        userData.SetPionProfile(profileData[i]);
                        profileData[i].profile.SetSDKGameProfileRequestData(userData);
                   }
               }
           }, userKeys.ToArray());

        yield break;
    }

    private void InitEvent()
    {
        eventIcon.SetActive(isInviteRewardEvent || isActiveWakeupEvent);
    }

    #region 더미 데이터
    private List<UserFriend> GetDummyFriends()
    {
        List<UserFriend> listTempData = new List<UserFriend>();
        {
            //친구정보 생성
            for (int i = 1; i < 60; i++)
            {
                if (i % 2 == 0)
                    continue;
                for (int d = 0; d < 3; d++)
                {
                    UserFriend fp = new UserFriend();
                    var p = new ServiceSDK.UserProfileData();
                    p.userKey = (100 + i).ToString();
                    p.name = "OREO" + Random.Range(1, 2000);
                    p.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
                    fp.SetTridentProfile(p);
                    listTempData.Add(fp);
                }
            }
        }

        // 친구
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.userKey = "1";
            p.name = "제아(Jea)";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            listTempData.Add(fp);
        }
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.userKey = "2";
            p.name = "Evie ㅇㅇㅈ";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            listTempData.Add(fp);
        }
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.userKey = "3";
            p.name = "안준호 (Jude)";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            listTempData.Add(fp);
        }
        return listTempData;
    }

    private void MakeInviteFriends()
    {
        inviteFriendList.Clear();
        inviteFriendList = GetDummyFriends();
    }

    private void MakeWakeupFriends()
    {
        List<UserFriend> listTempData = GetDummyFriends();
        List<string> listSaveKey = new List<string>();

        //유저 데이터 갱신이 필요한지 확인(키가 등록되어 있지 않거나, 친구 목록 갱신 시간이 지났는지 검사)
        if (PlayerPrefs.HasKey(wakeupKey) == true)
        {
            string jsonStr = PlayerPrefs.GetString(wakeupKey);
            WakeupFriendJsonData readData = JsonUtility.FromJson<WakeupFriendJsonData>(jsonStr);
            int eventIdx = ServerContents.WakeupEvent?.event_index ?? 0;

            //유저 데이터 갱신이 필요없는 경우에는, 현재 저장된 친구들 중 깨우기 불가능한 친구가 있는지 검사.
            if (eventIdx == readData.eventIdx && ServerRepos.UserWakeupEvent.timeStamp == readData.checkTime)
            {
                wakeupFriendList.Clear();
                bool isChangeData = false;
                for (int i = 0; i < readData.listUserKey.Count; i++)
                {
                    string tempUserKey = readData.listUserKey[i];
                    int findeIdx = listTempData.FindIndex(x => x._userKey == tempUserKey);
                    if (findeIdx != -1 && listTempData[findeIdx].CanWakeup == true)
                    {
                        wakeupFriendList.Add(listTempData[findeIdx]);
                        listSaveKey.Add(readData.listUserKey[i]);
                    }
                    else
                    {
                        isChangeData = true;
                    }
                }

                if (isChangeData == true)
                    UpdatePlayerPrefs(readData.checkTime, listSaveKey);
                return;
            }
        }

        //유저 데이터 갱신이 필요한 경우, 현재 친구 중, 깨우기 가능한 친구 가져옴.
        List<string> listTempkey = new List<string>();
        for (int i = 0; i < listTempData.Count; i++)
        {
            UserFriend uData = listTempData[i];
            if (uData.CanWakeup == true)
                listTempkey.Add(uData._userKey);
        }

        //CBU 대상 친구들 중 랜덤으로 몇명만 추려줌.
        int wakeupCnt = MaxWakeupCount - ServerRepos.UserWakeupEvent.sent_today;
        bool isRand = (listTempkey.Count > wakeupCnt) ? true : false;
        int checkCnt = (isRand == true) ? wakeupCnt : listTempkey.Count;

        //랜덤 사용할 떄, 리스트 셔플해줌.
        if (isRand == true)
        {
            GenericHelper.Shuffle(listTempkey);
        }

        wakeupFriendList.Clear();
        for (int i = 0; i < checkCnt; i++)
        {
            int findIndex = listTempData.FindIndex(x => x._userKey == listTempkey[i]);
            wakeupFriendList.Add(listTempData[findIndex]);
            listSaveKey.Add(listTempkey[i]);
        }
        UpdatePlayerPrefs(ServerRepos.UserWakeupEvent.timeStamp, listSaveKey);
    }
    #endregion

    private void InitTap()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].InitTap();
        }
    }

    private void OnClickinviteTap()
    {
        if (bCanTouch == false || inviteTapType == TypePopupInvite.eInvite)
            return;
        inviteTapType = TypePopupInvite.eInvite;
        SetInviteTap();
    }

    private void OnClickWakeupTap()
    {
        if (bCanTouch == false || inviteTapType == TypePopupInvite.eWakeup)
            return;
        inviteTapType = TypePopupInvite.eWakeup;
        SetInviteTap();
    }

    private void OnClickGameTap()
    {
        if (bCanTouch == false || inviteTapType == TypePopupInvite.eGame)
            return;
        inviteTapType = TypePopupInvite.eGame;
        SetInviteTap();
    }

    private void SetInviteTap()
    {
        for (int i=0; i< tabs.Length; i++)
        {
            if (tabs[i].SameFilter(inviteTapType))
                tabs[i].OnTab();
            else
                tabs[i].OffTab();
        }

        switch (inviteTapType)
        {
            case TypePopupInvite.eInvite:
                SetTap_InviteFriends();
                break;
            case TypePopupInvite.eWakeup:
                SetTap_WakeupFriends();
                break;
            case TypePopupInvite.eGame:
                SetTap_GameFriends();
                break;
        }
    }

    private void SetTap_InviteFriends()
    {
        if(currentTapObj != null)
        {
            currentTapObj.SetActive(false);
        }

        currentTapObj = InviteFriendsTap.gameObject;
        currentTapObj.SetActive(true);

        InviteFriendsTap.InitTap(uiPanel.depth, uiPanel.sortingOrder, inviteFriendList.Count == 0);
    }

    private void SetTap_WakeupFriends()
    {
        if (currentTapObj != null)
        {
            currentTapObj.SetActive(false);
        }

        currentTapObj = WakeupFriendsTap.gameObject;
        currentTapObj.SetActive(true);

        WakeupFriendsTap.InitTap(wakeupFriendList);
    }

    private void SetTap_GameFriends()
    {
        if (currentTapObj != null)
        {
            currentTapObj.SetActive(false);
        }

        currentTapObj = GameFriendTab.gameObject;
        currentTapObj.SetActive(true);

        GameFriendTab.InitTap(SDKGameProfileManager._instance.GetGameFriendsValue());
    }

    public UserFriend GetInviteData(int index)
    {
        if (inviteFriendList.Count <= index || inviteFriendList[index] == null)
            return null;

        return inviteFriendList[index];
    }

    public int GetInviteFriendsCount()
    {
        return (inviteFriendList.Count - 1);
    }

    public void InviteFriend(UIItemInvite inviteItem)
    {
        if (inviteFriendsTap != null)
        {
            UIPopupInvite._instance.bCanTouch = false;
            inviteFriendsTap.InviteFriend(inviteItem);
        }
    }

    public void WakeupFriends()
    {
        if (PlayerPrefs.HasKey(wakeupKey) == true)
        {   
            string jsonStr = PlayerPrefs.GetString(wakeupKey);
            WakeupFriendJsonData readData = JsonUtility.FromJson<WakeupFriendJsonData>(jsonStr);
            UpdateWakeupFriendsData(readData);
            if (wakeupFriendsTap != null)
                wakeupFriendsTap.Repaint();
        }
        this.bCanTouch = true;
    }

    public void UpdateGameFriendList()
    {
        StartCoroutine(CoUpdateGameFriendList());
    }

    private IEnumerator CoUpdateGameFriendList()
    {
        NetworkLoading.MakeNetworkLoading();

        yield return SDKGameProfileManager._instance.LoadGameFriendList();

        NetworkLoading.EndNetworkLoading();

        gameFriendTab?.InitTap(SDKGameProfileManager._instance.GetGameFriendsValue());
    }
}
