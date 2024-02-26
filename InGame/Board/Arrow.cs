using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : DecoBase
{
    public BlockDirection direction = BlockDirection.NONE;


    public override void Init()
    {
        uiSprite.depth = (int)GimmickDepth.UI_LABEL;
    }
}
