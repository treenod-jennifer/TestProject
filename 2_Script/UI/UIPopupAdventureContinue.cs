using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIPopupAdventureContinue :  UIPopupBase {

    public bool inFalseClickDelay = true;

    [Header("Linked Object")]
    [SerializeField] private UISprite continueIcon;
    [SerializeField] private UILabel[] continueIconValue;
    [SerializeField] private UILabel explanation;
    [SerializeField] private GameObject saleIcon;
    [SerializeField] private UILabel[] price;
    [SerializeField] private UILabel stage;
    [SerializeField] private UILabel wave;
    [SerializeField] private GenericReward coinReward;
    [SerializeField] private GameObject boxReward;

    //상품판매 법률 개정 관련.
    [SerializeField] private GameObject objDescription;
    
    private bool SaleActive
    {
        set
        {
            saleIcon.SetActive(value);

            for (int i = 0; i < price.Length; i++)
                price[i].text = ServerRepos.LoginCdn.AdvContinuePrice.ToString();
        }
        get
        {
            return saleIcon.activeSelf;
        }
    }

    private void Start()
    {
        StartCoroutine(ProceedFalseClickDelay());

        //testcase
        SaleActive = ServerRepos.LoginCdn.AdvContinueSale != 0;
        //SetContinueIcon("item_Skill_charge", 13);

        string temp = Global._instance.GetString("ad_ctn_1");
        temp.Replace("[n]", "10");
        explanation.text = temp;

        if(Global.GameType == GameType.ADVENTURE)
        {
            stage.text = Global.stageIndex > 100 ?
                $"{Global._instance.GetString("p_sr_1")} C{Global.chapterIndex} - {Global.stageIndex % 100}" : 
                $"{Global._instance.GetString("p_sr_1")} {Global.chapterIndex} - {Global.stageIndex}";
        }
        else if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            stage.text = $"{Global._instance.GetString("p_sr_1")} {Global.stageIndex}";
        }
        
        wave.text = "Wave" + (AdventureManager.instance.waveCount + 1) + "/" + ManagerBlock.instance.stageInfo.battleWaveList.Count;

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        ManagerSound._instance.PauseBGM();

        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        ManagerUI._instance.bTouchTopUI = true;

        //상품판매 법률 개정 관련
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
        
        SetReward();
    }

    private void SetReward()
    {
        if (ManagerBlock.instance.coins == 0)
            coinReward.gameObject.SetActive(false);
        else
        {
            coinReward.gameObject.SetActive(true);

            Reward reward = new Reward();
            reward.type = 2;
            reward.value = ManagerBlock.instance.coins;

            coinReward.SetReward(reward);
        }

        if (AdventureManager.instance.TreasureCnt == 0)
        {
            boxReward.SetActive(false);

            foreach (var enemy in AdventureManager.instance.EnemyLIst)
            {
                if (enemy.treasure.gameObject.activeSelf)
                {
                    boxReward.SetActive(true);
                    break;
                }
            }
        }
        else
            boxReward.SetActive(true);
    }

    private void SetContinueIcon(string iconName, int value)
    {
        for(int i=0; i< continueIconValue.Length; i++)
        {
            continueIconValue[i].text = (value == 0) ? "" : value.ToString();
        }

        continueIcon.spriteName = iconName;
        continueIcon.MakePixelPerfect();
    }

    IEnumerator ProceedFalseClickDelay()
    {
        this.inFalseClickDelay = true;

        yield return new WaitForSeconds(0.5f);
        
        this.inFalseClickDelay = false;
    }

    public void OnClickBtnYes()
    {
        if (this.inFalseClickDelay)
            return;

        if (bCanTouch == false)
            return;

        if (Global.jewel < ServerRepos.LoginCdn.AdvContinuePrice)
        {
            ManagerUI._instance.LackDiamondsPopUp();
            return;
        }

        bCanTouch = false;

        var req = new Protocol.AdventureGameContinueReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            chapter = Global.chapterIndex,
            stage = Global.stageIndex,
        };
        ServerAPI.AdventureGameContinue(req, recvGameContinue);
    }

    public override void OnClickBtnBack()
    {
        OnClickBtnNo();
    }

    public void OnClickBtnNo()
    {
        if (!bCanTouch)
            return;  
        
        base.OnClickBtnClose();

        int price = ServerRepos.LoginCdn.AdvContinuePrice;

        if ((int)ServerRepos.User.jewel >= price) usePJewel = price;
        else if ((int)ServerRepos.User.jewel > 0)
        {
            usePJewel = (int)ServerRepos.User.jewel;
            useFJewel = price - (int)ServerRepos.User.jewel;
        }
        else useFJewel = price;

        var arg = new Protocol.AdventureGameFailReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            stage = Global.stageIndex,
            chapter = Global.chapterIndex,
            coin = ManagerBlock.instance.coins,
            deckId = 1,
        };

        ServerAPI.AdventureGameFail(arg, recvAdventureGameFail);

    }

    void recvAdventureGameFail(Protocol.AdventureGameFailResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupAdventureFail._failResp = resp;
            ManagerUI._instance.OpenPopupAdventureFail();
            ManagerAdventure.User.SyncFromServer_Animal();
        }
    }

    bool networkEnd = false;
    IEnumerator CoEvent(float waitTime, UnityAction action)
    {
        float timer = 0;
        while (timer < waitTime)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        // 네트워크 통신이 완료될때까지 기다림
        while (!networkEnd)
            yield return null;

        action();
        yield return null;
    }

    int useFJewel = 0;
    int usePJewel = 0;

    void recvGameContinue(Protocol.AdventureGameContinueResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.jewel = (int)(GameData.Asset.AllJewel);
            Global.wing = (int)(GameData.Asset.AllWing);
            Global.exp = (int)GameData.User.expBall;

            ManagerUI._instance.UpdateUI();
            networkEnd = true;

            GameManager.instance.useContinueCount++;

            // TODO : 실제 게임 컨티뉴 처리
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            if (UIPopupShop._instance != null) UIPopupShop._instance.OnClickBtnBack();

            StartCoroutine(CoEvent(1.5f, () =>
            {
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.ClosePopUpUI();
                GameManager.instance.IsCanTouch = true;
                ManagerBlock.instance.state = BlockManagrState.WAIT;
                if (Global.GameInstance.GetProp(GameTypeProp.REOPEN_INGAMEITEM_AT_CONTINUE) == true)
                    GameUIManager.instance.ReOpenIngameItem();
                AdventureManager.instance.ContinueGame();
                GameManager.instance.GetAdventureCircleBomb();
            }));

            //    if (GameManager.instance.gameOverByDynamite)
            //    {
            //        //다이나마이트카운트 조절
            //        ManagerBlock.instance.ResetDynamite();
            //        GameManager.instance.gameOverByDynamite = false;
            //    }

            //    if (GameManager.gameMode == GameMode.NORMAL)
            //    {
            //        GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
            //    }
            //    else if (GameManager.gameMode == GameMode.DIG)
            //    {
            //        GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
            //    }
            //    else if (GameManager.gameMode == GameMode.LAVA)
            //    {
            //        ManagerBlock.instance.RecoverLavaMode(GameManager.instance.useContinueCount);
            //    }
            //    else
            //    {
            //        GameManager.instance.state = GameState.PLAY;
            //    }
            //}));


            //그로씨
            var growthyMoneyrecvGameContinue = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_CONTINUE_PLAY,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel),
                    mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
            var docMoney = JsonConvert.SerializeObject(growthyMoneyrecvGameContinue);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }
}
