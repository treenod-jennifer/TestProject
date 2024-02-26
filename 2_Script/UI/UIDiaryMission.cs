using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

public class QuestComparer : IComparer<QuestGameData>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(QuestGameData a, QuestGameData b)
    { 
        //이벤트 타입 검사.
        //a, b 두 개 다 이벤트 퀘스트가 아니거나 둘 다 이벤트 퀘스트라면, 인덱스 순으로 정렬(낮은 게 위로).
        if ((a.valueTime1 == 1 && b.valueTime1 == 1) || (a.valueTime1 > 1 && b.valueTime1 > 1))
        {
            if (a.index < b.index)
                return -1;
            else if (a.index > b.index)
                return 1;
            else
                return 0;
        }
        else if (a.valueTime1 > 1 && b.valueTime1 == 1)
        {
            return -1;
        }
        else
            return 1;
    }
}

public class UIDiaryMission : UIDiaryBase
{
    public static UIDiaryMission _instance = null;

    public UIPanel uiPanel;
    public Transform questTr;
    public GameObject missionBoxObj;
    public GameObject questBoxObj;
    public UILabel progressPercent;
    public UILabel day;
    public UILabel emptyText_m;
    public UILabel emptyText_q;
    public UIProgressBar progressBar;

    public GameObject _objItemMission;
    public GameObject _objItemQuest;

    //튜토리얼에서 버튼 눌러졌는지 확인하는데 사용됨.
    [HideInInspector]
    public bool bClearButton = false;

    const int MISSOIN_SPACE_XPOS = -332;  //  미션 아이콘 X 초기위치
    const int MISSOIN_SPACE_YPOS = -41;   //  미션 아이콘 Y 초기위치
    const int MISSOIN_SPACE_SIZE = 150;  //  미션 아이콘 간의 간격

    // 생성된 미션 아이콘들을 관리하는 리스트.
    List<UIItemDiaryMission> addMissionList = new List<UIItemDiaryMission>();
    List<UIItemDiaryMission> btnMissionList = new List<UIItemDiaryMission>();

    // 생성된 퀘스트 아이콘들을 관리하는 리스트.
    List<UIItemDiaryQuest> addQuestList = new List<UIItemDiaryQuest>();

    //미션이펙트 오브젝트풀에 넣기
    public GameObject objHeartParticleStar;
    public GameObject objHeartEffectShine;
    public GameObject objHeartBTNShine;
    public GameObject objHeartBTNEffect;

    private float progressOffset = 0f;
    private int clearMissionCount = 0;
    private int dayMissionCount = 0;

    //이벤트 퀘스트 관련.
    private int eventQuestCount = 0;

    //미션 보상 관련
    public Protocol.AppliedRewardSet dayCompleteReward;

    QuestComparer questComparer = new QuestComparer();

