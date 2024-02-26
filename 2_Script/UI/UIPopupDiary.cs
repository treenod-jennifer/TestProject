using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupDiary : UIPopupBase
{
    public static UIPopupDiary _instance = null;

    public UIDiaryTap[] arrayDiaryTap;

    public GameObject storage_newIcon;
    public GameObject stamp_newIcon;

    private UIDiaryTap currentTap = null;

    private int currentTapIdx = 0;

    TypePopupDiary diaryType = TypePopupDiary.eNone;

    //현재 다이어리 창에 띄워진 오브젝트.
    GameObject currentObject;

    [SerializeField]
    UISprite check;
    [SerializeField]
    bool openAtLobbyIn = true;
    [SerializeField]
    UILabel labelOpenAtLobbyIn = null;
    [SerializeField]
    public GameObject btnOpenAtLobbyInRoot = null;

    [System.NonSerialized]
    static public bool openAtLobbyOptionEnabled = true;

    void Awake()
    {
        _instance = this;
    }

    new void OnDestroy()
    {
        if(UIDiaryController._instance != null)
        {
            UIDiaryController._instance.UpdateQuestData(false);
            UIDiaryController._instance.SetttingDiaryTapNullData();
        }

        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        //유저가 뒤로 가기 버튼을 눌러서 팝업을 닫은 경우는, 다이어리 연출이 완전히 종료됐다고 판단.
        _callbackEnd += UIDiaryController._instance.DiaryActionComplete;
        base.OnClickBtnClose();
    }

    public void Init(TypePopupDiary in_type, int itemIndex = -1, int tapIndex = 0)
    {   
        btnOpenAtLobbyInRoot.SetActive(openAtLobbyOptionEnabled);

        // 설정된 적 없으면 기본값 false
        this.openAtLobbyIn = PlayerPrefs.HasKey("DoNotOpenDiaryAtLobbyIn") == false ? false : PlayerPrefs.GetInt("DoNotOpenDiaryAtLobbyIn") != 1;
        SetCheckDiaryOpen();
        labelOpenAtLobbyIn.text = Global._instance.GetString("p_d_1");

        //현재 다이어리 타입과 들어온 타입이 같은 경우에는 동작 안함.
        if (diaryType == in_type)
        {
            return;
        }

        //현재 랜드가 MainLand가 아닐 때 데코 탭 비 활성화
        arrayDiaryTap[4].InitTapLock(ManagerLobby.landIndex != 0, out bool isTapLock);

        UIDiaryController._instance.SettingDiaryTap(arrayDiaryTap);
        Destroy(currentObject);
        diaryType = in_type;

        if (diaryType == TypePopupDiary.eMission)
        {
            //알림 제거.
            if (UIDiaryController._instance != null)
            {
                UIDiaryController._instance.RemoveAllQuestAlarmData();
            }
            SetCurrentBtn(0);
            CheckHousingEvent();
            UIDiaryMission diaryMission = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryMission", mainSprite.gameObject).GetComponent<UIDiaryMission>();
            diaryMission.uiPanel.depth = uiPanel.depth + 1;
            currentObject = diaryMission.gameObject;
        }
        else if (diaryType == TypePopupDiary.eStorage)
        {
            //if (isTapLock) return;

            SetCurrentBtn(1);

            //구버전 하우징
            
            //UIDiaryStorage diaryStorage = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryStorage", mainSprite.gameObject).GetComponent<UIDiaryStorage>();
            //diaryStorage.scrollView.panel.depth = uiPanel.depth + 1;
            ////콜백으로 화면 모두 켜진 뒤, 스크롤 위치 설정.
            //if (itemIndex > -1)
            //{
            //    _callbackOpen += () => { diaryStorage.SetScroll(itemIndex, tapIndex); };
            //}
            //currentObject = diaryStorage.gameObject;

            //신버전 하우징

            UIDiaryDeco diaryDeco
               = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryDeco", mainSprite.gameObject).GetComponent<UIDiaryDeco>();
            diaryDeco.InitPanel(uiPanel.depth + 1);
            currentObject = diaryDeco.gameObject;

        }
        else if (diaryType == TypePopupDiary.eStamp)
        {
            SetCurrentBtn(2);
            UIDiaryStamp diaryStamp = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryStamp", mainSprite.gameObject).GetComponent<UIDiaryStamp>();
            diaryStamp.uiPanel.depth = uiPanel.depth + 1;
            currentObject = diaryStamp.gameObject;
            UIDiaryController._instance.OnClickStampTap();
        }
        else if (diaryType == TypePopupDiary.eCostume)
        {
            SetCurrentBtn(3);
            CheckHousingEvent();
            UIDiaryCostume diaryCostume 
                = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryCostume", mainSprite.gameObject).GetComponent<UIDiaryCostume>();
            diaryCostume.scrollView.panel.depth = uiPanel.depth + 1;
            currentObject = diaryCostume.gameObject;
        }
        else if (diaryType == TypePopupDiary.eGuest)
        {
            SetCurrentBtn(4);
            UIDiaryGuest diaryGuest
                = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryGuest", mainSprite.gameObject).GetComponent<UIDiaryGuest>();
            diaryGuest.uiPanel.depth = uiPanel.depth + 1;
            currentObject = diaryGuest.gameObject;
        }
    }

    void OnClickBtnMission()
    {
        if (diaryType != TypePopupDiary.eMission && bCanTouch == true)
        {
            Init(TypePopupDiary.eMission);
        }
    }

    void OnClickBtnStampBox()
    {
        if (diaryType != TypePopupDiary.eStamp && bCanTouch == true)
        {
            Init(TypePopupDiary.eStamp);
        }
    }

    void OnClickBtnStorageBox()
    {
        if (diaryType != TypePopupDiary.eStorage && bCanTouch == true)
        {
            Init(TypePopupDiary.eStorage);
        }
    }

    void OnClickBtnCostume()
    {
        if (diaryType != TypePopupDiary.eCostume && bCanTouch == true)
        {
            Init(TypePopupDiary.eCostume);
        }
    }

    void OnClickBtnGuest()
    {
        if (diaryType != TypePopupDiary.eGuest && bCanTouch == true)
        {
            Init(TypePopupDiary.eGuest);
        }
    }

    public void SetCurrentBtn(int _index)
    {
        if (currentTap != null)
        {
            currentTap.SetDiaryTap(false);
        }
        //탭이 변경될 때, 원래 선택되어 있던 탭의 알림 검사.
        UIDiaryController._instance.InitDiaryTapAlarmIcon(currentTapIdx);

        currentTapIdx = _index;
        currentTap = arrayDiaryTap[currentTapIdx];
        currentTap.SetDiaryTap(true);

        //현재 선택된 탭의 알림 아이콘 안보이게 설정.
        UIDiaryController._instance.SetDiaryTapAlarmIcon(currentTapIdx, false);
    }

    public void SettingEventTap(int index, bool bEvent)
    {
        if (arrayDiaryTap.Length <= index)
            return;

        arrayDiaryTap[index].SettingEventIcon(bEvent);
        bool bEventNow = false;
        for (int i = 0; i < arrayDiaryTap.Length; i++)
        {
            if (arrayDiaryTap[i] != null)
            {
                if (arrayDiaryTap[i].CheckEventIconState() == true)
                {
                    bEventNow = true;
                    break;
                }
            }
        }

        //현재 남아있는 이벤트 상태에 따라 다이어리 창의 이벤트 아이콘 설정.
        ManagerUI._instance.SettingEventIcon(bEventNow);
    }

    private void CheckHousingEvent()
    {
        if( ManagerHousing.GetUnfinishedEventItemCount() > 0 )
        {
            SettingEventTap(1, true);
        }
    }

    void OnClickBtnCheck()
    {
        if (this.openAtLobbyIn == false)
            this.openAtLobbyIn = true;
        else
            this.openAtLobbyIn = false;

        SetCheckDiaryOpen();

        if (openAtLobbyIn == false)
            PlayerPrefs.SetInt("DoNotOpenDiaryAtLobbyIn", 1);
        else
            PlayerPrefs.SetInt("DoNotOpenDiaryAtLobbyIn", 0);

    }

    private void SetCheckDiaryOpen()
    {
        if (this.openAtLobbyIn == false)
            check.spriteName = "ready_button_01_off";
        else
            check.spriteName = "ready_button_01_on";
    }

    public UIItemDiaryMission GetMissionItem(int mission)
    {
        if (diaryType != TypePopupDiary.eMission) return null;

        UIDiaryMission uiDiaryMission = currentObject.GetComponent<UIDiaryMission>();

        if (uiDiaryMission == null) return null;

        return uiDiaryMission.GetMissionItem(mission);
    }
}
