using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMarkController : MonoBehaviour
{
    [SerializeField] private UIItemAdventureAnimalInfo  animalInfo;
    [SerializeField] private GameObject                 newMark;

    // Start is called before the first frame update
    private void Awake()
    {
        animalInfo.setAnimalCallBack += NewMarkActive;
    }

    private void NewMarkActive(ManagerAdventure.AnimalInstance aData)
    {
        if (NewMarkUtility.CompareNewList(animalInfo.AnimalIdx))
        {
            NewMarkOn();
        }
        else
        {
            NewMarkOff();
        }

        DeckNewMarkDelete();
    }
    
    private void NewMarkOn()
    {
        newMark.gameObject.SetActive(true);
    }

    private void NewMarkOff()
    {
        newMark.gameObject.SetActive(false);
    }

    private void DeckNewMarkDelete()
    {
        for (int i = 2; i >= 0; --i)
        {
            var aId = ManagerAdventure.User.GetAnimalIdxFromDeck(1, i);
            if (aId == animalInfo.AnimalIdx)
                NewMarkOff();
        }
    }
}
