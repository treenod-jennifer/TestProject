using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIPopupTimeGiftBox : UIPopupBase
{
    public static UIPopupTimeGiftBox instance;

    public GameObject btnOpen;
    public GameObject btnSpeedUp;
    public GameObject description;

    public UILabel[] coinCount;
    public UILabel missionTime;

    int coin = 0;

    public ServerUserGiftBox GiftBoxData { get; private set; }

    public long BoxID { get { return GiftBoxData.index; } }
    public long BoxTime { get { return GiftBoxData.openTimer; } }

    private void Start()
    {
        instance = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (instance == this) instance = null;
    }

    public void InitPopUp(ServerUserGiftBox giftData)
    {
        GiftBoxData = giftData;
        long leftTime = Global.LeftTime(GiftBoxData.openTimer);

        if (leftTime <= 0)
        {
            btnOpen.SetActive(true);
        }
        else
        {
            btnSpeedUp.SetActive(true);
            StartCoroutine(CoMissionTimer());
        }

        ShowBuyInfo();
    }

    public IEnumerator CoMissionTimer()
    {
        long leftTime = Global.LeftTime(GiftBoxData.openTimer);
        while (leftTime > 0)
        {
            if (gameObject.activeInHierarchy == false)
                break;
            leftTime = Global.LeftTime(GiftBoxData.openTimer);
            if (leftTime >= 60)
            {
                missionTime.text = Global.GetTimeText_HHMMSS(GiftBoxData.openTimer);
            }
            else
            {
                missionTime.text = Global.GetTimeText_SS(GiftBoxData.openTimer);
            }

            if (leftTime <= 0)
            {
                break;
            }
            CoinSetting(leftTime);
            yield return null;
        }

        if (gameObject.activeInHierarchy == true)
        {
            missionTime.text = "00:00";
        }
    }

    void CoinSetting(long leftTime)
    {
        int timeCoin = 10;

        if (GiftBoxData.type == 0)
            timeCoin = 100;
        else if(GiftBoxData.type == 1)
            timeCoin = 100;
        else
            timeCoin = 100;

        int time = 0;
        if (leftTime < 3600)
        {
            coin = timeCoin;
        }
        else
        {
            time = (int)(leftTime / 3600);
        }

        coin = timeCoin + (timeCoin * time);
        coinCount[0].text = coin.ToString();
        coinCount[1].text = coin.ToString();
    }

    int nowCoin;

    void OnClickBtnSpeedUp()
    {
        if (bCanTouch == false)
            return;

        if (coin <= Global.coin)
        {
            bCanTouch = false;

            if ((int)(GameData.User.coin) > coin)
            {
                usePCoin = coin;
            }
            else if ((int)(GameData.User.coin) > 0)
            {
                useFCoin = coin - (int)(GameData.User.coin);
                usePCoin = (int)(GameData.User.coin);
            }
            else
            {
                useFCoin = coin;
            }

            ServerAPI.OpenGiftBoxWithCoin((int)GiftBoxData.index, recvGiftBoxTime);
        }
        else
        {
            ManagerUI._instance.LackCoinsPopUp();
        }
    }

    int useFCoin = 0;
    int usePCoin = 0;

    void recvGiftBoxTime(OpenGiftBoxResp resp)
    {
        if (resp.IsSuccess)
        {
            ObjectGiftbox.OpenBoxLog(false, GiftBoxData.type);

            bCanTouch = true;

            if(PlayerPrefs.HasKey("Giftbox" + GiftBoxData.index))
            {
                int index = PlayerPrefs.GetInt("Giftbox" + GiftBoxData.index);
                if (index < ManagerLobby._instance._spawnGiftBoxPosition.Length)
                    ManagerLobby._instance._spawnGiftBoxPosition[index].used = false;
                PlayerPrefs.DeleteKey("Giftbox" + GiftBoxData.index);
            }
            

            ManagerSound.AudioPlay(AudioLobby.UseClover);
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();
            ManagerLobby._instance.ReMakeGiftbox();
            _callbackEnd += OpenPopUpGiftBox;
            OnClickBtnClose();
            LocalNotification.RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, resp.id);

            //그로씨
            int rewardCoin = 0;
            foreach(var temp in GiftBoxData.rewardList)
            {
                if (temp.type == (int)RewardType.coin) rewardCoin = temp.value;
            }




            {
                var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_OPEN_PRESENT_BOX,
                   - usePCoin,
                   - useFCoin,
                    (int)(GameData.User.coin),
                    (int)(GameData.User.fcoin)- rewardCoin
                    );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
            }
        }
    }

    void OpenPopUpGiftBox()
    {
        UIPopUpOpenGiftBox popup = ManagerUI._instance.OpenPopupGiftBox();
        popup._data = GiftBoxData;

        QuestGameData.SetUserData();
        UIDiaryController._instance.UpdateQuestData(true);
    }

    public void OnClickShowChanceLanBoard()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_box", Global._instance.GetString("p_t_g_3"));
    }
    
    private void ShowBuyInfo()
    {
        description.SetActive(LanguageUtility.IsShowBuyInfo);
    }
}