    public Dictionary<int, MissionData> backupMissionData = null;
    public CdnDay backupDay = null;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    // Use this for initialization
    IEnumerator Start()
    {
        btnMissionList.Clear();

        int missionCount = 0;
        int addCount = 0;

        day.text = $"{Global.day}{Global._instance.GetString("time_1")}";

        dayMissionCount = 0;
        clearMissionCount = 0;
        dayCompleteReward = null;

        //bool missionStopped = ServerRepos.LoginCdn.LimitMission != 0 && (ServerRepos.User.missionCnt >= ServerRepos.LoginCdn.LimitMission);

        //미션 데이터 들고와서 세팅.
        var enumerator_M = ManagerData._instance._missionData.GetEnumerator();
        while (enumerator_M.MoveNext())
        {
            MissionData mData = enumerator_M.Current.Value;
            int mDataKey = enumerator_M.Current.Key;
            if (Global.day == mData.day)
            {
                if (mData.stepClear > 0)
                    dayMissionCount += mData.stepClear;
                else
                    dayMissionCount++;

                if (mData.state == TypeMissionState.Active)
                {
                    //단계미션 클리어 카운트 증가.
                    if (mData.clearCount > 0)
                        clearMissionCount += mData.clearCount;

                    //유저가 미션 체크했는지 정보 확인.
                    if (PlayerPrefs.HasKey("missionCheck" + (mDataKey)))
                    {
                        float yPos = MISSOIN_SPACE_YPOS + (-1 * MISSOIN_SPACE_SIZE) * addCount;
                        UIItemDiaryMission addMission = NGUITools.AddChild(missionBoxObj, _objItemMission).GetComponent<UIItemDiaryMission>();
                        addMission.InitBtnMission(mData);
                        addMission.transform.localScale = new Vector3(0f, 1f, 1f);
                        addMission.transform.localPosition = new Vector3(MISSOIN_SPACE_XPOS, yPos, addMission.transform.localPosition.z);
                        addMission.mainSprite.alpha = 0f;
                        addMission.itemIndex = addCount;
                        addMissionList.Add(addMission);
                        addCount++;
                        PlayerPrefs.DeleteKey("missionCheck" + mData.index);
                    }
                    else
                    {
                        float yPos = MISSOIN_SPACE_YPOS + (-1 * MISSOIN_SPACE_SIZE) * missionCount;
                        UIItemDiaryMission btnMission = NGUITools.AddChild(missionBoxObj, _objItemMission).GetComponent<UIItemDiaryMission>();
                        btnMission.InitBtnMission(mData);
                        btnMission.transform.localPosition = new Vector3(MISSOIN_SPACE_XPOS, yPos, btnMission.transform.localPosition.z);
                        btnMissionList.Add(btnMission);
                        missionCount++;
                    }
                }
                else if (mData.state == TypeMissionState.Clear)
                {
                    if (mData.stepClear > 0)
                    {
                        clearMissionCount += mData.stepClear;
                    }
                    else
                    {
                        clearMissionCount++;
                    }
                }
            }
        }

        //퀘스트 데이터 들고와서 세팅.
        List<QuestGameData> questList = new List<QuestGameData>();
        var enumerator_Q = ManagerData._instance._questGameData.GetEnumerator();
        while (enumerator_Q.MoveNext())
        {
            QuestGameData qData = enumerator_Q.Current.Value;
            if (qData.hidden == 1)
                continue;

            //끝나는 시간이 없거나, 이벤트가 끝나는 시간이 현재시간보다 더 많이 남았을 경우 리스트에 추가.
            if (qData.hidden == 0 && qData.type != QuestType.chapter_Candy && qData.type != QuestType.chapter_Duck 
                && (qData.valueTime1 == 1 || qData.valueTime1 >= Global.GetTime()) )
            {
                questList.Add(enumerator_Q.Current.Value);
            }
        }
        
        //퀘스트 데이터 정렬한 뒤, 해당 퀘스트 생성.
        if (questList.Count > 0)
        {
            questList.Sort(questComparer);

            addCount = 0;
            for (int i = 0; i < questList.Count; i++)
            {
                QuestGameData qData = questList[i];
                if (qData.state != QuestState.Finished && qData.valueTime1 == 1)
                {
                    float yPos = MISSOIN_SPACE_YPOS + (-1 * MISSOIN_SPACE_SIZE) * addCount;
                    UIItemDiaryQuest ItemQuest = NGUITools.AddChild(questBoxObj, _objItemQuest).GetComponent<UIItemDiaryQuest>();
                    ItemQuest.InitBtnQuest(qData);
                    ItemQuest.transform.localPosition = new Vector3(MISSOIN_SPACE_XPOS, yPos, ItemQuest.transform.localPosition.z);
                    addQuestList.Add(ItemQuest);
                    //이벤트 여부 검사.
                    if (qData.valueTime1 > 1)
                    {
                        eventQuestCount++;
                    }
                    addCount++;
                }
            }
        }

        //미션 프로그레스바 설정.
        InitMissionProgress(clearMissionCount);

        //미션수에 따라 empty 메세지 출력.
        if (btnMissionList.Count > 0 || addMissionList.Count > 0)
        {
            emptyText_m.gameObject.SetActive(false);
            //퀘스트 창은 마지막 미션 밑에 배치.
            questBoxObj.transform.localPosition = new Vector3(questBoxObj.transform.localPosition.x, 368f - (MISSOIN_SPACE_SIZE * btnMissionList.Count), questBoxObj.transform.localPosition.z);
        }
        else
        {
            emptyText_m.gameObject.SetActive(true);
            emptyText_m.text = Global._instance.GetString("p_e_7");
            //미션 없으면 no Mission 글자 밑에 배치.
            questBoxObj.transform.localPosition = new Vector3(questBoxObj.transform.localPosition.x, 368f - (MISSOIN_SPACE_SIZE), questBoxObj.transform.localPosition.z);
        }

        //퀘스트수에 따라 empty 메세지 출력.
        if (addQuestList.Count > 0)
        {
            emptyText_q.gameObject.SetActive(false);
        }
        else
        {
            emptyText_q.gameObject.SetActive(true);
            emptyText_q.text = Global._instance.GetString("p_e_4");
        }
        
        //미션 생성된 순서대로 인덱스 저장.
        int btnIndex = 0;

        //addMission이 있으면 아이템 인덱스 순서대로 주고, 등장 연출 재생.
        if (addMissionList.Count > 0)
        {
            CoActionMissionInit();
            yield return new WaitForSeconds(0.3f);
            btnIndex = addMissionList.Count;
        }

        //btnMission 아이템 인덱스 순서대로 줌.
        for (int i = 0; i < btnMissionList.Count; i++)
        {
            btnMissionList[i].itemIndex = btnIndex;
            btnIndex++;
        }

        //addMission 들 btn 리스트에 넣음(전체 한번에 관리하기 위해).
        for (int i = 0; i < addMissionList.Count; i++)
        {
            btnMissionList.Add(addMissionList[i]);
        }
        addMissionList.Clear();
    }

