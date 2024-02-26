using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemTurnRelay_ContinueItem : MonoBehaviour
{
    [SerializeField] private UILabel labelRemainCount;
    [SerializeField] private UISprite spriteRemainItem;
    [SerializeField] private UISprite spriteBG;
    [SerializeField] private ManagerTurnRelay.BONUSITEM_TYPE type = ManagerTurnRelay.BONUSITEM_TYPE.LINE_BOMB;

    #region UI컬러 관련
    private Color activeFontColor = new Color(1f, 141f / 255f, 48f / 255f);
    private Color activeEffectColor = new Color(192f / 255f, 56f / 255f, 3f / 255f);
    private Color inActiveFontColorCode = new Color(141f / 255f, 141f / 255f, 141f / 255f);
    private Color inActiveEffectColorCode = new Color(55f / 255f, 55f / 255f, 141f / 255f);
    private Color inActiveSpriteColor = new Color(100f / 255f, 100f / 255f, 100f / 255f, 180f / 255f);
    #endregion

    public void InitItem()
    {
        int remainCount = ManagerTurnRelay.turnRelayIngame.GetData_DicIngameItemCount(type);

        labelRemainCount.text = remainCount.ToString();
        if (remainCount > 0)
        {
            spriteBG.gameObject.SetActive(true);
            spriteRemainItem.color = Color.white;
            labelRemainCount.color = activeFontColor;
            labelRemainCount.effectColor = activeEffectColor;
        }
        else
        {
            spriteBG.gameObject.SetActive(false);
            spriteRemainItem.color = inActiveSpriteColor;
            labelRemainCount.color = inActiveFontColorCode;
            labelRemainCount.effectColor = inActiveEffectColorCode;
        }
    }
}
