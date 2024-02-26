using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCoinStashEvent : MonoBehaviour, IEventBase
{
    public static ManagerCoinStashEvent Instance { get; private set; } = null;

    public enum CoinStashState
    {
        NOT_BUY_COINLACK,
        NOT_BUY_COUNTLACK,
        BUY_COINSTASH,
        BUY_FULLCOIN,
    }

    private static CoinStashState coinStashState = CoinStashState.NOT_BUY_COINLACK;

    public static int currentUserCoin = 0;

    public static int currentCoinMultiplierState = 0;

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

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if(ServerContents.CoinStashEvent == null || ServerContents.CoinStashEvent.eventIndex == 0)
        {
            return false;
        }

        if (Global.LeftTime(ServerContents.CoinStashEvent.end_ts) <= 0)
        {
            return false;
        }

        if (GetCoinStashState() == CoinStashState.NOT_BUY_COUNTLACK)
        {
            return false;
        }

        return true;
    }

    public static bool IsBuyCoinStash()
    {
        if(GetCoinStashState() > CoinStashState.NOT_BUY_COUNTLACK)
            return true;
        else
            return false;
    }

    public static string GetIsFullCoin(int coinCount)
    {
        string text = "";

        if(coinCount == ServerContents.CoinStashEvent.coinMax)
            text = "FULL";
        else
            text = coinCount.ToString();

        return text;
    }

    public static CoinStashState GetCoinStashState()
    {
        int userCoin = ServerRepos.UserCoinStash.storedCoin;
        int minCoin = ServerContents.CoinStashEvent.coinMin;
        int maxCoin = ServerContents.CoinStashEvent.coinMax;

        if (ServerContents.CoinStashEvent.maxBuyCount > 0 && ServerRepos.UserCoinStash.buyCount >= ServerContents.CoinStashEvent.maxBuyCount)
        {
            coinStashState = CoinStashState.NOT_BUY_COUNTLACK;
        }
        else
        {
            if (userCoin >= maxCoin)
            {
                coinStashState = CoinStashState.BUY_FULLCOIN;
            }
            else if (userCoin < minCoin)
            {
                coinStashState = CoinStashState.NOT_BUY_COINLACK;
            }
            else
            {
                coinStashState = CoinStashState.BUY_COINSTASH;
            }
        }

        return coinStashState;
    }

    public static int GetBonusCoin(int coin)
    {
        int value = ServerContents.CoinStashEvent.price * 100;

        return coin - value > 0 ? coin - value : 0;
    }

    public GameEventType GetEventType()
    {
        return GameEventType.COINSTASH_EVENT;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (Instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerCoinStashEvent>();

        if (Instance == null)
            return;

        currentUserCoin = ServerRepos.UserCoinStash.storedCoin;

        ManagerEvent.instance.RegisterEvent(Instance);
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
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.none,() =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerCoinStashEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.CoinStashEvent));
            });
        }
    }

    public void OnReboot()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }
}