    public void DoBtnMission(int in_index, float _fBtnRemoveTime = 0.3f, bool bRemoveItem = true)
    {
        int deleteIndex = -1;
        int nCount = btnMissionList.Count;

        //미션 아이템 없어지는 연출 후 창 닫힘.
        if (bRemoveItem == true)
        {
            for (int i = 0; i < nCount; i++)
            {
                UIItemDiaryMission btn = btnMissionList[i];
                if (btn.mData.index == in_index)
                {
                    deleteIndex = btn.itemIndex;
                    if (_fBtnRemoveTime > 0)
                    {
                        BtnMissionScaleX(btn, 0f, 0f, _fBtnRemoveTime);
                    }
                    StartCoroutine(CoAction(_fBtnRemoveTime, () =>
                    {
                        btnMissionList.Remove(btnMissionList[i]);
                        Destroy(btn.gameObject);
                    }));
                    break;
                }
            }

            if (deleteIndex > -1)
            {
                MoveProgressBar(_fBtnRemoveTime);
                //현재 다이어리 전체 연출(남은 미션들 위로 올라가고, 팝업 창 닫히고)
                DoMissionAction(deleteIndex, in_index, _fBtnRemoveTime);
            }
        }

        //바로 미션 창 닫힘.
        else
        {
            DoMissionAction(deleteIndex, in_index, _fBtnRemoveTime, bRemoveItem);
        }
    }

    public void DoBtnQuest(int in_index, float _fBtnRemoveTime = 0.3f)
    {
        int deleteIndex = -1;
        int nCount = addQuestList.Count;

        //퀘스트 아이템 없어지는 연출 후 창 닫힘.
        for (int i = 0; i < nCount; i++)
        {
            UIItemDiaryQuest btn = addQuestList[i];
            if (btn.qData.index == in_index)
            {
                deleteIndex = i;
                if (_fBtnRemoveTime > 0)
                {
                    BtnQuestScaleX(btn, 0f, 0f, _fBtnRemoveTime);
                }

                //이벤트 타입일 경우, ui갱신.
                if (btn.qData.valueTime1 > 0)
                {
                    GetQuestEventItem();
                }

                StartCoroutine(CoAction(_fBtnRemoveTime, () =>
                {
                    addQuestList.Remove(addQuestList[i]);
                    Destroy(btn.gameObject);
                }));
                break;
            }
        }

        if (deleteIndex > -1)
        {
            //현재 다이어리 전체 연출(남은 미션들 위로 올라가고, 팝업 창 닫히고)
            DoQuestAction(deleteIndex, _fBtnRemoveTime);
        }
        else
        {
            //터치 가능.
            UIPopupDiary._instance.bCanTouch = true;
        }
    }

    //튜토리얼에서 미션 버튼 위치 가져올 때 사용.
    public GameObject GetMissionButton()
    {
        if (btnMissionList.Count > 0)
        {
           return btnMissionList[0].btnClearObject.gameObject;
        }
        return null;
    }

