using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemSendScoreUpItemProfile : UIItemProfileSmall
{
    [SerializeField] private GameObject check;

    public bool IsCheck
    {
        set
        {
            check.SetActive(value);
        }
    }
}
