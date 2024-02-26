using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureAnimalSlotMark : MonoBehaviour {
    [SerializeField] private UIItemAdventureAnimalInfo animalInfo;
    [SerializeField] private GameObject[] slotSprites;
    [SerializeField] private GameObject selectFrame;
    [SerializeField] private GameObject nonSelectFrame;

    private bool SelectFrame
    {
        set
        {
            if(selectFrame != null && nonSelectFrame != null)
            {
                selectFrame.SetActive(value);
                nonSelectFrame.SetActive(!value);
            }
        }
    }

    private void Awake()
    {
        animalInfo.setAnimalCallBack += (aData) =>
        {
            if (aData == null)
            {
                Off();
                return;
            }
            
            for (int i = 0; i < 3; i++)
            {
                if (aData.idx == ManagerAdventure.User.GetAnimalIdxFromDeck(1, i))
                {
                    On(i);
                    return;
                }
            }

            Off();
        };
    }

    public void On(int slotIndex)
    {
        SlotMark = slotIndex;
        SelectFrame = true;
    }

    public void Off()
    {
        SlotMark = -1;
        SelectFrame = false;
    }

    private int SlotMark
    {
        set
        {
            for(int i=0; i< slotSprites.Length; i++)
            {
                slotSprites[i].SetActive(i == value);
            }
        }
    }
}
