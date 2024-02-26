using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureAnimalTabExplanation : MonoBehaviour
{
    [SerializeField] private UILabel explanation;

    private void OnEnable()
    {
        SettingText();
    }

    public void SettingText()
    {
        switch (UIPopupStageAdventureAnimal._instance.currentFilter)
        {
            case ManagerAdventure.AnimalFilter.AF_ALL:
                explanation.text = Global._instance.GetString("p_frd_l_7");
                break;
            case ManagerAdventure.AnimalFilter.AF_MONSTER:
                explanation.text = Global._instance.GetString("p_frd_l_8");
                break;
            case ManagerAdventure.AnimalFilter.AF_EVENT_BONUS:
                explanation.text = Global._instance.GetString("adv_ev_li_1");
                break;
            case ManagerAdventure.AnimalFilter.AF_OVERLAP_1:
                explanation.text = IsOverlapAnimalListEmptyCheak();
                break;
            case ManagerAdventure.AnimalFilter.AF_OVERLAP_2:
                explanation.text = IsOverlapAnimalListEmptyCheak();
                break;
            case ManagerAdventure.AnimalFilter.AF_OVERLAP_3:
                explanation.text = IsOverlapAnimalListEmptyCheak();
                break;
            case ManagerAdventure.AnimalFilter.AF_OVERLAP_4:
                explanation.text = IsOverlapAnimalListEmptyCheak();
                break;
            case ManagerAdventure.AnimalFilter.AF_OVERLAP_5:
                explanation.text = IsOverlapAnimalListEmptyCheak();
                break;
            default:
                explanation.text = "";
                break;
        }
    }

    private string IsOverlapAnimalListEmptyCheak()
    {
        //Global._instance.GetString("p_frd_l_9")
        //형태로 키값 받기
        if (UIPopupStageAdventureAnimal._instance.characterList.Count > 0 && UIPopupStageAdventureAnimal._instance.characterList[0].overlap > 0)
            return Global._instance.GetString("p_ovl_li_2");
        else
            return Global._instance.GetString("p_ovl_li_3");

    }

}
