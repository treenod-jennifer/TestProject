using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupLoginADBonus : UIPopupBase
{
    public static UIPopupLoginADBonus _instance { get; private set; }

    [Header("Top_UI")]
    [SerializeField] private UILabel labelUserName;
    [SerializeField] private UILabel labelMessage;
    [SerializeField] private UILabel labelEndTs;
    [SerializeField] private UIUrlTexture texTitleImage;

    [Header("ScrollView_UI")]
    [SerializeField] private GameObject objScrollViewGrid;
    [SerializeField] private GameObject objLoginADBonusItem;
    [SerializeField] private UIScrollView scrollView;
    [SerializeField] private GameObject objComplete;
    [SerializeField] public  Animation anim;
    [SerializeField] private UIPanel panelComplete;

    [Header("AD_UI")]
    [SerializeField] private GenericReward adReward;
    [SerializeField] private GameObject objADButtonBlock;

    private CdnLoginAdBonus contentsLoginADBonus;
    private ServerUserLoginAdBonus userLoginADBonus;

    List<LoginADBonusData> listLoginADBonusData = new List<LoginADBonusData>();

    int type = 0;

    private void Awake()
    {
        _instance = this;
    }

    protected override void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollView.GetComponent<UIPanel>().depth = depth + 1;
        panelComplete.depth = depth + 2;
    }

    public void InitData(int type, Protocol.AppliedRewardSet rewardSet = null)
    {
        this.type = type;

        adPosition = objComplete.transform.position;

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        labelUserName.text = myProfile.GameName;
        
        string message = Global._instance.GetString($"login_ad_{type}");
        int adRewardIndex = ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).loginCnt - 1;
        if (ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).adRewardStatus[adRewardIndex] == 1)
            message = Global._instance.GetString($"login_ad_0");
        labelMessage.text = message;

        texTitleImage.SettingTextureScale(638, 236);
        texTitleImage.LoadCDN(Global.gameImageDirectory, "IconLoginAD/", $"l_Title_{ManagerLoginADBonus.Instance.GetLoginADBonus(type).resourceIndex}.png");

        //이벤트 데이터 세팅
        SetLoginADData(rewardSet);

        //이벤트 종료시간 세팅
        labelEndTs.text = Global.GetTimeText_MMDDHHMM_Plus1(userLoginADBonus.endTs);

        //로그인 보상 세팅
        SetScroll();

        //광고 보상 세팅
        SetADReward();

    }

    void SetADReward()
    {
        int adRewardIndex = ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).loginCnt - 1;

        Reward reward = ManagerLoginADBonus.Instance.GetLoginADBonus(type).addReward[adRewardIndex];

        adReward.SetReward(reward);

        objADButtonBlock.SetActive(ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).adRewardStatus[adRewardIndex] == 1);

        objComplete.SetActive(ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).adRewardStatus[adRewardIndex] == 1);
    }

    void SetScroll()
    {
        int arrayCount = Mathf.CeilToInt((float)contentsLoginADBonus.reward.Count / 3.0f);

        for (int i = 0; i < arrayCount; i++)
        {
            List<LoginADBonusData> loginADBonusDatas = new List<LoginADBonusData>();

            UIItemLoginADBonusArray itemRoot =
                NGUITools.AddChild(objScrollViewGrid.transform, objLoginADBonusItem).GetComponent<UIItemLoginADBonusArray>();

            for (int j = 0; j < 3; j++)
            {
                int index = (i * 3) + j;

                if (index < listLoginADBonusData.Count)
                    loginADBonusDatas.Add(listLoginADBonusData[index]);
            }

            itemRoot.InitData(loginADBonusDatas);
        }

        objScrollViewGrid.GetComponent<UIGrid>().Reposition();

        bool isScrollBar = ManagerLoginADBonus.Instance.GetLoginADBonus(type).reward.Count > 7;

        scrollView.enabled = isScrollBar;

        scrollView.verticalScrollBar.alpha = isScrollBar ? 1 : 0;

        if (isScrollBar)
        {
            int value = (ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).loginCnt - 1) / 3;

            scrollView.SetDragAmount(0, value * 0.5f, true);

            float OffsetY = scrollView.GetComponent<UIPanel>().clipOffset.y;

            scrollView.GetComponent<Transform>().localPosition = new Vector3(-10f, -OffsetY, scrollView.GetComponent<Transform>().localPosition.z);
        }
        else
        {
            scrollView.GetComponent<Transform>().localPosition = new Vector3(0f, scrollView.GetComponent<Transform>().localPosition.y, scrollView.GetComponent<Transform>().localPosition.z);
        }
    }

    void SetLoginADData(Protocol.AppliedRewardSet rewardSet)
    {
        contentsLoginADBonus = ManagerLoginADBonus.Instance.GetLoginADBonus(type);
        userLoginADBonus = ManagerLoginADBonus.Instance.GetUserLoginADBonus(type);

        
        for (int i = 0; i < contentsLoginADBonus.reward.Count; i++)
        {
            List<Reward> rewards = new List<Reward>();

            rewards.Add(contentsLoginADBonus.reward[i]);

            if (i == contentsLoginADBonus.reward.Count - 1
                && contentsLoginADBonus.lastReward.Count > 0)
                rewards.Add(contentsLoginADBonus.lastReward[0]);

            rewards.Add(contentsLoginADBonus.addReward[i]);

            listLoginADBonusData.Add(new LoginADBonusData(
                    day: i + 1,
                    complete: i + 1 <= userLoginADBonus.loginCnt,
                    action: rewardSet != null ?   i + 1 == userLoginADBonus.loginCnt : false,
                    rewards: rewards,
                    rewardSet: rewardSet
                    ));
        }
    }

    public IEnumerator PlayClear(Vector3 posComplete, System.Action action = null)
    {
        objComplete.transform.position = posComplete;

        anim.Stop();
        anim.Play("LoginADBonus_Complete");

        while (anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }

        action?.Invoke();

        objComplete.SetActive(action == null);
    }

    bool IsShowAD = false;
    Vector3 adPosition = Vector3.zero;

    public void ShowAD()
    {
        if (AdManager.ADCheck(AdManager.AdType.AD_14))
        {
            AdManager.ShowAD_ReqAdLoginBonus(AdManager.AdType.AD_14, type, ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).loginCnt, (isSuccess) =>
            {
                if (isSuccess)
                {
                    ManagerUI._instance.UpdateUI();
                    
                    objADButtonBlock.SetActive(true);

                    labelMessage.text = Global._instance.GetString($"login_ad_0");

                    IsShowAD = true;
                }
            });
        }
        else
        {
            //광고가 없는 경우
            OpenPopupNoADs();
        }
    }

    private void OpenPopupNoADs()
    {
        ManagerUI._instance?.OpenPopup<UIPopupSystem>
        (
            (popup) =>
            {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString("n_s_48"),
                    useButtons: false
                );
            }
        );
    }

    protected override void OnFocus(bool focus)
    {
        if(focus)
        {
            if(IsShowAD)
                StartCoroutine(PlayClear(adPosition));
        }
    }
}