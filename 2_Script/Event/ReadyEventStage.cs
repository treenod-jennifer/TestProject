using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyEventStage : MonoBehaviour {

    [System.NonSerialized]
    public int stageCount = -1;

    public GameObject materialRoot;
    public UIUrlTexture materialTexture;
    public UILabel GetCountLabel;
    public UILabel MaxCountLabel;

    public void SetGetMaterial(int tempMax, int tempGet, int tempMaterial)
    {        
        string fileName = "mt_" + (tempMaterial);
        materialTexture.SettingTextureScale(50, 50);
        materialTexture.Load(Global.gameImageDirectory, "IconMaterial/", fileName);

        GetCountLabel.text = tempGet + "/";
        MaxCountLabel.text = tempMax.ToString();

        if(tempGet == tempMax)
        {
            GetCountLabel.color = new Color(128f / 255f, 251f / 255f, 1f, 1f);
            MaxCountLabel.color = new Color(128f / 255f, 251f / 255f, 1f, 1f);

            GetCountLabel.effectColor = new Color(0f, 78f / 255f, 103f / 255f, 130f / 255f);
            MaxCountLabel.effectColor = new Color(0f, 78f / 255f, 103f / 255f, 130f / 255f);
        }
    }
    
    void SelectStage()
    {
        if (stageCount == -1) return;

        UIPopupReady._instance.ChangeEventStage(stageCount);
    }
}
