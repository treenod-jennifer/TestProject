using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosHelper : MonoBehaviour {

    static public float Block_OffsetX = 70f * -5.58f;
    static public float Block_OffsetY = -10f + 70f * 6.3f;

    //위치관련, 블럭, 보드 찾기
    public static Vector3 GetPosByBlock(BlockBase block)
    {
        return GetPosByIndex(block.indexX, block.indexY);
    }

    public static Vector3 GetPosByIndex(int inX, int inY)
    {
        //return new Vector3((inX - (GameManager.MAX_X-1)*0.5f)  * BlockManager.BLOCK_SIZE, (inY - (GameManager.MAX_Y-1) * 0.5f) * BlockManager.BLOCK_SIZE, 0);
        return new Vector3((inX * ManagerBlock.BLOCK_SIZE) + Block_OffsetX, (inY * -ManagerBlock.BLOCK_SIZE) + Block_OffsetY, 0f);
    }

    public static Board GetBoard(int inX, int inY, int offsetX = 0, int offsetY = 0)
    {
        inX += offsetX;
        inY += offsetY;
        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            return ManagerBlock.boards[inX, inY];
        }

        return null;
    }

    public static Board GetBoardSreeen(int inX, int inY, int offsetX = 0, int offsetY = 0)
    {
        inX += offsetX;
        inY += offsetY;

        if (GameManager.gameMode == GameMode.NORMAL || GameManager.gameMode == GameMode.ADVENTURE)
        {
            if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
            {
                return ManagerBlock.boards[inX, inY];
            }
            return null;
        }

        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            if (inX >= GameManager.MinScreenX && inX < GameManager.MaxScreenX && inY >= GameManager.MinScreenY && inY < GameManager.MaxScreenY)
            {
                return ManagerBlock.boards[inX, inY];
            }
        }

        return null;
    }


    public static BlockBase GetBlock(int inX, int inY, int offsetX = 0, int offsetY = 0)
    {
        inX += offsetX;
        inY += offsetY;
        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            if (ManagerBlock.boards[inX, inY] != null && ManagerBlock.boards[inX, inY].Block != null)
            {
                return ManagerBlock.boards[inX, inY].Block;
            }
        }

        return null;
    }

    public static BlockBase GetBlockScreen(int inX, int inY, int offsetX = 0, int offsetY = 0)
    {
        inX += offsetX;
        inY += offsetY;

        if (GameManager.gameMode == GameMode.NORMAL || GameManager.gameMode == GameMode.ADVENTURE)
        {
            if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
            {
                if (ManagerBlock.boards[inX, inY] != null && ManagerBlock.boards[inX, inY].Block != null)
                {
                    return ManagerBlock.boards[inX, inY].Block;
                }
            }
            return null;
        }

        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            if (inX >= GameManager.MinScreenX && inX < GameManager.MaxScreenX && inY >= GameManager.MinScreenY && inY < GameManager.MaxScreenY)
            {
                if (ManagerBlock.boards[inX, inY] != null && ManagerBlock.boards[inX, inY].Block != null)
                {
                    return ManagerBlock.boards[inX, inY].Block;
                }
            }
        }

        return null;
    }

    public static bool InExistScreen(int inX, int inY, int offsetX = 0, int offsetY = 0)
    {
        inX += offsetX;
        inY += offsetY;

        if (GameManager.gameMode == GameMode.NORMAL || GameManager.gameMode == GameMode.ADVENTURE)
        {
            if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
            {
                return true;
            }
        }
        if (inX >= 0 && inX < GameManager.MAX_X && inY >= 0 && inY < GameManager.MAX_Y)
        {
            if (inX >= GameManager.MinScreenX && inX < GameManager.MaxScreenX && inY >= GameManager.MinScreenY && inY < GameManager.MaxScreenY)
            {
                return true;
            }
        }
        return false;
    }

    public static BlockDirection GetDir(int aInX, int aInY, int bInX, int bInY)
    {
        if(aInX == bInX)
        {
            if (aInY < bInY) return BlockDirection.DOWN;
            if (aInY > bInY) return BlockDirection.UP;
        }
        else if (aInY == bInY)
        {
            if (aInX > bInX) return BlockDirection.LEFT;
            if (aInX < bInX) return BlockDirection.RIGHT;
        }
        return BlockDirection.NONE;
    }

    public static Board GetBoardByDir(int aInX, int aInY, BlockDirection dir)
    {
        if(dir == BlockDirection.UP)
        {
            return GetBoard(aInX, aInY, 0, -1);
        }
        else if (dir == BlockDirection.DOWN)
        {
            return GetBoard(aInX, aInY, 0, 1);
        }
        else if (dir == BlockDirection.RIGHT)
        {
            return GetBoard(aInX, aInY, 1, 0);
        }
        else if (dir == BlockDirection.LEFT)
        {
            return GetBoard(aInX, aInY, -1, 0);
        }
        if (dir == BlockDirection.UP_LEFT)
        {
            return GetBoard(aInX, aInY, -1, -1);
        }
        if (dir == BlockDirection.UP_RIGHT)
        {
            return GetBoard(aInX, aInY, 1, -1);
        }
        else if (dir == BlockDirection.DOWN_LEFT)
        {
            return GetBoard(aInX, aInY, -1, 1);
        }
        else if (dir == BlockDirection.DOWN_RIGHT)
        {
            return GetBoard(aInX, aInY, 1, 1);
        }
        return null;
    }

    public static BlockBase GetBlockScreenByDir(int aInX, int aInY, BlockDirection dir)
    {
        if (dir == BlockDirection.UP)
        {
            return GetBlockScreen(aInX, aInY, 0, -1);
        }
        else if (dir == BlockDirection.DOWN)
        {
            return GetBlockScreen(aInX, aInY, 0, 1);
        }
        else if (dir == BlockDirection.RIGHT)
        {
            return GetBlockScreen(aInX, aInY, 1, 0);
        }
        else if (dir == BlockDirection.LEFT)
        {
            return GetBlockScreen(aInX, aInY, -1, 0);
        }
        if (dir == BlockDirection.UP_LEFT)
        {
            return GetBlockScreen(aInX, aInY, -1, -1);
        }
        if (dir == BlockDirection.UP_RIGHT)
        {
            return GetBlockScreen(aInX, aInY, 1, -1);
        }
        else if (dir == BlockDirection.DOWN_LEFT)
        {
            return GetBlockScreen(aInX, aInY, -1, 1);
        }
        else if (dir == BlockDirection.DOWN_RIGHT)
        {
            return GetBlockScreen(aInX, aInY, 1, 1);
        }
        return null;
    }

    /// <summary>
    /// 게임 시작 시, 보너스 형태의 폭탄 생성할 때 랜덤블럭 찾는 함수
    /// (보너스 형태의 폭탄 - 레디아이템, 부스팅 이벤트 등으로 생성된 폭탄)
    /// 현재 스테이지에서 미리 찍혀진 일반 컬러 블럭위에 보너스 폭탄이 생성가능한지 확인하고 랜덤 블럭 반환.
    /// </summary>
    /// <returns></returns>
    static public BlockBase GetRandomBlockAtGameStart()
    {
        if (ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor == false)
        {
            return GetRandomBlockCanMakeBonusBomb();
        }
        else
        {
            return GetRandomBlock();
        }
    }

    static public BlockBase GetRandomBlock()
    {
        List<BlockBase> listBlock;

        //컬러블럭 모으기 목표가 있으면, 해당 컬러 블럭을 제외하고 폭탄 생성 가능한 블럭 찾기.
        listBlock = GetRandomBlockList(true);
        if (listBlock.Count > 0)
        {
            int index = GameManager.instance.GetIngameRandom(0, listBlock.Count);
            return listBlock[index];
        }

        //모으기 대상인 컬러블럭을 제외하고 남은 폭탄 생성 가능한 블럭이 없는 경우,모으기 대상인 컬러 블럭 포함해 폭탄 생성가능한 블럭찾기.
        listBlock = GetRandomBlockList(false);
        if (listBlock.Count > 0)
        {
            int indexA = GameManager.instance.GetIngameRandom(0, listBlock.Count);
            return listBlock[indexA];
        }
        return null;
    }

    static public List<BlockBase> GetRandomBlockList(bool ExcludeCollectionColors = true)
    {
        List<BlockBase> listBlock = new List<BlockBase>();

        listBlock.Clear();

        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
            {
                Board board = PosHelper.GetBoardSreeen(x, y);
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && board != null)
                {
                    if (block.IsNormalBlock() &&
                        block is NormalBlock &&
                        block.IsBombBlock() == false &&
                        block.state == BlockState.WAIT &&
                        block.blockDeco == null &&
                        board.HasDecoHideBlock() == false &&
                        board.HasDecoCoverBlock() == false &&
                        (!ExcludeCollectionColors || ManagerBlock.instance.IsStageTarget(TARGET_TYPE.COLORBLOCK, block.colorType) == false))
                    {
                        listBlock.Add(block);
                    }
                }
            }
        }

        return listBlock;
    }
    
    /// <summary>
    /// 현재 맵 내에 있는 모든 터질 수 있는 보드 리스트 반환
    /// </summary>
    static public List<Board> GetAllPangBoardList()
    {
        List<Board> listBoard = new List<Board>();
        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
            {
                Board board = GetBoardSreeen(x, y);
                BlockBase block = GetBlockScreen(x, y);
                if (board != null && block != null && block.IsCanPang())
                {
                    if (block is BlockSodaJelly)
                    {
                        if ((block as BlockSodaJelly).lifeCount >= 2)
                            listBoard.Add(board);
                    }
                    else if (block is BlockBlack) ;
                    else
                        listBoard.Add(board);
                }
                else if (board != null && board.DecoOnBoard.FindIndex(x => x is INet || x is IHide) > -1)
                    listBoard.Add(board);
                    
            }
        }

        return listBoard;
    }

    static public BlockBase GetRandomBlockCanMakeBonusBomb()
    {
        List<BlockBase> listBlock = new List<BlockBase>();
        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
            {
                Board board = PosHelper.GetBoardSreeen(x, y);
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && board != null)
                {
                    if (block.IsNormalBlock() && block.state == BlockState.WAIT && block.blockDeco == null 
                        && board.HasDecoHideBlock() == false && board.HasDecoCoverBlock() == false && block.IsBombBlock() == false)
                    {
                        NormalBlock normalBlock = block as NormalBlock;
                        if (normalBlock != null && normalBlock.isCanMakeBonusBombBlock == true)
                            listBlock.Add(block);
                    }
                }
            }
        }
        
        if (listBlock.Count > 0)
        {
            int index = GameManager.instance.GetIngameRandom(0, listBlock.Count);
            return listBlock[index];
        }

        //보너스로 만들 수 있는 폭탄 수 보다 랜덤 컬러를 가진 일반 블럭의 수가 더 적을 때는, 지정된 컬러 블럭에도 폭탄 생성.
        return GetRandomBlock();
    }

    static public List<BlockBase> GetRandomBlockListOnTheCountCrack()
    {
        List<BlockBase> listBlock = new List<BlockBase>();
        List<CountCrack> listCountCrack = ManagerBlock.instance.dicCountCrack[ManagerBlock.instance.activeCountCrackIdx];
        for (int i = 0; i < listCountCrack.Count; i++)
        {
            CountCrack countCrack = listCountCrack[i];
            Board board = countCrack.board;
            BlockBase block = board.Block;
            if (block != null && board != null && block.IsNormalBlock() && block is NormalBlock && block.state == BlockState.WAIT
                && block.blockDeco == null && board.HasDecoHideBlock() == false && board.HasDecoCoverBlock() == false && block.IsBombBlock() == false)
            {
                listBlock.Add(block);
            }
        }
        return listBlock;
    }
}
