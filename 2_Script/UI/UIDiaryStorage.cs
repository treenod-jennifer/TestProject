using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class HousingComparer : IComparer<PlusHousingModelData>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(PlusHousingModelData a, PlusHousingModelData b)
    {
        //하우징 인덱스 검사.
        if (a.housingIndex < b.housingIndex)
            return -1;
        else if (a.housingIndex > b.housingIndex)
            return 1;
        else
        {   //이벤트 타입 검사.
            //a, b 두 개 다 이벤트 데코가 아니라면 해당 데코의 new 상태 파악.
            if (a.expire_ts == 0 && b.expire_ts == 0)
            {
                if (UIDiaryController._instance != null)
                {
                    bool bKey_A = UIDiaryController._instance.CheckNewIconHousing(a.housingIndex, a.modelIndex);
                    bool bKey_B = UIDiaryController._instance.CheckNewIconHousing(b.housingIndex, b.modelIndex);

                    //둘 다 새로운 아이템이거나 아닌 경우, 모델 인덱스 순으로 결정.
                    if (bKey_A == bKey_B)
                    {
                        if (a.modelIndex > b.modelIndex)
                            return -1;
                        else if (a.modelIndex < b.modelIndex)
                            return 1;
                        else
                            return 0;
                    }
                    else if (bKey_A == true && bKey_B == false)
                    {
                        return -1;
                    }
                    else
                        return 1;
                }
                else
                    return 0;
            }
            else if (a.expire_ts > 0 && b.expire_ts > 0)
            {   //둘 다 이벤트 데코라면, 모델 인덱스 순으로 정렬.
                if (a.modelIndex < b.modelIndex)
                    return -1;
                else if (a.modelIndex > b.modelIndex)
                    return 1;
                else
                    return 0;
            }
            else if (a.expire_ts > 0 && b.expire_ts == 0)
            {
                return -1;
            }
            else
                return 1;
        }
    }
}

public class CompleteHousingComparer : IComparer<PlusHousingModelData>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(PlusHousingModelData a, PlusHousingModelData b)
    {
        //하우징 인덱스 검사.
        if (a.housingIndex < b.housingIndex)
            return -1;
        else if (a.housingIndex > b.housingIndex)
            return 1;
        else
        {
            int selectedModelIdx = ManagerHousing.GetSelectedHousingModelIdx(a.housingIndex);

            //모델 인덱스 순으로 결정.
            if (a.modelIndex == selectedModelIdx)
                return -1;
            else if (b.modelIndex == selectedModelIdx)
                return 1;
            else if (a.modelIndex > b.modelIndex)
                return -1;
            else if (a.modelIndex < b.modelIndex)
                return 1;
            else
                return 0;
        }
    }
}

public class UIDiaryStorage : UIDiaryBase
{
    public static UIDiaryStorage _instance = null;

    public UIScrollView scrollView;
    public GameObject progressRoot;
    public GameObject completedRoot;
    public UILabel materialCount;
    public UILabel emptyText_p;
    public UILabel emptyText_c;

    //Progress 에 표시되는 오브젝트.
    public GameObject _objItemDiaryProgress;
    public GameObject _objItemDiaryHousingP;
    //Complete 에 표시되는 아이템.
    public GameObject _objItemDiaryComplete;
    public GameObject _objItemDiaryHousingC;

    private List<UIItemDiaryProgress> itemProgressList = new List<UIItemDiaryProgress>();
    private List<UIItemDiaryComplete> itemCompleteList = new List<UIItemDiaryComplete>();

    private const float PROGRESS_START_XPOS = -285.5f;
    private const float PROGRESS_START_YPOS = -105f;
    private const float PROGRESS_SPACE_SIZE = -133f;

    private const float COMPLETE_START_XPOS = 0f;
    private const float COMPLETE_START_YPOS = -60f;
    private const float COMPLETE_SPACE_SIZE = -137f;

    private int eventItemCount = 0;

    //하우징 탭 연 시간 저장.
    private long openPopupTime = 0;

    //데코 획득했는지 저장하는 변수.
    [HideInInspector]
    public bool bGetDeco = false;

