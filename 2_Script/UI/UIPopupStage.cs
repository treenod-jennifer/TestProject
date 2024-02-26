using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using PokoAddressable;

public class UIPopupStage : UIPopupBase
{
    public static UIPopupStage _instance = null;

    public UIPanel stageScroll;
    public UILabel[] flowerCount;
    public GameObject whiteFlowerAlarmIcon;
    public GameObject blueFlowerAlarmIcon;
    public GameObject pokoFlowerAlarmIcon;

    [SerializeField] GameObject btnCacheReset;

    public GameObject _objEventStagePanel;

    [SerializeField] private UIStageBoniRun boniRun;
    [SerializeField] private UIStageBoniRun bluebirdCheer;
    [SerializeField] private UITexture comingSoon;
    [SerializeField] private UIReuseGrid_Stage uIReuseGrid;

    [SerializeField] private GameObject PokoFlowerEvent;

    [HideInInspector]
    public List<ChapterData> _listChapterData = new List<ChapterData>();
    public List<StageUIData> _listStageUIData = new List<StageUIData>();

    int _nCurrentIndex = -1;

    [SerializeField] private UIItemFriendsManager friendManager;
    public UIItemFriendsManager FriendManager { get { return friendManager; } }

    public const int stageArraySize = 4;

    public System.Action FlowerAction = null;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        btnCacheReset.SetActive(NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX);
        StartCoroutine(FriendManager.Load());
    }

    void OnClickCacheReset()
    {
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.SANDBOX)
            return;
        
        if (System.IO.Directory.Exists(Global.StageDirectory))
        {
            System.IO.Directory.Delete(Global.StageDirectory, true);
            System.IO.Directory.CreateDirectory(Global.StageDirectory);
        }
    }


    public override void OpenPopUp(int _depth)
    {
        PopupOpenAction(_depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = _depth + i;
        }

        UIItemStageAlarmBubble._instance.LastEpisodeCheck();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            stageScroll.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            stageScroll.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitStagePopUp(bool bChangeAction = false)
    {
        //bChangeAction == true : 꽃 변하는 연출/ false : 연출없음.
        SetStageUIData(bChangeAction);

        if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
            PokoFlowerEvent.SetActive(true);
        else
            PokoFlowerEvent.SetActive(false);

        if (bChangeAction)
            UIReuseGrid_Stage.StageScrollMode = UIReuseGrid_Stage.ScrollMode.None;

        InitWhiteFlowerAlarm(ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_WHITE));
        InitBlueFlowerAlarm(ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_BLUE));
        InitRedFlowerAlarm(ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_RED));
    }

    private void PopupOpenAction(int depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }
    }

    public void InitWhiteFlowerAlarm(bool bActive)
    {
        whiteFlowerAlarmIcon.SetActive(bActive);
    }
    public void InitBlueFlowerAlarm(bool bActive)
    {
        blueFlowerAlarmIcon.SetActive(bActive);
    }
    public void InitRedFlowerAlarm(bool bActive)
    {
        pokoFlowerAlarmIcon.SetActive(bActive);
    }

    //스테이지 수 만큼 UI데이터를 생성시켜 리스트에 담는 함수.
    private void SetStageUIData(bool bChangeAction)
    {
        _listChapterData = ManagerData._instance.chapterData;
        
        //마지막 레벨 저장할 변수.
        int lastLevel = -1;
        int flowerCnt = 0;
        int flowerCnt_blue = 0;
        int flowerCnt_red = 0;
        int chapterCount = _listChapterData.Count;
        int _nChapterNum = 0;
        int _nstageNum = 1;
        int nextChapter = -1;

        //퀘스트 데이터 저장.
        List<ChapterMission> questData = new List<ChapterMission>();

        //현재 퀘스트 중 챕터 퀘스트가 있으면 퀘스트 저장.
        var enumerator = ManagerData._instance._chapterMissionData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            ChapterMission qData = enumerator.Current.Value;
            if ((int)qData.type >= 1000 && (int)qData.type < 2000)
            {
                questData.Add(qData);
            }
        }
        _listStageUIData.Clear();

        //챕터 셀에있는 스테이지 번호는 (nstageNum -1)로 설정해야 그 전의 맵 정보를 읽어와서 화살표 세팅할 수 있음.
        StageUIData stageUIData = null;

        //스테이지 전체 갯수 = 마지막 챕터의 시작 번호 + 스테이지 갯수 - 1.
        //(시작 번호가 1부터 시작하기 때문에 -1을 빼줘야 정확한 갯수가 나옴).
        int stageCount = _listChapterData[chapterCount - 1]._stageIndex + _listChapterData[chapterCount - 1]._stageCount - 1;

        //스테이지 수 + 챕터 수(챕터 아이콘이 떠야하므로) 만큼 셀 데이터 생성.
        for (int i = 0; i < (stageCount + chapterCount); i++)
        {
            ChapterMission qData = null;

            //현재 스테이지의 별 갯수 받아옴.
            int stageFlower = ManagerData._instance._stageData[_nstageNum - 1]._flowerLevel;
            
            //미션 클리어 달성도
            int epMissionClear = 0;

            //<챕터 셀> 추가 : 다음 챕터가 있고, 현재 스테이지가 다음 챕터 시작 스테이지와 같으면.
            if (chapterCount >= (_nChapterNum +1) && _nstageNum == (_listChapterData[_nChapterNum]._stageIndex))
            {
                //퀘스트데이터를 가지고 있는 해당 챕터에 있는 아이템들에게만 퀘스트 데이터 줌.
                for (int idx = 0; idx < questData.Count; idx++)
                {
                    if (questData != null && _listChapterData[_nChapterNum]._cIndex == questData[idx].level)
                    {
                        qData = questData[idx];

                        // 퀘스트가 있는 챕터의 미션 클리어 갯수 체크
                        for (int index = ManagerData._instance.chapterData[_nChapterNum]._stageIndex - 1; index <
                            (ManagerData._instance.chapterData[_nChapterNum]._stageIndex - 1) + (ManagerData._instance.chapterData[_nChapterNum]._stageCount);index++ )
                        {
                            if (ManagerData._instance._stageData[index]._missionClear == 1)
                                epMissionClear++;
                        }
                    }
                }
                
                stageUIData = new StageUIData(i, (_listChapterData[_nChapterNum]._cIndex), _nstageNum - 1, qData);
                stageUIData.bChapterUI = true;

                //챕터 셀에서는 잠긴 상태가 아니라면 무조건 seed 상태로 추가 해놓음(화살표 설정을 위해서).
                if (stageFlower == 0)
                {
                    if (lastLevel == -1)
                    {
                        stageUIData.eBtnState = BtnStageState.BtnSeed;
                    }
                    //현재 플레이 하는 챕터의 다음 챕터가 지금 검사하는 챕터 셀일경우.
                    else if (nextChapter == _listChapterData[_nChapterNum]._cIndex)
                    { 
                        stageUIData.eBtnState = BtnStageState.BtnCheerUp;
                    }
                    else
                    {
                        stageUIData.eBtnState = BtnStageState.BtnLock;
                    }
                }
                else
                    stageUIData.eBtnState = BtnStageState.BtnSeed;

                //미션 달성도 
                stageUIData.questProg2 = epMissionClear;

                _listStageUIData.Add(stageUIData);
                _nChapterNum += 1;
                i += 1;

                //다음 스테이지가 없다면 return.
                if (i >= (stageCount + chapterCount))
                    return;
            }

            //퀘스트데이터를 가지고 있는 해당 챕터에 있는 아이템들에게만 퀘스트 데이터 줌.
            for (int idx = 0; idx < questData.Count; idx++)
            {
                if (questData != null && _listChapterData[_nChapterNum - 1]._cIndex == questData[idx].level)
                {
                    qData = questData[idx];
                }
            }

            //<일반 스테이지 셀> 생성.
            stageUIData = new StageUIData(i, _listChapterData[_nChapterNum - 1]._cIndex, _nstageNum, qData);
            stageUIData.questProg1 = ManagerData._instance._stageData[_nstageNum - 1]._missionClear;

            //잠긴 레벨인지 확인.
            if (stageFlower == 0)
            {
                //처음으로 star의 상태가 0인 스테이지가 마지막 스테이지.
                if (lastLevel == -1)
                {
                    lastLevel = _nstageNum;
                    nextChapter = _listChapterData[_nChapterNum - 1]._cIndex + 1;
                    stageUIData.eBtnState = BtnStageState.none;
                }
                else
                {
                    stageUIData.eBtnState = BtnStageState.BtnLock;
                }
            }

            //열린 스테이지라면 현재 스테이지의 별 상태 검사해서 이미지 세팅.
            else
            {
                if (stageFlower == 1)
                { 
                    stageUIData.eBtnState = BtnStageState.BtnSeed;
                }
                else if (stageFlower == 2)
                {
                    stageUIData.eBtnState = BtnStageState.BtnLeaf;
                }
                else if (stageFlower == 3)
                {
                    stageUIData.eBtnState = BtnStageState.BtnFlower;
                    flowerCnt++;
                }
                else if (stageFlower == 4)
                {
                    stageUIData.eBtnState = BtnStageState.BtnBlueFlower;
                    flowerCnt++;
                    flowerCnt_blue ++;
                }
                else if (stageFlower == 5)
                {
                    stageUIData.eBtnState = BtnStageState.BtnRedFlower;
                    flowerCnt++;
                    flowerCnt_blue++;
                    flowerCnt_red++;
                }
            }

            //스크롤 초기 위치 설정해줄 때 사용.
            //연출이 되는 위치가 있다면 해당 위치 인덱스 저장.
            if (bChangeAction == true)
            {
                if (_nstageNum == Global.stageIndex)
                {
                    _nCurrentIndex = i;
                    stageUIData.bAction = true;
                }
            }
            //연출이 없을 때, 현재 스테이지가 마지막 플레이한 스테이지일 경우 인덱스 저장.
            else
            {
                if (_nstageNum == lastLevel)
                {
                    _nCurrentIndex = i;
                }
            }

            _listStageUIData.Add(stageUIData);
            _nstageNum += 1;
        }

        //제일 마지막 스테이지일 경우(ComingSoon 있는 곳).
        if (_nCurrentIndex == -1)
        {
            _nCurrentIndex = (stageCount + chapterCount);
        }

        //마지막 comming순 ui 추가(스테이지 넘버 -1 로 구분).
        stageUIData = new StageUIData((stageCount + chapterCount + 1), (_listChapterData[_nChapterNum - 1]._cIndex + 1), -1, null);
        stageUIData.bChapterUI = true;
        stageUIData.eBtnState = BtnStageState.BtnComingSoon;
        _listStageUIData.Add(stageUIData);

        //꽃 갯수 설정.
        flowerCount[0].text = string.Format("{0}/{1}", flowerCnt, stageCount);
        flowerCount[1].text = string.Format("{0}/{1}", flowerCnt_blue, stageCount);
        flowerCount[2].text = string.Format("{0}", flowerCnt_red);

        //연출 없을 경우에, 꽃 정보 팝업. 
        //파란꽃 연출이 나올때는, 연출 후 꽃 보상 팝업 띄워줌.
        if (bChangeAction == false)
        {
            SettingPopupFlowerInfo(lastLevel);
            SetPopupTouch();
        }
        else
        {
            if (_listStageUIData[_nCurrentIndex].eBtnState > BtnStageState.BtnLeaf &&
                IsChapterAllFlowerBloom(_listStageUIData[_nCurrentIndex].nChapterNumber, _listStageUIData[_nCurrentIndex].eBtnState))
            {
                int topIndex = _listStageUIData[_nCurrentIndex].nChapterNumber - 1;
                StartCoroutine(CoOpenPopupFlowerReward(topIndex, (int)_listStageUIData[_nCurrentIndex].eBtnState - 3));
            }
            else
            {
                SetPopupTouch();
            }
        }
    }

    bool IsChapterAllFlowerBloom(int chapter, BtnStageState flowerType)
    {
        var chapterData = _listChapterData.Find(x => x._cIndex == chapter);

        int flowerCount = 0;

        for(int stageIdx = chapterData._stageIndex; stageIdx < chapterData._stageIndex + chapterData._stageCount; stageIdx++)
        {
            var stageData = _listStageUIData.Find(x => x.nStageNumber == stageIdx);

            if(stageData.eBtnState >= flowerType && stageData.eBtnState <= BtnStageState.BtnRedFlower)
            {
                flowerCount++;
            }
        }

        if (chapterData._stageCount == flowerCount)
            return true;
        else
            return false;
    }

    private void SetPopupTouch(float delayTime = openTime)
    {
        StartCoroutine(CoAction(delayTime, () =>
        {
            if (FlowerAction != null)
            {
                FlowerAction();
                FlowerAction = null;
            }
            bCanTouch = true;
            ManagerUI._instance.bTouchTopUI = true;
        }));
    }

    //스테이지 UI 데이터 4개를 가져오는 함수.
    public StageUIData[] GetStageData(int _nIndex)
    {
        int stageArrayCount = Mathf.CeilToInt((float)_listStageUIData.Count / stageArraySize);
        int stageArrayIndex = (stageArrayCount - _nIndex - 1);
        int firstStageIndex = (4 * stageArrayIndex);
        bool isReversal = stageArrayIndex % 2 != 0;

        StageUIData[] _arrStageData = new StageUIData[4];

        for (int i = 0; i < stageArraySize; i++)
        {
            if (firstStageIndex + i >= _listStageUIData.Count) break;

            int stageIndex = isReversal ? stageArraySize - i - 1 : i;

            _arrStageData[stageIndex] = _listStageUIData[firstStageIndex + i];
            _arrStageData[stageIndex].eArrowDir = GetBtnStageArrowDirection(i, firstStageIndex, isReversal);
            _arrStageData[stageIndex].bRight = !isReversal;
        }

        return _arrStageData;
    }

    public int GetCurrentIndex()
    {
        return _nCurrentIndex;
    }

    //스크롤 가능 최대 수 반환해주는 함수.
    public int GetBtnStageCount()
    {
        int _nCount =_listStageUIData.Count / stageArraySize;

        if (_listStageUIData.Count % stageArraySize == 0)
        {
            _nCount -= 1;
        }

        return _nCount + 1;
    }

    //스테이지 간 화살표 방향 설정(좌, 우, 하단)하는 함수.
    BtnStageArrowDir GetBtnStageArrowDirection(int i, int _firstIndex, bool isReversal)
    {
        int chapterCount = _listChapterData.Count;
        //스테이지 전체 갯수 = 마지막 챕터의 시작 번호 + 스테이지 갯수 - 1.
        //(시작 번호가 1부터 시작하기 때문에 -1을 빼줘야 정확한 갯수가 나옴).
        int stageCount = _listChapterData[chapterCount - 1]._stageIndex + _listChapterData[chapterCount - 1]._stageCount -1 ;

        //마지막 스테이지라면 화살표 없는 상태.
        if(_listStageUIData[_firstIndex + i].eBtnState == BtnStageState.BtnComingSoon)
        {
            return BtnStageArrowDir.none;
        }

        //마지막 스테이지 셀이 아니라면 현재 스테이지가 정렬된 방향으로 화살표 세팅.
        if (i < 3)
        {
            return isReversal ? BtnStageArrowDir.left : BtnStageArrowDir.right;
        }
        //현재 셀의 마지막 스테이지 셀이라면 다운.
        else
        {
            return BtnStageArrowDir.up;
        }
    }

    private void SettingPopupFlowerInfo(int lastLevel)
    {
        int checkIndex = 0;
        if (lastLevel > 9 && lastLevel <= 65)
        {
            checkIndex = 1;
        }
        else if (lastLevel > 65 && lastLevel <= 125)
        {
            checkIndex = 2;
        }
        else if(lastLevel > 125)
        {
            checkIndex = 3;
        }

        if (checkIndex == 0)
            return;

        //팝업 생성 조건 검사 후, 꽃 피우는 팝업 생성.
        if (CheckPopupFlowerInfo(checkIndex) == true)
        {
            //딜레이 후 팝업 띄움.
            StartCoroutine(CoWait(0.3f));
        }
    }
    
    private bool CheckPopupFlowerInfo(int index)
    {
        //해당 키값 검사.
        //키가 없다면 키 추가 후 팝업 출력.
        if (PlayerPrefs.HasKey("FlowerInfo") == false)
        {
            PlayerPrefs.SetInt("FlowerInfo", index);
            return true;
        }
        //이미 해당 스테이지 영역에서 팝업을 띄우가 아닐 경우에만 팝업 출력.
        else
        {
            if (PlayerPrefs.GetInt("FlowerInfo") == index)
            {
                return false;
            }
            else
            {
                PlayerPrefs.SetInt("FlowerInfo", index);
                return true;
            }
        }
    }

    private IEnumerator CoWait(float delay)
    {
        yield return new WaitForSeconds(delay);
        ManagerUI._instance.OpenPopupFlowerInfo();
    }

    private IEnumerator CoOpenPopupFlowerReward(int topIndex, int flowerType)
    {
        yield return new WaitWhile(() => FlowerAction == null);

        if (FlowerAction != null)
        {
            FlowerAction();
            FlowerAction = null;
        }

        yield return new WaitForSeconds(1.3f);
        ManagerUI._instance.OpenPopup<UIPopupFlowerReward>((popup) => { popup._flowerType = flowerType; popup.InitFlowerCount(topIndex); });
        SetPopupTouch(0.3f);
    }

    private void OnClickBtnFlowerReward()
    {
        FlowerReward((int)ScoreFlowerType.FLOWER_WHITE);
    }

    private void OnClickBtnBlueFlowerReward()
    {
        FlowerReward((int)ScoreFlowerType.FLOWER_BLUE);
    }

    private void OnClickBtnRedFlowerReward()
    {
        FlowerReward((int)ScoreFlowerType.FLOWER_RED);
    }

    void FlowerReward(int flowerType)
    {
        if (bCanTouch == false)
            return;

        int topIndex = 0;
        if (ManagerData._instance.listGetFlowersReward.ContainsKey((ScoreFlowerType)flowerType))
            topIndex = ManagerData._instance.listGetFlowersReward[(ScoreFlowerType)flowerType][0];
        ManagerUI._instance.OpenPopup<UIPopupFlowerReward>((popup) =>
        {
            popup._flowerType = flowerType;
            popup.InitFlowerCount(topIndex);
        });
    }

    public void SetBoniRun(StageUIData stageUIData, Transform target)
    {
        if (stageUIData == null) return;

        if (stageUIData.eBtnState == BtnStageState.none)
        {
            boniRun.transform.parent = target;
            boniRun.transform.localPosition = Vector3.zero;
            boniRun.SetCharacter(true);
            boniRun.SetBoniPos(stageUIData.bRight);
            boniRun.SetArrow(stageUIData.eArrowDir);
            StartCoroutine(boniRun.CoArrowAnimation());
            boniRun.gameObject.SetActive(true);
        }
        else if (boniRun.transform.parent == target)
        {
            boniRun.gameObject.SetActive(false);
        }
    }

    public void SetBlueBirdCheer(StageUIData stageUIData, Transform target)
    {
        if (stageUIData == null) return;

        if (stageUIData.eBtnState == BtnStageState.BtnCheerUp)
        {
            bluebirdCheer.transform.parent = target;
            bluebirdCheer.transform.localPosition = new Vector3(0f, -15f, 0f);
            bluebirdCheer.SetCharacter(false);
            bluebirdCheer.SetBlueBirdLook(boniRun.transform.position.x);
            bluebirdCheer.gameObject.SetActive(true);
        }
        else if (bluebirdCheer.transform.parent == target)
        {
            bluebirdCheer.gameObject.SetActive(false);
        }
    }

    public void SetComingSoon(StageUIData stageUIData, Transform target)
    {
        if (stageUIData == null) return;

        if (stageUIData.eBtnState == BtnStageState.BtnComingSoon)
        {
            gameObject.AddressableAssetLoad<Texture2D>("local_message/soon", (texture) => comingSoon.mainTexture = texture);
            
            comingSoon.transform.parent = target;
            comingSoon.transform.localPosition = new Vector3(-5f, 7.5f, 0f);
            comingSoon.gameObject.SetActive(true);
        }
        else if (comingSoon.transform.parent == target)
        {
            comingSoon.gameObject.SetActive(false);
        }
    }

    public void SetScroll(int stageIndex)
    {
        uIReuseGrid.ScrollItemMove(stageIndex);
    }

    public void OnClickPokoFlowerEventBtn()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ecp_2"), false);
        popup.SortOrderSetting();
    }
}
