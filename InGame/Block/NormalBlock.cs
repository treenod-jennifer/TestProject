using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBlock : BlockBase
{
    float secendBombTimer = 1.6f;

    [System.NonSerialized]
    public bool isCanMakeBonusBombBlock = true;

    public override bool IsNormalBlock()
    {
        return bombType == BlockBombType.NONE;
    }
    public override bool IsCanMakeCarpetByBomb()
    {
        return true;
    }
    public override bool IsCanMove()
    {
        return true;
    }
    public override bool IsBombBlock()
    {
        return bombType != BlockBombType.NONE;
    }

    public override bool IsCanPang()
    {
        return true;
    }


    public override bool isCanPangByRainbow()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        DecoBase decoType = null;
        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            decoType = board.DecoOnBoard[i] as DecoBase;

            if (decoType != null)
            {
                if(decoType.IsDisturbPangByRainbowBomb() == false)
                    return false;
            }
        }

        return true;
    }

    public override bool IsCanMakeCarpet()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockPang())
            return false;

        return true;
    }

    public override bool IsCanRemoveWater()
    {
        return bombType == BlockBombType.NONE;
    }

    public override bool IsCanLink()
    {
        if (isRainbowBomb) return false;

        if (blockDeco != null && blockDeco.IsInterruptBlockSelect()) return false;

        if (inGameItemUse) return false;

        if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            return false;

        if (isDestroyLink == true)
            return false;

        return true;
    }

    public override bool IsPangAffectBoard()
    {
        return true;
    }

    public override bool IsSelectable()
    {
        if (IsExplodeByBomb)
            return false;

        if (normalPangDelay)
            return false;

        if (secendBomb)
            return false;

        if (state != BlockState.WAIT)
            return false;

        return true;
    }

    public override bool IsCanAddCoin()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        if(bombType == BlockBombType.RAINBOW)
        {
            if(rainbowSprite == null)
            {
                rainbowSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockTempBombLabel).GetComponent<UIBlockSprite>();
                rainbowSprite.spriteName = "ToyRainbow_01";
                rainbowSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
                MakePixelPerfect(rainbowSprite);
            }
        }

        mainSprite.spriteName = GetSpriteName();
        SetMainSpriteDepth();

        if (toyBombSprite != null) toyBombSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;//indexY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT + ManagerBlock.BLOCK_SRPRITE_MIN + 1;
        SetExpectTypeSprite(expectType);
        MakePixelPerfect(mainSprite);
    }

    //외부에서 블럭을 터트릴때 //폭발을 막는지 안막는지 체크
    public override bool IsDisturbBlock_ByBomb(int tempPangIndex = 0, bool isPangByPowerBomb = false)
    {
        if (!IsCanPang())
            return IsPangExtendable();

        if (IsSkipPang)
            return IsPangExtendable();

        if (isRainbowBomb)
            return IsPangExtendable();

        if (state == BlockState.PANG)
            return IsPangExtendable();

        if (secendBomb)
            return IsPangExtendable();

        if (pangIndex != tempPangIndex)
        {
            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
            {
                return IsPangExtendable();
            }
        }
        return IsPangExtendable();
    }

    //외부에서 블럭을 터트릴때 //폭발을 막는지 안막는지 체크
    public override bool BlockPang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false, bool isPangByPowerBomb = false)
    {
        if (!IsCanPang())
            return IsPangExtendable();

        if (IsSkipPang)
            return IsPangExtendable();

        if (isRainbowBomb)
            return IsPangExtendable();

        if (state == BlockState.PANG)
            return IsPangExtendable();

        if (secendBomb)
            return IsPangExtendable();

        if (isMakeBomb)
        {
            isUseMakeBomb = false;
            isMakeBomb = false;
            pangIndex = tempPangIndex;
            state = BlockState.WAIT;
            //InGameEffectMaker.instance.MakeBlockPangEffec(_transform.position);            
            InGameEffectMaker.instance.MakeBombMakeEffect(_transform.position);

            //모으기일때 날리기
            ManagerBlock.instance.PangColorBlock(colorType, transform.position);
            
            if (ManagerBlock.instance.isFeverTime() && coinFeverBomb)
            {
                //폭탄효과
                BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.CLEAR_BOMB, BlockColorType.NONE, false);
                coinFeverBomb = false;
            }

            if (IsSpecialEventBlock)
            {
                ManagerBlock.instance.getSpecialEventBlock++;
                ManagerBlock.instance.specialEventAppearCount--;
                GameUIManager.instance.RefreshSpecialEvent();
                AddSpecialEventSprite(true);
            }

            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
            {
                if (!getBoard.HasDecoHideBlock(true, tempPangIndex, pangColorType)
                    || !getBoard.HasDecoCoverBlock(true, tempPangIndex, pangColorType))
                {
                    if (blockDeco == null || !blockDeco.DecoPang(tempPangIndex, pangColorType))
                    {
                        getBoard.BoardPang(tempPangIndex, pangColorType, PangByBomb);
                        SetSplashEffect(colorType, false);
                    }
                }
                return IsPangExtendable();
            }
            return IsPangExtendable();
        }

        if (pangIndex != tempPangIndex)
        {
            pangIndex = tempPangIndex;

            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
            {
                
                if (getBoard.HasDecoHideBlock(true, tempPangIndex, pangColorType)
                    || getBoard.HasDecoCoverBlock(true, tempPangIndex, pangColorType))
                {
                    isUseMakeBomb = false;
                    if (IsNormalBlock() && expectType != BlockBombType.NONE)
                    {
                        targetPos = _transform.localPosition;
                        state = BlockState.MOVE;
                    }
                    else
                    {
                        state = BlockState.WAIT;
                    }

                    bombEffect = false;
                    IsExplodeByBomb = false;
                    normalPangDelay = false;
                }
                else
                {
                    if (blockDeco == null)
                    {
                        if (IsBombBlock() && IsExplodeByBomb)
                        {
                            isUseMakeBomb = false;
                            state = BlockState.WAIT;
                        }
                        else if (normalPangDelay)
                        {
                            isUseMakeBomb = false;
                            state = BlockState.PANG;
                        }
                        else
                        {
                            state = BlockState.PANG;
                            Pang(pangColorType, PangByBomb);
                        }
                    }
                    else
                    {
                        if (blockDeco.DecoPang(tempPangIndex, pangColorType))
                        {
                            isUseMakeBomb = false;
                            state = BlockState.WAIT;
                            bombEffect = false;
                            IsExplodeByBomb = false;
                            normalPangDelay = false;
                        }
                        else
                        {
                            if (IsBombBlock() && IsExplodeByBomb)
                            {
                                isUseMakeBomb = false;
                                state = BlockState.WAIT;
                            }
                            else if (normalPangDelay)
                            {
                                isUseMakeBomb = false;
                                state = BlockState.PANG;
                            }
                            else
                            {
                                state = BlockState.PANG;
                                Pang(pangColorType, PangByBomb);
                            }
                        }
                    }
                }
                return IsPangExtendable();
            }
        }
        return IsPangExtendable();
    }
    
    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (bombType == BlockBombType.RAINBOW)
            isSkipDistroy = true;

        RemoveLinkerNoReset();

        //블럭 모으기 이벤트 처리.
        GetCollectEventBlock();

        //목표일때 날리기
        //슬라임날리기, 잡기돌날리기    
        Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoard != null)
        {
            getBoard.BoardPang(pangIndex, pangColorType, PangByBomb);
        }
        
        if (bombType != BlockBombType.NONE && bombType != BlockBombType.DUMMY)
        {
            BlockColorType tempColorType = colorType;
            if (colorType == BlockColorType.NONE || colorType == BlockColorType.RANDOM)
            {
                tempColorType = rainbowColorType;
            }            
            BlockMaker.instance.MakeBombBlock(this, indexX, indexY, bombType, tempColorType, secendBomb);
        }
        else
        {
            ManagerSound.AudioPlayMany(AudioInGame.PANG1);
        }

        // 폭탄 목표인 경우
        SetTargetBombBlock();

        if (bombType == BlockBombType.NONE 
            || bombType == BlockBombType.RAINBOW 
            || bombType == BlockBombType.RAINBOW_X_BOMB 
            || bombType == BlockBombType.RAINBOW_X_LINE)
        {
            SetSplashEffect(colorType, PangByBomb);   //팡인덱스가 십만보다 크면 폭탄에 의한 폭발 //사이드이펙트주기
        }

        if (bombType == BlockBombType.RAINBOW_X_RAINBOW)
        {
            SetSplashEffectRXR(bombType, false);
        }

        if( bombType == BlockBombType.LINE_H || bombType == BlockBombType.LINE_V)
        {
            SetSplashEffectLine(bombType, PangByBomb);
        }

        //if (!isAutoPang)
        {
            if (isUseMakeBomb)
            {
                InGameEffectMaker.instance.MakeBombMakePangEffect(_transform.position);
                isUseMakeBomb = false;
            }

            else if(PangByBomb == false)
                InGameEffectMaker.instance.MakeBlockPangEffect(_transform.position);
        }

        if ((ManagerBlock.instance.stageInfo.BombType == 0 && bombType == BlockBombType.BOMB) ||
            bombType == BlockBombType.BOMB_X_BOMB)
        {
            if (!secendBomb)
            {
                BlockBomb._liveCount++;
                
                CircleBombEffect();
                isCheckedMatchable = true;
                secendBomb = true;
                IsSkipPang = true;
                state = BlockState.WAIT;

                if (ManagerBlock.instance.isCarpetStage)
                {
                    Board tempBoard = PosHelper.GetBoardSreeen(indexX, indexY);
                    if (tempBoard != null && bombHasCarpet == false)
                    {
                        bombHasCarpet = tempBoard.IsExistCarpetAndCanExpand();
                    }
                }

                _pangAlphaDelay = 0.3f;
                _pangRemoveDelay = 0.7f;
                return;
            }
            else
            {
                CircleBombEffect(true);
                BlockBomb._liveCount--;
            }

        }

        if (bombType == BlockBombType.RAINBOW ||
            bombType == BlockBombType.RAINBOW_X_RAINBOW)
            StartCoroutine(CoPangRainbowSprite());

        if (bombType == BlockBombType.NONE)
        {
            ManagerBlock.instance.PangColorBlock(colorType, transform.position);

            if (ManagerBlock.instance.isFeverTime() && coinFeverBomb)
            {
                //폭탄효과
                BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.CLEAR_BOMB, BlockColorType.NONE, false);
                coinFeverBomb = false;
            }

            if (GameManager.gameMode == GameMode.ADVENTURE)
                AdventureManager.instance.AddAnimalAttack(colorType, transform.position);
        }
        else if (bombType != BlockBombType.NONE && inGameItemUse && rainbowBombHammerUse == false)
        {
            ManagerBlock.instance.PangColorBlock(colorType, transform.position);
        }

        if (hasCoin || ManagerBlock.instance.isFeverTime() == true)
        {   
            InGameEffectMaker.instance.MakeFlyCoin(transform.position, 1);
        }
        
        DestroyBlockData();
    }

    //즉시제거
    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;

        if (bombType != BlockBombType.NONE)
        {
            bombType = BlockBombType.DUMMY;
        }

        IsSkipPang = false;
        normalPangDelay = false;
        normaPangDelayTimer = 0f;
        state = BlockState.PANG;
        isUseMakeBomb = false;

        Pang(PangByBomb: true);

        yield return null;
    }

    public void SetTargetBombBlock()
    {
        if (rainbowBombHammerUse == false)
        {
            if (bombType == BlockBombType.LINE_H || bombType == BlockBombType.LINE_V
                || TempBombType == BlockBombType.LINE_H || TempBombType == BlockBombType.LINE_V
                || bombType == BlockBombType.R_LINE_H || bombType == BlockBombType.R_LINE_V)
            {
                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_LINE);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_LINE);

                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_ALL);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_ALL);
            }
            else if ((bombType == BlockBombType.BOMB && !secendBomb)
                || (TempBombType == BlockBombType.BOMB && !secendBomb)
                || bombType == BlockBombType.HALF_BOMB)
            {
                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_CIRCLE);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_CIRCLE);

                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_ALL);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_ALL);
            }
            else if (bombType == BlockBombType.RAINBOW
                || TempBombType == BlockBombType.RAINBOW)
            {
                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_RAINBOW);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_RAINBOW);

                ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.BOMB_ALL);
                GameUIManager.instance.RefreshTarget(TARGET_TYPE.BOMB_ALL);
            }
        }
    }

    public override void DestroyBlockData(int adType = -1)
    {
        if (bombType != BlockBombType.NONE)
        {
            if (bombType == BlockBombType.BOMB && secendBomb == false)
            {
                return;
            }
            BlockType adjustBlockType = type;

            switch (bombType)
            {
                case BlockBombType.LINE_H:
                case BlockBombType.LINE_V:
                    adjustBlockType = BlockType.START_Line;
                    break;
                case BlockBombType.BOMB:
                    adjustBlockType = BlockType.START_Bomb;
                    break;
                case BlockBombType.RAINBOW:
                    adjustBlockType = BlockType.START_Rainbow;
                    break;
                default:
                    if (TempBombType == BlockBombType.LINE_H || TempBombType == BlockBombType.LINE_V)
                        adjustBlockType = BlockType.START_Line;
                    else if (TempBombType == BlockBombType.BOMB)
                        adjustBlockType = BlockType.START_Bomb;
                    else if (TempBombType == BlockBombType.RAINBOW)
                        adjustBlockType = BlockType.START_Rainbow;
                    break;
            }

            base.DestroyBlockData((int)adjustBlockType);
        }
    }

    public void SetSplashEffectLine(BlockBombType tempBomb, bool PangByBomb = false)
    {
        if (tempBomb == BlockBombType.LINE_V)
        {
			if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).IsNotDisturbBlock(BlockDirection.LEFT) == true)
                {
                    if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).SetSplashEffect(pangIndex, colorType, PangByBomb) == false)
                    {
                        if (!PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoHideBlock()
                            && !PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoCoverBlock()
                            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null)
                            PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.SetSplashPang(pangIndex, colorType, PangByBomb, 0, byRainbowBomb);
                    }
                }
            }

			if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).IsNotDisturbBlock(BlockDirection.RIGHT) == true)
                {
                    if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).SetSplashEffect(pangIndex, colorType, PangByBomb) == false)
                    {
                        if (!PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoHideBlock()
                            && !PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoCoverBlock()
                            && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null)
                            PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.SetSplashPang(pangIndex, colorType, PangByBomb, 0, byRainbowBomb);
                    }
                }          
			}
        }

        if (tempBomb == BlockBombType.LINE_H)
        {
             if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).IsNotDisturbBlock(BlockDirection.UP) == true)
                {
                    if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).SetSplashEffect(pangIndex, colorType, PangByBomb) == false)
                    {
                        if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoHideBlock()
                            && !PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoCoverBlock()
                            && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null)
                            PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.SetSplashPang(pangIndex, colorType, PangByBomb, 0, byRainbowBomb);
                    }
                }
            }

            if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).IsNotDisturbBlock(BlockDirection.DOWN) == true)
                {
                    if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).SetSplashEffect(pangIndex, colorType, PangByBomb) == false) 
                    {
                        if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoHideBlock()
                            && !PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoCoverBlock()
                            && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null)
                            PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.SetSplashPang(pangIndex, colorType, PangByBomb, 0, byRainbowBomb);
                    }
                }
            }
        }
    }

    public void SetSplashEffectRXR(BlockBombType tempBomb, bool PangByBomb = false)
    {
        if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null)
        {
            if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).IsNotDisturbBlock(BlockDirection.LEFT) == true)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).SetSplashEffect(pangIndex, colorType, false) == false)
                {
                    if (!PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoHideBlock()
                        && !PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoCoverBlock()
                        && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null
                        && (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.type == BlockType.ColorBigJewel
                        || PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.type == BlockType.LITTLE_FLOWER_POT))
                        PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.SetSplashPang(pangIndex, colorType, false, 1, byRainbowBomb);
                }
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null)
        {
            if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).IsNotDisturbBlock(BlockDirection.RIGHT) == true)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).SetSplashEffect(pangIndex, colorType, false) == false)
                {
                    if (!PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoHideBlock()
                        && !PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoCoverBlock()
                        && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null
                        && (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.type == BlockType.ColorBigJewel
                        || PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.type == BlockType.LITTLE_FLOWER_POT))
                        PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.SetSplashPang(pangIndex, colorType, false, 3, byRainbowBomb);
                }
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null)
        {
            if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).IsNotDisturbBlock(BlockDirection.UP) == true)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).SetSplashEffect(pangIndex, colorType, false) == false)
                {
                    if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoHideBlock()
                        && !PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoCoverBlock()
                        && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null
                        && (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.type == BlockType.ColorBigJewel
                        || PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.type == BlockType.LITTLE_FLOWER_POT))
                        PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.SetSplashPang(pangIndex, colorType, false, 2, byRainbowBomb);
                }
            }
        }

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null)
        {
            if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).IsNotDisturbBlock(BlockDirection.DOWN) == true)
            {
                if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).SetSplashEffect(pangIndex, colorType, false) == false)
                {
                    if (!PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoHideBlock() 
                        && !PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoCoverBlock() 
                        && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null
                        && (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.type == BlockType.ColorBigJewel
                        || PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.type == BlockType.LITTLE_FLOWER_POT))
                        PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.SetSplashPang(pangIndex, colorType, false, 0, byRainbowBomb);
                }
            }
        }      
    }

    void GetRandomType()
    {

    }

    public override void UpdateBlock()
    {
        MoveBlock();
        BombSecendWaitEffect();
        ChangeLineType();

        ShowMakeBombEffect();

        BombSpriteAnimation();
        CheckExplodeByBomb();
        checkNormalDelay();
    }

    float explodeTimer = 0;
    float explodeAlpha = 0;

    void checkNormalDelay()
    {
        if (normalPangDelay == false) return;

        normaPangDelayTimer -= Global.deltaTimePuzzle;

        if(normaPangDelayTimer <= 0)
        {
            normalPangDelay = false;
            normaPangDelayTimer = 0f;
            state = BlockState.PANG;
            Pang();
            StartCoroutine(CoPangEffect());
            //이펙트
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


    void CheckExplodeByBomb()
    {
        if (!IsExplodeByBomb) return;

        if (secendBomb)
        {
            IsExplodeByBomb = false;
            return;
        }

        explodeTimer += Global.deltaTimePuzzle;

        float ratio = 1f - Mathf.Cos(explodeTimer * ManagerBlock.PI90 * 25);
        explodeAlpha = Mathf.Lerp(1, 0, ratio);
        mainSprite.color = new Color(1, 1, 1, 0.5f + explodeAlpha * 0.5f);
        mainSprite.cachedTransform.localScale = Vector3.one * (1 + explodeAlpha * 0.2f);

        float WAIT_TIME = 0.1f;
        //if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode == AdventureMode.ORIGIN)
        //    WAIT_TIME = 0.5f;

        if (explodeTimer > WAIT_TIME)
        {
            IsExplodeByBomb = false;
            explodeTimer = 0f;
            state = BlockState.PANG;
            Pang();
        }
    }

    public UIBlockSprite rainbowSprite;

    void BombSpriteAnimation()
    {
        if (PosHelper.GetBoardSreeen(indexX, indexY) != null && PosHelper.GetBoardSreeen(indexX, indexY).BoardOnNet.Count != 0) return;

        if (bombType == BlockBombType.NONE)
        {
            if (rainbowSprite != null) Destroy(rainbowSprite.gameObject);
                return;
        }
        if (bombType == BlockBombType.RAINBOW)
        {
            if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
                return;

            int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 7f) % 5 + 1);

            if (rainbowSprite == null)
            {
                rainbowSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockTempBombLabel).GetComponent<UIBlockSprite>();
                rainbowSprite.spriteName = "ToyRainbow_0" + spriteNumber;
                rainbowSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
                MakePixelPerfect(rainbowSprite);
            }
            rainbowSprite.spriteName = "ToyRainbow_0" + spriteNumber;

            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 10);
            mainSprite.cachedTransform.localEulerAngles = new Vector3(0, 0, ratioScale * 20);
        }
        else if (bombType == BlockBombType.BOMB && !playBombEffect && !secendBomb)
        {
            if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
                return;

            int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 7f) % 4 + 1);
            mainSprite.spriteName = GetSpriteName() + "_0" + spriteNumber;

           // if (state == BlockState.WAIT)
            {
                float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime*10);
                mainSprite.cachedTransform.localScale = Vector3.one * (0.95f + ratioScale * 0.07f);
            }

        }
        else if (bombType == BlockBombType.LINE_H || bombType == BlockBombType.LINE_V)
        {
            if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
                return;

            int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 7f) % 5 + 1);
            mainSprite.spriteName = GetSpriteName() + "_0" + spriteNumber;

            // if(state == BlockState.WAIT)
            {
                float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime*10);
                if(bombType == BlockBombType.LINE_V) mainSprite.cachedTransform.localScale = new Vector3(0.94f + ratioScale*0.08f, 0.9f - ratioScale *0.1f, 1) * 1.05f;
                if (bombType == BlockBombType.LINE_H) mainSprite.cachedTransform.localScale = new Vector3(0.9f - ratioScale * 0.1f,0.94f + ratioScale * 0.08f, 1) * 1.05f;
            }
        }
    }

    public override string GetSpriteName()
    {
        if (bombType == BlockBombType.DUMMY)
        {
            switch (TempBombType)
            {
                case BlockBombType.RAINBOW:
                case BlockBombType.RAINBOW_X_BOMB:
                case BlockBombType.RAINBOW_X_LINE:
                case BlockBombType.RAINBOW_X_RAINBOW:

                    if (colorType == BlockColorType.NONE)
                    {
                        colorType = rainbowColorType;
                    }

                    return "ToyRainbow_" + GetColorTypeString();

                case BlockBombType.CLEAR_BOMB:
                case BlockBombType.BOMB:
                case BlockBombType.HALF_BOMB:
                case BlockBombType.BOMB_X_BOMB:
                case BlockBombType.BOMB_X_LINE:
                    return "Toy_bomb";

                case BlockBombType.LINE_X_LINE:
                case BlockBombType.R_LINE_H:                
                case BlockBombType.LINE_H:
                    return "Toy_lineH";
                case BlockBombType.LINE_V:
                case BlockBombType.R_LINE_V:
                    return "Toy_lineV";
                default:
                    return "block" + GetColorTypeString();
            }
        }
        else
        {
            switch (bombType)
            {

                case BlockBombType.RAINBOW:
                case BlockBombType.RAINBOW_X_BOMB:
                case BlockBombType.RAINBOW_X_LINE:
                case BlockBombType.RAINBOW_X_RAINBOW:
                    if (colorType == BlockColorType.NONE)
                    {
                        colorType = rainbowColorType;
                    }
                    return "ToyRainbow_" + GetColorTypeString();

                case BlockBombType.CLEAR_BOMB:
                case BlockBombType.BOMB:
                case BlockBombType.HALF_BOMB:
                case BlockBombType.BOMB_X_BOMB:
                case BlockBombType.BOMB_X_LINE:
                    return "Toy_bomb";
                case BlockBombType.LINE_X_LINE:
                case BlockBombType.R_LINE_H:            
                case BlockBombType.LINE_H:
                    return "Toy_lineH";
                case BlockBombType.R_LINE_V:            
                case BlockBombType.LINE_V:
                    return "Toy_lineV";
                default:
                    return "block" + GetColorTypeString();
            }
        }
    }


    void BombSecendWaitEffect()
    {
        if (secendBomb && state != BlockState.PANG)
        {
            secendBombTimer -= Global.deltaTimePuzzle;

            {
                float ratio = 0.6f + Mathf.Abs(Mathf.Sin(Mathf.PI * ManagerBlock.instance.BlockTime * 5f)) * 0.5f;
                mainSprite.cachedTransform.localScale = new Vector3(ratio, ratio, 1f);

                float ratioColor = Mathf.Cos(Mathf.PI * ManagerBlock.instance.BlockTime * 10);
                mainSprite.color = new Color(0.8f + 0.2f * ratioColor, 0.8f + 0.2f * ratioColor, 0.8f + 0.2f * ratioColor, 1);
            }

            if (secendBombTimer <= 0 && state == BlockState.WAIT)
            {
                pangIndex++;
                IsSkipPang = false;
                _pangAlphaDelay = 0.3f;
                _pangRemoveDelay = 0.7f;
                state = BlockState.PANG;
                Pang();
            }
        }
    }


    public override void SetStateMove()
    {
        changeLineSprite = 0;
        changeLineTimer = 0;

    }

    public override void SetStatePang()
    {
        changeLineSprite = 0;
        changeLineTimer = 0;
    }

    IEnumerator ChangeBlockItem(bool isNoneType = false)
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;

        while (showTimer < 0.8f)
        {
            showTimer += Global.deltaTimePuzzle * 3.5f;

            if (showTimer < 0.5f)
            {
                scaleRatio = 0.7f + showTimer;
            }
            else
            {
                scaleRatio = 1.8f - showTimer;
            }

            if (toyBombSprite != null) toyBombSprite.cachedTransform.localScale = Vector3.one * scaleRatio;
            yield return null;
        }

        if (toyBombSprite != null) toyBombSprite.cachedTransform.localScale = Vector3.one;
        yield return null;
    }

    IEnumerator ChangeBlockBombItem(bool isNoneType = false)
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;
        bool changeSprite = false;

        while (showTimer < 8f)
        {
            showTimer += Global.deltaTimePuzzle * 4f;

            if (showTimer < 0.4f)
            {
                scaleRatio = 1 - showTimer;
            }
            else
            {
                scaleRatio = 0.2f + showTimer;

                if (!changeSprite)
                {
                    changeSprite = true;
                }
            }

          //  if (toyBombSprite != null) toyBombSprite.color = new Color(1, 1, 1, scaleRatio);
            SetSpriteAlpha(scaleRatio);//mainSprite.color = new Color(1, 1, 1, scaleRatio);

            
            yield return null;
        }

       // if (toyBombSprite != null) toyBombSprite.color = Color.white;
        SetSpriteAlpha(1); //mainSprite.color = Color.white;

        yield return null;
    }

    public override void RemoveTempBombType()
    {
        if (toyBombSprite != null) Destroy(toyBombSprite.gameObject);
        toyBombSprite = null;
    }

    public override void ChangeTempBombSprite(BlockBombType expectBombType)
    {
        if (bombType != BlockBombType.NONE)
        {
            UpdateSpriteByBlockType();
            if (toyBombSprite != null) Destroy(toyBombSprite.gameObject);
            toyBombSprite = null;
        }
        else if (expectBombType == BlockBombType.NONE)
        {
            UpdateSpriteByBlockType();
            if (toyBombSprite != null) Destroy(toyBombSprite.gameObject);
            toyBombSprite = null;
            StartCoroutine(ChangeBlockBombItem());
            StartCoroutine(ChangeBlockItem());
        }
        else
        {
            if(colorType == BlockColorType.A || colorType == BlockColorType.B || colorType == BlockColorType.C || colorType == BlockColorType.D || colorType == BlockColorType.E)             
                    mainSprite.spriteName = "block" + GetColorTypeString() + "_Base";

            if (toyBombSprite == null)
                toyBombSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockTempBombLabel).GetComponent<UISprite>();

            toyBombSprite.cachedTransform.localScale = Vector3.one;
            SetExpectTypeSprite(expectBombType);
            toyBombSprite.depth = mainSprite.depth + 1;
            MakePixelPerfect(toyBombSprite);
        }
    }

    private void SetExpectTypeSprite(BlockBombType eType)
    {
        if (toyBombSprite == null)
            return;

        switch (eType)
        {
            case BlockBombType.BOMB:
                toyBombSprite.spriteName = "block" + GetColorTypeString() + "_Bomb";
                toyBombSprite.cachedTransform.eulerAngles = Vector3.zero;
                StartCoroutine(ChangeBlockBombItem());
                StartCoroutine(ChangeBlockItem());
                break;
            case BlockBombType.RAINBOW:
                toyBombSprite.spriteName = "block" + GetColorTypeString() + "_rainbow";
                toyBombSprite.cachedTransform.eulerAngles = Vector3.zero;
                StartCoroutine(ChangeBlockBombItem());
                StartCoroutine(ChangeBlockItem());
                break;
            case BlockBombType.LINE_V : case BlockBombType.LINE_H:
                toyBombSprite.spriteName = "block" + GetColorTypeString() + "_line";
                if (changeLineSprite == 0)
                {
                    toyBombSprite.cachedTransform.eulerAngles = (eType == BlockBombType.LINE_H) ?
                        Vector3.zero : new Vector3(0, 0, -90f);
                    StartCoroutine(ChangeBlockBombItem());
                    StartCoroutine(ChangeBlockItem());
                }
                break;
        }
    }

    public override void MoveAction()
    {
        if (state != BlockState.WAIT)
            return;

        if (bombType != BlockBombType.NONE && toyBombSprite != null) Destroy(toyBombSprite.gameObject);
        
        if (GameManager.instance.lineBombRotate)
        {
            if (expectType == BlockBombType.LINE_H && ManagerBlock.instance.LineType == BlockBombType.LINE_V)
            {
                changeLineSprite = 1;
                expectType = ManagerBlock.instance.LineType;
            }
            else if (expectType == BlockBombType.LINE_V && ManagerBlock.instance.LineType == BlockBombType.LINE_H)
            {
                changeLineSprite = -1;
                expectType = ManagerBlock.instance.LineType;
            }
        } 
    }

    public int changeLineSprite = 0;
    public float changeLineTimer = 0;

    void ChangeLineType()
    {
        if (GameManager.instance.lineBombRotate == false) return;

        if (changeLineSprite != 0)
        {
            if (bombType != BlockBombType.NONE)
            {
                if (toyBombSprite != null) Destroy(toyBombSprite.gameObject);
                return;
            }

            if (changeLineTimer < 1f)
            {
                changeLineTimer += Global.deltaTimePuzzle * 10f;
                if (changeLineSprite == -1)
                {
                    if (toyBombSprite != null)
                        toyBombSprite.cachedTransform.eulerAngles = new Vector3(0, 0, -90 * (1 - changeLineTimer));
                }
                else
                {
                    if (toyBombSprite != null)
                        toyBombSprite.cachedTransform.eulerAngles = new Vector3(0, 0, -90 * changeLineTimer);
                }
            }
            else
            {
                if (changeLineSprite == -1)
                {
                    if (toyBombSprite != null) toyBombSprite.cachedTransform.eulerAngles = new Vector3(0, 0, 0);
                }
                else
                {
                    if (toyBombSprite != null) toyBombSprite.cachedTransform.eulerAngles = new Vector3(0, 0, -90);
                }
                changeLineTimer = 0;
                changeLineSprite = 0;
            }

            if (expectType != BlockBombType.LINE_V && expectType != BlockBombType.LINE_H)
            {
                changeLineTimer = 0;
                changeLineSprite = 0;
                if (toyBombSprite != null) toyBombSprite.cachedTransform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
    }

    public override string GetRainbowEffectSpriteName()
    {
        if (IsNormalBlock())
            return "block" + GetColorTypeString() + "H";
        else if (IsBombBlock()) //레인보우x레인보우 사용 시, 폭탄타입 이펙트 생성
            return GetSpriteName() + "_H";

        return "";
    }

    private float makeBombTimer = 0;
    void ShowMakeBombEffect()
    {
        if (isShowMakeBombEffect)
        {
            float scale = 1f;

            makeBombTimer += Global.deltaTimePuzzle * 1.5f;

            if (makeBombTimer > 0.3f && bombType != TempBombType)
            {
                if (TempBombType != BlockBombType.RAINBOW) colorType = BlockColorType.NONE;

                bombType = TempBombType;
                if (ManagerBlock.instance.isCarpetStage)
                {
                    Board tempBoard = PosHelper.GetBoard(indexX, indexY);
                    if (tempBoard != null && tempBoard.IsExistCarpetAndCanExpand())
                    {   
                        bombHasCarpet = true;
                        CoverBlockWithCarpet();
                    }
                }

                RemoveLinkerNoReset();

                if (toyBombSprite != null) Destroy(toyBombSprite.gameObject);
                InGameEffectMaker.instance.MakeMakeItemEffect(_transform.position);
            }

            if (makeBombTimer > 0.4f)
            {
                float effectTimer = makeBombTimer - 0.4f;

                float ratio = Mathf.Sin(effectTimer * 20f) / Mathf.Exp(effectTimer * 8f);
                float ratio2 = Mathf.Cos(effectTimer * 20f) / Mathf.Exp(effectTimer * 8f);

                scale = 1f + 1f * ratio;

                if (makeBombTimer > 1f)
                {
                    BlockMatchManager.instance.CheckBlockLinkToItem(this);
                    makeBombTimer = 0f;
                    isShowMakeBombEffect = false;
                    mainSprite.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                scale = 1f - (makeBombTimer) / 0.4f;
            }
            mainSprite.cachedTransform.localScale = Vector3.one * _spriteScale * scale; // new Vector3(scale * _spriteScale, (2f - scale) * _spriteScale, 1f);
        }
    }


    void CircleBombEffect(bool destroy = false)
    {
        StartCoroutine(CoCircleBomb(destroy));
    }

    bool playBombEffect = false;

    IEnumerator CoCircleBomb(bool destroy)
    {
        playBombEffect = true;

        float blockSize = 1.2f;

        mainSprite.depth = (int)GimmickDepth.UI_LABEL;
        float scaleRatio = 1.0f;

        float timer = 0f;

        mainSprite.spriteName = "Toy_bomb_glow";
            
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;
            if (timer > 0.2f)
                timer = 0.2f;

            scaleRatio = 1.1f - timer *1.5f;
            mainSprite.cachedTransform.localScale = Vector3.one * blockSize * scaleRatio;
            yield return null;
        }

        timer = 0f;
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle*2f;
            if (timer > 0.2f)
                timer = 0.2f;

            scaleRatio =  1.1f -0.3f - timer * 0.2f;
            mainSprite.cachedTransform.localScale = Vector3.one * blockSize * scaleRatio;
            yield return null;
        }

        playBombEffect = false;

        if (!destroy)
        {
            mainSprite.cachedTransform.localScale = Vector3.one;
            mainSprite.color = new Color(1, 1, 1, 1);
            UpdateSpriteByBlockType();
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        yield return null;
    }



    IEnumerator CoPangRainbowSprite()
    {
        float timer = 0f;
        float shakeRatio = 0;

        mainSprite.spriteName = GetSpriteName()+ "_glow";
        mainSprite.color = new Color(1, 1, 1, 0.75f);
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle*2f;
            shakeRatio = ManagerBlock.instance._curveRainbowPang.Evaluate(timer);
            mainSprite.cachedTransform.localScale = Vector3.one * shakeRatio;
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, timer * 600f));                       

            yield return null;
        }

        while (timer < 4.0f)
        {
            timer += Global.deltaTimePuzzle*2f;
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, timer * 600f));
            yield return null;
        } 

        yield return null;
    }

    public override bool EventAction()
    {
        if (state != BlockState.WAIT) return false;

        if (GameManager.instance.lineBombRotate)
        {
            /*
            if (expectType == BlockBombType.LINE_H && ManagerBlock.instance.LineType == BlockBombType.LINE_V)
            {
                changeLineSprite = 1;
                expectType = ManagerBlock.instance.LineType;
            }
            else if (expectType == BlockBombType.LINE_V && ManagerBlock.instance.LineType == BlockBombType.LINE_H)
            {
                changeLineSprite = -1;
                expectType = ManagerBlock.instance.LineType;
            }
            */
            return false;
        }
        else
        {
            if (bombType == BlockBombType.LINE_H || bombType == BlockBombType.LINE_V)
            {
                if (isMatchedBomb)
                {
                    return false;
                }

                if (makeLineBomb)
                {
                    makeLineBomb = false;
                    return false;
                }

                if (isRainbowBomb)
                {
                    return false;
                }

                StartCoroutine(CoChangeLineDir(bombType == BlockBombType.LINE_V));
                return true;
            }
        }  
        
        return false;
    }

    IEnumerator CoChangeLineDir(bool lineV = false)
    {
        IsSkipPang = true;
        yield return null;
        float speedRatio = 5f;

        float changeTimer = 0;

        if (lineV)
        {
            while(changeTimer < 1)
            {
                if (isMatchedBomb)break;
                

                changeTimer += Global.deltaTimePuzzle* speedRatio;
                mainSprite.gameObject.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0,0,-90), (1 - Mathf.Cos( changeTimer*ManagerBlock.PI90*2))*0.5f);
                yield return null;
            }

            if (isMatchedBomb == false) bombType = BlockBombType.LINE_H;
            mainSprite.gameObject.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            while (changeTimer < 1)
            {
                if (isMatchedBomb) break;

                changeTimer += Global.deltaTimePuzzle* speedRatio;
                mainSprite.gameObject.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0,90), (1 - Mathf.Cos(changeTimer * ManagerBlock.PI90 * 2)) * 0.5f);
                yield return null;
            }
            if (isMatchedBomb == false) bombType = BlockBombType.LINE_V;
            mainSprite.gameObject.transform.localEulerAngles = Vector3.zero;
        }

        //if (isMatchedBomb) Debug.Log("매치_라인변환중");

            IsSkipPang = false;
        yield return null;
    }

    public override void MakeDummyImage(Vector3 tartgetPos)
    {
        base.MakeDummyImage(tartgetPos);

        if (rainbowSprite != null)
        {
            UIBlockSprite castleSprite = NGUITools.AddChild(gameObject, BlockMaker.instance.blockDummySpriteObj).GetComponent<UIBlockSprite>();
            castleSprite.spriteName = rainbowSprite.spriteName;
            castleSprite.depth = rainbowSprite.depth;
            //ManagerUIAtlas.CheckAndApplyEventAtlas(castleSprite);
            MakePixelPerfect(castleSprite);
            castleSprite.cachedTransform.localPosition = tartgetPos;
            dummySpriteList.Add(castleSprite.customFill);
        }

        if (specialEventSprite != null)
        {
            UIBlockUrlTexture specialEventSpriteA = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.urlImageObj).GetComponent<UIBlockUrlTexture>();

            specialEventSpriteA.blockSpriteType = CustomFill.BlockSpriteType.Block;
            specialEventSpriteA.depth = specialEventSprite.depth;
            specialEventSpriteA.SettingTextureScale(46, 46);
            specialEventSpriteA.Load(Global.gameImageDirectory, "IconEvent/", "sEventBlock_" + Global.specialEventIndex);
            specialEventSpriteA.cachedTransform.localPosition = tartgetPos + new Vector3(22, -21, 0);
            dummySpriteList.Add(specialEventSpriteA.customFill);
        }
    }

    public override void SpriteRatio(float ratio, float vertRatio = 0)
    {
        if (rainbowSprite != null)
        {
            rainbowSprite.customFill.verticalRatio = vertRatio;
            rainbowSprite.customFill.blockRatio = ratio;
        }

        if (specialEventSprite != null)
        {
            specialEventSprite.customFill.verticalRatio = vertRatio;
            specialEventSprite.customFill.blockRatio = ratio;
        }
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (IsBombBlock() == true || TempBombType != BlockBombType.NONE) //현재 블럭이 폭탄일 경우 우선순위
        {
            //목표를 달성하지 않은 상태일 때의 우선순위
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.BOMB_COLLECT))
                return FireWorkRank.RANK_1;
            else
                return FireWorkRank.RANK_5;
        }
        else //일반 블럭일 경우 우선순위
        {
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.COLORBLOCK, colorType))    //컬러블럭
                return FireWorkRank.RANK_1;
            else
                return FireWorkRank.RANK_4;
        }
    }

    public override bool IsBlockCheckKeyTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        if (IsBombBlock() == true || TempBombType != BlockBombType.NONE) //현재 블럭이 폭탄일 경우
            return false;
        else
            return true;
    }

    public void InitBlock(BlockType addAdjustType = BlockType.NONE)
    {
        //흙, 돌, 식물 등에 싸여진 기믹인 경우 추가
        AddAdjutBlockType(addAdjustType);

        if(bombType == BlockBombType.LINE)
        {
            int randomLine = GameManager.instance.GetIngameRandom(0, 2);
            bombType = randomLine == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;
        }

        //폭탄 타입인 경우 추가
        if(bombType == BlockBombType.LINE_H || bombType == BlockBombType.LINE_V)
        {
            AddAdjutBlockType(BlockType.START_Line);
        }
        else if (bombType == BlockBombType.BOMB)
        {
            AddAdjutBlockType(BlockType.START_Bomb);
        }
        else if (bombType == BlockBombType.RAINBOW)
        {
            AddAdjutBlockType(BlockType.START_Rainbow);
        }
    }
}