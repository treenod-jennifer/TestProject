using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PokoAddressable;
using Spine.Unity;

public class UIPopupStageChallenge : UIPopupBase
{
    public static UIPopupStageChallenge _instance = null;

    #region 도전 정보
    //오늘의 스테이지
    [SerializeField]
    private UILabel[] todayStageText;
    [SerializeField]
    private UILabel[] todayStageLabel;

    //도전 스코어
    [SerializeField]
    private UILabel[] targetScoreText;
    [SerializeField]
    private UILabel[] targetScoreLabel;

    [SerializeField]
    private UILabel time;

    //종료 시간 캐싱
    private long endTsTime;
    //마지막 회차인지 캐싱
    private bool isLastEvent = false;
    #endregion

    #region 보상
    //나의 스코어
    [SerializeField]
    private UILabel[] bestScoreText;
    [SerializeField]
    private UILabel[] bestScoreLabel;

    [SerializeField]
    private UILabel rewardText;
    [SerializeField]
    private UILabel[] rewardTargetScoreLabel;
    [SerializeField]
    private UIProgressBar rewardProgress;
    [SerializeField]
    private UIItemRewardBubble rewardBubble;
    [SerializeField]
    public GameObject checkObject;

    //보상 정보
    Protocol.AppliedRewardSet rewardSet = null;
    #endregion

    #region 스파인
    public SkeletonAnimation stageChallengeNpc;
    #endregion

    #region 기타 오브젝트
    public UIItemReadyBubble dialogBubble;

    [SerializeField]
    private GameObject playButton;

    //팝업 타이틀
    [SerializeField]
    private UILabel[] popupTitleText;

    //배경
    [SerializeField]
    private UIUrlTexture eventBGTexture;

    //뛰는 포코타
    [SerializeField]
    private UIStageBoniRun boniRun;
    [SerializeField]
    private GameObject boniParentTransform;

    //안내문 텍스트
    [SerializeField]
    private UILabel informationText;

    //말풍선 스프라이트 (height 조절을 위함)
    [SerializeField]
    private UIBasicSprite bubbleSprite;

    //말풍선 내 텍스트 (위치 조정을 위함)
    [SerializeField]
    private Transform bubbleLable;

    [SerializeField]
    private GameObject urlButton;
    #endregion

