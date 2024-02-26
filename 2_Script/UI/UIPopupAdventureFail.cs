using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class UIPopupAdventureFail : UIPopupBase, ManagerAdventure.IUserAnimalListener
{
    public static Protocol.AdventureGameFailResp _failResp = null;

    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo[] animals;
    [SerializeField] private Animation[] animalsAni;
    [SerializeField] private UIButtonAdventureLevelUp[] levelUpButtons;
    [SerializeField] private GenericReward[] rewards;
    [SerializeField] private GameObject[] skillTitles;
    [SerializeField] private Transform[] clouds;
    [SerializeField] private UILabel tip;
    [SerializeField] private GameObject stopButton;
    [SerializeField] private GameObject retryButton;
    
    // 법률 대응 관련 위치/활성화 변경 필요한 오브젝트
    [SerializeField] private Transform BottomRoot;
    [SerializeField] private GameObject labelBuyInfo;

    private void Awake()
    {
        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Add(this);

        bShowTopUI = true;

        foreach (var button in levelUpButtons)
            button.bCanTouch = false;

        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].changeStartEvent += () =>
            {
                bCanTouch = false;
                foreach (var button in levelUpButtons)
                {
                    ManagerUI._instance.bTouchTopUI = false;
                    button.bCanTouch = false;
                }
            };

            animals[i].changeEndEvent += () =>
            {
                bCanTouch = true;
                foreach (var button in levelUpButtons)
                {
                    ManagerUI._instance.bTouchTopUI = true;
                    button.ResetButton();
                    button.bCanTouch = true;
                }
            };
            animals[i].levelUpStartEvent += levelUpEffect;

            int c = i;
            animals[c].fullLoadedEvent += (Texture texture) => 
            {
                clouds[c].localPosition = Vector3.up * Mathf.Clamp(texture.GetHeight() + 50, 0, 250);
            };
        }
    }

    void OnDestroy()
    {
        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Remove(this);

        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        List<Reward> rData = new List<Reward>();
        for (int i = 0; i < _failResp.rewards.Count; i++)
        {
            if (_failResp.rewards[i].value == 0)
                continue;

            Reward r = new Reward();
            r.type = _failResp.rewards[i].type;
            r.value = _failResp.rewards[i].value;

            rData.Add(r);

            switch (r.type)
            {
                case (int)RewardType.coin:
                    Global.coin = Global.coin + r.value;
                    break;
                case (int)RewardType.wing:
                    Global.wing = Global.wing + r.value;
                    break;
                case (int)RewardType.jewel:
                    Global.jewel = Global.jewel + r.value;
                    break;
                case (int)RewardType.expBall:
                    Global.exp = Global.exp + r.value;
                    break;

                default:
                    break;
            }
        }

        InitPopup(rData.ToArray());
        ButtonSetting();
        base.OpenPopUp(depth);

        ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);
        ManagerUI._instance.UpdateUI();
    }

    private void ButtonSetting()
    {
        if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            Vector3 pos = stopButton.transform.localPosition;
            pos.x = 0.0f;
            stopButton.transform.localPosition = pos;
            retryButton.SetActive(false);
        }

        if (LanguageUtility.IsShowBuyInfo)
        {
            BottomRoot.localPosition = new Vector2(0, 30f);
            labelBuyInfo.SetActive(true);
        }
    }

    private void InitPopup(Reward[] rData)
    {
        SetReward(rData);

        for (int i = 0; i < 3; i++)
        {
            ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            animals[i].SetAnimalSelect(aData);
            levelUpButtons[i].ResetButton();
        }

        ManagerAdventure.User.SyncFromServer_Animal();

        int tipIdx = Random.Range(1, 10 + 1);
        tip.text = Global._instance.GetString("adv_tip_f_" + tipIdx.ToString());

        //ManagerUI._instance.OpenPopupStageAdventureReady(OnTouch);

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL2);
        ManagerSound._instance.PauseBGM();

        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);
        ManagerUI._instance.bTouchTopUI = true;

        Invoke("SetExp", 0.5f);

        SendGrowthyPlayEnd(rData);
    }

    private void SendGrowthyPlayEnd(Reward[] rData)
    {
        // Growthy 그로씨
        var coinCount = 0;
        foreach (var tempReward in rData)
        {
            if (tempReward.type == (int)RewardType.coin)
            {
                coinCount = tempReward.value;
                break;
            }
        }
        // 사용한 인게임 아이템
        var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 4; i < 7; i++)
        {
            if (GameItemManager.useCount[i] > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = ((GameItemType)i + 1).ToString(),
                    L_CNT = GameItemManager.useCount[i]
                };
                itemList.Add(readyItem);
            }
        }
        // 컨티뉴 횟수
        if (GameManager.instance.useContinueCount > 0)
        {
            var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = "Continue",
                L_CNT = GameManager.instance.useContinueCount
            };
            itemList.Add(readyItem);
        }
        var docItem = JsonConvert.SerializeObject(itemList);

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
        (
            myProfile.userID,
            (myProfile.stage - 1).ToString(),
            GameManager.instance.GrowthyAfterStage.ToString(),
            Global.GameInstance.GetGrowthyStageIndex(),

            (Global.GameType == GameType.ADVENTURE_EVENT) ?
            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE_EVENT : ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE,

            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.LOSE,
            0,
            0,
            coinCount,
            (long)(Time.time - GameManager.instance.playTime),
            GameManager.instance.firstPlay,
            GameManager.instance.useContinueCount > 0,
            0,   //남은턴 다시계산
            docItem,
            0,
            false,
            false,
            null,                
            Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
            null,
            "N",
            boostLevel: "0",
            firstFlowerLevel: "N",
            usedTurn: GameManager.instance.useMoveCount,
            continueReconfirm: GameManager.instance.continueReconfirmCount,
            detailInfo: null
        );

        //사용동물
        var animalList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL>();
        for (var i = 0; i < 3; i++)
        {
            var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);

            var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL
            {
                L_CID = animalData.idx,
                L_LEV = animalData.level,
                L_CNT = animalData.overlap
            };
            animalList.Add(readyItem);
        }
        var docAnimal = JsonConvert.SerializeObject(animalList);
        playEnd.L_CHAR = docAnimal;

        var doc = JsonConvert.SerializeObject(playEnd);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);
    }

    public void RePaintButtons()
    {
        for (int i = 0; i < levelUpButtons.Length; i++)
            levelUpButtons[i].ResetButton();
    }

    [SerializeField] private GameObject levelUpEffectObj;
    [SerializeField] private AnimationCurve sizeAniController;
    private IEnumerator levelUpEffect(UIItemAdventureAnimalInfo target)
    {
        while (!target.isFullShotLoaded)
            yield return new WaitForSeconds(0.1f);

        GameObject effect = NGUITools.AddChild(target.gameObject, levelUpEffectObj);
        effect.transform.localPosition = new Vector3(0.0f, 220.0f, 0.0f);


        float totalTime = 0.0f;
        float endTime = sizeAniController.keys[sizeAniController.length - 1].time;

        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            target.FullshotObject.transform.localScale = Vector3.one * sizeAniController.Evaluate(totalTime);

            if (totalTime >= endTime)
                break;

            yield return null;
        }
    }

    private void SetReward(Reward[] rData)
    {
        if (rData.Length == 0)
        {
            rewards[0].transform.parent.parent.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < rewards.Length; i++)
        {
            if (rData != null && i < rData.Length)
            {
                rewards[i].SetReward(rData[i]);
                rewards[i].gameObject.SetActive(true);

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)rData[i].type,
                    rData[i].value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PLAY,
                    null
                    );

            }
            else
            {
                rewards[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickBtnRetry()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        StartCoroutine(CoOpenReady());
    }

    IEnumerator CoOpenReady()
    {
        string stageKey = Global.GameInstance.GetStageKey();
        string stageName = Global.GameInstance.GetStageFilename();

        using( var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName) )
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

                ManagerUI._instance.OpenPopupStageAdventureReady(tempData, OnTouch);

                ManagerAdventure.User.SyncFromServer_Stage();
                
                bCanTouch = true;
                yield return null;
            }
        }
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("Lobby");

        UIPopupStageAdventure.startChapter = Global.chapterIndex;
    }

    void OnTouch()
    {
        bCanTouch = true;
    }

    public void OnAnimalChanged(int animalIdx)
    {
        for (int i = 0; i < 3; ++i)
        {
            if (animals[i].AnimalIdx == animalIdx)
            {
                ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
                animals[i].SetAnimalSelect(aData);
                levelUpButtons[i].ResetButton();
            }
        }
    }

    protected override void OnFocus(bool focus)
    {
        if (focus)
        {
            RePaintButtons();
        }
    }

    private void SetExp()
    {
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].ChangeAnimal_Ani(ManagerAdventure.User.GetAnimalFromDeck(1, i));
        }
    }
}


