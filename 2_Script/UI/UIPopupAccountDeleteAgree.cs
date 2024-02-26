using UnityEngine;

public class UIPopupAccountDeleteAgree : UIPopupAccountDeleteBase
{
    #region Readonly
    private static readonly Color32 COLOR_AGREE    = new Color32(34, 100, 14, 255);
    private static readonly Color32 COLOR_DISAGREE = new Color32(84, 84, 84, 255);
    #endregion

    #region Private
    [SerializeField] private BoxCollider boxColliderAgree;
    [SerializeField] private GameObject  objAgreeCheck;
    [SerializeField] private UISprite    sprAgree;
    [SerializeField] private UILabel[]   labelAgree;
    #endregion

    private void OnClickAgree()
    {
        objAgreeCheck.SetActive(!objAgreeCheck.activeSelf);
        sprAgree.spriteName       = objAgreeCheck.activeSelf ? "button_play" : "button_play03";
        labelAgree[0].effectColor = objAgreeCheck.activeSelf ? COLOR_AGREE : COLOR_DISAGREE;
        labelAgree[1].color       = objAgreeCheck.activeSelf ? COLOR_AGREE : COLOR_DISAGREE;
        labelAgree[1].effectColor = objAgreeCheck.activeSelf ? COLOR_AGREE : COLOR_DISAGREE;
        boxColliderAgree.enabled  = objAgreeCheck.activeSelf;
    }

    private void OnClickAccountDelete() => OnClickAccountDelete<UIPopupAccountDeleteCheck>();
}