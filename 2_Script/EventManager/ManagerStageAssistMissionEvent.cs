using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerStageAssistMissionEvent : MonoBehaviour, IEventBase
{
    public static ManagerStageAssistMissionEvent Instance { get; private set; } = null;

    public static int currentMissionCount = 0;
    public static int currentMissionIndex;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (Instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerStageAssistMissionEvent>();

        if (Instance == null)
            return;

        currentMissionCount = ServerRepos.UserStageAssistMissionEvent.progress;
        currentMissionIndex = ServerRepos.UserStageAssistMissionEvent.missionIndex;

        ManagerEvent.instance.RegisterEvent(Instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if (ServerContents.StageAssistMissionEvent == null || ServerContents.StageAssistMissionEvent.eventIndex == 0 || ServerRepos.UserStageAssistMissionEvent == null)
        {
            return false;
        }

        //미션을 전부 완료한 상태
        if(ServerRepos.UserStageAssistMissionEvent.missionIndex == 0)
        {
            return false;
        }

        if (Global.LeftTime(ServerContents.StageAssistMissionEvent.endTs) <= 0)
        {
            return false;
        }

        //이벤트 최소 스테이지 검사
        if(ServerContents.StageAssistMissionEvent.stageMin > ServerRepos.User.stage)
        {
            return false;
        }

        //이벤트 미션이 있는지 검사
        if(ServerContents.StageAssistMissionEventDetails.TryGetValue(ServerRepos.UserStageAssistMissionEvent.missionIndex, out var cdnStageAssistMission))
        {
            if(cdnStageAssistMission.targetCount == ServerRepos.UserStageAssistMissionEvent.progress)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        
        return true;
    }

    public static bool CheckStartable_InGame()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        //게임 시작을 누른 타이밍에 이벤트가 종료되었는지 검사.
        if (ServerRepos.GameStartTs >= ServerContents.StageAssistMissionEvent.endTs)
        {
            return false;
        }

        //이벤트 최소 스테이지 검사
        if (ServerContents.StageAssistMissionEvent.stageMin > ServerRepos.User.stage)
        {
            return false;
        }

        //이벤트 미션이 있는지 검사
        if (ServerContents.StageAssistMissionEventDetails.TryGetValue(currentMissionIndex, out var cdnStageAssistMission))
        {
            if (cdnStageAssistMission.targetCount == ServerRepos.UserStageAssistMissionEvent.progress)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return IsNewStageCheck();
    }

    public static bool IsNewStageCheck()
    {
        if (Global.GameType != GameType.NORMAL) return false;

        if (Global.stageIndex == ServerRepos.User.stage)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsBlockMissionType()
    {
        return ServerContents.StageAssistMissionEventDetails[ServerRepos.UserStageAssistMissionEvent.missionIndex].missionType < 10;
    }

    public static bool IsStageAssistMissionClear()
    {
        if(currentMissionIndex < ServerRepos.UserStageAssistMissionEvent.missionIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int GetStageAssistMissionCount(int missionType)
    {
        int index = (missionType % 10) - 1;

        if(missionType / 10 > 0) // 폭탄
        {
            return ManagerBlock.instance.creatBombCount[index].Value;
        }
        else // 블럭
        {
            return ManagerBlock.instance.removeBlockCount[index].Value;
        }
    }

    public GameEventType GetEventType()
    {
        return GameEventType.STAGEASSISTMISSION_EVENT;
    }

    public string GetMessage()
    {
        string msg = Global._instance.GetString("p_sa_4");
        msg = msg.Replace("[n]", ServerContents.StageAssistMissionEvent.stageMin.ToString());
        msg = msg.Replace("[m]", ServerContents.StageAssistMissionEvent.stageMax.ToString());

        return msg;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
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
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
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
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconStageAssistMissionEvent>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) => icon.Init(ServerContents.StageAssistMissionEvent));
        }
    }

    public void OnReboot()
    {
        if (Instance != null)
        {
            currentMissionCount = 0;
            currentMissionIndex = 0;

            Destroy(Instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        if(CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopUpStageAssistMissionEvent>();
        }
        else
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false);
            systemPopup.SortOrderSetting();
        }
    }
}