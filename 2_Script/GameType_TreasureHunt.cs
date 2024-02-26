using Protocol;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class GameType_TreasureHunt : GameType_Base
{
    private GameObject readyPrefab;
    
    protected override void InitProperty()
    {
        base.InitProperty();
        Properties[GameTypeProp.IS_EVENT] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_PLAY_COLLECT_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_SPECIAL_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_PLAY_ALPHABET_EVENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.UNLIMITED_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.STAGE_REVIEW_ENABLED] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_READY] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.USE_READY_CHARACTER] = GameType_Base.ReturnTrue;
    }
    
    override public bool IsChangeSkin_IngameScoreUI()
    {
        return true;
    }

    override public string GetStageKey()
    {
        return $"TH_{Global.eventIndex}_{Global.stageIndex}.xml";
    }
    override public string GetStageFilename()
    {
        return GetStageKey();
    }
    
    override public int GetChapterIdx()
    {
        return 1;
    }
    
    override public void SetIngameUI()
    {
        GameUIManager.instance.SetTreasure();
    }
    
    override public IEnumerator CoOnPopupOpen_Ready(StageMapData tempData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null, System.Action callbackCancel = null)
    {
        ManagerUI._instance.OpenPopupReady_Event<UIPopupReady_TreasureHunt>(tempData, callBackStart, callBackClose, callbackCancel);
        yield return null;
    }


    override public string GetText_StageFail(ProceedPlayType proceedPlayType)
    {
        return Global._instance.GetString("p_sf_8");
    }
    
    override public void OnPopupInit_Continue(UIPopupContinue popup)
    {
        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        popup.stage.text = $"{Global._instance.GetString("ce_title_1").Replace("[n]", Global.stageIndex.ToString())}";
        popup.stage.MakePixelPerfect();
        popup.rankRoot.SetActive(false);
        popup.SettingBoniDialogBubbleTreasureHunt();
    }
    
    public override string GetReadyItemSelectKey()
    {
        return "coinReadyItemSelect";
    }
    
    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return true;
            case 1: return true;
            case 2: return true;
            case 3: return true;
            case 4: return true;
            case 5: return true;
            case 6: return CanUseDoubleReadyItem();
            case 7: return CanUseDoubleReadyItem();
            default: return false;
        }
    }

    override public bool CanUseDoubleReadyItem()
    {
        if (ServerRepos.LoginCdn == null
            || (ServerRepos.LoginCdn.DoubleReadyItems == null || ServerRepos.LoginCdn.DoubleReadyItems.Length == 0))
        {
            return false;
        }

        return true;
    }
    
    override public bool IsFirstPlay()
    {
        return ServerRepos.UserTreasureHuntStage == null || ServerRepos.UserTreasureHuntStage.Count == 0;
    }
    
    override public string GetStageText_ReadyPopup()
    {
        return $"{Global._instance.GetString("ce_title_2").Replace("[n]", Global.stageIndex.ToString())}";
    }
    
    override public string GetStageText_IngamePopup()
    {
        return $"{Global._instance.GetString("ce_title_1").Replace("[n]", Global.stageIndex.ToString())}";
    }
    
    override public string GetStageText_ClearPopup()
    {
        return $"{Global._instance.GetString("ce_title_1").Replace("[n]", Global.stageIndex.ToString())}";
    }

    override public int GetContinueCost()
    {
        if (GameManager.instance.useContinueCount >= ManagerTreasureHunt.instance.PriceContinue.Count)
            return ManagerTreasureHunt.instance.PriceContinue[ManagerTreasureHunt.instance.PriceContinue.Count - 1];
        else
            return ManagerTreasureHunt.instance.PriceContinue[GameManager.instance.useContinueCount];
    }

    override public List<int> GetItemCostList(ItemType itemType)
    {
        if (itemType == ItemType.READY_ITEM)    // 레디 아이템
            return ManagerTreasureHunt.instance.PriceReadyItem;
        else // 인게임 아이템 (컨티뉴의 경우 함수가 따로 있음)
            return ManagerTreasureHunt.instance.PriceIngameItem;
    }
    
    override public RewardType GetItemCostType(ItemType itemType)
    {
        if (itemType == ItemType.READY_ITEM)
            return RewardType.coin;
        else if (itemType == ItemType.INGAME_ITEM)
            return RewardType.coin;
        else if (itemType == ItemType.CONTINUE)
            return RewardType.coin;
        else
            return RewardType.coin;
    }
    
    
    override public GameObject GetSpine_GameClearPopup()
    {
        return ManagerTreasureHunt.instance.treasureHuntPack.ResultSpineObj;
    }
    
    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return (ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL)(ManagerBlock.instance.flowrCount);
    }

    public override void SendClearGrowthyLog()
    {
        SendClearGrowthyLog_Regular(ManagerTreasureHunt.instance.IsGetMap());
    }

    override public void OnRecvGameClear(GameManager gm, GameClearRespBase r)
    {
        GameClearResp resp = r as GameClearResp;

        if (resp.isFirst > 0)
        {
            gm.firstClear = true;
        }

        QuestGameData.SetUserData();
        PlusHousingModelData.SetUserData();

        gm.gainClover = resp.gainClover > 0;
    }
    
    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.CLOVER;
    }

    override public string GetGrowthyStageIndex()
    {
        return Global.stageIndex.ToString();
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.TREASURE_HUNT;
    }

    public override int GetGrowthyPlayCount()
    {        
        var userStageData = ServerRepos.UserTreasureHuntStage?.Find(x => x.eventIndex == Global.eventIndex && x.stage == Global.stageIndex) ?? null;
        
        return userStageData?.play ?? 0;
    }

    public override void CheckKeepPlay()
    {
        if (ManagerTreasureHunt.instance != null && ServerRepos.UserTreasureHuntStage[0].play > 0)
        {
            this.keepPlayFlag = true;
        }
    }
    
    override protected IEnumerator CheckKeepPlay_Internal()
    {
        StartCoroutine(ManagerUI._instance.CoOpenPopupTreasure());
        yield return new WaitUntil(() => UIPopupTreasureHunt._instance == null);
    }
    
    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartTreasureHuntResp>(req, complete, failCallback);
    }
    
    //게임 클리어 후, 보상 관련해서 출력되는 팝업
    public override IEnumerator CoOpenClearRewardPopup()
    {
        //상단 재화 UI 업데이트
        Global.clover = (int)GameData.Asset.AllClover;
        Global.coin = (int)GameData.Asset.AllCoin;
        Global.jewel = (int)GameData.Asset.AllJewel;
        Global.flower = (int)GameData.User.flower;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        GameClearResp clearResp = GameManager.instance.clearResp as GameClearResp;
        if (clearResp != null && clearResp.clearRewards != null)
        {
            AppliedRewardSet rewardSet = clearResp.clearRewards[GameClearResp.RewardReason.TREASURE_HUNT];
            bool isGetReward = false;

            if ((rewardSet.directApplied != null && rewardSet.directApplied.Count > 0) || (rewardSet.mailReceived != null && rewardSet.mailReceived.Length > 0))
            {
                if (rewardSet.directApplied != null)
                {
                    foreach (var item in rewardSet.directApplied)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog( item.Value.type, item.Value.valueDelta,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TREASURE_HUNT_CLEAR_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TREASURE_HUNT_CLEAR_REWARD,
                            Global.stageIndex.ToString()
                        );
                    }
                }
                
                if (rewardSet.mailReceived != null)
                {
                    foreach (var item in rewardSet.mailReceived)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog( item.type, item.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_TREASURE_HUNT_CLEAR_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_TREASURE_HUNT_CLEAR_REWARD,
                            Global.stageIndex.ToString()
                        );
                    }
                }

                ManagerUI._instance.OpenPopupGetRewardAlarm (Global._instance.GetString("n_s_46"), () => { isGetReward = true; }, rewardSet);
                yield return new WaitUntil(() => isGetReward == true);
            }
        }
    }

    public override bool IsLoadLive2DCharacters()
    {
        if (ManagerTreasureHunt.instance.ReadyCharIdx == 0) return false;
        
        List<TypeCharacterType> listLive2DCharacter = new List<TypeCharacterType>();

        if (ManagerCharacter._instance._live2dObjects.ContainsKey((int)(ManagerTreasureHunt.instance.ReadyCharIdx)) == false)
            listLive2DCharacter.Add((TypeCharacterType)ManagerTreasureHunt.instance.ReadyCharIdx);
        
        ManagerCharacter._instance.AddLoadLive2DList(listLive2DCharacter);

        return listLive2DCharacter.Count > 0;
    }
}
