using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockstart : DecoBase {

    public int _backStartPositionBlockType = int.MaxValue;

    public override bool IsMakerBlock()
    {
        return true;
    }

    public override bool IsCanPang_ByHeart()
    {
        return false;
    }

    public override void Init()
    {
        MakePixelPerfect(uiSprite);
        uiSprite.depth = (int)GimmickDepth.DECO_OBJECT;
        uiSprite.cachedTransform.localPosition = new Vector3(0, 45, 0);

        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            transform.localEulerAngles = new Vector3(0, 0, 180);
        }
    }
}
