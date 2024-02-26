using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Protocol;
using Newtonsoft.Json;

public class UIInviteFriendsTap : MonoBehaviour
{
    public UIPanel scrollPanel;
    public UIReuseGrid_Invite reuseScroll;
    public UILabel emptyText;
    public UILabel invitePopupInfo;
    public UILabel[] inviteAllCount;
    public GameObject[] rewardCheck;
    public UIProgressBar inviteProgressBar;
    public UIUrlTexture rewardImage;
    public UITexture boniImage;
    public UIPanel eventPanel;

    private float progressOffset = 0f;
    private int allCount = 50;
    private int inviteCount = 0;

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

    public void InitTap(int _depth, int layer, bool isEmpty = false)
    {   
        InitSortOrder(_depth, layer);
        StartCoroutine(reuseScroll.InitReuseGrid());
        InitText(isEmpty);
        InitTexture();
        InitRewardImage(ServerRepos.UserInfo.haveInviteVer);
        InitProgressBar();
        InitRewardCheck();
    }

    private void InitSortOrder(int _depth, int layer)
    {
        scrollPanel.depth = _depth + 1;
        if (layer >= 10)
        {
            scrollPanel.useSortingOrder = true;
            scrollPanel.sortingOrder = layer + 1;
        }

        bool bEvent = (ServerRepos.LoginCdn.PerInviteRewardEvent == 1) ? true : false;
        eventPanel.gameObject.SetActive(bEvent);
        if (bEvent == false)
            return;

        eventPanel.depth = _depth + 2;
        if (layer >= 10)
        {
            eventPanel.useSortingOrder = true;
            eventPanel.sortingOrder = layer + 2;
        }
    }

    private void InitText(bool isEmpty)
    {
        invitePopupInfo.text = Global._instance.GetString("p_iv_2");
        inviteCount = ServerRepos.TotalInviteCnt;   // 누적 초대수
        if (isEmpty == false)
        {
            emptyText.gameObject.SetActive(false);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_1");
        }
    }

    private void InitTexture()
    {
        boniImage.mainTexture = Box.LoadResource<Texture2D>("UI/invite_icon");
    }

    private void InitRewardImage(int ver)
    {
        string fileName = string.Format("invite_reward_{0}", ver);
        //이미지 사이즈 : x : 473 / y : 147
        rewardImage.LoadCDN(Global.gameImageDirectory, "Invite/", fileName);
    }

    private void InitProgressBar()
    {  
        //초대 수, 프로그레스 바 세팅.
        progressOffset = 100f / allCount;
        inviteAllCount[0].text = inviteCount + "/" + allCount;
        inviteAllCount[1].text = inviteCount + "/" + allCount;
        inviteProgressBar.value = (inviteCount * progressOffset) * 0.01f;
    }

    private void InitRewardCheck()
    {
        int checkNum = 0;
        if (inviteCount >= 50)
            checkNum = 3;
        else if (inviteCount >= 20)
            checkNum = 2;
        else if (inviteCount >= 10)
            checkNum = 1;

        for (int i = 0; i < checkNum; i++)
        {
            rewardCheck[i].SetActive(true);
        }
    }

    public void InviteFriend(UIItemInvite inviteItem)
    {
        RequestFUserKey req = new RequestFUserKey();
        req.fUserKey = inviteItem.GetProviderKey();
        ServerAPI.InviteFriend(req, 
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    //초대한 아이템 UI 갱신
                    inviteItem.SetInvitedUI();

                    //퀘스트 데이터 갱신
                    QuestGameData.SetUserData();

                    //현재 탭 UI 갱신
                    UpdateInviteTapUI();

                    //다이어리 쪽 퀘스트 데이터 갱신.
                    UIDiaryController._instance.UpdateQuestData(true);

                    ServiceSDK.AdjustManager.instance.OnInvite(ServerRepos.UserInvitedFriends.Count);

                    //터치 가능.
                    UIPopupInvite._instance.bCanTouch = true;

                    //그로씨
                    inviteItem.SendGrowthy();

                    //보상 팝업 출력
                    if (resp.clearReward != null)
                    {
                        string textMessage;
                        switch (inviteCount)
                        {
                            case 10:
                                textMessage = Global._instance.GetString("n_iv_3");
                                rewardCheck[0].SetActive(true);
                                SendGrowthyLog_Item(10);
                                break;
                            case 20:
                                textMessage = Global._instance.GetString("n_iv_4");
                                rewardCheck[1].SetActive(true);
                                SendGrowthyLog_Item(20);
                                break;
                            case 50:
                                textMessage = Global._instance.GetString("n_iv_5");
                                rewardCheck[2].SetActive(true);
                                SendGrowthyLog_Item(50);
                                break;
                            default:
                                textMessage = Global._instance.GetString("n_iv_6");
                                break;
                        }
                        ManagerUI._instance.OpenPopupGetRewardAlarm(textMessage, UpdateTopUI, resp.clearReward);
                    }
                }
            });
    }

    private void UpdateTopUI()
    {
        Global.coin = (int)(GameData.User.AllCoin);
        Global.jewel = (int)(GameData.User.AllJewel);
        Global.clover = (int)(GameData.User.AllClover);
        ManagerUI._instance.UpdateUI();
    }

    public void UpdateInviteTapUI()
    {
        inviteCount += 1;
        inviteAllCount[0].text = inviteCount + "/" + allCount;
        inviteAllCount[1].text = inviteCount + "/" + allCount;
        MoveProgressBar(0.2f);
    }

    private void MoveProgressBar(float _mainDelay)
    {
        float targetValue = inviteProgressBar.value + (progressOffset * 0.01f);
        if (targetValue > 1)
        {
            targetValue = 1;
        }
        DOTween.To(() => inviteProgressBar.value, x => inviteProgressBar.value = x, targetValue, _mainDelay).SetEase(Ease.Flash);
    }

    private void SendGrowthyLog_Item(int inviteCount)
    {
        if(ServerContents.InviteReward.TryGetValue(inviteCount, out CdnInviteReward inviteReward))
        {
            foreach(var reward in inviteReward.rewards)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                (
                    rewardType:  reward.type,
                    rewardCount: reward.value,
                    moneyMRSN:   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD,
                    itemRSN:     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD,
                    QuestName:   $"InVite_{inviteCount}"
                );
            }
        }
    }
}
