using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockCannon : BlockBase
{
    // 프리팹 Inspector
    public UILabel timeCountLabel;
    
    // 블럭 설정 관련
    public int dirType;
    public int blockCount = 10;
    public BlockColorType targetBlockType = BlockColorType.NONE;
    
    // 블럭 Pang 설정 관련
    private int uniqueIndex = -1;
    private int DynamiteBombToFlowerPotPangIndex = -1;
    private int isFixPangIndex = -1;
    
    // 기타 변수
    private Coroutine waitCoroutine = null;
    private int checkWaitFrame = 0;
    
    private enum directionType
    {
        UP = 1,
        RIGHT = 2,
        DOWN = 3,
        LEFT = 4
    }

    private void Start()
    {
        BlockBomb._bombUniqueIndex++;
        uniqueIndex = BlockBomb._bombUniqueIndex;
    }

    #region BlockBase 상속 함수

    public override bool IsCanMakeBombFieldEffect()
    {
        return false;
    }
    
    public override bool IsBlockType()
    {
        return false;
    }
    
    public override bool IsCoverStatue()
    {
        return true;
    }
    
    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        return true;
    }

    public override void UpdateBlock()
    {
        if (state == BlockState.WAIT)
        {
            //1x1 사이즈를 넘는 기믹들은 카운트 계산을 한 프레임을 저장해 중복해서 waitCount가 증가되지 않도록 함.
            if (checkWaitFrame != Time.frameCount)
            {
                checkWaitFrame = Time.frameCount;
                _waitCount++;
            }
        }
    }
    
    public override void UpdateSpriteByBlockType()
    {
        if ((directionType) dirType == directionType.UP)
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
        else if ((directionType) dirType == directionType.RIGHT)
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
        else if ((directionType) dirType == directionType.LEFT)
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270f));
        else if ((directionType) dirType == directionType.DOWN)
            mainSprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        
        timeCountLabel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        if (!isPang)
        {
            mainSprite.spriteName = $"BlockCannon_{targetBlockType}";
            timeCountLabel.text = blockCount.ToString();
            SetMainSpriteDepth();
            timeCountLabel.depth = mainSprite.depth + 1;
        }
        else
        {
            mainSprite.spriteName = $"BlockCannon_Off";
            SetMainSpriteDepth();
            timeCountLabel.gameObject.SetActive(false);
        }
        MakePixelPerfect(mainSprite, 1.3f);
    }

    #endregion

    #region 대포 기믹 전용 함수

    /// <summary>
    /// 대포 기믹 카운트 감소
    /// </summary>
    public void SetCount(int count)
    {
        blockCount -= count;
        if (blockCount <= 0)
            blockCount = 0;
        if (blockCount <= 0 && waitCoroutine == null)
            waitCoroutine = StartCoroutine(StartAction_WaitPang());
        UpdateSpriteByBlockType();
    }
    
    /// <summary>
    /// 대포 대기 연출 재생
    /// </summary>
    private IEnumerator StartAction_WaitPang()
    {
        while (true)
        {
            float color = 0.85f + (Mathf.Sin(ManagerBlock.instance.BlockTime * 20) * 0.1f);
            mainSprite.color = new Color(color, color, color, 1);
            yield return null;
            
            if (waitCoroutine == null)
                break;
        }
    }

    /// <summary>
    /// 대포 기믹 동작 가능한 상태인지 체크
    /// </summary>
    public bool CheckPang()
    {
        if (state == BlockState.WAIT)
        {
            if (blockCount <= 0)
            {
                Pang();
                return true;
            }
        }

        return false;
    }

    #endregion

    #region 팡 관련 함수

    // 현재 Pang 중인지 체크하는 변수로, state.Pang 체크와는 다른 타이밍에 사용되기 때문에 필요
    private bool isPang = false;
    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (blockCount > 0) return;
        if (state == BlockState.PANG) return;
        state = BlockState.PANG;
        lifeCount--;
        StartCoroutine(CoPang());
        UpdateSpriteByBlockType();
    }
    
    private IEnumerator CoPang()
    {
        EEffectCannonLine lineType = EEffectCannonLine.eVUp;
        int offX = 0;
        int offY = 0;
        Vector3 effectPos = Vector3.zero;

        switch ((directionType) dirType)
        {
            case directionType.UP :
                lineType = EEffectCannonLine.eVUp;
                offX = 0;
                offY = -1;
                effectPos = new Vector3(_transform.position.x, _transform.position.y + 0.05f, _transform.position.z);
                break;
            case directionType.DOWN :
                lineType = EEffectCannonLine.eVDown;
                offX = 0;
                offY = 1;
                effectPos = new Vector3(_transform.position.x, _transform.position.y - 0.05f, _transform.position.z);
                break;
            case directionType.LEFT :
                lineType = EEffectCannonLine.eHLeft;
                offX = -1;
                offY = 0;
                effectPos = new Vector3(_transform.position.x - 0.05f, _transform.position.y, _transform.position.z);
                break;
            case directionType.RIGHT :
                lineType = EEffectCannonLine.eHRight;
                offX = 1;
                offY = 0;
                effectPos = new Vector3(_transform.position.x + 0.05f, _transform.position.y, _transform.position.z);
                break;
        }
        

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        mainSprite.color = Color.white;
        mainSprite.transform.DOScale(new Vector2(1.2f, 0.9f), 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        InGameEffectMaker.instance.MakeCannonTargetEffect(effectPos);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine_NotDisturb(indexX, indexY, offX, offY);
        mainSprite.transform.DOScale(new Vector2(1.25f, 0.85f), 0.2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.2f);
        mainSprite.transform.DOScale(new Vector2(1.3f, 1.5f), 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        
        GameObject line = InGameEffectMaker.instance.MakeEffectCannonLine(effectPos, lineType).gameObject;
        StartCoroutine(CoPangToDirection(indexX, indexY, offX, offY, 10, 3));
        ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);
        ManagerSound.AudioPlayMany(AudioInGame.PANG_LINE);
        
        if (offY == 0)
        {
            Board UpbackA = PosHelper.GetBoardSreeen(indexX, indexY, 0, -1);
            if (UpbackA != null && UpbackA.Block != null) UpbackA.Block.BombShakeEffect(0);

            Board DownbackA = PosHelper.GetBoardSreeen(indexX, indexY, 0, 1);
            if (DownbackA != null && DownbackA.Block != null) DownbackA.Block.BombShakeEffect(2);

            Board UpbackB = PosHelper.GetBoardSreeen(indexX, indexY, 0, -1);
            Board DownbackB = PosHelper.GetBoardSreeen(indexX, indexY, 0, 1);
            if (UpbackB != null && UpbackB.Block != null) UpbackB.Block.BombShakeEffect(0);
            if (DownbackB != null && DownbackB.Block != null) DownbackB.Block.BombShakeEffect(2);
        }

        if (offX == 0)
        {
            Board UpbackA = PosHelper.GetBoardSreeen(indexX, indexY, 1, 0);
            Board DownbackA = PosHelper.GetBoardSreeen(indexX, indexY, -1, 0);
            if (UpbackA != null && UpbackA.Block != null) UpbackA.Block.BombShakeEffect(1);
            if (DownbackA != null && DownbackA.Block != null) DownbackA.Block.BombShakeEffect(3);

            Board UpbackB = PosHelper.GetBoardSreeen(indexX, indexY, 1, -0);
            Board DownbackB = PosHelper.GetBoardSreeen(indexX, indexY, -1, -0);
            if (UpbackB != null && UpbackB.Block != null) UpbackB.Block.BombShakeEffect(1);
            if (DownbackB != null && DownbackB.Block != null) DownbackB.Block.BombShakeEffect(3);
        }
        
        isPang = true;
        UpdateSpriteByBlockType();
        InGameEffectMaker.instance.MakeCannonHitEffect(_transform.position);
        mainSprite.transform.localScale = Vector3.one;
        mainSprite.transform.DOScale( Vector3.one * 1.3f, 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        mainSprite.transform.DOScale( Vector3.one * 0.95f, 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        mainSprite.transform.DOScale( Vector3.one * 1.05f, 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        mainSprite.transform.DOScale( Vector3.one * 0.65f, 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        mainSprite.transform.DOScale( Vector3.one * 0.8f, 0.1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.1f);
        mainSprite.transform.DOScale( Vector3.one * 0f, 0.1f).SetEase(Ease.Linear);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, 0.1f);
        yield return new WaitForSeconds(0.1f);

        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(transform.position, 80);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();


        if (back != null && back.BoardOnGrass.Count > 0)
        {
            foreach (var tempGrass in back.BoardOnGrass)
            {
                Grass grassA = tempGrass as Grass;
                grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                grassA.BGSprite.depth = grassA.uiSprite.depth + 1;
            }
        }
        ManagerBlock.instance.RemoveCannon(this);
        Destroy(gameObject);
        PangDestroyBoardData();
        if (back != null)
            back.CheckStatus();
        yield return null;
    }

    /// <summary>
    /// 대포 기믹 단방향 발사
    /// </summary>
    IEnumerator CoPangToDirection(int inX, int inY, int offX, int offY, int count = 10, int effect = 0, float waitTime = 0.030f, List<BombAreaInfo> infoList = null, bool bNoneDirection = false)
    {
        int offsetX = offX;
        int offsetY = offY;

        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        foreach (DecoBase boardDeco in board.DecoOnBoard)
        {
            Carpet carpet = boardDeco as Carpet;
            if (carpet != null && !boardDeco.IsCoverCarpet())
            {
                hasCarpet = true;
                break;
            }
        }
        
        bool upHasCarpet = hasCarpet;

        BlockDirection upDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);
        
        Board checkBoard = PosHelper.GetBoardSreeen(inX, inY, 0, 0);
        if (checkBoard != null)
            SetDirectionDecoPang(checkBoard, upDirection, infoList, bNoneDirection);

        for (int i = 0; i < count; i++)
        {
            float tempTimer = 0;

            Board upBack = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);
            
            if (upBack != null)
            {
                if (upHasCarpet)
                {
                    MakeCarpet(upBack); 
                }
                else
                    upHasCarpet = CheckExistCarpetAtTargetBoard(upBack);

                SetRemoveBlock(upBack);
                SetDirectionDecoPang(upBack, upDirection, infoList, bNoneDirection);
            }

            if (offsetY == 0)
            {
                Board upBackA = PosHelper.GetBoardSreeen(inX, inY, offsetX, -1);
                if (upBackA != null && upBackA.Block != null && (effect == 1 || effect == 3)) upBackA.Block.BombShakeEffect(0);

                Board downBackA = PosHelper.GetBoardSreeen(inX, inY, offsetX, 1);
                if (downBackA != null && downBackA.Block != null && (effect == 2 || effect == 3)) downBackA.Block.BombShakeEffect(2);
            }

            if (offsetX == 0)
            {
                Board upBackA = PosHelper.GetBoardSreeen(inX, inY, 1, offsetY);
                Board downBackA = PosHelper.GetBoardSreeen(inX, inY, -1, offsetY);
                if (upBackA != null && upBackA.Block != null && (effect == 1 || effect == 3)) upBackA.Block.BombShakeEffect(1);
                if (downBackA != null && downBackA.Block != null && (effect == 2 || effect == 3)) downBackA.Block.BombShakeEffect(3);
            }

            offsetX += offX;
            offsetY += offY;

            while (tempTimer < waitTime)
            {
                tempTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }
        
        yield return null;
    }
    
    
    /// <summary>
    /// 대포 기믹 단방향 발사 : 블럭 제거
    /// </summary>
    private bool SetRemoveBlock(Board board)
    {
        bool bCanPangDirection = true;
        
        if (board.Block != null && bCanPangDirection)
        {
            if ((board.Block.IsBombBlock()) &&
                board.Block.IsSkipPang == false &&
                board.Block.state != BlockState.PANG &&
                board.Block.IsExplodeByBomb == false)
            {
                if (board.HasDecoHideBlock() == false && board.HasDecoCoverBlock() == false && board.Block.pangIndex != BlockBomb._bombUniqueIndex)
                {
                    if (board.Block.blockDeco != null && board.Block.blockDeco.IsInterruptBlockSelect())
                    {
                        if (board.Block.blockDeco.DecoPang(BlockBomb._bombUniqueIndex, colorType))
                        {
                            return board.Block.IsPangExtendable();
                        }
                    }

                    board.Block.IsExplodeByBomb = true;     //폭탄일때 한템포 늦게 터는지는 설정
                    board.Block.pangIndex = BlockBomb._bombUniqueIndex;
                    ExplodeBombCount++;
                    
                    ManagerBlock.instance.comboCount++;
                    InGameEffectMaker.instance.MakeCombo(board.Block._transform.position, "Combo " + ManagerBlock.instance.comboCount.ToString());
                }
            }
            else
            {
                if (board.Block.type != BlockType.ColorBigJewel)
                {
                    ManagerBlock.instance.AddScore(80);
                    InGameEffectMaker.instance.MakeScore(board.Block._transform.position, 80);
                }
            }
            
            //화단위에 클로버가 있을 경우 처리를 위한 변수.
            bool isHideDeco = false;
            
            if (board.Block.type == BlockType.ColorBigJewel)
            {
                var ColorBigJewel = board.Block as BlockColorBigJewel;
                
                for (int i = 0; i < ColorBigJewel.blockBoardList.Count; i++)
                {
                    if (ColorBigJewel.blockBoardList[i].DecoOnBoard
                        .Find((DecoBoard) => DecoBoard.boardDecoOrder == BoardDecoOrder.CLOVER))
                    {
                        board.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex, colorType);
                        isHideDeco = true;
                    }
                }

                if (isHideDeco)
                    return true;
            }

            if (board.Block.type == BlockType.ColorBigJewel)
            {
                if (isFixPangIndex != (board.Block as BlockColorBigJewel).index)
                    DynamiteBombToFlowerPotPangIndex = -1;
                if (DynamiteBombToFlowerPotPangIndex == -1)
                {
                    BlockBomb._bombUniqueIndex++;
                    DynamiteBombToFlowerPotPangIndex = BlockBomb._bombUniqueIndex;
                    isFixPangIndex = (board.Block as BlockColorBigJewel).index;
                    return board.Block.BlockPang(BlockBomb._bombUniqueIndex, colorType, true);
                }
            }
            else if (board.Block.type == BlockType.BREAD)
            {
                return board.Block.BlockPang(uniqueIndex, colorType, true);
            }
            else
            {
                BlockBomb._bombUniqueIndex++;
                return board.Block.BlockPang(BlockBomb._bombUniqueIndex, colorType, true);
            }
        }

        if(board.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex, colorType) == false)
            board.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex, colorType);
        
        DynamiteBombToFlowerPotPangIndex = -1;
        return true;
    }

    private List<GrassFenceBlock> grassFenceList = new List<GrassFenceBlock>();
    /// <summary>
    /// 대포 기믹 단방향 발사 : 데코 제거
    /// </summary>
    private void SetDirectionDecoPang(Board checkBoard, BlockDirection direction, List<BombAreaInfo> infoList, bool bNoneDirection)
    {
        //현재 보드에 있는 데코 중, 방향 확인해서 터지는 데코들에 대한 처리.
        foreach (DecoBase boardDeco in checkBoard.DecoOnBoard)
        {
            // 대포 자리에 있는 울타리 기믹들 예외처리 (ex. 아래로 발사하는 대포 자리 위에 있는 울타리 기믹)
            bool isNearBoard = boardDeco.inX == indexX && boardDeco.inY == indexY;
            bool isOtherDir = (boardDeco is FenceBlock && (boardDeco as FenceBlock).direction != direction) ||
                              (boardDeco is GrassFenceBlock && (boardDeco as GrassFenceBlock).direction != direction);
            
            if (isNearBoard && isOtherDir)
                continue;
            
            // 대포 자리에 있는 울타리 기믹들 예외처리 2 : 대포 자리가 아니고, 대포 폭발 방향이 아닌 곳에 대포 방향으로 설치된 울타리 기믹 예외처리 (ex. 오른쪽으로 발사하는 대포 좌측 보드에 오른쪽으로 설치된 울타리 기믹)
            BlockDirection fenceDirection = BlockDirection.NONE;
            if (boardDeco is FenceBlock)
                fenceDirection = (boardDeco as FenceBlock).direction;
            if (boardDeco is GrassFenceBlock)
                fenceDirection = (boardDeco as GrassFenceBlock).direction;

            if (fenceDirection != BlockDirection.NONE)
            {
                bool isOtherBoardSameDir =
                    (fenceDirection == BlockDirection.UP && direction == BlockDirection.UP && indexY + 1 == boardDeco.inY) ||
                    (fenceDirection == BlockDirection.DOWN && direction == BlockDirection.DOWN && indexY - 1 == boardDeco.inY) ||
                    (fenceDirection == BlockDirection.RIGHT && direction == BlockDirection.RIGHT && indexX - 1 == boardDeco.inX) ||
                    (fenceDirection == BlockDirection.LEFT && direction == BlockDirection.LEFT && indexX + 1 == boardDeco.inX);
                
                if (isOtherBoardSameDir)
                    continue;
                
                // 대포가 터지는 길에 있는 풀울타리 기믹 예외처리 : 풀울타리 기믹이 이미 이전 보드에서 존재하면 Pang 처리가 되었으므로 중복으로 터지지 않도록 처리
                bool isDuplicateFence = false;
                foreach (var fence in grassFenceList)
                {
                    bool isSameDir = boardDeco is GrassFenceBlock && (boardDeco as GrassFenceBlock).direction == fence.direction;
                    bool isSamePos = boardDeco.inX == fence.inX && boardDeco.inY == fence.inY;
                    if (isSameDir && isSamePos)
                    {
                        isDuplicateFence = true;
                        break;
                    }
                }

                if (isDuplicateFence)
                    continue;
            }

            if (boardDeco is GrassFenceBlock)
                grassFenceList.Add(boardDeco as GrassFenceBlock);
            boardDeco.SetDirectionPang(checkBoard, indexX, indexY, BlockBomb._bombUniqueIndex, direction, infoList, bNoneDirection);
        }
    }

    #endregion

    #region 카펫    
    private bool CheckExistCarpetAtTargetBoard(Board board)
    {
        if (ManagerBlock.instance.isCarpetStage == false)
            return false;

        if (board == null)
            return false;

        return board.IsExistCarpetAndCanExpand();
    }

    private void MakeCarpet(Board makeBoard, float delay = 0)
    {
        if (makeBoard == null 
            || makeBoard.Block == null 
            || (makeBoard.Block.IsCanMakeCarpetByBomb() == false)
            || makeBoard.Block.pangIndex == BlockBomb._bombUniqueIndex)
            return;

        makeBoard.MakeCarpet(delay);
    }
    #endregion
}