    #region 초기화 관련

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }

    public void InitPopup(Protocol.AppliedRewardSet rewardSet)
    {
        InitText();
        InitReward();
        InitRewardProgress();
        InitBoni();
        InitSpine();
        InitUrlButton();

        //보상받는 경우
        if (rewardSet != null)
        {
            this.rewardSet = rewardSet;

            playButton.SetActive(false);
            StartCoroutine(CoCompleteEvent());
            informationText.gameObject.transform.localPosition = new Vector3(0, -488, 0);
        }
        else if (ManagerStageChallenge.instance.IsClearStageChallenge == true ||
            ManagerStageChallenge.instance.IsEndStageChallenge == true)
        {
            playButton.SetActive(false);
            informationText.gameObject.transform.localPosition = new Vector3(0, -488, 0);
        }
    }

    private void InitText()
    {
        //텍스트 설정
        foreach (var item in todayStageText)
            item.text = Global._instance.GetString("p_msc_2");
        foreach (var item in targetScoreText)
            item.text = Global._instance.GetString("p_msc_3");
        foreach (var item in bestScoreText)
            item.text = Global._instance.GetString("p_msc_4");
        foreach (var item in popupTitleText)
            item.text = Global._instance.GetString("p_msc_1");
        rewardText.text = Global._instance.GetString("btn_29");

        ManagerStageChallenge ManagerInst = ManagerStageChallenge.instance;

        //값 설정
        foreach (var item in todayStageLabel)
            item.text = ManagerInst.Stage.ToString();
        foreach (var item in targetScoreLabel)
            item.text = string.Format("{0:n0}", ManagerInst.TargetScore);
        foreach (var item in bestScoreLabel)
            item.text =  string.Format("{0:n0}", ManagerInst.BestScore);
        foreach (var item in rewardTargetScoreLabel)
            item.text = string.Format("{0:n0}", ManagerInst.TargetScore);

        EndTsTimer.Run(target: time, endTs: ManagerInst.EndTs);
        endTsTime = ManagerInst.EndTs;
        isLastEvent = ManagerInst.IsLastEventStep;

        if (ManagerInst.IsClearStageChallenge == false) // 클리어 하기 전
            dialogBubble.text = Global._instance.GetString("msc_c_1");
        else if (ManagerInst.IsLastEventStep == true) //마지막 스테이지
            dialogBubble.text = Global._instance.GetString("msc_c_3");
        else
            //클리어 한 후
            dialogBubble.text = Global._instance.GetString("msc_c_2");

        this.gameObject.AddressableAssetLoad<Texture>("local_ui/stagechallenge_bg",
            (x) => eventBGTexture.mainTexture = x);
        if (dialogBubble.GetTextLineCount() < 3)
        {
            bubbleSprite.height = 95; // 말풍선 크기 고정 : 1줄짜리 대만어 표시 시 말풍선 크기가 줄어드는 이슈때문
            bubbleLable.localPosition = new Vector3(bubbleLable.localPosition.x, 10.7f, bubbleLable.localPosition.z);
        }
        else
            bubbleLable.localPosition = new Vector3(bubbleLable.localPosition.x, 18.7f, bubbleLable.localPosition.z);
    }

    private void InitReward()
    {
        rewardBubble.InitBubble(ManagerStageChallenge.instance.RewardInfo, ManagerStageChallenge.instance.IsClearStageChallenge);
    }

    private void InitRewardProgress()
    {
        int allCount = ManagerStageChallenge.instance.TargetScore;
        int stepCount = ManagerStageChallenge.instance.BestScore;

        float progressCount = (float)stepCount / allCount;

        rewardProgress.value = progressCount;
    }

    private void InitBoni()
    {
        //프로그래스 바의 전체 크기(값이 full일때 크기)에서 현재 value 만큼일 때의 크기 구하기
        float prograssBarForce_EndPos = rewardProgress.foregroundWidget.width * rewardProgress.value + 10f;
        bool isCompleteEvent = rewardProgress.value >= 1 ? true: false;

        boniParentTransform.transform.localPosition = new Vector3(-248.5f + prograssBarForce_EndPos, -31, 0);
        
        SetBoniRun(boniParentTransform.transform, isCompleteEvent);
    }

    private void InitSpine()
    {
        stageChallengeNpc.state.SetAnimation(0, ManagerStageChallenge.instance.ResourceIndex.ToString() + "_idle", true);
    }

    private void InitUrlButton()
    {
        urlButton.SetActive(!string.IsNullOrEmpty(ManagerStageChallenge.instance.UrlStr));
    }

    #endregion
    

    public IEnumerator CoCompleteEvent()
    {
        yield return CoCompleteEvent_OpenPopup();

        checkObject.SetActive(true);
    }

    private IEnumerator CoCompleteEvent_OpenPopup()
    {
        bCanTouch = false;

        //보상 팝업 띄우기
        if (rewardSet != null)
        {
            yield return new WaitForSeconds(0.2f);
            bool isGetReward = false;
            ManagerUI._instance.OpenPopupGetRewardAlarm
                (Global._instance.GetString("n_s_46"),
                () => { isGetReward = true; },
                rewardSet, 0.75f);

            //보상 팝업 종료될 때까지 대기.
            yield return new WaitUntil(() => isGetReward == true);
        }
        yield return null;
        bCanTouch = true;
    }

    private void OnClickVideoButton()
    {
        if (ManagerStageChallenge.instance.UrlStr != null)
        {
            var url = ManagerStageChallenge.instance.UrlStr;
            if (url.Length > 0)
            {
                Application.OpenURL(url);
            }
        }
    }

    private void OnClickStartButton()
    {
        StartCoroutine(CoStartButton());
    }

    IEnumerator CoStartButton()
    {
        UserSelf myProfile = SDKGameProfileManager._instance.GetMyProfile();
        int stageIndex = ManagerStageChallenge.instance.Stage;

        if (Global.LeftTime(endTsTime) < 0)
        {
            //현재 이벤트 종료
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            if (isLastEvent == true)
            {
                //마지막 회차까지 종료
                popup.InitSystemPopUp("MESSAGE", Global._instance.GetString("n_ev_13"), false);
            }
            else
            {
                //다음 회차 있는 경우
                popup.InitSystemPopUp("MESSAGE", Global._instance.GetString("n_ev_17"), false);
            }

            popup.uiPanel.useSortingOrder = true;
            popup.uiPanel.sortingOrder = 0;

            //종료 안내 팝업 닫힐 때까지 대기
            yield return new WaitUntil(() => popup == null);

            ManagerUI._instance.ClosePopUpUI();
            yield return new WaitForSeconds(0.2f);

            //이벤트 정보 갱신
            ManagerStageChallenge.instance.InitData();

            //다음 이벤트 있으면 다음 이벤트 출력
            if(ManagerStageChallenge.instance.IsEndStageChallenge == false)
            {
                ManagerUI._instance.OpenPopupStageChallenge();
            }
        }
        else if (myProfile.stage >= stageIndex)
        {
            //도전장 진행
            ManagerUI._instance.ClosePopUpUI();
            yield return new WaitForSeconds(0.2f);

            Global.SetGameType_NormalGame(stageIndex);
            ManagerUI._instance.OpenPopupReadyStageCallBack();
        }
        else
        {
            //도전장 스테이지가 아직 오픈되지 않은 경우
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp("MESSAGE", Global._instance.GetString("n_ev_16"), false);

            popup.uiPanel.useSortingOrder = true;
            popup.uiPanel.sortingOrder = 0;
        }

        yield break;
    }

    public void SetBoniRun(Transform target, bool isBoniStop = false)
    {
        boniRun.transform.parent = target;
        boniRun.transform.localPosition = Vector3.zero;
        boniRun.SetCharacter(true, isBoniStop);
        boniRun.SetArrow(BtnStageArrowDir.none);
        boniRun.SetBoniRunSpeed(5.0f);
        StartCoroutine(boniRun.CoArrowAnimation());

        boniRun.gameObject.SetActive(true);
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
        if (stageChallengeNpc != null)
            stageChallengeNpc.gameObject.GetComponent<MeshRenderer>().sortingOrder = uiPanel.sortingOrder + 1;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

}
