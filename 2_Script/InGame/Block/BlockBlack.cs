using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBlack : BlockBase
{
    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanMakeBombFieldEffect()
    {
        //얼음이 설치되어 있는 상황이면 폭탄 영역 표시 가능
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;

        //연쇄폭탄이 매치된 상황이면 폭탄 영역 표시 가능
        if (normalPangDelay == true)
            return true;
        else
            return false;
    }

    public override bool IsCanPangByPowerBomb()
    {
        return true;
    }

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanLink()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            return false;

        return true;
    }

    public override bool IsBombBlock()
    {
        return false;
    }

    public override bool IsSelectable()
    {
        if (normalPangDelay)
            return false;

        return activeBlack;
    }

    public override bool IsSpecialMatchable()
    {
        return true;
    }

    public override bool IsPangStopMove() //이것이 터질때는 블럭의 움직임이 멈춤
    {
        return true;
    }

    public override bool IsCanMakeCarpet()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockPang())
            return false;

        return true;
    }

    public override void MakeBombFieldEffect(float? dTime = null)
    {
        Board getBoardB = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoardB.HasDecoCoverBlock(false, BlockBomb._bombUniqueIndex)) 
            return;

        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BlockBomb.BombShape.Cross, indexX, indexY, 3, 3);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.BLACK_BOMB, _infoList : infoList, dTime: dTime);
    }

    public override void UpdateBlock()
    {
        MoveBlock();

        if(blockDeco != null && blockDeco.IsInterruptBlockEvent() == true)
        {

        }
        else if ((state == BlockState.WAIT || state == BlockState.PANG)
            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.state == BlockState.WAIT
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 1, 0)
            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.type == BlockType.BLOCK_BLACK
            && (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.blockDeco == null || PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.blockDeco.IsInterruptBlockEvent() == false)
            )
        {
            activeBlack = true;
        }
        else if ((state == BlockState.WAIT || state == BlockState.PANG)
            && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null
            && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null
            && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.state == BlockState.WAIT
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, -1, 0)
            && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.type == BlockType.BLOCK_BLACK
            && (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.blockDeco == null || PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.blockDeco.IsInterruptBlockEvent() == false)
            )
        {
            activeBlack = true;
        }
        else if ((state == BlockState.WAIT || state == BlockState.PANG)
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.state == BlockState.WAIT
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, 1)
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.type == BlockType.BLOCK_BLACK
            && (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.blockDeco == null || PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.blockDeco.IsInterruptBlockEvent() == false))
        {
            activeBlack = true;
        }
        else if ((state == BlockState.WAIT || state == BlockState.PANG)
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.state == BlockState.WAIT
            && ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, 0, -1)
            && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.type == BlockType.BLOCK_BLACK
            && (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.blockDeco == null || PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.blockDeco.IsInterruptBlockEvent() == false))
        {
            activeBlack = true;
        }
        else
        {
            activeBlack = false;

            RemoveLinkerNoReset();
        }

        if (startPang) { }
        
        else if (activeBlack || normalPangDelay || PangBlack)
        {
            int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 5f) % 2);
            mainSprite.spriteName = "blockLaboratoryAnimal1_" + spriteNumber;
        }
        else
        {
            if (state != BlockState.PANG)
            {
                mainSprite.spriteName = "blockLaboratoryAnimal2";
            }
        }
        checkDelay();
    }

    bool PangBlack = false;

    void checkDelay()
    {
        if (normalPangDelay == false)
            return;

        normaPangDelayTimer -= Global.deltaTimePuzzle;
        if (normaPangDelayTimer <= 0)
        {
            normalPangDelay = false;
            normaPangDelayTimer = 0f;
            state = BlockState.PANG;
            PangBlack = true;
            BlackPang();
        }
    }

    IEnumerator CoPangEffect()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            _transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }
        yield return null;
    }

    IEnumerator CoWaitEffect()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            _transform.localScale = Vector3.one * ManagerBlock.instance._curveBlockPopUp.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }

        _transform.localScale = Vector3.one;
        yield return null;
    }



    public bool activeBlack = false;

    public override void UpdateSpriteByBlockType()
    {
        if (activeBlack)
            mainSprite.spriteName = "blockLaboratoryAnimal1_0";
        else
        {
            //int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 5f) % 2);
            mainSprite.spriteName = "blockLaboratoryAnimal2";// + spriteNumber;
        }

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    /*
    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false)
    {
        if (mBombEffect) return;
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        if (!activeBlack)return;
        
        pangIndex = uniqueIndex;
        Pang(splashColorType, true);
    }
    */

    bool startPang = false;


    void BlackPang()
    {
        if (startPang)
        {
            return;
        }
        startPang = true;

        if (!StopMovingBlock)
        {
            normalPangDelay = false;
            StopMovingBlock = false;
            PangBlack = false;
            startPang = false;

            state = BlockState.WAIT;
            return;
        }

        Board getBoardB = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoardB.HasDecoCoverBlock(false, BlockBomb._bombUniqueIndex))
        {
            getBoardB.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex);

            if (StopMovingBlock)
            {
                StopMovingBlockCount--;
                StopMovingBlock = false;
                if (StopMovingBlockCount == 0)
                {
                    ManagerBlock.instance.blockMove = true;
                    ManagerBlock.instance.creatBlock = true;
                }
            }

            pangIndex = BlockBomb._bombUniqueIndex;
            isUseMakeBomb = false;
            state = BlockState.WAIT;
            isCheckedMatchable = false;
            BlockMatchManager.instance.CheckBlockLinkToItem(this);
            normalPangDelay = false;
            StopMovingBlock = false;
            PangBlack = false;
            startPang = false;
            StartCoroutine(CoWaitEffect());
            return;
        }

        StartCoroutine(CoPangEffect());

        RemoveLinkerNoReset();

        if (StopMovingBlock)
        {
            StopMovingBlockCount--;
            StopMovingBlock = false;
            if (StopMovingBlockCount == 0)
            {
                ManagerBlock.instance.blockMove = true;
                ManagerBlock.instance.creatBlock = true;
            }
        }

        if (activeBlack || PangBlack)
        {
            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
            {
                getBoard.BoardPang(pangIndex);
            }
            // _pangRemoveDelay = 0.3f;

            IsSkipPang = true;
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlay(AudioInGame.BLOCKBLOCK_PANG);
            InGameEffectMaker.instance.MakeBlackEffect(_transform.position);
        }
        else
        {
            IsSkipPang = true;
            StartCoroutine(CoPang());
            state = BlockState.PANG;
            InGameEffectMaker.instance.MakeBlockPangEffect(_transform.position);
        }

    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {  
        if (!PangByBomb)
        {
            return;
        }        

        if (startPang)
        {
            return;
        }
        startPang = true;

        Board getBoardA = PosHelper.GetBoardSreeen(indexX, indexY);
        getBoardA.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex, pangColorType);

        normalPangDelay = false;
        StopMovingBlock = false;
        PangBlack = false;
        startPang = false;

        if (targetPos != Vector3.zero && Vector3.Distance(_transform.localPosition, targetPos) > 15f)
        {
            state = BlockState.MOVE;
        }
        else
        {
            state = BlockState.WAIT;
        }

        return;
    }

    public override void PangByPowerBomb(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (startPang)
            return;

        state = BlockState.WAIT;
        BlockMatchManager.instance.CheckMatchPangBlock(this, false);
    }

    IEnumerator CoPang()
    {
        mainSprite.spriteName = "blockLaboratoryAnimal2_1";

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        ManagerSound.AudioPlay(AudioInGame.BLACK_PANG);
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 2f;

            if (timer > 0.25f)
                mainSprite.color = new Color(1f, 1f, 1f, 1.25f - timer);// (Mathf.Cos(timer * Mathf.PI) + 1)*0.5f);

            //mainSprite.transform.localEulerAngles = new Vector3(0, 0, timer * 540);
            mainSprite.transform.localPosition = new Vector3(0, timer *100f, 0);
            mainSprite.transform.localScale = Vector3.one * (1 + 0.5f * timer);

            if (timer > 0.5f && _listBlock.Contains(this))
            {
                _listBlock.Remove(this);
                _listBlockTemp.Remove(this);
                back.Block = null;
            }

            yield return null;
        }

        //화면에 보일수 있는 검댕이 수에 미리 생성돼있는 검댕이가 포함돼있는 경우면 모든 검댕이가 삭제될 때 화면수 감소.
        //포함되어 있지 않다면 출발에서 나온 검댕이들만 화면 전체 수에서 감소 시켜줌.
        if (ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 0 || (ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 1 && isMakeByStart == true))
            DestroyBlockData();
        yield return null;
        
        BlockMaker.instance.RemoveBlock(this);

        yield return null;
    }

    IEnumerator CoPangFinal()
    {
        //이펙트
        mainSprite.spriteName = "blockLaboratoryAnimal1_0";

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        //폭발효과
        BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.BLACK_BOMB, BlockColorType.NONE, false, false, true, pangIndex);

        //화면에 보일수 있는 검댕이 수에 미리 생성돼있는 검댕이가 포함돼있는 경우면 모든 검댕이가 삭제될 때 화면수 감소.
        //포함되어 있지 않다면 출발에서 나온 검댕이들만 화면 전체 수에서 감소 시켜줌.
        if (ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 0 || (ManagerBlock.instance.stageInfo.isBlockBlackAdjust == 1 && isMakeByStart == true))
            DestroyBlockData();

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        back.Block = null;


        timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 1f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        BlockMaker.instance.RemoveBlock(this);
        yield return null;
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
}
