using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDeco : DecoBase
{
    const string SPRITE_NAME = "mapdeco";

    public override void Init()
    {
        uiSprite.spriteName = SPRITE_NAME + lifeCount;
        //배경 위치에 데코보다 위 거미홀, 열쇠홀 보다 밑인 기믹이 나온다면 추후 수정
        uiSprite.depth = (int)GimmickDepth.DECO_AREA;
        uiSprite.MakePixelPerfect();
    }
}
