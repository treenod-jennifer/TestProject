using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureAnimalInfo : UIPopupBase, ManagerAdventure.IUserAnimalListener
{
    public static UIPopupStageAdventureAnimalInfo _instance = null;
    private ManagerAdventure.AnimalInstance aData;
    
    [SerializeField] private GenericReward reward;
    [SerializeField] private UILabel giftTimeText;
    [SerializeField] private GameObject rewardIcon;
    [SerializeField] private GameObject buyInfoText;

    private float alphaColor = 125f / 255f;

    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo animalInfo;
    [SerializeField] private UIButtonAdventureLevelUp levelUpButton;
    [SerializeField] private GameObject skillTitle;

    private bool DummyAnumalInfo = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        bShowTopUI = true;

        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Add(this);

        animalInfo.changeStartEvent += () =>
        {
            bCanTouch = false;
            ManagerUI._instance.bTouchTopUI = false;
            levelUpButton.bCanTouch = false;
        };
        animalInfo.changeEndEvent += () =>
        {
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
            levelUpButton.bCanTouch = true;
        };
        animalInfo.levelUpStartEvent += levelUpEffect;
        animalInfo.setAnimalCallBack += (ManagerAdventure.AnimalInstance aData) => skillTitle.SetActive(aData.skill != 0);
        animalInfo.setAnimalCallBack += (ManagerAdventure.AnimalInstance aData) => InitReward();
    }

    [SerializeField] private GameObject levelUpEffectObj;
    [SerializeField] private AnimationCurve sizeAniController;
    private IEnumerator levelUpEffect(UIItemAdventureAnimalInfo target)
    {
        while (!target.isFullShotLoaded)
            yield return new WaitForSeconds(0.1f);

        GameObject effect = NGUITools.AddChild(gameObject, levelUpEffectObj);

        effect.transform.localPosition = new Vector3(-140.0f, 60.0f, 0.0f);

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

    #region 보상관련

    private void InitReward()
    {
        CdnAdventureAnimal cdnData = ServerContents.AdvAnimals[aData.idx];
        bool isExistReward = cdnData.lobby_rewards.Count > 0 ? true : false;

        if (isExistReward == true)
        {
            //보상 설정
            reward.SetReward(cdnData.lobby_rewards[0]);
            //보상 시간
            RewardTime();
        }
    }
    private void RewardTime()
    {
        long getRewardTs = 0;
        var animalData = ServerContents.AdvAnimals[aData.idx];

        if (animalData != null)
        {
            getRewardTs = animalData.lobby_reward_cool_time;
        }

        giftTimeText.text = Global.GetTimeText_HHMM(getRewardTs, false);
    }
    private void RewardSetActive()
    {
        GameObject rewardTrue = rewardIcon.transform.GetChild(0).gameObject;
        GameObject rewardFalse = rewardIcon.transform.GetChild(1).gameObject;
        GameObject rewardHomeButton = rewardIcon.transform.GetChild(2).gameObject;

        if (aData.lobbyCharIdx != -1)
        {
            //로비 캐릭터 o
            rewardTrue.SetActive(true);
            rewardFalse.SetActive(false);
            homeIconActive(rewardHomeButton, true);
        }
        else
        {
            //로비 캐릭터 x
            rewardTrue.SetActive(false);
            rewardFalse.SetActive(true);
            homeIconActive(rewardHomeButton, false);
        }
    }

    private void homeIconActive(GameObject rewardHomeButton, bool setActive)
    {
        GameObject homeIcon = rewardHomeButton.transform.GetChild(0).gameObject;

        if(setActive)
        {
            rewardHomeButton.GetComponent<BoxCollider>().enabled = true;
            homeIcon.GetComponent<TweenScale>().enabled = true;
            homeIcon.GetComponent<UISprite>().color = new Color(1f, 1f, 1f, 1f);
            if (ManagerCharacter._instance.IsSpecialLobbyChar(aData.idx)) // 스페셜 로비 배치 캐릭터 아이콘 예외 처리
            {
                homeIcon.GetComponent<UISprite>().spriteName = "icon_special_lobby";
                //기존의 아이콘은 크기를 조정하여 사용하기에 스페셜 로비 아이콘만 이미지 변경
                homeIcon.GetComponent<UISprite>().MakePixelPerfect();
            }
            else
                homeIcon.GetComponent<UISprite>().spriteName = "icon_home";
        }
        else
        {
            rewardHomeButton.GetComponent<BoxCollider>().enabled = false;
            homeIcon.GetComponent<TweenScale>().enabled = false;
            homeIcon.transform.localScale = Vector3.one;
            homeIcon.transform.GetComponent<UISprite>().color = new Color(1f, 1f, 1f, alphaColor);
        }
    }

    #endregion

    public void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        if (ManagerAdventure.User != null)
            ManagerAdventure.User.animalListeners.Remove(this);

        base.OnDestroy();

    }
    public void InitAnimalInfo(ManagerAdventure.AnimalInstance aData)
    {
        this.aData = aData;

        if (this.aData.gettime == 0)
        {
            DummyAnumalInfo = true;

            this.aData.overlap = 1;
            this.aData.level = 1;
            this.aData.atk = ManagerAdventure.UserData.CalcAtk(aData.idx, aData.grade, 1, 1);
            this.aData.hp = ManagerAdventure.UserData.CalcHp(aData.idx, aData.grade, 1, 1);
        }
        animalInfo.SetAnimalSelect(this.aData);
        if (this.aData.gettime == 0) animalInfo.SetOverlapZero();
        levelUpButton.ResetButton();

        RewardSetActive();
        AppearanceChangeBtnActive(this.aData.lookId);
        currentLookId = this.aData.lookId;

        buyInfoText.SetActive(LanguageUtility.IsShowBuyInfo);
    }

    public void OnAnimalChanged(int animalIdx)
    {
        ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalInstance(animalIdx);
        animalInfo.SetAnimalSelect(aData);
        levelUpButton.ResetButton();
    }


    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        StartCoroutine(OpenAni());
    }

    [SerializeField] private Transform animalFullShot;
    [SerializeField] private Transform animalBubble;
    [SerializeField] private AnimationCurve bubbleAniController;
    private IEnumerator OpenAni()
    {
        animalBubble.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.2f);

        float totalTime = 0.0f;
        float endTime = sizeAniController.keys[sizeAniController.length - 1].time;

        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            animalFullShot.localScale = Vector3.one * sizeAniController.Evaluate(totalTime);

            if (totalTime >= endTime)
                break;

            yield return null;
        }

        totalTime = 0.0f;
        endTime = bubbleAniController.keys[bubbleAniController.length - 1].time;
        while (true)
        {
            totalTime += Global.deltaTimeNoScale;

            animalBubble.localScale = Vector3.one * bubbleAniController.Evaluate(totalTime);

            if (totalTime >= endTime)
                break;

            yield return null;
        }
    }

    public void OnClickMaxLevelInfo()
    {
        if (bCanTouch == false)
            return;
        
        ManagerUI._instance.OpenPopupStageAdventureAnimalMaxInfo(aData);
    }
    protected override void OnFocus(bool focus)
    {
        if(focus)
        {
            levelUpButton.ResetButton();
        }
    }

    public void OnClickMax()
    {
        if (bCanTouch == false)
            return;
        
        ManagerUI._instance.OpenPopup<UIPopupStageAdventureOverlabMax>();
    }

    public void OnClickLobbyInfo()
    {
        if (bCanTouch == false)
            return;
        
        var popup = ManagerUI._instance.OpenPopup<UIPopupLobbyGiftInfo>();
        popup.Init(aData.idx);
    }

    [SerializeField] private GameObject[] LeftBtn;
    [SerializeField] private GameObject[] RightBtn;

    private int currentLookId = 0;
    private List<int> lookIdList = new List<int> { 0, 1 };

    public void OnClickAppearanceChangeLeftBtn()
    {
        if (bCanTouch == false)
            return;
        
        currentLookId--;

        AppearanceChangeBtnActive(currentLookId);

        animalInfo.LookID = currentLookId;

    }

    public void OnClickAppearanceChangeRightBtn()
    {
        if (bCanTouch == false)
            return;
        
        currentLookId++;

        AppearanceChangeBtnActive(currentLookId);

        animalInfo.LookID = currentLookId;
    }

    private void OnAnimalAppearanceChange(int animalIdx, int lookId = 0)
    {
        ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalInstance(animalIdx);
        animalInfo.SetAnimalSelect(aData);
    }

    private void AppearanceChangeBtnActive(int lookValue)
    {
        for (int i = 0; i < lookIdList.Count; i++)
        {
            if (lookValue == 0 && i == 0)
            {
                LeftBtn[0].SetActive(false);
                LeftBtn[1].SetActive(true);
                RightBtn[0].SetActive(true);
                RightBtn[1].SetActive(false);
                break;
            }
            else if (lookValue == lookIdList[lookIdList.Count - 1] && i == lookIdList.Count - 1)
            {
                LeftBtn[0].SetActive(true);
                LeftBtn[1].SetActive(false);
                RightBtn[0].SetActive(false);
                RightBtn[1].SetActive(true);
                break;
            }
            else
            {
                LeftBtn[0].SetActive(true);
                LeftBtn[1].SetActive(false);
                RightBtn[0].SetActive(true);
                RightBtn[1].SetActive(false);
            }
        }
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        
        if (DummyAnumalInfo)
        {
            this.aData.overlap = 0;
            this.aData.level = 0;
            this.aData.atk = 1;
            this.aData.hp = 1;
        }
        
        StartCoroutine(CoChangeSpecialLobbyChar());
    }
    
    private bool isChangeChar = false;
    IEnumerator CoChangeSpecialLobbyChar()
    {
        bCanTouch = false;
        
        animalInfo.CheckLookID((complete) =>
        {
            if (complete)
            {
                if (ManagerCharacter._instance.IsSpecialLobbyChar(aData.idx))
                    StartCoroutine(CoLobbyAnimalCostumChange());
                else
                    isChangeChar = true;
            }
            else
                isChangeChar = true;
        });
        
        yield return new WaitUntil(() => isChangeChar);

        bCanTouch = true;
        
        base.OnClickBtnClose();
    }

    IEnumerator CoLobbyAnimalCostumChange()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);

        yield return ManagerAIAnimal.instance.CoSelectLobbyAnimalCostumChange(animalInfo.AnimalIdx);

        isChangeChar = true;
        
        NetworkLoading.EndNetworkLoading();
    }
}