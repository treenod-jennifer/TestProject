using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLandItem : MonoBehaviour
{
    private UIPopupLandMove.LandData landData = new UIPopupLandMove.LandData();

    [SerializeField] private UIUrlTexture texLandImage;
    [SerializeField] private UITexture blockLandImage;
    [SerializeField] private UILabel labelLandName;
    [SerializeField] private UISprite sprLandNameBG;
    [SerializeField] private UISprite sprDottedLine;
    [SerializeField] private GameObject sprDiagonalLine;
    [SerializeField] private GameObject sprNewLand;
    
    [SerializeField] Color[] colorLandBG;
    [SerializeField] Color[] colorDottedLine;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void OnLoadComplete(Texture2D r)
    {
        texLandImage.mainTexture = r;
        blockLandImage.mainTexture = r;
        texLandImage.MakePixelPerfect();
    }

    public void UpdataData(UIPopupLandMove.LandData landData)
    {
        this.landData = landData;

        //랜드 이미지 세팅
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconLandImage", $"land_icon_{this.landData.landIndex}.png", OnLoadComplete, true);

        //새로운 랜드
        if (PlayerPrefs.HasKey($"VisitedLand_{landData.landIndex}") || landData.IsActiveLand == false || landData.landIndex == 0)
            sprNewLand.SetActive(false);
        else
            sprNewLand.SetActive(true);

        if (landData.IsActiveLand)
        {
            blockLandImage.gameObject.SetActive(false);
        }
        else
        {
            blockLandImage.gameObject.SetActive(true);
        }

        //랜드 이름 세팅
        labelLandName.text = Global._instance.GetString($"ld_{this.landData.landIndex}");
        SetLandBgSize();

        //랜드 컬러 세팅
        if(this.landData.landIndex != 0)
        {
            sprLandNameBG.color = colorLandBG[IsHousingLand() ? 0 : 1];
            SetDottedLine();
        }
    }

    void SetDottedLine()
    {
        if(landData.landIndex == 1 || landData.landIndex == 100)
        {
            sprDottedLine.gameObject.SetActive(false);
            sprDiagonalLine.SetActive(true);

            if (IsHousingLand())
            {
                sprDiagonalLine.transform.localScale = Vector3.one;
                sprDiagonalLine.GetComponentInChildren<UISprite>().color = colorDottedLine[0];
            }
            else
            {
                sprDiagonalLine.transform.localScale = new Vector3(-1f, 1f, 1f);
                sprDiagonalLine.GetComponentInChildren<UISprite>().color = colorDottedLine[1];
            }
        }
        else
        {
            sprDiagonalLine.SetActive(false);
            sprDottedLine.gameObject.SetActive(true);

            if (IsHousingLand())
            {
                sprDottedLine.color = colorDottedLine[0];
            }
            else
            {
                sprDottedLine.color = colorDottedLine[1];
            }
        }
    }

    bool IsHousingLand()
    {
        return landData.landIndex > 99;
    }

    public void OnClickLanMove()
    {
        if (UIPopupLandMove.canClickMoveLand == false)
            return;

        if (landData.landIndex == ManagerLobby.landIndex) return;

        if(landData.IsActiveLand == false)
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popup.SortOrderSetting();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_58"), false);

            return;
        }

        UIPopupLandMove.canClickMoveLand = false;
        if (PlayerPrefs.HasKey($"VisitedLand_{landData.landIndex}") == false)
            PlayerPrefs.SetInt($"VisitedLand_{landData.landIndex}", landData.landIndex);
        ManagerLobby._instance.MoveLand(landData.landIndex);
    }

    void SetLandBgSize()
    {
        sprLandNameBG.width = (int)labelLandName.localSize.x + 20;
    }
}
