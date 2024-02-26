using System.Collections.Generic;
using UnityEngine;

public class UIItemEndContentsBoost : MonoBehaviour
{
    [SerializeField] GameObject ScoreUpBg;
    [SerializeField] UILabel ScoreUpLabel;
    [SerializeField] List<UISprite> gaugeList;

    public void Init(int buffCount)
    {
        for (int i = 0; i < gaugeList.Count; i++)
        {
            gaugeList[i].spriteName = i < buffCount ? "ec_bonus_button_on" : "ec_bonus_button_off";
            gaugeList[i].MakePixelPerfect();
        }

        Color colorTop = Color.white;
        Color colorBottom = Color.white;
        ColorUtility.TryParseHtmlString(buffCount == 7 ? "#ffef83" : "#ffffff", out colorTop);
        ColorUtility.TryParseHtmlString(buffCount == 7 ? "#ffe431" : "#ffbb57", out colorBottom);
        ScoreUpLabel.gradientTop = colorTop;
        ScoreUpLabel.gradientBottom = colorBottom;
        ScoreUpLabel.text = $"+{ManagerEndContentsEvent.instance.GetBuffRatio()[buffCount]}%";
        ScoreUpBg.SetActive(buffCount == 7);
    }
}
    