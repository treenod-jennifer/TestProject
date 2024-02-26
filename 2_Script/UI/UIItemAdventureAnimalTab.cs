using UnityEngine;

public class UIItemAdventureAnimalTab : MonoBehaviour
{
    [SerializeField]
    private ManagerAdventure.AnimalFilter filter;
    [SerializeField]
    private Animation aniController;
    [SerializeField]
    private UIItemLabelActiveColor spriteTapName;

    public void OnTab()
    {
        aniController.Play("tab_start");
        aniController.PlayQueued("tab_loop");

        spriteTapName.SetLabel = spriteTapName.SetActiveColor(true, UIItemLabelActiveColor.ColorType.NORMAL);
        spriteTapName.SetShadowLabel = spriteTapName.SetActiveColor(true, UIItemLabelActiveColor.ColorType.SHADOW);
    }

    public void OffTab()
    {
        aniController.CrossFade("tab_end", 0.1f);

        spriteTapName.SetLabel = spriteTapName.SetActiveColor(false, UIItemLabelActiveColor.ColorType.NORMAL);
        spriteTapName.SetShadowLabel = spriteTapName.SetActiveColor(false, UIItemLabelActiveColor.ColorType.SHADOW);
    }

    public bool SameFilter(ManagerAdventure.AnimalFilter filter)
    {
        return this.filter == filter;
    }

    public void SetFilter(ManagerAdventure.AnimalFilter filter)
    {
        this.filter = filter;
    }
}

[System.Serializable]
public class AnimalTabList
{
    [SerializeField]
    private UIItemAdventureAnimalTab[] tabs;

    public void OnTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (i == index)
                tabs[i].OnTab();
            else
                tabs[i].OffTab();
        }
    }

    public void OnTab(ManagerAdventure.AnimalFilter filter)
    {
        foreach (var tab in tabs)
        {
            if (tab.SameFilter(filter))
                tab.OnTab();
            else
                tab.OffTab();
        }
    }
}
