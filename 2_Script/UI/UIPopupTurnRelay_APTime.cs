using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIPopupTurnRelay_APTime : UIPopupBase
{
    public static UIPopupTurnRelay_APTime _instance = null;
    public UIItemADButton adButton;

    //코인량 표시
    [SerializeField] private UILabel[] labelCoinCount;

    //광고 표시
    [SerializeField] private BoxCollider colliderBtnAD;
    [SerializeField] private UISprite spriteBtnAD;
    [SerializeField] private UILabel[] labelADCount;

    [SerializeField] private GameObject objDescription;

    private System.Action rechargeAction = null;

    //코인량
    private int speedUpCoin = 0;

    #region 버튼 UI컬러 관련
    private Color activeBtnColor = new Color(34f / 255f, 100f / 255f, 14f / 255f, 0.7f);
    private Color inactiveBtnColor = new Color(90f / 255f, 90f / 255f, 90f / 255f, 0.7f);
    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    /// 팝업 초기화 함수
    public void InitPopUp(System.Action rechargeAction, System.Action endAction)
    {
        this.rechargeAction = rechargeAction;
        this._callbackClose += () => { endAction(); };
        InitSpeedUpCoin();
        InitSpeedUpAP();
        adButton.SetADType(AdManager.AdType.AD_5);
        
        //상품판매 법률 개정 표기
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
    }

    private void InitSpeedUpCoin()
    {
        speedUpCoin = ManagerTurnRelay.instance.APPrice;
        string coinText = speedUpCoin.ToString();
        for (int i = 0; i < labelCoinCount.Length; i++)
            labelCoinCount[i].text = coinText;
    }

    private void InitSpeedUpAP()
    {
        int aDCount_limit = ServerContents.AdInfos[(int)AdManager.AdType.AD_5].dailyLimit;

        int aDCount_used = 0;

        if (ServerRepos.UserAdInfos != null)
        {
            foreach(var userAdInfos in ServerRepos.UserAdInfos)
            {
                if(userAdInfos.adType == (int)AdManager.AdType.AD_5)
                {
                    aDCount_used = userAdInfos.usedCount;
                    break;
                }
            }
        }

        //텍스트 설정
        string adText = string.Format("{0}/{1}", (aDCount_limit - aDCount_used), aDCount_limit);

        //광고 볼 수 있는 상태인지 확인
        bool isCanWatchAd = (aDCount_used < aDCount_limit);

        //남은 광고 상태에 따라 버튼 UI 및 콜라이더 설정
        spriteBtnAD.spriteName = (isCanWatchAd == true) ? "button_play" : "button_play03";
        colliderBtnAD.enabled = isCanWatchAd;

        for (int i = 0; i < labelADCount.Length; i++)
        {
            labelADCount[i].text = adText;
            if (isCanWatchAd == true)
                labelADCount[i].effectColor = activeBtnColor;
            else
                labelADCount[i].effectColor = inactiveBtnColor;
        }
    }

    #region 코인 사용
    private void OnClickBtnSpeedUp_UseCoin()
    {
        if (bCanTouch == false)
            return;

        if (speedUpCoin <= Global.coin)
        {
            bCanTouch = false;
            ServerAPI.TurnRelayRechargeAPByCoin(RecvRechargeAP_UseCoin);
        }
        else
        {
            ManagerUI._instance.LackCoinsPopUp();
        }
    }

    private void RecvRechargeAP_UseCoin(TurnRelayRechargeAPByCoinResp resp)
    {
        bCanTouch = true;
        if (resp.IsSuccess)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)RewardType.coin,
                        -ServerContents.TurnRelayEvent.apPrice,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_TURNRELAY_AP,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                        $"TURNRELAY_{ServerContents.TurnRelayEvent.eventIndex}"
                        );

            //서버 데이터 갱신
            ManagerTurnRelay.instance.SyncFromServerUserData();

            //사운드 출력
            ManagerSound.AudioPlay(AudioLobby.UseClover);

            //UI 갱신
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();

            //그로씨
            GrouthyTurnRelay_APSpeedUp();

            //콜백 실행
            rechargeAction?.Invoke();

            ///팝업 닫기
            OnClickBtnClose();
        }
    }

    /// <summary>
    /// 그로씨 관련 처리는 해당 함수에서 설정
    /// </summary>
    private void GrouthyTurnRelay_APSpeedUp()
    {
        int useFCoin = 0;
        int usePCoin = 0;

        if ((int)(GameData.User.coin) > speedUpCoin)
        {
            usePCoin = speedUpCoin;
        }
        else if ((int)(GameData.User.coin) > 0)
        {
            useFCoin = speedUpCoin - (int)(GameData.User.coin);
            usePCoin = (int)(GameData.User.coin);
        }
        else
        {
            useFCoin = speedUpCoin;
        }
    }
    #endregion
}
