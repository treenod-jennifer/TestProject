using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProfileTextureManager;

public class UIItemProfile : MonoBehaviour, IProfileTexture
{
    [SerializeField] private UIUrlTexture userUrlProfile;
    [SerializeField] private int sizeProfile;

    private IUserProfileData profile;

    private Rect uvLineRect = new Rect(0, 0, 1, 1);
    private Rect uvAnimalRect = new Rect(0.03f, 0.04f, 0.93f, 0.93f);

    private bool useOnlyAlterPicture = false;
    
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
        AddObserverDic(this);
    }

    private void OnDestroy()
    {
        RemoveObserverList(this);
    }

    /// <summary>
    /// 유저프로필 설정
    /// </summary>
    /// <param name="profileData">유저프로필 데이터</param>
    public void SetProfile(IUserProfileData profileData)
    {
        if (profileData == null) return;

        if (profile != profileData)
        {
            RemoveObserverList(this);

            profile = profileData;

            AddObserverDic(this);
        }

        if (IsAlterPicture(profile._alterPicture))
        {
            //동물 프로필 처리
            userUrlProfile.SettingTextureScale(sizeProfile, sizeProfile);
            userUrlProfile.LoadCDN(Global.adventureDirectory, "Animal/", $"ap_{profile._alterPicture}.png");

            userUrlProfile.uvRect = uvAnimalRect;
        }
        else
        {
            //디폴트 이미지 처리
            SetProfileImage("UI/profile_2");
            userUrlProfile.uvRect = uvLineRect;

            if (useOnlyAlterPicture == false && profileData._pictureUrl != string.Empty)
            {
                //라인 프로필 처리
                userUrlProfile.SettingTextureScale(sizeProfile, sizeProfile);
                userUrlProfile.LoadWeb($"{profileData._pictureUrl}/small");
            }
        }
    }

    public void SetDefaultProfile(string profileName)
    {
        SetProfileImage($"UI/{profileName}");
    }

    /// <summary>
    /// 유저프로필 설정_프로필 동의가 필요할 때 사용
    /// </summary>
    /// <param name="profileData">유저프로필 데이터</param>
    /// <param name="photoUseAgreed">유저프로필 사용 동의 여부</param>
    public void SetProfile(IUserProfileData profileData, bool photoUseAgreed)
    {
        if (profileData == null) return;

        //프로필 동의 컨트롤
        //동물사진 이거나 유저프로필을 동의한 경우 사진을 보여준다.
        bool isPhotoActive = IsAlterPicture(profileData._alterPicture) || photoUseAgreed;

        if(isPhotoActive)
        {
            SetProfile(profileData);
        }
        else
        {
            SetProfileImage("UI/profile_2");
        }
    }

    /// <summary>
    /// 유저프로필 설정_동물 프로필 이미지만 표기할 경우 사용
    /// </summary>
    /// <param name="profileData">유저프로필 데이터</param>
    public void SetProfileOnlyAlterPicture(IUserProfileData profileData)
    {
        useOnlyAlterPicture = true;
        
        if (profileData == null) return;

        if (profile != profileData)
        {
            RemoveObserverList(this);

            profile = profileData;

            AddObserverDic(this);
        }
        
        bool isPhotoActive = IsAlterPicture(profileData._alterPicture);
        if(isPhotoActive)
        {
            SetProfile(profileData);
        }
        else
        {
            SetProfileImage("UI/profile_2");
        }
    }
    
    public string userKey { get { return profile?._userKey; } }

    private void SetProfileImage(string resourcePath)
    {
        userUrlProfile.SettingTextureScale(sizeProfile, sizeProfile);
        userUrlProfile.LoadResource(resourcePath);
    }
}
