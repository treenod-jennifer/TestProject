using System.Collections.Generic;
using UnityEngine;

public class TutorialLobbyRenewal : TutorialBase
{
    public void SetBlindPanel()
    {
        blind._panel.depth = 30;
        blind.SetDepth(-1);
        float iconCount = Mathf.Min(ManagerUI._instance.ScrollbarRight.icons.Count, UIItemScrollbar.EVENT_VIEW_COUNT);
        float sizeY = iconCount * ManagerUI._instance.ScrollbarRight.grid.cellHeight + 100;
        blind.SetSize(200, (int)sizeY);
        blind.SetSizeCollider(0, 0);
        blind._panel.transform.position = ManagerUI._instance.ScrollbarRight.transform.position;
        Vector2 blindPos = blind._panel.transform.localPosition;
        blind._panel.transform.localPosition = new Vector2(blindPos.x, blindPos.y + ((UIItemScrollbar.SCREEN_CENTER_COUNT - iconCount) * ManagerUI._instance.ScrollbarRight.grid.cellHeight) / 2 + 10);
        blind._textureCenter.type = UIBasicSprite.Type.Sliced;
    }
}
