using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWakeupFriendsTap : MonoBehaviour
{
    public static UIWakeupFriendsTap _instance = null;

    [SerializeField]
    private UITexture textureBG;
    [SerializeField]
    private UITexture textureComingsoon;
    [SerializeField]
    private UILabel emptyText;
    [SerializeField]
    private UILabel tapMessage;
    [SerializeField]
    private List<UIItemWakeup> listItemWakeupFriends;

    [SerializeField]
    private GameObject refreshButton;

    private List<UserFriend> listWakeupFriendsData = new List<UserFriend>();
    private bool isOpenEvent = false;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if(box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void InitTap(List<UserFriend> listUserData)
    {
        isOpenEvent = (ServerContents.WakeupEvent == null || ServerContents.WakeupEvent.event_index == 0) ? false : true;
        listWakeupFriendsData = listUserData;
        InitText();
        InitTexture();
        UpdateItem();
    }

    #region Init 함수(창이 처음 열릴 때 한 번만 불리는 함수)
    private void InitText()
    {
        tapMessage.text = Global._instance.GetString("p_wu_1");
        if (isOpenEvent == false)
        {
            refreshButton.SetActive(false);
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("n_s_13");
        }
        else
        {
            refreshButton.SetActive(true);
            UpdateText();
        }
    }

    private void InitTexture()
    {
        textureBG.mainTexture = Box.LoadResource<Texture2D>("UI/wake_friend_bg");
        UpdateTexture();
    }
    #endregion

    #region Update 함수(창이 갱신되어야 할 때마다 불리는 함수)
    private void UpdateText()
    {
        if(isOpenEvent == true)
        {
            if (listWakeupFriendsData.Count > 0)
            {
                emptyText.gameObject.SetActive(false);
                refreshButton.SetActive((listWakeupFriendsData.Count + ServerRepos.UserWakeupEvent.sent_today )>= 5);
            }
            else
            {
                refreshButton.SetActive(false);
                emptyText.gameObject.SetActive(true);
                if (ServerRepos.UserWakeupEvent.sent_today >= UIPopupInvite._instance.MaxWakeupCount)
                    emptyText.text = Global._instance.GetString("n_s_13");  // coming soon
                else
                    emptyText.text = Global._instance.GetString("p_e_13");  // 친구가 없ㅇ
            }
        }
    }

    private void UpdateTexture()
    {
        if (listWakeupFriendsData.Count == 0)
        {
            textureComingsoon.mainTexture = Box.LoadResource<Texture2D>("UI/mailbox_empty");
        }
    }

    private void UpdateItem()
    {
        int itemIndex = 0;
        for (int i = 0; i < listWakeupFriendsData.Count; i++)
        {
            listItemWakeupFriends[itemIndex].UpdateData(listWakeupFriendsData[i]);
            itemIndex++;
        }

        for (int i = itemIndex; i < listItemWakeupFriends.Count; i++)
        {
            listItemWakeupFriends[i].gameObject.SetActive(false);
        }
    }
    #endregion

    public void Repaint()
    {
        UpdateText();
        UpdateTexture();
        UpdateItem();
    }

    public void OnClickRefreshWakeup ()
    {
        UIPopupInvite._instance.RefreshWakeupList(new List<UserBase>(this.listWakeupFriendsData));

        Repaint();
    }
}