using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;
using Protocol;

public class UIPopupUserProfile : UIPopupBase
{
    public static UIPopupUserProfile _instance = null;
    private const int pokoValue = 0;

    //포코고로 변경되고 나서 호출되는 콜백.
    public Method.FunctionVoid changeCallBack = null;

    public UIScrollView decoScroll;
    public UIScrollView pokoyuraScroll;

    //유저 프로필.
    public UILabel[]    title;
    public UITexture    pokogoro;
    public UILabel      userName;
    public UILabel      flower;
    public UILabel      stage;
    public UILabel      day;

    //유저 프로필 변경버튼
    public GameObject profileButton;

    //포코유라 체크.
    public GameObject check;

    //버튼(좌우 각각 2개씩)
    public GameObject[] decoButtons;
    public GameObject[] pokoyuraButtons;
    
    //데코,포코유라 리스트 오브젝트.
    public GameObject _objItemUserProfileDecoList;
    public GameObject _objItemUserProfilePokoYuraList;

    //데코, 포코유라 데이터 리스트.
    private List<ServerUserHousingItem> decoList = new List<ServerUserHousingItem>();
    private List<int> pokoyuraList = new List<int>();
    
    //데코, 포코유라 화면 인덱스.
    private int decoIndex;
    private int pokoIndex;
    private int decoMaxIndex;
    private int pokoMaxIndex;

    //유저인지 정보 저장 & 대표유라.
    private int selectYura = 0;
    private long userRank = 0;
    private bool bUser = false;

    //프로필 아이템
    [SerializeField] private UIItemProfile profileItem;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        decoScroll.panel.depth = uiPanel.depth + 1;
        pokoyuraScroll.panel.depth = uiPanel.depth + 2;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            decoScroll.panel.useSortingOrder = true;
            pokoyuraScroll.panel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            decoScroll.panel.sortingOrder = layer + 1;
            pokoyuraScroll.panel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private UserBase userData = new UserBase();

    public void Init (ProfileLookupResp userProfile, UserBase userLineProfile)
    {
        userData = userLineProfile;

        string userKey = "";
        if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer )
        {
            userKey = "null";
        }
        else
        {
            userKey = userLineProfile._userKey;
        }

        var profileData = userData.GetPionProfile();

        //타이틀 세팅.
        string titletext = Global._instance.GetString("p_u_p_1");
        title[0].text = titletext;
        title[1].text = titletext;

        //인덱스 초기화.
        decoIndex = 1;
        pokoIndex = 1;
        decoMaxIndex = 0;
        pokoMaxIndex = 0;

        //리스트 초기화.
        decoList = new List<ServerUserHousingItem>();
        pokoyuraList = new List<int>();

        //리스트 받아오기.
        setDecoList(userProfile.housing);
        pokoyuraList = userProfile.profileLookup.toys;

        //유저 데이터 세팅.
        string nick   = userProfile.profileLookup.nickName != null && userProfile.profileLookup.nickName != string.Empty ? $"({userProfile.profileLookup.nickName})" : string.Empty;
        string name   = userLineProfile.GetTridentProfile() != null ? $"{userLineProfile.DefaultName}{nick}" : $"{nick}";
        userName.text = Global.ClipString(name, 30);
        flower.text   = userLineProfile.Flower.ToString();
        stage.text    = userLineProfile.stage.ToString();/* userProfile.profileLookup.stage.ToString();*/
        day.text      = userProfile.profileLookup.day.ToString();

        //프로필 아이템 추가
        profileItem.SetProfile(userData);

        bool bSelect = false;
        //유저 랭크 저장.
        userRank = userLineProfile.Flower;
        //toy 값이 0이 아니라면 선택된 상태.
        if (profileData != null && profileData.profile.toy != 0)
        {
            selectYura = profileData.profile.toy;
            bSelect = true;
        }
        //포코 유라 이미지 세팅.
        SettingPokoYura(bSelect);

        //버튼들 세팅(처음에는 첫 페이지기 때문에 좌측 버튼은 안보이게 세팅).
        decoButtons[0].SetActive(false);
        pokoyuraButtons[0].SetActive(false);
        if (decoList.Count <= 8)
        {
            decoButtons[1].SetActive(false);
        }
        if (pokoyuraList.Count <= 10)
        {
            pokoyuraButtons[1].SetActive(false);
        }

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        //유저인지 구분.
        bUser = false;
        if (userLineProfile._userKey == myProfile._userKey)
        {
            bUser = true;
        }

