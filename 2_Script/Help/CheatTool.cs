using UnityEngine;

public class CheatTool : MonoBehaviour
{
    public GameObject showInGameInfoCheck;
    public GameObject missionIndexCheck;
    public GameObject failDataCheck;
    public GameObject showEndContentsStageCheck;
    public GameObject tutorialCheck;
    public GameObject inGameClearCheck;
    public GameObject showAdNetworkInfoCheck;
    public GameObject showMinorPriceCheck;

    private void Start()
    {
        showInGameInfoCheck.SetActive(Global._instance.ShowIngameInfo);
        missionIndexCheck.SetActive(Global._instance.isShowIndex);
        failDataCheck.SetActive(Global._instance.showFailDataBTN);
        showEndContentsStageCheck.SetActive(Global._instance.ShowECStage);
        tutorialCheck.SetActive(Global._instance.showTutorialBTN);
        inGameClearCheck.SetActive(Global._instance.showInGameClearBTN);
        showAdNetworkInfoCheck.SetActive(Global._instance.showAdNetworkInfo);
        showMinorPriceCheck.SetActive(Global._instance.showMinorPriceInfo);
    }

    private void ReceiveAdmin()
    {
        PlayerPrefs.DeleteAll();
        Global.star   = (int)GameData.User.Star;
        Global.clover = (int)(GameData.User.AllClover);
        Global.coin   = (int)(GameData.User.AllCoin);
        Global.jewel  = (int)(GameData.User.AllJewel);

        Application.Quit();
    }

    /// <summary>
    /// 배너 New 아이콘 캐시 제거.
    /// </summary>
    private void OnClickDeleteCheckerCache()
    {
        PlayerPrefs.DeleteKey("BannerNewChecker");
    }

    /// <summary>
    /// 랜드 선택 팝업 출력.
    /// </summary>
    private void OnClickMoveLand()
    {
        ManagerUI._instance.OpenPopup<UIPopUpLandSelect>();
    }

    /// <summary>
    /// 로그인 시간 30일 전으로 변경.
    /// </summary>
    private void OnClickBackLoginTs()
    {
        ServerAPI.Test_BackLoginTs();
    }

    /// <summary>
    /// 인게임 정보 출력(레벨조정 여부, 결제 횟수, 플레이 횟수...).
    /// </summary>
    private void OnClickShowIngameInfo()
    {
        Global._instance.ShowIngameInfo = !Global._instance.ShowIngameInfo;
        showInGameInfoCheck.SetActive(Global._instance.ShowIngameInfo);
    }
    
    /// <summary>
    /// 미션 인덱스 출력.
    /// </summary>
    private void OnClickShowMissionIdx()
    {
        Global._instance.isShowIndex = !Global._instance.isShowIndex;
        missionIndexCheck.SetActive(Global._instance.isShowIndex);
    }

    /// <summary>
    /// AppLovin 미디에이션 디버거.
    /// </summary>
    private void OnClickAdTest()
    {
        AdManager.AppLovinSDK_MediationDebugger();
    }
    
    /// <summary>
    /// AppLovin 광고 네트워크 이름 출력.
    /// </summary>
    private void OnClickShowAdNetwork()
    {
        Global._instance.showAdNetworkInfo = !Global._instance.showAdNetworkInfo;
        showAdNetworkInfoCheck.SetActive(Global._instance.showAdNetworkInfo);
    }
    
    /// <summary>
    /// 미성년자 보호 가이드라인 관련 정보 출력
    /// </summary>
    private void OnClickShowMinorPrice()
    {
        Global._instance.showMinorPriceInfo = !Global._instance.showMinorPriceInfo;
        showMinorPriceCheck.SetActive(Global._instance.showMinorPriceInfo);
    }

    /// <summary>
    /// 로컬 푸시 테스트(20초 후 알림).
    /// </summary>
    private void OnLocalNotification20s()
    {
#if !UNITY_EDITOR
        LocalNotification.RegisterNotification(LOCAL_NOTIFICATION_TYPE.CLOVER, 100, 20,
            "Local Push Test", true, "hehe");
#endif
    }

