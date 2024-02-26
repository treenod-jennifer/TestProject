using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupDiary : UIPopupBase
{
    public static UIPopupDiary _instance = null;

    //public UIDiaryTap[] arrayDiaryTap;

    public GameObject storage_newIcon;
    public GameObject stamp_newIcon;

   // private UIDiaryTap currentTap = null;
    TypePopupDiary diaryType = TypePopupDiary.eNone;

    //현재 다이어리 창에 띄워진 오브젝트.
    GameObject currentObject;

    void Awake()
    {
        _instance = this;
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public void Init(TypePopupDiary in_type)
    {/*
        //현재 다이어리 타입과 들어온 타입이 같은 경우에는 동작 안함.
        if (diaryType == in_type)
        {
            return;
        }
        Destroy(currentObject);
        diaryType = in_type;

        if (diaryType == TypePopupDiary.eMission)
        {
            SetCurrentBtn(0);
            CheckHousingEvent();
         //   UIDiaryMission diaryMission = NGUITools.AddChild(mainSprite.gameObject, ManagerUI._instance._objDiaryMission).GetComponent<UIDiaryMission>();
            diaryMission.uiPanel.depth = uiPanel.depth + 1;
            currentObject = diaryMission.gameObject;
        }

        else if (diaryType == TypePopupDiary.eStorage)
        {
            SetCurrentBtn(1);
         //   UIDiaryStorage diaryStorage = NGUITools.AddChild(mainSprite.gameObject, ManagerUI._instance._objDiaryStorage).GetComponent<UIDiaryStorage>();
          //  diaryStorage.scrollView.panel.depth = uiPanel.depth + 1;
            currentObject = diaryStorage.gameObject;
        }

        else if (diaryType == TypePopupDiary.eStamp)
        {
            SetCurrentBtn(2);
          //  UIDiaryStamp diaryStamp = NGUITools.AddChild(mainSprite.gameObject, ManagerUI._instance._objDiaryStamp).GetComponent<UIDiaryStamp>();
           // diaryStamp.uiPanel.depth = uiPanel.depth + 1;
         //   currentObject = diaryStamp.gameObject;
        }

        else if (diaryType == TypePopupDiary.eCostume)
        {
            SetCurrentBtn(3);
         //   UIDiaryCostume diaryCostume 
       //         = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIDiaryCostume", mainSprite.gameObject).GetComponent<UIDiaryCostume>();
            diaryCostume.scrollView.panel.depth = uiPanel.depth + 1;
            currentObject = diaryCostume.gameObject;
        }*/
    }

    #region New 아이콘 생성/제거.
    public void SettingStorageNewIcon(bool bActive)
    {
        storage_newIcon.SetActive(bActive);
    }

    public void SettingStampNewIcon(bool bActive)
    {
        stamp_newIcon.SetActive(bActive);
    }
    #endregion

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

    public void SetCurrentBtn(int _index)
    {/*
        if (currentTap != null)
        {
            currentTap.SetDiaryTap(false);
        }
        currentTap = arrayDiaryTap[_index];
        currentTap.SetDiaryTap(true);*/
    }

    public void SettingEventTap(int index, bool bEvent)
    {/*
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
        ManagerUI._instance.SettingEventIcon(bEventNow);*/
    }

    private void CheckHousingEvent()
    {/*
        //이벤트 하우징 열리는 조건이 아닐 경우는 검사 안 함.
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventHousingOpen)
            return;

        var enumerator = ManagerData._instance._housingGameData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            PlusHousingModelData hData = enumerator.Current.Value;
            if (hData.type != PlusHousingModelDataType.byProgress)
                continue;

            if (hData.active == 0)
            {
                var missionIndex = hData.housingProgressData.mission;
                if (!ManagerData._instance._missionData.ContainsKey(missionIndex))
                {
                    continue;
                }

                if (hData.type == PlusHousingModelDataType.byProgress && hData.expire_ts >= Global.GetTime())
                {
                    SettingEventTap(1, true);
                    return;
                }
            }
        }*/
    }
}
