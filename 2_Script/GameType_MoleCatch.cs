using Newtonsoft.Json;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameType_MoleCatch : GameType_Base
{
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
        Properties[GameTypeProp.FLOWER_ON_INGAME] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_USE_READYITEM] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PROFILE_AGREEMENT] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.CAN_CONTINUE] = this.CanContinue;
        Properties[GameTypeProp.CAN_RETRY_AT_PAUSE] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.IS_ON_BTN_GIMMICK_TUTORIAL] = GameType_Base.ReturnTrue;
        Properties[GameTypeProp.NEED_PREMIUM_PASS] = GameType_Base.ReturnFalse;
        Properties[GameTypeProp.APPLY_UNLIMITED_INGAMEITEM] = GameType_Base.ReturnTrue;
    }
    override public string GetStageKey()
    {
        //스테이지 키는 버전정보를 뺀 파일 이름만 사용.
        string fileName = string.Format("MC{0:D2}{1:D3}.xml", Global.eventIndex, ManagerMoleCatch.instance.GetMapIndex(Global.stageIndex));
        return fileName;
    }

    override public string GetStageFilename()
    {
        return Global.GetHashfromText(GetStageKey()) + ".xml";
    }

    public override int GetChapterIdx()
    {
        return Global.chapterIndex;
    }
    
    override public string GetStageText_ReadyPopup()
    {
        return Global._instance.GetString("p_mc_2");
    }
    
    override public bool CanUseReadyItem(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return true;
            case 1: return false;
            case 2: return true;
            case 3: return true;
            case 4: return true;
            case 5: return true;
            case 6: return CanUseDoubleReadyItem();
            case 7: return false;
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

    //override public ServerStageBase GetStageBase()
    //{
    //    return ServerRepos.UserMoleCatchStages.Find((us) => us.stage == Global.stageIndex && us.event_index == Global.eventIndex && us.chapter == Global.chapterIndex);
    //}

    override public int GetStageVer()
    {
        return ServerContents.MoleCatchChapter[Global.chapterIndex].stageVersions[Global.stageIndex - 1];
    }

    bool CanContinue()
    {
        if (ServerRepos.LoginCdn.EventContinueMax == 0 || ServerRepos.LoginCdn.EventContinueMax > GameManager.instance.useContinueCount)
        {
            return true;
        }
        return false;
    }

    override public IEnumerator GameModeProcess_OnIngameStart()
    {
        yield return base.GameModeProcess_OnIngameStart();
        ManagerMoleCatch.lastPlayedStage = Global.stageIndex;
    }

    override public bool IsFirstPlay()
    {
        foreach (var temp in ServerRepos.UserMoleCatchStages)
        {
            if (temp.event_index == Global.eventIndex && temp.stage == Global.stageIndex && temp.chapter == Global.chapterIndex)
            {
                if (temp.play == 1)
                    return true;
                else
                    return false;
            }
        }
        return true;
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL GetGrowthyStar()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN_DTL.NULL;
    }

    public override void SendClearGrowthyLog()
    {
        base.SendClearGrowthyLog_Regular(false);
    }

    override public IEnumerator SetBGM_OnIngameStart(GameManager gm)
    {
        if (Global.CollaboIndex == 2)
        {
            yield return gm.LoadBGM();
        }
        else
            yield return base.SetBGM_OnIngameStart(gm);
        
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

    override public void OnRecvGameFail()
    {
        //ManagerLobby._eventStageFail = true;
        //UIPopupReady.eventStageFail = true;
    }

    override public void OnRecvGameRestart()
    {
        //ManagerLobby._eventStageFail = true;
        //UIPopupReady.eventStageFail = true;
    }

    override public int GetFirstClearRewardType()
    {
        return (int)FirstClearRewardType.CLOVER;
    }

    override public string GetGrowthyStageIndex()
    {
        int mapIndex = ManagerMoleCatch.instance.GetMapIndex(Global.stageIndex);

        return mapIndex.ToString();
    }

    public override ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD GetGrowthyGameMode()
    {
        return ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.MOLE;
    }

    public override int GetGrowthyPlayCount()
    {
        var userStageData = ServerRepos.UserMoleCatchStages?.Find(x => x.event_index == Global.eventIndex && x.chapter == Global.chapterIndex && x.stage == Global.stageIndex) ?? null;
        
        return userStageData?.play ?? 0;
    }

    protected override void SendStageFirstClearGrowthyLog()
    {
        if (GameManager.instance.firstClear)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WHAC_A_MOLE,
                    string.Format("EVENT_{0}_STAGE_{1}", Global.eventIndex, GetGrowthyStageIndex()),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                );
            var userStageData = ServerRepos.UserMoleCatchStages.Find(x => x.event_index == Global.eventIndex && x.chapter == Global.chapterIndex && x.stage == Global.stageIndex);
            
            if( userStageData == null )
            {   // panic
                return;
            }
            achieve.L_NUM1 = userStageData.play;
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    public override void CheckKeepPlay()
    {
        if (ManagerMoleCatch.lastPlayedStage != 0)
        {
            this.keepPlayFlag = true;
        }
    }

    override protected IEnumerator CheckKeepPlay_Internal()
    {
        ManagerUI._instance.OpenPopupMoleCatch();
        yield return new WaitUntil(() => UIPopupMoleCatch._instance == null);

    }

    public override void GameStart(GameStartReq req, Action<BaseResp> complete, Action<GameStartReq> failCallback)
    {
        ServerAPI.GameStart<GameStartMoleCatchResp>(req, complete, failCallback);
    }
}
