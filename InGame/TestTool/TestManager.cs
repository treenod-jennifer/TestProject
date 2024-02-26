using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TEST_STATE
{
    NONE,
    TEST,
    WAIT
}

public class TestManager : MonoBehaviour {

    public static TestManager instance = null;

    void Awake()
    {
        instance = this;
    }

    const int APPLE_VALUE = 1000;   //사과획득
    const int RAINBOWXBOMB = 500;       //레인보우조합
    const int GOAL_VALUE = 100;     //목표획득 //

    const int COLOR_COLLECT_VALUE = 20;

    const int NEAR_BOMB_CROSS_POSITION = 200;   //폭탄조합이 생기는 위치
    const int EMPTY_SPACE = 200;

    const int NET_VALUE = 50;       //목표가 아래있을때
    const int START_UNDER_VALUE = 40;
    const int KEY_UNDERBLOKC_VALUE = 30;
    const int NORMAL_GIMIK_VALUE = 20;

    const int MAKE_BOMBXBOMB = 200;
    const int MAKE_RAINBOW = 250;
    const int MAKE_BOMB = 40;
    const int MAKE_LINE = 20;

    const int NORMAL_BLOCK = 1;

    const int NEAR_GOAL = 3;
    const int NEAR_CROSS_GOAL = 2;

