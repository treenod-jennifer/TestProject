using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupRewardInfo : UIPopupBase
{
    [SerializeField] private UITexture _texture;
    [SerializeField] private UILabel _label;

    private Reward reward;

    public void InitData(Reward rewardType)
    {
        reward = rewardType;

        StartCoroutine(CoSetRewardTexture());
        _label.text = SetSystemMessage();
    }

    IEnumerator CoSetRewardTexture()
    {
        Texture2D tex = null;
        bool isImgLoader = false;
        bool isFinished = false;

        switch ((RewardType)reward.type)
        {
            case RewardType.costume:
                {
                    isImgLoader = true;
                    string fileName = string.Format("0_{0}", reward.value);
                    ResourceManager.LoadCDN(Global.gameImageDirectory, "Costume", fileName, (Texture2D texture) =>
                    {
                        tex = texture;
                        isFinished = true;
                    });
                }
                break;
            case RewardType.toy:
                {
                    isImgLoader = true;
                    string fileName = $"y_i_{reward.value}";
                    ResourceManager.LoadCDN(Global.gameImageDirectory, "Pokoyura", fileName, (Texture2D texture) =>
                    {
                        tex = texture;
                        isFinished = true;
                    });
                }
                break;
            case RewardType.housing:
                {
                    isImgLoader = true;
                    int housingIdx = (int)(reward.value / 10000);
                    int modelIdx = (int)(reward.value % 10000);
                    string fileName = $"{housingIdx}_{modelIdx}";
                    ResourceManager.LoadCDN(Global.gameImageDirectory, "IconHousing", fileName, (Texture2D texture) =>
                    {
                        tex = texture;
                        isFinished = true;
                    });
                }
                break;
            default:
                tex = null;
                break;
        }

        if (isImgLoader) yield return new WaitUntil(() => isFinished);

        _texture.mainTexture = tex;
        _texture.MakePixelPerfect();
    }

    string SetSystemMessage()
    {
        string complex1 = Global._instance.GetString("reward_if_3")/* "이벤트를 진행하면 \n[1]을 획득할 수 있어!"*/;
        string complex2 = null;

        switch ((RewardType)reward.type)
        {
            case RewardType.costume:
                {
                    complex2 = Global._instance.GetString("item_9");
                }
                break;
            case RewardType.toy:
                {
                    complex2 = Global._instance.GetString("item_8");
                }
                break;
            case RewardType.stamp:
                {
                    complex2 = Global._instance.GetString("item_5");
                }
                break;
            case RewardType.housing:
                {
                    complex2 = Global._instance.GetString("item_24");
                }
                break;
            default:
                break;
        }

        string complexString = complex1.Replace("[1]", complex2);

        return complexString;
    }

}
