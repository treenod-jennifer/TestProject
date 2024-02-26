using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using AnimationState = Spine.AnimationState;
using Event = Spine.Event;
using Protocol;

public class UIPopupMoleCatch : UIPopupBase
{
    public static UIPopupMoleCatch _instance = null;

    public GameObject popupRoot;
    public GameObject bundleRoot;
    public UILabel popupText;

    //팝업 내에서 사용하는 패널 리스트들(리스트 순서대로 뎁스 올려줌)
    public List<UIPanel> listPanel;
    public List<UIWidget> listWidget;
    
    [HideInInspector]
    private MoleCatch moleCatchObj = null;
    private bool isNormalWave = true;

    #region 임시로 설정해둔 데이터
    private int selectStage = 1;    //현재 선택된 스테이지.
    [SerializeField]
    private int currentWave = 1;
    [SerializeField]
    private int allWaveCount = 2;
    private List<int> listAllStage = new List<int>();
    private List<int> listStage = new List<int>();

    private Dictionary<int, IMoleReward> rewards = new Dictionary<int, IMoleReward>();

    private bool isOpenActionEnd = false;
    #endregion

    #region 보상
    Protocol.AppliedRewardSet rewardSet = null;
    #endregion

    private enum MoleState
    {
        START,  //시작 시 숨어있는 상태
        APPEAR,
        HIDE,        
        HIT,
        MISS,
        APPEAR_AND_HIDE,
        TOUCH,
    }

    private enum MoleCatchState
    {
        APPEAR, //시작
        IDLE,
        CLEAR,  //스테이지 클리어
        FAIL,   //스테이지 실패
        WAVECLEAR,  //웨이브의 모든 스테이지 클리어(최종 보상 연출)
        WAVECHANGE, //다음 웨이브로 변경
        END,    //모든 스테이지 완료
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        ManagerSound._instance.StopBGM();
        Global.SetGameType_MoleCatch(ManagerMoleCatch.instance.GetEventIndex(), 1, 1);  //bgm 용 임시. 나중에 레디 들어가면 어짜피 갱신됨
        //터치 관련 막음.
        bCanTouch = false;
        InitWidget();
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;
        
        //스케일 효과만 재생
        popupRoot.transform.localScale = Vector3.one * 0.2f;
        popupRoot.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
      
        StartCoroutine(CoAction(openTime, () =>
        {
            //연출 뜨고 있는 도중에, 터치가능해지지 않도록.
            if (isOpenActionEnd == true)
            {
                bCanTouch = true;
                ManagerUI._instance.bTouchTopUI = true;
            }
            if (_callbackOpen != null)
                _callbackOpen();

            ManagerUI._instance.FocusCheck();

            ManagerSound._instance.PlayBGM(moleCatchObj.bgmClip);
        }));
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerSound._instance.StopBGM();
        Global.SetGameType_NormalGame();
        ManagerSound._instance.PlayBGM();

        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;

