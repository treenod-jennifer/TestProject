using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipExit : DecoBase
{
    const BlockType exitType = BlockType.SPACESHIP;

    public override void SetSprite()
    {
        uiSprite.spriteName = "SpaceShipExit";
        uiSprite.depth = (int)GimmickDepth.DECO_LAND;
        uiSprite.MakePixelPerfect();

        uiSprite.width = (int)(uiSprite.width * 1.5f);
        uiSprite.height = (int)(uiSprite.height * 1.4f);
    }

    public override bool GetTargetBoard(BlockType blockType)
    {
        if (exitType == blockType)
        {
            return true;
        }

        return false;
    }

    public override void Init()
    {
        ManagerBlock.instance.AddSpaceShipExit(this);

        base.Init();
    }
}