    public TEST_STATE state = TEST_STATE.NONE;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if(state != TEST_STATE.TEST)
            {
                state = TEST_STATE.TEST;
            }
            else
            {
                state = TEST_STATE.NONE;
            }
        }


        switch (state)
        {
            case TEST_STATE.NONE:
                break;
            case TEST_STATE.TEST:

                //게임클리어면 멈추기
                //실패도 멈추기
                if (GameManager.instance.state != GameState.PLAY)
                {
                    state = TEST_STATE.NONE;
                    return;
                }
                if (GameManager.instance.checkAllRemoveTarget())
                {
                    state = TEST_STATE.NONE;
                    return;
                }

                if (GameManager.instance.IsCanTouch == false) return;
                if (GameManager.instance.state != GameState.PLAY) return;
                if (GameManager.gameMode == GameMode.NORMAL && GameManager.instance.moveCount == 0) return;
                if (GameManager.gameMode == GameMode.DIG && GameManager.instance.moveCount == 0) return;
                if (ManagerBlock.instance.waitBlockToFinishMoving && ManagerBlock.instance.state != BlockManagrState.WAIT) return;
                if (ManagerBlock.instance.stageInfo.waitState == 1 && ManagerBlock.instance.state != BlockManagrState.WAIT) return;
                if (ManagerBlock.instance.movePanelPause) return;
                if (GameItemManager.instance != null && GameItemManager.instance.used == true) return;

                if (moveCount == GameManager.instance.moveCount) return;
                moveCount = GameManager.instance.moveCount;

                int tempPoint = 0;
                Board tempBlock = null;

                for (int i = GameManager.MaxScreenY; i >= GameManager.MinScreenY; i--)
                {
                    for (int j = GameManager.MinScreenX; j <= GameManager.MaxScreenX; j++)
                    {
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        int tempBackPoint = ExpectBlockPoint(back);

                        if (tempBackPoint > tempPoint)
                        {
                            tempPoint = tempBackPoint;
                            tempBlock = back;
                        }
                    }
                }

                if (tempBlock != null)
                {
                    Debug.Log("포인트"+ tempPoint);
                    GameManager.instance.RemoveTurn();
                    BlockMatchManager.instance.CheckMatchPangBlock(tempBlock.Block);
                }
                else
                {
                    Debug.Log("보드없음");
                    state = TEST_STATE.NONE;
                }

                break;
            case TEST_STATE.WAIT:
                break;             
        }        
    }

    int moveCount = 100;   

    private List<BlockBase> checkBlocks = new List<BlockBase>();

    //선택블럭의 예상점수
    int ExpectBlockPoint(Board tempBoard)
    {
        if (tempBoard == null) return 0;
        if (tempBoard.BoardOnNet.Count > 0)return 0;  
        if (tempBoard.Block == null) return 0;
        if (BlockMatchManager.instance.checkBlockToyBlastMatch(tempBoard.Block) == false) return 0;

        checkBlocks = new List<BlockBase>();
        int tempPoint = 0;

        
        if (tempBoard.Block.IsBombBlock())
        {
            tempPoint += checkBombBlock(tempBoard.Block);
        }
        else if (tempBoard.Block.IsNormalBlock())
        {
            CheckNormalBlock(tempBoard.Block);

            if (checkBlocks.Count >= ManagerBlock.MAKE_RAINBOW_BLOCK_COUNT) tempPoint += MAKE_RAINBOW;
            else if (checkBlocks.Count >= ManagerBlock.MAKE_BOMB_BLOCK_COUNT) tempPoint += MAKE_BOMB;
            else if (checkBlocks.Count >= ManagerBlock.MAKE_LINE_BLOCK_COUNT)
            {
                tempPoint += MAKE_LINE;
                tempPoint += GetLineVValue(tempBoard);
            }

            if (checkBlocks.Count >= ManagerBlock.MAKE_LINE_BLOCK_COUNT)
            {
                tempPoint += checkMakeBombPosition(tempBoard);              //밑이나 아래에 폭탄이 있으면 NEAR_BOMB_CROSS_POSITION
            }
            tempPoint += NORMAL_BLOCK * checkBlocks.Count;
        }

        foreach (var item in checkBlocks)tempPoint += GetBoardValue(PosHelper.GetBoardSreeen(item.indexX, item.indexY));

        for (int i = 0; i < checkBlocks.Count; i++)checkBlocks[i].isCheckedMatchable = false;

        checkBlocks.Clear();
        return tempPoint;
    }

    //라인폭탄을 만들때 변수
    int GetLineVValue(Board tempBoard)
    {
        int tempCount = 1;
        int boardValue = 0;

        if(ManagerBlock.instance.LineType == BlockBombType.LINE_V)
        {
            while (tempCount < 11)
            {
                Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempCount);

                if (temp != null && temp.Block != null && temp.Block.type == BlockType.KEY)
                {
                    boardValue += GOAL_VALUE;
                }

                if (temp.BoardOnCrack.Count > 0)
                    boardValue += NET_VALUE;

                if (temp.BoardOnGrass.Count > 0 && temp.BoardOnIStatue.Count > 0)
                {
                    boardValue += GOAL_VALUE;
                }

                tempCount++;
            }
        }
        else
        {
            //아래에는 
            if (tempBoard.Block.colorType != PosHelper.GetBlockScreen(tempBoard.indexX, tempBoard.indexY, 0, 1).colorType)
            {
                for (int j = GameManager.MinScreenX; j <= GameManager.MaxScreenX; j++)
                {
                    Board back = PosHelper.GetBoardSreeen(j, tempBoard.indexY);

                    if (back != null && back.Block != null && back.Block.isCheckedMatchable == false)
                    {
                        boardValue += GetBoardValue(PosHelper.GetBoardSreeen(back.indexX, back.indexY));
                    }
                }
            }
        }

        return boardValue;
    }

    /// 보드하나의 점수
    int GetBoardValue(Board tempBoard)
    {
        int boardValue = 0;

        if (tempBoard.BoardOnNet.Count > 0)
        {
            if (tempBoard.BoardOnCrack.Count > 0)
                boardValue += NET_VALUE;
            else if(tempBoard.BoardOnGrass.Count > 0 &&  tempBoard.BoardOnIStatue.Count > 0)
                boardValue += NET_VALUE;
            
            boardValue += checkTargetBlock(tempBoard.Block);
        }
        else
        {
            if (tempBoard.BoardOnCrack.Count > 0)
                boardValue += GOAL_VALUE;
            else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                boardValue += GOAL_VALUE;

            boardValue += checkTargetBlock(tempBoard.Block);
        }

        if(tempBoard.Block != null && tempBoard.Block is BlockPlant)
        {
            int tempCount = 1;

            while (tempCount < 11)
            {
                Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY + tempCount);

                if (temp != null)
                {
                    if (temp.BoardOnCrack.Count > 0)
                        boardValue += EMPTY_SPACE;
                    else if (temp.BoardOnGrass.Count > 0 && temp.BoardOnIStatue.Count > 0)
                        boardValue += EMPTY_SPACE;
                }
                else
                {
                    break;
                }

                tempCount++;
            }
        }


        return boardValue;
    }

    int checkMakeBombPosition(Board tempBoard)
    {
        int tempPoint = 0;
        int tempCount = 1;

        while (tempBoard.indexY + tempCount < GameManager.MAX_Y)
        {
            Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY + tempCount);
            if (temp == null) break;

            if(temp.Block != null && temp.Block.isCheckedMatchable == false && temp.Block.IsBombBlock())
            {

                if (temp.Block.bombType == BlockBombType.RAINBOW)
                    tempPoint += RAINBOWXBOMB;
                else
                   tempPoint += NEAR_BOMB_CROSS_POSITION;

                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX, temp.indexY + 1));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX, temp.indexY - 1));

                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY + 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY + 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY - 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY - 1));

                return tempPoint;
            }
            tempCount++;
        }

        tempPoint = 0;
        tempCount = 1;

        while (tempBoard.indexY - tempCount > GameManager.MIN_Y)
        {
            Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - tempCount);
            if (temp == null) break;

            if (temp.Block != null && temp.Block.isCheckedMatchable == false && temp.Block.IsBombBlock())
            {
                if (temp.Block.bombType == BlockBombType.RAINBOW)
                    tempPoint += RAINBOWXBOMB;
                else
                    tempPoint += NEAR_BOMB_CROSS_POSITION;

                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX, temp.indexY + 1));
                tempPoint += checkSplashValue(PosHelper.GetBoardSreeen(temp.indexX, temp.indexY - 1));

                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY + 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY + 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX + 1, temp.indexY - 1));
                tempPoint += checkNearValue(PosHelper.GetBoardSreeen(temp.indexX - 1, temp.indexY - 1));

                return tempPoint;
            }
            else if (temp.Block != null && temp.Block.isCheckedMatchable == false && temp.Block.expectType != BlockBombType.NONE)
            {
                if (temp.Block.bombType == BlockBombType.RAINBOW)
                    tempPoint += RAINBOWXBOMB;
                else
                    tempPoint += NEAR_BOMB_CROSS_POSITION;

                return tempPoint;
            }
            tempCount--;
        }
        return tempPoint;
    }


    //블럭자체점수체크
    int checkTargetBlock(BlockBase tempBlock)
    {
        if (tempBlock == null) return 0;

        int boardValue = 0;
        //주변블럭체크

        if (PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY - 1) != null
            && PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY - 1).Block != null
            && PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY - 1).Block.type == BlockType.KEY)
            boardValue += GOAL_VALUE;

        boardValue += checkSplashValue(PosHelper.GetBoardSreeen(tempBlock.indexX+1, tempBlock.indexY));
        boardValue += checkSplashValue(PosHelper.GetBoardSreeen(tempBlock.indexX-1, tempBlock.indexY));
        boardValue += checkSplashValue(PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY+1));
        boardValue += checkSplashValue(PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY-1));

        boardValue += checkNearValue(PosHelper.GetBoardSreeen(tempBlock.indexX + 1, tempBlock.indexY +1));
        boardValue += checkNearValue(PosHelper.GetBoardSreeen(tempBlock.indexX - 1, tempBlock.indexY +1));
        boardValue += checkNearValue(PosHelper.GetBoardSreeen(tempBlock.indexX +1, tempBlock.indexY - 1));
        boardValue += checkNearValue(PosHelper.GetBoardSreeen(tempBlock.indexX-1, tempBlock.indexY - 1));
        return boardValue;
    }


    int checkNearValue(Board tempBoard)
    {
        if (tempBoard == null) return 0;

        if (tempBoard.Block != null && tempBoard.Block.isCheckedMatchable == false)
        {
            if (tempBoard.Block is BlockGround)
            {
                BlockGround ground = tempBoard.Block as BlockGround;
                if (ground.groundType == GROUND_TYPE.JEWEL) return NEAR_GOAL;
                if (ground.groundType == GROUND_TYPE.KEY) return NEAR_GOAL;
            }

            if (tempBoard.Block is BlockPlant)
            {
                if (tempBoard.BoardOnCrack.Count > 0)
                    return NEAR_GOAL;
                else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                    return NEAR_GOAL;

                return NEAR_GOAL;
            }

            if (tempBoard.Block is BlockStone && tempBoard.Block.lifeCount == 1)
            {
                if (tempBoard.BoardOnCrack.Count > 0)
                    return NEAR_GOAL;
                else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                    return NEAR_GOAL;

                return NEAR_GOAL;
            }

            //주변에 목표가 있다
            if (tempBoard.BoardOnCrack.Count > 0)
            {
                return NEAR_CROSS_GOAL;
            }
        }
        else
        {
            //주변에 목표가 있다
            if (tempBoard.BoardOnCrack.Count > 0)
            {
                return NEAR_CROSS_GOAL;
            }
        }

        return 0;
    }

    int checkSplashValue(Board tempBoard)
    {
        if (tempBoard == null) return 0;

        if(tempBoard.Block != null && tempBoard.Block.isCheckedMatchable == false)
        {
            if (tempBoard.Block is BlockApple)
            {
                BlockApple apple = tempBoard.Block as BlockApple;
                if (apple.AppleCount > 1) return APPLE_VALUE;
            }

            if (tempBoard.Block is BlockGround)
            {
                BlockGround ground = tempBoard.Block as BlockGround;
                if (ground.groundType == GROUND_TYPE.JEWEL) return GOAL_VALUE;
                if (ground.groundType == GROUND_TYPE.KEY) return GOAL_VALUE;


                if (PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1) != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block.type == BlockType.KEY)
                    return GOAL_VALUE;
            }

            if (tempBoard.Block is BlockPlant)
            {
                BlockPlant ground = tempBoard.Block as BlockPlant;
                if (ground.plantType == PLANT_TYPE.KEY) return GOAL_VALUE;

                if (PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1) != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block.type == BlockType.KEY)
                    return GOAL_VALUE;

                if (tempBoard.BoardOnCrack.Count > 0)
                    return NET_VALUE;
                else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                    return NET_VALUE;

                return NORMAL_GIMIK_VALUE;
            }

            if (tempBoard.Block is BlockBox)
            {
                BlockBox ground = tempBoard.Block as BlockBox;

                if (tempBoard.BoardOnCrack.Count > 0)
                    return NET_VALUE;
                else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                    return NET_VALUE;

                if (PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1) != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block.type == BlockType.KEY)
                    return GOAL_VALUE;

                return NORMAL_GIMIK_VALUE;
            }

            if (tempBoard.Block is BlockStone && tempBoard.Block.lifeCount == 1)
            {
                BlockStone ground = tempBoard.Block as BlockStone;
                if (ground.stoneType == STONE_TYPE.KEY) return GOAL_VALUE;

                if (PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1) != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block != null
                    && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block.type == BlockType.KEY)
                    return GOAL_VALUE;

                if (tempBoard.BoardOnCrack.Count > 0)
                    return NET_VALUE;
                else if (tempBoard.BoardOnGrass.Count > 0 && tempBoard.BoardOnIStatue.Count > 0)
                    return NET_VALUE;

                return NORMAL_GIMIK_VALUE;
            }

            if (PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1) != null
                && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block != null
                && PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - 1).Block.type == BlockType.KEY)
                return GOAL_VALUE;


            //주변에 목표가 있다
            if (tempBoard.BoardOnCrack.Count > 0)
            {
                Debug.Log("주변목표");
                return NEAR_GOAL;
            }
        }
        else
        {
            //주변에 목표가 있다
            if (tempBoard.BoardOnCrack.Count > 0)
            {
                Debug.Log("주변목표");
                return NEAR_GOAL;
            }
        }

        return 0;
    }

    //일반블럭
    void CheckNormalBlock(BlockBase block)
    {
        checkBlocks.Add(block);
        getNearNormalBlock(block);
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
            if (!block1.isCheckedMatchable)getNearNormalBlock(block1);
        }
    }
    bool checkNearNormalBlock(BlockBase tempBlockA, BlockBase tempBlockB)
    {
        if (tempBlockB != null
            && !tempBlockB.isCheckedMatchable
            && !checkBlocks.Contains(tempBlockB)
            && tempBlockA.colorType == tempBlockB.colorType
            && tempBlockB.IsBombBlock() == false
            && tempBlockB.IsCanPang()
            )
        {
            if (tempBlockB.blockDeco != null && tempBlockB.blockDeco.IsInterruptBlockSelect()) return false;
            return true;
        }
        return false;
    }

    //예상매치링크만들기
    int checkBombBlock(BlockBase block)
    {
        checkBlocks.Clear();
        checkBlocks = GetNearBomb4D(block);
        List<BlockBase> checkTempBlocks = new List<BlockBase>();

        if (checkBlocks.Count > 0)
        {
            int checkRainbow = 0;
            int checkBomb = 0;
            int checkLine = 0;
            BlockColorType rainbowBlockType = BlockColorType.NONE;
            BlockBombType tempBombType = BlockBombType.NONE;
            BlockBase targetBlockRainbow = null;
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
                }
                if (checkBlock.bombType == BlockBombType.LINE_V || checkBlock.bombType == BlockBombType.LINE_H)
                {
                    checkLine++;
                }
            }

            if (block.bombType == BlockBombType.RAINBOW)
            {
                if (checkRainbow > 0)
                {
                    tempBombType = BlockBombType.RAINBOW_X_RAINBOW;
                }
                else if (checkBomb > 0)
                {
                    tempBombType = BlockBombType.RAINBOW_X_BOMB;
                }
                else if (checkLine > 0)
                {
                    tempBombType = BlockBombType.RAINBOW_X_LINE;
                }
                else
                {
                    tempBombType = BlockBombType.RAINBOW;
                }
            }
            else if (block.bombType == BlockBombType.BOMB)
            {
                if (checkRainbow > 0)
                {
                    tempBombType = BlockBombType.RAINBOW_X_BOMB;
                    rainbowBlockType = targetBlockRainbow.colorType;
                }
                else if (checkBomb > 0)
                {
                    tempBombType = BlockBombType.BOMB_X_BOMB;

                }
                else if (checkLine > 0)
                {
                    tempBombType = BlockBombType.BOMB_X_LINE;
                }
                else
                {
                    tempBombType = BlockBombType.BOMB;
                }
            }
            else if (block.bombType == BlockBombType.LINE_V || block.bombType == BlockBombType.LINE_H)
            {
                if (checkRainbow > 0)
                {
                    tempBombType = BlockBombType.RAINBOW_X_LINE;
                    rainbowBlockType = targetBlockRainbow.colorType;
                }
                else if (checkBomb > 0)
                {
                    tempBombType = BlockBombType.BOMB_X_LINE;
                }
                else if (checkLine > 0)
                {
                    tempBombType = BlockBombType.LINE_X_LINE;
                }
                else
                {
                    tempBombType = block.bombType;
                }
            }



            switch (tempBombType)
            {
                case BlockBombType.RAINBOW_X_RAINBOW:
                case BlockBombType.RAINBOW_X_BOMB:
                case BlockBombType.RAINBOW_X_LINE:
                    return RAINBOWXBOMB;
                case BlockBombType.BOMB_X_BOMB:
                    return MAKE_BOMBXBOMB;

                case BlockBombType.BOMB_X_LINE:

                    int ValueBOMB_X_LINE = 0;

                    for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(block.indexX, i);
                        Board back = PosHelper.GetBoardSreeen(block.indexX, i);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }
                    for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(block.indexX-1, i);
                        Board back = PosHelper.GetBoardSreeen(block.indexX-1, i);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }
                    for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(block.indexX + 1, i);
                        Board back = PosHelper.GetBoardSreeen(block.indexX + 1, i);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }


                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, block.indexY);
                        Board back = PosHelper.GetBoardSreeen(j, block.indexY);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, block.indexY+1);
                        Board back = PosHelper.GetBoardSreeen(j, block.indexY+1);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, block.indexY-1);
                        Board back = PosHelper.GetBoardSreeen(j, block.indexY-1);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueBOMB_X_LINE += GetBoardValue(back);
                        }
                    }

                    foreach (var temp in checkTempBlocks) temp.isCheckedMatchable = false;

                    return ValueBOMB_X_LINE;

                case BlockBombType.LINE_X_LINE:
                    int ValueLINE_X_LINE = 0;

                    for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(block.indexX, i);
                        Board back = PosHelper.GetBoardSreeen(block.indexX, i);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueLINE_X_LINE += GetBoardValue(back);
                        }
                    }

                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, block.indexY);
                        Board back = PosHelper.GetBoardSreeen(j, block.indexY);
                        if (backBlock != null && backBlock.isCheckedMatchable == false)
                        {
                            backBlock.isCheckedMatchable = true;
                            checkTempBlocks.Add(backBlock);
                            ValueLINE_X_LINE += GetBoardValue(back);
                        }
                    }

                    foreach (var temp in checkTempBlocks) temp.isCheckedMatchable = false;

                    return ValueLINE_X_LINE;
            }
        }

        

        int rainbowValue = 0;

        switch (block.bombType)
        {
            case BlockBombType.RAINBOW:

                if (CanMakeBombCross(block)) return 0;

                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {
                    for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, i);
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (backBlock != null && backBlock.colorType == backBlock.colorType)
                        {
                            rainbowValue+=GetBoardValue(back);
                        }
                    }
                }


                return rainbowValue;

            case BlockBombType.BOMB:
                if (CanMakeBombCross(block)) return 0;

                for (int i = block.indexY -1; i < block.indexY+2; i++)
                {
                    for (int j = block.indexX-1; j < block.indexX+2; j++)
                    {
                        BlockBase backBlock = PosHelper.GetBlockScreen(j, i);
                        Board back = PosHelper.GetBoardSreeen(j, i);
                        if (backBlock != null)
                        {
                            rainbowValue += GetBoardValue(back);
                        }
                    }
                }
                return rainbowValue;

            case BlockBombType.LINE_V:

                if (CanMakeBombCross(block)) return 0;
                for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                {                    
                        BlockBase backBlock = PosHelper.GetBlockScreen(block.indexX, i);
                        Board back = PosHelper.GetBoardSreeen(block.indexX, i);
                        if (backBlock != null)
                        {
                            rainbowValue += GetBoardValue(back);
                        }                    
                }
                return rainbowValue;
            case BlockBombType.LINE_H:

                if (CanMakeBombCross(block)) return 0;
                for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                {
                    BlockBase backBlock = PosHelper.GetBlockScreen(j,block.indexY);
                    Board back = PosHelper.GetBoardSreeen(j, block.indexY);
                    if (backBlock != null)
                    {
                        rainbowValue += GetBoardValue(back);
                    }
                }
                return rainbowValue;
        }


        return 0;
    }

    bool CanMakeBombCross(BlockBase tempBoard)
    {
        int tempCount = 1;

        while (tempBoard.indexY + tempCount < GameManager.MAX_Y)
        {
            Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY + tempCount);
            if (temp == null) break;

            if (temp.Block != null && temp.Block.expectType != BlockBombType.NONE)
            {
                return true;
            }
            tempCount--;
        }

        tempCount = 1;

        while (tempBoard.indexY - tempCount > GameManager.MIN_Y)
        {
            Board temp = PosHelper.GetBoardSreeen(tempBoard.indexX, tempBoard.indexY - tempCount);
            if (temp == null) break;

            if (temp.Block != null && temp.Block.expectType != BlockBombType.NONE)
            {
                return true;
            }

            tempCount--;
        }
        return false;
    }



    void CheckBombExpect(BlockBombType tempBombType, BlockBase tempBlock)
    {
        switch (tempBombType)
        {
            case BlockBombType.LINE_H:
                
                break;
        }
    }

    bool checkGetNearBomb4D(BlockBase tempGetNearBlock)
    {
        if (tempGetNearBlock != null
            && tempGetNearBlock.IsBombBlock()
            && tempGetNearBlock.IsCanPang())
        {
            return true;
        }
        return false;
    }

    List<BlockBase> GetNearBomb4D(BlockBase block)
    {
        List<BlockBase> tempListBlock = new List<BlockBase>();

        BlockBase getNearBlock = PosHelper.GetBlockScreen(block.indexX, block.indexY, 1, 0);
        if (checkGetNearBomb4D(getNearBlock)
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
}
