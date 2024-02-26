using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameFriendsTap : MonoBehaviour
{
    [SerializeField] private UIPanel scrollPanel;
    [SerializeField] private UIReuseGrid_GameFriend uIReuseGrid;
    [SerializeField] private GameObject noFriends;
    [SerializeField] private UILabel friendCount;
    [SerializeField] private UILabel friendMaxCount;
    
    [SerializeField] private UILabel myUserKeyLabel;

    [SerializeField] private UIItemLanpageButton lanpageButton;

    private void OnDestroy()
    {
        SDKGameProfileManager.SavePokoFriendCount();
    }

    public void SetDepth(UIPanel panel)
    {
        scrollPanel.depth = panel.depth + 1;

        scrollPanel.useSortingOrder = panel.useSortingOrder;

        if (scrollPanel.useSortingOrder)
        {
            scrollPanel.sortingOrder = panel.sortingOrder + 1;
        }
    }

    public void InitTap(List<Profile_PION> listUserData)
    {
        bool isNofriend = listUserData == null || listUserData.Count == 0;

        noFriends.SetActive(isNofriend);

        uIReuseGrid.InitReuseGrid(listUserData);

        friendCount.text = listUserData.Count.ToString();

        friendMaxCount.text = $"/{SDKGameProfileManager._instance.GameFriendMaximumCount}";
        myUserKeyLabel.text = $"{SDKGameProfileManager._instance.GetMyProfile()._userKey}";

        lanpageButton.On("LGPKV_pokotomo", Global._instance.GetString("p_gf_11"));
    }

    public void OnClickGameFriend()
    {
        ManagerUI._instance.OpenPopup<UIPopupGameFriend>();
    }

    public void OnClickMyUserKey()
    {
        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) => {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString("n_gf_14"),
                    useButtons: false
                );
            }
        );

        UniClipboard.SetText(this.myUserKeyLabel.text);
    }
}
