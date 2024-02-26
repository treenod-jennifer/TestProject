using Newtonsoft.Json;
using UnityEngine;
using DG.Tweening;

public class UIPopupRequestReview : UIPopupBase
{
    public static UIPopupRequestReview _instance = null;
    
    [SerializeField] private GameObject evalutionPopup;
    [SerializeField] private GameObject evalution_bDayDeco;

    [SerializeField] private GameObject requestReviewPopup;
    [SerializeField] private UISprite requestReviewImage;
    [SerializeField] private GameObject requestReview_bDayDeco;
    [SerializeField] private UILabel requestReview_text;
    
    [SerializeField] private GameObject surveyPopup;
    [SerializeField] private UISprite surveyPopupImage;
    [SerializeField] private UIItemRequestReview[] arrayItemSurvey;
    [SerializeField] private UISprite btnSurveyImage;
    [SerializeField] private UILabel[] btnSurveyText;
    
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private UISprite resultPopupImage;
    
    private int _version = 0;
    private bool isBirthDay = false;
    
    //선택된 설문 번호
    private int selectSurveyIdx = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _instance    = null;
    }
    
    public void InitData(int version)
    {
        _version = version;
        //생일 리소스 설정(0 : OFF, 1 : ON)
        isBirthDay = (ServerRepos.LoginCdn.appReviewResource == 1);     
        
        //탭 활성화
        evalutionPopup.SetActive(true);
        requestReviewPopup.SetActive(false);
        surveyPopup.SetActive(false);
        resultPopup.SetActive(false);
        
        //생일 UI
        evalution_bDayDeco.SetActive(isBirthDay);
    }

    private void OnClickBtnOK_Evalution()
    {
        evalutionPopup.SetActive(false);
#if UNITY_ANDROID
        OnClickOpenRequestReview();
#elif UNITY_IPHONE || UNITY_IOS
        OnClickBtnGoToReview();
#endif
    }

    /// <summary>
    /// "리뷰를 남겨줄래?" 팝업
    /// </summary>
    private void OnClickOpenRequestReview()
    {
        requestReviewPopup.SetActive(true);
        requestReviewImage.color = new Color(1f, 1f, 1f, 0.5f);
        DOTween.ToAlpha(() => requestReviewImage.color, x => requestReviewImage.color = x, 1f, 0.3f);
        requestReview_bDayDeco.SetActive(isBirthDay);
        
        requestReview_text.text = (isBirthDay == false)
            ? Global._instance.GetString("p_arv_2")
            : Global._instance.GetString("p_arv_3");
    }

    /// <summary>
    /// 마켓의 앱 리뷰 페이지로 이전 
    /// </summary>
    private void OnClickBtnGoToReview()
    {
        Review.RequestReview();
        PlayerPrefs.SetInt(Review.key, _version);

        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.APP_REVIEW,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.APP_REVIEW_ACTION,
            "ACCEPT",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

        ClosePopUp();
    }
    
    /// <summary>
    /// 설문 팝업 출력
    /// </summary>
    private void OnClickOpenSurvayPopup()
    {
        evalutionPopup.SetActive(false);
        surveyPopup.SetActive(true);
        surveyPopupImage.color = new Color(1f, 1f, 1f, 0.7f);
        DOTween.ToAlpha(() => surveyPopupImage.color, x => surveyPopupImage.color = x, 1f, 0.3f);
        selectSurveyIdx = 0;
        for (int i = 0; i < arrayItemSurvey.Length; i++)
        {
            arrayItemSurvey[i].InitItem((i + 1),(x) => OnClickBtnSurvey(x));
        }
    }

    /// <summary>
    /// 설문 항목 선택
    /// </summary>
    private void OnClickBtnSurvey(int selectIdx)
    {
        if (selectSurveyIdx == selectIdx)
            return;
        
        selectSurveyIdx = selectIdx;
        
        var colorData = ManagerUI._instance.GetButtonColorData_BigButton(BtnColorType.green);
        btnSurveyImage.spriteName = colorData.Item1;
        btnSurveyText[0].effectColor = colorData.Item2;
        btnSurveyText[1].color = colorData.Item2;
        btnSurveyText[1].effectColor = colorData.Item2;
        
        foreach (var item in arrayItemSurvey)
        {
            item.UpdateItem(item.Index == selectIdx);
        }
    }

    /// <summary>
    /// 설문 전송
    /// </summary>
    private void OnClickConfirmSurvayPopup()
    {
        if (selectSurveyIdx == 0)
            return;
        
        PlayerPrefs.SetInt(Review.key, _version);
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.APP_REVIEW,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.APP_REVIEW_ACTION,
            selectSurveyIdx.ToString(),
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

        OnClickOpenResult();
    }

    /// <summary>
    /// CS 웹페이지로 이동
    /// </summary>
    private void OnClickBtnGoToCS()
    {
        Application.OpenURL("https://contact-cc.line.me/lg/categoryId/13639?continue_without_login=true");
        ClosePopUp();
    }

    /// <summary>
    /// 결과 화면 출력
    /// </summary>
    private void OnClickOpenResult()
    {
        surveyPopup.SetActive(false);
        resultPopup.SetActive(true);
        resultPopupImage.color = new Color(1f, 1f, 1f, 0.7f);
        DOTween.ToAlpha(() => resultPopupImage.color, x => resultPopupImage.color = x, 1f, 0.3f);
    }
    
    /// <summary>
    /// X 버튼 클릭 (팝업 종료)
    /// </summary>
    private void OnClickClosePopup()
    {
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.APP_REVIEW,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.APP_REVIEW_ACTION,
            "CLOSE",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        
        base.OnClickBtnClose();
    }
}
