using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupGameFriend_Sub : UIPopupBase
{
    [SerializeField] private UIItemProfile profile;
    [SerializeField] private UILabel centerText;
    [SerializeField] private UILabel dayText;

    [SerializeField] private GameObject RequestRoot;
    [SerializeField] private GameObject AcceptRoot;
    [SerializeField] private GameObject DeleteRoot;

    private Profile_PION user;

    public enum Mode
    {
        Request,
        Accept,
        Delete
    }

    public void Init(Mode mode, Profile_PION user)
    {
        this.user = user;

        string text;

        switch (mode)
        {
            case Mode.Request:
                text = Global._instance.GetString("n_gf_5");
                break;

            case Mode.Accept:
                text = Global._instance.GetString("n_gf_9");
                break;

            case Mode.Delete:
                text = Global._instance.GetString("n_gf_1");
                break;

            default:
                return;
        }

        RequestRoot.SetActive(mode == Mode.Request);
        AcceptRoot.SetActive(mode == Mode.Accept);
        DeleteRoot.SetActive(mode == Mode.Delete);

        profile.SetProfile(ProfileConvert.ConvertToUserProfile(user));

        centerText.text = text.Replace("[u]", user.profile.name);
        dayText.text = Global.GetTimeText_UpdateTime(user.profile.lastLoginTs);
    }

    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public void RequestMode_Accept()
    {
        if (SDKGameProfileManager._instance.GetGameFriendsCount() >= SDKGameProfileManager._instance.GameFriendMaximumCount)
        {
            //내 친구 목록이 가득차서 신청할 수 없는 경우
            OpenPopupSystem("n_gf_7");
        }
        else
        {
            NetworkLoading.MakeNetworkLoading();

            StartCoroutine
            (
                SDKGameProfileManager.SendGameFriendRequest
                (
                    user.userKey,
                    (request) =>
                    {
                        OpenPopupSystem("n_gf_8");
                        UIPopupGameFriend._instance.RecommendedListDelete(user.userKey);

                        //쌍방 친구 신청으로 친구가 될 수 있기때문에 신청리스트에 해당 유저가 있다면 삭제 처리
                        UIPopupGameFriend._instance.RequestListDelete(user.userKey);

                        NetworkLoading.EndNetworkLoading();
                    },
                    (failInfo) =>
                    {
                        if(failInfo != null)
                        {
                            switch (failInfo.Code)
                            {
                                case "FRIEND_409_0001":
                                    //나의 포코친구 목록이 가득찬 경우
                                    OpenPopupSystem("n_gf_11");
                                    break;

                                case "FRIEND_409_0002":
                                    //상대의 친구목록이 가득찬 경우(일반적으로는 동작하지 않고 쌍방 친구신청 시 상대의 목록이 가득 찬 경우)
                                    OpenPopupSystem("n_gf_6");
                                    break;

                                case "FRIEND_409_1002":
                                    //중복 친구요청한 경우 (그냥 친구 신청이 되었다고 유저에게 알려줌)
                                    OpenPopupSystem("n_gf_8");
                                    UIPopupGameFriend._instance.RecommendedListDelete(user.userKey);
                                    break;

                                default:
                                    SDKGameProfileManager.OpenPopupFailInfo(failInfo);
                                    break;
                            }
                        }
                        
                        NetworkLoading.EndNetworkLoading();
                    }
                )
            );
        }
    }

    /// <summary>
    /// 추천친구 거절 (목록에서 사라짐 - 새로고침하면 다시 나올 수 있음)
    /// </summary>
    public void RequestMode_Decline()
    {
        Debug.Log("RequestMode_Decline");
        UIPopupGameFriend._instance.RecommendedListDelete(user.userKey);
        ClosePopUp();
    }

    /// <summary>
    /// 친구 요청 수락
    /// </summary>
    public void AcceptMode_Accept()
    {
        Debug.Log("AcceptMode_Accept");
        var reqData = user as SDKGameProfileManager.PION_GameFriendSendReq;
        if(reqData == null)
        {
            return;
        }

        //나의 포코친구 목록이 가득찬 경우
        if(SDKGameProfileManager._instance.GetGameFriendsCount() >= SDKGameProfileManager._instance.GameFriendMaximumCount)
        {
            OpenPopupSystem("n_gf_11");
            return;
        }
        
        ServerAPI.GameFriendAccept(reqData.friendRequestId, (resp) =>
        {
            if (resp.IsSuccess)
            {
                if (resp.result)
                {
                    SDKGameProfileManager.SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.ACCEPT_GAME_FRIEND, reqData.userKey);

                    //친구 맺기 성공
                    OpenPopupSystem("n_gf_12");
                    UIPopupGameFriend._instance?.RequestListDelete(user.userKey);
                }
                else
                {
                    switch (resp.desc)
                    {
                        case "FRIEND_409_0001":
                            //나의 포코친구 목록이 가득찬 경우(친구가 가득 차지 않은 상태인데 타인의 수락으로 친구관계가 되어 나의 목록이 가득찰 수 있다)
                            OpenPopupSystem("n_gf_11");
                            break;

                        case "FRIEND_409_0002":
                            //상대의 친구목록이 가득차서 수락할 수 없는 경우
                            OpenPopupSystem("n_gf_10");
                            break;

                        default:
                            SDKGameProfileManager.OpenPopupFailInfo(new SDKGameProfileManager.PION_FailInfo(resp.desc, string.Empty));
                            break;
                    }
                }
            }
        });
    }

    /// <summary>
    /// 친구 요청 거절
    /// </summary>
    public void AcceptMode_Decline()
    {
        Debug.Log("AcceptMode_Decline");
        var reqData = user as SDKGameProfileManager.PION_GameFriendSendReq;
        if (reqData == null)
        {
            return;
        }

        StartCoroutine(SDKGameProfileManager.DeclineGameFriendRequest
        (
            reqData.friendRequestId,
            () =>
            {
                SDKGameProfileManager.SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.REJECT_GAME_FRIEND, user.userKey);
                UIPopupGameFriend._instance?.RequestListDelete(user.userKey);
                ManagerUI._instance.ClosePopUpUI();
            },
            (failInfo) =>
            {
                SDKGameProfileManager.OpenPopupFailInfo(failInfo);
            }
        ));
    }

    /// <summary>
    /// 친구 삭제
    /// </summary>
    public void FriendDelete()
    {
        Debug.Log($"FriendDelete : {ServerRepos.UserGameFriendInfo.deletedToday}");
        if( ServerRepos.UserGameFriendInfo.deletedToday >= 3 )
        {
            //하루 해제 제한(3명)
            OpenPopupSystem("n_gf_2");
            return;
        }

        ServerAPI.GameFriendRemove(user.userKey,
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.result)
                    {
                        SDKGameProfileManager.SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.DELETE_GAME_FRIEND, user.userKey);
                        
                        OpenPopupSystem("n_gf_3");
                        SDKGameProfileManager.DeleteUser.Add(user.userKey);
                        UIPopupInvite._instance?.UpdateGameFriendList();
                    }
                    else
                    {
                        switch (resp.desc)
                        {
                            case "FRIEND_409_0004":
                                //상대가 먼저 친구삭제를 한 경우(일단 그냥 삭제되었다고 표시해줌)
                                OpenPopupSystem("n_gf_3");
                                SDKGameProfileManager.DeleteUser.Add(user.userKey);
                                UIPopupInvite._instance?.UpdateGameFriendList();
                                break;

                            default:
                                SDKGameProfileManager.OpenPopupFailInfo(new SDKGameProfileManager.PION_FailInfo(resp.desc, string.Empty));
                                break;
                        }
                    }
                }
            }
        );
    }

    private void OpenPopupSystem(string text)
    {
        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) => {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString(text),
                    useButtons: false,
                    in_callback: () => ManagerUI._instance.ClosePopUpUI()
                );
            }
        );
    }
}
