using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMatchManager : MonoSingletonOnlyScene<BlockMatchManager>
{

    #region 매치
    List<BlockBase> tempSelectList = new List<BlockBase>();

    void SetBlockSelectColor(float alphaColor)
    {
        foreach (var block in tempSelectList)
        {
            block.SetSpriteAlpha(alphaColor);
        }
    }
    
    public void SetSelectBlockEffect(BlockBase selectBlock)
    {
        SetBlockSelectColor(1);
        tempSelectList.Clear();

        if (selectBlock != null && selectBlock.IsSelectable() && !ManagerBlock.instance.isTouchDownBlock) 
        {
            List<BlockBase> tempNewSelectList = new List<BlockBase>();

            tempSelectList.Add(selectBlock);

            if (selectBlock.IsBombBlock())
            {
                GetNearByBombBlock(selectBlock);
            }
            else if(selectBlock.IsNormalBlock())
            {
                GetNearbyNormalBlock(selectBlock, tempNewSelectList);
                tempSelectList = tempNewSelectList;
            }
            SetBlockSelectColor(0.65f);
        }
    }

    //폭탄선택시 주변에 조합될 폭탄만 표시
    void GetNearByBombBlock(BlockBase block)
    {
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (getNearBlock != null
            && getNearBlock.IsBombBlock()
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (getNearBlock != null
            && getNearBlock.IsBombBlock()
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (getNearBlock != null
            && getNearBlock.IsBombBlock()
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (getNearBlock != null
            && getNearBlock.IsBombBlock()
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        int checkRainbow = 0;
        int checkBomb = 0;
        int checkLine = 0;
        BlockBase targetBlockRainbow = null;
        BlockBase targetBlockBomb = null;
        BlockBase targetBlockLine = null;

        foreach (var checkBlock in tempCheckBlockList)
        {
            if (checkBlock.bombType == BlockBombType.RAINBOW)
            {
                checkRainbow++;
                targetBlockRainbow = checkBlock;
            }
            if (checkBlock.bombType == BlockBombType.BOMB)
            {
                checkBomb++;
                targetBlockBomb = checkBlock;
            }
            if (checkBlock.bombType == BlockBombType.LINE_H || checkBlock.bombType == BlockBombType.LINE_V)
            {
                checkLine++;
                targetBlockLine = checkBlock;
            }
        }

        if (checkRainbow > 0)
        {
            tempSelectList.Add(targetBlockRainbow);
        }
        else if (checkBomb > 0)
        {
            tempSelectList.Add(targetBlockBomb);
        }
        else if (checkLine > 0)
        {
            tempSelectList.Add(targetBlockLine);
        }
    }


    void GetNearbyNormalBlock(BlockBase blockA, List<BlockBase> getTempSelectList)
    {
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(blockA.indexX, blockA.indexY, 1, 0);
        if (getNearBlock != null
            && !getTempSelectList.Contains(getNearBlock)
            && blockA.IsNormalBlock()
            && blockA.colorType == getNearBlock.colorType
            && ManagerBlock.instance.IsCanLinkSideBLock(blockA.indexX, blockA.indexY, 1, 0)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                getTempSelectList.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }

        }

        getNearBlock = PosHelper.GetBlockScreen(blockA.indexX, blockA.indexY, -1, 0);
        if (getNearBlock != null
            && !getTempSelectList.Contains(getNearBlock)
            && blockA.IsNormalBlock()
            && blockA.colorType == getNearBlock.colorType
            && ManagerBlock.instance.IsCanLinkSideBLock(blockA.indexX, blockA.indexY, -1, 0)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                getTempSelectList.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(blockA.indexX, blockA.indexY, 0, 1);
        if (getNearBlock != null
            && !getTempSelectList.Contains(getNearBlock)
            && blockA.IsNormalBlock()
            && blockA.colorType == getNearBlock.colorType
            && ManagerBlock.instance.IsCanLinkSideBLock(blockA.indexX, blockA.indexY, 0, 1)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                getTempSelectList.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(blockA.indexX, blockA.indexY, 0, -1);
        if (getNearBlock != null
            && !getTempSelectList.Contains(getNearBlock)
            && blockA.IsNormalBlock()
            && blockA.colorType == getNearBlock.colorType
            && ManagerBlock.instance.IsCanLinkSideBLock(blockA.indexX, blockA.indexY, 0, -1)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                getTempSelectList.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        foreach (var block1 in tempCheckBlockList)
        {
            GetNearbyNormalBlock(block1, getTempSelectList);
        }
    }


    //매치가능한 블럭인지 체크
    public bool checkBlockToyBlastMatch(BlockBase checkBlock, bool checkWait = false, bool checkNetBlock = true)
    {
        if (checkBlock == null)
        {
            return false;
        }

        if (checkNetBlock && PosHelper.GetBoardSreeen(checkBlock.indexX, checkBlock.indexY, 0, 0).HasDecoCoverBlock())
        {
            return false;
        }

        if (PosHelper.GetBoardSreeen(checkBlock.indexX, checkBlock.indexY).HasDecoHideBlock())
        {
            return false;
        }

        if (checkBlock.state != BlockState.WAIT)
            return false;

        if (checkBlock.blockDeco != null && checkBlock.blockDeco.IsInterruptBlockSelect())
            return false;

        if (!checkBlock.IsBlockType())
            return false;

        if (!checkBlock.IsSelectable())
            return false;

        if (checkBlock.IsNormalBlock())
        {
            //연결된것체크

            if (ManagerBlock.instance.IsCanLinkSideBLock(checkBlock.indexX, checkBlock.indexY, 1, 0)                 
                && checkNearBlockType(1, 0, checkBlock, checkWait)
                ) return true;
            else if (ManagerBlock.instance.IsCanLinkSideBLock(checkBlock.indexX, checkBlock.indexY, -1, 0)               
                && checkNearBlockType(-1, 0, checkBlock, checkWait)
                ) return true;
            else if (ManagerBlock.instance.IsCanLinkSideBLock(checkBlock.indexX, checkBlock.indexY, 0, 1)               
                && checkNearBlockType(0, 1, checkBlock, checkWait)
                ) return true;
            else if (ManagerBlock.instance.IsCanLinkSideBLock(checkBlock.indexX, checkBlock.indexY, 0, -1)              
                && checkNearBlockType(0, -1, checkBlock, checkWait)
                ) return true;
        }
        else if (checkBlock.IsBombBlock())
        {
            return true;
        }
        else if (checkBlock.IsSpecialMatchable())
        {
            return true;
        }

        return false;
    }

    bool checkNearBlockType(int addIndexX, int addIndexY, BlockBase block, bool checkWait = false)
    {
        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, addIndexX, addIndexY);
        if (getNearBlock != null && getNearBlock.colorType == block.colorType && getNearBlock.IsCanLink()  && getNearBlock.IsNormalBlock() )
        {
            if (getNearBlock.blockDeco != null && getNearBlock.blockDeco.IsInterruptBlockSelect())
            {
                return false;
            }

            if (checkWait)
            {
                if (getNearBlock.state == BlockState.WAIT)
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }

        }
        return false;
    }

    //매치체크
    private List<BlockBase> checkBlocks = new List<BlockBase>();

    ///////////////////
    //선택블럭 매치////
    ///////////////////
    public void CheckMatchPangBlock(BlockBase block, bool changeState = true)
    {
        SetBlockSelectColor(1);
        checkBlocks = new List<BlockBase>();
        checkBlocks.Clear();

        BlockBase.uniqueIndexCount++;

        if (block.IsBombBlock())
        {
            checkBombBlock(block);
        }
        else if(block.IsNormalBlock())
        {
            CheckNormalBlock(block, changeState);
        }
        else if (block.IsSpecialMatchable())
        {
            CheckSpecialBlock(block);
        }

        checkBlocks.Clear();

        if (changeState)
        {
            ManagerBlock.instance.state = BlockManagrState.MOVE;
        }

        ManagerSound.AudioPlay(AudioInGame.PANG1);
    }

    void CheckSpecialBlock(BlockBase tempBlock)
    {
        checkBlocks.Add(tempBlock);
        getNearSpecialBlock(tempBlock);
        BlockBomb._bombUniqueIndex++;
        bool hasCarpet = IsExistCarpetAtCheckBoards();

        for (int i = 0; i < checkBlocks.Count; i++)
        {
            if (checkBlocks[i].state != BlockState.PANG)
            {
                if (hasCarpet == true && checkBlocks[i].IsCanMakeCarpet())
                {
                    Board tempBoard = PosHelper.GetBoard(checkBlocks[i].indexX, checkBlocks[i].indexY);
                    tempBoard.MakeCarpet(0.08f * i);
                }

                checkBlocks[i].normalPangDelay = true;
                checkBlocks[i].normaPangDelayTimer = 0.08f * i;
                checkBlocks[i]._pangRemoveDelay = 0.3f;

                checkBlocks[i].MakeBombFieldEffect(0.08f * i);
                checkBlocks[i].BlockPang(BlockBase.uniqueIndexCount);
                checkBlocks[i].isCheckedMatchable = false;
                checkBlocks[i].expectType = BlockBombType.NONE;
                checkBlocks[i].RemoveLinkerNoReset();               


                if (checkBlocks[i].IsPangStopMove() && checkBlocks[i].StopMovingBlock == false)
                {
                    checkBlocks[i].pangIndex = BlockBomb._bombUniqueIndex;
                    BlockBase.StopMovingBlockCount++;
                    ManagerBlock.instance.blockMove = false;
                    ManagerBlock.instance.creatBlock = false;
                    checkBlocks[i].StopMovingBlock = true;
                }
            }
        }
            ManagerSound.AudioPlayPang(checkBlocks.Count);
    }

    bool checkNearSpecialBlock(BlockBase tempBlockA, BlockBase tempBlockB)
    {
        if (
            tempBlockB != null
            && !tempBlockB.isCheckedMatchable
            && !checkBlocks.Contains(tempBlockB)
            && tempBlockA.type == tempBlockB.type
            && tempBlockB.IsSpecialMatchable()
            && tempBlockB.IsCanPang()
            && tempBlockB.IsCanLink()
            )
        {
            if (tempBlockB.blockDeco != null && tempBlockB.blockDeco.IsInterruptBlockSelect()) return false;

            return true;
        }
        return false;
    }
    
    void getNearSpecialBlock(BlockBase block)
    {
        block.isCheckedMatchable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (checkNearSpecialBlock(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (checkNearSpecialBlock(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (checkNearSpecialBlock(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (checkNearSpecialBlock(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            if (!block1.isCheckedMatchable)
            {
                getNearSpecialBlock(block1);
            }
        }
    }

    bool IsExistCarpetAtCheckBoards()
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        for (int i = 0; i < checkBlocks.Count; i++)
        {
            if (IsExistCarpetAtBoard(PosHelper.GetBoard(checkBlocks[i].indexX, checkBlocks[i].indexY)) == true)
                return true;
        }
        return false;
    }

    bool IsExistCarpetAtCheckBlocks()
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        for (int i = 0; i < checkBlocks.Count; i++)
        {   
            if (checkBlocks[i].bombType != BlockBombType.NONE && checkBlocks[i].bombHasCarpet)
                return true;
        }
        return false;
    }

    bool IsExistCarpetAtBoard(Board board)
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        if (board == null)
            return false;

        if (board.IsExistCarpetAndCanExpand())
            return true;
        else
            return false;
    }

    bool IsExistCarpetAtBlock(BlockBase block)
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        if (block == null)
            return false;

        if (block.bombType != BlockBombType.NONE &&  block.bombHasCarpet == true)
            return true;
        else
            return false;
    }

    void CheckNormalBlock(BlockBase block, bool changeState)
    {        
        checkBlocks.Add(block);
        getNearNormalBlock(block);

        bool HasDynamite = false;
        bool hasCarpet = false;
        BlockBase targetBlock = checkBlocks[0];

        int tempCount = 0;
        foreach (var tempBlock in checkBlocks)
        {
            if (tempBlock.IsNormalBlock()) tempCount++;
            if (tempBlock is BlockDynamite) HasDynamite = true;
        }

        hasCarpet = IsExistCarpetAtCheckBoards();

        BlockBombType creatItem = BlockBombType.NONE;
        if (ManagerBlock.instance.isFeverTime() == false)
        {
            if (tempCount >= ManagerBlock.MAKE_RAINBOW_BLOCK_COUNT) //checkBlocks.Count
            {
                creatItem = BlockBombType.RAINBOW;
                checkBlocks[0]._pangAlphaDelay = 0.3f;
                checkBlocks[0]._pangRemoveDelay = 1f;
            }
            else if (tempCount >= ManagerBlock.MAKE_BOMB_BLOCK_COUNT)
            {
                creatItem = BlockBombType.BOMB;
            }
            else if (tempCount >= ManagerBlock.MAKE_LINE_BLOCK_COUNT)
            {
                creatItem = ManagerBlock.instance.LineType;
                if (block.expectType == BlockBombType.LINE_H || block.expectType == BlockBombType.LINE_V)
                    creatItem = block.expectType;
            }
        }

        if (GameManager.gameMode == GameMode.ADVENTURE && 
            GameManager.adventureMode != AdventureMode.ORIGIN)
        {
            creatItem = BlockBombType.NONE;
        }

        if (creatItem != BlockBombType.NONE)
        {
            switch (creatItem)
            {
            case BlockBombType.LINE:
            case BlockBombType.LINE_H:
            case BlockBombType.LINE_V:
                ManagerBlock.instance.creatBombCount[0].Value++;
                break;
            case BlockBombType.BOMB:
                ManagerBlock.instance.creatBombCount[1].Value++;
                break;
            case BlockBombType.RAINBOW:
                ManagerBlock.instance.creatBombCount[2].Value++;
                break;
            }

            if (checkBlocks[0] is NormalBlock)
            {
                checkBlocks[0].isCheckedMatchable = false;
                checkBlocks[0].TempBombType = creatItem;
                checkBlocks[0].isMakeBomb = true;
                checkBlocks[0].expectType = BlockBombType.NONE;
                checkBlocks[0].isShowMakeBombEffect = true;
                checkBlocks[0].makeLineBomb = true;
                ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                targetBlock = checkBlocks[0];
            }
            else
            {
                foreach(var temp in checkBlocks)
                {
                    if (temp is NormalBlock)
                    {
                        targetBlock = temp;

                        temp.isCheckedMatchable = false;
                        temp.TempBombType = creatItem;
                        temp.isMakeBomb = true;
                        temp.expectType = BlockBombType.NONE;
                        temp.isShowMakeBombEffect = true;
                        temp.makeLineBomb = true;
                        ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                        break;
                    }
                }
            }
            InGameEffectMaker.instance.MakeCoinStageCombo(checkBlocks[0]._transform.position, checkBlocks.Count);

            if (GameManager.gameMode == GameMode.ADVENTURE)
            {
                AdventureManager.instance.AddAnimalAttack(targetBlock.colorType, targetBlock._transform.position);
            }

        }

        InGameEffectMaker.instance.MakeScore(checkBlocks[0]._transform.position, checkBlocks.Count * 80);
        ManagerBlock.instance.AddScore(checkBlocks.Count * 80);
        

        for (int i = 0; i < checkBlocks.Count; i++)
        {
            if (checkBlocks[i].state != BlockState.PANG)
            {
                if (GameManager.gameMode == GameMode.ADVENTURE &&
                    GameManager.adventureMode != AdventureMode.ORIGIN)
                {
                    if (ManagerBlock.instance.comboCount == 0)
                    {
                        checkBlocks[i].normalPangDelay = true;
                        checkBlocks[i].normaPangDelayTimer = 0.05f; // *ManagerBlock.instance.comboCount; //0.05f * i + 0.2f;
                        checkBlocks[i]._pangRemoveDelay = 0.1f;
                    }
                    else
                    {
                        checkBlocks[i].normalPangDelay = true;
                        checkBlocks[i].normaPangDelayTimer = 0.35f;
                        checkBlocks[i]._pangRemoveDelay = 0.1f;
                    }

                }
                else if (creatItem != BlockBombType.NONE)
                {
                    checkBlocks[i].isUseMakeBomb = true;
                    ManagerBlock.instance.CollectBlocksUsedToMakeBomb(checkBlocks[i].colorType, checkBlocks[i].transform.position);
                }
                else if (creatItem == BlockBombType.NONE)
                {
                    checkBlocks[i].normalPangDelay = true;
                    if (GameManager.gameMode != GameMode.ADVENTURE) checkBlocks[i].normaPangDelayTimer = 0.07f * i;
                    checkBlocks[i]._pangRemoveDelay = 0.3f;
                }

                if (hasCarpet == true && checkBlocks[i].IsCanMakeCarpet())
                {
                    Board tempBoard = PosHelper.GetBoard(checkBlocks[i].indexX, checkBlocks[i].indexY);
                    tempBoard.MakeCarpet();
                }

                if (ManagerBlock.instance.isFeverTime() == true)
                    checkBlocks[i].coinFeverBomb = true;

                checkBlocks[i].BlockPang(BlockBase.uniqueIndexCount);
                checkBlocks[i].isCheckedMatchable = false;
                checkBlocks[i].expectType = BlockBombType.NONE;

                if (GameManager.gameMode != GameMode.ADVENTURE || GameManager.adventureMode == AdventureMode.ORIGIN)
                    checkBlocks[i].RemoveLinkerNoReset();


                if (creatItem != BlockBombType.NONE && checkBlocks[i] != block &&
                    checkBlocks[i].state == BlockState.PANG)
                {
                    checkBlocks[i]._pangRemoveDelay = 0.3f;
                    checkBlocks[i]._pangGatherEffect = targetBlock._transform.localPosition;
                }
            }
        }

        if(creatItem == BlockBombType.NONE)
        {
            if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode != AdventureMode.ORIGIN)             
                ManagerSound.AudioPlayPang(ManagerBlock.instance.comboCount + 1);            
            else 
                ManagerSound.AudioPlayPang(checkBlocks.Count);
        }
    }

    void checkBombBlock(BlockBase block)
    {
        checkBlocks.Clear();
        checkBlocks = GetNearBomb4D(block);

        bool hasCarpet = false;

        if(IsExistCarpetAtCheckBoards() == true || IsExistCarpetAtCheckBlocks() == true ||
           IsExistCarpetAtBoard(PosHelper.GetBoard(block.indexX, block.indexY)) == true ||
           IsExistCarpetAtBlock(block) == true)
            hasCarpet = true;

        if (checkBlocks.Count > 0)    
        {
            int checkRainbow = 0;
            int checkBomb = 0;
            int checkLine = 0;
            BlockColorType rainbowBlockType = BlockColorType.NONE;
            BlockBase targetBlockRainbow = null;
            BlockBase targetBlockBomb = null;
            BlockBase targetBlockLine = null;

            rainbowBlockType = block.colorType;

            foreach (var checkBlock in checkBlocks)
            {
                if (checkBlock.bombType == BlockBombType.RAINBOW)
                {
                    checkRainbow++;
                    if (rainbowBlockType == BlockColorType.NONE)
                    {
                        rainbowBlockType = checkBlock.colorType;
                    }
                    targetBlockRainbow = checkBlock;
                }
                if (checkBlock.bombType == BlockBombType.BOMB)
                {
                    checkBomb++;
                    targetBlockBomb = checkBlock;
                }
                if (checkBlock.bombType == BlockBombType.LINE_V || checkBlock.bombType == BlockBombType.LINE_H)
                {
                    checkLine++;
                    targetBlockLine = checkBlock;
                }

                if (hasCarpet == true)
                {
                    Board tempBoard = PosHelper.GetBoard(checkBlock.indexX, checkBlock.indexY);
                    tempBoard.MakeCarpet();
                }
            }

            block.rainbowColorType = rainbowBlockType;

            block.TempBombType = block.bombType;

            if (block.bombType == BlockBombType.RAINBOW)
            {
                if (checkRainbow > 0)
                {
                    block.bombType = BlockBombType.RAINBOW_X_RAINBOW;
                    block.isSkipDistroy = true;
                    RemoveMatchBombBlock(block, targetBlockRainbow);

                    block.isMatchedBomb = true;
                    targetBlockRainbow.isMatchedBomb = true;
                }
                else if (checkBomb > 0)
                {
                    block.bombType = BlockBombType.RAINBOW_X_BOMB;
                    block.isSkipDistroy = true;
                    RemoveMatchBombBlock(block, targetBlockBomb);
                    block._pangAlphaDelay = 0.3f;
                    block._pangRemoveDelay = 1f;

                    block.isMatchedBomb = true;
                    targetBlockBomb.isMatchedBomb = true;
                }
                else if (checkLine > 0)
                {
                    block.bombType = BlockBombType.RAINBOW_X_LINE;
                    block.isSkipDistroy = true;
                    RemoveMatchBombBlock(block, targetBlockLine);
                    block._pangAlphaDelay = 0.3f;
                    block._pangRemoveDelay = 1f;

                    block.isMatchedBomb = true;
                    targetBlockLine.isMatchedBomb = true;
                }
            }
            else if (block.bombType == BlockBombType.BOMB)
            {
                if (checkRainbow > 0)
                {
                    block.bombType = BlockBombType.RAINBOW_X_BOMB;
                    block.isSkipDistroy = true;
                    block.rainbowColorType = targetBlockRainbow.colorType;
                    RemoveMatchBombBlock(block, targetBlockRainbow);
                    block._pangAlphaDelay = 0.3f;
                    block._pangRemoveDelay = 1f;

                    block.isMatchedBomb = true;
                    targetBlockRainbow.isMatchedBomb = true;
                }
                else if (checkBomb > 0)
                {
                    block.bombType = BlockBombType.BOMB_X_BOMB;
                    RemoveMatchBombBlock(block, targetBlockBomb);

                    block.isMatchedBomb = true;
                    targetBlockBomb.isMatchedBomb = true;

                    block._pangAlphaDelay = 0.1f;
                    block._pangRemoveDelay = 0.5f;
                }
                else if (checkLine > 0)
                {
                    block.bombType = BlockBombType.BOMB_X_LINE;
                    RemoveMatchBombBlock(block, targetBlockLine);

                    block.isMatchedBomb = true;
                    targetBlockLine.isMatchedBomb = true;

                    block._pangAlphaDelay = 0.1f;
                    block._pangRemoveDelay = 0.5f;
                }
            }
            else if (block.bombType == BlockBombType.LINE_V || block.bombType == BlockBombType.LINE_H)
            {
                if (checkRainbow > 0)
                {
                    block.bombType = BlockBombType.RAINBOW_X_LINE;
                    block.isSkipDistroy = true;
                    block.rainbowColorType = targetBlockRainbow.colorType;
                    RemoveMatchBombBlock(block, targetBlockRainbow);
                    block._pangAlphaDelay = 0.3f;
                    block._pangRemoveDelay = 1f;

                    block.isMatchedBomb = true;
                    targetBlockRainbow.isMatchedBomb = true;
                }
                else if (checkBomb > 0)
                {
                    block.bombType = BlockBombType.BOMB_X_LINE;
                    RemoveMatchBombBlock(block, targetBlockBomb);

                    block.isMatchedBomb = true;
                    targetBlockBomb.isMatchedBomb = true;

                    block._pangAlphaDelay = 0.1f;
                    block._pangRemoveDelay = 0.5f;
                }
                else if (checkLine > 0)
                {
                    block.bombType = BlockBombType.LINE_X_LINE;
                    RemoveMatchBombBlock(block, targetBlockLine);

                    block.isMatchedBomb = true;
                    targetBlockLine.isMatchedBomb = true;

                    block._pangAlphaDelay = 0.1f;
                    block._pangRemoveDelay = 0.5f;
                }
            }

            if (hasCarpet == true)
            {
                Board tempBoard = PosHelper.GetBoard(block.indexX, block.indexY);
                tempBoard.MakeCarpet();
            }

            block.RemoveLinkerNoReset();
            //block.BlockPang(BlockBase.uniqueIndexCount);
            block.state = BlockState.PANG;
            block.pangIndex = BlockBase.uniqueIndexCount;
            block.Pang();
        }
        else
        {
            block.BlockPang(BlockBase.uniqueIndexCount);
        }
    }


    private void RemoveMatchBombBlock(BlockBase block, BlockBase targetBlockRainbow)
    {
        //매치되어 사라지는 폭탄 아래에 폭탄 영역 표시 출력
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(targetBlockRainbow.indexX, targetBlockRainbow.indexY, 0, 0, 1, _unique:BlockBase.uniqueIndexCount);

        targetBlockRainbow._pangAlphaDelay = 0.0f;
        targetBlockRainbow._pangRemoveDelay = 0.5f;

        targetBlockRainbow.TempBombType = targetBlockRainbow.bombType;
        targetBlockRainbow.bombType = BlockBombType.DUMMY;
        //targetBlockRainbow.pangIndex = BlockBase.uniqueIndexCount;
        targetBlockRainbow.BlockPang(BlockBase.uniqueIndexCount);

        targetBlockRainbow.Destroylinker();
        targetBlockRainbow._pangGatherEffect = block._transform.localPosition;

    }

    bool CheckGetNearAllBomb(BlockBase tempGetNearBlock)
    {
        if (tempGetNearBlock != null
            && !tempGetNearBlock.isCheckedMatchable
            && tempGetNearBlock.IsBombBlock()
            && tempGetNearBlock.IsCanPang())
        {
            if (tempGetNearBlock.blockDeco == null || tempGetNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }
        }
        return false;
    }

    void GetNearAllBomb(BlockBase block)  //주변에 모든 폭탄블럭 다찾기
    {
        block.isCheckedMatchable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (CheckGetNearAllBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (CheckGetNearAllBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (CheckGetNearAllBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (CheckGetNearAllBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            )
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            if (!block1.isCheckedMatchable)
            {
                GetNearAllBomb(block1);
            }
        }
    }

    bool checkGetNearBomb4D(BlockBase tempGetNearBlock)
    {
        if (tempGetNearBlock != null
            && tempGetNearBlock.IsBombBlock()
            && tempGetNearBlock.IsCanPang())
        {
            if (tempGetNearBlock.blockDeco == null || tempGetNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }
        }
        return false;
    }


    List<BlockBase> GetNearBomb4D(BlockBase block)
    {
        List<BlockBase> tempListBlock = new List<BlockBase>();
        
        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (checkGetNearBomb4D(getNearBlock)
            && PosHelper.GetBoard(block.indexX, block.indexY, 1, 0).HasDecoHideBlock() == false
            && PosHelper.GetBoard(block.indexX, block.indexY, 1, 0).HasDecoCoverBlock() == false
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempListBlock.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (checkGetNearBomb4D(getNearBlock)
            && PosHelper.GetBoard(block.indexX, block.indexY, -1, 0).HasDecoHideBlock() == false
            && PosHelper.GetBoard(block.indexX, block.indexY, -1, 0).HasDecoCoverBlock() == false
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempListBlock.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (checkGetNearBomb4D(getNearBlock)
            && PosHelper.GetBoard(block.indexX, block.indexY, 0, 1).HasDecoHideBlock() == false
            && PosHelper.GetBoard(block.indexX, block.indexY, 0, 1).HasDecoCoverBlock() == false
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempListBlock.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (checkGetNearBomb4D(getNearBlock)
            && PosHelper.GetBoard(block.indexX, block.indexY, 0, -1).HasDecoHideBlock() == false
            && PosHelper.GetBoard(block.indexX, block.indexY, 0, -1).HasDecoCoverBlock() == false
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            )
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempListBlock.Add(getNearBlock);
            }
        }

        return tempListBlock;
    }

    bool checkNearNormalBlock(BlockBase tempBlockA, BlockBase tempBlockB)
    {
        if (tempBlockB != null
            && !tempBlockB.isCheckedMatchable
            && !checkBlocks.Contains(tempBlockB)
            && tempBlockA.colorType == tempBlockB.colorType
            && tempBlockB.IsBombBlock() == false
            && tempBlockB.IsCanPang()
            && tempBlockB.IsCanLink()
            )
        {
            if(tempBlockB.blockDeco != null && tempBlockB.blockDeco.IsInterruptBlockSelect()) return false;

            return true;
        }
        return false;
    }


    void getNearNormalBlock(BlockBase block)
    {
        block.isCheckedMatchable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (checkNearNormalBlock(block, getNearBlock) && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0))
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (checkNearNormalBlock(block, getNearBlock) && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0))
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (checkNearNormalBlock(block, getNearBlock) && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1))
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (checkNearNormalBlock(block, getNearBlock) && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1))
        {
            checkBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            if (!block1.isCheckedMatchable)
                getNearNormalBlock(block1);            
        }
    }
    #endregion

    #region 링커

    public void DistroyAllLinker()
    {
        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlock(x, y);
                if (block != null)
                {
                    block.expectType = BlockBombType.NONE;
                    block.RemoveLinkerNoReset();
                }
            }
        }
    }

    float soundTimer = 0;
    int praiseIndex = 0;

    /// ////////////////////////////
    /// 링크랑 예상 폭탄 만들기
    /// ////////////////////////////
    public void SetBlockLink(bool SetForce = false)
    {
        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            if (SetForce == false)
            {
                DestroyLinkerNoReset();
                return;
            }
        }

        bool isBombCreate = false;

        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlock(x, y);
                if (block != null)
                    block.isCheckBombMakable = false;
            }
        }
        
        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null)
                {
                    if (!block.isCheckBombMakable && block.state == BlockState.WAIT)
                    {
                        if (toyBlastCheckMatchSpecialblock(block))
                        {
                            isBombCreate = true;
                        }
                    }
                    else if (!block.isCheckBombMakable && block.state == BlockState.MOVE)
                    {
                        block.isCheckBombMakable = true;
                        block.expectType = BlockBombType.NONE;
                       // block.RemoveLinkerNoReset(); 알아서 제거
                    }
                }
            }
        }
        
        //한턴에 한번만, 한상태에 한번만 
        if (isBombCreate && ManagerBlock.instance.BlockTime - soundTimer > 3f && ManagerTutorial._instance == null)
        {
            ManagerSound.AudioPlay((AudioInGame)((int)AudioInGame.PRAISE0 + praiseIndex));
            soundTimer = ManagerBlock.instance.BlockTime;

            praiseIndex++;
            if (praiseIndex >= 4)
            {
                praiseIndex = 0;
            }
        }
    }


    public void DestroyLinkerNoReset()
    {
        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlock(x, y);
                if (block != null)
                {
                    block.RemoveLinkerNoReset();
                }
            }
        }
    }




    //모험모드 연결 체크하기
    public int CheckAdventureSameBlockCount(BlockBase block)
    {
        List<BlockBase> TempBombcheckBlocks = new List<BlockBase>();
        TempBombcheckBlocks.Add(block);
        GetNearTempColor(block, TempBombcheckBlocks);
        
        return TempBombcheckBlocks.Count;
    }


    /// //////////////////////////
    //예상폭탄 체크하는 부분//
    /////////////////////////////
    public bool toyBlastCheckMatchSpecialblock(BlockBase block)
    {
        if (ManagerBlock.instance.isFeverTime() == true)
        {
            if (block.expectType != BlockBombType.NONE)
            {
                block.expectType = BlockBombType.NONE;
            }
            return false;
        }
        BlockBombType tempCreatItem = block.expectType;

        if (!block.IsCanLink())
        {
            return false;
        }

        if (block.blockDeco != null && block.blockDeco.IsInterruptBlockSelect())
            return false;

        List<BlockBase> TempBombcheckBlocks = new List<BlockBase>();

        bool isBombCreate = false;
        TempBombcheckBlocks.Add(block);

        //컬러타입이거나
        if (!block.IsBombBlock() && block.colorType != BlockColorType.NONE && block.state == BlockState.WAIT)
        {
            GetNearTempColor(block, TempBombcheckBlocks);

            int tempCount = 0;
            foreach(var tempBlock in TempBombcheckBlocks)
            {
                if (tempBlock.IsNormalBlock()) tempCount++;
            }

            BlockBombType creatItem = BlockBombType.NONE;
            if (tempCount >= ManagerBlock.MAKE_RAINBOW_BLOCK_COUNT)     //TempBombcheckBlocks.Count
            {
                creatItem = BlockBombType.RAINBOW;
            }
            else if (tempCount >= ManagerBlock.MAKE_BOMB_BLOCK_COUNT)
            {
                creatItem = BlockBombType.BOMB;
            }
            else if (tempCount >= ManagerBlock.MAKE_LINE_BLOCK_COUNT)
            {  
                if (GameManager.instance.lineBombRotate)
                {
                    creatItem = ManagerBlock.instance.LineType;// == BlockBombType.LINE_H ? BlockBombType.LINE_V : BlockBombType.LINE_H;

                    if (tempCreatItem == BlockBombType.LINE_H || tempCreatItem == BlockBombType.LINE_V)
                        creatItem = tempCreatItem;
                }
                else
                {
                    int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                    creatItem = randomLine > 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H; //ManagerBlock.instance.LineType;

                    if (tempCreatItem == BlockBombType.LINE_H || tempCreatItem == BlockBombType.LINE_V)
                        creatItem = tempCreatItem;
                }
            }

            if(creatItem != BlockBombType.NONE)
            {
                isBombCreate = true;
            }
            /*
            if (GameManager.gameMode == GameMode.ADVENTURE && 
                GameManager.adventureMode != AdventureMode.ORIGIN &&
                tempCount >= ManagerBlock.ADVENTURE_PANG_SAME_COLOR_COUNT)
            {
                creatItem = BlockBombType.NONE;

                if (ManagerBlock.instance.AdventureMatchBlock)
                {
                    ManagerBlock.instance.comboCount++;

                    float posX = 0f;
                    float posY = 0f;

                    foreach (var tempBlock in TempBombcheckBlocks)
                    {
                        posX += tempBlock._transform.position.x;
                        posY += tempBlock._transform.position.y;                        
                    }
                    Vector3 startPos = new Vector3(posX / TempBombcheckBlocks.Count, posY / TempBombcheckBlocks.Count, 0);
                    InGameEffectMaker.instance.MakeAdventureCombo(startPos, "Combo " + ManagerBlock.instance.comboCount.ToString());// + new Vector3(0, -0.06f, 0)

                    //if (GameManager.adventureMode != AdventureMode.ORIGIN)
                    //    AdventureManager.instance.GetAdventureGaige(3);

                    CheckMatchPangBlock(block, false);
                    ManagerBlock.instance.AdventureMatchBlock = false;
                }
            }


            if (creatItem != BlockBombType.NONE)
            {
                foreach (var tempBlock in TempBombcheckBlocks)
                {
                    if (tempBlock.IsNormalBlock())
                    {
                        NormalBlock tempBlockA = tempBlock as NormalBlock;
                        if (tempBlockA != null && tempBlockA.IsAutoBlock)
                        {
                            CheckMatchPangBlock(tempBlock, false);
                            ManagerBlock.instance.AdventureMatchBlock = false;
                            ManagerBlock.instance.waitComboTimer = 0f;
                            break;
                        }
                    }
                }
            }
            */

            for (int i = 0; i < TempBombcheckBlocks.Count; i++)
            {
                TempBombcheckBlocks[i].expectType = creatItem;
            }
        }
        //폭탄이거나
        else if (block.IsBombBlock())
        {
            GetNearTempBomb(block, TempBombcheckBlocks);
        }


        if (TempBombcheckBlocks.Count > 1)
        {
            for (int i = 0; i < TempBombcheckBlocks.Count; i++)
            {
                TempBombcheckBlocks[i].MakeLinkerByManager();
            }
        }

        TempBombcheckBlocks.Clear();
        return isBombCreate;
    }

    bool CheckGetNearTempBomb(BlockBase getNearBlock)
    {
        if (getNearBlock != null
            && getNearBlock.state == BlockState.WAIT
            && !getNearBlock.isCheckBombMakable
            && getNearBlock.IsBombBlock())
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }  
        }
        return false;
    }

    void GetNearTempBomb(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        block.isCheckBombMakable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (CheckGetNearTempBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (CheckGetNearTempBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
            {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);
        if (CheckGetNearTempBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && !tempCheckBlocks.Contains(getNearBlock))
            {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);
        if (CheckGetNearTempBomb(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        foreach (var block1 in tempCheckBlockList)
        {
            if (!block1.isCheckBombMakable)
            {
                GetNearTempBomb(block1, tempCheckBlocks);
            }
        }
    }

    bool CheckGetNearTempColor(BlockBase tempBlock)
    {
        if (tempBlock != null
            && tempBlock.state == BlockState.WAIT
            && tempBlock.isCheckBombMakable == false
            && tempBlock.IsCanLink()
            && tempBlock.TempBombType == BlockBombType.NONE
            && tempBlock.IsBombBlock() == false
            )
        {
            if (tempBlock.blockDeco == null || tempBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }  
        }
        return false;
    }

    void GetNearTempColor(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        block.isCheckBombMakable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        
        if (CheckGetNearTempColor(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);
        if (CheckGetNearTempColor(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (CheckGetNearTempColor(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if(CheckGetNearTempColor(getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && tempCheckBlocks.Contains(getNearBlock) == false
            && block.colorType == getNearBlock.colorType)
        {
            if (getNearBlock.blockDeco == null || getNearBlock.blockDeco.IsInterruptBlockSelect() == false)
            {
                tempCheckBlocks.Add(getNearBlock);
                tempCheckBlockList.Add(getNearBlock);
            }
        }

        foreach (var block1 in tempCheckBlockList)
        {
            if (!block1.isCheckBombMakable)
            {
                GetNearTempColor(block1, tempCheckBlocks);
            }
        }
    }


    public void CheckBlockLinkToItem(BlockBase blockA)
    {
        if(GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
            return;
        
        if (PosHelper.GetBlockScreen(blockA.indexX, blockA.indexY) == null)
            return;

        if (blockA.state == BlockState.PANG)
            return;

        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlock(x, y);
                if (block != null)
                {
                    block.isCheckBombMakable = false;
                }
            }
        }

        if (blockA.IsCanLink())
            toyBlastCheckMatchSpecialblock(blockA);//toyBlastCheckMatchSpecialblock//FindSameBlockToMakeLinker
    }

    /// ///////////////////////
    /// /////////////////////////
    /// 개별찾기
    /// ////////////////////////
    /*
    public void FindSameBlockToMakeLinker(BlockBase blockA)
    {
        BlockBombType creatItem = BlockBombType.NONE;

        List<BlockBase> TempBombCheckSameBlocks = new List<BlockBase>();
        TempBombCheckSameBlocks.Add(blockA);

        //컬러타입이거나
        if (!blockA.IsBombBlock() && blockA.colorType != BlockColorType.NONE)
        {
            GetNearSameColor(blockA, TempBombCheckSameBlocks);

            if (TempBombCheckSameBlocks.Count >= ManagerBlock.MAKE_RAINBOW_BLOCK_COUNT)
            {
                creatItem = BlockBombType.RAINBOW;
            }
            else if (TempBombCheckSameBlocks.Count >= ManagerBlock.MAKE_BOMB_BLOCK_COUNT)
            {
                creatItem = BlockBombType.BOMB;
            }
            else if (TempBombCheckSameBlocks.Count >= ManagerBlock.MAKE_LINE_BLOCK_COUNT)
            {
                creatItem = ManagerBlock.instance.LineType;
            }

            for (int i = 0; i < TempBombCheckSameBlocks.Count; i++)
            {
                TempBombCheckSameBlocks[i].expectType = creatItem;
            }
        }
        else
        {
            GetNearBombs(blockA, TempBombCheckSameBlocks);
        }
        for (int i = 0; i < TempBombCheckSameBlocks.Count; i++)
        {

            TempBombCheckSameBlocks[i].MakeLinkerByManager();
        }
        TempBombCheckSameBlocks.Clear();
    }
    */

    bool checkSameType(BlockBase blockA , BlockBase blockB)
    {
        if (blockB != null
            && blockB.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT      //
            && blockB.colorType == blockA.colorType
            && blockB.IsCanLink()
            && blockB.isCheckBombMakable == false
            && blockB.IsBombBlock() == false
            )
        {
            if (blockB.blockDeco == null || blockB.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }            
        }

        return false;
    }

    void GetNearSameColor(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        block.isCheckBombMakable = true;
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (checkSameType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (checkSameType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (checkSameType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (checkSameType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var blockA in tempCheckBlockList)
        {
            GetNearSameColor(blockA, tempCheckBlocks);
        }
    }

    bool checkBombType(BlockBase blockA, BlockBase blockB)
    {
        if (blockB != null
            && blockB.state == BlockState.WAIT//!= BlockState.PANG // state == BlockState.WAIT
            && blockB.IsCanLink()
            && blockB.IsBombBlock()
            )
        {
            if (blockB.blockDeco == null || blockB.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }
        }

        return false;
    }

    void GetNearBombs(BlockBase block, List<BlockBase> tempCheckBlocks)
    {
        List<BlockBase> tempCheckBlockList = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);

        if (checkBombType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, -1, 0);

        if (checkBombType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, -1, 0)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, 1);

        if (checkBombType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, 1)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 0, -1);

        if (checkBombType(block, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(block.indexX, block.indexY, 0, -1)
            && !tempCheckBlocks.Contains(getNearBlock))
        {
            tempCheckBlocks.Add(getNearBlock);
            tempCheckBlockList.Add(getNearBlock);
        }

        foreach (var block1 in tempCheckBlockList)
        {
            GetNearTempBomb(block1, tempCheckBlocks);
        }
    }
    #endregion



    #region 모험모드용

    public bool CheckMatchableBlock()
    {
        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && CheckAdventureMatchableBlock(block))                                   
                    return true; 
            }
        }

        return false;
    }

    public bool checkEmptyBlock()
    {
        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                Board tempBoard = PosHelper.GetBoardSreeen(x, y);
                if (tempBoard != null && tempBoard.IsActiveBoard && tempBoard.Block == null)                
                    return false;                
            }
        }
        return true;
    }

    public bool CheckAdventureMatchableBlock(BlockBase block)
    {
        if (!block.IsCanLink())
        {
            return false;
        }

        if (block.state != BlockState.WAIT)
            return false;

        if (block.blockDeco != null && block.blockDeco.IsInterruptBlockSelect())
            return false;

        List<BlockBase> TempBombcheckBlocks = new List<BlockBase>();
        TempBombcheckBlocks.Add(block);


        if (!block.IsBombBlock() && block.colorType != BlockColorType.NONE)
        {
            GetNearTempColor(block, TempBombcheckBlocks);

            if (TempBombcheckBlocks.Count >= ManagerBlock.ADVENTURE_PANG_SAME_COLOR_COUNT)            
                return true;            
        }
        return false;
    }


    #endregion


    #region 2X2블럭 관련

    public void Check2X2Block()
    {
        return;
        //블럭체크
        List<BlockBase> checkList2X2 = new List<BlockBase>();
        List<Block2X2> list2X2Block = new List<Block2X2>();

        for (int y = GameManager.MAX_Y - 1; y >= GameManager.MIN_Y; y--)
        {
            for (int x = GameManager.MAX_X - 1; x >= GameManager.MIN_X; x--)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null)
                {
                    if(check2x2BoardType(block, checkList2X2))
                    {
                        //2x2블럭만들기
                        BlockBase tempBlock = BlockMaker.instance.MakeBlockBase(x, y, BlockType.BLOCK2X2, block.colorType);
                        Block2X2 temp2x2Block = tempBlock as Block2X2;
                        list2X2Block.Add(temp2x2Block);
                    
                        foreach(var checkBlock in checkList2X2)
                        {
                            ManagerBlock.boards[checkBlock.indexX, checkBlock.indexY].Block = tempBlock;
                            ManagerBlock.boards[checkBlock.indexX, checkBlock.indexY].TempBlock = null;
                            //Board tempBoard = PosHelper.GetBoard(checkBlock.indexX, checkBlock.indexY);
                            //tempBoard.Block = tempBlock;
                            BlockBase._listBlock.Remove(checkBlock);
                            BlockBase._listBlockTemp.Remove(checkBlock);
                            BlockMaker.instance.RemoveBlock(checkBlock);

                            temp2x2Block.blockBoardList.Add(ManagerBlock.boards[checkBlock.indexX, checkBlock.indexY]);
                        }

                        temp2x2Block.initBlock();
                    }
                }
            }
        }

        foreach (var block in list2X2Block)
        {
            //2x2블럭제거하기
          //  block.BlockPang(BlockBase.uniqueIndexCount++);
        }

    }

    bool check2x2SameType(BlockBase blockA, BlockBase blockB)
    {
        if (blockB != null
            && blockB.state == BlockState.WAIT//!= BlockState.PANG //== BlockState.WAIT      //
            && blockB.colorType == blockA.colorType
            && blockB.IsCanLink()
            && blockB.IsBombBlock() == false
            )
        {
            if (blockB.blockDeco == null || blockB.blockDeco.IsInterruptBlockSelect() == false)
            {
                return true;
            }
        }

        return false;
    }

    public bool check2x2BoardType(BlockBase tempBlock, List<BlockBase> tempCheckBlockList)
    {
        if (tempBlock.type == BlockType.BLOCK2X2) return false;

        tempCheckBlockList.Clear();
        tempCheckBlockList.Add(tempBlock);


        BlockBase getNearBlock = PosHelper.GetBlockScreen(tempBlock.indexX, tempBlock.indexY, 1, 0);
        if (check2x2SameType(tempBlock, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(tempBlock.indexX, tempBlock.indexY, 1, 0))
        {
            tempCheckBlockList.Add(getNearBlock);
        }
        else
        {
            return false;
        }

        getNearBlock = PosHelper.GetBlockScreen(tempBlock.indexX, tempBlock.indexY, 0, 1);

        if (check2x2SameType(tempBlock, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(tempBlock.indexX, tempBlock.indexY, 0, 1))
        {
            tempCheckBlockList.Add(getNearBlock);
        }
        else
        {
            return false;
        }

        getNearBlock = PosHelper.GetBlockScreen(tempBlock.indexX, tempBlock.indexY, 1, 1);

        if (check2x2SameType(tempBlock, getNearBlock)
            && ManagerBlock.instance.IsCanLinkSideBLock(tempBlock.indexX, tempBlock.indexY, 1, 1))
        {
            tempCheckBlockList.Add(getNearBlock);
        }
        else
        {
            return false;
        }

        if (tempCheckBlockList.Count == 4) return true;

        return false;
    }


    #endregion
}
