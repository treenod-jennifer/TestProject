using System.Collections.Generic;


public enum BoardType
{
    NORMAL,
}

public class Board
{
    public int indexX;
    public int indexY;

    public bool IsActiveBoard = false;
    public bool IsHasScarp = false;
    public bool IsStartBoard = false;
    public int startBlockType = 0;
    public int startBlockType2 =0 ;

    public BlockBase Block;
    public BlockBase TempBlock;

    public Lava lava = null; 

    public List<INet> BoardOnNet = new List<INet>();
    public List<ICrack> BoardOnCrack = new List<ICrack>();
    public List<IGrass> BoardOnGrass = new List<IGrass>();
    public List<Istatue> BoardOnIStatue = new List<Istatue>();
    public List<IDisturb> BoardOnDisturbs = new List<IDisturb>();
    public List<IMover> BoardOnMover = new List<IMover>();
    public List<IHide> BoardOnHide = new List<IHide>();

    public List<DecoBase> DecoOnBoard = new List<DecoBase>();

    // 출발에서 나오지 않는 블럭들 리스트로 들고있음.
    public Dictionary<BlockType, List<BlockColorType>> dicIgnoreColor 
        = new Dictionary<BlockType, List<BlockColorType>>();
    public BlockDirection direction = BlockDirection.NONE;

    //폭죽용, 폭죽외에는 물을 제거하지 못하도록 
    public bool isFireWorkBoard = false;

    public void Init()
    {
        IsActiveBoard = true;
        IsHasScarp = false;

        if (Block != null) Block.PangDestroyBoardData();

        foreach (DecoBase boardDeco in DecoOnBoard) boardDeco.DestroySelf();
        DecoOnBoard.Clear();
    }

    public void CheckStatus()
    {
        if (BoardOnIStatue.Count == 0) return;

        for (int i = 0; i < BoardOnIStatue.Count; i++)
        {
            BoardOnIStatue[i].CheckBoardHasGrass();
        }
    }

    //잡기돌같이 블럭 팡을 방해하는게 있는지 // 제거X
    public bool IsDecoDisturbBlock()
    {
        if (BoardOnNet.Count == 0) return false;

        for (int i = 0; i < BoardOnNet.Count; i++)
            if (BoardOnNet[i].IsNetDeco()) return true;

        return false;
    }

    //잡기돌같이 블럭 팡을 방해하는게  있는지  // 제거O
    public bool HasDecoCoverBlock(bool isPang = false, int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (BoardOnNet.Count == 0) return false;
        
        for (int i = 0; i < BoardOnNet.Count; i++)
        {
            if (BoardOnNet[i].IsNetDeco())
            {
                if (isPang && isFireWorkBoard == false)
                    BoardOnNet[i].SetNetPang(uniquePang, pangColorType);

                return true;
            }
        }
        return false;
    }

    //Hide Type의 데코가 블럭 위에 있는지 검사
    public bool HasDecoHideBlock(bool isPang = false, int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        for (int i = 0; i < BoardOnHide.Count; i++)
        {
            if (BoardOnHide[i].IsHideDeco())
            {
                if (isPang && isFireWorkBoard == false)
                    BoardOnHide[i].SetHideDecoPang(uniquePang, pangColorType);

                return true;
            }
        }

        return false;
    }

    // 2, 3단계 얼음처럼 블럭 움직임을 막는 블럭데코가 있는지.
    public bool HasBlockDecoInterruptBlockMove(bool isPang = false, int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (Block.blockDeco != null && Block.blockDeco.IsInterruptBlockMove() == true)
            return true;
        return false;
    }

    public bool SetSplashEffect(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {
        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (boardDeco.SetSplashPang(uniquePang, pangColorType, bombEffect)) return true;
        }

        return false;
    }

