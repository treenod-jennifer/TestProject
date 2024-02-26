using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BlockWaterBomb : BlockBase
{
    public UILabel timeCountLabel;
    public UISprite levelSprite;
    public UISprite countSprite;
    public UISprite fireSprite;
    
    public int timeCount = 10;
    private bool isFirstTimeByStart = true;
    private bool isCoroutineEnd = false;
    //연출 코루틴
    private Coroutine countAction = null;

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsStopEvnetAtDestroyIce()
    {
        return true;
    }

    public override bool IsStopEventAtDestroyRandomBox()
    {
        return true;
    }
    
    public override bool IsStopEventAtOutClover()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        MakePixelPerfect(mainSprite);
        MakePixelPerfect(levelSprite);
        MakePixelPerfect(countSprite);
        MakePixelPerfect(fireSprite);
        
        SetCountAction();
        SetMainSpriteDepth();
        
        levelSprite.gameObject.SetActive(lifeCount > 1);
        timeCountLabel.text = timeCount.ToString();
        levelSprite.depth = mainSprite.depth + 1;
        countSprite.depth = mainSprite.depth + 2;
        fireSprite.gameObject.SetActive(timeCount <= 5);
        fireSprite.depth = mainSprite.depth + 2;
        timeCountLabel.depth = mainSprite.depth + 3;
        mainSprite.transform.localScale = Vector3.one;
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;

        if (blockDeco != null)
        {
            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }
        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount <= 0)
            return;
        lifeCount--;
        IsSkipPang = true;
        state = BlockState.PANG;
        if (lifeCount <= 0)                 // 물풍선 제거
            StartCoroutine(CoPangFinal());
        else                                // 단계 하락
        {
            StartCoroutine(CoPang());

            if (targetPos != Vector3.zero && Vector3.Distance(_transform.localPosition, targetPos) > 15f)
            {
                state = BlockState.MOVE;
            }
            else
            {
                state = BlockState.WAIT;
            }
            IsSkipPang = false;
        }
    }

    public IEnumerator CoPang()
    {
        ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_MATCH);
        UpdateSpriteByBlockType();

        InGameEffectMaker.instance.MakeDynamiteLineEffect(_transform.position);
        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        mainSprite.transform.localScale = Vector3.one;
    }

    public IEnumerator CoPangFinal()
    {
        if (timeCount <= 0)
        {
            ManagerBlock.instance.GetWater = true;
            if (countAction != null)
            {
                StopCoroutine(countAction);
                countAction = null;
                MakePixelPerfect(mainSprite);
            }
            float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
            mainSprite.transform.DOScale(0.65f, actionTime).SetEase(Ease.Linear);
            yield return new WaitForSeconds(actionTime);
            actionTime = ManagerBlock.instance.GetIngameTime(0.1f);
            mainSprite.transform.DOScale(1.35f, actionTime).SetEase(Ease.Linear);
            yield return new WaitForSeconds(actionTime);
            ManagerSound.AudioPlayMany(AudioInGame.PANG_BOMB);
            InGameEffectMaker.instance.MakeWaterBombEffect(_transform.position);
        }
        else
        {
            ManagerBlock.instance.AddScore(80);
            InGameEffectMaker.instance.MakeScore(_transform.position, 80);
            ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_1PANG);
            InGameEffectMaker.instance.MakeBlockPangEffect(_transform.position);
        }

        float timer = 0f;
        bool isMakeWater = false;
        bool isActionEnd = false;
        while (timer < 1f)
        {
            _transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            if (!isMakeWater && timer > 0.6f)
            {
                if (timeCount <= 0)
                    StartCoroutine(CheckAndMakeWater(() => isActionEnd = true));
                else
                    isActionEnd = true;
                isMakeWater = true;
            }
            yield return null;
        }

        yield return new WaitUntil(() => isActionEnd == true);

        IsSkipPang = false;
        DestroyBlockData();
        PangDestroyBoardData();
        ManagerBlock.instance.RemoveWaterBomb(this);

        yield return null;
    }

    #region 물 증식 관련

    private IEnumerator CheckAndMakeWater(System.Action endAction)
    {
        DecoBase baseWater = MakeWater(indexX, indexY);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.01f));

        PlayWaterAnimation(MakeWater(indexX, indexY - 1));
        PlayWaterAnimation(MakeWater(indexX, indexY + 1));
        PlayWaterAnimation(MakeWater(indexX - 1, indexY));
        PlayWaterAnimation(MakeWater(indexX + 1, indexY));
        ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
        endAction?.Invoke();
    }

    private void PlayWaterAnimation(DecoBase water)
    {
        Board baseBoard = PosHelper.GetBoard(indexX, indexY);
        if (water != null && baseBoard != null && baseBoard.DecoOnBoard.Count > 0)
        {
            float duration = ManagerBlock.instance.GetIngameTime(0.2f);
            var _pos = water.transform.localPosition;
            water.transform.localPosition = baseBoard.DecoOnBoard[0].transform.localPosition;
            water.transform.DOLocalMove(_pos, duration).SetEase(Ease.OutSine);
        }
    }

    private DecoBase MakeWater(int posX, int posY)
    {
        DecoInfo decoWater = new DecoInfo();
        decoWater.BoardType = (int)BoardDecoType.WATER;
        DecoBase _water = new DecoBase();

        if (IsCanMakeWater(posX, posY))
        {
            _water = BoardDecoMaker.instance.MakeBoardDeco(ManagerBlock.boards[posX, posY], posX, posY, decoWater);
            if (ManagerBlock.boards[posX, posY] != null && ManagerBlock.boards[posX, posY].Block != null)
            {
                if (ManagerBlock.boards[posX, posY].Block.IsCanRemoveWater())
                {
                    ManagerBlock.boards[posX, posY].Block.RemoveCollectEventBlock();
                    ManagerBlock.boards[posX, posY].Block.PangDestroyBoardData();
                }
                else
                {
                    ManagerBlock.boards[posX, posY].Block.RemoveLinkerNoReset();
                }
            }
        }

        return _water;
    }

    private bool IsCanMakeWater(int posX, int posY)
    {
        Board getBoard = PosHelper.GetBoard(posX, posY);

        // 기본적인 체크 : 활성화된 보드인지, 용암 보드인지, 커버하고 있는 데코가 있는지
        if (getBoard == null || !getBoard.IsActiveBoard || getBoard.lava != null || getBoard.HasDecoHideBlock() || getBoard.HasDecoCoverBlock())
            return false;
        else
        {
            // 블럭 체크 : 물이 증식하면 안 되는 블럭인지
            if (getBoard.Block != null)
            {
                if (!getBoard.Block.IsBlockType())
                    return false;
                else if (getBoard.Block.blockDeco != null && getBoard.Block.blockDeco.IsInterruptBlockSelect())
                    return false;
                else if (getBoard.Block is BlockHeart || getBoard.Block is BlockHeartHome)
                    return false;
                else if (getBoard.Block is BlockEventGround && (getBoard.Block as BlockEventGround).lifeCount > 0)
                    return false;
                else if (getBoard.Block is BlockGround)
                {
                    BlockGround tempBlockGround = getBoard.Block as BlockGround;
                    // 흙 내에 보석이 있고 단계가 1단계 아래일때 제외, 물에 뜨지 않도록 체크
                    if (!(tempBlockGround.groundType == GROUND_TYPE.JEWEL && tempBlockGround.lifeCount <= 1))
                        return false;
                }
            }
            // 데코 체크 : 물이 중복돼서 증식되지 않도록 체크
            if (getBoard.DecoOnBoard.Count != 0)
                foreach (DecoBase tempWater in getBoard.DecoOnBoard)
                    if (tempWater is Water) return false;

            // 방해 블럭 관련 체크
            Board centerBoard = PosHelper.GetBoardSreeen(indexX, indexY);

            // 상단
            if (indexX == posX && indexY - 1 == posY)
                return centerBoard.IsNotDisturbMoveWater(BlockDirection.UP);
            // 하단
            else if (indexX == posX && indexY + 1 == posY)
                return centerBoard.IsNotDisturbMoveWater(BlockDirection.DOWN);
            // 좌측
            else if (indexX - 1 == posX && indexY == posY)
                return centerBoard.IsNotDisturbMoveWater(BlockDirection.LEFT);
            // 우측
            else if (indexX + 1 == posX && indexY == posY)
                return centerBoard.IsNotDisturbMoveWater(BlockDirection.RIGHT);
        }
        return true;
    }

    #endregion

    public override bool EventAction()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        bool hasInterruptDeco = board.DecoOnBoard.FindIndex(x => x.IsInterruptBlockEvent() == true) > -1;

        // 이벤트를 막는 데코가 있는지
        if (hasInterruptDeco == true || (blockDeco != null && blockDeco.IsInterruptBlockEvent() == true))
            return false;

        // 출발 시점인지
        if ((isMakeByStart == true && isFirstTimeByStart == true) || isStopEvent == true)
        {
            isFirstTimeByStart = false;
            isStopEvent = false;
            return false;
        }

        timeCount--;
        ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_COUNT_DOWN);
        UpdateSpriteByBlockType();
        return false;
    }
    
    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
        timeCountLabel.enabled = setEnabled;
        levelSprite.enabled = setEnabled;
        countSprite.enabled = setEnabled;
        fireSprite.enabled = setEnabled;
    }

    #region 카운트 관련

    private void SetCountAction()
    {
        if (timeCount <= 5 && timeCount > 0)
        {
            if (GameManager.instance.state == GameState.EDIT)
                mainSprite.color = new Color(0.7f, 0.7f, 0.7f, 1);
            else if (countAction == null)
                countAction = StartCoroutine(Co5CountAction());
            else if (isCoroutineEnd)
            {
                isCoroutineEnd = false;
                StopCoroutine(countAction);
                countAction = StartCoroutine(Co5CountAction());
            }
        }
        else
        {
            if (countAction != null)
            {
                StopCoroutine(countAction);
                countAction = null;
            }
        }
    }

    IEnumerator Co5CountAction()
    {
        while (true)
        {
            if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
                break;

            if (timeCount > 5)
                break;

            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            float ratioRotate = Mathf.Sin(ManagerBlock.instance.BlockTime * 2.5f);
            float color = 0.85f + (ratioScale * 0.2f);
            mainSprite.color = new Color(color, color, color, 1);
            mainSprite.cachedTransform.localScale = Vector3.one * (0.98f + ratioScale * 0.04f);
            mainSprite.cachedTransform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -5.0f) * (ratioRotate * 0.7f));
            fireSprite.spriteName = $"water_bomb_fire-{(int)(ManagerBlock.instance.BlockTime * 4) % 2 + 1}";
            yield return null;
        }

        //이미지 원상태로 돌려줌.
        mainSprite.color = Color.white;
        mainSprite.cachedTransform.localScale = Vector3.one;
        isCoroutineEnd = true;
        yield return null;
    }

    #endregion

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_1;
    }
}
