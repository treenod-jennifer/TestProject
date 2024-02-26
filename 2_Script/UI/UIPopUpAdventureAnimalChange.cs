using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpAdventureAnimalChange : UIPopupBase {
    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo target;
    [SerializeField] private UIItemAdventureAnimalInfo changeTarget;
    [SerializeField] private UISprite changeIcon;
    [SerializeField] private UILabel changeLife;
    [SerializeField] private UILabel changeAtk;
    [SerializeField] private GameObject changeLife_Arrow_Up;
    [SerializeField] private GameObject changeLife_Arrow_Down;
    [SerializeField] private GameObject changeAtk_Arrow_Up;
    [SerializeField] private GameObject changeAtk_Arrow_Down;

    [Header("Effect")]
    [SerializeField] private Color upColor;
    [SerializeField] private Color downColor;

    private ManagerAdventure.AnimalInstance cData;

    private enum ArrowType
    {
        Off,
        Up,
        Down
    }
    private ArrowType LifeArrow
    {
        set
        {
            switch (value)
            {
                case ArrowType.Off:
                    changeLife_Arrow_Up.SetActive(false);
                    changeLife_Arrow_Down.SetActive(false);
                    break;
                case ArrowType.Up:
                    changeLife_Arrow_Up.SetActive(true);
                    changeLife_Arrow_Down.SetActive(false);
                    changeLife.color = upColor;
                    break;
                case ArrowType.Down:
                    changeLife_Arrow_Up.SetActive(false);
                    changeLife_Arrow_Down.SetActive(true);
                    changeLife.color = downColor;
                    break;
                default:
                    break;
            }
        }
    }
    private ArrowType AtkArrow
    {
        set
        {
            switch (value)
            {
                case ArrowType.Off:
                    changeAtk_Arrow_Up.SetActive(false);
                    changeAtk_Arrow_Down.SetActive(false);
                    break;
                case ArrowType.Up:
                    changeAtk_Arrow_Up.SetActive(true);
                    changeAtk_Arrow_Down.SetActive(false);
                    changeAtk.color = upColor;
                    break;
                case ArrowType.Down:
                    changeAtk_Arrow_Up.SetActive(false);
                    changeAtk_Arrow_Down.SetActive(true);
                    changeAtk.color = downColor;
                    break;
                default:
                    break;
            }
        }
    }

    public void InitPopup(ManagerAdventure.AnimalInstance target, ManagerAdventure.AnimalInstance changeTarget)
    {
        cData = changeTarget;

        this.target.SetAnimalSelect(target);
        this.changeTarget.SetAnimalSelect(changeTarget);

        bool selectCheck = false;
        for (int i = 0; i < 3; i++)
        {
            if (changeTarget.idx == ManagerAdventure.User.GetAnimalIdxFromDeck(1, i))
            {
                selectCheck = true;
                break;
            }
        }
        changeIcon.spriteName = selectCheck ? "icon_shift" : "icon_arrow";

        if (target.atk == changeTarget.atk)
            AtkArrow = ArrowType.Off;
        else if (target.atk < changeTarget.atk)
            AtkArrow = ArrowType.Up;
        else
            AtkArrow = ArrowType.Down;

        if (target.hp == changeTarget.hp)
            LifeArrow = ArrowType.Off;
        else if (target.hp < changeTarget.hp)
            LifeArrow = ArrowType.Up;
        else
            LifeArrow = ArrowType.Down;
    }

	public void OnClickOK()
    {
        if (this.bCanTouch == false)
            return;
        bCanTouch = false;

        int changeTargetDeckSlot = -1;
        for(int i = 0; i < 3; ++i)
        {
            if (UIPopupStageAdventureReady._instance.selectSlot == i)
                continue;

            var deckAnimal = ManagerAdventure.User.GetAnimalIdxFromDeck(1, i);
            if (deckAnimal == cData.idx)
            {
                changeTargetDeckSlot = i;
                break;
            }
        }

        if (changeTargetDeckSlot != -1)
        {
            int orgAnimal = ManagerAdventure.User.GetAnimalIdxFromDeck(1, UIPopupStageAdventureReady._instance.selectSlot);

            ManagerAdventure.User.SetAnimalToDeck(1, UIPopupStageAdventureReady._instance.selectSlot, cData.idx);
            ManagerAdventure.User.SetAnimalToDeck(1, changeTargetDeckSlot, orgAnimal);

        }
        else
        {
            ManagerAdventure.User.SetAnimalToDeck(1, UIPopupStageAdventureReady._instance.selectSlot, cData.idx);
        }

        StartCoroutine(Save());

        
    }

    IEnumerator Save()
    {
        NetworkLoading.MakeNetworkLoading(0f);
        yield return ManagerAdventure.User.SaveDeck();

        NetworkLoading.EndNetworkLoading();
        bCanTouch = true;

        this._callbackClose = UIPopupStageAdventureAnimal._instance.CloseSelf;

        UIPopupStageAdventureReady._instance.RePaintAll();

        OnClickBtnClose();
        yield break;
    }
}
