using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopupStage : UIPopupBase
{
    public static UIPopupStage _instance = null;

    public UIPanel backPanel;
    public UIPanel eventScroll;
    public UIPanel stageScroll;
    public UILabel eventCount;
    public UILabel[] flowerCount;

    public UITexture eventBoni;

    public GameObject eventRoot;
    public GameObject _objEventStagePanel;

    [HideInInspector]
    public List<ChapterData> _listChapterData = new List<ChapterData>();
    public List<StageUIData> _listStageUIData = new List<StageUIData>();

   int _nCurrentIndex = -1;

    //현재 셀의 첫번째 스테이지 정보가 있는 셀 인덱스.
    int _firstIndex = 0;
    //스테이지 버튼의 인덱스.
    int _cellIndex = 0;
    //현재 셀의 정렬 방향.
    bool _bArrowRight = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        backPanel.depth = uiPanel.depth + 1;
        eventScroll.depth = uiPanel.depth + 2;
        stageScroll.depth = uiPanel.depth + 3;
        StartCoroutine(CoActionBoni());
        SetEventStagePanel();
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
            eventScroll.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            stageScroll.sortingOrder = layer + 1;
            eventScroll.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitStagePopUp(bool bChangeAction = false)
    {
        //bChangeAction == true : 꽃 변하는 연출/ false : 연출없음.
        SetStageUIData(bChangeAction);
    }

    void SetEventStagePanel()
    {

    }

    void SettingPositionEventPanel()
    {

    }

    //스테이지 수 만큼 UI데이터를 생성시켜 리스트에 담는 함수.
    private void SetStageUIData(bool bChangeAction)
    {
        _listChapterData = ManagerData._instance.chapterData;
        
        //마지막 레벨 저장할 변수.
        int lastLevel = -1;
        int flowerCnt = 0;
        int flowerCnt_blue = 0;
        int chapterCount = _listChapterData.Count;
        int _nChapterNum = 0;
        int _nstageNum = 1;
        int nextChapter = -1;

        //퀘스트 데이터 저장.
        QuestGameData questData = null;

        //현재 퀘스트 중 챕터 퀘스트가 있으면 퀘스트 저장.
        var enumerator = ManagerData._instance._questGameData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            QuestGameData qData = enumerator.Current.Value;
            if ((int)qData.type >= 1000 && (int)qData.type < 2000)
            {
                questData = qData;
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
            QuestGameData qData = null;

            //현재 스테이지의 별 갯수 받아옴.
            int stageFlower = ManagerData._instance._stageData[_nstageNum - 1]._flowerLevel;

            //<챕터 셀> 추가 : 다음 챕터가 있고, 현재 스테이지가 다음 챕터 시작 스테이지와 같으면.
            if (chapterCount >= (_nChapterNum +1) && _nstageNum == (_listChapterData[_nChapterNum]._stageIndex))
            {
                //퀘스트데이터를 가지고 있는 해당 챕터에 있는 아이템들에게만 퀘스트 데이터 줌.
                if (questData != null && _listChapterData[_nChapterNum]._cIndex == questData.level)
                {
                    qData = questData;
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

                _listStageUIData.Add(stageUIData);
                _nChapterNum += 1;
                i += 1;

                //다음 스테이지가 없다면 return.
                if (i >= (stageCount + chapterCount))
                    return;
            }

            //퀘스트데이터를 가지고 있는 해당 챕터에 있는 아이템들에게만 퀘스트 데이터 줌.
            if (questData != null && _listChapterData[_nChapterNum - 1]._cIndex == questData.level)
            {
                qData = questData;
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

        //연출 없을 경우에만
        if (bChangeAction == false)
        {
            SettingPopupFlowerInfo(lastLevel);
        }
    }

    //스테이지 UI 데이터 4개를 가져오는 함수.
    public StageUIData[] GetStageData(int _nIndex)
    {
        StageUIData[] _arrStageData = new StageUIData[4];
        _firstIndex = (4 * _nIndex);

        //현재 셀의 인덱스에 따라 좌, 우 정렬방향 설정.
        if (_nIndex == 1 || (_nIndex > 1 && _nIndex % 2 > 0))
            _bArrowRight = false;
        else
            _bArrowRight = true;

        //첫번째 스테이지 인덱스부터 4개까지의 스테이지 데이터 반환
        for (int i = 0; i < 4; i++)
        {
            //찾고자 하는 인덱스값이 리스트의 수보다 작을 떄만 데이터 세팅.
            if (_firstIndex + i >= _listStageUIData.Count)
                break;

            //현재 셀의 정렬 방향에 따라 인덱스 결정.
            if (_bArrowRight == false)
                _cellIndex = (3 - i);
            else
                _cellIndex = i;

            _arrStageData[_cellIndex] = _listStageUIData[_firstIndex + i];
            _arrStageData[_cellIndex].eArrowDir = GetBtnStageArrowDirection(i, _firstIndex);
            //스테이지 셀 진행 방향 설정(좌, 우).
            _arrStageData[_cellIndex].bRight = _bArrowRight;
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
        int _nCount = _listStageUIData.Count / 4;
        if (_listStageUIData.Count % 4 == 0)
        {
            _nCount -= 1;
        }
        return _nCount;
    }

    //스테이지 간 화살표 방향 설정(좌, 우, 하단)하는 함수.
    BtnStageArrowDir GetBtnStageArrowDirection(int i, int _firstIndex)
    {
        int chapterCount = _listChapterData.Count;
        //스테이지 전체 갯수 = 마지막 챕터의 시작 번호 + 스테이지 갯수 - 1.
        //(시작 번호가 1부터 시작하기 때문에 -1을 빼줘야 정확한 갯수가 나옴).
        int stageCount = _listChapterData[chapterCount - 1]._stageIndex + _listChapterData[chapterCount - 1]._stageCount -1 ;

        //마지막 스테이지라면 화살표 없는 상태.
        if(_listStageUIData[_firstIndex + i].eBtnState == BtnStageState.BtnComingSoon)
            return BtnStageArrowDir.none;

        //마지막 스테이지 셀이 아니라면 현재 스테이지가 정렬된 방향으로 화살표 세팅.
        if (i < 3)
        {
            if (_bArrowRight == false)
                return BtnStageArrowDir.left;
            else
                return BtnStageArrowDir.right;
        }
        //현재 셀의 마지막 스테이지 셀이라면 다운.
        else
            return BtnStageArrowDir.down;
    }

    IEnumerator CoActionBoni()
    {
        bool bShow = false;
        float _nTimer = 0f;
        while (gameObject.activeInHierarchy == true)
        {
            _nTimer += 5.0f * Time.deltaTime;
            if (_nTimer > 1.0f)
            {
                if (bShow == true)
                    bShow = false;
                else
                    bShow = true;

                eventBoni.gameObject.SetActive(bShow);
                _nTimer = 0f;
            }
            yield return null;
        }
        yield return null;
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
}
