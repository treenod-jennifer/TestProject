using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopupContinue : UIPopupBase
{
    public static UIPopupContinue _instance = null;
    public UILabel stage;
    public List<UILabel> cost = new List<UILabel>();
    public List<UILabel> apple = new List<UILabel>();
    public UILabel info;
    public GameObject saleIcon;
    public GameObject live2DAnchor;
    public UISprite turnImage;

    //추가보너스
    public GameObject bonusRoot;
    public UISprite bonusSprite;
    public UISprite bonusSprite2;

    public bool pushContinue = false;

    //이벤트 용.
    public UITexture eventTextBox;
    public UILabel eventText;

    [HideInInspector]
    public LAppModelProxy boniLive2D;

    private int clearLayer = 1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {/*
        ServerUserStage stageData = ServerRepos.FindUserStage(Global.stageIndex, Global.eventIndex);
        if (stageData == null)
            return;
        if( (stageData.fail + 1) >= 5)
        {
            foreach( var p in ServerContents.Packages)
            {
                if (p.Value.type != 2)
                    continue;

                string prefKey = string.Format("spotDia_{0}", p.Value.idx);
                bool alreadyBuy = ServerRepos.UserShopPackages.Exists(x => { return x.idx == p.Value.idx; });

                // 한번이라도 구매한 적이라거나, 보여준 적 있으면 패스
                if (!alreadyBuy && PlayerPrefs.HasKey(prefKey) == false)
                {
                    ManagerUI._instance.OpenPopupPackage(ServerContents.Packages[p.Value.idx]);
                    break;
                }
            }
        }*/
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        ManagerSound._instance.PauseBGM();
        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL);
        BlockMatchManager.instance.DistroyAllLinker();
        SetCharacter();
        SetInGameMode();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        clearLayer = layer;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            clearLayer += 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public override void OnClickBtnBack()
    {
        OnClickBtnNo();
    }

    public void InitPopUp()
    {
        stage.text = string.Format("ステージ {0}", Global.stageIndex);
        stage.MakePixelPerfect();
        /*
        foreach (var item in cost)
        {
            if (GameManager.instance.useContinueCount > 4) item.text = ServerRepos.LoginCdn.ContinueCosts[ServerRepos.LoginCdn.ContinueCosts.Count-1].ToString();
            else item.text = ServerRepos.LoginCdn.ContinueCosts[GameManager.instance.useContinueCount].ToString();

            //item.text = ServerRepos.LoginCdn.ContinueCosts[GameManager.instance.useContinueCount].ToString();//ServerRepos.LoginCdn.ContinueCost.ToString(); //GameManager.instance.useContinueCount
        }

        if (ServerRepos.LoginCdn.ContinueSale == 1)
        {
            saleIcon.SetActive(true);
        }
        else
        {
            saleIcon.SetActive(false);
        }

        //현재 이벤트 스테이지 플레이 중이면 텍스트 띄우는 함수 호출.
        if (Global.eventIndex > 0)
        {
            SettingBoniDialogBubble();
        }*/
    }

    void SettingBoniDialogBubble()
    {
        //유저 데이터에서 현재 몇 번째 그룹에 있는지 받아와야함.
        int eventGroup = ManagerData._instance._eventChapterData[Global.eventIndex]._groupState;
        int eventStep = ManagerData._instance._eventChapterData[Global.eventIndex]._state;
        int eventType = 0;
        /*
        //현재 그룹에서의 스테이지 수 구해야 함.
        for (int i = 0; i < ServerContents.EventChapters.Count; i++)
        {
            if (ServerContents.EventChapters[i + 1].index == Global.eventIndex)
            {
                eventType = ServerContents.EventChapters[i + 1].type;
                //1번째 그룹 이상일 경우, 현재 스텝은 이전 그룹의 수만큼 빼 줘야함.
                if (eventGroup > 1)
                {
                    CdnEventChapter eventChapter = ServerContents.EventChapters[i + 1];
                    eventStep -= eventChapter.counts[eventGroup - 2];
                    if (eventStep < 0)
                    {
                        //Debug.Log("EventStep Error  -  EventStep" + eventStep);
                        return;
                    }
                }
            }
        }

        //트레이닝 모드에서 현재 그룹의 1번째 위치가 아니라면 "정말 끝낼거야?" 말풍선 생성.
        if (eventType == (int)EVENT_CHAPTER_TYPE.FAIL_RESET && Global.eventIndex > 0 && eventStep > 1)
        {
            eventTextBox.gameObject.SetActive(true);
            eventTextBox.transform.localScale = Vector3.one * 0.3f;
            eventTextBox.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutCirc);
            DOTween.ToAlpha(() => eventTextBox.color, x => eventTextBox.color = x, 1f, 0.2f);
            eventText.text = Global._instance.GetString("n_ev_2");
        }*/
    }

    void SetInGameMode()
    {
        if (GameManager.instance != null && GameManager.gameMode == GameMode.LAVA)
        {
            int count = ManagerBlock.instance.stageInfo.digCount * 3;

            foreach (var label in apple)
                label.text = "-" + count;

            turnImage.spriteName = "ingame_item_lava";
            turnImage.MakePixelPerfect();
            turnImage.width = 112;
            turnImage.height = 112;

            string infoText = Global._instance.GetString("p_ctn_4");


            if (GameManager.instance.useContinueCount == 0)
                infoText = Global._instance.GetString("p_ctn_4");
            else if (GameManager.instance.useContinueCount == 1)
                infoText = Global._instance.GetString("p_ctn_8");
            else if (GameManager.instance.useContinueCount == 2)
                infoText = Global._instance.GetString("p_ctn_9");
            else if (GameManager.instance.useContinueCount > 2)
                infoText = Global._instance.GetString("p_ctn_10");

            infoText = infoText.Replace("[n]", count.ToString());
            info.text = infoText;
        }
        else
        {
            int addCount = 5 + GameManager.instance.useContinueCount;
            if (addCount > 8) addCount = 8;

            foreach (var label in apple)
                label.text = addCount.ToString();

            turnImage.spriteName = "ingame_item_apple";
            turnImage.MakePixelPerfect();
            turnImage.width = 112;
            turnImage.height = 126;

            if (GameManager.instance.useContinueCount == 0)
                info.text = Global._instance.GetString("p_ctn_3");
            else if (GameManager.instance.useContinueCount == 1)
                info.text = Global._instance.GetString("p_ctn_5");
            else if (GameManager.instance.useContinueCount == 2)
                info.text = Global._instance.GetString("p_ctn_6");
            else if (GameManager.instance.useContinueCount > 2)
                info.text = Global._instance.GetString("p_ctn_7");
        }

        //추가 아이템 확인
        if (GameManager.instance.useContinueCount == 1)//라인추가
        {
            bonusRoot.gameObject.SetActive(true);
            bonusSprite.gameObject.SetActive(true);
            bonusSprite.spriteName = "icon_line_bomb_stroke";

            bonusSprite2.gameObject.SetActive(false);
        }
        else if (GameManager.instance.useContinueCount == 2)//더블추가
        {
            bonusRoot.gameObject.SetActive(true);
            bonusSprite.gameObject.SetActive(true);
            bonusSprite.spriteName = "icon_bomb_stroke";

            bonusSprite2.gameObject.SetActive(false);
        }
        else if (GameManager.instance.useContinueCount >= 3)//라인+더블추가
        {
            bonusRoot.gameObject.SetActive(true);

            bonusSprite.gameObject.SetActive(true);
            bonusSprite.spriteName = "icon_line_bomb_stroke";

            bonusSprite2.gameObject.SetActive(true);
            bonusSprite2.spriteName = "icon_bomb_stroke";
        }

    }

    void SetCharacter()
    {
        boniLive2D = NGUITools.AddChild(live2DAnchor, ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni].obj).GetComponent<LAppModelProxy>();
        boniLive2D.SetScale(450f);
        boniLive2D.gameObject.transform.localPosition = new Vector3(150f, 110f, 0f);
        boniLive2D.setAnimation(false, "Fail_01_appear");    //모션.
        boniLive2D._CubismRender.SortingOrder = clearLayer;
    }
    /*
    void recvGameContinue(GameActionResp resp)
    {
        if (resp.IsSuccess)
        {
            Global.jewel = (int)(GameData.Asset.AllJewel);
            ManagerUI._instance.UpdateUI();
            networkEnd = true;

            GameManager.instance.useContinueCount++;

            boniLive2D.setAnimation(false, "Continue_01");    //모션.
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            if (UIPopupCoinShop._instance != null) UIPopupCoinShop._instance.OnClickBtnBack();
            if (UIPopupDiamondShop._instance != null) UIPopupDiamondShop._instance.OnClickBtnBack();

            pushContinue = false;

            StartCoroutine(CoEvent(1.5f, () =>
            {
                ManagerSound._instance.PlayBGM();
                ManagerUI._instance.ClosePopUpUI();

                if (GameManager.gameMode == GameMode.NORMAL)
                {
                    GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
                }
                else if (GameManager.gameMode == GameMode.DIG)
                {
                    GameManager.instance.GetTurnCount(GameManager.instance.useContinueCount);
                }
                else if (GameManager.gameMode == GameMode.LAVA)
                {
                    ManagerBlock.instance.RecoverLavaMode(GameManager.instance.useContinueCount);
                }
                else
                {
                    GameManager.instance.state = GameState.PLAY;
                }
            }));


            //그로씨
            var growthyMoneyrecvGameContinue = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_CONTINUE_PLAY,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel)
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
    */
    int useFJewel = 0;
    int usePJewel = 0;

    float yesTimer = 0;

    void OnClickBtnYes()
    {/*
        if (bCanTouch == false)
            return;
        if (GameManager.instance.useContinueCount > 4)
        {
            if (Global.jewel < ServerRepos.LoginCdn.ContinueCosts[ServerRepos.LoginCdn.ContinueCosts.Count - 1])
            {
                ManagerUI._instance.LackDiamondsPopUp();
                return;
            }
        }
        else
        {
            if (Global.jewel < ServerRepos.LoginCdn.ContinueCosts[GameManager.instance.useContinueCount])
            {
                ManagerUI._instance.LackDiamondsPopUp();
                return;
            }
        }


        if (bCanTouch == false)
            return;
        bCanTouch = false;

        pushContinue = true;

        // 서버와 통신
        {
            int price = 0;
            if(GameManager.instance.useContinueCount > 4) price = ServerRepos.LoginCdn.ContinueCosts[ServerRepos.LoginCdn.ContinueCosts.Count - 1];
            else price = ServerRepos.LoginCdn.ContinueCosts[GameManager.instance.useContinueCount];

            if ((int)ServerRepos.User.jewel > price) usePJewel = price;
            else if ((int)ServerRepos.User.jewel > 0)
            {
                usePJewel = (int)ServerRepos.User.jewel;
                useFJewel = price - (int)ServerRepos.User.jewel;
            }
            else useFJewel = price;


            var req = new GameContinueReq()
            {
                stage = Global.stageIndex,
                stageEx = 0,
                eventIdx = Global.eventIndex,
                isEvent = Global.eventIndex > 0 ? 1 : 0,
            };
            ServerAPI.GameContinue(req, recvGameContinue);
        }*/
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        pushContinue = false;
    }


    void OnClickBtnNo()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        boniLive2D.setAnimation(false, "Continue_02_start");    //모션.

        pushContinue = false;

        // 0~5 레디아이템, 6~9 인게임아이템, 10 컨티뉴횟수 
        var usedItems = new List<int>();
        usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameManager.instance.useContinueCount);

        var remainTarget = ManagerBlock.instance.GetListRemainTarget();

        /*
        var arg = new GameFailReq()
        {
            type = (int)Global.GameType,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            isEvent = Global.GameInstance.IsEvent() ? 1 : 0,
            chapter = Global.GameInstance.GetChapterIdx(),
            gameCoin = 0,
            gameScore = 0,
            gameRemains = remainTarget,
            stageKey = Global.GameInstance.GetStageKey(),
            seed = ServerRepos.IngameSeed,
            easyMode = GameManager.instance.LevelAdjusted == true ? 1 : 0
            //items = usedItems
        };

        ServerAPI.GameFail(arg, recvGameFail);

        if (Global.GameType == GameType.NORMAL)
            FailCountManager._instance.SetFail(Global.stageIndex.ToString());
    }
    /*
    void recvGameFail(GameActionResp resp)
    {
        //Debug.Log("GameFail: " + resp);
        if (resp.IsSuccess)
        {
            if(uiingameBoostingGauge.gameObject.activeInHierarchy == true)
                uiingameBoostingGauge.gameObject.SetActive(false);
            Global.GameInstance.OnRecvGameFail();
            ManagerUI._instance.OpenPopupFail(resp.dcChanged == true);
            resultChat.uiPanel.depth += (panelCount + 1);
            resultChat.scrollView.panel.depth += (panelCount + 1);
            StartCoroutine(CoCloseBoniDialogBubble());
        }
        else
        {
            bCanTouch = true;
            OnClickBtnNo();
        }
    }
    */
        IEnumerator CoCloseBoniDialogBubble()
        {
            eventTextBox.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCirc);
            DOTween.ToAlpha(() => eventTextBox.color, x => eventTextBox.color = x, 0f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            eventTextBox.gameObject.SetActive(false);
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
    }
}
