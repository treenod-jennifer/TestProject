using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstallabelGuestData
{
    public int characterIndex = 0;
    public bool isNewCharacter = false;

    public InstallabelGuestData(int charIdx, bool isNew)
    {
        this.characterIndex = charIdx;
        this.isNewCharacter = isNew;
    }
}

public class UIDiaryGuest : UIDiaryBase
{
    public static UIDiaryGuest _instance = null;
    public UIPanel uiPanel;
    public UIReuseGrid_DiaryGuest scroll;

    //로비에 배치 중인 캐릭터.
    private List<int> listSelectedGuest = new List<int>();
    //로비에 배치 가능한 캐릭터.
    private List<InstallabelGuestData> listInstallableGuest = new List<InstallabelGuestData>();
    //신규 캐릭터 알림.
    private List<int> listNewComers = new List<int>();

    #region 로비에 배치된 캐릭터 탭
    public UILabel[] text_select;
    public UILabel allCount;
    public UILabel nowCount;
    public UILabel emptyText_select;
    public GameObject lobbyGuestRoot;
    public UIItemLobbyGuest[] itemLobbyGuests;
    #endregion

    #region 로비에 배치 가능한 캐릭터 리스트 탭
    public UILabel[] text_installable;
    public UILabel emptyText_installable;
    #endregion

    private int selectIndex = -1;
    
    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Start()
    {
        InitDiaryGuest();
    }

    private void InitDiaryGuest()
    {
        InitGuestData();
        InitGuestAlarm();
        InitSelectableTap();
        InitInstallableGuestTap();
        StartCoroutine(scroll.InitReuseGrid());
    }

    #region 로비 배치 캐릭터 데이터 로드.
    private void InitGuestData()
    {
        if (ManagerAIAnimal.instance == null)
            return;

        listSelectedGuest.Clear();
        listInstallableGuest.Clear();
        listNewComers.Clear();
        
        listSelectedGuest = ManagerAIAnimal.GetSelected();
        listNewComers = ManagerAIAnimal.GetNewComers();
        //설치가능한 캐릭터의 신규 정보까지 받아서 리스트에 넣어줌.
        List<int> listGuests = ManagerAIAnimal.GetInstallables();
        for (int i = 0; i < listGuests.Count; i++)
        {
            bool isNewGuest = false;
            if (listNewComers.Count > 0 && (listNewComers.FindIndex(x => x == listGuests[i]) != -1))
                isNewGuest = true;

            InstallabelGuestData guestData = new InstallabelGuestData(listGuests[i], isNewGuest);
            listInstallableGuest.Add(guestData);
        }
    }

    private void UpdateGuestData(int beforeIndex, int afterIndex)
    {
        int selectIndex = listSelectedGuest.FindIndex(x => x == beforeIndex);
        listSelectedGuest[selectIndex] = afterIndex;

        int installableIndex = listInstallableGuest.FindIndex(x => x.characterIndex == afterIndex);
        listInstallableGuest[installableIndex].characterIndex = beforeIndex;
        listInstallableGuest[installableIndex].isNewCharacter = false;
    }   
    #endregion

    #region 로비에 배치된 캐릭터 탭
    private void InitSelectableTap()
    {
        allCount.text = string.Format("/{0}", ManagerAIAnimal.GetMaxLobbyInstallCount());
        nowCount.text = listSelectedGuest.Count.ToString();

        string tapText = Global._instance.GetString("p_d_f_1");
        for (int i = 0; i < text_select.Length; i++)
        {
            text_select[i].text = tapText;
        }

        if (listSelectedGuest.Count == 0)
        {
            InitEmpty_SelectTap();
        }
        else
        {
            emptyText_select.gameObject.SetActive(false);
            InitSelectCharacter();
        }
    }

    private void InitEmpty_SelectTap()
    {
        emptyText_select.text = Global._instance.GetString("p_d_f_5");
        emptyText_select.gameObject.SetActive(true);
        lobbyGuestRoot.gameObject.SetActive(false);
    }

    private void InitSelectCharacter()
    {
        bool isCanChange = (listInstallableGuest.Count > 0) ? true : false;
        for (int i = 0; i < itemLobbyGuests.Length; i++)
        {
            if (listSelectedGuest.Count <= i)
            {
                itemLobbyGuests[i].InitDefaultItem(i);
            }
            else
            {
                itemLobbyGuests[i].InitItem(i, listSelectedGuest[i], isCanChange);
                itemLobbyGuests[i].selectCharacterAction = SelectChangeCharacter;
                itemLobbyGuests[i].selectCancelAction = SelectCancelCharacter;
            }
        }
    }
    
