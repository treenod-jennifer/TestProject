using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ServiceSDK;

public enum GameTypeProp
{
    IS_EVENT,
    CAN_PLAY_COLLECT_EVENT,
    CAN_PLAY_SPECIAL_EVENT,
    CAN_PLAY_ALPHABET_EVENT,
    REOPEN_INGAMEITEM_AT_CONTINUE,  // 컨티뉴 시, 인게임 아이템 재 오픈 할건지.
    UNLIMITED_CONTINUE,
    STAGE_REVIEW_ENABLED,    
    FLOWER_ON_READY,                //레디 팝업에서 꽃 이미지 사용하는지.
    FLOWER_ON_INGAME,               //인게임에서 꽃 점수 표시하는지.
    CAN_USE_READYITEM,
    NEED_PROFILE_AGREEMENT,         // 프로필 동의가 필요한 컨텐츠인지
    CAN_CONTINUE,
    CAN_RETRY_AT_PAUSE,             // 일시정지 팝업에서 다시하기 가능한지
    IS_ON_BTN_GIMMICK_TUTORIAL,     // 기믹 튜토리얼 버튼 표시
    NEED_PREMIUM_PASS,               // 권리형 패스가 사용되는 스테이지 인지
    APPLY_UNLIMITED_INGAMEITEM,     //인게임 아이템 무제한 이벤트 적용 가능한지
    USE_READY_CHARACTER,             // 레디 팝업 캐릭터 데이터 받아오는지
}

public enum ItemType
{
    READY_ITEM,
    INGAME_ITEM,
    INGAME_ITEM_ADVENTURE,
    CONTINUE
}

public abstract class GameType_Base : MonoBehaviour 
{
    protected delegate bool BoolFunc();

    protected Dictionary<GameTypeProp,  BoolFunc > Properties = new Dictionary<GameTypeProp, BoolFunc>();
    
    protected static bool ReturnTrue() { return true; }
    protected static bool ReturnFalse() { return false; }

    public bool GetProp(GameTypeProp p)
    {
        if( Properties.ContainsKey(p))
        {
            return Properties[p]();
        }
        return false;
    }

    private void Awake() => InitProperty();

    protected virtual void InitProperty()
    {
        Properties[GameTypeProp.IS_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.USE_READY_CHARACTER] = GameType_Base.ReturnFalse;
    }
    
    public void SetEventDataOnStageStart()
    {
        if (ManagerGroupRanking.instance != null && ManagerGroupRanking.isEventOn)
        {
            ManagerGroupRanking.instance.isPlayGroupRankingStage = ManagerGroupRanking.IsGroupRankingStage();
        }

        if (ManagerCriminalEvent.instance != null)
        {
            if (Global.GameType                                                                               == GameType.NORMAL                                        &&
                ServerRepos.UserCriminalEvent.stages.Count                                                    > ManagerCriminalEvent.instance.GetEventStep_ServerData() &&
                ServerRepos.UserCriminalEvent.stages[ManagerCriminalEvent.instance.GetEventStep_ServerData()] == GameManager.instance.CurrentStage)
            {
                ManagerCriminalEvent.instance.isPlayCriminalEventStage = true;
            }
            else
            {
                ManagerCriminalEvent.instance.isPlayCriminalEventStage = false;
            }
        }
    }

    /// <summary>
    /// 스테이지를 시작할 때, 게임 타입에 따라 초기화 해줘야하는 부분들 설정.
    /// </summary>
    public virtual void InitStage() { }

    virtual public string GetStageKey()
    {
        return "pp" + Global.stageIndex + ".xml";
    }
    virtual public string GetStageFilename()
    {
        return Global.GetHashfromText(GetStageKey()) + ".xml";
    }

    virtual public string GetDefaultMapName()
    {
        return Global.GetHashfromText("pp1.xml") + ".xml";
    }

    //인게임에서 사용할 배경 인덱스 가져오기
    virtual public int GetIngameBGIndex()
    {
        return 0;
    }

    //인게임에서 사용할 배경 다운로드
    virtual public IEnumerator LoadIngameBGAtlas()
    {
        yield return null;
    }


    virtual public int GetChapterIdx()
    {
        return 0;
    }

