using System.Collections;
using UnityEngine;
using System;

public class UIPopupOption : UIPopupBase
{
    public static UIPopupOption _instance = null;

    public UILabel      versionText;
    public UILabel      AppReviewVersionText;

    public GameObject   lineMessage_check;
    public GameObject[] sound_check;
    public GameObject   logoutBtn;
    public GameObject   guestMigrationButton;
    public GameObject   objAppIconChange;
    
    public UITable      table;
    
    // 개발 환경 치트
    public UILabel    uid;
    public UILabel    cdnVersion;
    public GameObject anchorDown;

    #region 팝업 크기 조정에 필요한 값
    private const int POPUP_ACCOUNT_SIZE_CORRECT    = 120;
    private const int POPUP_ACCOUNT_HEIGHT_CORRECT  = 60;
    private const int EXTERNAL_LINKS_HEIGHT_CORRECT = 75;

    [SerializeField] private GameObject _objPopupRoot;
    [SerializeField] private GameObject _objPopupExternalLinks;
    [SerializeField] private GameObject _objBtnAccountDelete;
    [SerializeField] private UIGrid     _objBtnGrid;
    #endregion
    
    private void Awake ()
    {
        if ( _instance == null )
        {
            _instance = this;
        }
    }

    private void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {

        }
        else
        {
            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
            Profile_PION profileData = myProfile.GetPionProfile();
            if (profileData != null)
            {
                // 값이 없으면 메세지 받기
                OnClickEventSpriteCheck(lineMessage_check, !profileData.profile.isLineMessageBlocked());
            }
        }

        OnClickEventSpriteCheck(sound_check[0], Global._optionBGM);
        OnClickEventSpriteCheck(sound_check[1], Global._optionSoundEffect);

        // 앱 버전 표시.
        versionText.text = "v" + NetworkSettings.Instance.gameAppVersion;
        if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.DevServer || NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.CustomDevServer)
        {
            versionText.text += " d";
            versionText.text += $" {LanguageUtility.SystemCountryCode}";
            versionText.text += " " + (NetworkSettings.Instance.buildedTimeString.Length > 0 ? NetworkSettings.Instance.buildedTimeString : string.Format("{0:yyMMdd}_{0:HHmm}", DateTime.Now));
            versionText.effectStyle = UILabel.Effect.Outline;
        }
        else if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LevelTeamServer)
        {
            versionText.text += " L";
            versionText.text += $" {LanguageUtility.SystemCountryCode}";
            versionText.text += " " + (NetworkSettings.Instance.buildedTimeString.Length > 0 ? NetworkSettings.Instance.buildedTimeString : string.Format("{0:yyMMdd}_{0:HHmm}", DateTime.Now));
            versionText.effectStyle = UILabel.Effect.Outline;
        }
        else if (
            NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.DevQAServer ||
            NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LiveQAServer ||
            NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.Pub_QAServer)
        {
            versionText.text += " a";
            versionText.text += $" {LanguageUtility.SystemCountryCode}";
            versionText.text += " " + (NetworkSettings.Instance.buildedTimeString.Length > 0 ? NetworkSettings.Instance.buildedTimeString : string.Format("{0:yyMMdd}_{0:HHmm}", DateTime.Now));
            versionText.effectStyle = UILabel.Effect.Outline;
        }
        
        // 빌드 버전 표기.
        bool dontSee = PlayerPrefs.HasKey(Review.key) && PlayerPrefs.GetInt(Review.key) == ServerRepos.LoginCdn.appReviewVer;
        if (AppReviewVersionText != null)
        {
            AppReviewVersionText.text = $"Data Reset(Ver{ServerRepos.LoginCdn.appReviewVer}) Don't See : {dontSee}";
        }

        #region 옵션 팝업창 버튼 활성화 여부
        var isGuestLogin = ServiceSDK.ServiceSDKManager.instance.IsGuestLogin();

        SetPopupSetting(isGuestLogin || ServerRepos.LoginCdn.deleteUserOnOff > 0 == false);
        logoutBtn.SetActive(!isGuestLogin);
        _objBtnAccountDelete.SetActive(ServerRepos.LoginCdn.deleteUserOnOff > 0);

        _objBtnGrid.Reposition();
        #endregion
        