    HousingComparer housingComparer = new HousingComparer();
    CompleteHousingComparer completeHousingComparer = new CompleteHousingComparer();

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void SetScroll(int itemIndex, int tapIndex)
    {
        int hIndex = (int)(itemIndex / 10000);
        int mIndex = (int)(itemIndex % 10000);

        int listIndex = -1;
        float moveTime = 0f;
        if (tapIndex == 2)
        {
            listIndex = itemCompleteList.FindIndex(x => x.housingIndex == hIndex);
            if(listIndex > -1)
                moveTime = itemCompleteList[listIndex].OnClickBtnMore();
        }

        if (listIndex <= -1)
            return;

        //더보기 버튼 오픈된 이후, 스크롤위치 계산 후 포커싱 되도록.
        float yPos = 0.0f;
        StartCoroutine(CoAction(moveTime, () =>
        {
            if(tapIndex == 2)
                yPos = GetCompleteScrollYPos(listIndex, mIndex);
            Vector3 targetPos = new Vector3(0f, -yPos, 0f);
            SpringPanel.Begin(scrollView.gameObject, targetPos, 8f);

        }));
    }

    public void OpenProgressTap(int hIndex)
    {
        for (int i = 0; i < itemProgressList.Count; i++)
        {
            if (itemProgressList[i].housingIndex == hIndex)
            {
                itemProgressList[i].OnClickBtnMore();
            }
        }
    }

    public GameObject GetBenchBox(int hIndex)
    {
        for (int i = 0; i < itemProgressList.Count; i++)
        {
            if (itemProgressList[i].housingIndex == hIndex)
            {
                return itemProgressList[i].gameObject;
            }
        }
        return null;
    }

    public UIItemDiaryHousing_P GetBenchItem(int hIndex, int mIndex)
    {
        for (int i = 0; i < itemProgressList.Count; i++)
        {
            if (itemProgressList[i].housingIndex == hIndex)
            {
                List<UIItemDiaryHousing_P> pList = itemProgressList[i].itemHousingPList;
                for (int j = 0; j < pList.Count; j++)
                {
                    if (pList[j].plusData.modelIndex == mIndex)
                    {
                        return pList[j];
                    }
                }
            }
        }
        return null;
    }

    public void OpenProgress(int index, float yPos, bool bOpen)
    {   
        int nCount = itemProgressList.Count;
        //open 상태에서는 위로 올라가야 함.
        if (bOpen == true)
        { 
            yPos *= -1;
        }

        float moveSpeed = 0.2f;
        //현재 인덱스 아래 모두 이동.
        for (int i = index; i < nCount; i++)
        {
            //progress 들 위치 : 현재위치 + offset만큼 Y 이동.
            float targetPos = itemProgressList[i].transform.localPosition.y + yPos;
            itemProgressList[i].transform.DOLocalMoveY(targetPos, moveSpeed);
        }
        if (itemProgressList.Count >= nCount && itemProgressList[nCount - 1] != null)
        {
            //complete 의 위치 : 현재위치 + yPos + 여백 공간 만큼Y 이동.
            float pos = completedRoot.transform.localPosition.y + yPos;
            completedRoot.transform.DOLocalMoveY(pos, moveSpeed);
        }
    }

    public void OpenComplete(int index, float yPos, bool bOpen)
    {
        int nCount = itemCompleteList.Count;
        //open 상태에서는 위로 올라가야 함.
        if (bOpen == true)
        {
            yPos *= -1;
        }

        float moveSpeed = 0.2f;
        //현재 인덱스 아래 모두 이동.
        for (int i = index; i < nCount; i++)
        {
            //progress 들 위치 : 현재위치 + offset만큼 Y 이동.
            float targetPos = itemCompleteList[i].transform.localPosition.y + yPos;
            itemCompleteList[i].transform.DOLocalMoveY(targetPos, moveSpeed);
        }
    }

    public void PurchaseEventItem()
    {
        eventItemCount--;
        //이벤트 여부에 따라 탭 이벤트 아이콘 설정.
        UIPopupDiary._instance.SettingEventTap(1, eventItemCount > 0);
    }

    private void Start()
    {
        openPopupTime = Global.GetTime();
        InitProgressItem();
        InitMaterialCount();
    }
    
