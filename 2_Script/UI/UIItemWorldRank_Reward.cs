using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWorldRank_Reward : MonoBehaviour
{
    [SerializeField] private UILabel rank;
    [SerializeField] private UILabel[] rankToken;
    [SerializeField] private GameObject[] FrameReward;
    [SerializeField] private GameObject objRankToken;

    private WorldRankReward Item;

    public void UpdateData(WorldRankReward CellData)
    {
        Item = CellData;

        //랭킹
        if (Item.start == Item.end)
        {
            rank.text = $"{Item.start}";
        }
        else if(Item.end > 100000)
        {
            rank.text = $"{Item.start} -";
        }
        else
        {
            rank.text = $"{Item.start} - {Item.end}";
        }

        //보상 정보 셋팅
        SetRewardData();
    }

    void SetRewardData()
    {
        //랭크 토큰 보상 정보
        rankToken[0].text = $"{Item.count}";
        rankToken[1].text = $"{Item.count}";

        for (int i = 0; i < 5; i++)
        {
            FrameReward[i].SetActive(false);
        }

        objRankToken.transform.localPosition = new Vector3(-120.0f, 0f, 0f);

        switch (Item.end)
        {
            case 1:
                FrameReward[0].SetActive(true);
                break;
            case 2:
                FrameReward[1].SetActive(true);
                break;
            case 3:
                FrameReward[2].SetActive(true);
                break;
            case 10:
                FrameReward[3].SetActive(true);
                break;
            case 100:
                FrameReward[4].SetActive(true);
                break;
            default:
                objRankToken.transform.localPosition = new Vector3(-50.0f, 0f, 0f);
                break;
        }
    }
}
