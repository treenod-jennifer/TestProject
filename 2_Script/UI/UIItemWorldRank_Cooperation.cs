using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWorldRank_Cooperation : MonoBehaviour
{
    [SerializeField] private UILabel labelUserName;
    [SerializeField] private UILabel labelUserRank;
    [SerializeField] private UIUrlTexture texScoreIcon;
    [SerializeField] private UILabel labelScoreCount;
    [SerializeField] private UILabel labelRankTokenCount;
    [SerializeField] private UILabel labelAddRankTokenCount;
    [SerializeField] private UISprite spriteMyProfileBack;
    [SerializeField] private UIItemProfile profileItem;

    private ManagerWorldRanking.WorldRankData item;

    private void Awake()
    {
        texScoreIcon.LoadCDN(Global.gameImageDirectory, "IconEvent", $"e_worldrank_{ManagerWorldRanking.contentsData.ResourceIndex}_collectibles.png");
    }

    public void UpdateData(ManagerWorldRanking.WorldRankData CellData)
    {
        item = CellData as ManagerWorldRanking.WorldRankData;

        if (item == null || gameObject.activeInHierarchy == false)
            return;

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        if (item.userKey == myProfile._userKey)
        {
            spriteMyProfileBack.enabled = true;
        }
        else
        {
            spriteMyProfileBack.enabled = false;
        }

        //프로필 아이템 추가
        profileItem.SetProfile(item, item.photoUseAgreed);

        labelUserRank.text = string.Format("{0}", item.rank);

        labelUserName.text = string.Format("{0}", Global.ClipString(item.ingameName, 12));
        labelScoreCount.text = StringHelper.IntToString((int)item.scoreValue);


        int jointReward = UIPopUpWorldRankCooperation._instance.GetCoopReward().rewardBase;

        labelRankTokenCount.text = jointReward.ToString();
        labelAddRankTokenCount.text = string.Format("+ {0}", (UIPopUpWorldRankCooperation._instance.GetCoopUserReward((int)item.rank) - jointReward).ToString());
    }

}
