using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Protocol;
using Spine.Unity;

//테스트
using System.IO;
using System.Xml.Serialization;

[IsReadyPopup]
public class UIPopupReadyCoinStage : UIPopupBase
{
    public static UIPopupReadyCoinStage instance = null;

    #region 이미지
    public Texture2D textureBackground;
    public Texture2D textureComplete;
    public UITexture textureBg;
    public UITexture textureCompleteIcon;
    public UISprite spriteButton;    
    #endregion

    #region 텍스트
    public UILabel[] title;
    public UILabel[] button;
    public UILabel textTime;
    public UILabel textStage;
    public UILabel textCondition;
    public UILabel textMission;
    public UILabel textTarget;
    #endregion

    #region 콜라이더
    public BoxCollider colliderBtn;
    #endregion

    #region 오브젝트
    public GameObject objTimeRoot;
    public GameObject objStartButton;
    public GameObject objCompelteIcon;
    #endregion

    #region 이펙트
    public GameObject _objEFfectUseClover;
    public GameObject _objEffectStartButton;
    public GameObject _objRingGlow;
    public GameObject _objEffectButton;
    #endregion

    #region 스파인
    public SkeletonAnimation coinStageNpc;
    #endregion

    #region 코인 배수 이벤트
    public GameObject coinEventRoot;
    public UILabel coinEventLabel;
    #endregion

    private ManagerCoinBonusStage.CoinBonusStageQuest currentQuestData = null;
    private int stagePlayTime = 0;
    private bool _recvGameStart_end = false;
    private bool isCompleteStage = false;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if( instance == this )
            instance = null;

        base.OnDestroy();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //레디창 전에 팝업들이 sortOrder을 사용하지 않는다면 live2d 쪽만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
        }
        if (coinStageNpc != null)
            coinStageNpc.gameObject.GetComponent<MeshRenderer>().sortingOrder = uiPanel.sortingOrder + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup(int time)
    {
        stagePlayTime = time;
        if (stagePlayTime == -1)
            isCompleteStage = true;
        InitData();
        InitSpine();
        InitTexture();
        InitText();
        InitCompleteIcon();
        InitCoinEvent();
    }

    private void InitData()
    {
        currentQuestData = ManagerCoinBonusStage.instance.GetCurrentQuestState();

        if (PlayerPrefs.HasKey("CoinStageTutorial_Ingame") == false)
        {
            if (ManagerCoinBonusStage.instance.GetStageIndex() > 1)
            {
                PlayerPrefs.SetInt("CoinStageTutorial_Ingame", 1);
            }
        }
    }

    private void InitSpine()
    {
        if (currentQuestData.state == 1)
        {
            coinStageNpc.state.SetAnimation(0, "1_idle", true);
        }
        else
        {
            coinStageNpc.state.SetAnimation(0, "1_ready_idle2", true);
        }
    }

    private void InitTexture()
    {
        textureBg.mainTexture = textureBackground;
    }

    private void InitText()
    {
        string titleText = Global._instance.GetString("p_cs_sr_1");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }

        string buttonText = Global._instance.GetString("btn_3");
        for (int i = 0; i < button.Length; i++)
        {
            button[i].text = buttonText;
        }

        textCondition.text = Global._instance.GetString("p_cs_sr_3");

        //조건 검사 하거나 미션 타입 검사해서 미션 넣어주기.
        string missionKey = string.Format("p_cs_m_{0}", currentQuestData.type);
        string missionText = Global._instance.GetString(missionKey).Replace("[n]", currentQuestData.targetCount.ToString());

        //미션 달성도 표시
        if (currentQuestData.type != 0)
        {
            int progress = (currentQuestData.progress > currentQuestData.targetCount) ?
                currentQuestData.targetCount : currentQuestData.progress;
            missionText = string.Format("{0}({1}/{2})", missionText, currentQuestData.progress, currentQuestData.targetCount);
        }
        textMission.text = missionText;
        
        //조건 검사해서 목표 넣어주기.
        if (isCompleteStage == false)
        {
            string stageText = Global._instance.GetString("p_cs_sr_2").Replace("[n]", ManagerCoinBonusStage.instance.GetStageIndex().ToString());
            textStage.text = stageText;

            string targetText = Global._instance.GetString("p_cs_tag_1").Replace("[n]", stagePlayTime.ToString());
            textTarget.text = targetText;

            textTime.text = stagePlayTime.ToString();
        }
        else
        {
            textStage.text = Global._instance.GetString("p_cs_sr_4");
            textTarget.text = Global._instance.GetString("p_cs_tag_2");
            objTimeRoot.SetActive(false);
        }
    }

    private void InitCompleteIcon()
    {
        if (isCompleteStage == true)
        {
            textureCompleteIcon.mainTexture = textureComplete;
            textureCompleteIcon.gameObject.SetActive(true);
        }
        else
        {
            textureCompleteIcon.gameObject.SetActive(false);
        }
    }

    void InitCoinEvent()
    {
        if (Global.coinEvent > 0)
        {
            coinEventRoot.SetActive(true);
            coinEventLabel.text = "x" + Global.coinEvent;
        }
        else
        {
            coinEventRoot.SetActive(false);
        }
    }

    public void OnClickGoInGame()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        switch (currentQuestData.state)
        {
            case 0:
                OpenStageReadyPopup();
                break;
            case 1:
                StartCoinStage();
                break;
            default:
                OpenSystemPopup();
                break;
        }
    }

    void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //게임 시작 설정
            ManagerLobby._stageStart = true;

            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            QuestGameData.SetUserData();

            if (ManagerCoinStashEvent.CheckStartable())
                ManagerCoinStashEvent.currentCoinMultiplierState = ServerRepos.UserCoinStash.multiplier;

            ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
            ManagerSound._instance.StopBGM();
            SceneLoading.MakeSceneLoading("InGame");
            ManagerUI._instance.bTouchTopUI = true;
        }
    }

    private void OpenStageReadyPopup()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "OpenPopupReadyLastStage", gameObject, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_cs_sr_6"), false, () => { bCanTouch = true; });
        popupSystem.SetResourceImage("Message/ok");
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_37"));
        popupSystem.SetButtonTextSpacing(1, 0);
    }

    private void OpenPopupReadyLastStage()
    {
        ManagerUI._instance.OpenPopupReadyLastStage(false);
        bCanTouch = true;
    }

    private void StartCoinStage()
    {
        for (int i = 0; i < 8; i++)
        {
            UIPopupReady.readyItemUseCount[i] = new EncValue();
            UIPopupReady.readyItemUseCount[i].Value = 0;
        }
        ManagerSound.AudioPlay(AudioLobby.UseClover);

        var req = new GameStartReq()
        {
            type = (int)Global.GameType,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex
        };

        QuestGameData.SetUserData();
        Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
    }

    void onFailStart(GameStartReq req)
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        popupSystem.FunctionSetting(1, "RetryGameStart", gameObject);
        popupSystem.FunctionSetting(3, "RetryCancel", gameObject);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_17"), false, null);
        popupSystem.SetResourceImage("Message/error");

        popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
        retryReq = req;
    }

    void RetryCancel()
    {
        Global.ReBoot();
    }

    GameStartReq retryReq = null;

    void RetryGameStart()
    {
        if (retryReq == null)
        {
            RetryCancel();
            return;
        }

        Global.GameInstance.GameStart(retryReq, recvGameStart, onFailStart);
    }

    private void OpenSystemPopup()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_cs_sr_5"), false, () => { bCanTouch = true; });
        popupSystem.SetResourceImage("Message/ok");
    }
}
