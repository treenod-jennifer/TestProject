using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using ServiceSDK;
using Protocol;
using UnityEngine.Serialization;

[IsReadyPopup]
public class UIPopupReady_EndContents : UIPopupReadyBase
{
    //포코 코인 이벤트
    [SerializeField] private GameObject RefreshButton;
    [SerializeField] private GameObject CoinBubble;
    [SerializeField] private GameObject RefreshSaleObject;
    
    [SerializeField] private UISprite FinalStageObj;
    [SerializeField] private UISprite readyUI;
    [SerializeField] private UILabel RefreshPrice;
    [SerializeField] private UILabel APTimer;
    
    [SerializeField] private UIItemEndContentsBoost BuffObj;
    [SerializeField] private UIItemLanpageButton Lanpage;
    
    //앤티크 스토어 이벤트
    [SerializeField] private UIUrlTexture texAntiqueStoreEventIcon;

    //이벤트 아이콘 Grid
    [SerializeField] private UIGrid gridEndContentsEventIcon;
    
    private Coroutine timerCoroutine;

    public override void InitPopUp(StageMapData stageData)
    {
        base.InitPopUp(stageData);
        
        readyUI.pivot = UIWidget.Pivot.Bottom;
        readyUI.height = 1000;
        readyUI.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? -568 : readyUI.transform.localPosition.y);
        readyBox.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? 515 : 575);

        InitReadyPopupPostLoadData();
        InitReadyPopUpApUI();
        
        //이벤트 아이콘 관련
        SetGroupRankingEvent();
        InitAntiqueStoreEvent();
        
        //이벤트 아이콘 정렬
        gridEndContentsEventIcon.Reposition();
    }

    private void InitReadyPopupPostLoadData()
    {
        //AP 타이머
        CheckAPChargeTimer();
        
        //맵 변경 관련
        RefreshSaleObject.SetActive(ManagerEndContentsEvent.instance.MapChangeSale);
        RefreshPrice.text = ManagerEndContentsEvent.instance.IsFirstMapChange ? Global._instance.GetString("btn_41") : ManagerEndContentsEvent.instance.MapChangeCoin.ToString();
        
        //랜 페이지
        Lanpage.gameObject.SetActive(true);
        Lanpage.On("LGPKV_mysterious_island", Global._instance.GetString("p_ec_4"));
        
        // 인게임 중 재시작 시 스코어업 버프 초기화 되는 부분 UI로 출력
        int buffCount = SceneManager.GetActiveScene().name == "InGame" ? 0 : ManagerEndContentsEvent.instance.Buff;
        BuffObj.Init(buffCount);
        BuffObj.transform.localPosition = new Vector2(BuffObj.transform.localPosition.x, LanguageUtility.IsShowBuyInfo ? 200f : 260f);
        
        //플레이가 불가능한 상황에서의 설정
        if (!ManagerEndContentsEvent.instance.CanPlayEndContentsStage())
        {
            if (!LanguageUtility.IsShowBuyInfo)
            {
                FinalStageObj.transform.localPosition = new Vector2(FinalStageObj.transform.localPosition.x, -100f);
                FinalStageObj.height = 600;
            }

            FinalStageObj.gameObject.SetActive(true);
            RefreshButton.SetActive(false);
            CoinBubble.SetActive(false);
            startButton.SetButton_Enable(false);
        }
        if (SceneManager.GetActiveScene().name == "InGame")
            RefreshButton.SetActive(false);
    }

    private void InitReadyPopUpApUI()
    {
        string currentAP = String.Format("{0}/", ManagerEndContentsEvent.instance.EventAP);
        string allAP = ManagerEndContentsEvent.instance.MaxEventAP.ToString();
        startButton.SetButtonText_AP(currentAP, allAP);
        CoinBubble.SetActive(ManagerEndContentsEvent.instance.EventAP <= 0 && ManagerEndContentsEvent.instance.CanPlayEndContentsStage());
    }
   
    private void CheckAPChargeTimer()
    {
        if (ManagerEndContentsEvent.instance.EventAP >= ManagerEndContentsEvent.instance.MaxEventAP)
            APTimer.text = Global._instance.GetString("p_t_tr_3");
        else
            EndTsTimer.Run(target: APTimer, endTs: ManagerEndContentsEvent.instance.ApChargeAt, timeOutAction: SetApChargeTime);
    }

    private void SetApChargeTime()
    {
        if (ManagerEndContentsEvent.instance.EventAP >= ManagerEndContentsEvent.instance.MaxEventAP)
            APTimer.text = Global._instance.GetString("p_t_tr_3");
        else
            APTimer.text = "00:00:00";

        bCanTouch = false;
        ServerAPI.EndContentsRefreshAp((resp) =>
        {
            if (resp.IsSuccess)
            {
                ManagerEndContentsEvent.instance.SyncFromServerUserData_AP();
                InitReadyPopUpApUI();
                CheckAPChargeTimer();
            }
            bCanTouch = true;
        });
    }

    protected override void OnClickGoInGame()
    {
        if (!ManagerEndContentsEvent.instance.CanPlayEndContentsStage())
        {
            return;
        }

        if (texAntiqueStoreEventIcon.gameObject.activeSelf)
        {
            if (ManagerAntiqueStore.instance != null && ManagerAntiqueStore.CheckStartable())
            {
                ManagerAntiqueStore.instance.SyncAntiqueStoreSpecialEvent();
            }
        }
        
        var needRefreshPopup = false;
        var refreshPopupKey  = "";
        if(_groupRankingEventTexture.gameObject.activeSelf)
        {
            if (ManagerGroupRanking.IsGroupRankingStage() == false)
            {
                needRefreshPopup = true;
                refreshPopupKey  = "n_s_54";
            }
        }
        
        if (needRefreshPopup)
        {
            _callbackClose = () => { ManagerUI._instance.OpenPopupReadyEndContents(); };
            bCanTouch      = true;

            var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(refreshPopupKey), false, OnClickBtnClose);
            popup.SortOrderSetting();
            return;
        }
        
        base.OnClickGoInGame();
    }

    protected override void OnClickGoInGame_Custom(long startTime)
    {
        if (ManagerEndContentsEvent.GetMaxPokoCoin() <= ManagerEndContentsEvent.GetPokoCoin())
        {
            UIPopupSystem popupGuide = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
            popupGuide.InitSystemPopUp( Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ec_4"), false, null );
            bCanTouch = true;
        }
        else if (ManagerEndContentsEvent.instance.EventAP < 1)
        {
            UIPopupChargeAPTime chargePopup = ManagerUI._instance.OpenPopup<UIPopupChargeAPTime>();
            chargePopup.InitPopUp(Global._instance.GetString("p_t_tr_1"), Global._instance.GetString("p_t_tr_5"), Global._instance.GetString("buyinfo_tr_1"));
            chargePopup.InitButton("icon_coin_stroke_yellow", ManagerEndContentsEvent.instance.ApPrice.ToString(), APCharge); 
            bCanTouch = true;
        }
        else
        {
            if (ManagerEndContentsEvent.instance.EventAP > 0)
            {   //일시정지 팝업에서 게임시작 눌렀다면 재시작 처리
                if (UIPopupPause._instance != null && UIPopupPause._instance.preservedRestart != null)
                {
                    StartCoroutine(CoRestart(startTime));
                }
                else
                {   //게임 시작
                    bStartGame = true;
                    StartGame(startTime);
                }
            }
        }
    }
    
    private void OnClickBtnStageRefresh()
    {
        if (!ManagerEndContentsEvent.instance.CanPlayEndContentsStage())
            return;
        
        if (!bCanTouch)
            return;

        bCanTouch = false;
        if (Global.coin >= ManagerEndContentsEvent.instance.MapChangeCoin || ManagerEndContentsEvent.instance.IsFirstMapChange)
        {
            bCanTouch = false;
            ServerAPI.EndContentsRefreshMap(RecvEndContent);
        }
        else
        {
            ManagerUI._instance.LackCoinsPopUp();
            bCanTouch = true;
        }
    }

    private void RecvEndContent(EndContentsResp resp)
    {   
        if (resp.IsSuccess == true)
        {
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            ManagerEndContentsEvent.instance.SyncFromServerUserData_PostRefreshMap();

            //아이템 선택 가격 해제
            ManagerUI._instance.InitActionCoin(ManagerUI.CurrencyType.READY_ITEM);
            ManagerUI._instance.InitActionJewel(ManagerUI.CurrencyType.READY_ITEM);
            
            //재화 갱신
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            
            //그로시 전송
            string mrsn_DTL = ServerRepos.IsPurchasedUser() ? $"PU_{Global.stageIndex}" : $"NPU_{Global.stageIndex}";
            Global.SetGameType_EndContents(ManagerEndContentsEvent.instance.EventIndex, ManagerEndContentsEvent.instance.MapIndex);
            SendGrowthy_UseCoin(ManagerEndContentsEvent.instance.MapChangeCoin, GrowthyCustomLog_Money.Code_L_MRSN.U_END_CONTENTS_CHANGEMAP, mrsn_DTL);
            
            //맵 교체(맵 로딩 이후, 상단 재화 UI 갱신됨)
            StartCoroutine(CoPostStageRefresh());
        }
        bCanTouch = true;
    }

    private void OnClickBtnEndContentsExchageStation()
    {
        UIPopupEndContentsEventExchangeStation chargePopup = ManagerUI._instance.OpenPopup<UIPopupEndContentsEventExchangeStation>();
        chargePopup.InitPopup();
    }

    private IEnumerator CoPostStageRefresh()
    {
        yield return CoCheckStageDataBeforeOpenPopUpReady();
        ManagerUI._instance.UpdateUI();
        SetReadyItem(tempData.gameMode == (int)GameMode.LAVA);
        RefreshPrice.text = ManagerEndContentsEvent.instance.IsFirstMapChange ? 
            Global._instance.GetString("icon_3") : ManagerEndContentsEvent.instance.MapChangeCoin.ToString();
    }
    
    private void APCharge()
    {
        if (Global.coin >= ManagerEndContentsEvent.instance.ApPrice)
        {
            bCanTouch = false;
            ServerAPI.EndContentChargeAp((resp) =>
            {
                if (resp.IsSuccess)
                {
                    ManagerSound.AudioPlay(AudioLobby.UseClover);
                    ServerAPI.RecvEndContentsChargeAp(resp);
                    InitReadyPopUpApUI();
                    string mrsn_DTL = ServerRepos.IsPurchasedUser() ? $"PU_{ManagerEndContentsEvent.instance.EventAP}" : $"NPU_{ManagerEndContentsEvent.instance.EventAP}";
                    SendGrowthy_UseCoin(ManagerEndContentsEvent.instance.ApPrice, GrowthyCustomLog_Money.Code_L_MRSN.U_END_CONTENTS_CHARGE_AP, mrsn_DTL);
                }
                bCanTouch = true;
            });
        }
        else
            ManagerUI._instance.LackCoinsPopUp();
    }

    private void SendGrowthy_UseCoin(int coin, GrowthyCustomLog_Money.Code_L_MRSN mrsn, string mrsn_DTL = null)
    {
        int usePCoin = 0;
        int useFCoin = 0;
                    
        if ((int)GameData.User.coin > coin) usePCoin = coin;
        else if ((int)GameData.User.coin > 0)
        {
            usePCoin = (int)GameData.User.coin;
            useFCoin = coin - (int)GameData.User.coin;
        }
        else useFCoin = coin;
                    
        var growthyMoney = new GrowthyCustomLog_Money
        (
            GrowthyCustomLog_Money.Code_L_TAG.SC,
            mrsn,
            -usePCoin,
            -useFCoin,
            (int)(ServerRepos.User.coin),
            (int)(ServerRepos.User.fcoin),
            mrsn_DTL
        );
        var docMoney = JsonConvert.SerializeObject(growthyMoney);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
    }

    #region 이벤트 아이콘 관련

    private void InitAntiqueStoreEvent()
    {
        if (ManagerAntiqueStore.instance != null && ManagerAntiqueStore.CheckStartable())
        {
            texAntiqueStoreEventIcon.gameObject.SetActive(true);
            texAntiqueStoreEventIcon.LoadCDN(Global.gameImageDirectory, "IconEvent/",
                $"asEventReady_{(ManagerAntiqueStore.IsSpecialEventCheck() ? "2" : "1")}_{ServerContents.AntiqueStore.resourceIndex}");
        }
        else
        {
            texAntiqueStoreEventIcon.gameObject.SetActive(false);
        }
    }

    private void SetGroupRankingEvent()
    {
        if (_groupRankingEventTexture == null)
        {
            return;
        }

        if (ManagerGroupRanking.IsGroupRankingStage())
        {
            _groupRankingEventTexture.gameObject.SetActive(true);
            _groupRankingEventTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"grEventReady");
        }
        else
        {
            _groupRankingEventTexture.gameObject.SetActive(false);
        }
    }
    
    #endregion
}
