using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDiaryMaterial : MonoBehaviour
{
    public UIMaterialTexture matTexture;
    public UILabel[]    matCount;
    public UISprite     check;

    private Color clearColor = new Color(112f/255f, 141f/255f, 160f/255f, 1f);
    private Color defaultColor = new Color(255f/255f, 2551f/255f, 255f/255f, 1f);

    public int InitDiaryMaterial(HousingMaterialData data)
    {
        matTexture.InitMaterialTexture(data._index, 80, 80);

        int materialCount = 0;
        for (int i = 0; i < ServerRepos.UserMaterials.Count; i++)
        {
            if (ServerRepos.UserMaterials[i].index == data._index)
            {
                materialCount = ServerRepos.UserMaterials[i].count;
            }
        }

        if (materialCount >= data._count)
        {
            check.enabled = true;
            matCount[0].effectStyle = UILabel.Effect.None;
            matCount[0].color = clearColor;
            matCount[1].enabled = false;
        }
        else
        {
            check.enabled = false;
            matCount[0].effectStyle = UILabel.Effect.Outline8;
            matCount[0].color = defaultColor;
            matCount[1].text = string.Format("{0}/{1}", materialCount, data._count);
        }

        //재료의 수가 필요한 재료수를 넘을 경우 같게 표시.
        if (materialCount > data._count)
        {
            materialCount = data._count;
        }

        matCount[0].text = string.Format("{0}/{1}", materialCount, data._count);

        if (materialCount >= data._count)
        {
            materialCount = data._count;
        }
        //재료 수 반환.
        return materialCount;
    }
}
