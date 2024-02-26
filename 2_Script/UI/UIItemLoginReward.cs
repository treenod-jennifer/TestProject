using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemLoginReward : MonoBehaviour
{
    public GenericReward genericReward;

    public UISprite rewardSprite;
    public UIUrlTexture rewardTexture;
    public UILabel rewardCountText;

    public void InitReward(Reward item, int fontSize = 30, float spriteScale = 1f, int textureScale = 100, bool bAlpha = false)
    {
        if (bAlpha == true)
        {
            rewardCountText.color = new Color(rewardCountText.color.r, rewardCountText.color.g, rewardCountText.color.b, 0f);
            rewardSprite.color = new Color(rewardSprite.color.r, rewardSprite.color.g, rewardSprite.color.b, 0f);
            rewardTexture.color = new Color(rewardTexture.color.r, rewardTexture.color.g, rewardTexture.color.b, 0f);
        }

        bool bCount = true;
        bool bSprite = true;
        //rewardType 따라서 sprite/texture 설정.

        rewardSprite.gameObject.SetActive(true);
        if (item.type == (int)RewardType.none)
        {
            rewardSprite.gameObject.SetActive(false);
        }
        else
        {
            genericReward.scale = spriteScale;
            genericReward.SetReward(item);

            UILabel[] labels = new UILabel[1];
            labels[0] = rewardCountText;
            RewardHelper.SetRewardImage(item, rewardSprite, rewardTexture, labels, spriteScale);
        }
                

        
        if (item.type == (int)RewardType.cloverFreeTime)
        {
            rewardSprite.spriteName = "icon_cloverTime_stroke_green";
            rewardCountText.transform.localPosition += new Vector3(-13f, 0f, 0f);
            rewardSprite.enabled = true;
            rewardTexture.enabled = false;
            return;
        }

        //카운트.
        if (bCount == true)
        {
            genericReward.SetTextFontSize(fontSize);
        }
    }

    public void RewardItemAlpha(float alphaTime)
    {
        if(rewardSprite.enabled == true)
            DOTween.ToAlpha(() => rewardSprite.color, x => rewardSprite.color = x, 1f, alphaTime).SetEase(Ease.InQuart);
        else
            DOTween.ToAlpha(() => rewardTexture.color, x => rewardTexture.color = x, 1f, alphaTime).SetEase(Ease.InQuart);
        DOTween.ToAlpha(() => rewardCountText.color, x => rewardCountText.color = x, 1f, alphaTime).SetEase(Ease.InQuart);
    }
}
