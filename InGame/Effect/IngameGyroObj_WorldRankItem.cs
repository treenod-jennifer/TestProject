using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameGyroObj_WorldRankItem : IngameGyroObj
{
    protected override void Init()
    {
        base.Init();
        /*if (ManagerWorldRanking.instance != null)
        {
            NGUIAtlas worldRankingAtlas = ManagerWorldRanking.resourceData.worldRankingPack.IngameAtlas;
            mainSprite.atlas = worldRankingAtlas;
            shadowSprite.atlas = worldRankingAtlas;
        }*/
    }
}