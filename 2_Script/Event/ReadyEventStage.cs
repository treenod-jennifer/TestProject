using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReadyEventStage : MonoBehaviour {

    [System.NonSerialized]
    public int stageCount = -1;

    public GameObject materialRoot;
    public UIUrlTexture materialTexture;
    public UILabel GetCountLabel;
    public UILabel MaxCountLabel;

    public GameObject scoreRoot;
    public UISprite[] scoreBadgeImages;

    public void SetGetMaterial(int tempMax, int tempGet, int tempMaterial)
    {        
        string fileName = "mt_" + (tempMaterial);
        materialTexture.SettingTextureScale(50, 50);
        materialTexture.LoadCDN(Global.gameImageDirectory, "IconMaterial/", fileName);

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

    public void SetGetScoreBadge(int flowerLevel)
    {
        for (int i = 0; i < scoreBadgeImages.Length; i++)
        {
            scoreBadgeImages[i].spriteName = ((i + 1) <= flowerLevel) ? "score_star_on" : "score_star_off";
        }
    }

    public void SetGetScoreBadge(int prevLevel, int currentLevel)
    {
        for (int i = prevLevel; i < currentLevel; i++)
        {
            if (scoreBadgeImages.Length <= i)
                break;

            StartCoroutine(CoActionChangeFlowerLevel(i));
        }
    }

    private IEnumerator CoActionChangeFlowerLevel(int index)
    {
        Vector3 originScale = scoreBadgeImages[index].transform.localScale;
        scoreBadgeImages[index].transform.DOScale(0f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        scoreBadgeImages[index].spriteName = "score_star_on";
        scoreBadgeImages[index].transform.DOScale(originScale, 0.2f).SetEase(Ease.OutBack);
    }

    void SelectStage()
    {
        if (stageCount == -1) return;

        UIPopupReady._instance.ChangeEventStage(stageCount);
    }
}