        if(bUser)
        {
            profileButton.SetActive(true);
        }
        else
        {
            profileButton.SetActive(false);
        }

        MakeDecoList();
        MakePokoYuraList();
    }

    public void OnLoadComplete(Texture2D r)
    {
        pokogoro.mainTexture = r;
        pokogoro.MakePixelPerfect();
        pokogoro.gameObject.SetActive(true);
    }

    public bool GetUser()
    {
        return bUser;
    }

    public void PokoYuraPosition(Transform tr)
    {
        //포코 유라 변경 후 check 위치 변경하는 콜백.
        changeCallBack += (() =>
        {
            SettingCheck(tr);
        });
    }

    public void SettingCheck(Transform tr)
    {
        if (selectYura != 0)
        {   
            check.transform.parent = tr;
            check.transform.localPosition = new Vector3(20f, 30f, 0f);
            check.SetActive(true);
        }
    }

    public void OnClickPokoYura(int index)
    {
        int selectIndex = 0;
        //현재 포코유라가 선택된 포코유라랑 다르다면 장착(같으면 해제).
        if (selectYura != index)
        {
            selectIndex = index;
        }
        ServerAPI.ToySet(selectIndex, recvSetToy);
    }

    void recvSetToy(ToySetResp resp)
    {
        if (resp.IsSuccess)
        {
            selectYura = resp.setIdx;

            bool bSelect = false;
            if (selectYura > 0)
                bSelect = true;
            //포코 유라 이미지 세팅.
            SettingPokoYura(bSelect);

            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

            //유저 프로필 데이터 포코유라 갱신.
            Profile_PION profileInfo = myProfile.GetPionProfile();
            if (profileInfo != null)
            {
                profileInfo.profile.toy = selectYura;
            }

            check.SetActive(false);
            changeCallBack();

            //포코유라 업데이트.
            ManagerUI._instance.SettingRankingPokoYura();
        }
    }

    private void SettingPokoYura(bool bSelect)
    {
        if (bSelect == true)
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Pokoyura", $"y_i_{selectYura}.png", OnLoadComplete);
        }
        else
        {
            int classIndex = (int) userRank / 10 + 1;
            gameObject.AddressableAssetLoadClass<Texture>(classIndex, texture =>
            {
                pokogoro.mainTexture = texture;
                pokogoro.MakePixelPerfect();
            });
        }
    }

    private void setDecoList(List<ServerUserHousingItem> housingItemList)
    {
        decoList.Clear();
        //현재 가지고 있는 하우징 중 미션으로 얻는 아이템을 제외한 아이템을 리스트에 추가.
        int nCount = housingItemList.Count;
        for (int i = 0; i < nCount; i++)
        {
            PlusHousingModelData model = ManagerHousing.GetHousingModel(housingItemList[i].index, housingItemList[i].modelIndex);
            if (model.type == PlusHousingModelDataType.none || model.type == PlusHousingModelDataType.byMission
                || housingItemList[i].active == 0)
                continue;
            decoList.Add(housingItemList[i]);
        }

        //에디터에서만.
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            decoList.Clear();
            for (int j = 0; j < 30; j++)
            {
                decoList.Add(housingItemList[0]);
            }
        }
    }

    private void MakeDecoList()
    {
        decoMaxIndex = decoList.Count / 8;
        if (decoList.Count % 8 > 0)
        {
            decoMaxIndex += 1;
        }
        
        int nCount = decoList.Count;
        for (int i = 0; i < decoMaxIndex; i++)
        {
            //현재 생성할 데코 아이템 데이터 리스트 생성.
            List<ServerUserHousingItem> hList = new List<ServerUserHousingItem>();
            for (int j = (i * 8); j < (i * 8) + 8; j++)
            {
                //리스트 끝까지 왔으면 종료.
                if (j >= nCount)
                    break;
                hList.Add(decoList[j]);
            }
            //리스트 오브젝트 생성 & 값 세팅.
            UIItemUserProfileDecoList itemDecoList =
              NGUITools.AddChild(decoScroll.gameObject, _objItemUserProfileDecoList).GetComponent<UIItemUserProfileDecoList>();
            itemDecoList.transform.localPosition = new Vector3((620f *i), 25f, 0f);
            itemDecoList.SettingItemDecoList(hList);
        }

        //리스트 없을 때 아무 데이터도 안 들어가 있는 리스트 생성.
        if (decoMaxIndex == 0)
        {
            UIItemUserProfileDecoList itemDecoList =
              NGUITools.AddChild(decoScroll.gameObject, _objItemUserProfileDecoList).GetComponent<UIItemUserProfileDecoList>();
            itemDecoList.transform.localPosition = new Vector3(0f, 25f, 0f);
        }
    }

    private void MakePokoYuraList()
    {
        pokoMaxIndex = pokoyuraList.Count / 10;
        if (pokoyuraList.Count % 10 > 0)
        {
            pokoMaxIndex += 1;
        }

        int nCount = pokoyuraList.Count;
        for (int i = 0; i < pokoMaxIndex; i++)
        {
            //현재 생성할 데코 아이템 데이터 리스트 생성.
            List<int> pList = new List<int>();
            for (int j = (i * 10); j < (i * 10) + 10; j++)
            {
                //리스트 끝까지 왔으면 종료.
                if (j >= nCount)
                    break;
                pList.Add(pokoyuraList[j]);
            }
            //리스트 오브젝트 생성 & 값 세팅.
            UIItemUserProfilePokoYuraList itemPokoYuraList =
              NGUITools.AddChild(pokoyuraScroll.gameObject, _objItemUserProfilePokoYuraList).GetComponent<UIItemUserProfilePokoYuraList>();
            itemPokoYuraList.transform.localPosition = new Vector3((620f * i), 20f, 0f);
            itemPokoYuraList.SettingPokoYuraList(pList, selectYura);
        }

        //리스트 없을 때 아무 데이터도 안 들어가 있는 리스트 생성.
        if (pokoMaxIndex == 0)
        {
            UIItemUserProfilePokoYuraList itemPokoYuraList =
              NGUITools.AddChild(pokoyuraScroll.gameObject, _objItemUserProfilePokoYuraList).GetComponent<UIItemUserProfilePokoYuraList>();
            itemPokoYuraList.transform.localPosition = new Vector3(0f, 25f, 0f);
        }
    }

    #region 좌/우 버튼 클릭.
    private void OnClickBtnDeco_L()
    {
        decoIndex -= 1;
        MoveDecoScroll();
        if (decoIndex <= 1)
        {
            decoButtons[0].SetActive(false);
        }
        if (decoButtons[1].activeInHierarchy == false)
        {
            decoButtons[1].SetActive(true);
        }
    }

    private void OnClickBtnDeco_R()
    {
        decoIndex += 1;
        MoveDecoScroll();
        if (decoIndex >= decoMaxIndex)
        {
            decoButtons[1].SetActive(false);
        }
        if (decoButtons[0].activeInHierarchy == false)
        {
            decoButtons[0].SetActive(true);
        }
    }

    private void MoveDecoScroll()
    {
        Vector3 pos = new Vector3(620f * (decoIndex - 1), 0f, 0f);
        SpringPanel.Begin(decoScroll.gameObject, -pos, 8f);
    }

    private void OnClickBtnPoko_L()
    {
        pokoIndex -= 1;
        MovePokoYuraScroll();
        if (pokoIndex <= 1)
        {
            pokoyuraButtons[0].SetActive(false);
        }
        if (pokoyuraButtons[1].activeInHierarchy == false)
        {
            pokoyuraButtons[1].SetActive(true);
        }
    }

    private void OnClickBtnPoko_R()
    {
        pokoIndex += 1;
        MovePokoYuraScroll();
        if (pokoIndex >= pokoMaxIndex)
        {
            pokoyuraButtons[1].SetActive(false);
        }
        if (pokoyuraButtons[0].activeInHierarchy == false)
        {
            pokoyuraButtons[0].SetActive(true);
        }
    }

    private void MovePokoYuraScroll()
    {
        Vector3 pos = new Vector3(620f * (pokoIndex - 1), 0f, 0f);
        SpringPanel.Begin(pokoyuraScroll.gameObject, -pos, 8f);
    }
    #endregion 좌/우 버튼 클릭.

    private void OnClickBtnInfo()
    {
        ManagerUI._instance.OpenPopupPokoYuraInfo();
    }

    private void OnClickBtnProfileSelection()
    {
        if(ManagerAdventure.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopUPUserProfileSelection>((popup) => popup.InitData(userData));
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopUPUserProfileSelection>((popup) => popup.InitDataNotOpenAdventure());
        }
    }
}
