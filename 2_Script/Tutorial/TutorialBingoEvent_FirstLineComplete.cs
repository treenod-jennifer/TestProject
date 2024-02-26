using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBingoEvent_FirstLineComplete : TutorialBase
{
    public GameObject GetBingoBoard()
    {
        return UIPopupBingoEvent_Board._instance.grid;
    }

    public GameObject GetRewardList()
    {
        return UIPopupBingoEvent_Board._instance.GetComponentInChildren<UIGrid>().gameObject;
    }

    public List<GameObject> GetBingoRewardButtonAnchor()
    {
        List<GameObject> listGameObject = new List<GameObject>();
        
        listGameObject.Add(UIPopupBingoEvent_Board._instance.sprBlockRewardGetButton);

        return listGameObject;
    }

}
