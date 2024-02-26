using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpAdventureAnimalSelectOverlap : UIPopupStageAdventureAnimal
{
    [Header("Label")]
    [SerializeField] private UILabel Title;
    [SerializeField] private UILabel Title_Shadow;

    [SerializeField] private UIItemAdventureAnimalTab overlaptap;

    [HideInInspector] static public int overlapItem;
    public delegate void OnSelectAnimal(int animalIndex);

    public OnSelectAnimal onSelectOK;

    public int selectedAnimal = 0;
   
    protected override void InitData()
    {
        characterList.Clear();

        var tempList = ManagerAdventure.User.GetAnimalList(
            includeNull: GetMode != PopupMode.ChangeMode,
            filter: currentFilter,
            sortOpt: currentSort,
            deckAnimalFirst : false
        );

        lanpageButtons[0].On("LGPKV_limit_break", Global._instance.GetString("p_ovl_li_4"));

        characterList.AddRange(tempList);
    }

    public void InitTarget(PopupMode popupMode, int overlapItemInfo = 0)
    {
        base.InitTarget(popupMode);

        overlapItem = overlapItemInfo;

        OverLapTitle();

        currentFilter = GetOverlapStar();

        overlaptap.SetFilter(currentFilter);

    }

    private void OverLapTitle()
    {
        Title.text = Global._instance.GetString("p_ovl_li_1").Replace("[n]", overlapItem.ToString());
        Title_Shadow.text = Global._instance.GetString("p_ovl_li_1").Replace("[n]", overlapItem.ToString());
    }


    private ManagerAdventure.AnimalFilter GetOverlapStar()
    {
        ManagerAdventure.AnimalFilter over;


        switch (overlapItem)
        {
            case 1:
                over = ManagerAdventure.AnimalFilter.AF_OVERLAP_1;
                break;
            case 2:
                over = ManagerAdventure.AnimalFilter.AF_OVERLAP_2;
                break;
            case 3:
                over = ManagerAdventure.AnimalFilter.AF_OVERLAP_3;
                break;
            case 4:
                over = ManagerAdventure.AnimalFilter.AF_OVERLAP_4;
                break;
            case 5:
                over = ManagerAdventure.AnimalFilter.AF_OVERLAP_5;
                break;
            default:
                over = ManagerAdventure.AnimalFilter.AF_ALL;
                break;
        }

        return over;
    }

    public void Tab_Grade()
    {
        OnTab(GetOverlapStar());
    }

    protected override void OnFocus(bool focus)
    {
        if (focus)
        {
            if( this.selectedAnimal != 0)
            {
                this.onSelectOK(selectedAnimal);
                ManagerUI._instance.ClosePopUpUI();
            }
        }
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        UIPopupMailBox._instance.bCanTouch = true;
    }
}
