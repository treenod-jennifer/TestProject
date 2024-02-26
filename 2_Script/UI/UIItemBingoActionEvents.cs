using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemBingoActionEvents : MonoBehaviour
{
    [SerializeField] private UIItemBingoEventBoard itemBingoBoard;
    
    void selectEffect()
    {
        itemBingoBoard.selectEffect();
    }

    void changeSlot()
    {
        itemBingoBoard.changeSlot();
    }

    void clearEffect()
    {
        StartCoroutine(itemBingoBoard.clearEffect());
    }
    
    void changeClearSlot()
    {
        StartCoroutine(itemBingoBoard.changeClearSlot());
    }
}