    /// <summary>
    /// Reboot.
    /// </summary>
    private void OnClickReboot()
    {
        Global.ReBoot();
    }

    /// <summary>
    /// 친구 초대 팝업 오픈.
    /// </summary>
    private void OnClickInvite()
    {
        ManagerUI._instance.OpenPopupInviteSmall();
    }

    /// <summary>
    /// 재화 초기화.
    /// </summary>
    private void OnClickBtnClearMoney()
    {
        if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
        {
            ServerAPI.Admin_ResetUserAsset(() => { Global.ReBoot(); });
        }
    }

    /// <summary>
    /// 캐싱된 내용 초기화.
    /// </summary>
    private void OnClickDeleteLocalImage()
    {
        PlayerPrefs.DeleteAll();

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

        if (System.IO.Directory.Exists(Global.movieDirectory))
        {
            System.IO.Directory.Delete(Global.movieDirectory, true);
            System.IO.Directory.CreateDirectory(Global.movieDirectory);
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

        if (System.IO.Directory.Exists(Global.adventureDirectory))
        {
            System.IO.Directory.Delete(Global.adventureDirectory, true);
            System.IO.Directory.CreateDirectory(Global.adventureDirectory);
        }

        if (System.IO.Directory.Exists(Global.effectDirectory))
        {
            System.IO.Directory.Delete(Global.effectDirectory, true);
            System.IO.Directory.CreateDirectory(Global.effectDirectory);
        }

        if (System.IO.Directory.Exists(Global.soundDirectory))
        {
            System.IO.Directory.Delete(Global.soundDirectory, true);
            System.IO.Directory.CreateDirectory(Global.soundDirectory);
        }

        if (System.IO.Directory.Exists(Global.cachedScriptDirectory))
        {
            System.IO.Directory.Delete(Global.cachedScriptDirectory, true);
            System.IO.Directory.CreateDirectory(Global.cachedScriptDirectory);
        }

        if (System.IO.Directory.Exists(Global.cachedResourceDirectory))
        {
            System.IO.Directory.Delete(Global.cachedResourceDirectory, true);
            System.IO.Directory.CreateDirectory(Global.cachedResourceDirectory);
        }
    }

    /// <summary>
    /// 클로버 요청 팝업 출력.
    /// </summary>
    private void OnClickReqClover()
    {
        ManagerUI._instance.OpenPopupRequestSmall();
    }

    /// <summary>
    /// 엔드컨텐츠 스테이지 번호 출력.
    /// </summary>
    private void OnClickShowECStage()
    {
        Global._instance.ShowECStage = !Global._instance.ShowECStage;
        if (showEndContentsStageCheck != null)
        {
            showEndContentsStageCheck.SetActive(Global._instance.ShowECStage);
        }
    }

    /// <summary>
    /// 튜토리얼 출력 치트(이미 확인한 튜토리얼도 재확인 가능).
    /// </summary>
    private void OnClickBtnShowTutorial()
    {
        Global._instance.showTutorialBTN = !Global._instance.showTutorialBTN;
        tutorialCheck.SetActive(Global._instance.showTutorialBTN);
    }

    /// <summary>
    /// 인게임 치트 활성화(스테이지 성공, 실패 등..).
    /// </summary>
    private void OnClickBtnHelper()
    {
        Global._instance.showInGameClearBTN = !Global._instance.showInGameClearBTN;
        inGameClearCheck.SetActive(Global._instance.showInGameClearBTN);
    }

    /// <summary>
    /// 캐싱된 FAIL 데이터 출력.
    /// </summary>
    private void OnClickBtnShowFailData()
    {
        Global._instance.showFailDataBTN = !Global._instance.showFailDataBTN;
        failDataCheck.SetActive(Global._instance.showFailDataBTN);
    }

    /// <summary>
    /// 데이터 리셋.
    /// </summary>
    private void OnClickBtnReset()
    {
        ServerAPI.Admin_PurgeUser(ReceiveAdmin);
    }

    /// <summary>
    /// 치트 UI 비활성화.
    /// </summary>
    private void OnClickHide()
    {
        gameObject.SetActive(false);
    }
}