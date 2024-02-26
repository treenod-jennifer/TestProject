using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockIceApple : BlockBase
{
    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "blockIceApple";

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool bombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (blockDeco != null)
        {
            if (bombEffect) return;

            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }
        
        if (pangIndex == uniqueIndex) return;
        if (state == BlockState.PANG) return;

        pangIndex = uniqueIndex;


        state = BlockState.PANG;
        _pangRemoveDelay = 0.3f;
        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        InGameEffectMaker.instance.MakeBlockPangEffect(_transform.position);
        //터지는 애니메이션
        //반칸내려가서 

        //턴으로 날아가는 효과
        ManagerSound.AudioPlayMany(AudioInGame.APPLE);
        //InGameEffectMaker.instance.MakeFlyApple(_transform.position, 1, 0.05f * i);
        InGameEffectMaker.instance.MakeFlyIceApple(_transform.position);
        ManagerBlock.instance.GetIceApple();
        DestroyBlockData();
        ManagerBlock.instance.creatBlockTypeTurnCount[(int)BlockType.GROUND_ICE_APPLE] = GameManager.instance.touchCount;
        //PangDestroyBoardData();
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_3;
    }
}
