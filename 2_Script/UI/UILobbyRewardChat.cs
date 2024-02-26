using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UILobbyRewardChat : UILobbyChat_Base
{
    //보상 아이콘
    public GameObject getIconRoot;
    public UISprite getIconBG;
    public GenericReward getIcon;

    //아이템 획득 아이콘
    public GenericReward rewardIcon;
    public UILabel rewardCount;

    public AnimationCurve _curveShow;

    private bool bChat = true;
    bool isCanClick = true;

    int animalIdx;

    static public UILobbyRewardChat MakeLobbyChat(Transform in_obj, string in_strChat, float in_dulation = 1f, bool in_show = false)
    {
        if (SceneLoading.IsSceneLoading)
            return null;

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();
        ManagerSound.AudioPlay(AudioLobby.Button_02);
        UILobbyRewardChat lobbyChat = NGUITools.AddChild(ManagerUI._instance.gameObject, ManagerUI._instance._objLobbyRewardChat).GetComponent<UILobbyRewardChat>();
        lobbyChat.targetObject = in_obj;
        lobbyChat.LobbyChat.text = in_strChat;
        lobbyChat.LobbyChat.text = in_strChat.Replace("[0]", myProfile.GameName);
        lobbyChat._dulation = in_dulation;
        lobbyChat.showOutScreen = in_show;
        lobbyChat.bChat = true;
        lobbyChat.getIconRoot.SetActive(false);
        return lobbyChat;
    }

    static public UILobbyRewardChat MakeLobbyRewardIcon(Transform in_obj, int animalIdx, Reward reward, float in_dulation = 1f, bool in_show = false)
    {
        if (SceneLoading.IsSceneLoading)
            return null;

        UILobbyRewardChat lobbyChat = NGUITools.AddChild(ManagerUI._instance.gameObject, ManagerUI._instance._objLobbyRewardChat).GetComponent<UILobbyRewardChat>();
        lobbyChat.targetObject = in_obj;
        lobbyChat._dulation = in_dulation;
        lobbyChat.showOutScreen = in_show;
        lobbyChat.bChat = false;
        lobbyChat.LobbyChat.gameObject.SetActive(false);
        lobbyChat.animalIdx = animalIdx;
        lobbyChat.SetReward(reward);
        lobbyChat.ChatBubbleBox.gameObject.SetActive(false);
        return lobbyChat;
    }
    
    protected override IEnumerator CoShowTimer()
    {
        if (bChat == true)
        {
            float elapsedTime = 0f;
            while (elapsedTime < _dulation)
            {
                elapsedTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            StartCoroutine(CoHide());
        }
        else
        {
            yield break;
        }
    }

    protected override void SetBoxSize()
    {
        if (bChat == true)
        {
            base.SetBoxSize();
        }
    }

    public override void SetDepthOffset(int offset)
    {
        var org = ManagerUI._instance._objLobbyRewardChat.GetComponent<UILobbyRewardChat>();
        this.ChatBubbleTail.depth = org.ChatBubbleTail.depth + offset;
        this.ChatBubbleBox.depth = org.ChatBubbleBox.depth + offset;
        this.LobbyChat.depth = org.LobbyChat.depth + offset;
        this.rewardIcon.SetDepth(org.LobbyChat.depth + offset);
        this.getIconBG.depth = org.getIconBG.depth + offset;
        this.getIcon.SetDepth(org.LobbyChat.depth + offset);
        this.rewardCount.depth = org.rewardCount.depth + offset;
        
    }

    private void SetReward(Reward reward)
    {
        rewardIcon.SetReward(reward);
        getIcon.SetReward(reward);
        rewardCount.text = " + " + reward.value.ToString();
    }

    public void OnTap()
    {
        if (isCanClick == false)
            return;
        isCanClick = false;

        ManagerSound.AudioPlay(AudioLobby.Mission_ButtonClick);
        StartCoroutine(DoTouchObjectAnimation());
    }

    IEnumerator DoTouchObjectAnimation(bool bEndOpenPopup = true)
    {
        bool retReceived = false;
        ServerAPI.AdventureGetLobbyAnimalReward(animalIdx,
                       (resp) =>
                       {
                           retReceived = true;
                           if (resp.IsSuccess)
                           {
                               ManagerAIAnimal.instance.OnReceivedLobbyAnimalReward(animalIdx);

                               Global.clover = (int)GameData.Asset.AllClover;
                               Global.coin = (int)GameData.Asset.AllCoin;
                               Global.jewel = (int)GameData.Asset.AllJewel;
                               Global.wing = (int)GameData.Asset.AllWing;
                               Global.exp = (int)GameData.User.expBall;
                               ManagerUI._instance.UpdateUI();

                               if (resp.rewards != null)
                               {
                                   for (int i = 0; i < resp.rewards.Count; ++i)
                                   {
                                       ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                           rewardType: resp.rewards[i].type,
                                           rewardCount: resp.rewards[i].value,
                                           moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOBBY_ANIMAL_GIFT,
                                           itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOBBY_ANIMAL_GIFT,
                                           QuestName: $"Animal_{animalIdx}"
                                           );

                                   }

                               }

                           }
                       });

        while (retReceived == false)
        {
            yield return null;
        }

        float animationTimer = 1;
        float ratio;
        while (animationTimer > 0)
        {
            ratio = lobbyChatAnimationCurve.Evaluate(animationTimer);
            getIconBG.transform.localScale = Vector3.one * ratio;
            getIconBG.alpha = ratio;
            animationTimer -= Global.deltaTime * 8f;
            yield return null;
        }

        getIcon.gameObject.SetActive(true);
        float targetY = getIcon.transform.localPosition.y + 8f;
        getIcon.transform.DOLocalMoveY(targetY, 0.3f).SetEase(_curveShow);
        yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
    }
}
