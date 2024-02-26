using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopupOption : UIPopupBase
{
    public static UIPopupOption _instance = null;

    public GameObject anchorDown;

    public GameObject check;
    public GameObject lineMessage_check;
    public GameObject[] language_check;
    public GameObject[] sound_check;
    public GameObject getStart;
    public GameObject starLock;
    public UISprite starBox;
    public UILabel versionText;
    public UILabel userNameText;
    public UILabel CDNVersionText;

    private int currentLanguageIndex = -1;

    void Awake ()
    {
        if ( _instance == null )
        {
            _instance = this;
        }
    }

    void Start()
    {
        check.SetActive(Global._instance.showInGameClearBTN);
        if (Global._systemLanguage == CurrLang.eJap)
            SetLanguageCheck(0);
     //   else if (Global._systemLanguage == CurrLang.eKor)
      //      SetLanguageCheck(1);
        else //if (Global._systemLanguage == CurrLang.eEng)
            SetLanguageCheck(1);

        if (Global._debugEdit == false)
        {
            starBox.spriteName = "ready_item_off";
            getStart.SetActive(false);
            starLock.SetActive(true);
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {

        }else
        {
           // SDKGameProfileInfo profileData = ManagerData._instance._dicUserProfileInfo[ManagerData._instance.userData._profile.userKey];

            // 값이 없으면 메세지 받기
          //  OnClickEventSpriteCheck(lineMessage_check, !profileData.profile.isLineMessageBlocked());
        }

        OnClickEventSpriteCheck(sound_check[0], Global._optionBGM);
        OnClickEventSpriteCheck(sound_check[1], Global._optionSoundEffect);

 

        GameObject objGameProfile = new GameObject();

    }
    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();

    }
    IEnumerator AnchorSetting()
    {
        yield return new WaitForSeconds(0.2f);
        anchorDown.SetActive(true);
    }

    void OnClickBtnLineMessage()
    {

    }
    void OnClickBtnReset()
    {

       //데이터 리셋.
    }

    void OnClickInvite()
    {
        ManagerUI._instance.OpenPopupInviteSmall();
    }

    void OnClickReqClover()
    {
        ManagerUI._instance.OpenPopupRequestSmall();
    }



    void OnClickIntro()
    {
        Application.LoadLevel("Intro");
        OnClickBtnClose();
        ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
        UIButtonPackage.RemoveAll();
        ManagerUI._instance.AnchorTopRightDestroy();
        ManagerSound._instance.StopBGM();
    }
    void OnClickBtnJapanese()
    {
        if (Global._systemLanguage == CurrLang.eJap)
            return;
        Global._systemLanguage = CurrLang.eJap;
        PlayerPrefs.SetInt("_systemLanguage", (int)Global._systemLanguage);
        SetLanguageCheck(0);
    }
    
  /*  void OnClickBtnKorea()
    {
        if (Global._systemLanguage == CurrLang.eKor)
            return;
        Global._systemLanguage = CurrLang.eKor;
        PlayerPrefs.SetInt("_systemLanguage", (int)Global._systemLanguage);
        SetLanguageCheck(1);
    }*/

    void OnClickBtnEnglish()
    {

        Global._systemLanguage = CurrLang.eEng;
        PlayerPrefs.SetInt("_systemLanguage", (int)Global._systemLanguage);
    }

    void OnClickBtnBGM()
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

    void GetAudioInFTP()
    {
        ManagerSound._instance.GetAudioFTP();
    }

    void OnClickBtnSoundEffect()
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

    void OnClickDeleteLocalImage()
    {
        if (System.IO.Directory.Exists(Global.gameDataDirectory))
        {
            System.IO.Directory.Delete(Global.gameDataDirectory, true);
            System.IO.Directory.CreateDirectory(Global.gameDataDirectory);
        }

        if (System.IO.Directory.Exists(Global.gameImageDirectory))
        {
            System.IO.Directory.Delete(Global.gameImageDirectory, true);
            System.IO.Directory.CreateDirectory(Global.gameImageDirectory);
        }

        if (System.IO.Directory.Exists(Global.noticeDirectory))
        {
            System.IO.Directory.Delete(Global.noticeDirectory, true);
            System.IO.Directory.CreateDirectory(Global.noticeDirectory);
        }

     /*   if (System.IO.Directory.Exists(Global.thumbnailDirectory))
        {
            System.IO.Directory.Delete(Global.thumbnailDirectory, true);
            System.IO.Directory.CreateDirectory(Global.thumbnailDirectory);
        }*/

        if (System.IO.Directory.Exists(Global.StageDirectory))
        {
            System.IO.Directory.Delete(Global.StageDirectory, true);
            System.IO.Directory.CreateDirectory(Global.StageDirectory);
        }
    }
    
    void OnClickBtnUserInfo()
    {
        ManagerUI._instance.OpenPopupUserInfo();
    }

    void OnClickBtnPushSetting()
    {
        ManagerUI._instance.OpenPopupPushSetting();
    }

    void SetLanguageCheck(int index)
    {
        if (currentLanguageIndex > -1)
        {
            language_check[currentLanguageIndex].SetActive(false);
        }
        language_check[index].SetActive(true);
        currentLanguageIndex = index;
    }

    void OnGUI()
    {
        //GUI.color = Color.white;
        //GUILayout.Label( ManagerData._instance.userData._profile.name + "  uid " + ServerRepos.User.uid );
    }

    /// <summary>
    /// help 페이지 오픈
    /// </summary>
    private void OnClickBtnHelp()
    {
        //TO DO: 라인측에 documentId를 받아 와야함
      //  ServiceSDK.ServiceSDKManager.instance.ShowLANBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardHelp, "", "");
    }

    /// <summary>
    /// Notice 페이지 오픈
    /// </summary>
    private void OnClickBtnNotice()
    {
        //TO DO: 라인측에 documentId를 받아 와야함
       // ServiceSDK.ServiceSDKManager.instance.ShowLANBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardNotice, "", "");
    }

    private void OnClickEventSpriteCheck(GameObject obj, bool bShow)
    {
        obj.SetActive(bShow);
    }
}