    //튜토리얼에서 퀘스트 버튼 위치 가져올 때 사용.
    public GameObject GetQuestButton()
    {
        for (int i = 0; i < addQuestList.Count; i++)
        {
            if (addQuestList[i].qData.index == 1)
                return addQuestList[i].btnGetObject;
        }
        return null;
    }

    //튜토리얼에서 미션 버튼 위치 가져올 때 사용.
    public Vector3 GetMissionButtonPosition()
    {
        if (btnMissionList.Count > 0)
        {
            return btnMissionList[0].buttonClear.transform.position;
        }
        return Vector3.zero;
    }

    //튜토리얼에서 퀘스트 버튼 위치 가져올 때 사용.
    public Vector3 GetQuestButtonPosition()
    {
        for (int i = 0; i < addQuestList.Count; i++)
        {
            if(addQuestList[i].qData.index == 1)
                return addQuestList[i].getText.transform.position;
        }
        return Vector3.zero;
    }

    //튜토리얼에서 미션 아이템 가져올 때 사용.
    public Transform GetMissionItem()
    {
        if (btnMissionList.Count > 0)
        {
            return btnMissionList[0].transform;
        }
        return null;
    }

    public UIItemDiaryMission GetMissionItem(int mission)
    {
        var missionItem = btnMissionList.Find((item) => item.mData.index == mission);
        return missionItem;
    }

    //튜토리얼에서 퀘스트 아이템 가져올 때 사용.
    public Transform GetQuestItem()
    {
        for (int i = 0; i < addQuestList.Count; i++)
        {
            if (addQuestList[i].qData.index == 1)
                return addQuestList[i].transform;
        }
        return null;
    }

    //유저가 확인 안 한 경우, 미션 생성 연출.
    private void CoActionMissionInit()
    {
        //이미 유저가 확인한 미션들 아래로 이동.
        MoveBtnMissionPos(true);
        StartCoroutine(CoAction(0.1f, () =>
        {
            //유저가 아직 확인안한 미션들 생성 연출.
            for (int i = 0; i < addMissionList.Count; i++)
            {
                addMissionList[i].itemIndex = i;
                BtnMissionScaleX(addMissionList[i], 1f, 1f, 0.3f);
            }
        }));
    }

    private void InitMissionProgress(int clearMissionCount)
    {
        if (dayMissionCount == 0)
        {
            progressOffset = 100f;
        }
        else
        {
            progressOffset = 100f / dayMissionCount;
        }
        progressBar.value = (clearMissionCount * progressOffset) * 0.01f;

        int percent = (int)(clearMissionCount * progressOffset);
        if (percent > 100)
        {
            percent = 100;
        }
        progressPercent.text = string.Format("{0}%", percent);
    }
   