    //인게임에서 움직일 수 있는 카운트
    virtual public void SetMoveCount_Ingame(int gameMode, int dicCount, int turnCount)
    {
        if (gameMode == (int)GameMode.LAVA)
            GameManager.instance.moveCount = dicCount;
        else
            GameManager.instance.moveCount = turnCount;
    }

    //인게임 UI 설정
    virtual public void SetIngameUI() { }

    virtual public ScoreFlowerType GetMaxType_FlowerScore() //해당 게임타입에서 피울 수 있는 꽃의 최대 단계 (현재 진행 상태에 따라 체크됨)
    {
        return ScoreFlowerType.FLOWER_RED;
    }

    virtual public ScoreFlowerType GetMaxType_UnlimitedFlowerScore() //해당 게임타입에서 피울 수 있는 꽃의 최대 단계 (무제한)
    {
        return ScoreFlowerType.FLOWER_RED;
    }

    //레디 팝업 초기화
    virtual public void OnPopupInit_Ready(UIPopupReady popup) { }
    
    //레디 팝업 오픈
    virtual public IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart, Method.FunctionVoid callBackClose, System.Action callbackCancel)
    {
        ManagerUI._instance.OpenPopupReady(tempData, callBackStart, callBackClose, callbackCancel);
        yield return null;
    }

    #region 레디 아이템 관련
    //레디 아이템 선택 상태 설정
    virtual public void SetSelectReadyItem() {}

    // 레디아이템 종류별로 사용가능여부 체크
    virtual public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return false;
            case 1: return false;
            case 2: return false;
            case 3: return false;
            case 4: return false;
            case 5: return false;
            case 6: return false;
            case 7: return false;
            default: return false;
        }
    }

    //더블 아이템 사용가능여부 체크
    virtual public bool CanUseDoubleReadyItem()
    {
        return false;
    }

    //레디 아이템으로 선택한 턴카운트 반환
    virtual public int GetReadyItem_AddTurnCount()
    {
        if ((Global.GameInstance.CanUseReadyItem(0) == false || UIPopupReady.readyItemUseCount[0].Value == 0)
            && (Global.GameInstance.CanUseReadyItem(6) == false || UIPopupReady.readyItemUseCount[6].Value == 0))
            return 0;

        if (UIPopupReady.readyItemUseCount[0].Value == 1)
            return 3;
        else if (UIPopupReady.readyItemUseCount[6].Value == 1)
            return 6;
        
        return 0;
    }

    //게임 시작 전, 레디 아이템 사용으로 턴 추가하는 부분
    virtual public IEnumerator CoApplyReadyItem_AddTurn(int appleCount = 0)
    {
        if (appleCount == 0) yield break;
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));
        yield return GameUIManager.instance.CoActionAddTurn_WithMakeBubble(appleCount);
    }

    //게임 시작 전, 레디 아이템 사용으로 폭탄 생성하는 부분
    virtual public IEnumerator CoApplyReadyItemBomb()
    {
        //폭탄 아이템은 3개 모두 동시에 오픈하기에, 한번에 검사
        if (Global.GameInstance.CanUseReadyItem(3) == false)
            yield break;

        // 폭탄 레디 아이템 4개 검사
        for (int i = 2; i < 6; i++)
        {
            //현재 아이템이 선택되지 않은 상황이고, 무료로 선택 가능한 아이템도 적용되어 있지 않다면 반환.
            if (UIPopupReady.readyItemUseCount[i].Value == 0 && UIPopupReady.readyItemAutoUse[i].Value == 0)
                continue;

            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));

            BlockBase lineBlock = PosHelper.GetRandomBlockAtGameStart();
            if (Global.GameType == GameType.NORMAL)
            {
                //스테이지 8의 경우, 인게임 아이템(망치) 튜토리얼이 들어기 때문에 망치가 때릴 위치에 폭탄이 생성되지 않도록 함.
                //이 경우는 이미 제한된 맵이기 때문에, 컬러 블럭 검사하지 않고 특정영역 내에서만 컬러블럭 생성
                if (ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0 && Global.GameInstance.IsFirstPlay() == true)
                {
                    if (Global.stageIndex == 8 && SDKGameProfileManager._instance.GetMyProfile().stage <= 8 && ServerRepos.UserItem.InGameItem(0) > 0)
                    {
                        while (lineBlock.indexX == 5 && lineBlock.indexY == 7)
                        {
                            lineBlock = PosHelper.GetRandomBlock();
                        }
                    }
                }
            }

            if (lineBlock != null)
            {
                switch (i)
                {
                    case 2:
                        int randomBombIndex = GameManager.instance.GetIngameRandom(0, 3);
                        BlockBombType randomBombType = BlockBombType.NONE;
                        
                        switch (randomBombIndex)
                        {
                            case 0:
                                randomBombType = (BlockBombType)GameManager.instance.GetIngameRandom(4, 6);
                                break;
                            case 1:
                                randomBombType = BlockBombType.BOMB;
                                break;
                            case 2:
                                randomBombType = BlockBombType.RAINBOW;
                                break;
                        }
                        
                        lineBlock.bombType = randomBombType;
                        break;
                    case 3:
                        int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                        if (randomLine == 0) lineBlock.bombType = BlockBombType.LINE_V;
                        else lineBlock.bombType = BlockBombType.LINE_H;
                        break;
                    case 4:
                        lineBlock.bombType = BlockBombType.BOMB;
                        break;
                    case 5:
                        lineBlock.bombType = BlockBombType.RAINBOW;
                        break;
                }
                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                lineBlock.JumpBlock();
                InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
                lineBlock.Destroylinker();
            }
        }
    }
    
    #endregion

    public virtual bool CanUseIngameItem(GameItemType itemIndex)
    {
        switch (itemIndex)
        {
            case GameItemType.NONE:                return true;
            case GameItemType.HAMMER:              return true;
            case GameItemType.CROSS_LINE:          return true;
            case GameItemType.THREE_HAMMER:        return true;
            case GameItemType.RAINBOW_BOMB_HAMMER: return true;
            case GameItemType.COLOR_BRUSH:         return true;
            default:                               return false;
        }
    }

    virtual public bool IsStageTargetHidden()
    {
        return false;
    }

    virtual public string GetStageTargetText()
    {
        return "";
    }

    virtual public string GetReadyItemSelectKey()
    {
        return "readyItemSelect";
    }

    #region 게임 시작시, 연출 및 설정 해주는 함수
    //목표 팝업이 등장하기 전, 게임 타입별로 실행될 액션
    virtual public IEnumerator CoActionIngameStart_BeforeOpenTargetPopup()
    {
        yield break;
    }

    //튜토리얼이 등장한 뒤, 게임 타입별로 실행될 액션
    virtual public IEnumerator CoActionIngameStart_AfterTutorial()
    {
        yield break;
    }
    #endregion

    virtual public bool IsHideIngameItemUI()
    {
        return false;
    }

    //인게임 팝업에서 스테이지 표시에 사용될 텍스트
    virtual public string GetStageText_IngamePopup()
    {
        return $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
    }
    
    //레디 팝업 상단에 표시될 텍스트
    virtual public string GetStageText_ReadyPopup()
    {
        return $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
    }
    
    //클리어 팝업에서 스테이지 표시에 사용될 텍스트
    virtual public string GetStageText_ClearPopup()
    {
        return $"{Global._instance.GetString("p_sr_1")} {GameManager.instance.CurrentStage}";
    }


    #region 일시정지 팝업 관련
    virtual public GameObject OpenPopupPause()
    {
        return ManagerUI._instance.OpenPopupPause().gameObject;
    }
    #endregion

    virtual public bool IsHideDefaultResultUI()
    {
        return false;
    }

    virtual public bool IsOpenTimeOverUI()
    {
        return false;
    }

    virtual public int GetStageVer()
    {
        return 0;
    }

    public enum FirstClearRewardType
    {
        NONE = 0,
        STAR = 1 << 1,
        CLOVER = 1 << 2
    }

    virtual public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.NONE;
    }

    virtual public float BlockMoveSpeedRatio() { return 1.0f; }


    public abstract bool IsFirstPlay();
    
    
    

    virtual public void OnReadyOpenTutorial()
    {

    }

    //게임 시작 프로세스가 완료된 뒤, 데이터 초기화 해주는 함수
    virtual public void ResetData_AfterGameStartProgress()
    {
        
    }

    #region 광고 관련
    //광고로 획득할 수 있는 턴 카운트
    virtual public int TurnCountByReadyAD()
    {
        return 2;
    }

    //턴 추가 광고로 획득한 턴 카운트
    virtual public int GetTurnCount_UseAD_AddTurn()
    {
        return 0;
    }

    //현재 스테이지에서 턴추가 광고가 확인 가능한지 검사하는 함수
    virtual public bool IsCanWatch_AD_AddTurn(bool isNormalStage, int gameMode, int failCountOffset = 0)
    {
        return false;
    }
    #endregion

    virtual public IEnumerator GameModeProcess_OnIngameStart()
    {
        //용암 땅파기일때 화면 끝으로 이동
        if (GameManager.mExtend_Y > 0)
        {
            if (GameManager.gameMode == GameMode.DIG)
            {
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, GameManager.mExtend_Y * 78, 0);
                ManagerBlock.instance.SetPanel(GameManager.mExtend_Y * 78);
            }
            else if (GameManager.gameMode == GameMode.LAVA)
            {
                GameUIManager.instance.groundAnchor.transform.localPosition = new Vector3(0, 0, 0);
                ManagerBlock.instance.SetLavaPanel(0);
            }
            yield return null;
        }
    }

    virtual public IEnumerator GameModeProcess_OnBeforeWait()
    {
        yield return ManagerBlock.instance.CoWaitEnd_BeforeWaitAction();
    }

    //인게임에서 사용할 스코어 스파인 반환
    virtual public GameObject GetSpine_IngameScoreUI()
    {
        return null;
    }

    virtual public bool IsChangeSkin_IngameScoreUI()
    {
        return false;
    }

    virtual public string GetSkinName_IngameScoreUI(int index)
    {
        return (index + 1).ToString();
    }

    //해당 게임모드에서 띄워줄 인게임 클리어 팝업
    virtual public void OpenPopupClear()
    {
        ManagerUI._instance.OpenPopupClear(ManagerBlock.instance.score);
    }

    //인게임 클리어에서 사용할 꽃 스파인 반환
    virtual public GameObject GetSpine_GameClearPopup()
    {
        return null;
    }

    //인게임 클리어에서 사용할 꽃 스파인 애니메이션 반환
    virtual public (string, string) GetSpineAniNames_GameClearPopup(int flowerCount)
    {
        return ("", "");
    }

    //인게임 클리어 후, 결과 처리
    virtual public IEnumerator CoGameClear() 
    {
        yield return GameManager.instance.CoStageClear_Default();
    }

    //서버에서 클리어 처리 한 뒤 설정해주는 부분
    public abstract void OnRecvGameClear(GameManager gm, GameClearRespBase resp);

    //스테이지 실패 시 호출되는 함수
    virtual public void OnRecvGameFail()
    {
    }

    //재시작 시 호출되는 함수
    virtual public void OnRecvGameRestart()
    {
    }

    //컨티뉴 시 호출되는 함수
    virtual public void OnRecvContinue()
    {
    }

    #region 인게임 컨티뉴 팝업 관련
    virtual public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        popup.stage.text = $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
        popup.stage.MakePixelPerfect();
        popup.rankRoot.SetActive(false);

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
    }

    virtual public int GetContinueCost()
    {
        if (GameManager.instance.useContinueCount > 4)
            return ServerRepos.LoginCdn.ContinueCosts[ServerRepos.LoginCdn.ContinueCosts.Count - 1];
        else
            return ServerRepos.LoginCdn.ContinueCosts[GameManager.instance.useContinueCount];
    }

    virtual public bool isContinueSale()
    {
        return ServerRepos.LoginCdn.ContinueSale == 1;
    }
    #endregion
    
    virtual public RewardType GetItemCostType(ItemType itemType)
    {
        if (itemType == ItemType.READY_ITEM)
            return RewardType.jewel;
        else if (itemType == ItemType.INGAME_ITEM)
            return RewardType.jewel;
        else if (itemType == ItemType.CONTINUE)
            return RewardType.jewel;
        else
            return RewardType.jewel;
    }
    
    virtual public List<int> GetItemCostList(ItemType itemType)
    {
        if (itemType == ItemType.READY_ITEM) // 레디 아이템
        {
            return ServerRepos.LoginCdn.ReadyItems;
        }
        else if(itemType == ItemType.INGAME_ITEM) // 인게임 아이템 (컨티뉴의 경우 함수가 따로 있음)
        {
            return ServerRepos.LoginCdn.RenewalIngameItems;
        }
        else
        {
            return ServerRepos.LoginCdn.RenewalAdvIngameItems;
        }
    }
    
    #region 인게임 실패 팝업 관련
    //스테이지 실패 팝업에서 사용할 텍스트 가져오기
    virtual public string GetText_StageFail(ProceedPlayType proceedPlayType)
    {
        if (proceedPlayType == ProceedPlayType.RETRY)
        {
            return Global._instance.GetString("p_sf_4");
        }
        else 
        {
            return Global._instance.GetString("n_ev_3");
        }
    }
    #endregion

    virtual public IEnumerator SetBGM_OnIngameStart(GameManager gm)
    {
        ManagerSound._instance.ResetInGameBGMClip();
        yield break;
    }

    virtual public IEnumerator PlaySound_ResultStar(bool isSkip, int tempScore)
    {
        if (isSkip == true)
        {
            if (ManagerBlock.instance.flowrCount > 1) ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);
            else if (ManagerBlock.instance.flowrCount > 2) ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
            else if (ManagerBlock.instance.flowrCount > 3) ManagerSound.AudioPlay(AudioInGame.RESULT_STAR4);
            else ManagerSound.AudioPlay(AudioInGame.RESULT_STAR1);
        }
        else
        {
            float timer = 0f;
            while (timer < 0.2f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
            ManagerSound.AudioPlay(AudioInGame.RESULT_STAR1);
            
            if (ManagerBlock.instance.flowrCount > 1)
            {
                timer = 0f;
                while (timer < 0.5f)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR2);
            }

            if (ManagerBlock.instance.flowrCount > 2)
            {
                timer = 0f;
                while (timer < 0.7f)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR3);
            }

            if (ManagerBlock.instance.flowrCount > 3)
            {
                timer = 0f;
                while (timer < 0.3f)
                {
                    timer += Global.deltaTimePuzzle;
                    yield return null;
                }
                ManagerSound.AudioPlay(AudioInGame.RESULT_STAR4);
            }

            timer = 0f;
            while (timer < 0.6f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
       yield break;
    }

    virtual public void OnRecvAdvantureGameClear(GameManager gm, AdventureGameClearResp resp)
    {

    }

    public abstract ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode();
    public abstract int GetGrowthyPlayCount();

    public abstract string GetGrowthyStageIndex();

    public abstract void SendClearGrowthyLog();

    public abstract ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar();
    virtual public int GetGrowthy_PLAYEND_L_NUM3()
    {
        return Global.eventIndex;
    }

    public virtual string GetGrowthy_PLAYSTART_DETAILINFO() => "null";

    public virtual string GetGrowthy_PLAYEND_DETAILINFO(GameClearResp clearResp, bool isFail)
    {
        var integratedEventData = new Dictionary<string, object>();
        
        // 프리미엄 패스
        if (clearResp != null && !isFail)
        {
            if (Global.GameInstance.GetProp(GameTypeProp.NEED_PREMIUM_PASS) && ManagerPremiumPass.CheckStartable() && clearResp.userPremiumPass != null)
            {
                var premiumPassMissionCount = clearResp.userPremiumPass.targetCount - ManagerPremiumPass.userMissionCount;
                integratedEventData.Add("L_PREMIUM_PASS_ACTIVE", true);
                integratedEventData.Add("L_PREMIUM_PASS_MISSION_COUNT", premiumPassMissionCount);
            }
        }

        // 코코의 수사일지
        if (ManagerCriminalEvent.IsCriminalStagePlaying())
        {
            integratedEventData.Add("L_CRIMINAL_EVENT", ManagerCriminalEvent.instance.GetEventStep());
        }
        
        // NPU 컨티뉴 광고
        if (GameManager.instance.useNPUContinue)
        {
            integratedEventData.Add("L_NPU_CONTINUE_AD_USED", true);
        }
        
        // 수문장 크루그
        if (Global.isSingleRoundEvent)
        {
            integratedEventData.Add("L_SINGLE_ROUND_EVENT", true);
        }
        
        // 그룹랭킹
        if (ManagerGroupRanking.IsGroupRankingStagePlaying())
        {
            integratedEventData.Add("L_GROUP_RANKING_EVENT", true);
        }

        return GetJsonData(integratedEventData);
    }

    protected string GetJsonData(Dictionary<string, object> rawData)
    {
        if (rawData.Count == 0)
        {
            return "null";
        }

        return JsonConvert.SerializeObject(rawData);
    }

    public void SendClearGrowthyLog_Regular(bool flowerlevelUpdated)
    {
        var clearResp = GameManager.instance.clearResp as GameClearResp;

        // Growthy 그로씨
        // 사용한 레디 아이템
        var itemList = new List<GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 0; i < 8; i++)
        {
            if (UIPopupReady.readyItemUseCount[i].Value > 0)
            {
                var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
                    L_IID = ((READY_ITEM_TYPE)i).ToString(),
                    L_CNT = UIPopupReady.readyItemUseCount[i].Value
                };
                itemList.Add(readyItem);
            }
        }
        // 사용한 인게임 아이템
        itemList.AddRange(GameItemManager.GetPlayEndInGameItemLogData());
        // 컨티뉴 횟수
        if (GameManager.instance.useContinueCount > 0)
        {
            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = GameManager.instance.useContinueCount
            };
            itemList.Add(readyItem);
        }
        // 턴 추가 광고
        if (GameManager.instance.addTurnCount_ByAD > 0)
        {
            var readyItem = new GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = GrowthyCustomLog_PLAYEND.Code_L_CAT.AD_ITEM_LOBBY.ToString(),
                L_IID = "AD_ADD_TURN",
                L_CNT = 1
            };
            itemList.Add(readyItem);
        }
        var docItem = JsonConvert.SerializeObject(itemList);
        
        // 획득한 스코어
        var tempScore  = ManagerBlock.instance.score;
        // 특정 모드에서 사용하는 스코어 배율 (1. 엔드 컨텐츠)
        var eventRatio = Global.GameInstance.GetBonusRatio();
        if (Global.GameInstance.CanUseReadyItem(1) && UIPopupReady.readyItemUseCount[1].Value == 1)
        {
            tempScore = (int)(tempScore * (1.1f + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }
        else if (Global.GameInstance.CanUseReadyItem(7) && UIPopupReady.readyItemUseCount[7].Value == 1)
        {
            tempScore = (int)(tempScore * (1.2f + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }
        else
        {
            tempScore = (int)(tempScore * (1 + ((float)ManagerBlock.instance.StageRankBonusRatio + eventRatio) * 0.01f));
        }

        // 스테이지 모드
        var growthyStageType = Global.GameInstance.GetGrowthyGameMode();
        var growthyStar      = GetGrowthyStar(); //(ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);
        
        var getCoinB = Global.coinEvent > 0 ? ManagerBlock.instance.coins * Global.coinEvent : ManagerBlock.instance.coins;

        // 프로필 동의가 필요한 컨텐스인지 (금붕어잡기, 월드랭킹)
        var rankMode = "N";
        if (Global.GameInstance.GetProp(GameTypeProp.NEED_PROFILE_AGREEMENT))
        {
            rankMode = Global.GameInstance.GetProfileAgreementString();
        }

        // 노이 부스팅 단계
        var boostingLevel = 0;
        if (ManagerNoyBoostEvent.instance != null && GameManager.instance.IsNoyBoostStage())
        {
            boostingLevel = GameManager.instance.boostingStep;
        }

        // 에코피 이벤트
        var pokoFlowerData = (clearResp.pokoFlower == null) ? null : clearResp.pokoFlower;
        var checkEcopiEvent = ServerContents.PokoFlowerEvent != null && pokoFlowerData != null && pokoFlowerData.get_reward == 1 &&
                              pokoFlowerData.achievedCount == ServerContents.PokoFlowerEvent.max_reward_count;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        
        var playEnd = new GrowthyCustomLog_PLAYEND
        (
            pid: myProfile.userID,
            lastStage: (myProfile.stage - 1).ToString(),
            beforeLastStage: GameManager.instance.GrowthyAfterStage.ToString(),
            currentStage: Global.GameInstance.GetGrowthyStageIndex(),
            stageType: growthyStageType,
            win: GrowthyCustomLog_PLAYEND.Code_L_PWIN.WIN,
            star: growthyStar,
            score: tempScore,
            getCoin: getCoinB, //ManagerBlock.instance.coins,
            playTime: (long)(Time.time - GameManager.instance.playTime),
            firstPlay: GameManager.instance.firstPlay, //최초플레이
            continuePlay: GameManager.instance.useContinueCount > 0,
            leftTurn: GameManager.instance.leftMoveCount, //남은턴 다시계산
            useItemList: docItem,
            getHeart: GameManager.instance.firstClear ? 1 : 0,
            clearStageMission: GameManager.instance.clearMission          > 0,
            clearChapterMission: GameManager.instance.allStageClearReward > 0,
            reviewCount: GameManager.instance.stageReview,
            tempEventIndex: Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            remainGoals: null,
            rankMode: rankMode,
            ecopi: checkEcopiEvent == true ? "Y" : "N",
            boostLevel: boostingLevel.ToString(),
            firstFlowerLevel: flowerlevelUpdated == true ? "Y" : "N",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: "[0]"
        );
        
        var doc = JsonConvert.SerializeObject(playEnd);
        doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYEND_DETAILINFO(clearResp, false));
        ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

        //권리형 패스 갱신
        if (Global.GameInstance.GetProp(GameTypeProp.NEED_PREMIUM_PASS) && ManagerPremiumPass.CheckStartable() && clearResp.userPremiumPass != null)
        {
            ManagerPremiumPass.userMissionCount = clearResp.userPremiumPass.targetCount;
        }

        SendStageFirstClearGrowthyLog();
    }

    protected virtual void SendStageFirstClearGrowthyLog()
    {

    }

    //게임 클리어 후, 보상받는 그로씨에 대한 처리
    //로비에서 클리어 처리가 필요할 때 파라미터에 값 추가.
    public virtual void SendClearRewardGrowthyLog(GameClearRespBase _resp = null)
    {
        var clearResp = _resp ?? GameManager.instance.clearResp;
        
        //다아이 주머니 그로시
        if(Global.GameType != GameType.COIN_BONUS_STAGE)
            SendClearRewardGrowthy_DiaStash(clearResp);
    }

    //게임 클리어 후, 보상 관련해서 출력되는 팝업
    public virtual IEnumerator CoOpenClearRewardPopup()
    {
        yield break;
    }

    public string GetProfileAgreementString()
    {
        string rankMode = "N";        

        UserSelf myProfile = SDKGameProfileManager._instance.GetMyProfile();
        Profile_PION profileData = myProfile.GetPionProfile();
        
        if(profileData != null)
        {
            if (profileData.profile.opts != null)
            {
                if (profileData.profile.isLineTumbnailUsed()) //동의                        
                    rankMode = " Y_AGREE";
                else
                    rankMode = " Y_DISAGREE";
            }
        }
        
        return rankMode;
    }

    public int CalcFlowerLevel(bool limit, int tmpScore)
    {
        if (ManagerBlock.instance == null || ManagerBlock.instance.stageInfo == null)
            return 0;

        ScoreFlowerType flowerLim = limit ? Global.GameInstance.GetMaxType_FlowerScore() : Global.GameInstance.GetMaxType_UnlimitedFlowerScore();
        int flowerLevel = 0;
        //꽃점수체크  
        if ((flowerLim >= ScoreFlowerType.FLOWER_RED) && (tmpScore >= (int)(ManagerBlock.instance.stageInfo.score4 * 1.1f)))
        {
            flowerLevel = 5;
        }
        else if ((flowerLim >= ScoreFlowerType.FLOWER_BLUE) && (tmpScore >= ManagerBlock.instance.stageInfo.score4))
        {
            flowerLevel = 4;
        }
        else if (tmpScore >= ManagerBlock.instance.stageInfo.score3)
        {
            flowerLevel = 3;
        }
        else if (tmpScore >= ManagerBlock.instance.stageInfo.score2)
        {
            flowerLevel = 2;
        }
        else if (tmpScore >= ManagerBlock.instance.stageInfo.score1)
        {
            flowerLevel = 1;
        }
        return flowerLevel;
    }

    public virtual float GetBonusRatio()
    {
        return 0;
    }

    protected bool keepPlayFlag = false;
    public virtual void CheckKeepPlay()
    {
        keepPlayFlag = false;
    }

    public void SkipKeepPlay()
    {
        keepPlayFlag = false;
    }


    virtual protected IEnumerator CheckKeepPlay_Internal()
    {
        yield break;
    }

    virtual public IEnumerator KeepPlayOn()
    {
        if(keepPlayFlag)
            yield return CheckKeepPlay_Internal();
        keepPlayFlag = false;
        yield break;
    }

    public abstract void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback);


    /// <summary>
    /// 레디, 인게임 아이템 사용 여부
    /// </summary>
    /// <returns></returns>
    public virtual List<bool> GetUseItemData()
    {
        List<bool> tempUseItemDatas = new List<bool>();

        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[0].Value > 0);
        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[1].Value > 0);
        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[2].Value > 0);
        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[3].Value > 0);
        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[4].Value > 0);
        tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[5].Value > 0);
        tempUseItemDatas.Add(GameItemManager.useCount[0] > 0);
        tempUseItemDatas.Add(GameItemManager.useCount[1] > 0);
        tempUseItemDatas.Add(GameItemManager.useCount[2] > 0);
        tempUseItemDatas.Add(GameItemManager.useCount[3] > 0);

        if (Global.GameInstance.CanUseReadyItem(6))
            tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[6].Value > 0);
        else tempUseItemDatas.Add(false);

        if (Global.GameInstance.CanUseReadyItem(7))
            tempUseItemDatas.Add(UIPopupReady.readyItemUseCount[7].Value > 0);
        else tempUseItemDatas.Add(false);
        
        tempUseItemDatas.Add(GameItemManager.useCount[7] > 0);
        
        return tempUseItemDatas;
    }

    public virtual bool IsLoadLive2DCharacters()
    {
        return false;
    }
    
    private void SendClearRewardGrowthy_DiaStash(GameClearRespBase clearResp)
    {
        //다이아 주머니 이벤트 진행중인지 확인
        if (ServerContents.DiaStashEvent == null || clearResp == null || clearResp.diaStash == null)
            return;
        
        //현재 인게임에서 다이아 스태시 적용중인지 검사하는 조건(인게임 진입 중, 이벤트가 종료되었을 때 체크를 위함)   
        if (ManagerDiaStash.instance == null || ManagerDiaStash.instance.isStageApplyDiaStash == false)
            return;
        
        //스테이지 클리어 횟수(다이아 적립 단계)가 올라간 상태에서만 그로시 로그를 남김
        if (ManagerDiaStash.instance.prevStageClearCount >= clearResp.diaStash.stageClearCount)
            return;

        //그로시 추가
        string segment = clearResp.diaStash.segment == 1 ? "NPU" : "PU";
        if (clearResp.diaStash.segment > 2)
            segment = clearResp.diaStash.segment.ToString();
            
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.DIA_STASH,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.DIA_STASH_STEP,
            $"{segment}_{ManagerDiaStash.instance.GetPackageGrade()}_{clearResp.diaStash.stageClearCount}_{ServerContents.DiaStashEvent.eventIndex}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        
        //그로시 로그 디버깅
        var achievementDoc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", achievementDoc);
    }

    protected void SendClearRewardGrowthy_AntiqueStore(GameClearResp clearResp)
    {
        //앤틱스토어 이벤트 진행 중인지 체크.
        if (ManagerAntiqueStore.CheckStartable() == false) return;

        //재화를 더 이상 획득하지 못할 때 체크.
        if (clearResp.antiqueStore.assetAmount == ManagerAntiqueStore.instance.currentUserToken) return;

        //그로시 로그            
        var growthyMoney = new GrowthyCustomLog_Money
        (
            GrowthyCustomLog_Money.Code_L_TAG.AS,
            GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY,
            0,
            clearResp.antiqueStore.assetAmount - ManagerAntiqueStore.instance.currentUserToken,
            0,
            ServerRepos.UserAntiqueStore.assetAmount,
            $"{ServerContents.AntiqueStore.eventIndex}_ANTIQUE_STORE"
        );

        var docMoney = JsonConvert.SerializeObject(growthyMoney);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
    }
}

