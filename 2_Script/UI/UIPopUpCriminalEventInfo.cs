using System.Collections.Generic;
using UnityEngine;

public class UIPopUpCriminalEventInfo : UIPopupBase
{
    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprCriminalInfoList;

    private void Start()
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprCriminalInfoList.Count; i++)
        {
            sprCriminalInfoList[i].atlas =
                ManagerCriminalEvent.instance.criminalEventPack.AtlasUI;
        }
    }
}