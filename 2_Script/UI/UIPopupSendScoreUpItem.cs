using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSendScoreUpItem : UIPopupBase
{
    private List<UserFriend> friends;

    private UserBase selectUserData = new UserBase();

    [SerializeField] private GenericReward RewardItem;
    [SerializeField] private UIReuseGrid_Generic uIReuseGrid;
    [SerializeField] private UIPanel scrollView;

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        scrollView.depth = depth + 1;

        RewardItem.SetReward(ManagerPokoFlowerEvent.PokoflowerEventReward);

#if !UNITY_EDITOR
        friends = ManagerPokoFlowerEvent.SendFriendList();
#else
        friends = MakeTestFriend(50);
#endif

        selectUserData = friends[0];

        uIReuseGrid.InItGrid(Mathf.CeilToInt(friends.Count * 0.5f), (target, Index) => 
        {
            var profile = target.GetComponentsInChildren<UIItemSendScoreUpItemProfile>(true);

            int leftItemIndex = Index * 2;
            int rightItemIndex = leftItemIndex + 1;

            SetItem(profile[0], leftItemIndex, friends.Count);
            SetItem(profile[1], rightItemIndex, friends.Count);
        });

        void SetItem(UIItemSendScoreUpItemProfile target, int index, int count)
        {
            if (index < count)
            {
                target.gameObject.SetActive(true);
                target.SettingItem(friends[index], index, OnClickItem);
                target.IsCheck = selectUserData?._userKey == friends[index]._userKey;
            }
            else
            {
                target.gameObject.SetActive(false);
            }
        }
    }

    public override void SettingSortOrder(int layer)
    {
        base.SettingSortOrder(layer);

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder = layer + 1;
    }

    private void OnClickItem(int index)
    {
        selectUserData = friends[index];
        uIReuseGrid.SetContent();
        //Debug.Log($"{selectUserData.DefaultName}");
    }

    public void OnClickClose()
    {
        MakeSystemPopup(Global._instance.GetString("n_ecp_3"));
    }

    public void OnClickSendBtn()
    {
        //ServerAPI.GetBlueFlowerReward(0, (Protocol.GetBlueFlowerRewardResp resp) => { if (resp.IsSuccess) { } });

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        //서버 API 성공시 작동
        ServerAPI.SharePokoFlowerReward(myProfile.DefaultName, selectUserData._userKey, (resp)
            =>
            {
                if(resp.IsSuccess)
                {
                    var tProfile = selectUserData.GetTridentProfile();
                    //그로씨
                    var socialLog = new ServiceSDK.GrowthyCustomLog_Social(
                        SDKGameProfileManager._instance.GetMyProfile().stage.ToString(),
                        ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SHARE_POKOFLOWER_EVENT_REWARD,
                        (tProfile != null && !string.IsNullOrEmpty(tProfile.userKey)) ? tProfile.userKey : "");
                    var doc = Newtonsoft.Json.JsonConvert.SerializeObject(socialLog);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);

                    MakeSystemPopup(Global._instance.GetString("n_ecp_4"), OnClickBtnClose);
                }
            });
    }

    private void MakeSystemPopup(string messageText, Method.FunctionVoid callBack = null)
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), messageText, false);
        popupSystem.SetResourceImage(RewardHelper.GetRewardTextureResourcePath((RewardType)ManagerPokoFlowerEvent.PokoflowerEventReward.type));
        popupSystem.FunctionSetting(1, "OnClickBtnClose", this.gameObject, true);
        popupSystem._callbackEnd = callBack;
    }

    #region MakeFriend
    private List<UserFriend> MakeTestFriend(int makeCount)
    {
        List<UserFriend> invitePossible = new List<UserFriend>();
        {
            //친구정보 생성
            for (int i = 0; i < makeCount; i++)
            {
                UserFriend fp = new UserFriend();
                var p = new ServiceSDK.UserProfileData();
                p.name = "TOMATO" + i;
                p.userKey = $"{i:D8}";
                p.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
                fp.SetTridentProfile(p);
                //초대가능한 친구.
                invitePossible.Add(fp);
            }
        }

        return invitePossible;
    }
#endregion
}
