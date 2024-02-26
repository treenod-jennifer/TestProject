using System.Collections.Generic;
using UnityEngine;

public class UIPopupAntiqueStoreInfo : UIPopupBase
{
    [SerializeField] private List<UILabel> listLabelInfo;
    
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprAntiqueStoreInfoList;
    
    private void Start()
    {
        listLabelInfo[0].text = Global._instance.GetString("antique_i_3").Replace("[n]", $"{ManagerAntiqueStore.instance.GetAntiqueToken(ServerContents.AntiqueStore.newClearCount)}");
        listLabelInfo[1].text = Global._instance.GetString("antique_i_3").Replace("[n]", $"{ManagerAntiqueStore.instance.GetAntiqueToken(ServerContents.AntiqueStore.endCount)}");

        for (int i = 0; i < sprAntiqueStoreInfoList.Count; i++)
        {
            sprAntiqueStoreInfoList[i].atlas =
                ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.AtlasUI;
        }
    }
}