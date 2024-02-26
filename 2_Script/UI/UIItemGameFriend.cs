using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemGameFriend : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private UIItemProfile profile;
    [SerializeField] private UILabel nameLabel;
    [SerializeField] private UILabel timeLabel;
    [SerializeField] private UIItemSendClover sendClover;

    private Profile_PION user;

    public void UpdateData(Profile_PION user)
    {
        this.user = user;
        string userKey = user?.userKey;

        if (user == null)
        {
            root.SetActive(false);
        }
        else
        {
            root.SetActive(true);

            nameLabel.text = user.profile.name;
            timeLabel.text = Global.GetTimeText_UpdateTime(user.profile.lastLoginTs);
            profile.SetProfile(ProfileConvert.ConvertToUserProfile(user));
        }

        if(userKey != null && SDKGameProfileManager._instance.TryGetPlayingFriend(userKey, out UserFriend userFriend))
        {
            sendClover?.Init(userFriend);
        }
        else
        {
            sendClover?.Init(null);
        }
    }

    public void Friend_Request()
    {
        ManagerUI._instance.OpenPopup<UIPopupGameFriend_Sub>((popup) => popup.Init(UIPopupGameFriend_Sub.Mode.Request, user));
    }

    public void Friend_Accept()
    {
        ManagerUI._instance.OpenPopup<UIPopupGameFriend_Sub>((popup) => popup.Init(UIPopupGameFriend_Sub.Mode.Accept, user));
    }

    public void Friend_Delete()
    {
        ManagerUI._instance.OpenPopup<UIPopupGameFriend_Sub>((popup) => popup.Init(UIPopupGameFriend_Sub.Mode.Delete, user));
    }
}

public static class ProfileConvert
{
    private class PIONProfileToUserProfile : ProfileTextureManager.IUserProfileData
    {
        private Profile_PION profile_PION;

        public PIONProfileToUserProfile(Profile_PION profile_PION)
        {
            this.profile_PION = profile_PION;
        }

        public string _userKey => profile_PION?.userKey ?? string.Empty;

        public string _pictureUrl => string.Empty;

        public string _alterPicture => profile_PION?.profile?.alterPicture ?? string.Empty;
    }

    public static ProfileTextureManager.IUserProfileData ConvertToUserProfile(Profile_PION profile_PION)
    {
        return new PIONProfileToUserProfile(profile_PION);
    }
}