#if UNITY_IOS
        // 게스트 계정 마이그레이션 버튼 출력.
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            guestMigrationButton.SetActive(true);
        }
        
        // 앱 아이콘 변경 버튼 출력.
        if (AppIconChanger.iOS.SupportsAlternateIcons)
        {
            if (ServerContents.AppIconVer > 0)
            {
                objAppIconChange.SetActive(true);
            }
        }
#else
        guestMigrationButton.SetActive(false);
        objAppIconChange.SetActive(false);
#endif
        // 테이블 재정렬.
        table.Reposition();
        
        // 테스트 버튼들 아래 위치 (개발 환경 치트).
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
        {
            StartCoroutine(AnchorSetting());
        }
        
        // 유저 이름 표시 (개발 환경 치트).
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
        {
            uid.text = $"User ID : {ServerRepos.User.uid}";
            uid.gameObject.SetActive(true);

            cdnVersion.gameObject.SetActive(true);
            cdnVersion.text = $"CdnV: {ServerRepos.CdnVsn.ToString()}";
        }
    }
    
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }
    }

    protected override void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    private IEnumerator AnchorSetting()
    {
        yield return new WaitForSeconds(0.2f);
        anchorDown.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnClickBtnPlusInfo()
    {
        ServerAPI.GetOptionLink((resp) =>
        {
            ManagerUI._instance.OpenPopupPlusInfo(resp);
        });
    }

    /// <summary>
    /// BGM ON/OFF
    /// </summary>
    private void OnClickBtnBGM()
    {
        if (Global._optionBGM == true)
        {
            Global._optionBGM = false;
            PlayerPrefs.SetInt("_optionBGM", 0);
            sound_check[0].SetActive(false);
            ManagerSound._instance.PauseBGM();
        }
        else
        {
            Global._optionBGM = true;
            PlayerPrefs.SetInt("_optionBGM", 1);
            sound_check[0].SetActive(true);
            ManagerSound._instance.PlayBGM();
        }
    }

    /// <summary>
    /// Sound Effect ON/OFF
    /// </summary>
    private void OnClickBtnSoundEffect()
    {
        if (Global._optionSoundEffect == true)
        {
            Global._optionSoundEffect = false;
            PlayerPrefs.SetInt("_optionSoundEffect", 0);
            sound_check[1].SetActive(false);
        }
        else
        {
            Global._optionSoundEffect = true;
            PlayerPrefs.SetInt("_optionSoundEffect", 1);
            sound_check[1].SetActive(true);
        }
    }
    
    /// <summary>
    /// 라인 메시지 ON/OFF
    /// </summary>
    private void OnClickBtnLineMessage()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            //UpdateGameProfileData
        }
        else
        {
            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var profileData = myProfile.GetPionProfile();
            if (profileData != null)
            {
                int[] opts = { profileData.profile.isLineMessageBlocked() ? 0 : 1, profileData.profile.isLineTumbnailUsed() ? 1 : 0 };
                profileData.profile.opts = opts;

                SDKGameProfileManager._instance.UpdateGameProfileData(profileData.profile, OnClickBtnMyProfileHandler);
            }
        }
    }

    private void OnClickBtnMyProfileHandler(Profile_PIONCustom profileInfo)
    {
        //Debug.Log("라인 메세지 블록 설정 완료 " + profileInfo.profile.isLineMessageBlocked());
        if (profileInfo != null)
        {
            int[] opts = { profileInfo.isLineMessageBlocked() ? 1 : 0, profileInfo.isLineTumbnailUsed() ? 1 : 0 };
            
            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
            myProfile.GetPionProfile().profile.opts = opts;
            
            OnClickEventSpriteCheck(lineMessage_check, !profileInfo.isLineMessageBlocked());
        }
    }
    
    /// <summary>
    /// 언어 선택 팝업 출력
    /// </summary>
    public void OnClickLanguageSelectButton()
    {
        ManagerUI._instance.OpenPopup<UIPopUpLanguageSelect>((popup) => popup.InitPopup(true));
    }
    
    /// <summary>
    /// 게스트 계정 마이그레이션 팝업 출력
    /// </summary>
    private void OnClickBtnGuestMigration()
    {
        if (UIPopupGuestMigrationHowTo.instance == null)
        {
            UIPopupGuestMigrationHowTo popup = ManagerUI._instance.OpenPopup<UIPopupGuestMigrationHowTo>();
            popup.InitPopup();
        }
    }
    
    /// <summary>
    /// 앱 변경 기능 팝업 출력
    /// </summary>
    private void OnClickBtnAppIconChange()
    {
        ManagerUI._instance.OpenPopup<UIPopupChangeAppIcon>();
    }

    /// <summary>
    /// Notice 페이지 오픈
    /// </summary>
    private void OnClickBtnNotice()
    {
        StartCoroutine(NoticeHelper.CoShowNotice());
    }
    
    /// <summary>
    /// help 페이지 오픈
    /// </summary>
    private void OnClickBtnHelp()
    {
        //TO DO: 라인측에 documentId를 받아 와야함
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardHelp, "", "");
    }
    
    /// <summary>
    /// 인트로 씬 출력
    /// </summary>
    private void OnClickIntro()
    {
        ManagerLobby.showIntro = true;
        
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Intro");
        OnClickBtnClose();
        ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
        ManagerUI._instance.AnchorTopDestroy();
        ManagerSound._instance.StopBGM();
    }
    
    /// <summary>
    /// 유저 정보 팝업 출력
    /// </summary>
    private void OnClickBtnUserInfo()
    {
        ManagerUI._instance.OpenPopupUserInfo();
    }
    
    /// <summary>
    /// 푸시 알림 설정 팝업 출력
    /// </summary>
    private void OnClickBtnPushSetting()
    {
        ManagerUI._instance.OpenPopupPushSetting();
    }
    
    /// <summary>
    /// 라인 로그아웃
    /// </summary>
    private void OnClickLineLogout()
    {
        ManagerUI._instance.RebootSignOut();
    }

    /// <summary>
    /// 유저 어카운트 삭제
    /// </summary>
    private void OnClickAccountDelete()
    {
        _callbackClose += () => ManagerUI._instance.OpenPopup<UIPopupAccountDeleteAgree>();
        OnClickBtnClose();
    }

    private void OnClickEventSpriteCheck(GameObject obj, bool bShow)
    {
        obj.SetActive(bShow);
    }

    private SystemLanguage StringToLanguage(string language)
    {
        return (SystemLanguage)Enum.Parse(typeof(SystemLanguage), language);
    }

    private void SetPopupSetting(bool isSmallPopup = true)
    {
        mainSprite.SetRect(0f, 0f, mainSprite.localSize.x,
            mainSprite.localSize.y + (isSmallPopup ? -POPUP_ACCOUNT_SIZE_CORRECT : 0));
        mainSprite.transform.localPosition = new Vector3(0f, 120f, 0f);
        _objPopupRoot.transform.localPosition = new Vector3(_objPopupRoot.transform.localPosition.x,
            _objPopupRoot.transform.localPosition.y +
            (isSmallPopup ? -POPUP_ACCOUNT_HEIGHT_CORRECT : 0), 0);
        _objPopupExternalLinks.transform.localPosition = new Vector3(_objPopupExternalLinks.transform.localPosition.x,
            _objPopupExternalLinks.transform.localPosition.y +
            (isSmallPopup ? EXTERNAL_LINKS_HEIGHT_CORRECT : 0), 0);
    }
}
