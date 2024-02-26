using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupADView : UIPopupBase
{
    [Header("Singular Reward")]
    [SerializeField] private GameObject singularRoot;
    [SerializeField] private GenericReward reward;
    [SerializeField] private GameObject timeIcon;
    [SerializeField] private UILabel timeLabel;

    [Header("Plural Reward")]
    [SerializeField] private GameObject pluralRoot;
    [SerializeField] private GenericReward[] rewards;
    [SerializeField] private UISprite speechBubble;
    [SerializeField] private UILabel mainLabel;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private UIReuseGrid_Move reuseGridMove;
    [SerializeField] private float scrollSpeed = 10;

    [Header("None Reward")]
    [SerializeField] private GameObject noneRoot;
    [SerializeField] private UILabel noneRewardLabel;

    [Header("Material Reward")]
    [SerializeField] private GameObject materialRoot;

    [Header("Box Sprite")]
    [SerializeField] private UISprite boxSprite;

    private const int ITEM_WIDTH = 80;
    private const int ITEM_WHITE_SPACE = 30;
    //한번에 보여지는 아이템 갯수
    private const int MAX_SHOW_ITEM_COUNT = 8;   

    private System.Action successAction = null;
    private System.Action failAction = null;

    private AdManager.AdType adType;
    
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollView.depth = depth + 1;
    }
    
    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }
    
    public void SetRequestAdReward(AdManager.AdType adType, params Reward[] rewards)
    {
        SetBaseType(adType);
        if (rewards.Length == 1)
        {
            SetSingularReward(rewards[0]);
        }
        else if (rewards.Length > 1)
        {
            SetPluralReward("p_ads_2", rewards);
        }
    }

    private int mission;
    public void SetMissionTime(int mission, long time, long maxTime)
    {
        SetBaseType(AdManager.AdType.AD_4);
        this.mission = mission;

        SetSingularReward(time, maxTime);
    }

    private int boxID;
    public void SetReduceGiftboxTime(int boxID, long time, long maxTime)
    {
        SetBaseType(AdManager.AdType.AD_3);
        this.boxID = boxID;

        SetSingularReward(time, maxTime);
    }
    
    public void SetWorldRankExchangeStation(System.Action action, AdManager.AdType adType, params Reward[] rewards)
    {
        SetBaseType(adType);
        if (rewards.Length == 1)
        {
            SetSingularReward(rewards[0]);
        }
        else if (rewards.Length > 1)
        {
            SetPluralReward("p_ads_10", rewards);
        }
        successAction = action;
    }
    
    public void SetEventChapterAd(AdManager.AdType type, System.Action successAction, System.Action failAction)
    {
        this.adType = type;
        SetNoneReward(Global._instance.GetString("p_ads_9"));

        this.successAction = successAction;
        this.failAction = failAction;
    }
    
    public void SetMaterialAdReward(System.Action action)
    {
        SetBaseType(AdManager.AdType.AD_8);

        singularRoot.SetActive(false);
        pluralRoot.SetActive(false);
        noneRoot.SetActive(false);
        materialRoot.SetActive(true);

        timeIcon.SetActive(false);

        successAction = action;
    }

    public void SetPackageRandomBox(Reward[] rewards, System.Action action)
    {
        SetBaseType(AdManager.AdType.AD_13);
        if (rewards.Length == 1)
        {
            SetSingularReward(rewards[0]);
        }
        else if (rewards.Length > 1)
        {
            SetPluralReward("p_ads_2", rewards);
        }

        successAction = action;
    }

    // 공통 세팅
    private void SetBaseType(AdManager.AdType adType)
    {
        this.adType = adType;
        if (adType == AdManager.AdType.AD_13)
        {
            boxSprite.spriteName = "icon_randombox_advertising";
            boxSprite.transform.localPosition = new Vector3(0, -105f, 0);
        }
        else
        {
            boxSprite.spriteName = "icon_randombox_open";
            boxSprite.transform.localPosition = new Vector3(0, -100f, 0);
        }
        boxSprite.MakePixelPerfect();
    }

    #region 베이스 세팅
    private void SetPluralReward(string stringKey, params Reward[] rewards)
    {
        singularRoot.SetActive(false);
        pluralRoot.SetActive(true);
        noneRoot.SetActive(false);
        materialRoot.SetActive(false);

        //아이템 데이터 설정
        bool isUseScroll = rewards.Length > MAX_SHOW_ITEM_COUNT; 
        if (isUseScroll == true)
        {   //획득할 수 있는 보상의 수가 보여지는 ui 보다 많을 때, 스크롤 표시
            reuseGridMove.InItGrid(rewards.Length, (go, index) =>
            {
                if (rewards.Length > index)
                    go.GetComponent<GenericReward>().SetReward(rewards[index]);
            });
            scrollView.clipping = UIDrawCall.Clipping.SoftClip;
        }
        else
        {   //스크롤이 불필요하다면 보여지는 보상만 설정
            for (int i = 0; i < this.rewards.Length; i++)
            {
                this.rewards[i].gameObject.SetActive(i < rewards.Length);
                if (rewards.Length > i)
                    this.rewards[i].SetReward(rewards[i]);
            }
            scrollView.clipping = UIDrawCall.Clipping.None;
        }

        //그리드 위치 설정
        float itemSpace = (ITEM_WIDTH * 0.5f);
        Vector3 reuseGridPos = reuseGridMove.transform.localPosition;
        reuseGridPos.x = (isUseScroll == true)
            ? (itemSpace * -(MAX_SHOW_ITEM_COUNT - 1))
            : (itemSpace * -(rewards.Length - 1));
        reuseGridMove.transform.localPosition = reuseGridPos;
        
        //아이템 간격 설정
        reuseGridMove.itemSize = ITEM_WIDTH;
        
        //말풍선 크기 설정
        int activeUICount = (isUseScroll) ? MAX_SHOW_ITEM_COUNT : rewards.Length;
        speechBubble.width = activeUICount * ITEM_WIDTH + ITEM_WHITE_SPACE;
        
        //메인 텍스트 설정
        mainLabel.text = Global._instance.GetString(stringKey);

        //스크롤 사용될 때, 스크롤 연출 출력
        if (isUseScroll == true)
            StartCoroutine(CoPluralRewardMove());
    }

    private IEnumerator CoPluralRewardMove()
    {
        yield return new WaitForSeconds(0.3f);
        
        while (this.gameObject.activeInHierarchy == true)
        {
            scrollView.transform.localPosition += -Vector3.right * Time.deltaTime * scrollSpeed;
            scrollView.clipOffset += Vector2.right * Time.deltaTime * scrollSpeed;
            yield return null;
        }
    }

    private void SetSingularReward(Reward reward)
    {
        singularRoot.SetActive(true);
        pluralRoot.SetActive(false);
        noneRoot.SetActive(false);
        materialRoot.SetActive(false);

        this.reward.SetReward(reward);
        this.reward.gameObject.SetActive(true);
        timeIcon.SetActive(false);
    }

    private void SetSingularReward(long time, long maxTime, bool isTimer = true)
    {
        singularRoot.SetActive(true);
        pluralRoot.SetActive(false);
        noneRoot.SetActive(false);
        materialRoot.SetActive(false);

        reward.gameObject.SetActive(false);
        timeIcon.SetActive(true);

        if (isTimer)
        {
            StartCoroutine(SetTime(time, maxTime));
        }
        else
        {
            timeLabel.text = $"-{Global.GetTimeText_HHMMSS(time, false)}";
        }
    }

    private void SetNoneReward(string popupText)
    {
        singularRoot.SetActive(false);
        pluralRoot.SetActive(false);
        noneRoot.SetActive(true);
        materialRoot.SetActive(false);

        this.noneRewardLabel.text = popupText;
        timeIcon.SetActive(false);
    }

    #endregion

    private IEnumerator SetTime(long time, long maxTime)
    {
        while (true)
        {
            long leftTime = Global.LeftTime(time);

            leftTime = leftTime < maxTime ? leftTime : maxTime;

            timeLabel.text = $"-{Global.GetTimeText_HHMMSS(leftTime, false)}";

            if (leftTime <= 0) break;

            yield return new WaitForSeconds(1.0f);
        }

        // 광고보고있는 중일 때 타이머가 끝난 경우 창을 닫아서는 안된다
        if (!bCanTouch)
            yield break;

        ClosePopUp();
    }

    public void ShowAD()
    {
        if (!this.bCanTouch)
            return;
        bCanTouch = false;


        switch (adType)
        {
            case AdManager.AdType.AD_1:
            case AdManager.AdType.AD_2:
            case AdManager.AdType.AD_6:
            case AdManager.AdType.AD_7:
            case AdManager.AdType.AD_8:
            case AdManager.AdType.AD_13:
            case AdManager.AdType.AD_15:
                AdManager.ShowAD_RequestAdReward(adType, (isSuccess, reward) =>
                {
                    if (!isSuccess)
                    {
                        bCanTouch = true;
                        return;
                    }
                    else
                    {
                        ClosePopUp(successAction);
                    }
                });
                break;
            case AdManager.AdType.AD_19:
                if (ManagerWorldRanking.userData != null &&
                    ManagerWorldRanking.userData.GetRankToken() >= UIPopUpWorldRankExchangeStation.maxRankToken)
                {
                    ClosePopUp();
                    
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_72"), false);
                    popup.SortOrderSetting();
                    popup.HideCloseButton();
                }
                else
                {
                    AdManager.ShowAD_ReqAdWorldRankExchangeStation((isSuccess, reward) =>
                    {
                        if (!isSuccess)
                        {
                            bCanTouch = true;
                            return;
                        }
                        else
                        {
                            ClosePopUp(successAction);
                        }
                    });
                }
                break;
            case AdManager.AdType.AD_3:
                AdManager.ShowAD_ReduceGiftboxTime(boxID, (isSuccess) =>
                {
                    if (!isSuccess)
                    {
                        bCanTouch = true;
                        return;
                    }
                    else
                    {
                        ClosePopUp();
                    }
                });
                break;

            case AdManager.AdType.AD_4:
                AdManager.ShowAD_ReduceMissionTime(mission, (isSuccess) =>
                {
                    if (!isSuccess)
                    {
                        bCanTouch = true;
                        return;
                    }
                    else
                    {
                        ClosePopUp();
                    }
                });
                break;
            
            case AdManager.AdType.AD_17:
            case AdManager.AdType.AD_18:
                AdManager.ShowAD_FailReset(adType, isSuccess =>
                {
                    if (!isSuccess)
                    {
                        bCanTouch = true;
                    }
                    else
                    {
                        ClosePopUp(successAction);
                    }
                });
                break;   
            default:
                break;
        }
    }

    void ClosePopUp(System.Action successAction = null)
    {
        successAction?.Invoke();

        base.ClosePopUp();
    }
    
    protected override void OnClickBtnClose()
    {
        failAction?.Invoke();
        
        base.OnClickBtnClose();
    }
}
