using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManagerForceDisplayEvent : MonoBehaviour, IEventBase
{
    public enum ForceDisplayEventType
    {
        NONE = -1,
        EVENT_STAGE,
        BINGO_EVENT,
        TREASURE_HUNT,
        ATELIER,
        SPACE_TRAVEL,
    }
    
    public static ManagerForceDisplayEvent instance = null;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Init()
    {
        if (CheckStartable() == false)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerForceDisplayEvent>();
        if (instance == null)
        {
            return;
        }

        ManagerEvent.instance.RegisterEvent(instance);
    }
    
    public static bool CheckStartable()
    {
        if (ServerRepos.LoginCdn.forceDisplayOnOff          <= 0    ||
            ServerRepos.UserForceDisplayEvent              == null ||
            ServerRepos.UserForceDisplayEvent.Count == 0)
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 리워드 갱신
    /// </summary>
    public void UpdateReward(ForceDisplayEventType eType)
    {
        if (eType == ForceDisplayEventType.NONE)
        {
            return;
        }

        var e = ServerRepos.UserForceDisplayEvent.FindIndex(x => x.eventType == (int)eType);

        if (e < 0)
        {
            return;
        }
        
        ServerRepos.UserForceDisplayEvent[e].rewardState = true;
    }
    
    public IEnumerator OnForceDisplayEvent()
    {
        //로비 진입 튜토리얼이 끝난 후에 유저 클릭으로 이벤트 자체 튜토리얼 진행 시 강제팝업 x
        if (ManagerTutorial._instance != null)
        {
            yield break;
        }
        
        // 로비 연출이 끝나기 전 로비 오브젝트 클릭을 진행할 경우 팝업이 겹쳐 출력되는 이슈가 있어 팝업 카운트가 0이 아닐 경우 대기하도록 수정.
        yield return new WaitUntil(() => ManagerUI._instance != null && ManagerUI._instance._popupList.Count == 0 && ManagerUI._instance.isOpeningEventPopupCount == 0);
        
        // 로비 연출이 끝나기 전 로비 오브젝트를 통해 인게임으로 진입할 경우 팝업이 출력되지 않도록 예외처리 추가.
        if (ManagerLobby._stageStart)
        {
            yield break;
        }
        
        //on/off
        if (CheckStartable() && ServerRepos.UserForceDisplayEvent.Count > 0)
        {
            var rand             = new System.Random();
            var randomSortEvents = ServerRepos.UserForceDisplayEvent.OrderBy(x => rand.Next()).ToList();
            
            foreach (var fe in randomSortEvents)
            {
                //이벤트가 진행 중이고 모든 보상을 받지 않았을 때
                if (Global.LeftTime(fe.endTs) > 0 && fe.rewardState == false)
                {
                    switch ((ForceDisplayEventType)fe.eventType)
                    {
                        case ForceDisplayEventType.EVENT_STAGE:
                            if (ManagerEventStage.instance != null)
                            {
                                yield return ManagerEventStage.instance.CoOpenForceDisplayEventPopup();
                            }
                            break;
                        case ForceDisplayEventType.BINGO_EVENT:
                            if (ManagerBingoEvent.instance != null)
                            {
                                yield return ManagerBingoEvent.instance.CoOpenForceDisplayEventPopup();
                            }
                            break;
                        case ForceDisplayEventType.TREASURE_HUNT:
                            if (ManagerTreasureHunt.instance != null)
                            {
                                yield return ManagerTreasureHunt.instance.CoOpenForceDisplayEventPopup();
                            }
                            break;
                        case ForceDisplayEventType.SPACE_TRAVEL:
                            if (ManagerSpaceTravel.instance != null)
                            {
                                yield return ManagerSpaceTravel.instance.OpenForceDisplayEventPopup();
                            }
                            break;
                        case ForceDisplayEventType.ATELIER:
                            if (ManagerAtelier.instance != null)
                            {
                                yield return ManagerAtelier.instance.CoOpenForceDisplayEventPopup();
                            }
                            break;
                    }
                    
                    //아무 팝업도 안열렸다면 다음 이벤트 체크
                    if (UIPopupBingoEvent_Board._instance == null &&
                        UIPopupTreasureHunt._instance     == null &&
                        UIPopupReady._instance            == null &&
                        UIPopUpSpaceTravel.instance       == null &&
                        UIPopupAtelier.instance           == null)
                    {
                        continue;
                    }
                    
                    //팝업이 닫힐 때까지 대기
                    yield return new WaitWhile(() => UIPopupBingoEvent_Board._instance != null ||
                                                     UIPopupTreasureHunt._instance != null ||
                                                     UIPopupReady._instance != null || 
                                                     UIPopUpSpaceTravel.instance != null||
                                                     UIPopupReady._instance != null ||
                                                     UIPopupAtelier.instance != null);
                    break;
                }
            }
        }
    }
    
    void IEventBase.OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }
    
    GameEventType IEventBase.GetEventType() => GameEventType.FORCE_DISPLAY_EVENT;

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    void IEventBase.OnLobbyStart()
    {
    }

    bool IEventBase.OnTutorialCheck() => false;

    bool IEventBase.OnLobbySceneCheck() => false;

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield return null;
    }
    
    public IEnumerator CoLoadAssetBundle()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield return null;
    }

    void IEventBase.OnIconPhase()
    {
    }
}
