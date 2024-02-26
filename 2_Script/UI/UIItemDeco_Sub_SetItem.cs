using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_Sub_SetItem : MonoBehaviour
{
    [Header("ObjectLink")]
    [SerializeField] private UIUrlTexture texDeco;
    [SerializeField] private UIUrlTexture texDecoShadow;
    [SerializeField] private GameObject btnCheck;
    [SerializeField] private UILabel labelDeco;

    [Header("bActiveDecoLink")]
    [SerializeField] private GameObject objbActiveDeco;

    private PlusHousingModelData decoItem;

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
        texDeco.mainTexture = r;
        texDeco.width = 80;
        texDeco.height = 80;

        texDecoShadow.mainTexture = r;
        texDecoShadow.width = 80;
        texDecoShadow.height = 80;
    }

    void SetDecoItem()
    {
        labelDeco.text = Global._instance.GetString($"h_{decoItem.housingIndex}_{decoItem.modelIndex}");
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconHousing", $"{decoItem.housingIndex}_{decoItem.modelIndex}", OnLoadComplete, true);
    }

    public void UpdateData(PlusHousingModelData cellData)
    {
        decoItem = cellData;

        //아이템 세팅
        SetDecoItem();

        int selectedModelIdx = ManagerHousing.GetSelectedHousingModelIdx(decoItem.housingIndex);

        if (selectedModelIdx == decoItem.modelIndex)
            btnCheck.SetActive(true);
        else
            btnCheck.SetActive(false);

        if(decoItem.active == 0)
        {
            objbActiveDeco.SetActive(true);
            labelDeco.color = new Color32(120, 134, 143, 255);
        }
        else
        {
            objbActiveDeco.SetActive(false);
            labelDeco.color = new Color32(58, 94, 145, 255);
        }
    }
}
