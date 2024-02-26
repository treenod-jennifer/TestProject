using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemInviteTap : MonoBehaviour
{
    [SerializeField]
    private UIPopupInvite.TypePopupInvite tapType;
    [SerializeField]
    private UISprite spriteTapBG;
    [SerializeField]
    private UIItemLabelActiveColor spriteTapName;

    private Vector3 orginTr = Vector3.zero;
    private float textInActiveColor = 180f / 255f;

    public void InitTap()
    {
        orginTr = spriteTapBG.transform.localPosition;
    }

    public void OnTab()
    {
        spriteTapBG.transform.localPosition = orginTr;
        spriteTapBG.spriteName ="diary_tap_01";
        spriteTapBG.transform.DOScale(Vector3.one, 0.1f);
        spriteTapName.transform.DOScale(Vector3.one * 1.15f, 0.1f);
        spriteTapName.SetLabel = spriteTapName.SetActiveColor(true, UIItemLabelActiveColor.ColorType.NORMAL);
        spriteTapName.SetShadowLabel = spriteTapName.SetActiveColor(true, UIItemLabelActiveColor.ColorType.SHADOW);
    }

    public void OffTab()
    {
        spriteTapBG.transform.localPosition = orginTr - new Vector3(0f, -2f, 0f);
        spriteTapBG.spriteName = "diary_tap_02";
        spriteTapBG.transform.DOScale(Vector3.one * 0.95f, 0.1f);
        spriteTapName.transform.DOScale(Vector3.one * 1.0f, 0.1f);
        spriteTapName.SetLabel = spriteTapName.SetActiveColor(false, UIItemLabelActiveColor.ColorType.NORMAL);
        spriteTapName.SetShadowLabel = spriteTapName.SetActiveColor(false, UIItemLabelActiveColor.ColorType.SHADOW);
    }

    public bool SameFilter(UIPopupInvite.TypePopupInvite type)
    {
        return this.tapType == type;
    }
}
