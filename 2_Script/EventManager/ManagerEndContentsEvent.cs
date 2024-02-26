using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SideIcon;

public partial class ManagerEndContentsEvent : MonoBehaviour, IEventBase
{
    public static ManagerEndContentsEvent instance = null;
    
    // Contents Data
    public int EventIndex { get; set; }
    public int ResourceIndex { get; set; }
    public int MapChangeCoin { get; set; }
    public int ApPrice { get; set; }
    public bool MapChangeSale { get; set; }
    public int MaxEventAP { get; set; }
    private long ApRechargeAt { get; set; }
    
    // User Data
    public int Status { get; set; }     // 0:오브젝트/팝업 비활성화/플레이 불가능, 1 : 오브젝트/팝업 활성화/플레이 불가능(End - 60 스테이지), 2 : 플레이 가능
    public int EventAP { get; set; }
    public long ApChargeAt { get; set; }
    public int MapIndex { get; set; }
    public int StageIndex { get; set; }
    public int Buff { get; set; }
    public Dictionary<string, int> GoodsInfo { get; set; }
    public int Play { get; set; }
    public bool IsFirstMapChange { get; set; }
    public bool IsFirstOpen { get; set; }

    private int EventAP_Dummy = 0;
    private long ApChargeAt_Dummy = 0;
    private Coroutine timerCoroutine;

