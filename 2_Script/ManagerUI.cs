using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
//using Protocol;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;

public enum TypePopupDiary
{
    eNone,
    eMission,
    eStamp,
    eStorage,
    eCostume,
}

public enum TypeShowUI
{
    eTopUI,
    eBackUI,
    eAll,
}

public class ManagerUI : MonoBehaviour
{
    public static ManagerUI _instance = null;
    [System.NonSerialized]
    public Transform _transform = null;

    public Camera _camera;
    public GameObject _objTouchFlower;
    public GameObject anchorCenter;
    public UILobbyButtonListManager anchorTopLeft;
    public UILobbyButtonListManager anchorTopRight;
    public UILobbyButtonListManager anchorBottomLeft;
    public GameObject anchorBottomRight;
    public GameObject topCenterPanel;
    public GameObject buttonDiary;
    public GameObject diaryNewIcon;
    public GameObject diaryEventIcon;
    public UISprite[] diaryIcon;

    public GameObject saleIcon_clover;
    public GameObject saleIcon_coin;
    public GameObject saleIcon_jewel;
    public List<UIWidget> iphoneXWidget = new List<UIWidget>();
    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    //오른쪽에 있는 ui들.
    public GameObject[] anchorRightUI;

    public AnimationCurve popupScaleAnimation;
    public AnimationCurve popupAlphaAnimation;

    //플레이 버튼 UI.
    public UILabel[] _labelPlay;
    public UILabel[] _labelStage;
    public UISprite _rootStage;
    public GameObject _btnPlay;
    public GameObject _btnComingSoon;

    public GameObject _coinEventRoot;
    public UILabel _coinEventLabel;

    public UILabel[] _labelStageFlower;
    public UISprite _rootStageFlower;
    
    public GameObject _messageNewIcon;
    public UILabel _messageNewCount;

    // 상단 재화 관련
    public UISprite _CloverSprite;
    public UISprite _CloverInfinitySprite;
    public UISprite _StarSprite;
    public UILabel _labelClover;
    public UILabel _labelCloverS;

    public UILabel _labelCherry;
    public UILabel _labelJewer;
    public UILabel _labelStar;
    public UILabel _labelCloverTime;
    int tempClover = -1;
    int tempCherry = -1;
    int tempJewer = -1;
    int tempStar = -1;

    // 랭킹 등급
    public UIUrlTexture _textureClass;

    // prefab들 등록
    public GameObject _objPopupSystem;
    public GameObject _objPopupStage;
    public GameObject _objPopupFlowerInfo;
    public GameObject _objPopupRanking;
    public GameObject _objPopupUserProfile;
    public GameObject _objPopupPokoYuraInfo;
    public GameObject _objPopupComingSoon;
    public GameObject _objPopupPackage;
    public GameObject _objPopupStampEvent;
    public GameObject _objPopupNoticeHelp;
    public GameObject _objPopupOption;
    public GameObject _objPopupNotice;
    public GameObject _objPopupUserInfo;
    public GameObject _objPopupPushSetting;
    public GameObject _objPopupPlusInfo;
    public GameObject _objPopupHousing;
    public GameObject _objPopupDiary;
    public GameObject _objPopupMaterial;
    public GameObject _objPopupMailBox;
    public GameObject _objPopupBoxShop;
    public GameObject _objPopupBoxShopInfo;
    public GameObject _objPopupBoxShopInfoDetail;
    public GameObject _objPopupCloverShop;
    public GameObject _objPopupCoinShop;
    public GameObject _objPopupDiamondShop;
    public GameObject _objPopupMethod;
    public GameObject _objPopupRankUp;
    public GameObject _objPopupOpenGiftBox;
    public GameObject _objPopupTimeMission;
    public GameObject _objPopupTimeGiftBox;
    public GameObject _objPopupUserNameBox;
    public GameObject _objPopupInvite;
    public GameObject _objPopupInviteSmall;
    public GameObject _objPopupRequestClover;
    public GameObject _objPopupRequestSmall;
    public GameObject _objPopupSpecialEvent;
    public GameObject _objPopupReady;
    public GameObject _objPopupClear;
    public GameObject _objPopupFail;
    public GameObject _objPopupContinue;
    public GameObject _objPopupPause;
    public GameObject _objPopupNoMoreMove;
    public GameObject _objPopupStageTarget;
    public GameObject _objPopupStageReview;
    public GameObject _objDiaryMission;
    public GameObject _objDiaryMessenger;
    public GameObject _objDiaryStamp;
    public GameObject _objDiaryStorage;
    public GameObject _objPopupShare;
    public GameObject _objAreaIcon;
    public GameObject _objLobbyChat;
    public GameObject _objLobbyMission;
    public GameObject _objChat;
    public GameObject _objInGameTarget;
    public GameObject _objPopupConfirm;
    public GameObject _objPopupInputText;
    public GameObject _objPopupSendItemToSocial;
    public GameObject _ObjHeartEffect;
    public GameObject _objPopupInputColorPad;
    public GameObject _objHeartGetEffect;
    public GameObject _objPopupPanelEmotionIcon;
    public GameObject _objButtonMaterial;
    public GameObject _objButtonEvent;
    public GameObject _objButtonSpecialEvent;
    public GameObject _objButtonMissionStampEvent;
    public GameObject _objButtonPackage;
    public GameObject _objReviewScroll;


    #region 라이브 2D 모델 프리팹.
    public GameObject _objBoni;
    public GameObject _objCoco;
    public GameObject _objBlueBird;
    public GameObject _objPeng;
    public GameObject _objJeff;
    public GameObject _objZelly;
    public GameObject _objAroo;
    public GameObject _objAlphonse;
    public GameObject _objMai;
    public GameObject _objKiri;
    
    #endregion

    //상단, 하단 UI.
    public UIPanel topUiPanel;
    public UIPanel backUiPanel;

    //화면 사이즈 영역 구할때 사용.
    public UIRoot uiRoot;
    [HideInInspector]
    public Vector2 uiScreenSize = Vector2.zero;
    // 월드에 있는 재료를 획득할때 날아갈 타켓
    public Transform _transformDiary;
    /////////////////////////////////////////////////////////////////////////////////////////
    // 생선됨 모든 팝업이 생성되어 등록되고 지워짐
    public List<UIPopupBase> _popupList = new List<UIPopupBase>();

    [HideInInspector]
    public bool bTouchTopUI = true;
    
    private const int Default_Depth_High = 3;

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;
        _transform = transform;

        uiScreenSize = CalculateUISize(uiRoot.activeHeight * 0.5f);


        if (Application.loadedLevelName.Equals("Title"))
        {
            topUiPanel.gameObject.SetActive(false);
            backUiPanel.gameObject.SetActive(false);
        }

