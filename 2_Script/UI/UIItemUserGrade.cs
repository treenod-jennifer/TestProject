using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIItemUserGrade : MonoBehaviour
{
    public UILabel gradeRangeLabel;
    public UILabel gradeNameLabel;

    RankingPointGradeData item;

    private void InitItemText()
    {
        string pointText = Global._instance.GetString("p_g_if_4");
        pointText = pointText.Replace("[0]", item.pointMin.ToString());

        gradeRangeLabel.text = pointText;
        
        gradeNameLabel.text = Global._instance.GetString(item.strKey);
        if (gradeNameLabel.text == "")
        {
            gradeNameLabel.text = "-";
        }
        else
        {
            gradeNameLabel.color = item.colorTint;
            gradeNameLabel.effectColor = item.effectColor;
            gradeNameLabel.gradientTop = item.topColor;
            gradeNameLabel.gradientBottom = item.bottomColor;
            gradeNameLabel.applyGradient = true;
        }
    }

    public void UpdateData(RankingPointGradeData gradeData)
    {
        item = gradeData;
        InitItemText();
    }

    void OnClickBtnClose()
    {
        ManagerUI._instance.ClosePopUpUI();
    }
}
