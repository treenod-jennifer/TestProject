using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMaterialAcquisition : UIPopupBase
{
    [SerializeField] private UIMaterialTexture materialTexture;
    [SerializeField] private UILabel materialCount;
    [SerializeField] private UILabel materialName;

    public void Init(ServerUserMaterial serverUserMaterial)
    {
        materialTexture.InitMaterialTexture(serverUserMaterial.index, 100, 100);
        materialCount.text = $"+{serverUserMaterial.count}";
        materialName.text = Global._instance.GetString($"mt_{serverUserMaterial.index}");
    }
}
