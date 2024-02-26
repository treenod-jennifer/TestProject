using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GenericReward: MonoBehaviour
{
    public UIUrlTexture rewardIcon_T;
    public UISprite     rewardIcon_S;
    public UILabel[]    rewardCount;
    public float scale = 1.0f;

    public GameObject checkObject;
    public GameObject lockObject;
    public GameObject infoButton;
    public GameObject animalFrame;

    public GameObject btnTweenHolder;
    public int tweenGroupId;

    public int type;
    public int value;

    public bool countIncludeX = true;
    public bool detailItemMode = false;
    public bool includeItemName = false;
    public bool includeAnimalFrame = false;

    private bool AnimalFrame
    {
        set
        {
            if(animalFrame != null)
            {
                animalFrame.SetActive(value);
            }
        }
    }

    Dictionary<string, string> spriteOverride;

    public void SetReward(Reward tv)
    {
        type = tv.type;
        value = tv.value;
        //보상 이미지 세팅.

        rewardIcon_T.FailEvent += () =>
        {
            rewardIcon_T.mainTexture = null;
        };

        RewardHelper.SetRewardImage(tv, rewardIcon_S, rewardIcon_T, rewardCount, scale, countIncludeX, detailItemMode);

        if(spriteOverride != null)
        {
            if( spriteOverride.ContainsKey(this.rewardIcon_S.spriteName ) )
            {
                this.rewardIcon_S.spriteName = spriteOverride[this.rewardIcon_S.spriteName];
            }
        }

        if( includeItemName && rewardCount.Length > 0)
        {
            for (int i = 0; i < rewardCount.Length; ++i)
                rewardCount[i].enabled = true;

            var str = this.rewardCount[0].text;
            this.rewardCount.SetText(RewardHelper.GetRewardName((RewardType)tv.type, value) + str);
        }

        if( infoButton != null )
        {
            switch ((RewardType)type)
            {
                case RewardType.toy:
                //case RewardType.stamp:
                case RewardType.housing:
                case RewardType.costume:
                case RewardType.animal:
                    infoButton.SetActive(true);
                    break;
                default:
                    infoButton.SetActive(false);
                    break;
            }
        }

        if (btnTweenHolder != null)
        {
            bool isTweenHoler = true;

            switch ((RewardType)type)
            {
                case RewardType.toy:
                case RewardType.housing:
                //case RewardType.stamp:
                case RewardType.animal:
                case RewardType.costume:
                    if (this.detailItemMode)
                    {
                        isTweenHoler = false;
                    }
                    break;
                default:
                    {
                        isTweenHoler = false;
                    }
                    break;
            }
            TweenersActive(isTweenHoler);
        }

        switch ((RewardType)type)
        {
            case RewardType.animal:
                AnimalFrame = includeAnimalFrame;
                break;
        }
    }
    void TweenersActive(bool isTweener)
    {
        //재사용 스크롤을 사용하게 될 때 Tweener을 Enabled을 하면 TimeScale값이 멈춰 버려서
        //Tweenr 동작은 계속 실행하고 영향을 받는 이미지 오브젝트들의 위치를 변경 해줬다.

        if (isTweener == false)
        {
            //btnTweenHolder.SetActive(isTweener);
            rewardIcon_S.transform.SetParent(this.transform, true);
            rewardIcon_T.transform.SetParent(this.transform, true);
            return;
        }

        var tweeners = btnTweenHolder.GetComponents<UITweener>();
        for (int i = 0; i < tweeners.Length; ++i)
        {
            if (tweeners[i].tweenGroup == tweenGroupId)
            {
                tweeners[i].enabled = isTweener;
                rewardIcon_S.transform.SetParent(btnTweenHolder.transform, true);
                rewardIcon_T.transform.SetParent(btnTweenHolder.transform, true);
            }
        }
    }

    public void AddSpriteOverride(string org, string overrided)
    {
        if (this.spriteOverride == null)
            spriteOverride = new Dictionary<string, string>();
        if(spriteOverride.ContainsKey(org) )
        {
            spriteOverride.Remove(org);
        }
        spriteOverride.Add(org, overrided);
    }

    public void RemoveSpriteOverride(string org)
    {
        if (this.spriteOverride == null)
            return;

        if (spriteOverride.ContainsKey(org))
        {
            spriteOverride.Remove(org);
        }
    }

    public void EnableInfoBtn(bool c)
    {
        if(btnTweenHolder != null && infoButton != null)
        {
            btnTweenHolder.GetComponent<UITweener>().enabled = c;
            infoButton.SetActive(c);
        }
    }

    public void EnableCheck(bool c)
    {
        if(checkObject != null)
        checkObject.SetActive(c);
    }

    public void EnableLock(bool c)
    {
        if (lockObject != null)
            lockObject.SetActive(c);
    }

    public void SetDepth(int depth)
    {
        rewardIcon_T.depth = depth;
        rewardIcon_S.depth = depth;
    }

    public void SetColor(Color c)
    {
        rewardIcon_T.color = c;
        rewardIcon_S.color = c;
    }

    public void SetTextColor(Color c)
    {
        for (int i = 0; i < rewardCount.Length; i++)
        {
            rewardCount[i].color = c;
        }
    }

    public void SetEffectTextColor(Color c)
    {
        for (int i = 0; i < rewardCount.Length; i++)
        {
            if (rewardCount[i].effectStyle == UILabel.Effect.None) continue;

            rewardCount[i].effectColor = c;
        }
    }

    public void SetTextFontSize(int fontSize)
    {
        for (int i = 0; i < rewardCount.Length; i++)
        {
            rewardCount[i].fontSize = fontSize;
        }
    }

    public void FadeAction(float alpha, float delay)
    {
        if (rewardIcon_T.enabled == true)
            DOTween.ToAlpha(() => rewardIcon_T.color, x => rewardIcon_T.color = x, alpha, delay);
        if (rewardIcon_S.enabled == true)
            DOTween.ToAlpha(() => rewardIcon_S.color, x => rewardIcon_S.color = x, alpha, delay);
    }

    public IEnumerator CoFadeOut(float alpha, float delay)
    {
        float time = 0f;
        while (true)
        {
            if (time > delay)
                break;

            time += Time.deltaTime * 1.5f;
            if (rewardIcon_T.enabled == true)
                rewardIcon_T.color = new Color(1f, 1, 1f, Mathf.Lerp(1f, alpha, time * 4f));
            if (rewardIcon_S.enabled == true)
                rewardIcon_S.color = new Color(1f, 1, 1f, Mathf.Lerp(1f, alpha, time * 4f));
            yield return null;
        }

        if (rewardIcon_T.enabled == true)
            rewardIcon_T.color = new Color(1f, 1, 1f, alpha);
        if (rewardIcon_S.enabled == true)
            rewardIcon_S.color = new Color(1f, 1, 1f, alpha);
    }

    public void OnClickInfo()
    {
        switch ((RewardType)type)
        {
            case RewardType.animal:
                {
                    var p = ManagerUI._instance.OpenPopup<UIPopupAnimalRewardInfo>();
                    p.InitAnimalInfo(value);
                }
                break;
            default:
                ManagerUI._instance.OpenPopup<UIPopupRewardInfo>((popup) => popup.InitData(new Reward() { type = type, value = value } ));
                break;
        }
    }

    #region 예전 상세 정보 팝업

    //IEnumerator OpenCostumeInfoPopup()
    //{
    //    Texture2D tex = null;
    //    UIPopupSystem.ImageLoadCallbackHolder imgLoader = new UIPopupSystem.ImageLoadCallbackHolder((ImageRequestableResult r) =>
    //    {
    //        tex = (Texture2D)r.texture;
    //    });

    //    string fileName = string.Format("0_{0}", value);
    //    UIImageLoader.Instance.Load(Global.gameImageDirectory, "Costume/", fileName, imgLoader);

    //    while (true)
    //    {
    //        if (imgLoader.finished == true)
    //            break;
    //        yield return null;
    //    }

    //    string cName = string.Format("cos_0_{0}", value);
    //    string costumeName = Global._instance.GetString(cName);
    //    costumeName = costumeName.Replace("\n", "");
    //    string complex1 = Global._instance.GetString("reward_if_1");
    //    string complex2 = Global._instance.GetString("reward_if_2");

    //    string baseString = "";
    //    if( costumeName.Length <= 4)
    //    {
    //        baseString = complex1 + "\n「{0}」" + complex2;
    //    }
    //    else if ( costumeName.Length <= 7 )
    //    {
    //        baseString = complex1 + "「{0}」\n" + complex2;
    //    }
    //    else
    //    {
    //        baseString = complex1 + "\n「{0}」\n"+ complex2;
    //    }
    //    string complexString = string.Format(baseString, costumeName);

    //    string title = "コスチューム情報";
    //    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
    //    popup.InitSystemPopUp(title, complexString, false, tex);
    //    popup.SortOrderSetting();

    //    while (true)
    //    {
    //        if (UIPopupSystem._instance == null)
    //            break;
    //        yield return null;
    //    }
    //}

    #endregion

}