    private void DoMissionAction(int deleteIndex, int in_index, float _fRemoveTime = 0.3f, bool bRemoveItem = true)
    {        
        if (bRemoveItem == true)
        {
            // RemoveBtnMission() 함수 작동 후.
            StartCoroutine(CoAction(_fRemoveTime, () =>
            {
                //미션 박스 올라가는 연출 시간 후에 다음 씬 실행되고 팝업 닫힘.
                float _startTime = 0.0f;
                _startTime = MoveBtnMissionPos(false, deleteIndex);
                StartCoroutine(CoAction(_startTime, () =>
                {
                    var missionBackup = this.backupMissionData;
                    var dayBackup = this.backupDay;
                    this.backupMissionData = null;
                    this.backupDay = null;
                    
                    MissionData mData = ManagerData._instance._missionData[in_index];

                    int sceneIndex = mData.sceneIndexWakeup;

                    if (mData.stepClear > 1)// 단계미션의 경우,설정된 웨이크씬에서 하나씩 증가 시키면서 발동
                    {
                        sceneIndex = mData.sceneIndexWakeup + mData.clearCount - 1;
                    }
                    else if (mData.waitTime > 0)    // 시간 미션일 경우
                    {
                        if (Global.LeftTime(mData.clearTime) <= 0)
                            sceneIndex = mData.sceneIndexWakeup + 1;
                    }

                    if (dayCompleteReward != null)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.jewel,
                            5,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_DAY_CLEAR_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                            $"DAYCLEAR_{mData.day}"
                            );

                        ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), () => dayCompleteReward = null, dayCompleteReward);
                        Global.jewel = (int)(GameData.Asset.AllJewel);
                        ManagerUI._instance.UpdateUI();
                    }

                    StartCoroutine(CoAction(() => dayCompleteReward == null, () => {
                        var landMap = ServerContents.Day.CreateAreaToLandMap();
                        int landIdx = 0;
                        if (landMap.TryGetValue(mData.sceneArea, out landIdx))
                        {
                            if (ManagerLobby.landIndex != landIdx)
                            {
                                ReservedScene scn = new ReservedScene()
                                {
                                    scnStartDay = mData.day,
                                    scnDestDay = Global.day,
                                    orgDay = dayBackup,
                                    missionBackup = missionBackup,
                                    areaIndex = mData.sceneArea,
                                    sceneIndex = sceneIndex,
                                    missionIndex = in_index
                                };
                                ManagerLobby._instance.MoveLand(landIdx, scn);
                                return;
                            }
                        }
                        ManagerLobby._instance.PlaySceneWakeUp(ManagerArea._instance._areaStep[mData.sceneArea].area, sceneIndex, in_index);
                        ManagerCinemaBox._instance.completeCinemaCallback += UIDiaryController._instance.DiaryActionComplete;
                        LobbyEntryFocus.ResetFocusCandidates();
                        ManagerUI._instance.ClosePopUpUI();
                        ManagerUI._instance.ClosePopUpUI();
                    }));
                }));
            }));
        }
        else
        {
            var missionBackup = this.backupMissionData;
            var dayBackup = this.backupDay;
            this.backupMissionData = null;
            this.backupDay = null;
            MissionData mData = ManagerData._instance._missionData[in_index];
            int sceneIndex = mData.sceneIndexWakeup;

            if (mData.stepClear > 1)// 단계미션의 경우,설정된 웨이크씬에서 하나씩 증가 시키면서 발동
            {
                sceneIndex = mData.sceneIndexWakeup + mData.clearCount - 1;
            }
            else if (mData.waitTime > 0)    // 시간 미션일 경우
            {
                if (mData.state == TypeMissionState.Clear)
                    sceneIndex = mData.sceneIndexWakeup + 1;
            }

            var landMap = ServerContents.Day.CreateAreaToLandMap();
            int landIdx = 0;
            if (landMap.TryGetValue(mData.sceneArea, out landIdx))
            {
                if (ManagerLobby.landIndex != landIdx)
                {
                    ReservedScene scn = new ReservedScene()
                    {
                        scnStartDay = mData.day,
                        scnDestDay = Global.day,
                        orgDay = dayBackup,
                        missionBackup = missionBackup,
                        areaIndex = mData.sceneArea,
                        sceneIndex = sceneIndex,
                        missionIndex = in_index
                    };
                    ManagerLobby._instance.MoveLand(landIdx, scn);
                    return;
                }
            }
            ManagerLobby._instance.PlaySceneWakeUp(ManagerArea._instance._areaStep[mData.sceneArea].area, sceneIndex, in_index);

            ManagerCinemaBox._instance.completeCinemaCallback += UIDiaryController._instance.DiaryActionComplete;
            LobbyEntryFocus.ResetFocusCandidates();
            ManagerUI._instance.ClosePopUpUI();
        }
    }
    

    private void GetQuestEventItem()
    {
        eventQuestCount--;
    }

    private void DoQuestAction(int deleteIndex, float _fRemoveTime = 0.3f)
    {
        float startTime = 0.0f;
        // RemoveBtnMission() 함수 작동 후.
        StartCoroutine(CoAction(_fRemoveTime, () =>
        {
            //퀘스트 올라가는 연출.
            startTime = MoveBtnQuestPos(false, deleteIndex);
        }));

        //연출 후 터치 가능.
        StartCoroutine(CoAction(startTime, () =>
        {
            //터치 가능.
            UIPopupDiary._instance.bCanTouch = true;
        }));
    }

    void OnClickedQuestInfo()
    {
        if (!UIPopupDiary._instance.bCanTouch)
            return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_9"), Global._instance.GetString("n_s_32"), false, null);
    }

    #region MissionBoxAction(미션 연출 관련한 함수들).
    private float MoveBtnMissionPos(bool bMoveDown, int targetIndex = -1)
    {
        int nCount = btnMissionList.Count;
        for (int i = 0; i < nCount; i++)
        {
            if (btnMissionList[i].itemIndex > targetIndex)
            {
                SetBtnMissionPosY(bMoveDown, btnMissionList[i], addMissionList.Count, 0.2f);
            }
        }
        float yPos = 0f;
        if (bMoveDown == false)
        {
            yPos = questBoxObj.transform.localPosition.y + MISSOIN_SPACE_SIZE;
        }
        else
        {
            yPos = questBoxObj.transform.localPosition.y - (MISSOIN_SPACE_SIZE * addMissionList.Count);
        }
        SetQuesetBoxPosY(yPos, 0.2f);
        //버튼 올라가는 시간 반환.
        return 0.3f;
    }

    private float MoveBtnQuestPos(bool bMoveDown, int targetIndex = 0)
    {
        int nCount = addQuestList.Count;
        for (int i = targetIndex; i < nCount; i++)
        {
            SetBtnQuestPosY(bMoveDown, addQuestList[i], addMissionList.Count, 0.2f);
        }
        //버튼 올라가는 시간 반환.
        return 0.3f;
    }

    private void BtnMissionScaleX(UIItemDiaryMission btn, float xScale, float alpha,  float _mainDelay = 0.0f)
    {
        btn.transform.DOScaleX(xScale, _mainDelay);
        DOTween.ToAlpha(() => btn.mainSprite.color, x => btn.mainSprite.color = x, alpha, _mainDelay);
    }

    private void BtnQuestScaleX(UIItemDiaryQuest btn, float xScale, float alpha, float _mainDelay = 0.0f)
    {
        btn.transform.DOScaleX(xScale, _mainDelay);
        DOTween.ToAlpha(() => btn.mainSprite.color, x => btn.mainSprite.color = x, alpha, _mainDelay);
    }

    private void SetBtnMissionPosY(bool bMoveDown, UIItemDiaryMission _btnMission, int moveSize, float _mainDelay = 0.0f)
    {
        float y = MISSOIN_SPACE_SIZE;

        //아래로 내려갈 때에는 (위쪽에 생성될 미션 수 * 간견)만큼 움직임.
        //위로 올라갈 때는 한칸씩만 올라감.
        if (bMoveDown == true)
        {
            y = (-1 * MISSOIN_SPACE_SIZE) * moveSize;
        }
        _btnMission.transform.DOLocalMoveY(_btnMission.transform.localPosition.y + y, _mainDelay, true).SetEase(Ease.Linear);
    }

    private void SetBtnQuestPosY(bool bMoveDown, UIItemDiaryQuest btnQuest, int moveSize, float _mainDelay = 0.0f)
    {
        float y = MISSOIN_SPACE_SIZE;

        //아래로 내려갈 때에는 (위쪽에 생성될 미션 수 * 간견)만큼 움직임.
        //위로 올라갈 때는 한칸씩만 올라감.
        if (bMoveDown == true)
        {
            y = (-1 * MISSOIN_SPACE_SIZE) * moveSize;
        }
        btnQuest.transform.DOLocalMoveY(btnQuest.transform.localPosition.y + y, _mainDelay, true).SetEase(Ease.Linear);
    }

    private void SetQuesetBoxPosY(float yPos, float _mainDelay = 0.0f)
    {
        questBoxObj.transform.DOLocalMoveY(yPos, _mainDelay, true).SetEase(Ease.Linear);
    }

    public void MoveProgressBar(float _mainDelay)
    {
        float targetValue = progressBar.value + (progressOffset * 0.01f);
        DOTween.To(() => progressBar.value, x => progressBar.value = x, targetValue, _mainDelay).SetEase(Ease.Flash);

        clearMissionCount += 1;
        int percent = (int)(clearMissionCount * progressOffset);
        if (percent > 100)
        {
            percent = 100;
        }

        if (clearMissionCount == dayMissionCount)
        {
            percent = 100;
        }

        progressPercent.text = string.Format("{0}%", percent);
    }

    private IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }

    private IEnumerator CoAction(Func<bool> in_callback = null, UnityAction _action = null)
    {
        yield return new WaitUntil(in_callback);
        _action();
    }

    #endregion
}
