using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureAnimalEvent : UIPopupStageAdventureAnimal
{
    public override void InitTarget(PopupMode popupMode)
    {
        base.InitTarget(popupMode);

        if(popupMode == PopupMode.ViewMode)
        {
            currentFilter = ManagerAdventure.AnimalFilter.AF_EVENT_BONUS;
        }
    }

    public void Tab_EventBonus()
    {
        OnTab(ManagerAdventure.AnimalFilter.AF_EVENT_BONUS);
    }
}
