using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonAnimalSort : MonoBehaviour
{
    private UIButtonExpansionSort rootButton;

    [SerializeField] private ManagerAdventure.AnimalSortOption sortOption;
    [SerializeField] private UISprite icon;
    [SerializeField] private BoxCollider buttonCollider;
    
    public int height
    {
        get
        {
            return 50;
        }
    }

    public void Init(UIButtonExpansionSort root)
    {
        rootButton = root;
    }

    public void Sort()
    {
        rootButton.ChangeSortIcon(icon);
        rootButton.Sort(sortOption);
    }

    public void SetButtonActive(bool trigger)
    {
        buttonCollider.enabled = trigger;
    }
}
