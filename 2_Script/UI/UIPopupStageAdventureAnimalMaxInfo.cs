using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureAnimalMaxInfo : UIPopupBase {
    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo animalMaxInfo;

    public void InitAnimalMaxInfo(ManagerAdventure.AnimalInstance aData)
    {
        animalMaxInfo.SetAnimalSelect(aData);
    }
}