    public bool BoardPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)//폭발을 막는지 안막는지 체크
    {
        if (DecoOnBoard.Count == 0) return true;

        bool extendPang = true;

        for (int i = 0; i < BoardOnHide.Count; i++)
        {
            BoardOnHide[i].SetHideDecoPang(uniquePang, pangColorType);
            return extendPang;
        }
        
        for (int i = 0; i < BoardOnDisturbs.Count; i++)
        {
            //블럭 터치 시, 주변 방해블럭에 영향을 미치는 부분은 blockBomb.cs 에서 처리.
            BoardOnDisturbs[i].SetDisturbPang(uniquePang, this.Block.bombType, PangByBomb);
        }

        if (BoardOnGrass.Count != 0)
        {
            for (int i = 0; i < BoardOnGrass.Count; i++)
            {
                BoardOnGrass[i].SetCrassPang(uniquePang, pangColorType);
                return extendPang;
            }
        }

        if (BoardOnCrack.Count != 0)
        {
            for (int i = 0; i < BoardOnCrack.Count; i++)
            {
                BoardOnCrack[i].SetCrackPang(uniquePang, pangColorType);
            }
        }

        return extendPang;
    }

    public bool IsCanFill()     //블럭이 위치할수 있는지
    {
        if (!IsActiveBoard) return false;
        if (Block != null) return false;

        if (DecoOnBoard.Count == 0) return movable;

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (!boardDeco.IsCanFill()) return false;
        }

        return movable;
    }

    public bool movable = true;

    public bool IsCanMove()     //이동할수 있는지 //물속에블럭이 있을수있지만 흘러내릴수는 없음
    {
        if (DecoOnBoard.Count == 0)
        {
            return movable;
        }

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (!boardDeco.IsCanFlow()) return false;
        }

        return movable;
    }


    public void SetUnderBoard()
    {

    }

    public bool IsMakerBlock()
    {
        if (IsStartBoard) return true;

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (boardDeco.IsMakerBlock()) return true;
        }
        return false;
    }

    public void MakeBlockAction() 
    {
        for (int i = 0; i < DecoOnBoard.Count; i++)
        {
            DecoOnBoard[i].MakeBlockAction();
        }
    }

	//해당 방향에 방해블럭이 없는지 확인하는 함수.
    public bool IsNotDisturbBlock(BlockDirection bombDirection = BlockDirection.NONE)
    {
        for (int i = 0; i < BoardOnDisturbs.Count; i++)
        {
            if (BoardOnDisturbs[i].IsDisturbBoard(bombDirection, indexX, indexY) == true)
            {
                return false;
            }
        }
        return true;
    }

    //해당 방향에 물을 막는 방해블럭이 없는지 확인하는 함수.
    public bool IsNotDisturbMoveWater(BlockDirection bombDirection = BlockDirection.NONE)
    {
        for (int i = 0; i < BoardOnDisturbs.Count; i++)
        {
            if (BoardOnDisturbs[i].IsDisturbMoveWater(bombDirection, indexX, indexY) == true)
            {
                return false;
            }
        }
        return true;
    }

    public Potal IsHasPotal()
    {
        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            Potal potal = boardDeco as Potal;
            if (potal != null && potal.type == POTAL_TYPE.IN) return potal;
        }
        return null;
    }

    public Potal IsHasPotalOut()
    {
        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            Potal potal = boardDeco as Potal;
            if (potal != null && potal.type == POTAL_TYPE.OUT) return potal;
        }
        return null;
    }

    //카펫이 현재 설치되어 있고 퍼질 수 있는 상태인지 검사(카펫을 확장 시킬 때 사용)
    public bool IsExistCarpetAndCanExpand()
    {
        //카펫 스테이지가 아닐 경우, 아예 검사 안함.
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        bool isExistCarpet = false;
        if (Block != null && Block.IsCoverCarpet() == true)
            return false;

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (boardDeco.IsCoverCarpet() == true)
                return false;

            Carpet carpet = boardDeco as Carpet;
            if (carpet != null)
                isExistCarpet = true;
        }
        return isExistCarpet;
    }

    //카펫이 현재 보드에 설치되어 있는 상태인지 검사(카펫이 있는지 검사할 때 사용)
    public bool IsExistCarpet()
    {
        //카펫 스테이지가 아닐 경우, 아예 검사 안함.
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;
        
        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            Carpet carpet = boardDeco as Carpet;
            if (carpet != null)
                return true;
        }
        return false;
    }

    //석판 있는지 검사
    public bool IsExistCrack()
    {
        if (BoardOnCrack.Count > 0)
            return true;
        return false;
    }

    //카펫이미지 재설정
    public void CheckCarpetByPlant()
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return ;

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            Carpet carpet = boardDeco as Carpet;
            if (carpet != null)
                carpet.CheckBoard(true);
        }
    }

    public void MakeCarpet(float delay = 0.0f)
    {
        if (IsActiveBoard == false)
            return;
        if (IsExistCarpet() == true)
            return;
        if (BoardOnNet.Count > 0)
            return;
        if (Block != null && Block.blockDeco != null)
            return;
        if (Block != null && Block.IsCanMakeCarpet() == false)
            return;
        if (BoardOnHide.Count > 0)
            return;

        foreach (DecoBase boardDeco in DecoOnBoard)
        {
            if (boardDeco.IsCoverCarpet() == true)
                return;
        }

        DecoInfo decoCarpet = new DecoInfo();
        decoCarpet.BoardType = (int)BoardDecoType.CARPET;
        Carpet carpet = BoardDecoMaker.instance.MakeBoardDeco(this, indexX, indexY, decoCarpet) as Carpet;
        carpet.SetSprite();
        carpet.ShowCarpet(delay);

        //블럭에 양털 설치
        if (Block != null && Block.IsCanCoveredCarpet())
        {
            Block.CoverBlockWithCarpet();
        }

        //목표 제거.
        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.CARPET);
        GameUIManager.instance.RefreshTarget(TARGET_TYPE.CARPET);

        //점수.
        ManagerBlock.instance.AddScore(500);
        InGameEffectMaker.instance.MakeScore(carpet.transform.position, 500, 0.25f + delay);
    }

    public bool IsGetTargetBoard(BlockType blockType)
    {
        foreach(var deco in DecoOnBoard)
        {
            if (deco.GetTargetBoard(blockType))
            {
                return true;
            }
        }

        if(Block.GetTargetBoard(blockType))
        {
            return true;
        }

        return false;
    }

    public bool IsExistLavaModeTargetDeco()
    {
        for (int i = 0; i < DecoOnBoard.Count; i++)
        {
            if (DecoOnBoard[i].IsTarget_LavaMode())
                return true;
        }
        return false;
    }

    public void AddDeco(DecoBase deco)
    {
        DecoOnBoard.Add(deco);

        if (deco is INet)
            BoardOnNet.Add(deco as INet);

        if (deco is ICrack)
            BoardOnCrack.Add(deco as ICrack);

        if (deco is Istatue)
            BoardOnIStatue.Add(deco as Istatue);

        if (deco is IDisturb)
            BoardOnDisturbs.Add(deco as IDisturb);

        if (deco is IGrass)
            BoardOnGrass.Add(deco as IGrass);

        if (deco is IMover)
            BoardOnMover.Add(deco as IMover);
        
        if(deco is IHide)
            BoardOnHide.Add(deco as IHide);

        DecoOnBoard.Sort(delegate (DecoBase a, DecoBase b)
        {
            if (a.boardDecoOrder > b.boardDecoOrder) return 1;
            else if (a.boardDecoOrder < b.boardDecoOrder) return -1;
            return 0;
        });

        BoardOnNet.Sort(delegate (INet _a, INet _b)
        {
            DecoBase a = _a as DecoBase;
            DecoBase b = _b as DecoBase;

            if (a.boardDecoOrder > b.boardDecoOrder) return 1;
            else if (a.boardDecoOrder < b.boardDecoOrder) return -1;
            return 0;
        });
    }

    public void RemoveDeco(DecoBase deco)
    {
        DecoOnBoard.Remove(deco);

        if (deco is INet)
            BoardOnNet.Remove(deco as INet);

        if (deco is ICrack)
            BoardOnCrack.Remove(deco as ICrack);

        if (deco is Istatue)
            BoardOnIStatue.Remove(deco as Istatue);

        if (deco is IDisturb)
            BoardOnDisturbs.Remove(deco as IDisturb);

        if (deco is IGrass)
            BoardOnGrass.Remove(deco as IGrass);

        if (deco is IMover)
            BoardOnMover.Remove(deco as IMover);

        if (deco is IHide)
            BoardOnHide.Remove(deco as IHide);
    }

    /// <summary>
    /// 해당 보드에서 나오지 않아야 할 컬러값을 가져오는 함수
    /// </summary>
    public List<BlockColorType> GetListIgnoreColorType(BlockType bType = BlockType.NORMAL)
    {
        BlockType checkType = bType;
        if (bType != BlockType.NORMAL)
        {
            if (dicIgnoreColor.ContainsKey(bType) == false)
                checkType = BlockType.NORMAL;
        }

        if (dicIgnoreColor.ContainsKey(checkType) == false)
            return new List<BlockColorType>();
        else
            return dicIgnoreColor[checkType];
    }

    /// <summary>
    /// 해당 보드에서 나와야 하는 컬러를 가져오는 함수
    /// </summary
    public List<BlockColorType> GetListCanMakeColorType(BlockType bType = BlockType.NORMAL)
    {
        BlockType checkType = bType;
        if (bType != BlockType.NORMAL)
        {
            if (dicIgnoreColor.ContainsKey(bType) == false)
                checkType = BlockType.NORMAL;
        }

        if (dicIgnoreColor.ContainsKey(checkType) == false)
        {
            return new List<BlockColorType>() { BlockColorType.A, BlockColorType.B, BlockColorType.C, BlockColorType.D, BlockColorType.E };
        }
        else
        {
            List<BlockColorType> listCanMakeColor = new List<BlockColorType>();
            for (int i = 1; i <= 5; i++)
            {
                BlockColorType cType = (BlockColorType)i;
                if (dicIgnoreColor[checkType].FindIndex(x => x == cType) == -1)
                    listCanMakeColor.Add(cType);
            }
            return listCanMakeColor;
        }
    }
}


