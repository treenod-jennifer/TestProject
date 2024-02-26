using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupTimeOver : UIPopupBase
{
    public UITexture textureIcon;

    public Texture2D coinstageIcon;

    public void InitPopup()
    {
        if (Global.GameType == GameType.COIN_BONUS_STAGE)
        {
            SetCoinStageTimeOverUI();
        }
        return;
    }

    private void SetCoinStageTimeOverUI()
    {
        textureIcon.mainTexture = coinstageIcon;
        textureIcon.width = 186;
        textureIcon.height = 183;
    }

    public override void OnClickBtnBack()
    {
        return;
    }
}
