using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerDiaStash : MonoBehaviour, IEventBase
{
    public static ManagerDiaStash instance = null;

    //리소스 및 이펙트 데이터 
    public DiaStashResource diaStashResource;
    
    //현재 유저가 구매 할 수 있는 다이아 주머니 상품.
    private DiaStashPackage currentDiaStashPackage = null;
    
    //게임 클리어 전 유저가 가지고 있는 다이아 갯수.
    public int currentUserDia = 0;

    #region 인게임에서 사용되는 변수
    //현재 스테이지에 다이아 주머니 이벤트가 적용중인지 검사하는 변수
    public bool isStageApplyDiaStash = false;
    //마지막으로 달성한 스테이지 클리어 카운트
    public int prevStageClearCount = 0;
    //마지막으로 획득한 보너스 다이아 갯수
    public int prevBonusDia = 0;
    #endregion
    
    private void Awake()
    {
        instance = this;

        diaStashResource = new DiaStashResource();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
        {
            return false;
        }
        
        if(ServerContents.DiaStashEvent == null || ServerContents.DiaStashEvent.eventIndex == 0)
        {
            return false;
        }

        if (ServerRepos.UserDiaStash == null)
            return false;
        
        if (Global.LeftTime(ServerContents.DiaStashEvent.end_ts) <= 0 ||
            Global.LeftTime(ServerContents.DiaStashEvent.start_ts) > 0)
            return false;
        
        
        if (IsAllBuyPackage())
            return false;
        
        return true;
    }

    public static bool IsAllBuyPackage()
    {
        //유저의 grade가 패키지 보다 높을 때 이벤트 종료
        int packageAllCount = 0;

        foreach (var package in ServerContents.DiaStashEvent.packages)
        {
            packageAllCount += package.Value.Count;
        }
        
        if (ServerRepos.UserDiaStash.grade > packageAllCount)
            return true;
        else
            return false;
    }
    
    public void SyncDiaStashPackageData()
    {
        DiaStashPackage package = new DiaStashPackage();

        int segment = ServerRepos.UserDiaStash.segment;

        if (ServerContents.DiaStashEvent.packages.ContainsKey(segment) == false) return;
        
        foreach (var value in ServerContents.DiaStashEvent.packages[segment])
        {
            if (ServerRepos.UserDiaStash.grade == value.grade)
                package = value;
        }
        
        currentDiaStashPackage = package;
    }

    public void SyncUserDiaCount()
    {
        currentUserDia = instance.GetCurrentDia();
    }
    
    
    //다이아를 전부 채웠을 때 "FULL"표시 해주는 함수.
    public string GetIsFullDia(int diaCount)
    {
        string text = "";

        if(diaCount == instance.GetFullDia())
            text = "FULL";
        else
            text = diaCount.ToString();

        return text;
    }
    
    //패키지에서 세그먼트 별 Grade를 반환하는 함수.
    public int GetPackageGrade()
    {
        //유저 grade - 패키지 최소 grade + 1
        return ServerRepos.UserDiaStash.grade - ServerContents.DiaStashEvent.packages[ServerRepos.UserDiaStash.segment][0].grade + 1;
    }

    //이벤트 남은 시간 표시.
    public long GetEndTs()
    {
        return ServerContents.DiaStashEvent.end_ts;
    }

    public int GetCurrentDia()
    {
        //기본 지급 다이아 + 현재 유저가 받는 보너스 다이아
        int defaultDia = currentDiaStashPackage.defaultDia;
        int bonusDia = currentDiaStashPackage.bonusDia[ServerRepos.UserDiaStash.stageClearCount];
        return defaultDia + bonusDia;
    }

    public int GetDefaultDia()
    {
        //기본 지급 다이아
        return currentDiaStashPackage.defaultDia;
    }

    public int GetAddDia()
    {
        //현재 보너스 다이아.
        return currentDiaStashPackage.bonusDia[ServerRepos.UserDiaStash.stageClearCount];
    }

    public int GetBonusDia()
    {
        //보너스 다이아
        //데이터 각 배열에 보너스로 받는 다이아 전체가 있어 빼줘서 얼마의 보너스를 받는지 계산
        //ex) "bonus_dia": [0, 20, 40, 60, 80, 100] // 이렇게 데이터가 들어있어
        // bonusDia[1] - bonusDia[0] 을 하여 구함.
        return currentDiaStashPackage.bonusDia[1] - currentDiaStashPackage.bonusDia[0];
    }

    public int GetAddBonusDia()
    {
        //유저가 획득한 보너스 다이아
        return ManagerDiaStash.instance.GetCurrentDia() - prevBonusDia;
    }

    public string GetPackagePrice(CdnShopPackage data)
    {
        string price = string.Empty;
        
#if UNITY_IPHONE
        price = data.sku_i;
#elif UNITY_ANDROID
        price = data.sku_a;
#else
        price = data.sku_a;
#endif
        
#if !UNITY_EDITOR
        if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(price))
            return ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[price].displayPrice;
#endif
        return LanguageUtility.GetPrices(data.prices);
    }

    /// <summary>
    /// 현재 단계의 다이아 주머니의 패키지 데이터 가져오기 위한 함수 
    /// </summary>
    public CdnShopPackage GetCurrentShopPackageData()
    {
        if (ServerContents.Packages.ContainsKey(currentDiaStashPackage.packageId[ServerRepos.UserDiaStash.stageClearCount]) == false)
            return null;
        
        return ServerContents.Packages[currentDiaStashPackage.packageId[ServerRepos.UserDiaStash.stageClearCount]];
    }

    public int GetFullDia()
    {
        //기본 지급 다이아 + 최대 보너스 다이아.
        int defaultDia = currentDiaStashPackage.defaultDia;
        int bonusDia = currentDiaStashPackage.bonusDia[currentDiaStashPackage.maxStageClear];
        
        return defaultDia + bonusDia;
    }

    public int GetFullBonusDia()
    {
        //최대 보너스 다이아
        return currentDiaStashPackage.bonusDia[currentDiaStashPackage.maxStageClear];
    }

    public int GetMinBonusDia()
    {
        //최소 보너스 다이아
        return currentDiaStashPackage.bonusDia[0];
    }

    public float GetCurrentStageValue()
    {
        // 유저의 현재 스테이지 클리어 횟수 / Max 스테이지 클리어 횟수
        float currentClearCount = ServerRepos.UserDiaStash.stageClearCount;

        return currentClearCount / currentDiaStashPackage.maxStageClear;
    }

    public int GetBuyCount()
    {
        int buyCount = 0;
        
        //현재 유저가 노출되어 있는 패키지를 구매한 횟수
        foreach (var packages in ServerRepos.UserShopPackages)
        {
            for (int i = 0; i < currentDiaStashPackage.packageId.Count; i++)
            {
                if (packages.idx == currentDiaStashPackage.packageId[i])
                {
                    buyCount += packages.buyCount;
                }
            }
        }
        
        return buyCount > GetPackageBuyLimit() ? GetPackageBuyLimit() : buyCount;
    }

    public int GetPackageBuyLimit()
    {
        return currentDiaStashPackage.maxBuyCount;
    }

    public bool IsFullDia()
    {
        //서버 패키지 데이터의 스테이지 최대 갯수 == 현재 유저 스테이지 클리어 횟수.
        return currentDiaStashPackage.maxStageClear == ServerRepos.UserDiaStash.stageClearCount;
    }

    public int GetCurrentPackageIdx()
    {
        return currentDiaStashPackage.packageId[ServerRepos.UserDiaStash.stageClearCount];
    }
    
    public GameEventType GetEventType()
    {
        return GameEventType.DIASTASH_EVENT;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerDiaStash>();

        if (instance == null)
            return;

        //데이터 초기화 할 때, 현재 유저의 스테이지 클리어 카운트 저장
        if (ServerRepos.UserDiaStash != null)
            ManagerDiaStash.instance.SyncPrevStageClearCount(ServerRepos.UserDiaStash.stageClearCount);
        else
            ManagerDiaStash.instance.SyncPrevStageClearCount(0);

        ManagerDiaStash.instance.SyncDiaStashPackageData();
        ManagerDiaStash.instance.SyncUserDiaCount();
        ManagerDiaStash.instance.SyncPrevBonusDiaCount();

        ManagerEvent.instance.RegisterEvent(instance);
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
        yield return ManagerDiaStash.instance.diaStashResource.CoSetDiaStashResource();
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
        var pakage = GetCurrentShopPackageData();
        if (CheckStartable() && pakage != null)
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.DiaStash, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerDiaStashEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.DiaStashEvent));
            });
        }
    }

    public void OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    #region 인게임에서 호출되는 함수
    public void SetStageApplyDiaStash(bool enable)
    { 
        isStageApplyDiaStash = enable;
    }

    public void SyncPrevStageClearCount(int currentGrade)
    { 
        prevStageClearCount = currentGrade;
    }

    public void SyncPrevBonusDiaCount()
    {
        prevBonusDia = ManagerDiaStash.instance.GetCurrentDia();
    }
    #endregion
}
