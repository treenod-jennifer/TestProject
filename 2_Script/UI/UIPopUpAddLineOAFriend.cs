using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpAddLineOAFriend : UIPopupBase
{
    static public UIPopUpAddLineOAFriend instance;

    [SerializeField] private GameObject sprCheackbtn;
    [SerializeField] private GameObject btnAddLineFriend;
    [SerializeField] private GameObject btnAddLineFriend_Off;
    [SerializeField] private BoxCollider colAddFriendBtn;
    [SerializeField] private UIUrlTexture eventTextrue;
    System.DateTime popupOpenedTime;

    private void Awake()
    {
        instance = this;

        eventTextrue.SettingTextureScale(660, 860);
        eventTextrue.LoadCDN(Global.gameImageDirectory, "CachedResource/", $"oa_add_{ServerRepos.LoginCdn.oAAddition}");
        popupOpenedTime = System.DateTime.UtcNow;

        var oaAddLog = new ServiceSDK.GrowthyCustomLog_ADDOA(
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_TAG.POPUP_SHOW,
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_STR1.OA_NOT_FOLLOW);
        var oaAddDoc = JsonConvert.SerializeObject(oaAddLog);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SPECIFIC", oaAddDoc);
    }
    private void OnDestroy()
    {
        if( instance == this)
            instance = null;
    }

    bool _agreeValue = false;
    void OnClickCheackBox()
    {
        ChangeValue(_agreeValue);
    }

    void ChangeValue(bool agreeValue)
    {
        sprCheackbtn.SetActive(!agreeValue);
        colAddFriendBtn.enabled = !agreeValue;
        btnAddLineFriend_Off.SetActive(agreeValue);
        btnAddLineFriend.SetActive(!agreeValue);

        _agreeValue = !agreeValue;
    }

    void OnClickClose()
    {
        OnClickBtnClose();
        
        int gazedTime = (int)((System.DateTime.UtcNow - popupOpenedTime).TotalSeconds);
        var oaAddLog = new ServiceSDK.GrowthyCustomLog_ADDOA(
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_TAG.POPUP_CLOSE,
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_STR1.OA_NOT_FOLLOW,
            gazedTime);
        var oaAddDoc = JsonConvert.SerializeObject(oaAddLog);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SPECIFIC", oaAddDoc);
    }

    void OnClickAgreeBtn()
    {
        this.bCanTouch = false;

        int gazedTime = (int)((System.DateTime.UtcNow - popupOpenedTime).TotalSeconds);
        var oaAddLog = new ServiceSDK.GrowthyCustomLog_ADDOA(
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_TAG.ADD_OA,
            ServiceSDK.GrowthyCustomLog_ADDOA.Code_L_STR1.OA_NOT_FOLLOW,
            gazedTime);
        var oaAddDoc = JsonConvert.SerializeObject(oaAddLog);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SPECIFIC", oaAddDoc);

        ServerAPI.FollowOA(ServerRepos.OAStatus.id, 
            (r) => 
            {
                if( r.IsSuccess )
                {
                    if (r.result == true)
                    {
                        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

                        var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social(
                            myProfile.stage.ToString(),
                            ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.ADD_LINE_OA,
                            ""
                            );
                        var doc = JsonConvert.SerializeObject(inviteFriend);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);

                        
                    }
                }

                this.bCanTouch = true;
                ClosePopUp(); 
            }
        );
    }

}