    private void InitProgressItem()
    {   
        //기간한정 데코 검사.
        UIDiaryController._instance.CheckLimitHousingProgressData();

        List<PlusHousingModelData> listProgress = new List<PlusHousingModelData>();
        List<PlusHousingModelData> listComplete = new List<PlusHousingModelData>();

        //하우징 타입이 미션 타입인거 제외하고 리스트에 추가(active 상태에 따라 Progrss / Complete 리스트 구분).
        ManagerHousing.GetHousingProgress(listProgress, listComplete);
        this.eventItemCount = ManagerHousing.GetUnfinishedEventItemCount();

        //이벤트 여부에 따라 탭 이벤트 아이콘 설정.
        UIPopupDiary._instance.SettingEventTap(1, eventItemCount > 0);

        //프로그레스의 마지막 카테고리 위치를 얻어올 변수.
        float progressYPos = 150f;
        //순서대로 정렬.
        if (listProgress.Count > 0)
        {
            emptyText_p.gameObject.SetActive(false);
            listProgress.Sort(housingComparer);
            progressYPos = MakeItemDiaryProgress(listProgress);
            completedRoot.transform.localPosition = new Vector3(0, progressRoot.transform.localPosition.y - progressYPos, 0);
        }
        else
        {
            emptyText_p.gameObject.SetActive(true);
            emptyText_p.text = Global._instance.GetString("p_e_5");
            completedRoot.transform.localPosition = new Vector3(0, progressRoot.transform.localPosition.y - progressYPos, 0);
        }

        if (listComplete.Count > 0)
        {
            listComplete.Sort(completeHousingComparer);
            MakeItemDiaryComplete(listComplete);
            emptyText_c.gameObject.SetActive(false);
        }
        else
        {
            emptyText_c.gameObject.SetActive(true);
            emptyText_c.text = Global._instance.GetString("p_e_6");
        }
        
        SpringPanel.Begin(scrollView.gameObject, Vector3.zero, 8);
        //newIcon 저장된 playerPrefs 갱신.
        UIDiaryController._instance.SaveHousingNewIconPlayerPrefs();
    }

