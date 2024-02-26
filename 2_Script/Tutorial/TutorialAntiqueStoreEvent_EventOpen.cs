using System.Collections.Generic;
using UnityEngine;

public class TutorialAntiqueStoreEvent_EventOpen : TutorialBase
{
    public GameObject InitAntiqueStoreScroll()
    {
        return UIPopupAntiqueStore._instance.GetAntiqueStoreItemScroll();
    }

    public List<GameObject> GetAntiqueStoreScroll()
    {
        List<GameObject> listScrollObject = new List<GameObject>();
        
        listScrollObject.Add(InitAntiqueStoreScroll());

        return listScrollObject;
    }

    public GameObject InitAntiqueStoreTopUI()
    {
        return UIPopupAntiqueStore._instance.GetAntiqueStoreTopUI();
    }

    public List<GameObject> GetAntiqueStoreTopUI()
    {
        List<GameObject> listScrollObject = new List<GameObject>();
        
        listScrollObject.Add(InitAntiqueStoreTopUI());

        return listScrollObject;
    }

    public GameObject InitAntiqueStoreDecoInfo()
    {
        return UIPopupAntiqueStore._instance.GetSpineObjectRoot();
    }
    
    public List<GameObject> GetAntiqueStoreDecoInfo()
    {
        List<GameObject> listScrollObject = new List<GameObject>();
        
        listScrollObject.Add(InitAntiqueStoreDecoInfo());

        return listScrollObject;
    }

    public void OpenPopupDecoInfo()
    {
        ManagerUI._instance.OpenPopup<UIPopupDecoInformation>(); 
    }

    public bool IsOpenPopupDecoInfo()
    {
        if (UIPopupDecoInformation._instance != null)
            return true;
        else
            return false;
    }

}
