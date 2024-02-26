using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GimmickTutorialItem : MonoBehaviour
{
    public delegate void gimmickItemEvent(GimmickTutorial_Data data);

    [System.NonSerialized] public GimmickTutorial_Data gimmickData;
    [SerializeField] public UITexture gimmickTexture;
    [SerializeField] public UISprite btnSprite;

    [System.NonSerialized] public gimmickItemEvent clickEvent;

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

    public void Init(GimmickTutorial_Data data)
    {
        gimmickData = new GimmickTutorial_Data();
        data.CopyAllTo(gimmickData);

        //스프라이트 변경
        string gimmickTextureName = ManagerGimmickTutorial.instance.GetGimmickNameByTutorialType(gimmickData.gimmickType);
        Box.LoadCDN("GimmickTutorial", $"{gimmickTextureName}.png", (Texture2D loadTexture) => {
            gimmickTexture.mainTexture = loadTexture;
        });
    }

    private void BtnUse()
    {
        clickEvent?.Invoke(gimmickData);
        SetButtonSprite(true);
    }

    public void SetButtonSprite(bool isSelected)
    {
        if (isSelected) btnSprite.spriteName = "housing_box_01";
        else btnSprite.spriteName = "housing_box_02";
    }
}
