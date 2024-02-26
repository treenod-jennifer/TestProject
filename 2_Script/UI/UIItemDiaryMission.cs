using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Protocol;
using Newtonsoft.Json;

public class UIItemDiaryMission : MonoBehaviour
{
    public GameObject timeObject;
    public GameObject progressObject;
    public GameObject btnSpeedUpObject;
    public GameObject btnClearObject;
    public GameObject starObject;

    public UISprite mainSprite;
    public UISprite activeButton;
    public UIUrlTexture missionIcon;
    public UILabel missionInfo;
    public UILabel missionTime;
    public UILabel missionStar;
    public UILabel missionCoin;
    public UILabel buttonClear;
    public UILabel progressStep;
    public UIProgressBar progressBar;

    [HideInInspector]
    public MissionData mData = null;
    [HideInInspector]
    public int itemIndex = 0;

    static bool isClearMission = false;

    private bool bUseStar = true;
    private float missionRemoveTime = 0.3f;
    private float progressOffset = 0f;
    private int coin = 0;

    [SerializeField]
    private UILabel missionIdxLabel;

    //그로씨용 
    static public int GrowthyMisisonIndex = 0;

    public void InitBtnMission(MissionData in_data)
    {
        mData = in_data;
        string fileName = "m_" + mData.index;
        missionIcon.SettingTextureScale(90, 90);
        missionIcon.LoadCDN(Global.gameImageDirectory, "IconMission/", fileName);
        //StartCoroutine(missionIcon.SetTextureScale(90, 90));
        string key = "m" + mData.index;
        missionInfo.text = Global._instance.GetString(key);
        missionStar.text = string.Format("x{0}", mData.needStar);

        //미션 인덱스 보이게 할지 설정.
        SetShowMissionIndex();

        //시간 미션 관련.
        if (mData.waitTime > 0)
        {
            //시간이 덜 됐을 때 설정.
            if (mData.clearTime != 0)
            {
                float leftTime = Global.LeftTime(mData.clearTime);
                if (leftTime > 0)
                {
                    //코인 설정.
                    int timeCoin = 100;
                    int time = 0;

                    if (leftTime < 3600)
                    {
                        coin = timeCoin;
                    }
                    else
                    {
                        time = (int)(leftTime / 3600);
                    }
                    coin = timeCoin + (timeCoin * time);
                    missionCoin.text = coin.ToString();

                    timeObject.SetActive(true);
                    btnSpeedUpObject.SetActive(true);
                    btnClearObject.SetActive(false);
                    StartCoroutine(CoMissionTimer(mData));
                }
                //시간이 다 지난 상태에서의 설정.
                else
                {
                    bUseStar = false;
                    ButtonDoNotUseStar();
                    //버튼 깜빡깜빡.
                    StartCoroutine(CoMissionActive());
                }
            }
            //아직 미션 시작 안한 상태에서의 설정.
            else
            {
                timeObject.SetActive(true);
                missionTime.text = Global.GetTimeText_HHMMSS(mData.waitTime, false);
                //버튼 누를 수 있는 상태일 떄는 깜빡깜빡.
                if (mData.needStar <= ServerRepos.User.Star && isClearMission == false)
                {
                    StartCoroutine(CoMissionActive());
                }
            }
        }
        else
        {
            //버튼 누를 수 있는 상태일 떄는 깜빡깜빡.
            if (mData.needStar <= ServerRepos.User.Star && isClearMission == false)
            {
                StartCoroutine(CoMissionActive());
            }
        }

        //단계 미션 관련.
        if (mData.stepClear > 0 && mData.stepClear != mData.clearCount)
        {
            progressObject.SetActive(true);
            progressOffset = 100f / mData.stepClear;
            progressBar.value = (mData.clearCount * progressOffset) * 0.01f;
            progressStep.text = string.Format("{0}/{1}", mData.clearCount, mData.stepClear);
        }
    }

    public void DestroyMission()
    {
        StartCoroutine(DoDestroyMission());
    }

    void OnClickBtnClear()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return;

        //터치막음.
        UIPopupDiary._instance.bCanTouch = false;

        UIDiaryMission._instance.backupMissionData = ManagerData.BackupMissionData();
        UIDiaryMission._instance.backupDay = ServerContents.Day;

