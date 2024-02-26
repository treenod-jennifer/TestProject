using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemEpScrollView : MonoBehaviour
{
    public static UIItemEpScrollView _instance = null;

    public class EpisodeUIData
    {
        public int episodeCnt { get; set; }

        public int flowerCnt { get; set; }

        public bool chapterClear { get; set; }

        public bool waitFlower { get; set; }

        public ChapterState chapterState { get; set; }

        public ChapterMission questData = null;

        public bool questProg { get; set; }

        public EpisodeUIData(int epCnt, ChapterMission qData)
        {
            episodeCnt = epCnt;
            questData = qData;
        }
    }

    [HideInInspector] public List<ChapterData> _listChapterData = new List<ChapterData>();
    [HideInInspector] public List<EpisodeUIData> _listEpisodeUIData = new List<EpisodeUIData>();

    [SerializeField] private GameObject sprCollider;
    [SerializeField] private UIReuseGrid_EpisodeScroll scroll;
    [SerializeField] private UIButtonDropDown dropDown;

    [HideInInspector] public int currentEpisode = 0;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }
    private void Start()
    {
        dropDown.OnClickDropDownBtnEvent += (state) =>
        {
            sprCollider.SetActive(UIButtonDropDown.ExpansionState.Reduction == state);
        };

        dropDown.PostReductionEvent += () => scroll.ScrollItemMove(currentEpisode);

        SetEpisodeData();
    }

    public void DropDown()
    {
        dropDown.ButtonCall();
    }

    private void SetEpisodeData()
    {
        _listChapterData = ManagerData._instance.chapterData;

        int _nStageNum = 0;
        
        //퀘스트 데이터 저장.
        List<ChapterMission> questData = new List<ChapterMission>();

        var enumerator = ManagerData._instance._chapterMissionData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            ChapterMission qData = enumerator.Current.Value;
            if ((int)qData.type >= 1000 && (int)qData.type < 2000)
            {
                questData.Add(qData);
            }
        }

        _listEpisodeUIData.Clear();

        EpisodeUIData epUIData = null;

        for(int chapterIdx = 0; chapterIdx < _listChapterData.Count; chapterIdx++)
        {
            ChapterMission qData = null;

            int lockStageCnt = 0;
            int flowerCnt = 0;
            int flowerCnt_blue = 0;
            int flowerCnt_red = 0;
            int questClear = 0;

            for (int idx = 0; idx < questData.Count; idx++)
            {
                if (questData != null && _listChapterData[chapterIdx]._cIndex == questData[idx].level)
                {
                    qData = questData[idx];
                }
            }

            epUIData = new EpisodeUIData(_listChapterData[chapterIdx]._cIndex, qData);

            //스테이지 갯수를 알기 위한 for문
            for (int stageIdx = 0; stageIdx < ManagerData._instance.chapterData[chapterIdx]._stageCount; stageIdx++)
            {
                int stageFlower = ManagerData._instance._stageData[_nStageNum]._flowerLevel;

                switch(stageFlower)
                {
                    case 0:
                        lockStageCnt++;
                        break;
                    case 3: //흰꽃
                        flowerCnt++;
                        break;
                    case 4: //파란꽃
                        flowerCnt++;
                        flowerCnt_blue++;
                        break;
                    case 5: //빨간꽃
                        flowerCnt++;
                        flowerCnt_blue++;
                        flowerCnt_red++;
                        break;
                }

                //각각 스테이지 에피소드 퀘스트 클리어 여부
                if(ManagerData._instance._stageData[_nStageNum]._missionClear == 1)
                {
                    questClear++;
                }

                _nStageNum += 1;
            }

            //꽃의 상태와 갯수, 챕터가 열림 상태를 구함.
            if (lockStageCnt == ManagerData._instance.chapterData[chapterIdx]._stageCount)
            {
                if (chapterIdx == 0 || _listEpisodeUIData[chapterIdx - 1].chapterClear)
                {
                    epUIData.chapterState = ChapterState.Chapter_Flower_White;
                    epUIData.flowerCnt = flowerCnt;
                }
                else
                {
                    epUIData.chapterState = ChapterState.Chapter_Lock;
                    epUIData.flowerCnt = 0;
                }
            }
            else
            {
                if (flowerCnt_red == ManagerData._instance.chapterData[chapterIdx]._stageCount)
                {
                    epUIData.chapterState = ChapterState.Chapter_Flower_Complete;
                    epUIData.flowerCnt = flowerCnt_red;
                }
                else if (flowerCnt_blue == ManagerData._instance.chapterData[chapterIdx]._stageCount &&
                    flowerCnt_red < ManagerData._instance.chapterData[chapterIdx]._stageCount)
                {
                    if(flowerCnt_red == 0)
                        epUIData.waitFlower = true;

                    epUIData.chapterState = ChapterState.Chapter_Flower_Red;
                    epUIData.flowerCnt = flowerCnt_red;
                }
                else if (flowerCnt == ManagerData._instance.chapterData[chapterIdx]._stageCount)
                {
                    if (flowerCnt_blue == 0)
                        epUIData.waitFlower = true;

                    epUIData.chapterState = ChapterState.Chapter_Flower_Blue;
                    epUIData.flowerCnt = flowerCnt_blue;
                }
                else
                {
                    epUIData.chapterState = ChapterState.Chapter_Flower_White;
                    epUIData.flowerCnt = flowerCnt;
                }
            }

            //챕터 클리어 유무 판단
            if(lockStageCnt == 0)
            {
                epUIData.chapterClear = true;
            }
            else
            {
                epUIData.chapterClear = false;
                //에피소드내의 스테이지를 하나라도 클리어 하지 않았을 때
                if (currentEpisode == 0)
                    currentEpisode = epUIData.episodeCnt;
            }

            //에피소드 퀘스트 챕터 전체 클리어 확인
            if(questClear == ManagerData._instance.chapterData[chapterIdx]._stageCount)
                epUIData.questProg = true;
            else
                epUIData.questProg = false;

            _listEpisodeUIData.Add(epUIData);
        }
        //모든 에피소드가 클리어일 때는 마지막 에피소드 넘버를 넣어준다.
        if (currentEpisode == 0)
        {
            currentEpisode = GetFirstFlowerState();
        }
    }

    //포코꽃 이벤트 조건 : if (ServerContents.BlossomEvents != null)
    int GetFirstFlowerState()
    {
        int flowerEpisode = 0;

        //흰꽃을 다 모았는지 검사
        flowerEpisode = SearchFirstFlowerState(ChapterState.Chapter_Flower_White);


        if (flowerEpisode == 0)
            flowerEpisode = SearchFirstFlowerState(ChapterState.Chapter_Flower_Blue);
            
        if(flowerEpisode == 0)
        {
            if (ManagerPokoFlowerEvent.IsRunningPokoFlowerEvent())
                flowerEpisode = SearchFirstFlowerState(ChapterState.Chapter_Flower_Red);
            else
                return GetEpisodeDataCount();
        }

            return flowerEpisode;        
    }

    int SearchFirstFlowerState(ChapterState chapterState)
    {
        for (int i = 0; i < _listEpisodeUIData.Count; i++)
        {
            if (_listEpisodeUIData[i].chapterState == chapterState)
            {
                if(_listEpisodeUIData[i].flowerCnt != ManagerData._instance.chapterData[_listEpisodeUIData[i].episodeCnt - 1]._stageCount)
                    return _listEpisodeUIData[i].episodeCnt;
            }
        }
        return 0;
    }

    public EpisodeUIData GetEpisodeData(int _nIndex)
    {
        EpisodeUIData _epData = _listEpisodeUIData[_nIndex];

        return _epData;
    }

    public int GetEpisodeDataCount()
    {
        return _listEpisodeUIData.Count;
    }
}