        {
            
         //   anchorTopLeft.transform.parent.transform.localPosition
        }
        }

	// Use this for initialization
	void Start ()
	{
        if ( Application.platform != RuntimePlatform.Android )
        {
            //// Debug.Log(" 아이폰X 대응 " + (float)Screen.height / (float)Screen.width);
            if ( ( float ) Screen.height / ( float ) Screen.width > 2f || ( float ) Screen.width / ( float ) Screen.height > 2f )
            {
                for ( int i = 0; i < iphoneXWidget.Count; i++ )
                {
                    iphoneXWidget[i].topAnchor.absolute = -120;
                    iphoneXWidget[i].UpdateAnchors();
                }

                //  Debug.Log(" 아이폰X 대응 " + iphoneXWidget.Count);
                for ( int i = 0; i < iphoneXWidgetY.Count; i++ )
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 60;
                    iphoneXWidgetY[i].UpdateAnchors();
                }
            }
        }
        

        //Invalidate;
	    UpdateUI();

    }

    void LateUpdate()
    {
        if( reservedUpdateUI)
        {
            if( DoUpdateUI() )
                reservedUpdateUI = false;
        }
    }

    bool recursiveLock = false;
    bool reservedUpdateUI = false;
    public void UpdateUI(bool updateImmidiate = false)
    {
        if( updateImmidiate )
        {
            if (!recursiveLock && !reservedUpdateUI)
            {
                recursiveLock = true;
                if (!DoUpdateUI())
                    reservedUpdateUI = true;
                recursiveLock = false;
            }
            else
            {
                reservedUpdateUI = true;
            }
        }
        else
        {
            reservedUpdateUI = true;
        }
    }
    
    private bool DoUpdateUI() //나중에 재화별로 빼기
    {
        if (Global._pendingReboot) {
            return false;
        }
        
       
        if (EditManager.instance != null) return false;
        //if (ManagerData._instance == null) return;

        if (tempClover != Global.clover)
        {
            tempClover = Global.clover;
            _labelClover.text = tempClover.ToString();
            _labelCloverS.text = _labelClover.text;
        }
        if (tempCherry != Global.coin)
        {
            tempCherry = Global.coin;
            _labelCherry.text = tempCherry.ToString();
        }
        if (tempJewer != Global.jewel)
        {
            tempJewer = Global.jewel;
            _labelJewer.text = tempJewer.ToString();
        }
        if (tempStar != Global.star)
        {
            tempStar = Global.star;
            _labelStar.text = tempStar.ToString();
        }

       
        /*
        int messeageCount = ServerRepos.MailCnt;
        if (messeageCount > 0)
        {
            _messageNewIcon.SetActive(true);
            if (messeageCount > 9)
            {
                _messageNewCount.text = "N";
            }
            else
            {
                _messageNewCount.text = messeageCount.ToString();
            }
        }
        else
        {
            _messageNewIcon.SetActive(false);
        }

        if (ServerRepos.LoginCdn != null)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if(ServerRepos.LoginCdn.EnableInvite == 0)
                SetAnchorRightUI_Ios();
#endif
            if(ManagerData._instance.userData.stage < ManagerData._instance.maxStageCount)
            {
                _btnPlay.SetActive(true);
                _btnComingSoon.SetActive(false);
            }
            else if (ManagerData._instance.userData.stage >= ManagerData._instance.maxStageCount)
            {
                int step = 0; // 0이면 꽃, 1이면 파란꽃  2이면 전체 파란꽃
                int flowerCount = 0;
                step = 2;
                for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                {
                    if (ManagerData._instance._stageData[i]._flowerLevel < 3)
                    {
                        step = 0;
                        break;
                    }
                }
                if (step == 2)
                {
                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                    {
                        if (ManagerData._instance._stageData[i]._flowerLevel < 4)
                        {
                            step = 1;
                            break;
                        }
                    }
                }

                if (step < 2)
                {
                    _btnPlay.SetActive(true);
                    _btnComingSoon.SetActive(false);

                    if (step == 0)
                        _rootStageFlower.spriteName = "icon_flower_stroke_blue";
                    else if (step == 1)
                        _rootStageFlower.spriteName = "icon_blueflower_stroke_blue";
                    _rootStageFlower.MakePixelPerfect();

                    for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
                    {
                        if (step == 0)
                        {
                            if (ManagerData._instance._stageData[i]._flowerLevel >= 3)
                                flowerCount++;
                        }
                        else if (step == 1)
                        {
                            if (ManagerData._instance._stageData[i]._flowerLevel >= 4)
                                flowerCount++;
                        }
                    }

                    for (int i = 0; i < _labelStageFlower.Length; i++)
                        _labelStageFlower[i].text = flowerCount + "/" + ManagerData._instance.maxStageCount;
                }
                else
                {
                    _btnPlay.SetActive(false);
                    _btnComingSoon.SetActive(true);
                }   
            }
            else
            {

            }
            PackageCheck();

            if(Global.coinEvent > 0)
            {
                _coinEventRoot.gameObject.SetActive(true);
                _coinEventLabel.text = "x" + ServerRepos.LoginCdn.CoinEventRatio;
                StartCoroutine(CoMoveCoinEventObject());
            }
            else
            {
                _coinEventRoot.gameObject.SetActive(false);
            }
        }
        */
        SaleInfoSetting();
        return true;
    }

    //랭킹 아래 포코유라 설정.
    public void SettingRankingPokoYura()
    {
       // if (ManagerData._instance.userData._profile == null || ManagerData._instance.userData == null)
            return;
    }
    
    public int GetPopupCount()
    {
        return _popupList.Count;
    }

    public void SettingDiaryButton()
    {
        diaryIcon[0].color = new Color(1f, 1f, 1f, 1f);
        diaryIcon[1].color = new Color(1f, 1f, 1f, 1f);
        diaryIcon[0].transform.localScale = Vector3.one;
    }
    /*
    void recvChargeClover(BaseResp code)
    {
        if (code.IsSuccess)
        {
            Global.clover = (int)GameData.Asset.AllClover;
            _labelCloverTime.text = Global.GetCloverTimeText(GameData.RemainChargeTime());
            _labelClover.text = ((int)GameData.Asset.AllClover).ToString();
            _labelCloverS.text = _labelClover.text;
            callChargeClover = false;

        }
    }   
    
    void PackageCheck()
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        //Debug.Log(" UIButtonPackage Remove All ");

        if (UIButtonPackage.packageList.Count > 0)
        {
            UIButtonPackage.RemoveAll();
        }
        //Debug.Log(ServerContents.Packages.Count);
        if (ServerContents.Packages.Count > 0 && GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            //ManagerUI._instance.anchorTopRight.LogObjListCount();           

            //전체 패키지 수 만큼 검사.
            var enumerator = ServerContents.Packages.GetEnumerator();
            while (enumerator.MoveNext())
            {
                bool checkPassed = true;
                bool buyBefore = false;
                var packageData = enumerator.Current.Value;
                long expireTs = packageData.expireTs;
                switch (packageData.type)
                {
                    case 0: //0 : (Speed Clear) 모든사용자에게 expire_ts에 설정된 기간까지 판매되는 상품 (스피드 클리어)
                        break;
                    case 1: //1 : (New User) User_infos의 created_at +display_day까지만 판매되는 패키지 / expire_ts 값 무시
                        {
                            System.DateTime date = ServerRepos.User.createdAt;
                            date = date.AddDays(packageData.display_day);
                            expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;
                        }
                        break;
                    case 2: //2 : (Stage Fail) 누적 스테이지 실패가 5회 이상인 사용자에게 1회만 노출되는 패키지 / display_day, expire_ts 값 무시
                        // 기본 UI상에는 표시될 일이 없음
                        {
                            checkPassed = false;
                        }
                        break;
                    case 3: //3 : (Deluxe) 초심자 패키지를 구입한 사용자에게만 패키지 적용일 + display_day 기간까지 판매되는 패키지 / expire_ts 값 무시
                        {
                            var buyRecord = ServerRepos.UserShopPackages.Find(x => { return x.idx == 1; });
                            if (buyRecord != null)
                            {
                                System.DateTime date = buyRecord.createdAt;
                                date = date.AddDays(packageData.display_day);
                                expireTs = (date.Ticks - new DateTime(1970, 1, 1).Ticks) / TimeSpan.TicksPerSecond;
                            }
                        }
                        break;
                    case 4: //4 : (Non Pay User) 결재정보가 없는 사용자에게만 expire_ts 기간까지 표시되는 패키지
                        if (ServerRepos.UserShopPackages.Count > 0)
                            checkPassed = false;
                        break;
                }

                if (!checkPassed)
                    continue;

                //시간이 지났다면 버튼 생성 안함.
                if (Global._instance != null && Global.LeftTime(expireTs) <= 0)
                    continue;

                //현재 유저가 구매한 패키지 수 만큼 검사.
                for (int i = 0; i < ServerRepos.UserShopPackages.Count; i++)
                {
                    if (ServerRepos.UserShopPackages[i].vsn == enumerator.Current.Value.vsn &&
                        ServerRepos.UserShopPackages[i].idx == enumerator.Current.Value.idx)
                    {
                        buyBefore = true;
                    }
                }
                //현재 패키지를 유저가 이미 구매한 상태라면 버튼 생성 안함.
                if (buyBefore == true)
                    continue;

                UIButtonPackage btnPackage = NGUITools.AddChild(anchorTopRight.gameObject, _objButtonPackage).GetComponent<UIButtonPackage>();
                anchorTopRight.AddLobbyButton(btnPackage.gameObject);
                btnPackage.SettingBtnPackage(enumerator.Current.Value, expireTs);
            }
            anchorTopRight.RefreshUI();
            //anchorTopRight.LogObjListCount();
        }
    }
    */
    bool callChargeClover = false;
	void Update () {

        /*
        // 클로버 타이머
        if (ManagerData._instance != null)
        {
            // UI가 활성화 되어있을때,, 클로버가 5개 이하일때
            if (ManagerData._instance._state == DataLoadState.eComplete && _labelCloverTime.gameObject.active)
            {
                if (GameData.RemainFreePlayTime() > 0)
                {
                    _labelCloverTime.text = Global.GetCloverFreeTimeText(GameData.RemainFreePlayTime());

                    if (!_CloverInfinitySprite.gameObject.active)
                    {
                        _CloverInfinitySprite.gameObject.SetActive(true);
                        _labelClover.gameObject.SetActive(false);
                        _labelCloverS.gameObject.SetActive(false);
                        _labelCloverTime.color = new Color(156f / 255f, 230f / 255f, 1f);
                    }

                    _CloverInfinitySprite.cachedTransform.localPosition = Vector3.up * Mathf.Cos(Time.time * 15f) * 1.5f;
                }
                else if (GameData.Asset.AllClover < 5)
                {
                    _labelCloverTime.text = Global.GetCloverTimeText(GameData.RemainChargeTime());
                    _labelClover.text = ((int)GameData.Asset.AllClover).ToString();
                    _labelCloverS.text = _labelClover.text;
                    //Debug.Log("GameData.RemainChargeTime " + GameData.RemainChargeTime() + "    ");


                    if (GameData.RemainChargeTime() <= 0 && !callChargeClover)
                    {
                        callChargeClover = true;
                        ServerAPI.ChargeClover(recvChargeClover);
                    }
                }
                else
                {
                    _labelCloverTime.text = "Full";

                }

                if (GameData.RemainFreePlayTime() > 0)
                {

                }
                else
                {
                    if (_CloverInfinitySprite.gameObject.active)
                    {
                        _CloverInfinitySprite.gameObject.SetActive(false);
                        _labelClover.gameObject.SetActive(true);
                        _labelCloverS.gameObject.SetActive(true);

                        _labelCloverTime.color = Color.white;
                    }
                }

            }
        }
        */

        // 터치 이펙트,실지 동작과 별도로 터치 할때마다 생성
        if (Global._touchBegin)
        {
            //인게임이고 팝업이 없을때 안나옴
            if (SceneManager.GetActiveScene().name == "InGame" && _popupList.Count == 0)
            {

            }
            else if (SceneManager.GetActiveScene().name == "InGameTool" && _popupList.Count == 0)
            {

            }
            else
            {
                GameObject obj = ManagerObjectPool.Spawn(_objTouchFlower, _transform);
                obj.transform.position = _camera.ScreenToWorldPoint(Global._touchPos);
                obj.transform.localScale = Vector3.one;
            }
        }
/*
        if (Input.GetKeyDown(KeyCode.M))
        {
            OpenPopupDiaryMission();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ClosePopUpUI();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            UILobbyChat lobbyChat = NGUITools.AddChild(gameObject, _objLobbyChat).GetComponent<UILobbyChat>();
            StartCoroutine(lobbyChat.ShowLobbyChat(Character._boni._transform, "おはよう!!", true));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            UIChat uiChat = NGUITools.AddChild(anchorCenter, _objChat).GetComponent<UIChat>();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            OpenPopupStageTarget();
        }*/
      
        //if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ManagerTutorial._instance != null)
                    return;

                if (UIPopupStageTarget.instance != null)
                    return;

                int nCount = ManagerUI._instance._popupList.Count;
                if (nCount > 0)
                {
                    if (ManagerUI._instance._popupList[nCount - 1].GetPopupType() == PopupType.housing)
                        return;

                    if (Global._instance == null) return;
                    if (UIButtonCanClickCheck() == false) return;
                    ManagerUI._instance._popupList[nCount-1].OnClickBtnBack();
                }
                else
                {
                    if (SceneLoading._sceneLoading) return;

                    if (SceneManager.GetActiveScene().name == "InGame")
                    {
                        if (SceneManager.GetActiveScene().name == "InGameTool") return;
                        if (GameItemManager.instance != null)
                        {
                            if (GameItemManager.instance.used == false)
                            {
                                GameItemManager.instance.BtnCancel();
                            }
                            return;
                        }
                        if (GameManager.instance.state == GameState.GAMECLEAR || GameManager.instance.state == GameState.GAMEOVER) return;

                        if (Input.touchCount > 0) return;

                        if (GameManager.instance != null && GameManager.instance.LoadScene) return;
                        
                        if (Global._instance == null) return;
                        if (UIButtonCanClickCheck() == false) return;
                        ManagerUI._instance.OpenPopupPause();
                    }
                    else
                    {
                        if (Global._instance == null) return;
                        if (UIButtonCanClickCheck() == false) return;
                        Global.InitClickTime();

                        UIPopupSystem popupSystem = OpenPopupSystem().GetComponent<UIPopupSystem>();
                        popupSystem.FunctionSetting(1, "OnClickExit", gameObject);
                        popupSystem.SetButtonText(1, "終了");
                        Texture2D texture = Resources.Load("Message/boniExit") as Texture2D;
                        popupSystem.InitSystemPopUp("ゲーム終了", "本当にポコパンタウンから\n離れるの？", true, texture);
                        popupSystem.TypeSetting(PopupType.exit);
                        return;
                    }
                }
            }
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {   
            if (Input.GetKeyDown(KeyCode.F1))
            {
             /*   Notice data = new Notice();
                data.noticeIndex = 1;
                ManagerUI._instance.OpenPopupNotice(data);*/
               // Global.ReBoot();
                OpenPopupRequestReview();
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                //ManagerUI._instance.OpenPopupRankUp(2,1);
                //ManagerUI._instance.OpenPopupShare();
                //ManagerUI._instance.OpenPopupRequestSmall();
                //ManagerUI._instance.OpenPopupNoticeHelp(ServerRepos.Notices[0]);
                //ManagerUI._instance.OpenPopupMissionStampEvent();
                //ManagerUI._instance.OpenPopupBoxShop();
                ManagerLobby._instance.SetCostume(2);
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                ManagerUI._instance.OpenPopupComingSoon();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                //ManagerUI._instance.OpenPopupPackage();
                //    ManagerUI._instance.OpenPopupReadyEventStage();
                UIPopupSystem popupSystem = OpenPopupSystem().GetComponent<UIPopupSystem>();
                popupSystem.SetButtonText(1, Global._instance.GetString("btn_18"));
                popupSystem.HideCloseButton();
                Texture2D texture = Resources.Load("Message/boniExit") as Texture2D;
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_7"), Global._instance.GetString("n_s_5"), true, texture);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyMission);
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialDiaryMission);
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialQuestComplete);
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGetPlusHousing);
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGetMaterial);
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialReadyItem);
            }
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                ManagerTutorial.PlayTutorial(TutorialType.TutorialGetSticker);
            }
        }
    }

    public void CoShowUI(float time, bool bShow, TypeShowUI uiType)
    {
        if (uiType == TypeShowUI.eTopUI)
        {
            if (bShow == true)
            {
                topUiPanel.gameObject.SetActive(true);
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 1, time);
            }
            else
            {
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 0, time)
                    .OnComplete(()=>topUiPanel.gameObject.SetActive(false));
            }
        }

        else if (uiType == TypeShowUI.eBackUI)
        {
            if (bShow == true)
            {
                backUiPanel.gameObject.SetActive(true);
                // 코인 이벤트 진행 중이면, 코인 움직임 다시 재생.
                if (Global.coinEvent > 0)
                {
                    StartCoroutine(CoMoveCoinEventObject());
                }
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 1, time);
            }
            else
            {
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 0, time)
                    .OnComplete(() => backUiPanel.gameObject.SetActive(false));
            }
        }

        else
        {
            if (bShow == true)
            {
                topUiPanel.gameObject.SetActive(true);
                backUiPanel.gameObject.SetActive(true);
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 1, time);
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 1, time);
                // 코인 이벤트 진행 중이면, 코인 움직임 다시 재생.
                if (Global.coinEvent > 0)
                {
                    StartCoroutine(CoMoveCoinEventObject());
                }
            }
            else
            {
                DOTween.To(() => topUiPanel.alpha, x => topUiPanel.alpha = x, 0, time)
                   .OnComplete(() => topUiPanel.gameObject.SetActive(false));
                DOTween.To(() => backUiPanel.alpha, x => backUiPanel.alpha = x, 0, time)
                    .OnComplete(() => backUiPanel.gameObject.SetActive(false));
            }
        }
    }

    public void AnchorTopRightDestroy()
    {
        anchorTopRight.DestroyAllButtons();
    }

    public void PanelAlphaTween(UIPanel panel, float alpha, float time, TweenCallback action)
    {
        DOTween.To(() => panel.alpha, x => panel.alpha = x, alpha, time).OnComplete(action);
    }

    public void OpenPopupChat(ChatGameData in_data, Method.FunctionVoid in_callback)
    {
        UIChat uiChat = NGUITools.AddChild(anchorCenter, _objChat).GetComponent<UIChat>();
        uiChat._chatGameData = in_data;
        uiChat._callbackEnd = in_callback;
    }

    public void OpenPopupTimeMission(MissionData mData)
    {
        UIPopupTimeMission popupTimeMission = NGUITools.AddChild(anchorCenter, _objPopupTimeMission).GetComponent<UIPopupTimeMission>();
        popupTimeMission.OpenPopUp(GetPanelDepth(popupTimeMission.bShowTopUI, popupTimeMission.panelCount));
        popupTimeMission.SettingSortOrder(GetSortLayer(popupTimeMission.sortOrderCount));
        popupTimeMission.InitPopUp(mData);
        _popupList.Add(popupTimeMission);
    }
    /*
    public void OpenPopupTimeGiftBox(ServerUserGiftBox qData)
    {
        if (UIButtonCanClickCheck() == false)
            return;
        UIPopupTimeGiftBox popupTimeGiftBox = NGUITools.AddChild(anchorCenter, _objPopupTimeGiftBox).GetComponent<UIPopupTimeGiftBox>();
        popupTimeGiftBox.OpenPopUp(GetPanelDepth(popupTimeGiftBox.bShowTopUI, popupTimeGiftBox.panelCount));
        popupTimeGiftBox.SettingSortOrder(GetSortLayer(popupTimeGiftBox.sortOrderCount));
        popupTimeGiftBox.InitPopUp(qData);
        _popupList.Add(popupTimeGiftBox);
    }
    */
    public void OpenPopupUserNameBox(bool bChat)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupUserNameBox popupUserNameBox = NGUITools.AddChild(anchorCenter, _objPopupUserNameBox).GetComponent<UIPopupUserNameBox>();
        popupUserNameBox.OpenPopUp(GetPanelDepth(popupUserNameBox.bShowTopUI, popupUserNameBox.panelCount));
        popupUserNameBox.SettingSortOrder(GetSortLayer(popupUserNameBox.sortOrderCount));
        popupUserNameBox.InitPopUp(bChat);
        _popupList.Add(popupUserNameBox);
    }

    public void OpenPopupInvite()
    {

    }
    /*
    void recvInvitedFriends(InvitedFriendsResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Invited Friends Count : " + resp.invitedFriends.Count);

            for (int i = 0; i < resp.invitedFriends.Count; i++)
            {
                UserData data = null;
                if (ManagerData._instance._friendsData.TryGetValue(resp.invitedFriends[i].fUserKey, out data))
                    data.inviteTime = resp.invitedFriends[i].expiredAt;
            }

            UIPopupInvite popupInvite = NGUITools.AddChild(anchorCenter, _objPopupInvite).GetComponent<UIPopupInvite>();
            popupInvite.OpenPopUp(GetPanelDepth(popupInvite.bShowTopUI, popupInvite.panelCount));
            popupInvite.SettingSortOrder(GetSortLayer(popupInvite.sortOrderCount));
            popupInvite.InitPopUpInvite(resp.invitedFriends);
            _popupList.Add(popupInvite);
        }
        else
        {
            ManagerUI._instance.bTouchTopUI = true;
        }
    }
    */
    public void OpenPopupInviteSmall()
    {
        int friendCount = ManagerData._instance._inviteFriendsData.Count;
        if (friendCount < 6)
            return;
       // ServerAPI.InvitedFriends(recvInvitedFriendsSmall);
    }
    /*
    void recvInvitedFriendsSmall(InvitedFriendsResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Invited Friends Count : " + resp.invitedFriends.Count);

            //최대 초대 수나 하루 최대 초대 수를 넘었으면 return.
            if (resp.invitedFriends.Count > 994 || ServerRepos.InviteDayCnt > 44)
            {
                return;
            }

            for (int i = 0; i < resp.invitedFriends.Count; i++)
            {
                UserData data = null;
                if (ManagerData._instance._inviteFriendsData.TryGetValue(resp.invitedFriends[i].fUserKey, out data))
                    data.inviteTime = resp.invitedFriends[i].expiredAt;
            }

            //친구 리스트 검사.
            List<UserData> invitePossible = new List<UserData>();
            var enumerator = ManagerData._instance._inviteFriendsData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UserData uData = enumerator.Current.Value;
                //초대 가능한 친구들만 넣음.
                if (uData.inviteTime != 0)
                    continue;
                invitePossible.Add(uData);
            }

            //초대가능한 친구 수가 안되면 반환.
            int inviteCount = invitePossible.Count;
            if (inviteCount < 6)
                return;
            
            UIPopupInviteSmall popupInviteSmall = NGUITools.AddChild(anchorCenter, _objPopupInviteSmall).GetComponent<UIPopupInviteSmall>();
            popupInviteSmall.OpenPopUp(GetPanelDepth(popupInviteSmall.bShowTopUI, popupInviteSmall.panelCount));
            popupInviteSmall.SettingSortOrder(GetSortLayer(popupInviteSmall.sortOrderCount));
            popupInviteSmall.InitPopUp(invitePossible);
            _popupList.Add(popupInviteSmall);
        }
    }
    */
    public void OpenPopupRequestClover()
    {
      //  ServerAPI.RequestedCloverList(recvRequestedCloverList);
    }
    /*
    void recvRequestedCloverList(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Requested Clover List Count :" + resp.cloverCoolTime.Count);

            foreach (var item in resp.cloverCoolTime)
            {
                UserData data = null;
                if (ManagerData._instance._friendsData.TryGetValue(item.fUserKey, out data))
                    data.cloverRequestCoolTime = item.reqCoolTime;
            }

            UIPopupRequestClover popupRequestClover = NGUITools.AddChild(anchorCenter, _objPopupRequestClover).GetComponent<UIPopupRequestClover>();
            popupRequestClover.OpenPopUp(GetPanelDepth(popupRequestClover.bShowTopUI, popupRequestClover.panelCount));
            popupRequestClover.SettingSortOrder(GetSortLayer(popupRequestClover.sortOrderCount));
            popupRequestClover.InitPopUpRequestClover(resp.cloverCoolTime.Count);
            _popupList.Add(popupRequestClover);
        }
        else
        {
            ManagerUI._instance.bTouchTopUI = true;
        }
    }
    */
    public void OpenPopupRequestSmall()
    {
        int friendCount = ManagerData._instance._friendsData.Count;
        if (friendCount < 6)
            return;

        _OpenPopupRequestSmallWorkEnd = 0;
       // ServerAPI.RequestedCloverList(recvRequestedCloverListSmall);
    }
    [System.NonSerialized]
    public int _OpenPopupRequestSmallWorkEnd = 0;
   /* void recvRequestedCloverListSmall(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            int nCount = resp.cloverCoolTime.Count;
            //Debug.Log("** Requested Clover List Count : " + nCount + " 명");

            //쿨타임 데이터 들고와서 세팅.
            for (int i = 0; i < nCount; i++)
            {
                UserData data = null;
                if (ManagerData._instance._friendsData.TryGetValue(resp.cloverCoolTime[i].fUserKey, out data))
                    data.cloverRequestCoolTime = resp.cloverCoolTime[i].reqCoolTime;
            }

            //친구 리스트 검사.
            List<UserData> requestPossible = new List<UserData>();
            var enumerator = ManagerData._instance._friendsData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UserData uData = enumerator.Current.Value;
                //초대 가능한 친구들만 넣음.
                if (uData.cloverRequestCoolTime != 0)
                    continue;
                requestPossible.Add(uData);
            }

            //초대가능한 친구 수가 안되면 반환.
            int friendCount = requestPossible.Count;
            if (friendCount < 6)
            {
                _OpenPopupRequestSmallWorkEnd = -1;
                return;
            }

            UIPopupRequestCloverSmall popupRequestSmall = NGUITools.AddChild(anchorCenter, _objPopupRequestSmall).GetComponent<UIPopupRequestCloverSmall>();
            popupRequestSmall.OpenPopUp(GetPanelDepth(popupRequestSmall.bShowTopUI, popupRequestSmall.panelCount));
            popupRequestSmall.SettingSortOrder(GetSortLayer(popupRequestSmall.sortOrderCount));
            popupRequestSmall.InitPopUp(requestPossible);
            _popupList.Add(popupRequestSmall);

            _OpenPopupRequestSmallWorkEnd = 1;
        }else
            _OpenPopupRequestSmallWorkEnd = -1;
    }
    */
    public void OpenPopupSpecialEvent(int index, bool notifyGetReward, Method.FunctionVoid callBackEnd = null)
    {
        UIPopupSpecialEvent popupSpecialEvent = NGUITools.AddChild(anchorCenter, _objPopupSpecialEvent).GetComponent<UIPopupSpecialEvent>();
        popupSpecialEvent.OpenPopUp(GetPanelDepth(popupSpecialEvent.bShowTopUI, popupSpecialEvent.panelCount));
        popupSpecialEvent.SettingSortOrder(GetSortLayer(popupSpecialEvent.sortOrderCount));
        popupSpecialEvent.InitPopUp(index, notifyGetReward);
        _popupList.Add(popupSpecialEvent);
    }

    public void OpenPopupDiary(TypePopupDiary in_type)
    {
        UIPopupDiary popupDiary = NGUITools.AddChild(anchorCenter, _objPopupDiary).GetComponent<UIPopupDiary>();
        popupDiary.OpenPopUp(GetPanelDepth(popupDiary.bShowTopUI, popupDiary.panelCount));
        popupDiary.SettingSortOrder(GetSortLayer(popupDiary.sortOrderCount));
        popupDiary.Init(in_type);
        _popupList.Add(popupDiary);
    }

    public void OpenPopupDiaryMission()
    {
        OpenPopupDiary(TypePopupDiary.eMission);
    }

    public void OpenPopupLobbyMission(MissionData in_data,Vector3 in_positoin)
    {
        UILobbyMission popup = NGUITools.AddChild(anchorCenter, _objLobbyMission).GetComponent<UILobbyMission>();
        popup.InitLobbyMission(in_data, in_positoin);
    }
    [System.NonSerialized]
    public bool _OpenPopupRankingWorkEnd = false;
    public void OpenPopupRaking()
    {

    }
    public void OpenPopupRaking_BySystem()
    {
        UIPopupRanking popupRanking = NGUITools.AddChild(anchorCenter, _objPopupRanking).GetComponent<UIPopupRanking>();
        popupRanking.OpenPopUp(GetPanelDepth(popupRanking.bShowTopUI, popupRanking.panelCount));
        popupRanking.SettingSortOrder(GetSortLayer(popupRanking.sortOrderCount));
        _popupList.Add(popupRanking);
        _OpenPopupRankingWorkEnd = true;
    }

    public void OpenPopupPokoYuraInfo()
    {
        UIPopupPokoYuraInfo popupPokoYuraInfo = NGUITools.AddChild(anchorCenter, _objPopupPokoYuraInfo).GetComponent<UIPopupPokoYuraInfo>();
        popupPokoYuraInfo.OpenPopUp(GetPanelDepth(popupPokoYuraInfo.bShowTopUI, popupPokoYuraInfo.panelCount));
        popupPokoYuraInfo.SettingSortOrder(GetSortLayer(popupPokoYuraInfo.sortOrderCount));
        _popupList.Add(popupPokoYuraInfo);
    }

    public void OpenPopupMailBox()
    {
       // ServerAPI.Inbox(recvMessageBox);
    }
    /*
    void recvMessageBox(UserInboxResp resp)
    {
        if (resp.IsSuccess)
        {
            UIPopupMailBox popupMailBox = NGUITools.AddChild(gameObject, _objPopupMailBox).GetComponent<UIPopupMailBox>();
            popupMailBox.OpenPopUp(GetPanelDepth(popupMailBox.bShowTopUI, popupMailBox.panelCount));
            popupMailBox.SettingSortOrder(GetSortLayer(popupMailBox.sortOrderCount));
            popupMailBox.SetMailBox();
            _popupList.Add(popupMailBox);
        }
    }
    */
    public void OpenPopupComingSoon()
    {
        if (ManagerData._instance._stageData[0]._flowerLevel == 0)
        {
           // recvGameList_popType = 1;
            //ServerAPI.GetUserStageList(recvGameList);
        }
        else
        {
            UIPopupComingSoon popupComingSoon = NGUITools.AddChild(anchorCenter, _objPopupComingSoon).GetComponent<UIPopupComingSoon>();
            popupComingSoon.OpenPopUp(GetPanelDepth(popupComingSoon.bShowTopUI, popupComingSoon.panelCount));
            popupComingSoon.SettingSortOrder(GetSortLayer(popupComingSoon.sortOrderCount));
            popupComingSoon.InitPopUp();
            _popupList.Add(popupComingSoon);
        }
    }
    /*
    public void OpenPopupPackage(CdnShopPackage packageData, Method.FunctionVoid func = null)
    {
        UIPopupPackage popupPackage = NGUITools.AddChild(anchorCenter, _objPopupPackage).GetComponent<UIPopupPackage>();
        popupPackage.OpenPopUp(GetPanelDepth(popupPackage.bShowTopUI, popupPackage.panelCount));
        popupPackage.SettingSortOrder(GetSortLayer(popupPackage.sortOrderCount));
        popupPackage.InitPopUp(packageData, func);
        _popupList.Add(popupPackage);
    }
    */
    public void OpenPopupMissionStampEvent(int eventIndex)
    {
        UIPopupMissionStampEvent stampEvent = NGUITools.AddChild(anchorCenter, _objPopupStampEvent).GetComponent<UIPopupMissionStampEvent>();
        stampEvent.eventIndex = eventIndex;
        stampEvent.OpenPopUp(GetPanelDepth(stampEvent.bShowTopUI, stampEvent.panelCount));
        stampEvent.SettingSortOrder(GetSortLayer(stampEvent.sortOrderCount));
        _popupList.Add(stampEvent);
    }
    /*
    public void OpenPopupNoticeHelp(Notice noticeIndex)
    {
        UIPopupNoticeHelp popupNoticeHelp = NGUITools.AddChild(anchorCenter, _objPopupNoticeHelp).GetComponent<UIPopupNoticeHelp>();
        popupNoticeHelp.OpenPopUp(GetPanelDepth(popupNoticeHelp.bShowTopUI, popupNoticeHelp.panelCount));
        popupNoticeHelp.SettingSortOrder(GetSortLayer(popupNoticeHelp.sortOrderCount));
        popupNoticeHelp.InitPopUp(noticeIndex);
        _popupList.Add(popupNoticeHelp);
    }
    */
    public void OpenPopupOption()
    {
        UIPopupOption popupOption = NGUITools.AddChild(anchorCenter, _objPopupOption).GetComponent<UIPopupOption>();
        popupOption.OpenPopUp(GetPanelDepth(popupOption.bShowTopUI, popupOption.panelCount));
        popupOption.SettingSortOrder(GetSortLayer(popupOption.sortOrderCount));
        _popupList.Add(popupOption);
    }
    /*
    public void OpenPopupNotice(Notice noticeIndex)
    {
        UIPopupNotice popupNotice = NGUITools.AddChild(anchorCenter, _objPopupNotice).GetComponent<UIPopupNotice>();
        popupNotice.OpenPopUp(GetPanelDepth(popupNotice.bShowTopUI, popupNotice.panelCount));
        popupNotice.SettingSortOrder(GetSortLayer(popupNotice.sortOrderCount));
        popupNotice.InitPopUp(noticeIndex);
        _popupList.Add(popupNotice);
    }
    public void OpenPopupNoticeInfo(Notice noticeIndex)
    {
        UIPopupNotice popupNotice = NGUITools.AddChild(anchorCenter, _objPopupNotice).GetComponent<UIPopupNotice>();
        popupNotice.OpenPopUp(GetPanelDepth(popupNotice.bShowTopUI, popupNotice.panelCount));
        popupNotice.SettingSortOrder(GetSortLayer(popupNotice.sortOrderCount));
        popupNotice.InitPopUpInfo(noticeIndex);
        _popupList.Add(popupNotice);
    }
    */
    public void OpenPopupUserInfo()
    {
        UIPopUpUserInfo popupUserInfo = NGUITools.AddChild(anchorCenter, _objPopupUserInfo).GetComponent<UIPopUpUserInfo>();
        popupUserInfo.OpenPopUp(GetPanelDepth(popupUserInfo.bShowTopUI, popupUserInfo.panelCount));
        popupUserInfo.SettingSortOrder(GetSortLayer(popupUserInfo.sortOrderCount));
        _popupList.Add(popupUserInfo);
    }

    public void OpenPopupPushSetting()
    {
        UIPopUpPushSetting popupPushSetting = NGUITools.AddChild(anchorCenter, _objPopupPushSetting).GetComponent<UIPopUpPushSetting>();
        popupPushSetting.OpenPopUp(GetPanelDepth(popupPushSetting.bShowTopUI, popupPushSetting.panelCount));
        popupPushSetting.SettingSortOrder(GetSortLayer(popupPushSetting.sortOrderCount));
        _popupList.Add(popupPushSetting);
    }
    /*
    public void OpenPopupPlusInfo(List<ServerImageLink> links)
    {
        UIPopupPlusInfo popupPlusInfo = NGUITools.AddChild(anchorCenter, _objPopupPlusInfo).GetComponent<UIPopupPlusInfo>();
        popupPlusInfo.OpenPopUp(GetPanelDepth(popupPlusInfo.bShowTopUI, popupPlusInfo.panelCount));
        popupPlusInfo.SettingSortOrder(GetSortLayer(popupPlusInfo.sortOrderCount));
        popupPlusInfo.InitPopUp(links);
        _popupList.Add(popupPlusInfo);
    }
    */
    public GameObject OpenPopupSystem(bool bShowTopUI = false)
    {
        UIPopupSystem popupSystem = NGUITools.AddChild(anchorCenter, _objPopupSystem).GetComponent<UIPopupSystem>();
        popupSystem.bShowTopUI = bShowTopUI;
        popupSystem.OpenPopUp(GetPanelDepth(popupSystem.bShowTopUI, popupSystem.panelCount));
        popupSystem.SettingSortOrder(GetSortLayer(popupSystem.sortOrderCount));
        _popupList.Add(popupSystem);
        return popupSystem.gameObject;
    }

    public void OpenPopupHousing(ActionObjectHousing Obj, int selectIndex, bool bSelectAction, Method.FunctionVoid in_callback = null)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupHousing popupHousing = NGUITools.AddChild(anchorCenter, _objPopupHousing).GetComponent<UIPopupHousing>();
        popupHousing.OpenPopUp(GetPanelDepth(popupHousing.bShowTopUI, popupHousing.panelCount));
        popupHousing.SettingSortOrder(GetSortLayer(popupHousing.sortOrderCount));
        popupHousing.InitHousing(Obj, selectIndex, bSelectAction, in_callback);
        _popupList.Add(popupHousing);
    }

    public void OpenPopupBoxShop()
    {
        UIPopupBoxShop popupBoxShop = NGUITools.AddChild(gameObject, _objPopupBoxShop).GetComponent<UIPopupBoxShop>();
        popupBoxShop.OpenPopUp(GetPanelDepth(popupBoxShop.bShowTopUI, popupBoxShop.panelCount));
        popupBoxShop.SettingSortOrder(GetSortLayer(popupBoxShop.sortOrderCount));
        _popupList.Add(popupBoxShop);
    }

    public void OpenPopupBoxShopInfo()
    {
        UIPopupBoxShopInfo popupBoxShopInfo = NGUITools.AddChild(gameObject, _objPopupBoxShopInfo).GetComponent<UIPopupBoxShopInfo>();
        popupBoxShopInfo.OpenPopUp(GetPanelDepth(popupBoxShopInfo.bShowTopUI, popupBoxShopInfo.panelCount));
        popupBoxShopInfo.SettingSortOrder(GetSortLayer(popupBoxShopInfo.sortOrderCount));
        _popupList.Add(popupBoxShopInfo);
    }

    public void OpenPopupBoxShopInfoDetail()
    {
        var popupBoxShopInfo = NGUITools.AddChild(gameObject, _objPopupBoxShopInfoDetail).GetComponent<UIPopupBoxShopInfoDetail>();
        popupBoxShopInfo.OpenPopUp(GetPanelDepth(popupBoxShopInfo.bShowTopUI, popupBoxShopInfo.panelCount));
        popupBoxShopInfo.SettingSortOrder(GetSortLayer(popupBoxShopInfo.sortOrderCount));
        _popupList.Add(popupBoxShopInfo);
    }

    public void OpenPopupCloverShop()
    {
        UIPopupCloverShop popupCloverShop = NGUITools.AddChild(gameObject, _objPopupCloverShop).GetComponent<UIPopupCloverShop>();
        popupCloverShop.OpenPopUp(GetPanelDepth(popupCloverShop.bShowTopUI, popupCloverShop.panelCount));
        popupCloverShop.SettingSortOrder(GetSortLayer(popupCloverShop.sortOrderCount));
        _popupList.Add(popupCloverShop);
    }

    public void OpenPopupCoinShop()
    {
        UIPopupCoinShop popupCoinShop = NGUITools.AddChild(gameObject, _objPopupCoinShop).GetComponent<UIPopupCoinShop>();
        popupCoinShop.OpenPopUp(GetPanelDepth(popupCoinShop.bShowTopUI, popupCoinShop.panelCount));
        popupCoinShop.SettingSortOrder(GetSortLayer(popupCoinShop.sortOrderCount));
        _popupList.Add(popupCoinShop);
    }

    public void OpenPopupDiamondShop()
    {
        UIPopupDiamondShop popupDiamondShop = NGUITools.AddChild(gameObject, _objPopupDiamondShop).GetComponent<UIPopupDiamondShop>();
        popupDiamondShop.OpenPopUp(GetPanelDepth(popupDiamondShop.bShowTopUI, popupDiamondShop.panelCount));
        popupDiamondShop.SettingSortOrder(GetSortLayer(popupDiamondShop.sortOrderCount));
        _popupList.Add(popupDiamondShop);
    }

    public void OpenPopupMethod()
    {
        UIPopUpMethod popupMethod = NGUITools.AddChild(gameObject, _objPopupMethod).GetComponent<UIPopUpMethod>();
        popupMethod.OpenPopUp(GetPanelDepth(popupMethod.bShowTopUI, popupMethod.panelCount));
        popupMethod.SettingSortOrder(GetSortLayer(popupMethod.sortOrderCount));
        _popupList.Add(popupMethod);
    }
    public void OpenPopupRankUp(int classNum,int pokoyura)
    {
        UIPopupRankUp popupRankUp = NGUITools.AddChild(gameObject, _objPopupRankUp).GetComponent<UIPopupRankUp>();
        popupRankUp.OpenPopUp(GetPanelDepth(popupRankUp.bShowTopUI, popupRankUp.panelCount));
        popupRankUp.SettingSortOrder(GetSortLayer(popupRankUp.sortOrderCount));
        popupRankUp.InitRankUpPopup(classNum, pokoyura);
        _popupList.Add(popupRankUp);
    }

    public UIPopUpOpenGiftBox OpenPopupGiftBox()
    {
     //   ServerAPI.OpenGiftBox(1,recvOpenGiftBox);

        UIPopUpOpenGiftBox popupOpenGiftBox = NGUITools.AddChild(gameObject, _objPopupOpenGiftBox).GetComponent<UIPopUpOpenGiftBox>();
        popupOpenGiftBox.OpenPopUp(GetPanelDepth(popupOpenGiftBox.bShowTopUI, popupOpenGiftBox.panelCount));
        popupOpenGiftBox.SettingSortOrder(GetSortLayer(popupOpenGiftBox.sortOrderCount));
        _popupList.Add(popupOpenGiftBox);

        return popupOpenGiftBox;
    }/*
    void recvOpenGiftBox(OpenGiftBoxResp resp)
    {
        if (resp.IsSuccess)
        {

        }
    }*/
    public void OpenPopupMaterial()
    {
        UIPopupMaterial popupMaterial = NGUITools.AddChild(gameObject, _objPopupMaterial).GetComponent<UIPopupMaterial>();
        popupMaterial.OpenPopUp(GetPanelDepth(popupMaterial.bShowTopUI, popupMaterial.panelCount));
        popupMaterial.SettingSortOrder(GetSortLayer(popupMaterial.sortOrderCount));
        _popupList.Add(popupMaterial);
    }

    public void OpenPopupStage()
    {
        OpenPopupStageAction();
    }

    public void OpenPopupStageAction(bool bChangeAction = false)
    {
        if (CheckExitUI() == true)
            return;

        // 스테이지 정보가 비어있으면 받아서 창열기
        if (ManagerData._instance._stageData[0]._flowerLevel == 0)
        { }// ServerAPI.GetUserStageList(recvGameList);
        else
        {
            UIPopupStage popupStage = NGUITools.AddChild(anchorCenter, _objPopupStage).GetComponent<UIPopupStage>();
            popupStage.transform.localScale = Vector3.one;
            popupStage.OpenPopUp(GetPanelDepth(popupStage.bShowTopUI, popupStage.panelCount));
            popupStage.InitStagePopUp(bChangeAction);
            popupStage.SettingSortOrder(GetSortLayer(popupStage.sortOrderCount));
            _popupList.Add(popupStage);
        }
    }
    /*
    int recvGameList_popType = 0;
    void recvGameList(BaseResp resp)
    {
        if (resp.IsSuccess)
        {

            foreach (var item in ServerRepos.UserStages)
            {
                //Debug.Log(item.stage + "___play" + item.play + "  score" + item.score + "  score" + item.score);

                StageData stage = ManagerData._instance._stageData[item.stage - 1];
                stage._continue = item.continue_;
                stage._play = item.play;
                stage._fail = item.fail;
                stage._score = item.score;
                stage._missionProg1 = item.mprog1;
                stage._missionProg2 = item.mprog2;
                stage._flowerLevel = item.flowerLevel;
                stage._missionClear = item.missionClear;
            }

            if (recvGameList_popType == 1)
            {
                UIPopupComingSoon popupComingSoon = NGUITools.AddChild(anchorCenter, _objPopupComingSoon).GetComponent<UIPopupComingSoon>();
                popupComingSoon.OpenPopUp(GetPanelDepth(popupComingSoon.bShowTopUI, popupComingSoon.panelCount));
                popupComingSoon.SettingSortOrder(GetSortLayer(popupComingSoon.sortOrderCount));
                popupComingSoon.InitPopUp();
                _popupList.Add(popupComingSoon);
            }
            else
            {
                UIPopupStage popupStage = NGUITools.AddChild(anchorCenter, _objPopupStage).GetComponent<UIPopupStage>();
                popupStage.transform.localScale = Vector3.one;
                popupStage.OpenPopUp(GetPanelDepth(popupStage.bShowTopUI, popupStage.panelCount));
                popupStage.InitStagePopUp(false);
                popupStage.SettingSortOrder(GetSortLayer(popupStage.sortOrderCount));
                _popupList.Add(popupStage);
            }
        }
        recvGameList_popType = 0;
    }
    */
    public void OpenPopupFlowerInfo()
    {
        UIPopupFlowerInfo popupFlowerInfo = NGUITools.AddChild(anchorCenter, _objPopupFlowerInfo).GetComponent<UIPopupFlowerInfo>();
        popupFlowerInfo.OpenPopUp(GetPanelDepth(popupFlowerInfo.bShowTopUI, popupFlowerInfo.panelCount));
        popupFlowerInfo.SettingSortOrder(GetSortLayer(popupFlowerInfo.sortOrderCount));
        _popupList.Add(popupFlowerInfo);
    }

    bool IsOpenReady = false;
    public void OpenPopupReadyLastStage()
    {
        if (ManagerUI._instance._rootStageFlower.gameObject.activeInHierarchy == true)
        {
            ManagerUI._instance.OpenPopupComingSoon();
        }
        else
        {
            int pCount = _popupList.Count;
            if (pCount > 0 && _popupList[pCount - 1].name == "UIPopUpReady")
                return;

            if (IsOpenReady)
                return;
            IsOpenReady = true;
            NetworkLoading.MakeNetworkLoading(0.5f);
/*
            Global.stageIndex = (int)ManagerData._instance.userData.stage;
            Global.eventIndex = 0;
*/
            StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady());
        }
    }

    public void OpenPopupReadyStageCallBack(Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart, callBackClose));
    }


    public void OpenPopupReadyStageEvent(Method.FunctionVoid callBackStart = null)
    {
        if (IsOpenReady)
            return;
        IsOpenReady = true;
        NetworkLoading.MakeNetworkLoading(0.5f);

        StartCoroutine(CoCheckStageDataBeforeOpenPopUpReady(callBackStart));
    }

    public void MakeMaterialIcon(int mCount)
    {
        if (UIButtonMaterial._instance == null)
        {
            NGUITools.AddChild(anchorBottomLeft.gameObject, _objButtonMaterial);
            anchorBottomLeft.AddLobbyButton(UIButtonMaterial._instance.gameObject);
            UIButtonMaterial._instance.FirstReset(mCount);
        }
        else
            UIButtonMaterial._instance.Reset();
        //StartCoroutine(UIButtonMaterial._instance.CoSetMaterialIcon(mCount));
    }

    public void MakeBoxShopIcon(int npcIndex)
    {
        if (anchorTopLeft.CheckBoxShopButton() == false)
            return;

        var btnBoxShop = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIButtonBoxShop", anchorTopLeft.gameObject).GetComponent<UIButtonBoxShop>();
        btnBoxShop.SetButtonEvent(npcIndex);
        anchorTopLeft.AddLobbyButton(btnBoxShop.gameObject);
    }

    public void MakeEventIcon(int index)
    {
        if (anchorTopRight.CheckEventButton(index) == false)
            return;
        UIButtonEvent btnEvent= NGUITools.AddChild(anchorTopRight.gameObject, _objButtonEvent).GetComponent<UIButtonEvent>();
        btnEvent.SetButtonEvent(index);
        anchorTopRight.AddLobbyButton(btnEvent.gameObject);
    }

    public void MakeSpecialEventIcon(int index)
    {
        if (anchorTopRight.CheckSpecialEventButton(index) == false)
            return;
        UIButtonSpecialEvent btnEvent = NGUITools.AddChild(anchorTopRight.gameObject, _objButtonSpecialEvent).GetComponent<UIButtonSpecialEvent>();
        btnEvent.SetButtonEvent(index);
        anchorTopRight.AddLobbyButton(btnEvent.gameObject);
    }

    public void MakeMissionStampEventIcon(int eventIndex, bool eventState)
    {
        if (anchorTopRight.CheckMissionStampEventButton() == false)
            return;

        var btnEvent = NGUITools.AddChild(anchorTopRight.gameObject, this._objButtonMissionStampEvent).GetComponent<UIButtonMissionStampEvent>();
        btnEvent.SetButtonEvent(eventIndex);
        btnEvent.SetButtonState(false);
        anchorTopRight.AddLobbyButton(btnEvent.gameObject);

        StartCoroutine(CoStampMissionChecker(btnEvent));
    }

    IEnumerator CoStampMissionChecker(UIButtonMissionStampEvent btn)
    {
        while (!btn.CheckCompleted())
        {
            yield return new WaitForSeconds(0.5f);
        }

        btn.SetButtonState(true);
        yield break;
    }

    IEnumerator CoCheckStageDataBeforeOpenPopUpReady(Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null)
    {
        //Cdn에서 스테이지 버전데이타 받아오기
        //임시로 StageInfo 에 버전추가 더 상위 파일 만들기 -> StageMapData
        string stageKey = "pp" + Global.stageIndex+ ".xml";
        string stageName = Global.GetHashfromText(stageKey) + ".xml";
        bool loadStage = false;
        int stageCurrentVer = 0;
        
        // 이벤트 스테이지 일 경우
        if (Global.eventIndex > 0)
        {
            stageName = "E" + Global.eventIndex +"_" +Global.stageIndex + ".xml";
           // stageCurrentVer = ServerContents.GetEventChapter(Global.eventIndex).stageVersions[Global.stageIndex - 1];
            /*
            for (int i = 0; i < ServerContents.EventChapters.Count; i++)
            {
                if (ServerContents.EventChapters[i+1].index == Global.eventIndex)
                {
                    stageCurrentVer = ServerContents.EventChapters[i+1].stageVersions[Global.stageIndex - 1];
                }
            }*/
        }
        else
            stageCurrentVer = ManagerData._instance.StageVersionList[Global.stageIndex - 1];
        
        WWW www;
        WWWForm form = new WWWForm();
        form.AddField("Cache-Control", "no-cache");


       // if (PlayerPrefs.HasKey(stageKey))
        {
            string lastname =  PlayerPrefs.GetString("LastStageName", "");
            if(lastname.Length>0)
            {
                if (File.Exists(lastname))
                    File.Delete(lastname);
            }
            string stagePath = Global.StageDirectory + stageName;
            PlayerPrefs.SetString("LastStageName", stagePath);
            PlayerPrefs.Save();
        }


        www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
        yield return www;

        if (www.error == null)
        {
            try
            {
                int mapSize = www.bytes.Length;
                if (mapSize > 1024)
                {
                    StringReader reader = new StringReader(www.text);
                    var serializer = new XmlSerializer(typeof(StageMapData));
                    StageMapData stageData = serializer.Deserialize(reader) as StageMapData;

                    if (stageCurrentVer > stageData.version) loadStage = true;
                }
                else
                {
                    throw new System.Exception();
                }
            }
            catch
            {
                string stagePath = Global.StageDirectory + stageName;
                File.Delete(stagePath);
                loadStage = true;
            }
        }
        else
        {
            loadStage = true;
        }










        if (loadStage)
        {
            www = new WWW(Global._cdnAddress + "stage/" + stageName);//, form);
            yield return www;

            if (www.error != null)
            {
                www.Dispose();
                yield return null;
            }
            else
            {
                try
                {
                    int mapSize = www.bytes.Length;
                    if (mapSize > 256)
                    {
                        MemoryStream memoryStream = new MemoryStream(www.bytes);
                        memoryStream.Position = 0;
                        byte[] bytes = memoryStream.ToArray();
                        File.WriteAllBytes(Global.StageDirectory + stageName, bytes);
                        www.Dispose();
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
                catch
                {
                    string stagePath = Global.StageDirectory + stageName;
                    File.Delete(stagePath);
                }
            }
        }

        //stageName = "pp" + Global.stageIndex + ".xml";
        www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
        yield return www;

        if (www.error == null)
        {
            StringReader reader = new StringReader(www.text);
            var serializer = new XmlSerializer(typeof(StageMapData));
            StageMapData tempData = serializer.Deserialize(reader) as StageMapData;    


            // 이벤트 스테이지이지만 받은 에셋이 없을때
            if (Global.eventIndex > 0)
            {
                /*string assetFile = ServerContents.EventChapters[Global.eventIndex].assetName;
                if (!ManagerLobby._assetBankEvent.ContainsKey(assetFile))
                {
                    //Debug.Log("에셋 없어서 읽기 " + assetFile);

                    IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetFile);
                    while (e.MoveNext())
                        yield return e.Current;
                    if (e.Current != null)
                    {
                        AssetBundle assetBundle = e.Current as AssetBundle;
                        if (assetBundle != null)
                        {
                            GameObject obj = assetBundle.LoadAsset<GameObject>(assetFile);
                            ManagerLobby._assetBankEvent.Add(assetFile, obj);

                            for (int x = 0; x < ServerContents.EventChapters[Global.eventIndex].counts.Count; x++)
                            {
                                GameObject objReady = assetBundle.LoadAsset<GameObject>(assetFile + "_ready" + (x + 1));
                                ManagerLobby._assetBankEvent.Add(assetFile + "_ready" + (x + 1), objReady);
                            }
                         //   ManagerLobby._assetBankEvent.Add(assetFile, assetBundle);
                        }
                    }
                }
                else
                {

                }
               // Debug.Log("+++aaa+++++++++  " + assetFile);
               */
            }


            OpenPopupReady(tempData, callBackStart, callBackClose);
        }
        else
        {
            //Debug.Log("파일없음");
            IsOpenReady = false;
            NetworkLoading.EndNetworkLoading();
            //ErrorController.ShowNetworkErrorDialogAndRetry("", () => IsOpenReady = false);
        }

        yield return null;
    }

    public void OpenPopupReady(StageMapData stageData, Method.FunctionVoid callBackStart = null, Method.FunctionVoid callBackClose = null)
    {
        UIPopupReady popupReady = NGUITools.AddChild(anchorCenter, _objPopupReady).GetComponent<UIPopupReady>();
        if (callBackStart != null)
        {
            popupReady._callbackOpen += callBackStart;
        }
        if (callBackClose != null)
        {
            popupReady._callbackClose += callBackClose;
        }
        popupReady.OpenPopUp(GetPanelDepth(popupReady.bShowTopUI, popupReady.panelCount));

        // 이벤트 스테이지 경우
        if (Global.eventIndex > 0)
        {/*
            CdnEventChapter eventUser = ServerContents.EventChapters[Global.eventIndex];

            int groupState = ServerRepos.EventChapters[Global.eventIndex].groupState;

            if (UIPopupReady._instance != null)
            {
                if (UIPopupReady.eventGroupClear == true)
                {
                    groupState -= 1;
                }
            }
            
            if (groupState > eventUser.counts.Count)
                groupState = eventUser.counts.Count;

            //Debug.Log("이벤트 그룹 상태  "+groupState);
            // 이벤트 스테이지에 필요한 에셋 읽어서 넣어주기
            if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
            {
#if  UNITY_EDITOR
                string path = "Assets/5_OutResource/events/" + eventUser.assetName + "/" + eventUser.assetName + "_ready" + groupState + ".prefab";
                GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                popupReady.EventStageSetting(BundleObject);
#endif
            }
            else
            {
                string assetFile = eventUser.assetName;
                string assetName = assetFile + "_ready" + groupState;
                //GameObject BundleObject = ManagerLobby._assetBankEvent[assetFile].LoadAsset<GameObject>(assetName);
                popupReady.EventStageSetting(ManagerLobby._assetBankEvent[assetName]);
            }*/
        }

        popupReady.SettingSortOrder(GetSortLayer(popupReady.sortOrderCount));

        //마지막으로 연 스테이지.
        popupReady.InitPopUp(stageData);
        _popupList.Add(popupReady);





        NetworkLoading.EndNetworkLoading();
        IsOpenReady = false;
    }

    public void OpenPopupClear(int score)
    {
        //나가기 팝업이 있는지 체크.
        if (this.CheckExitUI() == true)
            ClosePopUpUI();
        UIPopupClear popupClear = NGUITools.AddChild(anchorCenter, _objPopupClear).GetComponent<UIPopupClear>();
        popupClear.OpenPopUp(GetPanelDepth(popupClear.bShowTopUI, popupClear.panelCount));
        popupClear.SettingSortOrder(GetSortLayer(popupClear.sortOrderCount));
        popupClear.InitPopUp(score);
        _popupList.Add(popupClear);
    }

    public void OpenPopupFail()
    {
        UIPopupFail popupFail = NGUITools.AddChild(anchorCenter, _objPopupFail).GetComponent<UIPopupFail>();
        popupFail.OpenPopUp(GetPanelDepth(popupFail.bShowTopUI, popupFail.panelCount));
        popupFail.SettingSortOrder(GetSortLayer(popupFail.sortOrderCount));
        popupFail.InitPopUp();
        _popupList.Add(popupFail);
    }

    public void OpenPopupContinue()
    {
        UIPopupContinue popupContinue = NGUITools.AddChild(gameObject, _objPopupContinue).GetComponent<UIPopupContinue>();
        popupContinue.OpenPopUp(GetPanelDepth(popupContinue.bShowTopUI, popupContinue.panelCount));
        popupContinue.SettingSortOrder(GetSortLayer(popupContinue.sortOrderCount));
        popupContinue.InitPopUp();
        _popupList.Add(popupContinue);
    }

    public void OpenPopupPause()
    {
        UIPopupPause popupPause = NGUITools.AddChild(gameObject, _objPopupPause).GetComponent<UIPopupPause>();
        popupPause.OpenPopUp(GetPanelDepth(popupPause.bShowTopUI, popupPause.panelCount));
        popupPause.SettingSortOrder(GetSortLayer(popupPause.sortOrderCount));
        _popupList.Add(popupPause);
    }

    public void OpenPopupNoMoreMove()
    {
        UIPopupNoMoreMove popupNoMoreMove = NGUITools.AddChild(gameObject, _objPopupNoMoreMove).GetComponent<UIPopupNoMoreMove>();
        popupNoMoreMove.OpenPopUp(GetPanelDepth(popupNoMoreMove.bShowTopUI, popupNoMoreMove.panelCount));
        popupNoMoreMove.SettingSortOrder(GetSortLayer(popupNoMoreMove.sortOrderCount));
        _popupList.Add(popupNoMoreMove);
    }

    public void OpenPopupStageTarget()
    {
        UIPopupStageTarget popupStageTarget = NGUITools.AddChild(gameObject, _objPopupStageTarget).GetComponent<UIPopupStageTarget>();
        popupStageTarget.OpenPopUp(GetPanelDepth(popupStageTarget.bShowTopUI, popupStageTarget.panelCount));
        popupStageTarget.SettingSortOrder(GetSortLayer(popupStageTarget.sortOrderCount));
        _popupList.Add(popupStageTarget);
    }

    public void OpenPopupStageReview()
    {
        UIPopupStageReview popupStageReview = NGUITools.AddChild(gameObject, _objPopupStageReview).GetComponent<UIPopupStageReview>();
        popupStageReview.OpenPopUp(GetPanelDepth(popupStageReview.bShowTopUI, popupStageReview.panelCount));
        popupStageReview.SettingSortOrder(GetSortLayer(popupStageReview.sortOrderCount));
        _popupList.Add(popupStageReview);
    }
    /*
    public void OpenPopupShare ( UIItemStamp itemStamp, bool isSetupData )
    {
        UIPopupShare popupShare = NGUITools.AddChild(gameObject, _objPopupShare).GetComponent<UIPopupShare>();
        popupShare.OpenPopUp(GetPanelDepth(popupShare.bShowTopUI, popupShare.panelCount));
        popupShare.SettingSortOrder(GetSortLayer(popupShare.sortOrderCount));
        popupShare.InitPopUp( itemStamp, isSetupData );
        _popupList.Add(popupShare);
    }
    */
    public UIPopupConfirm OpenPopupConfirm (string _description, System.Action _callBack = null)
    {
        UIPopupConfirm popupConfirm = NGUITools.AddChild( gameObject, _objPopupConfirm ).GetComponent<UIPopupConfirm>();
        popupConfirm.OpenPopUp( GetPanelDepth( popupConfirm.bShowTopUI, popupConfirm.panelCount ) );
        popupConfirm.SettingSortOrder(GetSortLayer(popupConfirm.sortOrderCount));
        popupConfirm.InitPopUp( _description, _callBack );
        _popupList.Add( popupConfirm );

        return popupConfirm;
    }

    public void OpenPopupInputText ( System.Action<string> _callback )
    {
        UIPopupInputText popupInputText = NGUITools.AddChild( gameObject, _objPopupInputText ).GetComponent<UIPopupInputText>();
        popupInputText.OpenPopUp( GetPanelDepth( popupInputText.bShowTopUI, popupInputText.panelCount ) );
        popupInputText.SettingSortOrder(GetSortLayer(popupInputText.sortOrderCount));
        popupInputText.InitPopUp( _callback );
        _popupList.Add( popupInputText );
    }
    /*
    public void OpenPopopSendItemToSocial (UIItemStamp _sendItem, Stamp originData, System.Action<UIItemStamp> _callbackBtnHandler, System.Action<Stamp> _callbackResetHandler, int stampIndex = 0)
    {
        UIPopupSendItemToSocial popupSendItemToSocial = NGUITools.AddChild( gameObject, _objPopupSendItemToSocial ).GetComponent<UIPopupSendItemToSocial>();
        popupSendItemToSocial.OpenPopUp( GetPanelDepth( popupSendItemToSocial.bShowTopUI, popupSendItemToSocial.panelCount ) );
        popupSendItemToSocial.SettingSortOrder(GetSortLayer(popupSendItemToSocial.sortOrderCount));
        popupSendItemToSocial.InitPopup( _sendItem, originData, _callbackBtnHandler, _callbackResetHandler, false, stampIndex);
        _popupList.Add( popupSendItemToSocial );
    }
    */
    public void OpenPopupPanelEmotionIcon ( System.Action callbackHandler, GameObject parent )
    {
        UIPopupPanelEmotionIcon popupEmotionIcon = NGUITools.AddChild(parent, this._objPopupPanelEmotionIcon).GetComponent<UIPopupPanelEmotionIcon>();
        popupEmotionIcon.OpenPopUp(GetPanelDepth(popupEmotionIcon.bShowTopUI, popupEmotionIcon.panelCount));
        popupEmotionIcon.SettingSortOrder(GetSortLayer(popupEmotionIcon.sortOrderCount));
        popupEmotionIcon.InitPopup(callbackHandler);
        popupEmotionIcon.transform.localPosition = new Vector3(-18f, 0f, 0f);
        _popupList.Add(popupEmotionIcon);
    }
    /*
    public PopupInputColorPad OpenPopupInputColorPad (System.Action<Color> eventDelegate, System.Action<List<UIItemColor>> eventDelegateHandler )
    {
        PopupInputColorPad popupInputColorPad = NGUITools.AddChild(gameObject, _objPopupInputColorPad).GetComponent<PopupInputColorPad>();
        popupInputColorPad.OpenPopUp(GetPanelDepth(popupInputColorPad.bShowTopUI, popupInputColorPad.panelCount));
        popupInputColorPad.SettingSortOrder(GetSortLayer(popupInputColorPad.sortOrderCount));
        popupInputColorPad.InitData(eventDelegate, eventDelegateHandler);
        _popupList.Add(popupInputColorPad);

        return popupInputColorPad;
    }
    */
    public GameObject InstantiateUIObject(string path, GameObject parent)
    {
        GameObject prefab = Resources.Load(path) as GameObject;
        var go = Instantiate(prefab);
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    public bool CheckExitUI()
    {
        int nCount = _popupList.Count;
        if (nCount == 0)
            return false;
        if (_popupList[nCount - 1].GetPopupType() != PopupType.exit)
            return false;
        return true;
    }

    public void ClosePopUpUI(float _mainTime = 0.3f)
    {   
        int _nCount = _popupList.Count;
        if (_nCount == 0)
        {
            return;
        }
        _popupList[_nCount - 1].ClosePopUp(_mainTime, TopUIDeathAndSortLayer);
        _popupList.Remove(_popupList[_nCount - 1]);
    }

    public void ImmediatelyCloseAllPopUp()
    {
        int _nCount = _popupList.Count;
        for (int i = 0; i < _nCount; i++)
        {
            if(_popupList[i].gameObject != null)
            { 
                Destroy(_popupList[i].gameObject);
            }
        }
        _popupList.Clear();
    }

    public void DestroyPopupList ( System.Action eventHandler )
    {
        int length = this._popupList.Count;
        for ( int i = 0; i < length; i++ )
        {
            DestroyImmediate( this._popupList[i].gameObject );
        }
        this._popupList.Clear();

        this.StartCoroutine (this.DestroySystemPopup(eventHandler));
    }

    IEnumerator DestroySystemPopup ( System.Action eventHandler )
    {
        while( UIPopupSystem._instance != null )
        {
            yield return null;
        }
        eventHandler();
    }

    private int GetPanelDepth(bool bShowTopUI, int panelCount)
    {
        int count = _popupList.Count;
        int nextDepth = Default_Depth_High;

        if (count > 0)
        {
            //패널의 다음 뎁스값 = 현재패널의 뎁스값 + 패널 수 + (topUI가 들어갈 공간 1).
            nextDepth = (_popupList[count - 1].uiPanel.depth) + (_popupList[count - 1].panelCount) + 1;
        }
        if (bShowTopUI == true)
            topUiPanel.depth = nextDepth + panelCount;
        return nextDepth;
    }

    private void TopUIDeathAndSortLayer()
    {
        SettingTopUIDeath();
        SettingSortLayer();
    }

    private void SettingTopUIDeath()
    {
        int _nCount = _popupList.Count;
        if (_nCount - 1 >= 0)
        {
            if (_popupList[_nCount - 1].bShowTopUI == false)
                topUiPanel.depth = _popupList[_nCount - 1].uiPanel.depth - 1;
            else
                topUiPanel.depth = _popupList[_nCount - 1].uiPanel.depth + _popupList[_nCount - 1].panelCount;
        }
        else
            topUiPanel.depth = 1;
    }

    private void SettingSortLayer()
    {
        int pCount = _popupList.Count;
        if (pCount - 1 >= 0)
        {
            UIPopupBase popup = _popupList[pCount - 1];
            //이전 팝업이 재화 UI를 띄워야 하고, sortOrder를 사용 중이라면 topUI가 해당 팝업 위로 가도록.
            if (popup.bShowTopUI == true && popup.uiPanel.useSortingOrder == true)
            {
                topUiPanel.useSortingOrder = true;
                topUiPanel.sortingOrder = (popup.uiPanel.sortingOrder) + (popup.sortOrderCount) + (popup.panelCount) + 1;
            }
            else
            {
                topUiPanel.useSortingOrder = false;
                return;
            }
        }
        else
        {
            topUiPanel.useSortingOrder = false;
        }
    }

    private int GetSortLayer(int orderCount)
    {
        int count = _popupList.Count;
        int nextLayer = -1;

        //이전 팝업이 sortLayer 사용 안하고, 현재 팝업은 사용 한다면 초기 값(3) 부터 시작.
        if (count == 0 || (_popupList[count - 1].sortOrderCount == 0 && _popupList[count - 1].uiPanel.useSortingOrder == false))
        {
            if (orderCount > 0)
            {
                //10부터 시작.
                nextLayer = 10;
            }
        }
        //이전 팝업이 sortLayer 사용하고 있다면(패널에는 적용안돼있는 경우).
        else if (_popupList[count - 1].sortOrderCount > 0 && _popupList[count - 1].uiPanel.useSortingOrder == false)
        {
            nextLayer = 10 + (_popupList[count - 1].uiPanel.sortingOrder) + (_popupList[count - 1].sortOrderCount) + (_popupList[count - 1].panelCount) + 1;
        }
        //이전 팝업이 sortLayer 사용하고 있다면.
        else
        {
            //패널의 다음 layer값 = 현재패널의 layer값 + layer 수 + 패널 카운트+ (topUI가 들어갈 공간 1).
            nextLayer = (_popupList[count - 1].uiPanel.sortingOrder) + (_popupList[count - 1].sortOrderCount) + (_popupList[count - 1].panelCount) + 1;
        }
        return nextLayer;
    }

    public void TopUIPanelSortOrder(UIPopupBase popup)
    {
        if (popup.bShowTopUI == false || popup.uiPanel.useSortingOrder == false)
            return;
        topUiPanel.useSortingOrder = true;
        //topUI layer값 = 현재패널의 layer값 + layer 수 + 패널 카운트.
        topUiPanel.sortingOrder = popup.uiPanel.sortingOrder + popup.sortOrderCount + popup.panelCount;
    }

    private Vector2 CalculateUISize(float _fHeight)
    {
        Vector2 _size = Vector2.zero;
        float _fAspect = _camera.aspect;
        _size = new Vector2((_fAspect * _fHeight), _fHeight);
        return _size;
    }

    //하트부족시 하트흔들기
    public void ShakeHeart()
    {
        StartCoroutine(shakingHeart());
    }

    IEnumerator shakingHeart()
    {
        float timer = 0f;

        while (true)
        {

            timer += Global.deltaTimeLobby*10f;
            if (timer > ManagerBlock.PI90*2)
            {
                _StarSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            }
            float ratio = Mathf.Sin(timer*8)/Mathf.Exp(timer);
            _StarSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, ratio*30f));
            yield return null;
        }
        bTouchTopUI = true;
    }

    public void GuestLoginSignInCheck(bool bSortOrder = false, int order = 0)
    {
        UIPopupSystem popupConfirm = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        /*"System" ,"Sign in?" */
        popupConfirm.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_1" ), false, null );
        popupConfirm.FunctionSetting( 1, "RebootSignIn", gameObject, true );
        if (bSortOrder == true)
        {
            popupConfirm.SortOrderSetting(true, order);
        }
    }

    /// <summary>
    /// SignInGuide
    /// </summary>
    public void GuestLoginSignInGuide ()
    {
        UIPopupSystem popupConfirm = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
        popupConfirm.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_s_6" ), false, null );
        popupConfirm.FunctionSetting( 1, "RebootSignIn", gameObject, true );
    }

    void RebootSignIn()
    {
    }

    public void LackCoinsPopUp()
    {
        if (UIPopupSystem._instance != null) return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        popupSystem.FunctionSetting(1, "OnClickCoinShop", gameObject, true);
        Texture2D texture = Resources.Load("Message/coin") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_6"), false, texture);
    }

    public void LackDiamondsPopUp()
    {
        if (UIPopupSystem._instance != null) return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        popupSystem.FunctionSetting(1, "OnClickDiamondShop", gameObject, true);
        Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_5"), false, texture);
    }

    public string UserNameLengthCheck(string name)
    {
        string name_trim = name.Trim();
        if (name_trim.Length < 1)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_11"), false);
            return "";
        }
        return name_trim;
    }

    public string UserNameCharacterCheck(string name)
    {
        string nameChange = name;
        nameChange = nameChange.Replace("[", "");
        nameChange = nameChange.Replace("]", "");

        if (nameChange.Length != name.Length)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_4"), false);
        }
        return nameChange;
    }

    public void SettingDiaryNewIcon(bool bActive)
    {
        diaryNewIcon.SetActive(bActive);
    }

    public void SettingEventIcon(bool bActive)
    {
        diaryEventIcon.SetActive(bActive);
    }

    void OnClickBtnComingSoon()
    {
        if (UIPopupSystem._instance != null) return;

        //커밍순 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/soon") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_15"), false, texture);
    }

    void SaleInfoSetting()
    {/*
        if (ServerRepos.LoginCdn == null)
            return;

        //세일 정보 받아와서 세팅.
        if (ServerRepos.LoginCdn.clover5Sale == 1 || ServerRepos.LoginCdn.cloverFreeTimeSale == 1)
        {
            saleIcon_clover.SetActive(true);
        }
        else
        {
            saleIcon_clover.SetActive(false);
        }

        if (ServerRepos.LoginCdn.coinSale == 1)
        {
            saleIcon_coin.SetActive(true);
        }
        else
        {
            saleIcon_coin.SetActive(false);
        }

        if (ServerRepos.LoginCdn.jewelSale == 1)
        {
            saleIcon_jewel.SetActive(true);
        }
        else
        {
            saleIcon_jewel.SetActive(false);
        }*/
    }

    void OnClickCloverShop()
    {
        if (bTouchTopUI == false)
            return;


        //인게임일경우 안열림
        if (SceneManager.GetActiveScene().name == "InGame")
        {
            if (UIPopupReady._instance == null) return;
        }
        

        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.cloverShop)
            return;

        bTouchTopUI = false;
        if ((pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.jewelShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.coinShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method))
        {
            _popupList[pCount - 1]._callbackClose += OpenPopupCloverShop;
            _popupList[pCount - 1]._callbackEnd -= SortingOrderFalse;
            _popupList[pCount - 1].bShopBehind = true;
            ClosePopUpUI();
        }
        else
        {   
            OpenPopupCloverShop();
        }
    }

    public void OnClickCoinShop()
    {
        if (bTouchTopUI == false)
            return;

        if (SceneManager.GetActiveScene().name == "InGame")
        {
            if (GameManager.instance.state == GameState.GAMECLEAR) return;

            if (UIPopupContinue._instance != null && UIPopupContinue._instance.pushContinue == true) return;
        }


        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.coinShop)
            return;

        bTouchTopUI = false;
        if ((pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.cloverShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.jewelShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method))
        {
            _popupList[pCount - 1]._callbackClose += OpenPopupCoinShop;
            _popupList[pCount - 1]._callbackEnd -= SortingOrderFalse;
            _popupList[pCount - 1].bShopBehind = true;
            ClosePopUpUI();
        }
        else
        {
            OpenPopupCoinShop();
        }
    }

    public void OnClickDiamondShop()
    {
        if (bTouchTopUI == false)
            return;

        if (SceneManager.GetActiveScene().name == "InGame" )
        {
            if (GameManager.instance.state == GameState.GAMECLEAR) return;

            if (UIPopupContinue._instance != null && UIPopupContinue._instance.pushContinue == true) return;
        }


        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.jewelShop)
            return;

        bTouchTopUI = false;
        if ((pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.cloverShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.coinShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method))
        {
            _popupList[pCount - 1]._callbackClose += OpenPopupDiamondShop;
            _popupList[pCount - 1]._callbackEnd -= SortingOrderFalse;
            _popupList[pCount - 1].bShopBehind = true;
            ClosePopUpUI();
        }
        else
        {
            OpenPopupDiamondShop();
        }
    }

    public void OnClickMethod()
    {
        if (bTouchTopUI == false)
            return;

        //인게임일경우 안열림
        if (SceneManager.GetActiveScene().name == "InGame")        
            return;
        

        int pCount = _popupList.Count;
        if (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.method)
            return;

        bTouchTopUI = false;
        if ((pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.cloverShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.coinShop) || (pCount > 0 && _popupList[pCount - 1].GetPopupType() == PopupType.jewelShop))
        {
            _popupList[pCount - 1]._callbackClose += OpenPopupMethod;
            _popupList[pCount - 1].bShopBehind = true;
            ClosePopUpUI();
        }
        else
        {
            OpenPopupMethod();
        }
    }

    public void OpenPopupRequestReview()
    {
        if (Review.CanReviewInApp())
        {
            Review.RequestReview();
        }
        else
        {
            UIPopupSystem popupSystem = OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.FunctionSetting(1, "OnRequestReview", gameObject);
            popupSystem.SetButtonText(1, "評価する");
            popupSystem.SetButtonText(2, "今はしない");
            Texture2D texture = Resources.Load("Message/happy2") as Texture2D;
            popupSystem.InitSystemPopUp("お願い", Global._instance.GetString("p_arv_1"), true, texture);


            ManagerSound.AudioPlay(AudioLobby.m_bird_hehe);
            
        }
    }
    public void OnRequestReview()
    {
        Review.RequestReview();
    }

    public void SortingOrderFalse()
    {
        topUiPanel.useSortingOrder = false;
    }

    void OnClickExit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }

    void SetAnchorRightUI_Ios()
    {
        anchorRightUI[2].SetActive(false);
        for (int i = 0; i < 2; i++)
        {
            float yPos = 380f - (100f * i);
            anchorRightUI[i].transform.localPosition = new Vector3(-45f, yPos, 0f);
        }
    }

    public bool UIButtonCanClickCheck()
    {
        if (Global.CanClickButton() == false)
            return false;
        Global.InitClickTime();
        return true;
    }

    private IEnumerator CoMoveCoinEventObject()
    {
        float initPos = 122.9f+5f;
        while (_coinEventRoot.activeInHierarchy == true)
        {
            _coinEventRoot.transform.localPosition
                = new Vector3(_coinEventRoot.transform.localPosition.x, initPos + (Mathf.Cos(Time.time * 5f) * 3f), 0f);
            yield return null;
        }
        yield return null;
    }


    public static string GetColorTypeString(BlockColorType colorType)
    {
        switch (colorType)
        {
            case BlockColorType.A:
                return "A";

            case BlockColorType.B:
                return "B";

            case BlockColorType.C:
                return "C";

            case BlockColorType.D:
                return "D";

            case BlockColorType.E:

                if (Global.eventIndex == 4  || Global.eventIndex == 5)
                {
                   // if (ServerContents.GetEventChapter(4).active == 1 || ServerContents.GetEventChapter(5).active == 1) //if (Global.CollaboIndex == 1) //if (ServerContents.GetEventChapter(Global.eventIndex).active == 1)//if (Global.CollaboIndex == 1)   //Bono
                    {
                        return "Bono";
                    }
                }
                return "E";

            case BlockColorType.F:
                return "F";

            default:
                return "";
        }
    }
}
