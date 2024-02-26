using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectEventRewardType
{
    normal,     //보상 1개.
    multi,      //보상 2개
    big_normal, //큰 보상 말풍선(1개).
    big_multi,  //큰 보상 말풍선(2개).
}

public class UIItemSpecialEventBubble : MonoBehaviour
{
    public UISprite bubble;
    public GameObject tailObj;
    public GameObject checkObj;

    //보상.
    public GameObject[]     rewardObj;
    public UIUrlTexture[]   rewardIcon_T;
    public UISprite[]       rewardIcon_S;
    public UILabel[]        rewardCount;
    public UILabel[]        rewardCount_S;
    public GenericReward[]  rewardGeneric;

    //말풍선 타입.
    private CollectEventRewardType type;

    public void InitBubble(CollectEventRewardType rType, List<Reward> tv, bool bCheck)
    {
        bool bBig = false;
        type = rType;
        
        //기본 타입일 경우.
        if (type == CollectEventRewardType.normal)
        {
            rewardObj[1].gameObject.SetActive(false);

            bubble.width = 100;
            bubble.height = 85;
            gameObject.transform.localPosition = new Vector3(-20f, 25f, 0f);
            tailObj.transform.localPosition = new Vector3(19f, 1.2f, 0f);
            checkObj.transform.localPosition = new Vector3(38f, 85f, 0f);
            rewardObj[0].transform.localPosition = new Vector3(-7.5f, 50f, 0f);
        }

        //보상이 2개인 타입일 경우.
        else if (type == CollectEventRewardType.multi)
        {
            rewardObj[1].gameObject.SetActive(true);

            bubble.width = 170;
            bubble.height = 85;
            gameObject.transform.localPosition = new Vector3(-50f, 25f, 0f);
            tailObj.transform.localPosition = new Vector3(50f, 1.2f, 0f);
            checkObj.transform.localPosition = new Vector3(70f, 85f, 0f);
            rewardObj[0].transform.localPosition = new Vector3(-40.0f, 50f, 0f);
            rewardObj[1].transform.localPosition = new Vector3(33.0f, 50f, 0f);
        }

        //맨 마지막 말풍선(보상 1개) 타입일 경우.
        else if (type == CollectEventRewardType.big_normal)
        {
            rewardObj[1].gameObject.SetActive(false);

            bubble.width = 150;
            bubble.height = 130;
            tailObj.transform.localPosition = new Vector3(43f, 1.2f, 0f);
            checkObj.transform.localPosition = new Vector3(72f, 130f, 0f);
            rewardObj[0].transform.localPosition = new Vector3(7.5f, 70f, 0f);
            rewardCount[0].transform.localPosition = new Vector3(45f, -35f, 0f);
            bBig = true;
        }

        //맨 마지막 말풍선(보상 2개) 타입일 경우.
        else
        {
            rewardObj[1].gameObject.SetActive(true);
            
            bubble.width = 180;
            bubble.height = 105;
            gameObject.transform.localPosition = new Vector3(-53f, 55f, 0f);
            tailObj.transform.localPosition = new Vector3(53f, 1.2f, 0f);
            checkObj.transform.localPosition = new Vector3(72f, 105f, 0f);
            rewardObj[0].transform.localPosition = new Vector3(-40.0f, 57f, 0f);
            rewardObj[1].transform.localPosition = new Vector3(40.0f, 57f, 0f);
        }

        //유저가 아이템 모은 수에 따라 보상 체크 설정.
        checkObj.SetActive(bCheck);

        //보상 이미지 세팅.
        SettingRewardImage(tv, bBig);
    }
    
    public void SetCheckObject(bool active)
    {
        checkObj.SetActive(active);
    }

    private void SettingRewardImage(List<Reward> tv, bool bBig)
    {
        for (int i = 0; i < tv.Count; i++)
        {
            if (i >= rewardIcon_S.Length)
                break;

            bool bSprite = true;
            bool bCount = true;
            RewardType type = (RewardType)tv[i].type;
            int value = tv[i].value;

            UILabel[] lblCount = new UILabel[2];
            lblCount[0] = rewardCount[i];
            lblCount[1] = rewardCount_S[i];
            
            //RewardHelper.SetRewardImage(tv[i], rewardIcon_S[i], rewardIcon_T[i], lblCount, bBig ? 1.5f : 1.0f);

            rewardGeneric[i].SetReward(tv[i]);

            //카운트.
            if (bCount == true)
            {
                if (bBig == true)
                {
                    rewardObj[i].transform.localPosition = new Vector3(-11f, rewardObj[i].transform.localPosition.y, 0f);
                }
            }
            else
            {
                if (bBig == false)
                {
                    rewardIcon_S[i].transform.localPosition = new Vector3(10f, rewardIcon_S[i].transform.localPosition.y, 0f);
                    rewardIcon_T[i].transform.localPosition = new Vector3(10f, rewardIcon_T[i].transform.localPosition.y, 0f);
                }
            }
        }
    }

    //private void SettingSpriteSize(UISprite sprite, bool bBig)
    //{
    //    sprite.MakePixelPerfect();
    //    if (bBig == true)
    //    {
    //        sprite.width = (int)(sprite.width * 1.8f);
    //        sprite.height = (int)(sprite.height * 1.8f);
    //    }
    //}

    //private void SettingTextureSize(UITexture texture, bool bBig)
    //{
    //    if (bBig == true)
    //    {
    //        texture.width = 110;
    //        texture.height = 110;
    //    }
    //    else
    //    {
    //        texture.width = 70;
    //        texture.height = 70;
    //    }
    //}

    //private void SettingUrlTextureSize(UIUrlTexture texture, RewardType rewardType, bool bBig)
    //{
    //    if (bBig == true)
    //    {
    //        if (rewardType == RewardType.toy)
    //        {
    //            texture.SettingCallBack(SettingTextureBigSize);
    //        }
    //        else
    //        {
    //            rewardIcon_T[0].width = 110;
    //            rewardIcon_T[0].height = 110;
    //        }
    //    }
    //    else
    //    {
    //        texture.SettingTextureScale(70, 70);
    //    }
    //}

    //private void SettingTextureBigSize()
    //{
    //    rewardIcon_T[0].MakePixelPerfect();
    //    rewardIcon_T[0].width = (int)(rewardIcon_T[0].width * 1.5f);
    //    rewardIcon_T[0].height = (int)(rewardIcon_T[0].height * 1.5f);
    //}
}
