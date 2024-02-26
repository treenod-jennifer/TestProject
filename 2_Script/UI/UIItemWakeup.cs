using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWakeup : MonoBehaviour
{
    public UISprite wakeupButton;
    public UILabel friendName;
    public UILabel[] wakeupText;
    public List<GenericReward> rewardIcons;
    private UserFriend item;

    [SerializeField] private UIItemProfile profileItem;

    public void UpdateData(UserFriend CellData)
    {
        item = CellData as UserFriend;
        if (item == null || gameObject.activeInHierarchy == false)
            return;

        //프로필 아이템 추가
        profileItem.SetProfile(item);

        friendName.text = string.Format("{0}", Global.ClipString(item.DefaultName, 10));

        string buttonText = Global._instance.GetString("btn_39");
        for (int i=0; i< wakeupText.Length; i++)
        {
            wakeupText[i].text = buttonText;
        }

        for (int i = 0; i < rewardIcons.Count; ++i)
        {
            //깨우기 이벤트 보상받기
            if (i < ServerContents.WakeupEvent.req_reward.Count)
            {
                if (rewardIcons[i].gameObject.activeInHierarchy == false)
                    rewardIcons[i].gameObject.SetActive(true);
                rewardIcons[i].SetReward(ServerContents.WakeupEvent.req_reward[i]);
            }
            else
            {
                rewardIcons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickBtnWakeup()
    {
        if (UIPopupInvite._instance.bCanTouch == false || item.IsAlreadyWakeupSent == true)
            return;

        //깨우기 메세지를 보낼게! 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmWakeupFrined", gameObject, false);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        string text = Global._instance.GetString("n_wu_1");
        text = text.Replace("[u]", friendName.text);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
        popupSystem.SetResourceImage("Message/coin");
    }

    void ConfirmWakeupFrined()
    {
        if (UIPopupInvite._instance.bCanTouch == false || item.IsAlreadyWakeupSent == true)
            return;

        UIPopupInvite._instance.bCanTouch = false;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {   //에디터에서 테스트 할 떄는, 초대버튼 눌렀을 때, 강제로 초대 불가능한 상태로 만들어줌.
            item.SetLastLoginTsToNowForTest();
            recvWakeUpFriends();
            UpdateUI();
        }
        else
        { 
            StartCoroutine(CoWakeup());
        }
    }

    IEnumerator CoWakeup()
    {
        bool coEnd = false;
        ServerAPI.WakeupFriend(this.item._userKey, (resp) => 
        {
            coEnd = true;
            if (resp.clearReward != null)
            {
                ManagerUI._instance.OpenPopupGetRewardAlarm 
                (Global._instance.GetString("n_wu_2"),
                null,
                resp.clearReward);
            }
            SendGrowthyLog_Social_SEND_WAKEUP();
        });

        yield return new WaitUntil(() => coEnd);

        UpdateUI();
        recvWakeUpFriends();
    }

    private void UpdateUI()
    {
        Global.coin = (int)(GameData.User.AllCoin);
        Global.jewel = (int)(GameData.User.AllJewel);
        Global.clover = (int)(GameData.User.AllClover);
        ManagerUI._instance.UpdateUI();
    }

    private void recvWakeUpFriends()
    {
        //깨우기 같은 경우, 라인 메세지는 서버에서 보내줌(클라에서는 처리되는 부분이 없음).
        UIPopupInvite._instance.WakeupFriends();
    }

    private void SendGrowthyLog_Social_SEND_WAKEUP()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        var social = new ServiceSDK.GrowthyCustomLog_Social
        (
            lastStage: myProfile.stage.ToString(),
            socialType: ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SEND_WAKEUP,
            friendUid: item._userKey
        );

        var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(social);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", jsonData);
    }
}
