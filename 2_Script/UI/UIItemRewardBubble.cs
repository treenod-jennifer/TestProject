using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemRewardBubble : MonoBehaviour
{
    [SerializeField]
    private UISprite bubble;
    [SerializeField]
    private GameObject checkObj;
    [SerializeField]
    private GameObject rewardObject;
    [SerializeField]
    private List<GenericReward> listRewards;

    private int bubbleWidth = 25;
    private int rewardSize = 95;

    private Vector3 rewardStartPosition = new Vector3(-7f, 5f, 0f);
    private int rewardOffestX = -56;

    public void InitBubble(List<Reward> rewards, bool isGetReward)
    {
        //보상 아이콘 설정
        int rewardCnt = rewards.Count;
        InitGenericReward(rewardCnt);
        for (int i = 0; i < rewardCnt; i++)
        {
            if (listRewards.Count <= i)
                break;

            listRewards[i].SetReward(rewards[i]);
            SetPosition_GenericReward(i, rewardCnt - 1);
        }

        //버블 크기 설정
        bubble.width = bubbleWidth + (rewardSize * rewards.Count);

        //보상 획득 상태에 따라 체크표시
        SetCheckObj(isGetReward);
    }

    public void InitBubble(Reward reward, bool isGetReward)
    {
        //보상 아이콘 설정
        InitGenericReward(1);
        listRewards[0].SetReward(reward);
        SetPosition_GenericReward(0, 0);

        //버블 크기 설정
        bubble.width = rewardSize;

        //보상 획득 상태에 따라 체크표시
        SetCheckObj(isGetReward);
    }

    public void SetCheckObj(bool isActive)
    {
        checkObj.SetActive(isActive);
    }

    //보상 아이콘이 보상 수 보다 적을 경우, 오브젝트 생성한 뒤, 리스트에 추가해줌.
    private void InitGenericReward(int rewardCount)
    {
        for (int i = 0; i < rewardCount; i++)
        {
            if (listRewards.Count <= i)
            {
                MakeGenericReward();
            }
        }
    }

    private void MakeGenericReward()
    {
        var go = Instantiate(rewardObject, this.transform);
        GenericReward genericReward = go.GetComponent<GenericReward>();
        if (genericReward == null)
        {
            go.SetActive(false);
            return;
        }
        else
        {
            listRewards.Add(genericReward);
        }
    }

    private void SetPosition_GenericReward(int index, int maxIndex)
    {
        float posX = rewardStartPosition.x + (rewardOffestX * (maxIndex - index));
        Vector3 targetPos = new Vector3(posX, rewardStartPosition.y, rewardStartPosition.z);
        listRewards[index].transform.localPosition = targetPos;
    }
}