        //스케일 효과만 재생
        popupRoot.transform.DOScale(Vector3.zero, _mainTime).SetEase(Ease.InBack);

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(_mainTime, () =>
          PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            Destroy(gameObject);
        }));
    }

    private void InitWidget()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            //// Debug.Log(" 아이폰X 대응 " + (float)Screen.height / (float)Screen.width);
            if ((float)Screen.height / (float)Screen.width > 2f || (float)Screen.width / (float)Screen.height > 2f)
            {
                listWidget[0].topAnchor.absolute = -120;
            }
        }

        for (int i = 0; i < listWidget.Count; i++)
        {
            listWidget[i].SetAnchor(ManagerUI._instance.transform);
        }
    }

    #region 스파인, 패널들 뎁스/sortOrder 값 조정코드
    //팝업 오픈할 때 번들 미리 읽어오기, 해당 번들에서 사용하는 레이어수 읽어와서 settingSortOrder 계산
    public override void SettingSortOrder(int layer)
    {
        int startLayer = 0;
        if (layer >= 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            startLayer = layer;
        }
        else
        {
            startLayer = 10;
        }
        
        //현재 팝업에서 사용하는 패널들의 sortOrder, depth값 설정
        InitPanelDepthAndSortOrder(startLayer);

        //재화 UI 현재 팝업 위로 올려줌
        ManagerUI._instance.TopUIPanelDepth(this);
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private void InitPanelDepthAndSortOrder(int startLayer)
    {
        int startDepth = uiPanel.depth + 1;

        //맨 첫 레이어의 경우, 뒤에 배경판이 레이어 1개 위로 올라가므로 한칸 더해서 시작.
        startLayer += 1;
        //두더지 팝업의 레이어들 올려줌
        for (int i = 0; i < moleCatchObj.listSortLayerObject.Count; i++)
        {
            UIPanel panel = moleCatchObj.listSortLayerObject[i].GetComponent<UIPanel>();
            if (panel != null)
            {
                panel.sortingOrder = startLayer;
                panel.depth = startDepth;
                startLayer++;
                startDepth++;
                panelCount++;
                continue;
            }

            SkeletonAnimation spine = moleCatchObj.listSortLayerObject[i].GetComponent<SkeletonAnimation>();
            if (spine != null)
            {
                spine.GetComponent<MeshRenderer>().sortingOrder = startLayer;
                startLayer++;
                sortOrderCount++;
            }
        }

        //두더지 팝업 상단에 들어가는 레이어들 올려줌.
        for (int i = 0; i < listPanel.Count; i++)
        {
            listPanel[i].sortingOrder = startLayer;
            listPanel[i].depth = startDepth;
            startLayer++;
            startDepth++;
        }
    }
    #endregion

    public void InitPopup(GameObject bundleObject)
    {
        //이벤트 오브젝트 생성/등록
        moleCatchObj = NGUITools.AddChild(bundleRoot, bundleObject).GetComponent<MoleCatch>();
        moleCatchObj.transform.localScale = Vector3.one * 0.92f;
        moleCatchObj.transform.localPosition = new Vector3(0f, -700f, 0f);
    }

    public void InitPopup()
    {
        currentWave = ManagerMoleCatch.instance.GetWaveIndex();
        allWaveCount = ManagerMoleCatch.instance.GetWaveCount();

        // wave count가 1이하이거나, 마지막 wave가 아닐 경우 일반 웨이브.
        isNormalWave = (allWaveCount == 1 || currentWave != allWaveCount) ? true : false;

        InitPopupText();
        InitWaveText();
        InitHole();
        InitMoleCollider();
        InitMachine();
        InitClearIcon();
        InitBoard();
        InitReward();
        InitHammer();
        ActiveMachineLight(false);
        ActiveRewardLight(false);
    }

    void ApplyCurrentManagerData()
    {
        listAllStage = ManagerMoleCatch.instance.GetAllStages();
        listStage = ManagerMoleCatch.instance.GetOpenedStages();
        Debug.Log("ApplyCurrentManagerData() called: Allstage" + listAllStage.MembersToString() + "\n OpenStages: " + listStage.MembersToString());
    }

    public IEnumerator Start()
    {
        ManagerUI._instance.bTouchTopUI = false;
        this.bCanTouch = false;
        InitShaderAlphaSource();
        ApplyCurrentManagerData();
        InitPopup();

        //첫 진입검사, 처음 진입이면 처음 연출만 띄워줌.
        int firstExpressionLookedEventIndex = PlayerPrefs.GetInt("m_ev_first", 0);
        if ( ManagerMoleCatch.instance.GetEventIndex() != firstExpressionLookedEventIndex 
            && ManagerMoleCatch.instance.CheckNoPlay() )
        {
            PlayerPrefs.SetInt("m_ev_first", ManagerMoleCatch.instance.GetEventIndex());
            yield return FirstExpression();

            if (!PlayerPrefs.HasKey("TutorialMoleCatch"))
            {
                PlayerPrefs.SetInt("TutorialMoleCatch", 0);
                ManagerTutorial.PlayTutorial(TutorialType.TutorialMoleCatch);
                if (ManagerTutorial._instance != null)
                {
                    while (ManagerTutorial._instance._playing)
                        yield return new WaitForSeconds(0.1f);
                }
            }
            ManagerUI._instance.bTouchTopUI = true;
            SetMoleColliderEnabled();
            isOpenActionEnd = true;
            this.bCanTouch = true;
            yield break;
        }

        //첫 진입이 아닌데 튜토리얼 키가 없는경우, 튜토리얼 키 추가
        //(이후 두더지 이벤트가 열렸을 때 튜토리얼이 또 뜨는 경우를 방지)
        if (!PlayerPrefs.HasKey("TutorialMoleCatch"))
            PlayerPrefs.SetInt("TutorialMoleCatch", 0);

        StartCoroutine(SetMachineAnimation(MoleCatchState.IDLE));
        InitMole();
        RefreshMole();

        var nowClearedStage = ManagerMoleCatch.instance.GetNowClearedStage();
        // 두더지 게임 모두 클리어 했는지, 게임 종료연출 본 적이 없는지 검사.
        if (ManagerMoleCatch.instance.IsWaveCompleted() && ManagerMoleCatch.instance.NextWaveExist() == false
            && nowClearedStage == 0 )
        {
            moleCatchObj.midRewardLineRoot.SetActive(false);
            yield return moleCatchObj.boardCover.CoGameEnd(false);
            isOpenActionEnd = true;
            this.bCanTouch = true;
            yield break;
        }

        ManagerMoleCatch.instance.SyncClearedStageList();
        
        yield return new WaitForSeconds(0.3f);

        bool isFailAction = false;
        if (nowClearedStage != 0)
        {
            //클리어 후.
            //팝업 열리고, 좀 대기 한 뒤 연출 재생.
            yield return new WaitForSeconds(0.5f);

            selectStage = nowClearedStage;
            Debug.Log("Cleared Now : " + nowClearedStage);
            SetHammer(nowClearedStage, MoleCatchState.CLEAR);
            yield return new WaitForSeconds(2.0f);
        }
        else if (ManagerMoleCatch.lastPlayedStage != 0)
        {
            //실패 후.
            isFailAction = true;
            //팝업 열리고, 좀 대기 한 뒤 연출 재생.
            yield return new WaitForSeconds(0.5f);

            selectStage = ManagerMoleCatch.lastPlayedStage;
            SetHammer(selectStage, MoleCatchState.FAIL);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            //다른 연출 나오지 않고, 그냥 창이 열린 경우 현재 두더지 터치영역 활성화.
            SetMoleColliderEnabled();
        }

        ManagerMoleCatch.lastPlayedStage = 0;
        yield return CoProgress(nowClearedStage != 0);

        ManagerUI._instance.bTouchTopUI = true;
        //스테이지 실패했을 경우에는 bCanTouch 두더지 연출 다 끝난 뒤 풀어줘야함.
        if (isFailAction == false)
        {
            this.bCanTouch = true;
            isOpenActionEnd = true;
        }
        yield break;
    }

    IEnumerator CoProgress(bool clearedStage)
    {
        bool rewardAck = false;
        var nowGetRewards = ManagerMoleCatch.instance.GetNowCompletedRewardList();
        if (nowGetRewards.Count > 0)
        {
            Protocol.MoleCatchRewardResp resp = null;
            ServerAPI.MoleCatchReward(currentWave, ManagerMoleCatch.instance.GetEventIndex(),
                    (r) =>
                    {
                        resp = r;
                        rewardAck = true;

                        //보상 설정
                        this.rewardSet = resp.clearReward;

                        ManagerMoleCatch.LogReward(resp.rewards, ManagerMoleCatch.instance.GetEventIndex(), ManagerMoleCatch.instance.GetWaveIndex());

                        // 웨이브 클리어 최종처리가 되는 기점은 사실 보상을 받으면서 state == 2가 되기 때문에
                        // 괜히 연출중에 크래시 나거나 해서 로그가 남지 않는 문제를 막기 위해 여기서 처리
                        if ( ManagerMoleCatch.instance.IsWaveCompleted() )
                        {
                            ManagerMoleCatch.instance.LogWaveEnd();
                            if( ManagerMoleCatch.instance.NextWaveExist() == false)
                            {
                                ManagerMoleCatch.instance.LogAllWaveEnd();
                            }
                        }

                        ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();
                    });

            while (true)
            {
                if (rewardAck == false)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                break;
            }

            List<ManagerMoleCatch.RewardData> listRewardInfo = new List<ManagerMoleCatch.RewardData>();
            for (int i = 0; i < nowGetRewards.Count; ++i)
            {
                var rewardObj = rewards[nowGetRewards[i]];
                if (rewardObj != null)
                {
                    //웨이브 클리어 보상 사운드.
                    var rewInfo = ManagerMoleCatch.instance.GetRewardInfo(nowGetRewards[i]);
                    listRewardInfo.Add(rewInfo);
                    if (rewInfo.isWaveReward == true)
                    {
                        ManagerSound.AudioPlay(AudioLobby.event_mole_fanfare);
                        StartCoroutine(SetMachineAnimation(MoleCatchState.WAVECLEAR));
                     
                        //점선제거.
                        moleCatchObj.midRewardLineRoot.SetActive(false);
                        yield return rewardObj.CoAppear();
                        StartCoroutine(SetMachineAnimation(MoleCatchState.IDLE));
                        if (ManagerMoleCatch.instance.NextWaveExist() == false)
                        {
                            ActiveMachineLight(false);
                            ActiveRewardLight(false);
                        }
                    }
                    else
                    {
                        ManagerSound.AudioPlay(AudioLobby.HEART_SHORTAGE);
                    }

                    //보상 사라지는 연출
                    yield return rewardObj.CoDisappear();

                    this.rewards.Remove(nowGetRewards[i]);
                }
                yield return new WaitForSeconds(.2f);
            }

            //보상 팝업 띄우기
            if (rewardSet != null)
            {
                bool isGetReward = false;
                ManagerUI._instance.OpenPopupGetRewardAlarm
                    (Global._instance.GetString("p_mc_4"),
                    () => { isGetReward = true; },
                    rewardSet);

                //보상 팝업 종료될 때까지 대기.
                yield return new WaitUntil(() => isGetReward == true);
            }

            //보상 UI 업데이트
            for (int i = 0; i < listRewardInfo.Count; ++i)
            {
                var rewInfo = listRewardInfo[i];
                RewardHelper.ProcessTopUIFakeupdate(rewInfo.reward.type, rewInfo.reward.value);
            }

            if (resp != null)
                ManagerMoleCatch.OnGetRewardList(resp); // 여기에 ui 업데이트 최종적인 내용이 있으므로 위에서 페이크로 한번 보여주고나서 최종정리
        }

        ManagerMoleCatch.instance.SyncRewardList();

        if (ManagerMoleCatch.instance.IsWaveCompleted())
        {
            if (ManagerMoleCatch.instance.NextWaveExist())
            {
                bCanTouch = false;

                //정보 갱신.
                ManagerMoleCatch.instance.SyncWave();
                this.ApplyCurrentManagerData();
                this.currentWave = ManagerMoleCatch.instance.GetWaveIndex();
                isNormalWave = (allWaveCount == 1 || currentWave != allWaveCount) ? true : false;
               
                //웨이브 변경되었을 때 연출.
                yield return CoWaveChange();
            }
            else
            {
                yield return moleCatchObj.boardCover.CoGameEnd(clearedStage);
                ManagerMoleCatch.instance.SyncClearedStageList();
                ManagerMoleCatch.instance.SyncUpMoles();
                this.ApplyCurrentManagerData();
                bCanTouch = true;
            }
        }
    }

    private IEnumerator FirstExpression()
    {
        InitMole();
        StartCoroutine(SetMachineAnimation(MoleCatchState.APPEAR));

        //숫자 텍스트 없애기.
        StartCoroutine(SetBoardTextActive(false));
        yield return this.moleCatchObj.boardCover.CoInitMachine();
        yield return new WaitForSeconds(0.0f);

        //스타트
        Color textColor = (isNormalWave == true) ? moleCatchObj.textColor_1 : moleCatchObj.textColor_2;
        yield return this.moleCatchObj.boardCover.CoStartText(currentWave, textColor);

        //두더지 셔플연출.
        yield return ShuffleMole();
        yield return new WaitForSeconds(0.5f);

        this.listStage = ManagerMoleCatch.instance.GetOpenedStages();
        for (int i = 0; i < listStage.Count; i++)
        {
            SetMoleAnimation(listStage[i], MoleState.APPEAR);
        }
        SetMoleSound(MoleState.APPEAR);

        this.moleCatchObj.boardCover.SetActiveStartText(false);
        //보드 숫자 텍스트 켜지는 연출.
        yield return SetBoardTextActive(true, true);
        yield break;
    }
    
    private IEnumerator ShuffleMole()
    {
        List<int> expArr = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        expArr.Shuffle();

        List<float> delayArr = new List<float> { 0.2f, 0.2f, 0f, 0f, 0.1f, 0.1f, 0.05f, 0.1f, 0.05f };
        delayArr.Shuffle();
        float del = 0.2f;
        for (int i = 0; i < expArr.Count; i++)
        {
            SetMoleAnimation(expArr[i], MoleState.APPEAR);
            SetMoleSound(MoleState.APPEAR);
            del *= 0.9f;
            yield return new WaitForSeconds(delayArr[i]);
        }
        yield return new WaitForSeconds(0.4f);

        SetMoleAnimation(MoleState.HIDE);
        SetMoleSound(MoleState.HIDE);
    }

    private void RefreshPopup()
    {
        InitHole();
    }

    //클리어 연출 후, 보드판 새로 재정렬하는 부분.
    private IEnumerator CoRefreshBoard()
    {
        //남아 있는 두더지 숨김.
        for (int i = 0; i < listStage.Count; i++)
        {
            SetMoleAnimation(listStage[i], MoleState.HIDE);
        }
        if (listStage.Count > 0)
        {
            //ActiveMachineLight(false);
            SetMoleSound(MoleState.HIDE);
            yield return new WaitForSeconds(0.5f);

            //처음 오픈 시 뜨는 연출 들이 다 끝난 경우에만 두더지 나왔을 때 터치 활성화해줌.
            if (isOpenActionEnd == true)            
                bCanTouch = true;
        }

        if( touchState == MoleCatchTouchState.SELECT)
        {
            Debug.Log("CoRefreshBoard:  AllStage" + listAllStage.MembersToString() + "\n OpenStages: " + listStage.MembersToString());

            ManagerMoleCatch.instance.SyncUpMoles();

            this.ApplyCurrentManagerData();
        }
        RefreshMole();
        SetMoleColliderEnabled();
    }

    private IEnumerator CoWaveChange()
    {
        ActiveMachineLight(false);
        ActiveRewardLight(false);
        ManagerSound.AudioPlay(AudioLobby.event_mole_turnup);

        //보드판 새로 생성, 텍스쳐 알파 쉐이더 다 같은걸로 설정(마지막 텍스쳐로), 텍스트 안보이게 설정.
        InitBoard();
        SetShaderWaveClearAlphaSource();
        StartCoroutine(SetBoardTextActive(false));
        
        //기계 스킨 변경.
        RefreshPopup();
        StartCoroutine(WaveChangeAction());

        //보드 관련 연출.
        yield return moleCatchObj.boardCover.CoOnScreen();
        InitReward();
        moleCatchObj.midRewardLineRoot.SetActive(true);
        InitWaveText();

        //스타트 텍스트 연출.
        Color textColor = (isNormalWave == true) ? moleCatchObj.textColor_1 : moleCatchObj.textColor_2;
        yield return this.moleCatchObj.boardCover.CoStartText(currentWave, textColor);

        //클리어 아이콘 갱신.
        InitClearIcon(true);

        if (this.touchState == MoleCatchTouchState.SELECT)
        {
            ApplyCurrentManagerData();

            for (int i = 0; i < listAllStage.Count; ++i)
            {
                SetMoleMode(listAllStage[i], ManagerMoleCatch.instance.IsHardStage(listAllStage[i]));
            }
        }
        else
        {
            //더미
            listAllStage = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        //두더지 셔플연출.
        yield return ShuffleMole();
        yield return new WaitForSeconds(0.5f);
        
        //스타트 글자 없애줌, 숫자 텍스트 보이도록 설정.
        this.moleCatchObj.boardCover.SetActiveStartText(false);
        yield return SetBoardTextActive(true, true);
        
        //두더지 재정렬
        RefreshMole();
        SetMoleColliderEnabled();
        bCanTouch = true;
    }

    private void InitPopupText()
    {
        popupText.text = Global._instance.GetString("p_mc_1");
    }

    private void InitWaveText()
    {
        moleCatchObj.waveText.text = string.Format("WAVE {0}/{1}", currentWave, allWaveCount);
    }

    #region 두더지 홀 관련
    private void InitHole()
    {   
        for (int i=0; i<moleCatchObj.listMoleCatchHole.Count; i++)
        {
            Texture texture = (isNormalWave == true) ? 
                moleCatchObj.listMoleCatchHole[i]._holeNormal : moleCatchObj.listMoleCatchHole[i]._holedHard;
            moleCatchObj.listMoleCatchHole[i].hole.mainTexture = texture;
        }
    }
    #endregion

    #region 기계 관련
    private void InitMachine()
    {
        string name = (isNormalWave == true) ? "1" : "2";
        moleCatchObj.spineMachine.skeleton.SetSkin(name);
        moleCatchObj.spineMachine.skeleton.SetSlotsToSetupPose();
        moleCatchObj.spineMachine.state.Event += OnEventMachine;
        moleCatchObj.spineMachine.state.Complete += OnCompletedMachine;
    }

    private void OnEventMachine(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (trackEntry.Animation.Name == "2_appear")
        {   
            if (e.Data.Name == "led")
            {
                ActiveMachineLight(true);
                ActiveRewardLight(true);
            }
        }
    }

    /// <summary>
    /// 머신 애니메이션 상태가 변한 뒤, 보드/두더지 판 재정렬 해주는 함수들 호출.
    /// </summary>
    /// <param name="trackEntry"></param>
    private void OnCompletedMachine(Spine.TrackEntry trackEntry)
    {
        switch (trackEntry.Animation.Name)
        {   
            case "1_appear":
                {
                    //스테이지 클리어 할 때 애니메이션
                    //두더지 재정렬
                    ActiveMachineLight(false);
                    StartCoroutine(CoRefreshBoard());
                }
                break;

            case "3_appear":
                {
                    //웨이브 변하는 연출 애니메이션
                    //기계 스킨 변경()
                    moleCatchObj.spineMachine.skeleton.SetSkin("2");
                    moleCatchObj.spineMachine.skeleton.SetSlotsToSetupPose();
                    moleCatchObj.spineMachine.state.SetAnimation(0, "0_idle", true);
                    moleCatchObj.spineMachine.Update(0f);
                }
                break;
        }
    }

    private IEnumerator SetMachineAnimation(MoleCatchState state)
    {
        bool aniEnded = false;
        switch (state)
        {
            case MoleCatchState.APPEAR:
                moleCatchObj.spineMachine.state.SetAnimation(0, "0_appear", false);
                moleCatchObj.spineMachine.state.AddAnimation(0, "0_idle", true, 0f);
                break;
            case MoleCatchState.IDLE:
                moleCatchObj.spineMachine.state.SetAnimation(0, "0_idle", true);
                break;
            case MoleCatchState.CLEAR:
                moleCatchObj.spineMachine.state.SetAnimation(0, "1_appear", false);
                moleCatchObj.spineMachine.state.AddAnimation(0, "0_idle", true, 0f);
                break;
            case MoleCatchState.WAVECLEAR:
                moleCatchObj.spineMachine.state.SetAnimation(0, "2_appear", false);
                moleCatchObj.spineMachine.state.AddAnimation(0, "2_idle", true, 0f);
                break;
            case MoleCatchState.WAVECHANGE:
                if (isNormalWave == false)
                    moleCatchObj.spineMachine.state.SetAnimation(0, "3_appear", false);
                else
                    moleCatchObj.spineMachine.state.SetAnimation(0, "0_idle", true);
                break;
            case MoleCatchState.END:
                moleCatchObj.spineMachine.state.SetAnimation(0, "0_idle", false);
                break;
        }
        Spine.AnimationState.TrackEntryDelegate d = (t) => { aniEnded = true; };
        moleCatchObj.spineMachine.state.End += d;

        while( aniEnded == false )
        {
            yield return new WaitForSeconds(0.01f);
        }

        moleCatchObj.spineMachine.state.End -= d;
    }
    #endregion

    #region 클리어 아이콘 관련
    private void InitClearIcon(bool isAction = false)
    {
        //이미 클리어 한 스테이지에 클리어 아이콘 표시.
        var clearedStages = ManagerMoleCatch.instance.GetClearedStages();
        for (int i = 0; i < listAllStage.Count; i++)
        {
            if (clearedStages.FindIndex(x => x == listAllStage[i]) == -1)
            {
                ShowClearIcon(listAllStage[i], false, isAction);
                continue;
            }
            ShowClearIcon(listAllStage[i], true, isAction);
        }
    }

    private void ShowClearIcon(int stageIdx, bool isShow, bool isAction = false)
    {
        int moleIndex = moleCatchObj.listMole.FindIndex(x => x.stageIndex == stageIdx);
        if (moleIndex < 0)
            return;

        UITexture texture = moleCatchObj.listMole[moleIndex].clearIcon;
        
        //연출이 없을 경우 바로 종료.
        if (isAction == false)
        {
            float targetAlpha = (isShow == true) ? 1f : 0f;
            texture.color = new Color(1f, 1f, 1f, targetAlpha);
            return;
        }

        if (isShow == true)
        {
            StartCoroutine(CoClearShowAction(texture));
        }
        else
        {
            StartCoroutine(CoClearHideAction(texture));
        }
    }

    private IEnumerator CoClearShowAction(UITexture texture)
    {
        float targetY = texture.transform.localPosition.y;
        texture.color = new Color(1f, 1f, 1f, 0f);
        texture.transform.localPosition = new Vector3(texture.transform.localPosition.x, texture.transform.localPosition.y - 20f, 0f);
        texture.transform.localScale = Vector3.one * 0.3f;
        texture.transform.DOLocalMoveY(targetY, 0.3f).SetEase(Ease.OutBack);
        texture.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        DOTween.ToAlpha(() => texture.color, x => texture.color = x, 1f, 0.3f);
        yield return new WaitForSeconds(0.25f);
        texture.transform.DOShakePosition(0.3f, 8f, 20, 90f, false, false);
        ManagerSound.AudioPlay(AudioLobby.event_mole_star2);
    }

    private IEnumerator CoClearHideAction(UITexture texture)
    {
        Vector3 pos = texture.transform.localPosition;
        texture.color = new Color(1f, 1f, 1f, 1f);
        texture.transform.DOLocalMoveY(texture.transform.localPosition.y + 50f, 0.5f);
        DOTween.ToAlpha(() => texture.color, x => texture.color = x, 0f, 0.4f).SetEase(Ease.InQuint);
        yield return new WaitForSeconds(0.5f);
        //웨이브 이동 후, 보너스 스테이지 클릭할 수도 있으니 클리어 아이콘 원래 위치로 돌려줌.
        texture.transform.localPosition = pos;
    }
    #endregion

    #region 두더지 관련
    //두더지 초기화 함수.
    private void InitMole()
    {
        //두더지 터치했을 때 실행될 콜백 등록, 스파인 설정.
        for (int i = 0; i < moleCatchObj.listMole.Count; i++)
        {
            moleCatchObj.listMole[i].spineMole.state.Complete += OnCompletedMole;

            moleCatchObj.listMole[i].InitItemMoleCatch(OnClickItemMole);
        }

        for(int i = 0; i < listAllStage.Count; ++i)
        {
            SetMoleMode(listAllStage[i], ManagerMoleCatch.instance.IsHardStage(listAllStage[i]));
        }

        //모든 두더지 아래에 있는 상태에서 시작.
        SetMoleAnimation(MoleState.START);
    }

    private void InitMoleCollider()
    {
        //두더지 터치 콜라이더 비활성화.
        for (int i = 0; i < moleCatchObj.listMole.Count; i++)
        {
            moleCatchObj.listMole[i].moleCollider.enabled = false;
        }
    }

    private void SetMoleColliderEnabled()
    {
        //현재 선택가능한 스테이지의 두더지 터치 콜라이더 활성화.
        for (int i = 0; i < listStage.Count; i++)
        {
            Debug.Log("stage : " + listStage[i]);
            int moleIndex = moleCatchObj.listMole.FindIndex(x => x.stageIndex == listStage[i]);
            if (moleIndex < 0)
                return;

            moleCatchObj.listMole[moleIndex].moleCollider.enabled = true;
        }
    }

    private void OnCompletedMole(Spine.TrackEntry trackEntry)
    {
        string name = (isNormalWave == true) ? "" : "_HARD";
        if (trackEntry.Animation.Name == "HIT" + name)
        {
            ShowClearIcon(selectStage, true, true);
            ClearBoard(selectStage, true);
            ManagerSound.AudioPlay(AudioLobby.event_mole_stage_clear);
        }
        else if (trackEntry.Animation.Name == "MISS" + name)
        {
            StartCoroutine(FailAction_3());
        }
    }

    private void RefreshMole()
    {   
        if( touchState != MoleCatchTouchState.SELECT)
        {
            //더미로 스테이지 3개 선택
            SetRandomStage();
        }

        AppearMole();
    }

    //두더지 등장하는 함수.
    private void AppearMole()
    {
        string refreshedMoleIdxString = "";
        //두더지 스파인.
        for (int i = 0; i < listStage.Count; i++)
        {
            refreshedMoleIdxString += listStage[i].ToString() + ", ";
            SetMoleAnimation(listStage[i], MoleState.APPEAR);
        }
        SetMoleSound(MoleState.APPEAR);

        Debug.Log("RefreshMole Called: " + refreshedMoleIdxString);
    }


    //일단 더미로 랜덤한 스테이지 설정해주는 함수.
    private void SetRandomStage()
    {
        listStage.Clear();

        if (listAllStage.Count > 3)
        {
            List<int> listTemp = new List<int>(listAllStage);
            for (int i = 0; i < 3; i++)
            {
                int randIndex = Random.Range(0, listTemp.Count);
                listStage.Add(listTemp[randIndex]);
                listTemp.Remove(listTemp[randIndex]);
            }
        }
        else
        {
            listStage = new List<int>(listAllStage);
        }
    }

    //두더지 클릭했을 때 실행될 함수.
    private void OnClickItemMole(int stageIndex)
    {
        if (bCanTouch == false)
            return;
        //현재 선택 불가능한 스테이지라면 터치 입력 받지않음.
        if (listStage.FindIndex(x => x == stageIndex) == -1)
            return;

        bCanTouch = false;
        selectStage = stageIndex;

        CoClickAction();
    }

    private void CoClickAction()
    {
        SetMoleAnimation(selectStage, MoleState.TOUCH);
        if (ManagerMoleCatch.instance.IsFreeStage(selectStage))
        {
            StartCoroutine(BonusStage(selectStage));
        }
        else
        {
            //더미로 넣어둠.
            if (touchState == MoleCatchTouchState.SELECT)
            {
                OpenStage(selectStage);
            }
            else if (touchState == MoleCatchTouchState.CLEAR)
            {
                bCanTouch = true;
                SetHammer(selectStage, MoleCatchState.CLEAR);
            }
            else
            {
                bCanTouch = true;
                SetHammer(selectStage, MoleCatchState.FAIL);
            }
        }
    }

    //모든 두더지의 애니메이션을 함께 변경
    private void SetMoleAnimation(MoleState state)
    {
        for (int i = 0; i < moleCatchObj.listMole.Count; i++)
        {
            MoleAnimation(i, state);
        }
    }

    private void SetMoleMode(int stageIdx, bool hardMode)
    {
        int moleIndex = moleCatchObj.listMole.FindIndex(x => x.stageIndex == stageIdx);
        if (moleIndex < 0)
            return;

        moleCatchObj.listMole[moleIndex].spineMole.skeleton.SetSkin(hardMode == false ? "1": "2");
        moleCatchObj.listMole[moleIndex].spineMole.skeleton.SetSlotsToSetupPose();
        moleCatchObj.listMole[moleIndex].spineMole.Update(0f);
    }

    //특정 스테이지 인덱스를 가지는 두더지의 애니메이션을 변경
    private void SetMoleAnimation(int stageIdx, MoleState state)
    {
        int moleIndex = moleCatchObj.listMole.FindIndex(x => x.stageIndex == stageIdx);
        if (moleIndex < 0)
            return;

        MoleAnimation(moleIndex, state);
    }

    private void MoleAnimation(int moleIndex, MoleState state)
    {
        string name = (isNormalWave == true) ? "" : "_HARD";
        switch (state)
        {
            case MoleState.START:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "DOWN_LOOP", true);
                break;
            case MoleState.APPEAR:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "UP" + name, false);
                moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "UP_LOOP" + name, true, 0f);
                break;
            case MoleState.HIDE:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "DOWN" + name, false);
                moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "DOWN_LOOP", true, 0f);
                break;
            case MoleState.HIT:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "HIT" + name, false);
                break;
            case MoleState.MISS:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "MISS" + name, false);
                //moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "UP_LOOP" + name, true, 0f);
                break;
            case MoleState.APPEAR_AND_HIDE:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "UP" + name, false);                    
                moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "DOWN" + name, false, 0.3f);
                moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "DOWN_LOOP", true, 0f);
                break;
            case MoleState.TOUCH:
                moleCatchObj.listMole[moleIndex].spineMole.state.SetAnimation(0, "TOUCH" + name, false);
                moleCatchObj.listMole[moleIndex].spineMole.state.AddAnimation(0, "UP_LOOP" + name, true, 0f);
                break;
        }
    }

    private void SetMoleSound(MoleState state)
    {
        switch (state)
        {
            case MoleState.APPEAR:
            case MoleState.APPEAR_AND_HIDE:
                ManagerSound.AudioPlay(AudioLobby.event_mole_up);
                break;
            case MoleState.HIDE:
                ManagerSound.AudioPlay(AudioLobby.event_mole_down);
                break;
            case MoleState.HIT:
                if (moleCatchObj.hitVoiceClip == null)
                    AudioPlay(moleCatchObj.hitVoice);
                else
                    ManagerSound.AudioPlay(moleCatchObj.hitVoiceClip);
                break;
            case MoleState.MISS:
                if (moleCatchObj.missVoiceClip == null)
                    AudioPlay(moleCatchObj.missVoice);
                else
                    ManagerSound.AudioPlay(moleCatchObj.missVoiceClip);
                break;
        }
    }
    #endregion

    private void AudioPlay(AudioLobby audio)
    {
        if (audio != AudioLobby.NO_SOUND)
            ManagerSound.AudioPlay(audio);
    }

    #region 보드 관련
    private void InitBoard()
    {
        //클리어 한 보드는 보드 이미지 보이지 않도록 설정
        var clearedStages = ManagerMoleCatch.instance.GetClearedStages();
        for (int i = 0; i < listAllStage.Count; i++)
        {
            if (clearedStages.FindIndex(x => x == listAllStage[i]) > -1)
            {
                this.ClearBoard(listAllStage[i], false);
            }
        }

        //보드 컬러 설정
        for (int i = 0; i < moleCatchObj.listBoardTexture.Count; i++)
        {
            if (clearedStages.FindIndex(x => x == (i + 1)) > -1)
                continue;

            //홀수 - 흰색, 짝수보드 - 컬러
            if (i > 0 && (i + 1) % 2 == 0)
            {
                if (isNormalWave == true)
                    moleCatchObj.listBoardTexture[i].color = moleCatchObj.boardColor_1;
                else
                    moleCatchObj.listBoardTexture[i].color = moleCatchObj.boardColor_2;
            }
            else
            {
                moleCatchObj.listBoardTexture[i].color
                    = new Color(moleCatchObj.listBoardTexture[i].color.r, moleCatchObj.listBoardTexture[i].color.g, moleCatchObj.listBoardTexture[i].color.b, 1f);
            }
        }

        //텍스트 컬러 설정
        for (int i = 0; i < moleCatchObj.listStageText.Count; i++)
        {
            if (clearedStages.FindIndex(x => x == (i + 1)) > -1)
                continue;

            if (isNormalWave == true)
                moleCatchObj.listStageText[i].color = moleCatchObj.textColor_1;
            else
                moleCatchObj.listStageText[i].color = moleCatchObj.textColor_2;
        }

        //보너스 텍스트 설정.
        for (int i = 0; i < moleCatchObj.listBonusText.Count; i++)
        {
            moleCatchObj.listBonusText[i].gameObject.SetActive(false);
            if (isNormalWave == true)
                moleCatchObj.listBonusText[i].color = moleCatchObj.bonusTextColor_1;
            else
                moleCatchObj.listBonusText[i].color = moleCatchObj.bonusTextColor_2;
        }
    }

    private void InitShaderAlphaSource()
    {
        for (int i = 0; i < moleCatchObj.listBoardTexture.Count; i++)
        {
            moleCatchObj.listBoardTexture[i].mainTexture  = moleCatchObj.boardAlphaSource[Random.Range(0, moleCatchObj.boardAlphaSource.Length)];
        }
    }

    private void SetShaderWaveClearAlphaSource()
    {
        for (int i = 0; i < moleCatchObj.listBoardTexture.Count; i++)
        {
            moleCatchObj.listBoardTexture[i].mainTexture = moleCatchObj.waveBoardClearAlphaSource;
        }
    }

    private void InitReward()
    {
        if( this.rewards.Count > 0 )
        {
            for(int i = 0; i < this.rewards.Count; ++i)
            {
                this.rewards[i].SelfDestroy();
            }
            this.rewards.Clear();
        }

        var rewards = ManagerMoleCatch.instance.GetIncompletedRewards();
        for (int i = 0; i < rewards.Count; ++i)
        {
            var rewInfo = ManagerMoleCatch.instance.GetRewardInfo(rewards[i]);

            if( rewInfo.isWaveReward )
            {
                var newReward = Instantiate(this.moleCatchObj.finalRewardImage.gameObject);
                newReward.gameObject.SetActive(true);

                newReward.transform.parent = moleCatchObj.finalRewardImage.transform.parent;
                newReward.transform.localPosition = moleCatchObj.finalRewardImage.transform.localPosition;
                newReward.transform.localScale = moleCatchObj.finalRewardImage.transform.localScale;
                newReward.transform.localRotation = moleCatchObj.finalRewardImage.transform.localRotation;

                var waveRew = newReward.AddMissingComponent<UIItemMoleReward_Wave>();
                waveRew.waveIndex = ManagerMoleCatch.instance.GetWaveIndex();
                this.rewards.Add(rewards[i], waveRew);
            }
            else
            {
                var newReward = Instantiate(this.moleCatchObj.midRewardBase);
                newReward.gameObject.SetActive(true);
                newReward.transform.parent = moleCatchObj.midRewardRoot.transform;

                var pos = GetRewardPosition(rewInfo.requiredStages);

                newReward.transform.localPosition = pos;
                newReward.transform.localScale = Vector3.one;
                newReward.transform.localRotation = new Quaternion();

                var rewObj = newReward.gameObject.AddMissingComponent<UIItemMoleReward_Mid>();
                rewObj.reward = newReward.GetComponentInChildren<GenericReward>();
                rewObj.reward.scale = 1.2f;
                rewObj.reward.SetReward(rewInfo.reward);

                this.rewards.Add(rewards[i], rewObj);
            }
        }
    }

    Vector3 GetRewardPosition(List<int> reqStages)
    {
        if (reqStages.Count == 0)
            return Vector3.zero;

        Vector3 pos = Vector3.zero;
        for(int i = 0; i < reqStages.Count; ++i)
        {
            pos += moleCatchObj.listBoardTexture[reqStages[i] - 1].transform.localPosition;
        }

        pos /= reqStages.Count;
        return pos;
    }

    private IEnumerator SetBoardTextActive(bool isActive, bool isAction = false)
    {
        if (isAction == false)
        {
            //텍스트 컬러 설정
            for (int i = 0; i < moleCatchObj.listStageText.Count; i++)
            {
                moleCatchObj.listStageText[i].gameObject.SetActive(isActive);
            }
        }
        else
        {
            if (isActive == true)
            {
                //텍스트 컬러 설정
                for (int i = 0; i < moleCatchObj.listStageText.Count; i++)
                {
                    UITexture texture = moleCatchObj.listStageText[i];
                    texture.color = new Color(texture.color.r, texture.color.g, texture.color.b, 0f);
                    texture.gameObject.SetActive(isActive);
                    DOTween.ToAlpha(() => texture.color, x => texture.color = x, 1f, 0.2f);
                }
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                //텍스트 컬러 설정
                for (int i = 0; i < moleCatchObj.listStageText.Count; i++)
                {
                    UITexture texture = moleCatchObj.listStageText[i];
                    texture.color = new Color(texture.color.r, texture.color.g, texture.color.b, 1f);
                    DOTween.ToAlpha(() => texture.color, x => texture.color = x, 0f, 0.2f);
                }
                yield return new WaitForSeconds(0.2f);

                for (int i = 0; i < moleCatchObj.listStageText.Count; i++)
                {
                    moleCatchObj.listStageText[i].gameObject.SetActive(isActive);
                }
            }
        }
    }

    /// <summary>
    /// 보드 연출관련된 함수
    /// </summary>
    /// <param name="stageIdx"></param>
    /// 연출을 넣을 보드의 스테이지 인덱스
    /// <param name="bAction"></param>
    /// true - 알파로 사라지는 연출/ false - 연출 없이 알파값 바로 적용
    private void ClearBoard(int stageIdx, bool bAction = false, float actionTime = 0.2f)
    {
        UITexture board = moleCatchObj.listBoardTexture[stageIdx - 1];
        if (bAction == true)
        {
            DOTween.ToAlpha(() => board.color, x => board.color = x, 0f, actionTime);
        }
        else
        {
            board.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    /// <summary>
    /// 보너스 스테이지 보드 연출
    /// </summary>
    /// <param name="stageIdx"></param>
    private IEnumerator CoBonusBoard(int stageIdx)
    {
        //보너스 텍스트.
        UITexture bonusText = moleCatchObj.listBonusText[stageIdx - 1];
        bonusText.transform.localScale = Vector3.zero;
        bonusText.color = new Color(bonusText.color.r, bonusText.color.g, bonusText.color.b, 0f);

        //숫자 텍스트 없애고 lucky 텍스트 띄우기
        UITexture boardText = moleCatchObj.listStageText[stageIdx - 1];
        DOTween.ToAlpha(() => boardText.color, x => boardText.color = x, 0f, 0.1f);
        yield return new WaitForSeconds(0.1f);

        //보너스 텍스트 연출
        bonusText.gameObject.SetActive(true);
        bonusText.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
        DOTween.ToAlpha(() => bonusText.color, x => bonusText.color = x, 1f, 0.1f);
    }
    #endregion

    #region 뿅망치 관련
    private void InitHammer()
    {
        moleCatchObj.spineHammer.state.Event += OnEventHammer;
        moleCatchObj.spineHammer.state.Complete += OnCompletedHammer;
        moleCatchObj.hammerObj.SetActive(false);
    }

    private void OnEventHammer(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (trackEntry.Animation.Name == "HIT")
        {
            if (e.Data.Name == "hit")
            {
                ClearAction();
            }
        }
        else if (trackEntry.Animation.Name == "MISS")
        {
            if (e.Data.Name == "hit")
            {
                FailAction_1();
            }
            else if (e.Data.Name == "on")
            {
                FailAction_2();
            }
        }
    }

    private void OnCompletedHammer(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == "HIT")
        {
            moleCatchObj.hammerObj.SetActive(false);
        }
        else if (trackEntry.Animation.Name == "MISS")
        {
            moleCatchObj.hammerObj.SetActive(false);
        }
    }

    private void SetHammer(int stageIndex, MoleCatchState state)
    {
        int moleIndex = moleCatchObj.listMole.FindIndex(x => x.stageIndex == stageIndex);
        if (moleIndex == -1)
            return;

        moleCatchObj.hammerObj.transform.position = moleCatchObj.listMole[moleIndex].transform.position;
        moleCatchObj.spineHammer.GetComponent<MeshRenderer>().sortingOrder
            = moleCatchObj.listMole[moleIndex].spineMole.GetComponent<MeshRenderer>().sortingOrder;

        HammerAnimation(state);
    }

    private void HammerAnimation(MoleCatchState state)
    {
        moleCatchObj.hammerObj.SetActive(true);
        switch (state)
        {
            case MoleCatchState.CLEAR:
                moleCatchObj.spineHammer.state.SetAnimation(0, "HIT", false);
                HammerHitSound(true);
                break;
            case MoleCatchState.FAIL:
                moleCatchObj.spineHammer.state.SetAnimation(0, "MISS", false);
                HammerHitSound(false);
                break;
        }
    }

    private void HammerHitSound(bool isClear)
    {
        if (isClear == true)
        {
            //뿅망치사운드
            if (ManagerMoleCatch.instance.IsHardStage(selectStage) == false)
                ManagerSound.AudioPlay(AudioLobby.event_mole__Hit_clear_normal);
            else
                ManagerSound.AudioPlay(AudioLobby.event_mole__Hit_clear_hard);
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.event_mole_hit_fail);
        }
    }

    #endregion

    private void ClearAction()
    {
        ActiveMachineLight(true);
        ManagerSound.AudioPlay(AudioLobby.event_mole_lightning);

        ShowHitEffect();
        if (ManagerMoleCatch.instance.IsFreeStage(selectStage))
        {
            StartCoroutine(CoBonusBoard(selectStage));
            ShowBonusEffect();
        }

        //두더지 애니메이션
        SetMoleAnimation(selectStage, MoleState.HIT);
        SetMoleSound(MoleState.HIT);

        listStage.Remove(selectStage);
        if (touchState != MoleCatchTouchState.SELECT)
        {
            //더미 데이터
            listAllStage.Remove(selectStage);

            //남은 스테이지 없는 경우, 최종 보상 연출
            if (listAllStage.Count == 0)
            {
                if (currentWave != allWaveCount)
                {
                    StartCoroutine(CoRewardAction());
                    ManagerSound.AudioPlay(AudioLobby.event_mole_fanfare);
                }
            }
            else
            {
                StartCoroutine(SetMachineAnimation(MoleCatchState.CLEAR));
            }
        }
        else
        {
            StartCoroutine(SetMachineAnimation(MoleCatchState.CLEAR));
        }
    }

    private IEnumerator CoRewardAction()
    {
        ActiveMachineLight(true);

        // 보상 연출이 들어가야 함.

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator WaveChangeAction()
    {
        yield return StartCoroutine(SetMachineAnimation(MoleCatchState.WAVECHANGE));
    }

    private void FailAction_1()
    {
        for (int i = 0; i < listStage.Count; i++)
        {
            SetMoleAnimation(listStage[i], MoleState.HIDE);
        }
        SetMoleSound(MoleState.HIDE);
    }

    private void FailAction_2()
    {
        SetMoleAnimation(selectStage, MoleState.MISS);
        SetMoleSound(MoleState.MISS);
    }

    private IEnumerator FailAction_3()
    {
        //두더지 miss 연출 끝난뒤, 숨었다가 두더지들 다같이 나오는 연출.
        SetMoleAnimation(selectStage, MoleState.HIDE);
        SetMoleSound(MoleState.HIDE);
        yield return new WaitForSeconds(0.5f);
        AppearMole();
        yield return new WaitForSeconds(0.2f);
        SetMoleColliderEnabled();
        this.bCanTouch = true;
        isOpenActionEnd = true;
    }

    private void OpenStage(int stageIndex)
    {
        ManagerMoleCatch.instance.OpenReady(stageIndex, () => { this.bCanTouch = true; });
    }

    private IEnumerator BonusStage(int stageIndex)
    {
        if ( touchState == MoleCatchTouchState.SELECT )
        {
            ManagerUI._instance.bTouchTopUI = false;
            InitMoleCollider();
            var arg = new GameClearReq()
            {
                stage = stageIndex,
                eventIdx = ManagerMoleCatch.instance.GetEventIndex(),
                chapter = ManagerMoleCatch.instance.GetWaveIndex(),
                type = (int)GameType.MOLE_CATCH,
                stageKey = Global.GameInstance.GetStageKey(),
                seed = ServerRepos.IngameSeed,
                easyMode = 0,
            };
            bool ack = false;
            ServerAPI.GameClear(arg, (resp) => { ack = true; });
            Global.SetIsClear(true);

            while (!ack)
            {
                yield return new WaitForSeconds(0.1f);
            }
            SetHammer(stageIndex, MoleCatchState.CLEAR);

            yield return new WaitForSeconds(2f);

            ManagerMoleCatch.instance.SyncClearedStageList();

            yield return CoProgress(true);
            ManagerUI._instance.bTouchTopUI = true;
        }
        yield break;
    }

    #region 이펙트
    private void ShowHitEffect()
    {
        //때리는 이펙트
        GameObject obj = NGUITools.AddChild(moleCatchObj.effectRoot.gameObject, moleCatchObj._objEffectHit);
        ParticleSystem[] arrayEffect = obj.GetComponentsInChildren<ParticleSystem>();

        //이펙트 최상단으로 올려주기
        int sortOrder = uiPanel.useSortingOrder == false ? (10 + sortOrderCount + panelCount) : uiPanel.sortingOrder + sortOrderCount + panelCount;
        for (int i = 0; i < arrayEffect.Length; i++)
        {
            arrayEffect[i].GetComponent<Renderer>().sortingOrder = sortOrder;
        }
        obj.transform.position = moleCatchObj.hammerObj.gameObject.transform.position;
        obj.transform.localPosition += moleCatchObj.hitEfOffset;
    }

    private void ShowBonusEffect()
    {
        //보너스 때렸을 때 이벤트.
        GameObject obj = NGUITools.AddChild(moleCatchObj.effectRoot.gameObject, moleCatchObj._objEffectBonus);
        obj.transform.position = moleCatchObj.bonusEffectTr.position;

        ParticleSystem[] arrayEffect = obj.GetComponentsInChildren<ParticleSystem>();
        //이펙트 최상단으로 올려주기
        int sortOrder = uiPanel.useSortingOrder == false ? (10 + sortOrderCount + panelCount) : uiPanel.sortingOrder + sortOrderCount + panelCount;
        for (int i = 0; i < arrayEffect.Length; i++)
        {
            arrayEffect[i].GetComponent<Renderer>().sortingOrder = sortOrder;
        }
    }

    private void ActiveMachineLight(bool isActive)
    {
        for(int i=0; i< moleCatchObj.listEffect_Light.Count; i++)
        {
            moleCatchObj.listEffect_Light[i].SetActive(isActive);
        }
    }

    private void ActiveRewardLight(bool isActive)
    {
        moleCatchObj.lastRewardEffect.SetActive(isActive);
    }
    #endregion

    #region 버튼
    private void OnClickBtnWaveChange()
    {
        ActiveMachineLight(false);
        ActiveRewardLight(false);

        //웨이브 전환 연출
        currentWave++;
        if (currentWave == allWaveCount)
        {
            isNormalWave = (allWaveCount == 1 || currentWave != allWaveCount) ? true : false;
            StartCoroutine(WaveChangeAction());
        }
    }
    #endregion

    #region 튜토리얼 관련함수
    public Transform GetMoleCatchTr()
    {
        if (moleCatchObj != null)
            return moleCatchObj.transform;
        else
            return null;
    }

    public Vector3 GetHoleTr()
    {
        if (moleCatchObj != null)
            return moleCatchObj.holeTr;
        else
            return new Vector3(0f, -280f, 0f);
    }

    public Vector3 GetBoardTr()
    {
        if (moleCatchObj != null)
            return moleCatchObj.boardTr;
        else
            return new Vector3(0f, 70f, 0f);
    }

    public int GetStageListCount()
    {
        return listStage.Count;
    }

    public List<Transform> GetUpMoleList()
    {
        List<Transform> listMoleTr = new List<Transform>();
        for (int i = 0; i < listStage.Count; i++)
        {
            int moleIdx = moleCatchObj.listMole.FindIndex(x => x.stageIndex == listStage[i]);
            if (moleIdx > -1)
            {
                listMoleTr.Add(moleCatchObj.listMole[moleIdx].transform);
            }
        }
        return listMoleTr;
    }

    public Vector3 GetHitOffset()
    {
        return moleCatchObj.hitEfOffset;
    }
    #endregion

    #region 테스트용 버튼(나중에 지워야함)
    public UISprite buttonClear;
    public UISprite buttonFail;

    private enum MoleCatchTouchState
    {
        CLEAR,
        FAIL,
        SELECT
    }

    private MoleCatchTouchState touchState = MoleCatchTouchState.SELECT;
    private MoleCatchTouchState TouchState
    {
        get { return touchState; }
        set
        {
            touchState = value;
            switch (touchState)
            {
                case MoleCatchTouchState.SELECT:
                    buttonClear.spriteName = "ready_button_02_off";
                    buttonFail.spriteName = "ready_button_02_off";
                    break;
                case MoleCatchTouchState.CLEAR:
                    buttonClear.spriteName = "ready_button_02_on";
                    buttonFail.spriteName = "ready_button_02_off";
                    break;
                case MoleCatchTouchState.FAIL:
                    buttonClear.spriteName = "ready_button_02_off";
                    buttonFail.spriteName = "ready_button_02_on";
                    break;
                
            }
        }
    }
    
    private void OnClickBtnClear()
    {
        OnClickBtnTest(MoleCatchTouchState.CLEAR);
    }

    private void OnClickBtnFail()
    {
        OnClickBtnTest(MoleCatchTouchState.FAIL);
    }

    private void OnClickBtnTest(MoleCatchTouchState state)
    {
        if (TouchState == state)
            TouchState = MoleCatchTouchState.SELECT;
        else
            TouchState = state;
    }

    private void OnClickBtnWaveClear()
    {
        SetMoleAnimation(MoleState.HIDE);
        for (int i = 0; i < moleCatchObj.listMole.Count; i++)
        {
            ClearBoard(moleCatchObj.listMole[i].stageIndex, false);
        }

        if (currentWave != allWaveCount)
        {
            StartCoroutine(CoRewardAction());
            StartCoroutine(SetMachineAnimation(MoleCatchState.WAVECLEAR));
            ManagerSound.AudioPlay(AudioLobby.event_mole_fanfare);
        }
    }
    #endregion
}
