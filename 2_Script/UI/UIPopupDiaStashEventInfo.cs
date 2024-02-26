using System.Collections.Generic;
using UnityEngine;

public class UIPopupDiaStashEventInfo : UIPopupBase
{
    [SerializeField] private List<UISprite> listDiaStashSprite;
    
    private void Start()
    {
        for (int i = 0; i < listDiaStashSprite.Count; i++)
        {
            listDiaStashSprite[i].atlas = ManagerDiaStash.instance.diaStashResource.GetDiaStashPack().AtlasUI;
        }
    }
}
