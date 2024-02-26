using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBingoEvent_EventOpen : TutorialBase
{
    public GameObject InitBingoBoard()
    {
        return UIPopupBingoEvent_Board._instance.grid;
    }

    public List<GameObject> GetBingoSlotAnchor()
    {
        List<GameObject> listObject = new List<GameObject>();
        
        for (int i = 0; i < ManagerBingoEvent.instance.SelectSlot.Count; i++)
        {
            listObject.Add(UIPopupBingoEvent_Board._instance.listBingoBoardItems[ManagerBingoEvent.instance.SelectSlot[i]].gameObject);
        }

        return listObject;
    }

    public bool IsOpenPopupBingoEvent()
    {
        if (UIPopUpReady_BingoEvent._instance != null || UIPopUpBingoEvent_Bonus._instance != null)
            return true;

        return false;
    }
}