    private void InitMaterialCount()
    {
        List<ServerUserMaterial> materialDataList = ServerRepos.UserMaterials;
        int dataCount = materialDataList.Count;
        int count = 0;
        for (int i = 0; i < dataCount; i++)
        {
            if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialDataList[i].index) == true)
            {
                //기간이 지난 한정 재료의 수는 카운트 하지않음.
                if (ManagerData._instance._materialSpawnProgress[materialDataList[i].index] != 0
                    && ManagerData._instance._materialSpawnProgress[materialDataList[i].index] < openPopupTime)
                    continue;
            }
            count += ServerRepos.UserMaterials[i].count;
        }
        materialCount.text = count.ToString();
    }

    float MakeItemDiaryProgress(List<PlusHousingModelData> listProgress)
    {
        itemProgressList.Clear();
        List<PlusHousingModelData> itemList = new List<PlusHousingModelData>();
        float yPos = PROGRESS_START_YPOS;

        float boxHeight = 0f;
        float completePos = 0f;
        for (int i = 0; i < listProgress.Count; i++)
        {
            //itemList에 값이 들어가 있고 현재 하우징이 이전 것과 다른 카테고리의 하우징일 경우, itemList에 있는 데이터로 progress생성.
            //itemList 에 값이 없거나, 이전 하우징과 현재 하우징이 같을 경우는 리스트에 그대로 추가.
            if (itemList.Count > 0 && listProgress[(i - 1)].housingIndex != listProgress[i].housingIndex)
            {
                //newIconList받아오기.
                List<int> newIconList = new List<int>();
                newIconList = CheckProgressNewIcon(itemList[0].housingIndex);

                UIItemDiaryProgress itemProgress = NGUITools.AddChild(progressRoot, _objItemDiaryProgress).GetComponent<UIItemDiaryProgress>();
                boxHeight = itemProgress.InitItemProgress(itemList, newIconList, itemProgressList.Count);
                itemProgress.transform.localPosition = new Vector3(PROGRESS_START_XPOS, yPos, 0);
                yPos -= (boxHeight + 25f);
                itemProgressList.Add(itemProgress);
                //more버튼에 알림 아이콘 설정할 지 결정.
                UIDiaryController._instance.SettingItemHousingProgressAlarmIcon(itemList[0].housingIndex, itemProgress);

                itemList.Clear();
            }
            //현재 하우징 데이터를 리스트에 추가.
            itemList.Add(listProgress[i]);

            //현재 검사하는 하우징이 마지막 데이터일 경우, itemList에 남아있는 데이터로 progress생성.
            if (i == (listProgress.Count - 1))
            {   
                //newIconList받아오기.
                List<int> newIconList = new List<int>();
                newIconList = CheckProgressNewIcon(itemList[0].housingIndex);

                UIItemDiaryProgress itemProgress = NGUITools.AddChild(progressRoot, _objItemDiaryProgress).GetComponent<UIItemDiaryProgress>();
                completePos += (itemProgress.InitItemProgress(itemList, newIconList, itemProgressList.Count));
                itemProgress.transform.localPosition = new Vector3(PROGRESS_START_XPOS, yPos, 0);
                itemProgressList.Add(itemProgress);

                //more버튼에 알림 아이콘 설정할 지 결정.
                UIDiaryController._instance.SettingItemHousingProgressAlarmIcon(itemList[0].housingIndex, itemProgress);
            }
        }
        return completePos - yPos;
    }

    private List<int> CheckProgressNewIcon(int housingIdx)
    {
        List<int> newIconList = new List<int>();
        newIconList = UIDiaryController._instance.GetHousingNewIconPlayerPrefs(housingIdx);        
        return newIconList;
    }

    void MakeItemDiaryComplete(List<PlusHousingModelData> listComplete)
    {
        itemCompleteList.Clear();
        List<PlusHousingModelData> itemList = new List<PlusHousingModelData>();
        float yPos = COMPLETE_START_YPOS;

        float boxHeight = 0f;
        for (int i = 0; i < listComplete.Count; i++)
        {
            //itemList에 값이 들어가 있고 현재 하우징이 이전 것과 다른 카테고리의 하우징일 경우, itemList에 있는 데이터로 complete생성.
            //itemList 에 값이 없거나, 이전 하우징과 현재 하우징이 같을 경우는 리스트에 그대로 추가.
            if (itemList.Count > 0 && listComplete[(i - 1)].housingIndex != listComplete[i].housingIndex)
            {
                UIItemDiaryComplete itemComplete = NGUITools.AddChild(completedRoot, _objItemDiaryComplete).GetComponent<UIItemDiaryComplete>();
                boxHeight = itemComplete.InitItemComplete(itemList, itemCompleteList.Count);
                itemComplete.transform.localPosition = new Vector3(COMPLETE_START_XPOS, yPos, 0);
                yPos -= (boxHeight + 25f);

                itemList.Clear();
                itemCompleteList.Add(itemComplete);
            }
            //현재 하우징 데이터를 리스트에 추가.
            itemList.Add(listComplete[i]);

            //현재 검사하는 하우징이 마지막 데이터일 경우, itemList에 남아있는 데이터로 progress생성.
            if (i == (listComplete.Count - 1))
            {
                UIItemDiaryComplete itemComplete = NGUITools.AddChild(completedRoot, _objItemDiaryComplete).GetComponent<UIItemDiaryComplete>();
                itemComplete.InitItemComplete(itemList, itemCompleteList.Count);
                itemComplete.transform.localPosition = new Vector3(COMPLETE_START_XPOS, yPos, 0);
                itemCompleteList.Add(itemComplete);
            }
        }
    }

    void OnClickBtnMaterial()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        ManagerUI._instance.OpenPopupMaterial(openPopupTime);
    }

    public void GetItemProgress(int in_hIndex, int in_mInxex, float _fBtnRemoveTime = 0.3f)
    {
        StartCoroutine(CoGetItemProgress(in_hIndex, in_mInxex, 0.3f));
    }   

    public IEnumerator CoGetItemProgress(int in_hIndex, int in_mInxex, float _fBtnRemoveTime = 0.3f)
    {
        int deleteIndex_category = -1;
        int nCount = itemProgressList.Count;

        //해당 모델이 있는 하우징 카테고리 찾기.
        for (int i = 0; i < nCount; i++)
        {   
            UIItemDiaryProgress progress = itemProgressList[i];
            if (progress.housingIndex == in_hIndex)
            {
                //해당 카테고리의 인덱스.
                deleteIndex_category = i;

                int deleteIndex_item = -1;
                int itemCount = progress.itemHousingPList.Count;

                //카테고리에서 해당 모델찾기.
                for (int j = 0; j < itemCount; j++)
                {
                    UIItemDiaryHousing_P item = progress.itemHousingPList[j];
                    if (item.plusData.modelIndex == in_mInxex)
                    {
                        //해당 모델의 인덱스 기억.
                        deleteIndex_item = j;

                        //해당 모델 사라지는 연출.
                        item.transform.DOScaleX(0, _fBtnRemoveTime);
                        DOTween.ToAlpha(() => item.mainSprite.color, x => item.mainSprite.color = x, 0f, _fBtnRemoveTime - 0.05f);

                        //사라진 연출 뒤 리스트에서 제거, 아이템삭제.
                        yield return new WaitForSeconds(_fBtnRemoveTime);
                        progress.itemHousingPList.Remove(progress.itemHousingPList[j]);
                        Destroy(item.gameObject);
                        break;
                    }
                }
                yield return null;

                int alarmCount = UIDiaryController._instance.GetItemHousingProgressAlarmCount(progress.housingIndex);
                //모델 아이템 사라지는 연출 후 모델 아이템 아래 있는 아이템들 움직이는 연출.
                progress.MoveProgressItemPos(deleteIndex_item, alarmCount);
                break;
            }
        }
        yield return null;

        //카테고리에 더 이상 아이템이 없을 경우, 0.2초(카테고리가 사라지는 연출 시간 후 카테고리 아이템 삭제.
        bool bCategory = true;
        UIItemDiaryProgress progressItem = itemProgressList[deleteIndex_category];
        if (progressItem != null)
        {
            //현재 카테고리의 남은 데코(데코 목록 - 방금 사라진 데코 수)가 없을 경우.
            if ((progressItem.GetDataCount() -1) <= 0)
            {
                yield return new WaitForSeconds(0.2f);
                itemProgressList.Remove(itemProgressList[deleteIndex_category]);
                Destroy(progressItem.gameObject);
                bCategory = false;
            }
        }

        //현재 창고 창 전체 연출(남은 콜렉션들 위로 올라가고, 팝업 창 닫히고)
        if (deleteIndex_category > -1)
        {
            float _startTime = 0.0f;
            _startTime = MoveProgressPos(deleteIndex_category, bCategory);

            //움직인 후 창 닫힘.
            yield return new WaitForSeconds(_startTime);
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    private float MoveProgressPos(int _nDeleteIndex, bool bCategory)
    {
        int nCount = itemProgressList.Count;

        float yOffset = PROGRESS_SPACE_SIZE;
        //카테고리가 남아있다면, 카테고리 아래에 있는 애들만 옮겨주면 됨.
        if (bCategory == true)
        {
            _nDeleteIndex += 1;
        }
        //카테고리가 아예 없어지면 progress간격 달라져야 함.
        else
        {
            yOffset -= 20f;
        }

        if (_nDeleteIndex < nCount)
        {
            for (int i = _nDeleteIndex; i < nCount; i++)
            {
                itemProgressList[i].transform.DOLocalMoveY(itemProgressList[i].transform.localPosition.y - yOffset, 0.2f, true).SetEase(Ease.Linear);
            }
        }

        Transform trQuest = completedRoot.transform;
        trQuest.DOLocalMoveY(trQuest.localPosition.y - yOffset, 0.2f, true).SetEase(Ease.Linear);
        return 0.3f;
    }

    #region 스크롤 위치 설정
    private float GetCompleteScrollYPos(int listIndex, int mIndex)
    {
        float yPos = 0f;
        yPos = -progressRoot.transform.localPosition.y + completedRoot.transform.localPosition.y + itemCompleteList[listIndex].transform.localPosition.y - 13f;

        int itemIndex = -1;
        itemIndex = itemCompleteList[listIndex].itemHousingCList.FindIndex(x => x.plusData.modelIndex == mIndex);

        // 현재 카테고리 젤 윗줄에 있는 데코들은 카테고리 이름이 보이는 위치로 스크롤
        // 윗줄이 아닌 데코들은 현재 위치에서 위에 여백정도만 보이게 스크롤.
        if (itemIndex >= 3)
        {
            yPos += (itemCompleteList[listIndex].itemHousingCList[itemIndex].transform.localPosition.y + 35f);
        }

        // 스크롤 영역 넘어가면 맨 마지막 위치로 설정.
        float lastPosition = ((scrollView.bounds.size.y) - scrollView.panel.height) * -1;
        if (yPos < lastPosition)
        {   
            yPos = lastPosition;
        }
        return yPos;
    }
    #endregion


    private IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }
}
