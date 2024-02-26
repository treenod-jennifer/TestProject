using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupAdventureAnimalOverlapConfirm : UIPopupBase
{
    public static UIPopupAdventureAnimalOverlapConfirm _instance = null;

    [Header("Linked Object")]
    [SerializeField] private UIItemAdventureAnimalInfo currentAnimalInfo;
    [SerializeField] private UIItemAdventureAnimalInfo afterAnimalInfo;
    [SerializeField] private UISprite overlapItem;
    [SerializeField] private UILabel afterLifeValue;
    [SerializeField] private UILabel afterAtkValue;
    [SerializeField] private GameObject overlapButton;
    [SerializeField] private GameObject afterLife_Arrow_Up;
    [SerializeField] private GameObject afterLife_Arrow_Down;
    [SerializeField] private GameObject afterAtk_Arrow_Up;
    [SerializeField] private GameObject afterAtk_Arrow_Down;

    [Header("Color")]
    [SerializeField] private Color upColor;
    [SerializeField] private Color offColor;
    [SerializeField] private Color downColor;

    private ManagerAdventure.AnimalInstance aCurrentAnimalData;
    private ManagerAdventure.AnimalInstance aAfterAnimalData;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private enum ColorType
    {
        Off,
        Up,
        Down
    }

    private ColorType LifeColor
    {
        set
        {
            switch (value)
            {
                case ColorType.Off:
                    afterLife_Arrow_Up.SetActive(false);
                    afterLife_Arrow_Down.SetActive(false);
                    afterLifeValue.color = offColor;
                    break;
                case ColorType.Up:
                    afterLife_Arrow_Up.SetActive(true);
                    afterLife_Arrow_Down.SetActive(false);
                    afterLifeValue.color = upColor;
                    break;
                case ColorType.Down:
                    afterLife_Arrow_Up.SetActive(false);
                    afterLife_Arrow_Down.SetActive(true);
                    afterLifeValue.color = downColor;
                    break;
                default:
                    break;
            }
        }
    }

    private ColorType AtkColor
    {
        set
        {
            switch (value)
            {
                case ColorType.Off:
                    afterAtk_Arrow_Up.SetActive(false);
                    afterAtk_Arrow_Down.SetActive(false);
                    afterAtkValue.color = offColor;
                    break;
                case ColorType.Up:
                    afterAtk_Arrow_Up.SetActive(true);
                    afterAtk_Arrow_Down.SetActive(false);
                    afterAtkValue.color = upColor;
                    break;
                case ColorType.Down:
                    afterAtk_Arrow_Up.SetActive(false);
                    afterAtk_Arrow_Down.SetActive(true);
                    afterAtkValue.color = downColor;
                    break;
                default:
                    break;
            }
        }
    }

    public void InitAnimalInfo(ManagerAdventure.AnimalInstance aCurrentAnimal, ManagerAdventure.AnimalInstance aAfterAnimal = null, int overlap = 0)
    {
        aCurrentAnimalData = aCurrentAnimal;
        aAfterAnimalData = aAfterAnimal;

        if (aAfterAnimal != null)
        {
            //체인지 기능
            aAfterAnimalData = aAfterAnimal;

            this.currentAnimalInfo.SetAnimalSelect(aCurrentAnimal);
            this.afterAnimalInfo.SetAnimalSelect(aAfterAnimalData);

            overlapButton.SetActive(false);
        }
        else
        {
            //중첩 기능
            if(aCurrentAnimal.overlap == ManagerAdventure.ManagerAnimalInfo.GetMaxOverlap(aCurrentAnimal.idx) - 1)
                aAfterAnimalData = ManagerAdventure.User.GetAnimalInstance(GetIfAnimalData(aCurrentAnimal, 1, 1));
            else
                aAfterAnimalData = ManagerAdventure.User.GetAnimalInstance(GetIfAnimalData(aCurrentAnimal, 1));

            this.currentAnimalInfo.SetAnimalSelect(aCurrentAnimal);
            this.afterAnimalInfo.SetAnimalSelect(aAfterAnimalData);

            overlapItem.spriteName = $"item_overlap_{overlap}";

        }

        if (aCurrentAnimal.atk == aAfterAnimalData.atk)
        {
            AtkColor = ColorType.Off;
        }
        else if (aCurrentAnimal.atk < aAfterAnimalData.atk)
        {
            AtkColor = ColorType.Up;
            afterAtkValue.text = $"{aAfterAnimalData.atk} (+{aAfterAnimalData.atk - aCurrentAnimal.atk})";
        }
        else
        {
            AtkColor = ColorType.Down;
            afterAtkValue.text = $"{aAfterAnimalData.atk} ({aAfterAnimalData.atk - aCurrentAnimal.atk})";
        }

        if (aCurrentAnimal.hp == aAfterAnimalData.hp)
        {
            LifeColor = ColorType.Off;
        }
        else if (aCurrentAnimal.hp < aAfterAnimalData.hp)
        {
            LifeColor = ColorType.Up;
            afterLifeValue.text = $"{aAfterAnimalData.hp} (+{aAfterAnimalData.hp - aCurrentAnimal.hp})";
        }
        else
        {
            LifeColor = ColorType.Down;
            afterLifeValue.text = $"{aAfterAnimalData.hp} ({aAfterAnimalData.hp - aCurrentAnimal.hp})";
        }


        //중첩 버튼 기능 
        //overlapButton.InitButton(aData);
    }

    private ManagerAdventure.UserDataAnimal GetIfAnimalData(ManagerAdventure.AnimalInstance aData, int overlapValue, int LookId = 0)
    {
        ManagerAdventure.UserDataAnimal ret = new ManagerAdventure.UserDataAnimal
        {
            gettime = aData.gettime,
            animalIdx = aData.idx,
            grade = aData.grade,
            level = aData.level,
            exp = aData.exp,
            overlap = aData.overlap + overlapValue,
            lookId = LookId
        };

        return ret;
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        UIPanel[] panels = GetComponentsInChildren<UIPanel>();
        for (int i = 1; i < panels.Length; i++)
        {
            panels[i].depth = depth + i;
        }
    }

    public void OnClickOverlap()
    {
        if (bCanTouch == false)
            return;

        var instance = UIPopupStageAdventureAnimal._instance as UIPopUpAdventureAnimalSelectOverlap;
        instance.selectedAnimal = this.aCurrentAnimalData.idx;
        ManagerUI._instance.ClosePopUpUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_instance == this)
            _instance = null;
    }
}
