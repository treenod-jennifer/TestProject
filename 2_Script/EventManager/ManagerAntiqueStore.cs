using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAntiqueStore : MonoBehaviour, IEventBase
{
    public static ManagerAntiqueStore instance = null;

    public AntiqueStoreResource antiqueStoreResource = null;

    public int currentUserToken = 0;

    public bool isSpecialEvent = false;
    
    public const string TutorialOpenKey = "AntiqueStoreOpenTutorial";
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        antiqueStoreResource = new AntiqueStoreResource();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public Reward GetDecoInfoHousingIndex()
    {
        Reward reward = new Reward();

        var housing = ServerContents.AntiqueStore.housingList.Find(housing => housing.idx == 1);

        reward.type = (int)RewardType.housing;
        reward.value = GetHousingValue(housingIndex: housing.housingIndex, modelIndex: housing.modelIndex);
        
        return reward;
    }

    public int GetHousingValue(int housingIndex, int modelIndex)
    {
        return (housingIndex * 10000) + modelIndex;
    }

    public bool IsBuyHousing(int housingIdx)
    {
        return ServerRepos.UserAntiqueStore.buyState[housingIdx - 1] > 0;
    }

    public bool IsBuyBonus(int idx)
    {
        return ServerRepos.UserAntiqueStore.buyCount >= ServerContents.AntiqueStore.accumulateCount[idx];
    }

    public int GetAntiqueToken(int token)
    {
        return token * (IsSpecialEventCheck() ? ServerContents.AntiqueStore.specialRatio : 1);
    }

    public bool CheckTutorial()
    {
        //튜토리얼을 한번이라도 안보거나 앱 재설치 or 캐시 삭제를 했을 경우 튜토리얼 노출.
        if (PlayerPrefs.HasKey(TutorialOpenKey))
            return false;
        
        PlayerPrefs.SetString(TutorialOpenKey, "PlayAntiqueStoreTutorial");

        return true;
    }

    public static void Init()
    {
        if (CheckStartable() == false)
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerAntiqueStore>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
        
        //Sync
        ManagerAntiqueStore.instance.SyncUserToken();
    }
    
    public static bool CheckStartable()
    {
        //1Day를 완료해야 이벤트 오픈
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
            return false;

        if (ServerContents.AntiqueStore == null || ServerRepos.UserAntiqueStore == null)
            return false;

        if (Global.LeftTime(ServerContents.AntiqueStore.endTs) < 0 ||
            Global.LeftTime(ServerContents.AntiqueStore.startTs) > 0)
            return false;
        
        return true;
    }

    public static bool IsSpecialEventCheck()
    {
        return Global.LeftTime(ServerContents.AntiqueStore.specialEventEndTs) >= 0 &&
               Global.LeftTime(ServerContents.AntiqueStore.specialEventStartTs) <= 0;
    }

    public void PostCheckAntiqueStorePopupOpen(System.Action eventCallBack = null, System.Action eventClosePopup = null)
    {
        if (eventCallBack == null)
            ManagerUI._instance.OpenPopup<UIPopupAntiqueStore>(popup =>
            {
                popup.InitData();
                popup._callbackClose += () =>
                {
                    eventClosePopup?.Invoke();
                };
            });
        else
            eventCallBack();
    }
    
    public void PostCheckAntiqueStorePopupOpen_CheckEvent(System.Action eventCallBack = null, System.Action eventClosePopup = null)
    {
        if (ManagerAntiqueStore.CheckStartable())
        {
            if (eventCallBack == null)
                ManagerUI._instance.OpenPopup<UIPopupAntiqueStore>(popup =>
                {
                    popup.InitData();
                    popup._callbackClose += () =>
                    {
                        eventClosePopup?.Invoke();
                    };
                });
            else
                eventCallBack();
        }
        else
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false);
            systemPopup.SortOrderSetting();
        }
    }
    
    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        PostCheckAntiqueStorePopupOpen_CheckEvent();
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.ANTIQUE_STORE;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    void IEventBase.OnLobbyStart()
    {
        
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
        if(ManagerAntiqueStore.instance.antiqueStoreResource == null) yield break;
        
        yield return ManagerAntiqueStore.instance.antiqueStoreResource.CoLoadAssetBundle();
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
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconAntiqueStore>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) => icon.Init(ServerContents.AntiqueStore));
        }
    }

    #region SyncData

    public void SyncUserToken()
    {
        currentUserToken = ServerRepos.UserAntiqueStore.assetAmount;
    }

    public void SyncAntiqueStoreSpecialEvent()
    {
        isSpecialEvent = IsSpecialEventCheck();
    }

    #endregion

    #region SpineObject

    public GameObject GetSpine_Glass()
    {
        return ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.ObjGlassSpine
            ? ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.ObjGlassSpine : null;
    }

    public GameObject GetSpine_Twinkle()
    {
        return ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.ObjTwinkleSpine
            ? ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.ObjTwinkleSpine : null;
    }

    #endregion
}
