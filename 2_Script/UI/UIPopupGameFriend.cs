using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupGameFriend : UIPopupBase
{
    public static UIPopupGameFriend _instance;

    [SerializeField] private UIPanel uiRecommendedPanel;
    [SerializeField] private UIPanel uiRequestPanel;
    [SerializeField] private UIReuseGrid_GameFriend uIRecommendedReuseGrid;
    [SerializeField] private UIReuseGrid_GameFriend uIRequestReuseGrid;
    [SerializeField] private GameObject uIRecommendedReuseGrid_Empty;
    [SerializeField] private GameObject uIRequestReuseGrid_Empty;
    [SerializeField] private UIInput inputKeyBoard;

    private List<Profile_PION> recommended;
    private List<Profile_PION> request;

    private void Awake()
    {
        _instance = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(_instance == this) _instance = null;
        
        LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.friend] = request.Count;
        ManagerUI._instance.UpdateUI();
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        base.ClosePopUp(_mainTime, callback);

        if (UIPopupInvite._instance != null)
        {
            UIPopupInvite._instance.UpdateGameFriendList();
        }
    }

    public override void SettingSortOrder(int layer)
    {
        base.SettingSortOrder(layer);

        uiRecommendedPanel.depth = uiPanel.depth + 1;
        uiRequestPanel.depth = uiPanel.depth + 1;

        uiRecommendedPanel.useSortingOrder = uiPanel.useSortingOrder;
        uiRequestPanel.useSortingOrder = uiPanel.useSortingOrder;

        if (uiRecommendedPanel.useSortingOrder)
        {
            uiRecommendedPanel.sortingOrder = uiPanel.sortingOrder + 1;
        }

        if (uiRequestPanel.useSortingOrder)
        {
            uiRequestPanel.sortingOrder = uiPanel.sortingOrder + 1;
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }

        Init();
    }

    private void Init()
    {
        NetworkLoading.MakeNetworkLoading();

        UpdateRequestList(() => 
        { 
            UpdateRecommendedList(()=> 
            {
                NetworkLoading.EndNetworkLoading();
            }); 
        });
    }

    private void UpdateRecommendedGrid(bool scrollPosReset = true)
    {
        bool isRecommendedActive = recommended != null && recommended.Count != 0;
        uIRecommendedReuseGrid_Empty.SetActive(!isRecommendedActive);
        uIRecommendedReuseGrid.InitReuseGrid(recommended, scrollPosReset);
    }

    private void UpdateRequestGrid()
    {
        bool isRequestActive = request != null && request.Count != 0;
        uIRequestReuseGrid_Empty.SetActive(!isRequestActive);
        uIRequestReuseGrid.InitReuseGrid(request);
    }

    private void UpdateRecommendedList(System.Action complete = null)
    {
        List<Profile_PION> friendList = new List<Profile_PION>();
        
        List<string> userFiltering = new List<string>();
        
        //(필터링) 라인 친구는 필터링 해서 보여주지 않음
        userFiltering.AddRange(SDKGameProfileManager._instance.GetPlayingLineFriendKeys());
        userFiltering.AddRange(SDKGameProfileManager._instance.GetNonPlayLineFriendsKeys());

        //(필터링) 나에게 친구 신청한 리스트에 있는 친구는 보여주지 않음
        if (request != null)
        {
            foreach (var item in request)
            {
                userFiltering.Add(item.userKey);
            }
        }
        
        //(필터링) 삭제 목록에 있는 유저는 보여주지 않음
        userFiltering.AddRange(SDKGameProfileManager.DeleteUser.Array);

        StartCoroutine
        (
            SDKGameProfileManager.GetGameFriendSuggestion
            (
                (suggestionList) =>
                {
                    foreach (var item in suggestionList)
                    {
                        if (!userFiltering.Contains(item.userKey))
                        {
                            friendList.Add(item);
                        }
                    }

                    recommended = friendList;

                    UpdateRecommendedGrid();
                    complete?.Invoke();
                },
                (failInfo) =>
                {
                    SDKGameProfileManager.OpenPopupFailInfo(failInfo);

                    UpdateRecommendedGrid();
                    complete?.Invoke();
                }
            )
        );
    }

    private void UpdateRequestList(System.Action complete = null)
    {
        List<Profile_PION> friendList = new List<Profile_PION>();

        StartCoroutine
        (
            SDKGameProfileManager.GetIncomingGameFriendRequests
            (
                (requestList) =>
                {
                    foreach (var item in requestList)
                    {
                        friendList.Add(item);
                    }

                    request = friendList;

                    UpdateRequestGrid();
                    complete?.Invoke();
                },
                (failInfo) =>
                {
                    SDKGameProfileManager.OpenPopupFailInfo(failInfo);

                    UpdateRequestGrid();
                    complete?.Invoke();
                }
            )
        );
    }

    public void OnClickRecommendRefresh()
    {
        NetworkLoading.MakeNetworkLoading();
        UpdateRecommendedList(() => NetworkLoading.EndNetworkLoading());
    }

    public void OnClickTextBox()
    {
        inputKeyBoard.isSelected = true;
    }

    public void RecommendedListDelete(string userKey)
    {
        var user = recommended.Find((profile) => profile.userKey == userKey);

        if (user != null)
        {
            recommended.Remove(user);
            UpdateRecommendedGrid();
        }
    }

    public void RequestListDelete(string userKey)
    {
        var user = request.Find((profile) => profile.userKey == userKey);

        if(user != null)
        {
            request.Remove(user);
            UpdateRequestGrid();
        }
    }

    public void OnClickAllAccept()
    {
        if (request.Count == 0) return;

        if (SDKGameProfileManager._instance.GetGameFriendsCount() >= SDKGameProfileManager._instance.GameFriendMaximumCount)
        {
            //내 친구 목록이 가득차서 수락을 할 수 없는 경우
            OpenPopupMessage("n_gf_11");
            return;
        }

        NetworkLoading.MakeNetworkLoading();

        string error = string.Empty;

        List<Profile_PION> tempRequestList = new List<Profile_PION>();

        tempRequestList.AddRange(request);

        int AcceptCount = 0;
        bool isAcceptSuccess = false;

        foreach (var profile in tempRequestList)
        {
            Accept(profile, (isSuccess) =>
            {
                AcceptCount++;

                if (isSuccess) isAcceptSuccess = true;

                if (AcceptCount == tempRequestList.Count)
                {
                    NetworkLoading.EndNetworkLoading();

                    if (isAcceptSuccess)
                    {
                        //한명이라도 친구 맺기가 성공하면 친구 맺기가 완료 되었다는 팝업을 보여줍니다.
                        OpenPopupMessage("n_gf_12");
                    }
                    //else if(!string.IsNullOrEmpty(error))
                    //{
                    //    //2020.03.26 일단 모두 수락 / 거절 에서 에러 목록은 보여주지 않는 것으로 처리
                    //    //에러가 있으면 에러목록을 보여줍니다.
                    //    OpenPopupMessage(error);
                    //}
                }
            });
        }

        void Accept(Profile_PION profile_PION, System.Action<bool> complete)
        {
            var reqData = profile_PION as SDKGameProfileManager.PION_GameFriendSendReq;

            if (reqData == null) complete(false);

            ServerAPI.GameFriendAccept
            (
                reqData.friendRequestId,
                (resp) =>
                {
                    bool isSuccess = false;

                    if (resp.IsSuccess)
                    {
                        if (resp.result)
                        {
                            RequestListDelete(profile_PION.userKey);
                            isSuccess = true;
                        }
                        else
                        {
                            error += (error == string.Empty ? string.Empty : "\n") + $"{resp.desc}";
                        }
                    }

                    complete(isSuccess);
                }
            );
        }
    }

    public void OnClickAllDecline()
    {
        if (request.Count == 0) return;

        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) => {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString("n_gf_13"),
                    useButtons: false
                );

                popup.SetButtonText(1, Global._instance.GetString("btn_50"));
                popup.FunctionSetting(1, "AllDecline", gameObject);
            }
        );
    }

    private void AllDecline()
    {
        NetworkLoading.MakeNetworkLoading();

        string error = string.Empty;

        List<Profile_PION> tempRequestList = new List<Profile_PION>();

        tempRequestList.AddRange(request);

        int declineCount = 0;

        foreach (var profile in tempRequestList)
        {
            Decline(profile, (isSuccess) =>
            {
                declineCount++;

                if (declineCount == tempRequestList.Count)
                {
                    NetworkLoading.EndNetworkLoading();

                    //2020.03.26 일단 모두 수락 / 거절 에서 에러 목록은 보여주지 않는 것으로 처리
                    //에러가 있으면 에러목록을 보여줍니다.
                    //if (!string.IsNullOrEmpty(error))
                    //{
                    //    OpenPopupMessage(error);
                    //}
                }
            });
        }

        void Decline(Profile_PION profile_PION, System.Action<bool> complete)
        {
            var reqData = profile_PION as SDKGameProfileManager.PION_GameFriendSendReq;

            if (reqData == null) complete(false);

            StartCoroutine(SDKGameProfileManager.DeclineGameFriendRequest
            (
                reqData.friendRequestId,
                () =>
                {
                    SDKGameProfileManager.SendGameFriendGrowthy(ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.REJECT_GAME_FRIEND, profile_PION.userKey);
                    RequestListDelete(profile_PION.userKey);
                    complete(true);
                },
                (failInfo) =>
                {
                    error += (error == string.Empty ? string.Empty : "\n") + $"{failInfo.Code}";
                    complete(false);
                }
            ));
        }
    }

    void OnClickRequestGameFriendDirect()
    {
        inputKeyBoard.value = inputKeyBoard.value.Replace(" ", "");

        if (string.IsNullOrEmpty(inputKeyBoard.value) || inputKeyBoard.value == SDKGameProfileManager._instance.GetMyProfile()._userKey)
        {
            OpenPopupMessage("n_gf_4");
            return;
        }

        //이미 친구인 경우
        List<string> userFiltering = new List<string>();
        userFiltering.AddRange(SDKGameProfileManager._instance.GetPlayingLineFriendKeys());
        userFiltering.AddRange(SDKGameProfileManager._instance.GetNonPlayLineFriendsKeys());
        userFiltering.AddRange(SDKGameProfileManager._instance.GetGameFriendsKey());
        if (userFiltering.Contains(inputKeyBoard.value))
        {
            OpenPopupMessage("n_gf_15");
            return;
        }

        NetworkLoading.MakeNetworkLoading();

        StartCoroutine(SDKGameProfileManager._instance.GetAllProfileList
        (
            (profileList) => 
            {
                if (profileList == null || 
                    profileList.Count == 0 || 
                    (profileList[0].createdTime == 0 && profileList[0].updatedTime == 0))
                {
                    OpenPopupMessage("n_gf_4");
                }
                else
                {
                    Profile_PION profile_PION = profileList[0];
                    ManagerUI._instance.OpenPopup<UIPopupGameFriend_Sub>((popup) => popup.Init(UIPopupGameFriend_Sub.Mode.Request, profile_PION));
                }

                NetworkLoading.EndNetworkLoading();
            },

            inputKeyBoard.value
        ));
    }

    private void OpenPopupMessage(string text)
    {
        ManagerUI._instance.OpenPopup<UIPopupSystem>
        (
            (popup) => {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString(text),
                    useButtons: false
                );
            }
        );
    }
}
