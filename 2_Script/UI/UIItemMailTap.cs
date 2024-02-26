using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemMailTap : MonoBehaviour
{
    [Header("ObjectLink")]
    [SerializeField] private UISprite sprTapBg;
    [SerializeField] private Transform tapItemRoot;
    [SerializeField] private UISprite sprTapIcon;
    [SerializeField] private UILabel labelItem;
    [SerializeField] private UIItemLabelActiveColor labelActiveColor;

    [Header("MailCountLink")]
    [SerializeField] private GameObject objMailCountBg;
    [SerializeField] private UILabel    labelMailCount;

    public void ActiveButton(bool Active = true)
    {
        sprTapBg.spriteName = Active ? "diary_tap_01" : "diary_tap_02";
        
        DOTween.To(() => sprTapBg.height, x => sprTapBg.height = x, Active ? 78 : 73, 0.1f);
        tapItemRoot.DOScale(Active ? Vector3.one : Vector3.one * 0.85f, 0.1f);

        sprTapIcon.color = Active ? Color.white : new Color(180f / 255f, 180f / 255f, 180f / 255f);
        labelItem.color = labelActiveColor.SetActiveColor(Active, UIItemLabelActiveColor.ColorType.NORMAL);
        labelItem.effectColor = labelActiveColor.SetActiveColor(Active, UIItemLabelActiveColor.ColorType.SHADOW);

    }

}
