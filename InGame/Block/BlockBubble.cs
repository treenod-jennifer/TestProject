using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBubble : BlockBase
{
    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsDigTarget()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType() //스프라이트 이름정해주기
    {
        mainSprite.spriteName = "bubble";

        SetMainSpriteDepth();
        mainSprite.MakePixelPerfect();
    }

    float shineTimer = -1f;

    public override void UpdateBlock()
    {
        MoveBlock();

        if (state == BlockState.WAIT)
        {
            Board board = PosHelper.GetBoardSreeen(indexX, indexY);
            if (board != null && board.HasDecoCoverBlock() == false && board.IsGetTargetBoard(type))
            {
                state = BlockState.PANG;
                Pang();
            }
        }
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        ManagerBlock.instance.AddScore(1000);
        InGameEffectMaker.instance.MakeScore(_transform.position, 1000);
        InGameEffectMaker.instance.MakeGetKeyEffect(_transform.position);

        //터지는 애니메이션
        //반칸내려가서 
        ManagerSound.AudioPlay(AudioInGame.KEY);
        //목표로 날아가는 것 만들기
        FlyTarget.flyTargetCount++;
        //InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.BUBBLE);
        DestroyBlockData();

        ManagerBlock.instance.creatBlockTypeTurnCount[(int)BlockType.BUBBLE] = GameManager.instance.touchCount;

        PangDestroyBoardData();
    }
}

