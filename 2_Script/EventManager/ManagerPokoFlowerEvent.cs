using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPokoFlowerEvent : MonoBehaviour, IEventBase
{
    public static ManagerPokoFlowerEvent instance = null;
    
    public const string SCN_KEY = "PFlowerEv_Scn";
    public static int currentPokoFlowerCount { get { return ServerRepos.UserPokoFlowerEvent.progress; } }

    public static int targetPokoFlowerCount { get { return ServerRepos.UserPokoFlowerEvent.target; } }

    public static int PokoFlowerEventIndex { get { return ServerContents.PokoFlowerEvent.event_index; } }

    public static long PokoFlowerEventEndTs { get { return ServerContents.PokoFlowerEvent.end_ts; } }

    public static int PokoFlowerEventResourceIndex { get { return ServerContents.PokoFlowerEvent.resource_index; } }

    public static int PokoFlowerEventAcievedCount { get { return ServerRepos.UserPokoFlowerEvent.achievedCount; } }

    public static int PokoFlowerEventMaxRewardCount { get { return ServerContents.PokoFlowerEvent.max_reward_count; } }

    public static Reward PokoflowerEventReward { get { return ServerContents.PokoFlowerEvent.reward; } }

    void Awake()
    {
        instance = this;
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerPokoFlowerEvent>();
        if (instance == null)
            return; // PANIC: 윗줄에서 awake 불려야되는데?!
        ManagerEvent.instance.RegisterEvent(instance);
        

    }

    public static bool CheckStartable()
    {
        return IsRunningPokoFlowerEvent();
    }

    /// <summary>
    /// 개편된 에코피 이벤트 클리어 체
    /// </summary>
    /// <returns></returns>
    public static bool CheckPokoFlowerEventClear()
    {
        bool pokoflowerEventClear = currentPokoFlowerCount == targetPokoFlowerCount;

        if (pokoflowerEventClear)
            return true;
        else
            return false;
    }
    /// <summary>
    /// 개편된 에코피 이벤트가 진행 중 일 때
    /// </summary>
    /// <returns></returns>
    public static bool IsRunningPokoFlowerEvent()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        var pokoFlowerEvent = ServerContents.PokoFlowerEvent;

        if (pokoFlowerEvent == null || pokoFlowerEvent.event_index == 0)
            return false;
        else        
            return Global.LeftTime(pokoFlowerEvent.end_ts) > 0;
    }


    /// <summary>
    /// 인게임 내에서 에코피 이벤트가 진행중인지 확인할 때 사용.
    /// </summary>
    public static bool IsRunningPokoFlowerEvent_Ingame()
    {
        if (EditManager.instance != null)
            return false;

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        var pokoFlowerEvent = ServerContents.PokoFlowerEvent;
        if (pokoFlowerEvent == null || pokoFlowerEvent.event_index == 0)
            return false;

        //게임 시작을 누른 타이밍에 에코피 이벤트가 종료되었는지 검사.
        if (ServerRepos.GameStartTs >= pokoFlowerEvent.end_ts)
            return false;
        
        return true;
    }

    public static bool IsRewardExhausted()
    {
        var pokoFlowerEvent = ServerContents.PokoFlowerEvent;
        if (pokoFlowerEvent == null || pokoFlowerEvent.event_index == 0)
            return true;

        if( ServerRepos.UserPokoFlowerEvent.achievedCount >= pokoFlowerEvent.max_reward_count)
        {
            return true;
        }
        return false;
    }

    public static List<UserFriend> SendFriendList()
    {
        List<UserFriend> requestPossible = new List<UserFriend>();

        foreach (var key in SDKGameProfileManager._instance.GetAllPlayingFriendKeys())
        {
            if (SDKGameProfileManager._instance.TryGetPlayingFriend(key, out UserFriend user))
            {
                //오늘 아이템을 보내준 친구는 제외
                if (ServerRepos.UserPokoFlowerEvent.sentUsers.Contains(user._userKey)) continue;

                requestPossible.Add(user);
            }
        }

        return requestPossible;
    }

    //연출 부분도 추가 예정

    public static int CalcEventLevel()
    {
        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        // 하늘꽃 이상이 하나도 없는 유저
        if ( ManagerData._instance._stageData.Exists( stg => stg._flowerLevel >= 4) == false )
        {
            return 1;
        }

        // 하늘꽃 이상이 있긴 한데, 빨간꽃은 하나도 없는 유저
        if (ManagerData._instance._stageData.Exists(stg => stg._flowerLevel >= 5) == false)
        {
            return 2;
        }

        // 빨간꽃이 있는 유저
        return 3;
    }

	public static int CalcStageBtnLevel()
    {
        if(ManagerData._instance._stageData.Exists(stg => stg._flowerLevel < 5) == true)
        {
            if(ManagerData._instance._stageData.Exists(stg => stg._flowerLevel < 4) == true)
            {
                if(ManagerData._instance._stageData.Exists(stg => stg._flowerLevel < 3) == true)
                {
                    //흰꽃을 피운 스테이지가 없을 때
                    return 1;
                }
                //흰꽃은 다 피우고 파란 꽃은 다 피우지 않았을 때
                return 2;
            }
            //파란 꽃은 다 피웠지만, 빨간 꽃은 다 피우지 않았을 때
            return 3;
        }
        //전부 다 빨간꽃인 유저
        return 0;
    }
	public static int EventLevelChanged()    
	{
        string key = $"{ServerContents.PokoFlowerEvent.event_index}_{ManagerPokoFlowerEvent.CalcEventLevel()}";
        if( PlayerPrefs.HasKey(SCN_KEY) == false || PlayerPrefs.GetString(SCN_KEY).StartsWith($"{ServerContents.PokoFlowerEvent.event_index}_") == false )
        {   //기록이 없거나, 이벤트번호가 바뀐 경우
            return 1;
        }
        else if(PlayerPrefs.GetString(SCN_KEY) != key)
        {
            return 2;
        }

        return 0;
    }

    internal static bool ShowLobbyInScene()
    {
        if (ManagerArea._instance.GetEventLobbyObject(GameEventType.POKOFLOWER) != null && ServerContents.PokoFlowerEvent != null)
        {
            var userBlossomData = ServerRepos.UserPokoFlowerEvent;

            if (userBlossomData != null)
            {
                if (ManagerPokoFlowerEvent.EventLevelChanged() > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    internal static void SetPlayCutscene()
    {
        string key = $"{ServerContents.PokoFlowerEvent.event_index}_{ManagerPokoFlowerEvent.CalcEventLevel()}";

        PlayerPrefs.SetString(ManagerPokoFlowerEvent.SCN_KEY, key);

    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.POKOFLOWER;
    }

    void IEventBase.OnLobbyStart()
    {
    }
    
    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        string assetName = "pokoflower_event";
        string prefabName = "pokoflower_";
        var eventUserData = ServerRepos.UserPokoFlowerEvent;
        if (eventUserData != null)
        {
            // 흰꽃 3/ 파란꽃 4 / 빨간꽃 5인데 리소스는 1,2,3이니까 -2해준다
            prefabName += ManagerPokoFlowerEvent.CalcEventLevel().ToString();
        }

        GameObject obj = null;
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path = "Assets/5_OutResource/pokoflower_event/" + prefabName + ".prefab";
            Debug.Log($"pokoflower {path}");
            GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            obj = ManagerLobby.NewObject(BundleObject);

            AreaBase areaBase = BundleObject.GetComponent<AreaBase>();
            if (areaBase)
            {
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
            }
#endif
        }
        else
        {
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject objn = assetBundle.LoadAsset<GameObject>(prefabName);
                    obj = ManagerLobby.NewObject(objn);
                    AreaBase areaBase = obj.GetComponent<AreaBase>();
                    if (areaBase)
                    {
                        ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                    }
                }
            }
        }

        yield return ManagerCharacter._instance.LoadCharacters();

        if (obj != null)
        {
            PokoFlowerEventAreaBase pfEvent = obj.GetComponent<PokoFlowerEventAreaBase>();
            ManagerArea._instance.RegisterEventLobbyObject(pfEvent);

            GameObject ui = Instantiate<GameObject>(ManagerLobby._instance._objEventObjectUI);
            ui.transform.parent = pfEvent._touchTarget.transform;
            ui.transform.localPosition = new Vector3(0.2f, 0.2f, -0.2f);
            pfEvent._uiEvent = ui.GetComponent<EventObjectUI>();
            pfEvent.SetProgressCount();
        }
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        var ec = ManagerPokoFlowerEvent.EventLevelChanged();
        ManagerPokoFlowerEvent.SetPlayCutscene();
        yield return ManagerLobby._instance.WaitForSceneEnd(ManagerArea._instance.GetEventLobbyObject(GameEventType.POKOFLOWER).GetAreaBase(), ec);

    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconPokoFlowerEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.PokoFlowerEvent));
        }
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
        ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ServerContents.PokoFlowerEvent.event_index));
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return  ManagerPokoFlowerEvent.ShowLobbyInScene();
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
        {
            string assetName = "pokoflower_event";
            ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
            {
                uri = assetName,
                successCallback = null,
                failCallback = (r) =>
                {
                    Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                    failCallback(r, $"{assetName}\n");
                }
            };
            loadList.Add(bundleReq);
        }
    }
}