    void Awake()
    {
        instance = this;
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

        Global._instance.gameObject.AddMissingComponent<ManagerEndContentsEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;
        
        if (ServerContents.EndContentsEvent == null || ServerRepos.UserEndContentsEvent == null)
            return false;
        
        return ServerRepos.UserEndContentsEvent.status > 0;
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.END_CONTENTS;
    }
    
    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    void IEventBase.OnLobbyStart()
    {
        if (instance != null && ServerContents.EndContentsEvent != null && ServerRepos.UserEndContentsEvent != null)
        {
            instance.SyncFromServerContentsData();
            instance.SyncFromServerUserData_All();
        }
    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public bool OnLobbySceneCheck()
    {
        return ManagerArea._instance.GetEventLobbyObject(GameEventType.END_CONTENTS) != null && CheckPlayStartCutScene();
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield return LoadEndContentsResource("Object");
        GameObject obj = null;
        if (endContentsPack_Object != null)
        {
            obj = ManagerLobby.NewObject(endContentsPack_Object.gameObject);
            AreaBase areaBase = endContentsPack_Object.GetComponent<AreaBase>();
            if (areaBase != null)
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
        }
        
        yield return ManagerCharacter._instance.LoadCharacters();
        if (obj != null)
            ManagerArea._instance.RegisterEventLobbyObject(obj.GetComponent<EndContentsAreaBase>());
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        ServerAPI.EndContentsRefreshAp((resp) => { });  // 연출 직후 연출 재생 데이터를 받아올 수 있도록 업데이트 API 호출
        yield return ManagerLobby._instance.WaitForSceneEnd(ManagerArea._instance.GetEventLobbyObject(GameEventType.END_CONTENTS).GetAreaBase(), 1);
    }
    
    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable() && CanPlayEndContentsStage())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconEndContentsEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.EndContentsEvent));
        }
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
        ManagerUI._instance.OpenPopupReadyEndContents();
    }

    public bool CanPlayEndContentsStage()
    {
        return Status == 2;
    }

    public static bool CheckPlayStartCutScene()
    {
        if (instance == null)
            return false;
        if (instance.Status == 0)
            return false;

        return instance.IsFirstOpen;
    }
    
    public List<int> GetScoreRatio()
    {
        return new List<int>() {1, 1, 2, 3, 4};
    }

    public List<int> GetBuffRatio()
    {
        return new List<int>() { 0, 1, 2, 3, 4, 5, 7, 10 };
    }

    #region 엔드컨텐츠 상점 관련 데이터

    public enum BuyError
    {
        NO_ERROR = 0,
        NOT_ENOUGH_TOKEN,
        BUY_LIMIT,
        INVALID_GOODS,
    }
    
    public BuyError CanBuy(int goodsId )
    {
        CdnEndContentsShop goodsInfo = null;
        foreach (var item in ServerContents.EndContentsShop)
            if (item.Value.goodsIndex == goodsId)
                goodsInfo = item.Value;
        
        if (goodsInfo == null)
        {
            return BuyError.INVALID_GOODS;
        }

        if (goodsInfo.buyLimit >= 1 && GoodsInfo[goodsId.ToString()] >= goodsInfo.buyLimit)
        {
            return BuyError.BUY_LIMIT;
        }

        if (GetPokoCoin() < goodsInfo.price)
        {
            return BuyError.NOT_ENOUGH_TOKEN;
        }


        return BuyError.NO_ERROR;
    }

    #endregion

    #region API 통신 후 서버 데이터 동기화
    
    public void SyncFromServerContentsData()
    {
        EventIndex = ServerContents.EndContentsEvent.eventIndex;
        ResourceIndex = ServerContents.EndContentsEvent.resourceIndex;
        MapChangeCoin = ServerContents.EndContentsEvent.mapChangeCoin;
        ApPrice = ServerContents.EndContentsEvent.apChargeCoin;
        MapChangeSale = ServerContents.EndContentsEvent.mapChangeSale;
        MaxEventAP = ServerContents.EndContentsEvent.maxAp;
        ApRechargeAt = ServerContents.EndContentsEvent.apChargePeriod;
    }
    
    public void SyncFromServerUserData_All()
    {
        Status = ServerRepos.UserEndContentsEvent.status;
        EventAP = ServerRepos.UserEndContentsEvent.apCount;
        ApChargeAt = ServerRepos.UserEndContentsEvent.apChargeAt;
        MapIndex = ServerRepos.UserEndContentsEvent.mapIndex;
        StageIndex = ServerRepos.UserEndContentsEvent.stageIndex;
        Buff = ServerRepos.UserEndContentsEvent.buff;
        Play = ServerRepos.UserEndContentsEvent.play;
        IsFirstMapChange = ServerRepos.UserEndContentsEvent.isFirstMapChange;
        IsFirstOpen = ServerRepos.UserEndContentsEvent.isFirstOpen;
        GoodsInfo = ServerRepos.UserEndContentsEvent.shopInfo.goodsInfo;
        EventAP_Dummy = 0;
        ApChargeAt_Dummy = 0;
    }

    public void SyncFromServerUserData_Ingame()
    {
        Buff = ServerRepos.UserEndContentsEvent.buff;
    }

    public void SyncFromServerUserData_AP()
    {
        Status = ServerRepos.UserEndContentsEvent.status;
        EventAP = ServerRepos.UserEndContentsEvent.apCount;
        ApChargeAt = ServerRepos.UserEndContentsEvent.apChargeAt;
        EventAP_Dummy = 0;
        ApChargeAt_Dummy = 0;
    }
    
    public void SyncFromServerUserData_PostRefreshMap()
    {
        MapIndex = ServerRepos.UserEndContentsEvent.mapIndex;
        IsFirstMapChange = ServerRepos.UserEndContentsEvent.isFirstMapChange;
    }

    public void SyncFromServerUserData_PostBuyShop()
    {
        GoodsInfo = ServerRepos.UserEndContentsEvent.shopInfo.goodsInfo;
    }
    
    #endregion
    
    // 로비에서 타이머 돌릴 때 클라이언트에서 임의로 AP값 조정
    public void SyncFromDummyUserData_AP()
    {
        EventAP_Dummy = ( EventAP_Dummy > EventAP ? EventAP_Dummy : EventAP ) + 1;
        ApChargeAt_Dummy = ( ApChargeAt_Dummy > ApChargeAt ? ApChargeAt_Dummy : ApChargeAt ) + ApRechargeAt;
    }
    
    #region 타이머 체크 : UIIcon AP 체크를 위한 타이머 코드

    public void CheckTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        int apCount = EventAP_Dummy > EventAP ? EventAP_Dummy : EventAP;
        if (MaxEventAP > apCount)
            timerCoroutine = StartCoroutine(ApChargeTimer());
    }

    private IEnumerator ApChargeTimer()
    {
        long apCharge = ApChargeAt_Dummy > ApChargeAt ? ApChargeAt_Dummy : ApChargeAt;
        while (true)
        {
            if (Global.LeftTime(apCharge) <= 0)
                break;
            yield return new WaitForSeconds(1.0f);
        }
        
        timerCoroutine = null;
        IconEndContentsEvent endContentsIcon = null;
        foreach (var icon in ManagerUI._instance.ScrollbarRight.icons)
        {
            if (icon is IconEndContentsEvent)
                endContentsIcon = icon as IconEndContentsEvent;
        }
        
        SyncFromDummyUserData_AP();
        if (endContentsIcon != null)
            endContentsIcon.RefreshCollectCount(EventAP_Dummy);
        CheckTimer();
    }
    
    #endregion
    
    public static int GetPokoCoin()
    {
        ServerUserTokenAsset userTokenAsset = null;
        if (ServerRepos.UserTokenAssets != null)
        {
            if (ServerRepos.UserTokenAssets.TryGetValue(3, out userTokenAsset))
            {
                return userTokenAsset.amount;
            }
        }

        return 0;
    }
    
    public static int GetMaxPokoCoin()
    {
        return 30000;
    }
}
