using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWeightInfo : MonoBehaviour
{
    [SerializeField] private UISprite mark;
    [SerializeField] private UILabel text;

    private UIItemWeightIcon.WeightInfo weightInfo;

    public void SetWeightInfo(UIItemWeightIcon.WeightInfo weightInfo)
    {
        this.weightInfo = weightInfo;

        mark.spriteName = GetMarkName(weightInfo.weightType);
        mark.MakePixelPerfect();
        mark.transform.localScale = Vector3.one * 0.7f;

        WeightUtility.SetLabel(weightInfo.weight, text);

        gameObject.SetActive(true);
    }

    public void OffWeightInfo()
    {
        gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public int GetWeight()
    {
        return weightInfo.weight;
    }

    private string GetMarkName(UIItemWeightIcon.WeightType weightType)
    {
        switch (weightType)
        {
            case UIItemWeightIcon.WeightType.Attribute_Scissors:
                return "animal_attr_1";
            case UIItemWeightIcon.WeightType.Attribute_Rock:
                return "animal_attr_2";
            case UIItemWeightIcon.WeightType.Attribute_Paper:
                return "animal_attr_3";
            case UIItemWeightIcon.WeightType.EventBonus:
                return "icon_adventureEvent";
            default:
                return string.Empty;
        }
    }
}
