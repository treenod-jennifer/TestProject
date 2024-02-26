using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupPokoFlowerEvent : UIPopupBase
{
    public static UIPopupPokoFlowerEvent _instance = null;

    public UIProgressBar progress;
    public UIUrlTexture backImage;

    public UILabel flowerCountText;

    public UILabel eventTime;
    public GameObject loadText;

    public GameObject startButton;

    public UILabel targetCount;

    //보상
    public GenericReward rewardBubbles;
    public GenericReward rewardFriend;

    [SerializeField] private UILabel acievedCount;
    [SerializeField] private UILabel maxRewardCount;

    [SerializeField] private GameObject rewardRoot;
    [SerializeField] private GameObject maxRewardRoot;

    private Method.FunctionVoid callBackEnd = null;

    private int contentIndex = -1;
    private int allCount = 0;
    private int progressCount = 0;

    private long time = 0;

    static public bool rewardReceivedNotified = false;
    static public bool needDisplayOnLobbyIn = false;

    #region 보상
    Protocol.AppliedRewardSet rewardSet = null;
    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
            _instance = null;
    }

    public void InitData(int eventIndex, bool clearEcofiEvent = false, Protocol.AppliedRewardSet rewardSet = null)
    {
        SettingLoad();

        contentIndex = eventIndex;

        //보상 적용
        this.rewardSet = rewardSet;

        // 타이틀 이미지를 어떻게 받아올지에 대해서 이야기 필요.
        string name = string.Format("bEventPopupBG_{0}", ManagerPokoFlowerEvent.PokoFlowerEventResourceIndex);
        backImage.SettingTextureScale(706, 982);
        backImage.SuccessEvent += SettingLoadComplete;
        backImage.LoadCDN(Global.gameImageDirectory, "IconEvent/", name);

        //보상 달성 횟수
        acievedCount.text = ManagerPokoFlowerEvent.PokoFlowerEventAcievedCount.ToString();
        maxRewardCount.text = $"/{ManagerPokoFlowerEvent.PokoFlowerEventMaxRewardCount}";

        //보상 설정
        rewardBubbles.SetReward(ManagerPokoFlowerEvent.PokoflowerEventReward);
        rewardFriend.SetReward(ManagerPokoFlowerEvent.PokoflowerEventReward);

        //시간 설정
        eventTime.text = Global.GetTimeText_MMDDHHMM_Plus1(ManagerPokoFlowerEvent.PokoFlowerEventEndTs);

        SettingCount();

        SettingProgressBar();

        //스테이지 클리어 후 에코피 이벤트 클리어 했을 경우
        if (clearEcofiEvent)
        {
            startButton.gameObject.SetActive(false);

            if(ManagerPokoFlowerEvent.currentPokoFlowerCount == 0)
            {
                progress.value = 1;
                flowerCountText.text = $"{ManagerPokoFlowerEvent.targetPokoFlowerCount}/{ManagerPokoFlowerEvent.targetPokoFlowerCount}";
            }
            _callbackOpen += () => OpenRewardAlarmPopup();
        }
    }

    private void OpenRewardAlarmPopup()
    {
        //보상 팝업 띄우기
        if (rewardSet != null)
        {
            ManagerUI._instance.OpenPopupGetRewardAlarm
            (Global._instance.GetString("n_s_46"),
            SetSendItemPopup,
            rewardSet, 0.75f);
        }
    }

    private void SetSendItemPopup()
    {
#if UNITY_EDITOR
        ManagerUI._instance.OpenPopup<UIPopupSendScoreUpItem>();
#else
        if(ManagerPokoFlowerEvent.SendFriendList().Count == 0)
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ecp_5"), false);
            systemPopup.SortOrderSetting();
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSendScoreUpItem>();
        }
#endif
    }

    private void SettingProgressBar()
    {
        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
        {
            progress.value = (float)ManagerPokoFlowerEvent.currentPokoFlowerCount / (float)ManagerPokoFlowerEvent.targetPokoFlowerCount;
        }
        else
            progress.value = 0;
    }

    private void SettingCount()
    {
        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
        {
            progressCount = ManagerPokoFlowerEvent.currentPokoFlowerCount;
            allCount = ManagerPokoFlowerEvent.targetPokoFlowerCount;
        }
        else
        {
            progressCount = 0;
            allCount = 1;
        }

        //혹시라도 모은 수가 전체 수보다 많다면, 전체수 만큼만 출력.
        if (progressCount > allCount)
            progressCount = allCount;

        //카운트 설정.
        targetCount.text = allCount.ToString();
        flowerCountText.text = string.Format("{0}/{1}", progressCount.ToString(), allCount.ToString()); ;
    }

    private void SettingLoad()
    {
        rewardRoot.SetActive(false);
        maxRewardRoot.SetActive(false);
        loadText.SetActive(true);
    }

    private void SettingLoadComplete()
    {
        if (ManagerPokoFlowerEvent.PokoFlowerEventAcievedCount >= ManagerPokoFlowerEvent.PokoFlowerEventMaxRewardCount)
        {
            rewardRoot.SetActive(false);
            maxRewardRoot.SetActive(true);
        }
        else
        {
            rewardRoot.SetActive(true);
            maxRewardRoot.SetActive(false);
        }
        loadText.SetActive(false);
    }

    void OnClickStartButton()
    {
        StartCoroutine(CoStartButton());
    }

    IEnumerator CoStartButton()
    {
        ManagerUI._instance.ClosePopUpUI();

        yield return new WaitForSeconds(0.2f);

        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
        {
            UIReuseGrid_Stage.ScrollMode scrollMode = UIReuseGrid_Stage.ScrollMode.FlowerFind;
            switch (ManagerPokoFlowerEvent.CalcStageBtnLevel())
            {
                case 1:
                    scrollMode = UIReuseGrid_Stage.ScrollMode.FlowerFind; break;
                case 2:
                    scrollMode = UIReuseGrid_Stage.ScrollMode.BlueFlowerFind; break;
                case 3:
                    scrollMode = UIReuseGrid_Stage.ScrollMode.RedFlowerFind; break;
                default:
                    ManagerUI._instance.OpenPopupStage();
                    yield break;
            }
            ManagerUI._instance.OpenPopupStage(scrollMode);
        }
    }

    void OnClickRedFlowerInfo()
    {
        ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEventRedFlowerInfo>();
    }
}
