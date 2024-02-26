using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupBingoEvent_Reward : UIPopupBase
{
    [SerializeField] private GenericReward defultReward;
    [SerializeField] private GameObject objNotReward;
    [SerializeField] private List<UISprite> listUISprite = null;
    [SerializeField] private UILabel textDefaultReward;
    [SerializeField] private List<UIItemLineReward> lineRewards;
    public void InitData()
    {
        var reward = ManagerBingoEvent.instance.GetDefultReward();

        if (reward != null)
        {
            defultReward.SetReward(reward);
            objNotReward.SetActive(false);

            Vector3 targetPos = defultReward.transform.localPosition;
            targetPos.x = textDefaultReward.transform.localPosition.x + (textDefaultReward.width * 0.5f) + 2;
            defultReward.transform.localPosition = targetPos;
        }
        else
        {
            defultReward.gameObject.SetActive(false);
            objNotReward.SetActive(true);
        }

        List<int> tempDecoRewards = ManagerBingoEvent.instance.GetLineDecoRewards();
        for (int i = 0; i < lineRewards.Count; i++)
        {
            lineRewards[i].InitData(tempDecoRewards[i]);
        }

        for (int i = 0; i < listUISprite.Count; i++)
        {
            listUISprite[i].atlas = ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasOutgame;
        }
    }
}
