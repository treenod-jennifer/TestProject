using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonExpansionSort : MonoBehaviour 
{
    [Header("Linked Object")]
    [SerializeField] private UIButtonAnimalSort[] sortButtons;

    [SerializeField] private UISprite selectIcon;
    [SerializeField] private UIButtonDropDown dropDown;

    private void Awake()
    {
        foreach (var button in sortButtons)
        {
            button.Init(this);
        }
        dropDown.PostExpansionEvent += () => SetSortButtonsActive(true);
        dropDown.PostReductionEvent += () => SetSortButtonsActive(false);
    }
    #region 버튼 상태(현재 정렬 상태) 관련 기능


    public void ChangeSortIcon(UISprite icon)
    {
        selectIcon.spriteName = icon.spriteName;
        selectIcon.height = icon.height;
        selectIcon.width = icon.width;
        selectIcon.transform.localScale = icon.transform.localScale;
    }

    public void Sort(ManagerAdventure.AnimalSortOption sortOption)
    {
        UIPopupStageAdventureAnimal._instance.currentSort = sortOption;
        UIPopupStageAdventureAnimal._instance.RePaint();
    }

    private void SetSortButtonsActive(bool trigger)
    {
        foreach(var button in sortButtons)
            button.SetButtonActive(trigger);
    }
    #endregion
}
