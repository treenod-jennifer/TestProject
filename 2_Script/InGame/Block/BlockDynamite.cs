using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockDynamite : BlockBase
{
    public UILabel lifeCountLabel;
    public UIBlockSprite[] countSprite;
    public UIBlockSprite countBaseSprite;

    private bool isFirstTimeByStart = true;
    private bool isCoroutineEnd = false;

    //째깍폭탄 남은 턴 수
    public int DynamiteCount
    {
        get { return dynamiteCount; }
        set
        {
            dynamiteCount = value;
            SetDynamiteCountAction();
        }
    }
    private int dynamiteCount = 0;

    //연출 코루틴
    private Coroutine dynamiteCountAction = null;

    public override bool IsCanLink()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
            return false;

        return true;
    }

    public override bool IsSelectable()
    {
        return false;
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

    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
        lifeCountLabel.enabled = setEnabled;
        countBaseSprite.enabled = setEnabled;

        for (int i = 0; i < countSprite.Length; i++)
        {
            countSprite[i].enabled = setEnabled;
        }
    }
    /*
    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0)
    {
        if (mBombEffect) return;
        if (pangIndex == uniqueIndex) return;
        if (state == BlockState.PANG) return;
        if (colorType != splashColorType) return;
        
        pangIndex = uniqueIndex;
        state = BlockState.PANG;
        _pangRemoveDelay = 0.5f;
        Pang();
    }
    */
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

    public void RecoverDynamite()
    {
        UISprite[] allIamge = gameObject.GetComponentsInChildren<UISprite>();
        foreach (var temp in allIamge)
        {
            temp.color = Color.white;
        }
        mainSprite.color = Color.white;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "Dynamite_" + GetColorTypeString();
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE;
        MakePixelPerfect(mainSprite);

        countBaseSprite.spriteName = "Dynamite_" + GetColorTypeString() + "_Count";
        countBaseSprite.depth = (int)GimmickDepth.BLOCK_LAND + 4;
        MakePixelPerfect(countBaseSprite);

        lifeCountLabel.text = DynamiteCount.ToString();
        lifeCountLabel.depth = (int)GimmickDepth.BLOCK_LAND + 5;

        SetDynamiteCountAction();
    }

    public override void SpriteRatio(float ratio, float vertRatio = 0)
    {
        foreach (var tempCount in countSprite)
        {
            tempCount.customFill.verticalRatio = vertRatio;
            tempCount.customFill.blockRatio = ratio;
        }
        countBaseSprite.customFill.verticalRatio = vertRatio;
        countBaseSprite.customFill.blockRatio = ratio;
    }

    public void CheckDynamiteCount()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < lifeCount)
            {
                countSprite[i].spriteName = "DynamiteLineOn";
                MakePixelPerfect(countSprite[i]);
                countSprite[i].cachedTransform.localScale = new Vector3(0.9f, 1, 1);
                countSprite[i].depth = ((int)GimmickDepth.BLOCK_LAND + 1) + i;
            }
            else
            {
                countSprite[i].spriteName = "DynamiteLineOff";
                MakePixelPerfect(countSprite[i]);
                countSprite[i].cachedTransform.localScale = new Vector3(0.9f, 1, 1);
                countSprite[i].depth = ((int)GimmickDepth.BLOCK_LAND + 1) + i;
            }
        }
    }

    public override bool EventAction()
    {
        if ((isMakeByStart == true && isFirstTimeByStart == true) || isStopEvent == true)
        {
            isFirstTimeByStart = false;
            isStopEvent = false;
        }
        else
        {
            Board board = PosHelper.GetBoardSreeen(indexX, indexY);
            bool hasInterruptDeco = board.DecoOnBoard.FindIndex(x => x.IsInterruptBlockEvent() == true) > -1;

            if (hasInterruptDeco == false &&
                (blockDeco == null || blockDeco.IsInterruptBlockEvent() == false))
            {
                DynamiteCount--;
                lifeCountLabel.text = DynamiteCount.ToString();
                StartCoroutine(CoDownCount());

                //카운트내려가는소리
                ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_COUNT_DOWN);
            }
        }

        if (DynamiteCount <= 0)
        {   
            StartCoroutine(CoCountDown());
            GameManager.instance.IsCanTouch = false;

            if (dynamiteCountAction != null)
            {
                StopCoroutine(dynamiteCountAction);
                dynamiteCountAction = null;
            }
            return true;
        }
        return false;
    }


    //째깍폭탄 남은 카운트에 따른 연출
    private void SetDynamiteCountAction()
    {
        if (DynamiteCount <= 5)
        {
            if (GameManager.instance.state == GameState.EDIT)
            {
                mainSprite.color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            else
            {
                if (dynamiteCountAction == null)
                {
                    dynamiteCountAction = StartCoroutine(show5turnLeft());
                }
                else if (isCoroutineEnd)
                {
                    isCoroutineEnd = false;
                    StopCoroutine(dynamiteCountAction);
                    dynamiteCountAction = StartCoroutine(show5turnLeft());
                }
            }
        }
        else
        {
            if (dynamiteCountAction != null)
            {
                StopCoroutine(dynamiteCountAction);
                dynamiteCountAction = null;
            }
        }
    }

    IEnumerator show5turnLeft()
    {
        while (true)
        {
            if (blockDeco != null && blockDeco.IsInterruptBlockSelect())
                break;

            if (DynamiteCount > 5)
                break;
            
            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            float color = 0.7f + (ratioScale * 0.3f);
            mainSprite.color = new Color(color, color, color, 1);
            mainSprite.cachedTransform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);
            yield return null;
        }

        //이미지 원상태로 돌려줌.
        mainSprite.color = Color.white;
        mainSprite.cachedTransform.localScale = Vector3.one;
        isCoroutineEnd = true;
        yield return null;
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (PangBlack) return;
        PangBlack = true;

        lifeCount--;
        CheckDynamiteCount();
        InGameEffectMaker.instance.MakeDynamiteLineEffect(_transform.position);

        if (lifeCount == 0)
        {
            //제거되어 폭발되는 사운드
            ManagerSound.AudioPlayMany(AudioInGame.DYNAMITE_REMOVE);
            Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
            if (getBoard != null)
            {
                getBoard.BoardPang(pangIndex, pangColorType);
            }

            isSkipDistroy = true;

            DestroyBlockData();

            StartCoroutine(CoPangFinal());
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

    IEnumerator CoPangFinal()
    {
        InGameEffectMaker.instance.MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1);
        BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.BOMB, BlockColorType.NONE, false, false, false, pangIndex);
        InGameEffectMaker.instance.MakeDynamitePangColorEffect(colorType, _transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 3f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        timer = 0f;
        while (timer < 0.4f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
        yield return null;
    }


    IEnumerator CoPang()
    {
        UpdateSpriteByBlockType();

        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        mainSprite.transform.localScale = Vector3.one;
    }

    IEnumerator CoCountDown()
    {
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));

        while (true)
        {
            if (ManagerBlock.instance.state == BlockManagrState.WAIT && ManagerBlock.instance.checkBlockWait() && FlyTarget.flyTargetCount <= 0)
            {
                if (GameManager.instance.checkAllRemoveTarget() == false)
                    GameManager.instance.StageFail(true);
                break;
            }
            yield return null;
        } 

        InGameEffectMaker.instance.MakeDynamiteZeroPangEffect(_transform.position);
        yield return null;

        UISprite[] allIamge = gameObject.GetComponentsInChildren<UISprite>();
        foreach (var temp in allIamge)
        {
            temp.color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        mainSprite.color = new Color(0.5f, 0.5f, 0.5f, 1);
        yield return null;
    }

    IEnumerator CoDownCount()
    {
        float timer = 0.6f;
        while (timer < 1f)
        {
            lifeCountLabel.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 3f;
            yield return null;
        }
        lifeCountLabel.transform.localScale = Vector3.one;
        yield return null;
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

    public override void CoverBlockWithCarpet()
    {
        if (lifeCount > 1)
            return;

        Board tempBoard = PosHelper.GetBoard(indexX, indexY);
        if (tempBoard != null && tempBoard.HasDecoCoverBlock() == false
            && (blockDeco == null || blockDeco.IsInterruptBlockSelect() == false)
            && carpetSprite == null)
        {
            carpetSprite = NGUITools.AddChild(mainSprite.gameObject, BlockMaker.instance.blockCarpetSpriteObj).GetComponent<BlockCarpetSprite>();
            carpetSprite.InitCarpetSprite((int)GimmickDepth.BLOCK_LAND + 6);
            hasCarpet = true;
        }
    }
}
