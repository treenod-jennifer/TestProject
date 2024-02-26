using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureAnimalSelect : MonoBehaviour {
    private ManagerAdventure.AnimalInstance aData;

    [SerializeField] private UIItemAdventureAnimalInfo animalInfo;

    private void Awake()
    {
        var button = animalInfo.GetComponent<UIPokoButton>();

        animalInfo.setAnimalCallBack += (aData) =>
        {
            this.aData = aData;

            bool isChangeMode = (UIPopupStageAdventureAnimal._instance.GetMode == UIPopupStageAdventureAnimal.PopupMode.ChangeMode);
            animalInfo.AnimalInfoButton = isChangeMode;
            
            if (button != null)
            {
                switch(UIPopupStageAdventureAnimal._instance.GetMode)
                {
                    case UIPopupStageAdventureAnimal.PopupMode.ViewMode:
                        button.functionName = "OpenPopupAnimalInfo";
                        break;
                    case UIPopupStageAdventureAnimal.PopupMode.ChangeMode:
                        button.functionName = "OnClickChange";
                        break;
                    case UIPopupStageAdventureAnimal.PopupMode.OverLapMode:
                        button.functionName = "OnClickOverlapping";
                        break;
                }
            }

        };
    }
    
    public void OnClickChange()
    {
        if (!UIPopupStageAdventureAnimal._instance.bCanTouch)
            return;

        if (aData.idx == ManagerAdventure.User.GetAnimalIdxFromDeck(1, UIPopupStageAdventureReady._instance.selectSlot))
            return;

        UIPopupStageAdventureAnimal._instance.bCanTouch = false;
        ManagerUI._instance.OpenPopupAdventureAnimalChange(ManagerAdventure.User.GetAnimalFromDeck(1, UIPopupStageAdventureReady._instance.selectSlot), aData);
    }

    public void OnClickOverlapping()
    {
        if (!animalInfo.AnimalActive)
            return;

        ManagerUI._instance.OpenPopup<UIPopupAdventureAnimalOverlapConfirm>((p) => { p.InitAnimalInfo(aData, null, UIPopUpAdventureAnimalSelectOverlap.overlapItem); });
    }

}
