using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemDiaryDecoTap : MonoBehaviour
{
    [SerializeField] private UIDiaryDeco.DecoTapType tapType;

    public static event System.Action   TapClickAction;

    static event System.Action BtnActiveEvent;

    [Header("ObjectLink")]
    [SerializeField] private UISprite   sprTapBg;
    [SerializeField] private UILabel    labelDecoTap;


    public void ButtonEvent(bool IsActive = true)
    {
        sprTapBg.spriteName = $"diary_button_{(IsActive ? "green" : "gray")}";
        labelDecoTap.effectColor = IsActive ? new Color32(68, 144, 41, 255) : new Color32(92, 92, 92, 255);
    }
}
