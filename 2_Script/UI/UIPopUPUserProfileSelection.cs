using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserProfileData
{
    public string aniID;
    public bool selectionData;

    public UserProfileData(string _aniID)
    {
        aniID = _aniID;
    }
}

public class UIPopUPUserProfileSelection : UIPopupBase
{
    public static UIPopUPUserProfileSelection _instance = null;

    [SerializeField] private UIPanel scroll;
    [SerializeField] private UIReuseGrid_UserProfileData uIReuseGrid;

    // 탐험모드가 열리지 않을 때 나오는 텍스트
    [SerializeField] private GameObject label;

    // 프로필 사진으로 쓸 수 있는 동료 리스트
    private List<UserProfileData> userProfileDataList = new List<UserProfileData>();

    // 현재 선택된 프로필 사진 동료ID
    private string currentProfileName = "";

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }

    UserBase userData = new UserBase();

    public void InitData(UserBase CellData)
    {
        userData = CellData;

        userProfileDataList.Clear();

        // 현재 선택된 프로필 사진의 ID 값 추가
        currentProfileName = $"ap_{userData._alterPicture}";

        var animalList = ManagerAdventure.User.GetAnimalList
            (false, ManagerAdventure.AnimalFilter.AF_ALL, ManagerAdventure.AnimalSortOption.byGrade, false);

        int index = 0;

        for (int i = 0; i < animalList.Count; i++)
        {
            if (ServerContents.AdvAnimals[animalList[i].idx].profile_usable == 0)
                continue;

            userProfileDataList.Add(new UserProfileData(ManagerAdventure.GetAnimalProfileFilename(animalList[i].idx, 0)) );

            if (animalList[i].overlap == 20)
            {
                userProfileDataList.Add(new UserProfileData(ManagerAdventure.GetAnimalProfileFilename(animalList[i].idx, 1)) );
            }
        }

        SetProfileSelectionData();

        uIReuseGrid.ScrollReset();
    }

    public void InitDataNotOpenAdventure()
    {
        userProfileDataList.Clear();

        scroll.enabled = false;
        label.SetActive(true);

    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scroll.depth = uiPanel.depth + 1;
    }

    public override void SettingSortOrder(int layer)
    {
        base.SettingSortOrder(layer);

        if (layer < 10)
            return;

        scroll.useSortingOrder = true;
        scroll.sortingOrder = layer;
    }

    void SetProfileSelectionData()
    {
        for(int index = 0; index < userProfileDataList.Count; index++)
        {
            if (userProfileDataList[index].aniID.Equals(currentProfileName))
            {
                userProfileDataList[index].selectionData = true;
            }
            else
            {
                userProfileDataList[index].selectionData = false;
            }
        }
    }

    private const int userProfileData = 6;
    public UserProfileData[] GetUserProfileData(int index)
    {
        int firstUserProfileIndex = index * userProfileData;

        UserProfileData[] _arrayUserProfileDatas = new UserProfileData[userProfileData];

        for(int i = 0; i < userProfileData; i++)
        {
            if (firstUserProfileIndex + i >= userProfileDataList.Count) break;

            _arrayUserProfileDatas[i] = userProfileDataList[firstUserProfileIndex + i];
        }

        return _arrayUserProfileDatas;
    }

    public int GetUserProfileDataCount()
    {
        return Mathf.CeilToInt((float)userProfileDataList.Count / userProfileData);
    }

    public void SetCurrentProfileName(string aniID)
    {
        if (currentProfileName.Equals(aniID))
            currentProfileName = "";
        else
            currentProfileName = aniID;

        SetProfileSelectionData();

        uIReuseGrid.ScrollReset();
        
        _callbackClose = SetSDKGameProfileInfo;
    }

    void SetSDKGameProfileInfo()
    {
        int aniId = 0, lookId = 0;
        if( currentProfileName.Length == 0 )
            aniId = 0;
        else {
            var alterPicture = currentProfileName.Replace("ap_", "");
            var strArr = alterPicture.Split('_');
            aniId = System.Convert.ToInt32( strArr[0]);
            if(strArr.Length > 1)
                lookId = System.Convert.ToInt32(strArr[1]);            
        }

        ServerAPI.SetAlterPicture(aniId, lookId, OnClickBtnMyProfileHandler);
    }
    
    void OnClickBtnMyProfileHandler(Protocol.SetAlterPicResp resp)
    {
        if(resp.IsSuccess)
        {
            UserSelf myProfile = SDKGameProfileManager._instance.GetMyProfile();
            myProfile.SetAlterPicture(resp.alterPicture);
            ProfileTextureManager.SetProfileTextureUrl(userData);
        }
    }
}