    //캐릭터 선택했을 때의 콜백.
    private void SelectChangeCharacter(int listIndex)
    {
        if (selectIndex == listIndex)
            return;
        selectIndex = listIndex;
        for (int i = 0; i < itemLobbyGuests.Length; i++)
        {
            if (i == selectIndex)
                continue;
            itemLobbyGuests[i].SetItemState_Inactive();
        }
        UpdateInstallableItem();
    }

    //캐릭터 선택 취소 했을 때의 콜백.
    private void SelectCancelCharacter()
    {
        SetItemLobbyGuestState_Normal(selectIndex);
        selectIndex = -1;
        UpdateInstallableItem();
    }

    private void UpdateInstallableItem()
    {
        if (listInstallableGuest.Count > 0)
        {
            scroll.UpdateItem();
        }
    }

    private void SetItemLobbyGuestState_Normal(int exceptIndex)
    {
        for (int i = 0; i < itemLobbyGuests.Length; i++)
        {
            if (i == exceptIndex)
                continue;
            itemLobbyGuests[i].SetItemState_Normal();
        }
    }
    #endregion

    #region 로비에 배치 가능한 캐릭터 리스트 탭
    private void InitInstallableGuestTap()
    {
        string tapText = Global._instance.GetString("p_d_f_2");
        for (int i = 0; i < text_installable.Length; i++)
        {
            text_installable[i].text = tapText;
        }

        if (listInstallableGuest.Count == 0)
        {
            InitEmpty_InstallableTap();
        }
        else
        {
            emptyText_installable.gameObject.SetActive(false);
        }
    }

    private void InitEmpty_InstallableTap()
    {
        if (listSelectedGuest.Count == 0)
        {
            emptyText_installable.text = Global._instance.GetString("p_d_f_6");
        }
        else
        {
            emptyText_installable.text = Global._instance.GetString("p_d_f_7");
        }
        emptyText_installable.gameObject.SetActive(true);
    }

    private void OnClickBtnChangeGuestInfo()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_d_f_3"), false);
        popup.SortOrderSetting();
    }
    #endregion

    public int GetGuestCount()
    {
        int count = listInstallableGuest.Count / 3;
        if (listInstallableGuest.Count % 3 != 0)
            count += 1;
        return count;
    }

    //필요한 로비 동물 리스트 3개 들고오는 함수.
    public List<InstallabelGuestData> GetListGuestIndex(int _nIndex)
    {
        List<InstallabelGuestData> listGuestIndex = new List<InstallabelGuestData>();
        int firstIndex = 3 * _nIndex;

        for (int i = 0; i < 3; i++)
        {   
            if (firstIndex + i >= listInstallableGuest.Count)
                break;

            listGuestIndex.Add(listInstallableGuest[firstIndex + i]);
        }
        return listGuestIndex;
    }

    public bool IsCanChangeCharacter()
    {
        return (selectIndex > -1);
    }

    #region 신규 동물 알람 초기화
    private void InitGuestAlarm()
    {
        if (ManagerAIAnimal.HaveNewcomer() == true)
        {
            ManagerAIAnimal.MarkAsRead();
            UIDiaryController._instance.SetBtnDiaryAlarm();
            if (UIButtonLobbyGuest._instance != null)
                UIButtonLobbyGuest._instance.DestroyImmetiately();
        }
    }
    #endregion

    #region 캐릭터 교체 팝업(UIPopupLobbyGuestChange.cs) 관련
    public void OpenPopupLobbyGuestChange(int changeIndex)
    {
        ManagerUI._instance.OpenPopupLobbyGuestChange(listSelectedGuest[selectIndex], changeIndex, ChangeLobbyGuest);
    }

    //로비에 배치된 캐릭터 변경 시키는 함수.
    private void ChangeLobbyGuest(int beforeIndex, int afterIndex)
    {
        UIPopupDiary._instance.bCanTouch = false;
        StartCoroutine(CoChangeLobbyGuest(beforeIndex, afterIndex));
    }

    private IEnumerator CoChangeLobbyGuest(int beforeIndex, int afterIndex)
    {
        selectIndex = -1;
        yield return ManagerAIAnimal.instance.ChangeInstalled(beforeIndex, afterIndex);

        //데이터 갱신
        UpdateGuestData(beforeIndex, afterIndex);

        //캐릭터 갱신.
        UpdateItemLobbyGuest();

        //스크롤 갱신
        UpdateInstallableItem();

        yield return null;
        UIPopupDiary._instance.bCanTouch = true;
    }

    private void UpdateItemLobbyGuest()
    {
        bool isCanChange = (listInstallableGuest.Count > 0) ? true : false;
        for (int i = 0; i < itemLobbyGuests.Length; i++)
        {
            if(i < listSelectedGuest.Count)
                itemLobbyGuests[i].UpdateItem(listSelectedGuest[i], isCanChange);
        }
    }
    #endregion
}
