using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyExit : DecoBase
{
    const string SPRITE_NAME = "Lock";
    const BlockType exitType = BlockType.KEY;

    public override void SetSprite()
    {
        uiSprite.spriteName = SPRITE_NAME;
        uiSprite.depth = (int)GimmickDepth.DECO_LAND;
        MakePixelPerfect(uiSprite);
        uiSprite.transform.localScale = Vector3.one * 1.25f;

        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            transform.localEulerAngles = new Vector3(0, 0, 180);
        }
    }

    public override bool GetTargetBoard(BlockType blockType)
    {
        if (exitType == blockType) return true;

        return false;
    }

}
