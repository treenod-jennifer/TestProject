using System;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public partial class ManagerBingoEvent : MonoBehaviour, IEventBase
{
    static public ManagerBingoEvent instance = null;

    //리소스 및 이펙트 데이터 
    static public BingoEventResource bingoEventResource;

    //기본 라이브2D
    public TypeCharacterType live2dCharacter = TypeCharacterType.ANIMAL_046;

    private IconBingoEvent icon;
    
    #region 튜토리얼

    static public string tutorialOpenKey = "BingoEventOpenTutorial";
    
    static public string tutorialLineCompleteKey = "BingoEventLineCompleteTutorial";

    private int beforeSyncLineProgress;
    
    #endregion
    
    #region 연출에 관련된 데이터

    //연출 실행 여부 데이터
    public bool isSlotOpen = true;

    public bool isSlotClear = false;

    public bool isLineClear = false;

    public bool isBingoClear = false;

    public bool isBingoTempDataSetting = false;
    
    //연출 실행 여부를 판단할 더미 데이터
    public List<int> tempClearSlotIndex { get; set; } = new List<int>();
    
    #endregion
    
    #region 서버에서 받은 데이터 캐싱처리

    //Contents Data
    
    public int EventIndex { get; private set; }
    
    public long EndTs { get; private set; }
    
    public int StageCount { get; private set; }
    
    public int BonusCount { get; private set; }
    
    public int SpecialBonusCount { get; private set; }
    
    public List<List<Reward>> LineReward { get; private set; }

    public Dictionary<int, Reward> BonusReward { get; private set; }
    
    public Dictionary<int, Reward> SpecialBonusReward { get; private set; }
    
    public Reward ResetAsset { get; private set; }
    
    public Reward ResetAssetSale { get; private set; }

    public int ReadyCharacterType { get; private set; }
    
    public int resourceIndex { get; set; }

    // User Data
    
    public List<int> SelectSlot { get; set; }
    
    public List<int> SlotSetting { get; set; }
    
    public List<int> BingoBoardState { get; set; }
    
    public List<int> LineRewardState { get; set; }
    
    public int LineCompleteProgress { get; set; }
    
    public int EventState { get; set; }
    
    public bool ResetActive { get; set; }
    
    public int PlayCount { get; set; }

    public int SaleState { get; set; }
    
    #endregion

    #region API 통신 후 서버 데이터 초기화

    public void SyncFromServerContentsData()
    {
        EventIndex = ServerContents.BingoEvent.eventIndex;
        EndTs = ServerContents.BingoEvent.endTs;
        StageCount = ServerContents.BingoEvent.stageCount;
        BonusCount = ServerContents.BingoEvent.bonusCount;
        SpecialBonusCount = ServerContents.BingoEvent.specialBonusCount;
        LineReward = ServerContents.BingoEvent.lineReward;
        BonusReward = ServerContents.BingoEvent.bonusReward;
        SpecialBonusReward = ServerContents.BingoEvent.specialBonusReward;
        ResetAsset = ServerContents.BingoEvent.resetAsset;
        ResetAssetSale = ServerContents.BingoEvent.resetAssetSale;
        ReadyCharacterType = ServerContents.BingoEvent.isCollabo;
        resourceIndex = ServerContents.BingoEvent.resourceIndex;
    }

    public void SyncFromServerUserData()
    {
        SelectSlot = ServerRepos.UserBingoEvent.selectSlot;
        SlotSetting = ServerRepos.UserBingoEvent.slotSetting;
        BingoBoardState = ServerRepos.UserBingoEvent.bingoBoardState;
        LineRewardState = ServerRepos.UserBingoEvent.lineRewardState;
        LineCompleteProgress = ServerRepos.UserBingoEvent.lineCompleteProgress;
        EventState = ServerRepos.UserBingoEvent.eventState;
        ResetActive = ServerRepos.UserBingoEvent.resetActive;
        PlayCount = ServerRepos.UserBingoEvent.playCount;
        SaleState = ServerRepos.LoginCdn.bingoEventResetSaleOnOff;
    }

    #endregion
    
    private void Awake()
    {
        instance = this;

        bingoEventResource = new BingoEventResource();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    
    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerBingoEvent>();
        
        if (instance == null)
            return;
        
        ManagerEvent.instance.RegisterEvent(instance);
        
    }
    
    //현재 이벤트가 열려있는지 검사.
    public static bool IsActiveEvent()
    {
        if (instance == null)
            return false;

        return CheckStartable();
    }
    
    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        if (ServerContents.BingoEvent == null || ServerRepos.UserBingoEvent == null)
            return false;

        if (Global.LeftTime(ServerContents.BingoEvent.endTs) < 0 ||
            Global.LeftTime(ServerContents.BingoEvent.startTs) > 0)
            return false;
        
        return true;
    }

    public bool CheckTutorial(string key)
    {
        int _beforeSyncLineProgress = beforeSyncLineProgress;
        beforeSyncLineProgress = LineCompleteProgress;
        
        if (PlayerPrefs.HasKey(key))
            return false;

        // 재생 기록이 없는 오픈 튜토리얼의 경우 플레이 기록이 있든 없든 무조건 키 생성 (플레이 기록이 있는 상태에서 캐시 삭제 시 튜토리얼이 등장하지 않게 하기 위함)
        if (key == tutorialOpenKey)
        {
            PlayerPrefs.SetInt(tutorialOpenKey, 1);
            return IsTutorialOpenCheck();
        }

        // 재생 기록이 없는 라인 튜토리얼의 경우 다음 경우에서 키 생성
        // (플레이 기록이 있는 상태에서 캐시 삭제 시 튜토리얼이 등장하지 않게 하기 위함, 오픈 튜토리얼처럼 처리하면 라인이 클리어되지 않은 상태(튜토리얼을 시청하지 않은 상태)에서 키 생성이 될 수 있음)
        // 1. 튜토리얼 재생 가능할 경우 (클리어 전 라인 진행도가 0, 클리어 후 1 이상), 2. 이전에 라인 클리어 기록이 있을 경우 (클리어 전 라인 진행도가 1 이상)
        if (key == tutorialLineCompleteKey)
        {
            if (_beforeSyncLineProgress == 0 && LineCompleteProgress > 0)
            {
                PlayerPrefs.SetInt(tutorialLineCompleteKey, 1);
                return true;
            }

            if (_beforeSyncLineProgress > 0)
            {
                PlayerPrefs.SetInt(tutorialLineCompleteKey, 1);
                return false;
            }
        }

        return false;
    }

    public Reward GetBingoBoardResetCostFromManager()
    {
        if (SaleState == 0)
        {
            return ResetAsset;
        }
        else
        {
            return ResetAssetSale;
        }
    }
    
    #region 외부에서 데이터를 가져갈 때

    public Reward GetBingoBoardResetCost()
    {
        Reward cost;

        if (SaleState == 0)
        {
            cost = ServerContents.BingoEvent.resetAsset;
        }
        else
        {
            cost = ServerContents.BingoEvent.resetAssetSale;
        }
        
        return cost;
    }

    public int GetSlotState(int slotIndex)
    {
        return SlotSetting[slotIndex];
    }
    
    public Reward GetBonusReward(int bonusIndex)
    {
        var reward = new Reward();

        if (IsSpecialBonus(bonusIndex))
        {
            if (ManagerBingoEvent.instance.SpecialBonusReward.TryGetValue(bonusIndex, out reward))
                return reward;
        }
        else
        {
            if (ManagerBingoEvent.instance.BonusReward.TryGetValue(bonusIndex, out reward))
                return reward;
        }

        return null;
    }

    private bool IsTutorialOpenCheck()
    {
        foreach (var boardState in BingoBoardState)
        {
            if (boardState == 1)
                return false;
        }
        
        return PlayCount == 0;
    }

    private bool isADView()
    {
        foreach (var adInfo in ServerContents.AdInfos)
        {
            if (adInfo.Key == 16)
                return true;
        }

        return false;
    }
    
    public bool IsSpecialBonus(int index)
    {
        return index < 200 == false;
    }

    public bool IsGetLineReward(int index)
    {
        return LineRewardState[index] > 0;
    }

    public Reward GetDefultReward()
    {
        int curRewardIdx = GetCurrentLineRewardIndex();
        
        //받을 수 있는 보상을 모두 받은 상태라면 null 반환
        if (curRewardIdx >= ManagerBingoEvent.instance.LineReward.Count)
            return null;
        
        var rewards = ManagerBingoEvent.instance.LineReward[curRewardIdx];
        return rewards[0];
    }

    public List<int> GetLineDecoRewards()
    {
        List<int> lineDecoRewards = new List<int>();

        for (int i = 0; i < LineReward.Count; i++)
        {
            if(LineReward[i].Count > 1)
                lineDecoRewards.Add(i);
        }

        return lineDecoRewards;
    }
    
    public Reward GetLineDecoReward(int lineIndex)
    {
        var lineRewards = ManagerBingoEvent.instance.LineReward[lineIndex];

        return lineRewards[lineRewards.Count-1];
    }
    
    public bool IsUseReadyCollaboCharacter()
    {
        return ReadyCharacterType > 0;
    }

    public UIItemBingoEventBoard.SlotState GetSlotSetting(int index)
    {
        var slotState = SlotSetting[index];

        if (slotState < 100)
            return UIItemBingoEventBoard.SlotState.OPEN_STAGE;
        else if (slotState < 200)
            return UIItemBingoEventBoard.SlotState.OPEN_BONUS;
        else
        {
            if (isADView())
                return UIItemBingoEventBoard.SlotState.OPEN_SPECIAL_BONUS;
            else
                return UIItemBingoEventBoard.SlotState.OPEN_BONUS;
        }

    }

    public bool IsSelectSlot(int index)
    {
        for (int i = 0; i < SelectSlot.Count; i++)
        {
            if (SelectSlot[i] == index)
                return true;
        }

        return false;
    }

    public bool IsSlotClear(int index)
    {
        return BingoBoardState[index] > 0;
    }
    
    /// <summary>
    /// 라인 보상을 수령할 수 있는지 여부 확인.
    /// </summary>
    /// <returns></returns>
    public bool CheckGetLineReward()
    {
        if (GetCurrentLineRewardIndex() < LineCompleteProgress)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 라인 보상 중에서 받지 않는 보상의 인덱스 
    /// </summary>
    /// <returns></returns>
    public int GetCurrentLineRewardIndex()
    {
        for (int i = 0; i < LineRewardState.Count; i++)
        {
            if (LineRewardState[i] == 0)
                return i;
        }

        return 12;
    }

    public bool CheckComplete()
    {
        if (isLineClear || isSlotClear)
            return true;
        else
            return false;
    }

    public void SetCompleteEvent()
    {
        if (IsBingoComplete() && IsLineComplete())
        {
            isBingoClear = true;
        }
        else if (IsLineComplete())
        {
            isLineClear = true;
        }
        else if (IsSlotComplete())
        {
            isSlotClear = true;
        }
    }
    
    public int GetBingoEvent_EventIndex()
    {
        return EventIndex;
    }

    public bool IsRessetButton()
    {
        return ResetActive == false;
    }

    public bool IsClearSlot(int slotIndex)
    {
        for (int i = 0; i < tempClearSlotIndex.Count; i++)
        {
            if(tempClearSlotIndex[i] == slotIndex)
                return true;
        }

        return false;
    }
    
    public bool IsSlotComplete()
    {
        for (int i = 0; i < BingoBoardState.Count; i++)
        {
            if (ServerRepos.UserBingoEvent.bingoBoardState[i] != BingoBoardState[i])
            {
                tempClearSlotIndex.Add(i);
                return true;
            }
        }

        return false;
    }
    
    public bool IsLineComplete()
    {
        for (int i = 0; i < BingoBoardState.Count; i++)
        {
            if (ServerRepos.UserBingoEvent.bingoBoardState[i] != BingoBoardState[i])
            {
                CheckLineBingoIndex(i);

                if (tempClearSlotIndex.Count > 4)
                    return true;
                else
                    return false;
            }
        }

        return false;
    }

    void AddClearSlotIndex(List<int> tempList)
    {
        bool isLineClear = true;
        
        for (int i = 0; i < tempList.Count; i++)
        {
            if (ServerRepos.UserBingoEvent.bingoBoardState[tempList[i]] == 0)
            {
                return;
            }
        }

        if (isLineClear)
        {
            tempClearSlotIndex.AddRange(tempList);
        }
    }
    
    void CheckLineBingoIndex(int clearIndex)
    {
        bool leftDiagonal = clearIndex % 6 == 0;
        bool rightDiagonal = clearIndex / 4 > 5 ? false : clearIndex % 4 == 0;

        List<int> tempList = new List<int>();
        
        int temp = (clearIndex / 5);
            
        for (int i = 0; i < 5; i++)
        {
            tempList.Add(temp * 5 + i);
        }

        AddClearSlotIndex(tempList);
        tempList.Clear();

        temp = clearIndex % 5;
            
        for (int i = 0; i < 5; i++)
        {
            tempList.Add(i * 5 + temp);
        }
        
        AddClearSlotIndex(tempList);
        tempList.Clear();
        
        if (leftDiagonal)
        {
            for (int i = 0; i < 5; i++)
            {
                tempList.Add(i * 6);
            }
            
            AddClearSlotIndex(tempList);
            tempList.Clear();
        }

        if (rightDiagonal)
        {
            for (int i = 1; i < 6; i++)
            {
                tempList.Add(i * 4);
            }
            
            AddClearSlotIndex(tempList);
            tempList.Clear();
        }
        
    }

    public bool IsBingoComplete()
    {
        return ServerRepos.UserBingoEvent.eventState == 1;
    }
    
    #endregion
    
    void IEventBase.OnReboot()
    {
        //빙고 이벤트 컴플리트 시 데이터 초기화 하는부분 스킵하는 데이터 초기화.
        isBingoTempDataSetting = false;
        
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        if (IsActiveEvent())
        {
            icon?.SetNewIcon(IsPlayCurrentVersion() == false); 
            ManagerUI._instance.OpenPopupBingoEvent();
        }
        else
        {
            icon?.SetNewIcon(false);
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_41"), false);
            systemPopup.SortOrderSetting();
        }
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.BINGO_EVENT;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    void IEventBase.OnLobbyStart()
    {
        if (isBingoTempDataSetting == false)
        {
            isBingoTempDataSetting = true;
            beforeSyncLineProgress = LineCompleteProgress;
            instance.SyncFromServerContentsData();
            instance.SyncFromServerUserData();
            return;
        }

        if (IsBingoComplete() && IsLineComplete())
        {
            ManagerBingoEvent.instance.isBingoClear = true;
        }
        else if (IsLineComplete())
        {
            ManagerBingoEvent.instance.isLineClear = true;
            return;
        }
        else if (IsSlotComplete())
        {
            ManagerBingoEvent.instance.isSlotClear = true;
            return;
        }
        
        instance.SyncFromServerUserData();
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
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
        if (IsActiveEvent())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconBingoEvent>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) =>
            {
                icon.Init(ServerContents.BingoEvent);
                this.icon = icon;
            });
        }
    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserBingoEvent != null && ServerRepos.UserBingoEvent.playCount == 0 && ServerRepos.UserBingoEvent.bingoBoardState.FindIndex(x=> x == 1) == -1)
            return false;
        else
            return true;
    }
    
    /// <summary>
    /// 스테이지형 이벤트 팝업 강제 노출
    /// </summary>
    public IEnumerator CoOpenForceDisplayEventPopup()
    {
        if (CheckStartable() == false)
        {
            yield break;
        }
        
        yield return ManagerUI._instance.CoOpenPopupBingoEvent();
    }
}
