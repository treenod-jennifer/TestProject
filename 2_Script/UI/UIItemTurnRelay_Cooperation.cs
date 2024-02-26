using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemTurnRelay_Cooperation : MonoBehaviour
{
    [SerializeField] private UILabel labelUserName;
    [SerializeField] private UILabel labelUserRank;
    [SerializeField] private UILabel labelScoreCount;
    [SerializeField] private UISprite spriteMyProfileBack;
    [SerializeField] private UIItemProfile profileItem;
    [SerializeField] private GenericReward genericReward;

    private ManagerTurnRelay.TurnRelayRankData item;

    public void UpdateData(ManagerTurnRelay.TurnRelayRankData cellData)
    {
        item = cellData;

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

        int index = (int)item.rank - 1;
        Reward jointReward = ManagerTurnRelay.turnRelayCoop.GetCoopReward(index);
        if (jointReward != null)
            genericReward.SetReward(jointReward);
    }
}
