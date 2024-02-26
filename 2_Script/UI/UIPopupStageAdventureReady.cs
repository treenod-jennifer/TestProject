using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIPopupStageAdventureReady : UIPopupBase, ManagerAdventure.IUserAnimalListener
{
    public static UIPopupStageAdventureReady _instance;
    public int selectSlot { get; protected set; }

    protected StageMapData stageData;

    [Header("Linked Object")]
    [SerializeField] protected UIItemAdventureTopPanel topPanel;
    [SerializeField] protected UIItemAdventureAnimalInfo[] animals;
    [SerializeField] protected UIButtonAdventureLevelUp[] levelUpButtons;
    [SerializeField] private GameObject[] skillTitles;
    [SerializeField] private GameObject effectPanel;
    [SerializeField] protected GameObject objDescription;
    
    protected virtual void Awake()
    {
        _instance = this;
        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Add(this);

        for(int i=0; i<animals.Length; i++)
        {
            animals[i].changeStartEvent += () => 
            {
                bCanTouch = false;
                ManagerUI._instance.bTouchTopUI = false;
                foreach (var button in levelUpButtons)
                    button.bCanTouch = false;
            };
            animals[i].changeEndEvent += () =>
            {
                bCanTouch = true;
                ManagerUI._instance.bTouchTopUI = true;
                foreach (var button in levelUpButtons)
                    button.bCanTouch = true;
            };
            animals[i].levelUpStartEvent += levelUpEffect;
        }

        bShowTopUI = true;

        ChangeButtonActiveCheck();
    }

    [SerializeField] private GameObject levelUpEffectObj;
    private IEnumerator levelUpEffect(UIItemAdventureAnimalInfo target)
    {
        NGUITools.AddChild(target.gameObject, levelUpEffectObj);
        yield return new WaitForSeconds(1.0f);
    }

    protected override void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Remove(this);

        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        panelCount = panels.Length - 1;
    }

    public event System.Action closeEvent;
    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        base.ClosePopUp(_mainTime, callback);

        if (closeEvent != null)
            closeEvent();
    }

    public void InitData(StageMapData sData)
    {
        //상품판매 법률 개정
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
        
        SettingFreeWingButton();

        if (Global.chapterIndex == 2 && Global.stageIndex == 1)
        {
            if(ManagerAdventure.User.GetChapterProgress(2) != null)
            {
                PlayerPrefs.SetInt("TutorialAttribute_Adventure", 0);
            }

            if (!PlayerPrefs.HasKey("TutorialAttribute_Adventure"))
            {
                PlayerPrefs.SetInt("TutorialAttribute_Adventure", 0);
                ManagerTutorial.PlayTutorial(TutorialType.TutorialAttribute_Adventure);
            }
        }

        this.stageData = sData;

        SetStageText();

        for (int i = 0; i < 3; i++)
        {
            var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            animals[i].SetAnimalSelect(animalData);
            levelUpButtons[i].ResetButton();
            topPanel.SetAnimal(i, animalData.idx);
        }
        
        var chapterData = ManagerAdventure.Stage.GetChapter(Global.chapterIndex);
        var stageData = ManagerAdventure.Stage.GetStage(Global.chapterIndex, Global.stageIndex);
        topPanel.SetBoss(sData.bossIdx);
        topPanel.SetWeight();
    }

    private void SetStageText()
    {
        var vsPanel = topPanel as UIItemAdventureVSPanel;

        if(vsPanel == null)
        {
            return;
        }

        if (Global.stageIndex > 100)
        {
            vsPanel.StageText = "C" + Global.chapterIndex.ToString() + "-" + (Global.stageIndex % 100).ToString();
        }
        else
        {
            vsPanel.StageText = Global.chapterIndex.ToString() + "-" + Global.stageIndex.ToString();
        }
    }

    public void OnAnimalChanged(int animalIdx)
    {
        for (int i = 0; i < 3; ++i)
        {
            if (animals[i].AnimalIdx == animalIdx)
            {
                ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
                animals[i].SetAnimalSelect(aData);
            }
        }

        topPanel.RepaintAnimal(animalIdx);
        RePaintButtons();
    }

    public void RePaintAll()
    {
        InitData(this.stageData);
    }

    public void RePaintButtons()
    {
        for (int i = 0; i < levelUpButtons.Length; i++)
            levelUpButtons[i].ResetButton();
    }

    protected override void OnFocus(bool focus)
    {
        if(focus)
        {
            bCanTouch = true;
            RePaintButtons();
        }
    }

    public void ChangeAnimal_Slot1()
    {
        ChangeAnimal(0);
    }
    public void ChangeAnimal_Slot2()
    {
        ChangeAnimal(1);
    }
    public void ChangeAnimal_Slot3()
    {
        ChangeAnimal(2);
    }

    protected virtual void ChangeAnimal(int slot)
    {
        if (!bCanTouch)
            return;

        bCanTouch = false;

        selectSlot = slot;

        ManagerUI._instance.OpenPopup<UIPopupStageAdventureAnimal>((popup) => {popup.InitTarget(UIPopupStageAdventureAnimal.PopupMode.ChangeMode);});
    }

    public virtual void OnClickRecommendDeck()
    {
        if (!bCanTouch)
            return;

        BossInfo bossInfo = StageUtility.StageInfoDecryption(stageData).bossInfo;
        StartCoroutine(ManagerAdventure.User.SetRecommendDeck(bossInfo.attribute, bossInfo.attrSize, false, RePaintAll));
    }

    bool touchButton = false;
    protected bool _recvGameStart_end = false;
    public GameObject _objEFfectUseWIng;
    public Transform startBtnTr;

    protected bool GoIngameProduction()
    {
        if ((Global.wing > 0 || GameData.RemainFreeWingPlayTime() > 0)&& touchButton == false)
        {
            touchButton = true;
            bCanTouch = false;
            ManagerUI._instance.bTouchTopUI = false;
            foreach (var button in levelUpButtons)
                button.bCanTouch = false;

            ManagerSound.AudioPlay(AudioLobby.UseClover);

            UIUseCloverEffect cloverEffect = NGUITools.AddChild(effectPanel, _objEFfectUseWIng).GetComponent<UIUseCloverEffect>();
            cloverEffect.targetObj = this.gameObject;

            if (GameData.RemainFreeWingPlayTime() > 0)
            {
                cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position, true, true);
            }
            else
            {
                cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, startBtnTr.transform.position);
                Global.wing--;
            }

            return true;
        }
        else if (Global.wing <= 0 && touchButton == false)
        {
            ManagerUI._instance.OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Wing));
            bCanTouch = true;
        }

        return false;
    }

    protected virtual void OnClickGoInGame()
    {
        if (bCanTouch == false)
            return;

        if(GoIngameProduction())
        {
            var req = new GameStartReq()
            {
                type = (int)Global.GameType,
                eventIdx = Global.eventIndex,
                stage = Global.stageIndex,
                chapter = Global.chapterIndex,
            };
            ServerRepos.GameStartTs = Global.GetTime();
            QuestGameData.SetUserData();
            Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
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

            _recvGameStart_end = true;
            bool nowWingFreeTime = GameData.RemainFreeWingPlayTime() > 0;

            //그로씨
            var useClover = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
                nowWingFreeTime ? ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_ADVENTURE_PLAY_FREE : ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_ADVENTURE_PLAY,
                0,
                nowWingFreeTime ? 0 : -1,
                0,
                (int)(ServerRepos.User.AllWing),
                mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
            );
            var cloverDoc = JsonConvert.SerializeObject(useClover);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
        }
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

    [SerializeField] private UISprite startWing;
    [SerializeField] private GameObject startWingShadow;
    [SerializeField] private GameObject _objRingGlow;
    [SerializeField] private GameObject _objEffectStartButton;
    public void ShowUseClover()
    {
        StartCoroutine(ShowWingEffect());
    }

    IEnumerator ShowWingEffect()
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;
        float defaultScaleValue = startWing.cachedTransform.localScale.x;
        Vector3 startButtonPos = startBtnTr.localPosition;

        RingGlowEffect ringGlow = NGUITools.AddChild(startBtnTr.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.9f;

        NGUITools.AddChild(startBtnTr.gameObject, _objEffectStartButton);

        //이펙트 터질 때 버튼 움직임.
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        while (showTimer < 0.8f)
        {
            showTimer += Global.deltaTimeLobby * 4f;

            if (showTimer < 0.5f)
            {
                scaleRatio = 0.7f + showTimer;
            }
            else
            {
                scaleRatio = 1.8f - showTimer;
            }

            startBtnTr.localPosition = startButtonPos * (1 + (1 - showTimer) * 0.04f);
            startWing.cachedTransform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            yield return null;
        }

        showTimer = 0;

        while (showTimer < 0.5f)
        {
            showTimer += Global.deltaTimeLobby;
            yield return null;
        }

        while (true)
        {
            if (_recvGameStart_end)
                break;
            yield return null;
        }

        touchButton = false;
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("InGame");
        ManagerUI._instance.bTouchTopUI = true;

        yield return null;
    }

    [SerializeField] private UIPokoButton[] changeButtons;
    private void ChangeButtonActiveCheck()
    {
        var chapter = ManagerAdventure.User.GetChapterProgress(1);
        if (chapter != null && chapter.stageProgress[1] != null && chapter.stageProgress[1].clearLevel != 0)
            return;

        for (int i=0; i<changeButtons.Length; i++)
        {
            var sprite = changeButtons[i].GetComponentInChildren<UISprite>();
            var label = changeButtons[i].GetComponentInChildren<UILabel>();

            sprite.spriteName = "diary_button_gray";
            label.effectColor = new Color(0.4f, 0.4f, 0.4f);

            changeButtons[i].functionName = "";
        }
    }

    protected void SettingFreeWingButton()
    {
        bool bFree = false;

        if (GameData.RemainFreeWingPlayTime() > 0)
            bFree = true;

        if (bFree == true)
        {
            startWing.spriteName = "adven_wing_icon_infinity";
            startWing.width = 86;
            startWing.height = 80;
            startWingShadow.SetActive(false);
        }
        else
        {
            startWing.spriteName = "adven_wing_icon";
            startWing.width = 62;
            startWing.height = 56;
            startWingShadow.SetActive(true);
        }
    }

}
