using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupUserGradeInfo : UIPopupBase
{
    public UIReuseGrid_UserGradeInfo reuserGrid;
    public UIPanel scrollPanel;
    public UILabel[] title;
    public UILabel[] btnCloseText;
    public UILabel lodingText;
    public UILabel pointText;
    public UILabel gradeText;

    public void Init()
    {   
        scrollPanel.depth = uiPanel.depth + 1;
        InitPopupText();
        
        lodingText.gameObject.SetActive(false);
        reuserGrid.InitReuseGrid((Global._instance._strRankingPointGradeData.Count - 1), 0);
    }

    private void InitPopupText()
    {
        string titleText = Global._instance.GetString("p_g_if_1");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }
        string btnText = Global._instance.GetString("btn_1");
        for (int i = 0; i < title.Length; i++)
        {
            btnCloseText[i].text = btnText;
        }
        pointText.text = Global._instance.GetString("p_g_if_2");
        gradeText.text = Global._instance.GetString("p_g_if_3");
    }
}
