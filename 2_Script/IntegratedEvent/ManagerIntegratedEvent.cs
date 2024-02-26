using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerIntegratedEvent : MonoBehaviour, IEventBase
{
    public static ManagerIntegratedEvent Instance { get; private set; }

    private static bool showIntegratedEventPopup;
    private List<Protocol.IntegratedBannerInfo> bannerInfos = new List<Protocol.IntegratedBannerInfo>();
   

    public static bool CanShowIntegratedEventPopup()
    {
        return showIntegratedEventPopup == false && GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen;
    }
    
    public static void Init()
    {
        if (Instance != null)
        {
            return;
        }

        if (!CheckStartable())
        {
            return;
        }
        
        Global._instance.gameObject.AddMissingComponent<ManagerIntegratedEvent>();

        if (Instance == null)
        {
            return;
        }

        ManagerEvent.instance.RegisterEvent(Instance);
    }
    
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    public IEnumerator CoOpenIntegratedEventPopup()
    {
        yield return StartCoroutine(CoRequestIntegratedEventData());

        if (Instance == null)
        {
            yield break;
        }

        List<IntegratedEventInfo> eventBannerInfos = new List<IntegratedEventInfo>();
        foreach (var banner in bannerInfos)
        {
            eventBannerInfos.Add(new IntegratedEventInfo(banner));
        }
        eventBannerInfos.RemoveAll(banner => banner.enable == false);
        
        if (eventBannerInfos.Count <= 0)
        {
            yield break;
        }
        
        //로비 진입 튜토리얼이 진행중일 경우 대기.
        yield return new WaitUntil(() => ManagerTutorial._instance == null);
        
        // 로비 연출이 끝나기 전 로비 오브젝트 클릭을 진행할 경우 팝업이 겹쳐 출력되는 이슈가 있어 팝업 카운트가 0이 아닐 경우 대기하도록 수정.
        yield return new WaitUntil(() => ManagerUI._instance != null && ManagerUI._instance._popupList.Count == 0 && ManagerUI._instance.isOpeningEventPopupCount == 0);
        
        // 로비 연출이 끝나기 전 로비 오브젝트를 통해 인게임으로 진입할 경우 팝업이 출력되지 않도록 예외처리 추가.
        if (ManagerLobby._stageStart)
        {
            yield break;
        }
        
        ClickBlocker.Make(0.3f);
        var popup = ManagerUI._instance.OpenPopup<UIPopupIntegratedEvent>();
        popup.InitData(eventBannerInfos);
        showIntegratedEventPopup = true;
        
        yield return new WaitUntil(() => UIPopupIntegratedEvent._instance == null);
    }

    private IEnumerator CoRequestIntegratedEventData()
    {
        bool isComplete = false;

        bannerInfos.Clear();
        ServerAPI.RequestIntegratedEventData((resp) =>
        {
            if (resp.IsSuccess)
            {
                bannerInfos = resp.bannerInfos;
            }

            isComplete  = true;
        });
        
        yield return new WaitUntil(() => isComplete == true);
    }

    private static bool CheckStartable()
    {
        return ServerRepos.LoginCdn.integratedEventOn;
    }
    
    public GameEventType GetEventType()
    {
        return GameEventType.INTEGRATED_EVENT;
    }

    #region Manager

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList,
        Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public void OnLobbyStart()
    {
    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public void OnIconPhase()
    {
    }

    public void OnReboot()
    {
        showIntegratedEventPopup = false;
        
        if (Instance != null)
        {
            Destroy(Instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    #endregion
}
