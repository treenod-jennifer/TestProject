using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFireWork : BlockBase
{
    public override bool IsCanLink()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            return false;

        if (lifeCount == 0)
            return false;

        return true;
    }

    public override void MoveAction()
    {
        if (state != BlockState.WAIT)
            return;

        if (GameManager.instance.lineBombRotate)
        {
            if (expectType == BlockBombType.LINE_H && ManagerBlock.instance.LineType == BlockBombType.LINE_V)
            {
                expectType = ManagerBlock.instance.LineType;
            }
            else if (expectType == BlockBombType.LINE_V && ManagerBlock.instance.LineType == BlockBombType.LINE_H)
            {
                expectType = ManagerBlock.instance.LineType;
            }
        }
    }

    public override bool IsNormalBlock()
    {
        return true;
    }
    public override bool IsCanMakeCarpetByBomb()
    {
        return true;
    }

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanMakeCarpet()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockPang())
            return false;

        if (lifeCount > 1)
            return false;

        return true;
    }

    public override bool isCanPangByRainbow()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        RandomBox randomBox = null;
        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            randomBox = board.DecoOnBoard[i] as RandomBox;

            if (randomBox != null)
            {
                return false;
            }
        }

        return true;
    }


    public override bool IsCanAddCoin()
    {
        return true;
    }

    void Update()
    {
        checkDelay();
    }

    bool PangBlack = false;

    void checkDelay()
    {
        if (normalPangDelay == false) return;

        normaPangDelayTimer -= Global.deltaTimePuzzle;

        if (normaPangDelayTimer <= 0)
        {
            normalPangDelay = false;
            normaPangDelayTimer = 0f;
            state = BlockState.PANG;
            Pang();
        }
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "Fire" + lifeCount + "_" + GetColorTypeString();

        if (lifeCount < 0)
            mainSprite.spriteName = "Fire1_" + GetColorTypeString();

        MakePixelPerfect(mainSprite);
        mainSprite.depth = (int)GimmickDepth.BLOCK_AREA;
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (PangBlack) return;
        PangBlack = true;

        lifeCount--;
        UpdateSpriteByBlockType();

        if (lifeCount <= 0)
        {
            SetSplashEffect(colorType, PangByBomb);

            ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_REMOVE);
            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
                getBoard.BoardPang(pangIndex, pangColorType);

            isSkipDistroy = true;
            DestroyBlockData();
            StartCoroutine(CoPangFinal());

            RemoveLinkerNoReset();
        }
        else
        {
            ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_MATCH);
            //단계내려가는 사운드
            StartCoroutine(CoPang());
            normalPangDelay = false;
            normaPangDelayTimer = 0f;

            if (targetPos != Vector3.zero && Vector3.Distance(_transform.localPosition, targetPos) > 15f)
            {
                state = BlockState.MOVE;
            }
            else
            {
                state = BlockState.WAIT;
            }

            PangBlack = false;
        }
    }

    IEnumerator CoPang()
    {
        float timer = 0f;
        UpdateSpriteByBlockType();

        timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        mainSprite.transform.localScale = Vector3.one;
    }

    IEnumerator CoPangFinal()
    {
        float timer = 0f;
        float colorTimer = 0f;

        colorTimer = 0f;

        //카펫잇는지 확인
        bool HasCarpet = PosHelper.GetBoardSreeen(indexX, indexY).IsExistCarpetAndCanExpand();
        if (HasCarpet == true)
        {
            this.CoverBlockWithCarpet();
        }

        for (int i = 0; i < 3; i++)
        {
            Board targetBoard = ManagerBlock.instance.GetTargetBoardAtFireWork(HasCarpet);
            if (targetBoard == null)
                continue;

            BlockBase targetBlock = targetBoard.Block;
            
            //블럭이 있으면, 블럭기준으로 목표지점 설정(1x1크기를 넘는 기믹때문에), 없으면 보드 기준으로 설정
            Vector3 targetPos = Vector3.zero;
            
            
            if(targetBlock != null && targetBlock.size == BlockSizeType.BIG) // 1x1 보다 큰 사이즈의 블럭들 예외 처리 
            {
                if (targetBlock != null && targetBoard.HasDecoHideBlock() == false)
                    targetBlock.isRainbowBomb = true;
                
                if (targetBoard.HasDecoHideBlock())
                    targetBoard.isFireWorkBoard = true;
                
                if(targetBoard.Block != null && targetBoard.HasDecoHideBlock() == false)
                    targetPos = targetBoard.Block._transform.localPosition;
                else
                    targetPos = PosHelper.GetPosByIndex(targetBoard.indexX, targetBoard.indexY);
            }
            else
            {
                if (targetBlock != null)
                    targetBlock.isRainbowBomb = true;
                
                if(targetBoard.Block != null)
                    targetPos = targetBoard.Block._transform.localPosition;
                else
                    targetPos = PosHelper.GetPosByIndex(targetBoard.indexX, targetBoard.indexY);
            }

            if (targetBlock == null && targetBoard.BoardOnNet.Count > 0)
                targetBoard.isFireWorkBoard = true;

            for (int j = 0; j < targetBoard.BoardOnHide.Count; j++)
            {
                targetBoard.BoardOnHide[j].IsRainbowBomb = true;
                break;
            }
            
            InGameEffectMaker.instance.MakeFireWork(_transform.localPosition, targetPos, targetBlock, targetBoard, uniqueIndexCount, HasCarpet);

            timer = 0f;
            while (timer < 0.3f)
            {
                timer += Global.deltaTimePuzzle;

                colorTimer += Global.deltaTimePuzzle * 3 *1.5f;
                float colorRatio = Mathf.Abs(Mathf.Sin(Mathf.PI * 2 * colorTimer)) * 0.25f + 0.75f;
                mainSprite.color = new Color(colorRatio, colorRatio, colorRatio, 1f);

                float scaleRatio = Mathf.Sin(Mathf.PI * 2 * colorTimer);
                mainSprite.cachedTransform.localScale = Vector3.one * (1 + scaleRatio * 0.1f);
                yield return null;
            }
            yield return null;
        }

        timer = 0f;  
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 8f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
        yield return null;
    }

    public override void CoverBlockWithCarpet()
    {
        if (lifeCount > 1)
            return;

        Board tempBoard = PosHelper.GetBoard(indexX, indexY);
        if (tempBoard != null && tempBoard.HasDecoCoverBlock() == false 
                              && (blockDeco == null || blockDeco.IsInterruptBlockSelect() == false) 
                              && carpetSprite == null && mainSprite != null)
        {
            carpetSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockCarpetSpriteObj).GetComponent<BlockCarpetSprite>();
            carpetSprite.InitCarpetSprite((int)GimmickDepth.BLOCK_AREA + 1);
            hasCarpet = true;
        }
    }

    public override FireWorkRank GetBlockRankAtFireWork(Board board, bool hasCarpet = false)
    {
        if (IsBlockRankExclusionTypeAtFireWork() == true)
            return FireWorkRank.RANK_NONE;

        FireWorkRank blockRank = GetDefaultBlockRankAtFireWork();

        if (blockRank == FireWorkRank.RANK_1)
            return blockRank;

        #region 얼음검사
        if (IsCanCoverIce() == true && blockDeco != null)
        {
            //얼음이 목표인 경우 검사
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.ICE))
                return FireWorkRank.RANK_1;

            //현재 블록의 우선순위의 값이 얼음의 우선순위보다 큰 경우 얼음 우선순위를 반환해줌.
            if (blockRank > FireWorkRank.RANK_3)
                blockRank = FireWorkRank.RANK_3;
        }
        #endregion

        #region 열쇠, 우주선 검사
        //열쇠, 우주선 검사, 이미 기본 목표 순위가 2순위보다 높거나 같을 경우에는 검사하지 않음.
        if (IsBlockCheckKeyTypeAtFireWork() == true && blockRank > FireWorkRank.RANK_2)
        {
            if (IsCheckTypeExistUpBoardsAtFireWork(TARGET_TYPE.KEY, BlockType.KEY) == true)
                return FireWorkRank.RANK_2;

            if (IsCheckTypeExistUpBoardsAtFireWork(TARGET_TYPE.SPACESHIP, BlockType.SPACESHIP) == true)
                return FireWorkRank.RANK_2;
        }
        #endregion

        #region 3순위 검사
        //벌집의 경우, 우선순위가 석판, 카펫, 잔디위의 벌집보다 우선순위가 낮으면 우선순위 반환.
        if (blockRank <= FireWorkRank.RANK_3)
            return blockRank;
        
        //석판 우선순위 검사.
        if (board.BoardOnCrack.Count > 0)
            return FireWorkRank.RANK_3;

        //카펫 우선순위 검사.
        if (ManagerBlock.instance.isCarpetStage)
        {
            if (board.IsExistCarpet())
            {   //카펫위에 벌이 있는 경우, 카펫이 전부 깔려 있으면 검사 안 함, 아직 덜 깔렸으면 3순위.
                if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.CARPET))
                    return FireWorkRank.RANK_3;
            }
            else
            {   //카펫이 깔리지 않은 위치에 벌이 있고, 현재 쏘는 벌에 카펫이 묻어있으면 3순위.
                if (hasCarpet == true)
                    return FireWorkRank.RANK_3;
            }
        }

        //석상 목표를 달성하지 않은 상태일 때, 잔디의 우선순위.
        if (board.BoardOnGrass.Count > 0)
        {
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.STATUE))
                return FireWorkRank.RANK_3;
        }
        #endregion

        //모든 조건 해당하지 않으면, 블럭의 기본 우선순위 반환.
        return blockRank;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_5;
    }

    public override bool IsBlockCheckKeyTypeAtFireWork()
    {
        return true;
    }
}
