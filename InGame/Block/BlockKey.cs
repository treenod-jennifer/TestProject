using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockKey : BlockBase
{
    public bool isCanShining = true;

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsDigTarget()
    {
        return true;
    }
    
    public override bool IsCanMakeBombFieldEffect()
    {
        //얼음이 설치되어 있는 상황이면 폭탄 영역 표시 가능
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        else
            return false;
    }

    public override void SetRandomBoxHide(bool isHide)
    {
        base.SetRandomBoxHide(isHide);

        isCanShining = isHide;
    }
    
    public override void SetCloverHide(bool isHide)
    {
        isCanShining = isHide;
    }

    public override void UpdateSpriteByBlockType() //스프라이트 이름정해주기
    {
        mainSprite.spriteName = "blockKey";

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    float shineTimer = -1f;

    public override void UpdateBlock()
    {
        MoveBlock();

        if(state == BlockState.WAIT)
        {
            //열쇠 출구쪽에 방해하는 기믹들이 있는지 확인.
            Board board = PosHelper.GetBoardSreeen(indexX, indexY);
            if(board != null && board.HasDecoCoverBlock() == false && board.IsGetTargetBoard(type) 
                && (blockDeco == null || blockDeco.IsInterruptBlockSelect() == false) 
                && board.IsNotDisturbBlock(BlockDirection.DOWN) == true)
            {
                state = BlockState.PANG;
                Pang();
            }
            else if(isCanShining == true && PosHelper.InExistScreen(indexX, indexY))
            {
                if (shineTimer == -1f)
                {
                    shineTimer = (float)(Random.Range(4, 8)) * 0.5f;
                }
                else if (0 >= shineTimer)
                {
                    InGameEffectMaker.instance.MakeKeyShineEffect(_transform);
                    shineTimer = (float)(Random.Range(5, 10)) * 0.5f;
                }
                else
                {
                    shineTimer -= Global.deltaTimePuzzle;
                }
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
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.KEY)
            {
                InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.KEY);
                break;
            }
        }

        DestroyBlockData();

        ManagerBlock.instance.creatBlockTypeTurnCount[(int)BlockType.GROUND_KEY] = GameManager.instance.touchCount;
        ManagerBlock.instance.creatBlockTypeTurnCount[(int)BlockType.KEY] = GameManager.instance.touchCount;

        PangDestroyBoardData();
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return false;
        else
            return true;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }

    public override bool EventAction()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);

        if (board.HasDecoHideBlock() || board.HasDecoCoverBlock())
            SetCloverHide(false);
        else
            SetCloverHide(true);

        return false;
    }
}
