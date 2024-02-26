using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDeco_Production : UIItemDeco
{
    [Header("ProductionLin")]
    [SerializeField] private UILabel labelDeco;
    [SerializeField] private UILabel labelDecoCount;
    [SerializeField] private GameObject objEventIcon;
    [SerializeField] private GameObject productionDecoRoot;
    [SerializeField] private UILabel productionDecoCount;

    public override void UpdataData(DecoItemData cellData)
    {
        decoItem = cellData;

        base.UpdataData(decoItem);

        if (cellData == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        if (ManagerHousing.IsHaveEventHousingList(decoItem.listDecoItem))
            objEventIcon.SetActive(true);
        else
            objEventIcon.SetActive(false);

        labelDeco.text = Global._instance.GetString($"h_{decoItem.housingIndex}_n");
        labelDeco.color = GetItemFontColor();
        labelDecoCount.text = $"{decoItem.listDecoItem.Count}";

        if(IsProductionDecoCount() > 0)
        {
            productionDecoRoot.SetActive(true);
            productionDecoCount.text = IsProductionDecoCount().ToString();
        }
        else
        {
            productionDecoRoot.SetActive(false);
        }
    }

    protected override void ActiveButton(bool IsActive = true)
    {
        base.ActiveButton(IsActive);
        
        //제작 탭에서 버튼의 라벨의 텍스트 색을 변경.
        if (labelDeco != null)
        {
            labelDeco.color = IsActive ? new Color32(255, 255, 255, 255) : GetItemFontColor();
            labelDeco.effectStyle = IsActive ? UILabel.Effect.Outline8 : UILabel.Effect.None;
        }
    }

    private Color32 GetItemFontColor()
    {
        Color32 color = new Color32();

        color = decoItem.landIndex % 2 == 0 ? new Color32(20, 118, 140, 255) : new Color32(194, 111, 7, 255);

        return color;
    }

    private int IsProductionDecoCount()
    {
        int _productionCount = 0;

        foreach(var plusData in decoItem.listDecoItem)
        {
            int SuccessProduction = 0;

            for (int i = 0; i < plusData.material.Count; i++)
            {
                for(int j=0; j < ServerRepos.UserMaterials.Count; j++)
                {
                    if(ServerRepos.UserMaterials[j].index == plusData.material[i]._index)
                    {
                        if(ServerRepos.UserMaterials[j].count >= plusData.material[i]._count)
                        {
                            SuccessProduction++;
                        }
                    }
                }
            }

            if(SuccessProduction == plusData.material.Count)
            {
                _productionCount++;
            }
        }

        return _productionCount;
    }
}