        //현재 열린 스테이지 다 열었을 때, 커밍 순 팝업.
        if (ServerRepos.User.missionCnt >= ServerRepos.LoginCdn.OpenMission || (ServerRepos.LoginCdn.LimitMission != 0 && ServerRepos.User.missionCnt >= ServerRepos.LoginCdn.LimitMission))
        {
            ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
            ManagerUI._instance.ShakeHeart();

            ManagerUI._instance.OpenPopup<UIPopUpComingSoonComplete>();

            //터치 가능.
            UIPopupDiary._instance.bCanTouch = true;
            return;
        }

        //미션 클리어 연출.
        if ((mData.needStar <= ServerRepos.User.Star && isClearMission == false) || bUseStar == false)
        {
            ServerAPI.ApplyMissionStar(mData.index, recvMissionApply);
        }

        //별 부족 시.
        else
        {
            ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
            ManagerUI._instance.ShakeHeart();

            //별 부족 메세지.
            ManagerUI._instance.OnClickMethod_OpenPopUp(true);

            //터치 가능.
            UIPopupDiary._instance.bCanTouch = true;
        }
    }

    private void SetShowMissionIndex()
    {
        //옵션에서 미션 번호 켜면, 해당 미션 인덱스 보이도록 해줌
        if (Global._instance.isShowIndex == true)
        {
            missionIdxLabel.gameObject.SetActive(true);
            missionIdxLabel.text = mData.index.ToString();
        }
        else
        {
            missionIdxLabel.gameObject.SetActive(false);
        }
    }

    void recvMissionApply(ApplyMissionResp resp)
    {
        if (resp.IsSuccess)
        {
            {
                //미션 클리어 시에는 팝업 사라져서 터치 가능 안 풀어줌.
                UIDiaryMission._instance.bClearButton = true;
                ManagerSound.AudioPlay(AudioLobby.UseClover);

                isClearMission = true;

                //별 사용할 때 별 감소연출 재생.
                if (bUseStar == true)
                {
                    NGUITools.AddChild(ManagerUI._instance._StarSprite.gameObject, UIDiaryMission._instance.objHeartEffectShine).GetComponent<RingGlowEffect>();

                    //오브젝트풀에 넣어야함
                    UseHeartEffect obj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, ManagerUI._instance._ObjHeartEffect).GetComponent<UseHeartEffect>();  //ManagerObjectPool.Spawn("FlyUseHeart")
                    obj.Init(ManagerUI._instance._StarSprite.transform.position, starObject.transform.position, this);

                    //bool newDay = false;
                    //ManagerNetwork._instance.SendMissionClear(missionIndex, ref newDay);

                    // 미리 감소 보여줌,
                    Global.star -= mData.needStar;
                    ManagerUI._instance.UpdateUI();
                }

                //별 사용하지 않는다면 누르는 즉시 미션 제거.
                else
                {
                    DestroyMission();
                }
            }

            int beforeDay = Global.day;
            int activeMission = 0;

            foreach (var item in ServerRepos.OpenMission)
            {
                // 비활성화였던 미션이 활성화 되는 상황 아이콘 미리 읽어두기
                if (ManagerData._instance._missionData[item.idx].state == TypeMissionState.Inactive && (TypeMissionState)item.state == TypeMissionState.Active)
                {
                    ResourceManager.LoadCDN(Global.gameImageDirectory, "IconMission", $"m_{item.idx}", (Texture2D texture) => ResourceManager.UnLoad(texture));
                    PlayerPrefs.SetInt("missionCheck" + item.idx, 1);   // 다이어리 미션 아이콘 연출관련
                }
                ManagerData._instance._missionData[item.idx].state = (TypeMissionState)item.state;
                ManagerData._instance._missionData[item.idx].clearCount = item.clearCount;
                ManagerData._instance._missionData[item.idx].clearTime = item.clearTime;

                if (item.state == 1)
                    activeMission = item.idx;
            }
            Global.day = (int)ServerRepos.User.day;
            Global.star = (int)ServerRepos.User.Star;
            // 새로운 날이면 첫 미션 씬 비활성화로 임시 설정,,, 연출이후 활성화 설정
            if (beforeDay < ServerRepos.User.day)
                ManagerData._instance._missionData[activeMission].state = TypeMissionState.Inactive;

            //데이터들 갱신.
            QuestGameData.SetUserData();
            HousingUserData.SetUserData();
            PlusHousingModelData.SetUserData();
            MaterialSpawnUserData.SetUserData();
            ManagerLobby._instance.ReMakeMaterial();

            //다이어리 쪽 데이터 갱신.
            UIDiaryController._instance.UpdateMissionData();
            UIDiaryController._instance.UpdateProgressHousingData();
            UIDiaryController._instance.UpdateQuestData(true);

            if (resp.mission.clearTime > 0)
            {
                LocalNotification.TimeMissionNotification(resp.mission.idx, (int)resp.mission.clearTime - (int)GameData.GetTime());
            }
            if (resp.dayCompleteReward.directApplied != null)
            {
                if (UIDiaryMission._instance != null)
                    UIDiaryMission._instance.dayCompleteReward = resp.dayCompleteReward;
            }

            //그로씨            
            {
                GrowthyMisisonIndex = mData.index;

                string key = "m" + mData.index;

                if (bUseStar != false)
                {
                    var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_QUEST_PLAY,
                        0,
                        -mData.needStar,
                        0,
                        (int)(ServerRepos.User.star),
                        key
                        );
                    var docMoney = JsonConvert.SerializeObject(growthyMoney);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                }


                var playEnd = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.MISSION,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.NORMAL,
                    "m_" + mData.index, //key,//AchievementName,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

        }
    }

    void recvMissionTime(TimeMissionResp resp)
    {
        if (resp.IsSuccess)
        {
            bUseStar = false;
            ButtonDoNotUseStar();

            mData.clearTime = resp.mission.clearTime;

            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();
            UIDiaryController._instance.UpdateMissionData();

            //그로씨
            {
                string key = "m" + mData.index;
                var playEndrecvMT = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_REDUCE_TIME_MISSION,
                    -usePCoin,
                    -useFCoin,
                    (int)(GameData.User.coin),
                    (int)(GameData.User.fcoin),
                    key// mData.index.ToString()//ManagerArea._instance.GetAreaString(mData.sceneArea, key)
                    );

                var doc = JsonConvert.SerializeObject(playEndrecvMT);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
            }
            LocalNotification.RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE.TIME_MISSION, resp.mission.idx);
        }
        //통신 후 터치 가능하게.
        UIPopupDiary._instance.bCanTouch = true;
    }

    int usePCoin = 0;
    int useFCoin = 0;

    void OnClickBtnSpeedUp()
    {
        //터치 가능 조건 검사.
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        //터치막음.
        UIPopupDiary._instance.bCanTouch = false;

        if (LanguageUtility.IsShowBuyInfo)
        {
            UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
            popupSystem.SetCallbackSetting(1, OnClickSpeedUpConfirm, true);
            popupSystem._callbackClose = () => UIPopupDiary._instance.bCanTouch = true;
            popupSystem.SetCallbackSetting(3, ()=> UIPopupDiary._instance.bCanTouch = true, true);
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_23"), false, price_type: PackagePriceType.Coin,price_value: missionCoin.text);
            popupSystem.ShowBuyInfo("buyinfo_m_1");
        }
        else
        {
            OnClickSpeedUpConfirm();
        }
    }
    
    void OnClickSpeedUpConfirm()
    {
        if (coin <= Global.coin)
        {
            if ((int)GameData.User.coin > coin) usePCoin = coin;
            else if ((int)GameData.User.coin > 0)
            {
                usePCoin = (int)GameData.User.coin;
                useFCoin = coin - (int)GameData.User.coin;
            }
            else useFCoin = coin;
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            ServerAPI.ApplyMissionTime(mData.index, recvMissionTime);
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
            ManagerUI._instance.LackCoinsPopUp();
            //터치 가능하게.
            UIPopupDiary._instance.bCanTouch = true;
        }
    }

    IEnumerator DoDestroyMission()
    {
        //별 사용하는 경우에만 별 연출 재생.
        if (bUseStar == true)
        {
            float showTimer = 0;
            float scaleRatio = 0.7f;
            Vector3 startButtonPos = btnClearObject.transform.localPosition;
            NGUITools.AddChild(missionStar.gameObject, UIDiaryMission._instance.objHeartParticleStar).GetComponent<RingGlowEffect>();
            NGUITools.AddChild(missionStar.gameObject, UIDiaryMission._instance.objHeartBTNShine).GetComponent<RingGlowEffect>();
            NGUITools.AddChild(missionStar.gameObject, UIDiaryMission._instance.objHeartBTNEffect);

            //버튼애니메이션
            while (showTimer < 0.8f)
            {
                showTimer += Global.deltaTimeLobby * 4f;

                if (showTimer < 0.5f)
                {
                    scaleRatio = 0.7f + showTimer;
                }
                else
                {
                    scaleRatio = 1.8f - showTimer;
                }

                btnClearObject.transform.localPosition = new Vector3(startButtonPos.x, startButtonPos.y * (1 + (showTimer - 1) * 0.2f), startButtonPos.z);
                // startClover.cachedTransform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
                //  _objEffectButton.transform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
                yield return null;
            }
            //ManagerData._instance.userData.star -= missionData.needStar;

            //로비 상단 별 재화 업데이트.
            //LobbyUI.instance.Update_UI();
            // 

            showTimer = 0;
            while (showTimer < 0.25f)
            {
                showTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        isClearMission = false;

        //다이어리의 미션 아이템 제거 여부.
        bool bRemoveItem = true;
        float startTime = 0f;

        //단계 미션일경우 프로그레스 바 연출.
        if (mData.stepClear > 0 && (mData.stepClear != mData.clearCount))
        {
            MoveProgressBar(0.2f);
            UIDiaryMission._instance.MoveProgressBar(0.2f);
            startTime = 0.2f;
            if (mData.stepClear == mData.clearCount)
            {
                bRemoveItem = true;
            }
            else
            {
                bRemoveItem = false;
            }
        }

        //시간미션인 경우, 아직 시작 안했을 경우/ 시간 다 됐을 때 연출.
        else if (mData.waitTime > 0)
        {
            if (Global.GetTime() < mData.clearTime)
            {
                SetTimeMission(mData);
                bRemoveItem = false;
            }
            else
            {
                bRemoveItem = true;
            }
        }

        //미션 연출이 있다면 연출후에 창닫고 bSceneWakeUp 상태에 따라 씬 재생하거나 안함.
        StartCoroutine(CoAction(startTime, () =>
        {
            UIDiaryMission._instance.DoBtnMission(mData.index, missionRemoveTime, bRemoveItem);
        }));
        yield return null;
    }

    private void MoveProgressBar(float _mainDelay)
    {
        progressStep.text = string.Format("{0}/{1}", mData.clearCount, mData.stepClear);

        float targetValue = progressBar.value + (progressOffset * 0.01f);
        DOTween.To(() => progressBar.value, x => progressBar.value = x, targetValue, _mainDelay).SetEase(Ease.Flash);
    }

    private void SetTimeMission(MissionData mData)
    {
        btnSpeedUpObject.SetActive(true);
        missionCoin.text = string.Format("{0}", 100/*mData.waitCoin*/);
        btnClearObject.SetActive(false);
    }

    public IEnumerator CoMissionTimer(MissionData mData)
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            long leftTime = Global.LeftTime(mData.clearTime);
            missionTime.text = Global.GetTimeText_HHMMSS(mData.clearTime);

            if (leftTime <= 0)
            {
                break;
            }
            yield return null;
        }

        MissionTimeEnd();
    }

    public void MissionTimeEnd()
    {
        if (gameObject.activeInHierarchy == true)
        {
            StartCoroutine(CoMissionActive());
            bUseStar = false;
            missionTime.text = "00:00";
            timeObject.SetActive(false);
            btnSpeedUpObject.gameObject.SetActive(false);
            btnClearObject.gameObject.SetActive(true);
            ButtonDoNotUseStar();
        }
    }

    private IEnumerator CoMissionActive()
    {
        float stateTimer = 0f;
        float color = 70f / 255f;
        activeButton.enabled = true;
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            //터치 불가 상태일 때는 멈춤.
            if (UIDiaryMission._instance.bClearButton == true)
                break;

            stateTimer += Global.deltaTime;
            float ratio = (0.2f + Mathf.Cos(Time.time * 10f) * 0.1f);
            activeButton.color = new Color(color, color, color, ratio);
            yield return null;
        }
        yield return null;
        activeButton.enabled = false;
    }

    private void ButtonDoNotUseStar()
    {
        buttonClear.transform.localPosition = new Vector3(-100f, 0f, 0f);
        starObject.SetActive(false);
    }

    private IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }
}
