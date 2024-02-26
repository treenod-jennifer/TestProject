using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameEventType
{
    DECO_COLLECTION,    // 2순위 데코 컬렉션
    EVENT_QUEST,        // 3순위 이벤트 퀘스트
    
    // 이후 추가되는 이벤트 순서대로 아래에 추가.

    SPACE_TRAVEL,
    ATELIER,
    GROUP_RANKING,
    CRIMINAL_EVENT,
    ANTIQUE_STORE,
    BINGO_EVENT,
    END_CONTENTS,
    TREASURE_HUNT,

    LOGIN_EVENT,
    RENEWAL_LOGIN_EVENT,

    EVENT_STAGE,
    COIN_BONUS,
    MOLE_CATCH,
    STAGE_RANKING,
    WORLD_RANKING,
    ADVENTURE_EVENT,
    TURN_RELAY,

    VENDER,                //사용하지 않는 이벤트
    POKOFLOWER,
    BOOST,
    NOY_BOOST,
    SPECIAL_EVENT,    
    STICKER,
    ALPHABET,
    CAPSULE_GACHA,
    WELCOME_EVENT,
    COINSTASH_EVENT,
    DIASTASH_EVENT,
    STAGE_CHALLENGE,
    STAGEASSISTMISSION_EVENT,
    WELCOME_BACK_EVENT,
    LOGIN_AD_BONUS,
    PREMIUM_PASS,
    SINGLE_ROUND_EVENT,
    ADVENTURE_PASS,
    INTEGRATED_EVENT,
    OFFERWALL,
    OFFERWALL_TAPJOY,
    LUCKY_ROULETTE,
    FORCE_DISPLAY_EVENT,
}


public interface IEventBase
{
    GameEventType GetEventType();

    void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback);
    void OnLobbyStart();

    bool OnTutorialCheck();
    bool OnLobbySceneCheck();
    IEnumerator OnPostLobbyEnter();
    IEnumerator OnLobbyObjectLoadPhase();
    
    IEnumerator OnLobbyObjectLoadPhase_Outland();
    IEnumerator OnTutorialPhase();
    IEnumerator OnLobbyScenePhase();
    IEnumerator OnRewardPhase();
    void OnIconPhase();
    void OnReboot();
    void OnEventIconClick(object obj = null);
}

public interface IEventLobbyObject
{
    GameEventType GetEventType();
    void Invalidate();
    void TriggerStart();
    void TriggerSetting();

    AreaBase GetAreaBase();
}

public interface IExpansionEventLobbyObject : IEventLobbyObject
{

}

public static class EventLobbyObjectOption
{
    public static void NotEventPopup()
    {
        string messageText = null;

        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            //이벤트 진행 조건을 충족시키지 못했을 때
            messageText = Global._instance.GetString("n_wrk_10");
        }
        else
        {
            //이벤트가 열리지 않았을 때
            messageText = Global._instance.GetString("n_ev_15");
        }

        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), messageText, false);
        popup.SortOrderSetting();

    }
}
