using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockPeaBoss : BlockBase
{
    [SerializeField]
    private bool isActive = false;
    private bool isFirstTimeByStart = true;

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanMakeCarpetByBomb()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return isActive;
    }

    public override bool IsCanPangByPowerBomb()
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

    public override bool IsCanMakeBombFieldEffect()
    {
        //얼음이 설치되어 있는 상황이면 폭탄 영역 표시 가능
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        else
        {   //온/오프 상태에 따라 폭탄 영역 표시 가능 여부가 결정됨
            return (isActive == true) ? true : false;
        }
    }

    public override bool IsCanMakeCarpet()
    {
        if (blockDeco != null && blockDeco.IsInterruptBlockPang())
            return false;

        if (isActive == false)
            return false;

        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        SetMainSpriteDepth();
        mainSprite.spriteName = (isActive == true) ? "peaBoss_3" : "peaBoss_1";
    }

    public override bool EventAction()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        bool hasInterruptDeco = board.DecoOnBoard.FindIndex(x => x.IsInterruptBlockEvent() == true) > -1;

        if (hasInterruptDeco == true || (blockDeco != null && blockDeco.IsInterruptBlockEvent() == true))
            return false;

        if ((isMakeByStart == true && isFirstTimeByStart == true) || isStopEvent == true)
        {
            isFirstTimeByStart = false;
            isStopEvent = false;
            return false;
        }

        isActive = (isActive == true) ? false : true;
        ManagerSound.AudioPlayMany(AudioInGame.PEA_OPEN);
        StartCoroutine(CoChangeImage());
        StartCoroutine(CoScaleAction());
        return false;
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
        if (lifeCount <= 0 || isActive == false)
            return;

        lifeCount--;
        IsSkipPang = true;
        state = BlockState.PANG;
        StartCoroutine(CoPangFinal());
    }

    public override void PangByPowerBomb(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount <= 0)
            return;

        lifeCount--;
        IsSkipPang = true;
        state = BlockState.PANG;
        StartCoroutine(CoPangFinal());
    }

    IEnumerator CoPangFinal()
    {
        //이미지 스케일
        StartCoroutine(CoScaleAction());
        float actionTime = ManagerBlock.instance.GetIngameTime(0.05f);
        yield return new WaitForSeconds(actionTime);

        mainSprite.spriteName = "peaBoss_4";
        
        //카펫잇는지 확인
        bool HasCarpet = PosHelper.GetBoardSreeen(indexX, indexY).IsExistCarpetAndCanExpand();
        if (HasCarpet == true)
        {
            this.CoverBlockWithCarpet();
        }

        //석판 확인
        Board getBoard = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoard != null)
        {
            getBoard.BoardPang(pangIndex);
        }

        ManagerBlock.instance.AddScore(500);
        InGameEffectMaker.instance.MakeScore(_transform.position, 500);
        ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
        InGameEffectMaker.instance.MakeEffectPeaBomb(_transform.position);

        //폭탄 효과
        List<BombAreaInfo> infoList = ManagerBlock.instance.GetBombAreaInfoList(BlockBomb.BombShape.Cross, indexX, indexY, 3, 3);
        InGameEffectMaker.instance.MakePangFieldEffectToDirection(indexX, indexY, BlockBombType.BLACK_BOMB, _infoList : infoList);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));
        yield return CoMakeBomb();
        
        DestroyBlockData();

        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.PEA)
            {
                InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.PEA);
                break;
            }
        }

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        float destroySpeed = 4f;
        float timer = 0f;
        while (timer < 0.15f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
        
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * destroySpeed;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
        yield return null;
        PangDestroyBoardData();
    }

    IEnumerator CoMakeBomb()
    {
        BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.BLACK_BOMB, BlockColorType.NONE, false, false, false, pangIndex);
        yield return null;
    }

    IEnumerator CoChangeImage()
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.05f);
        yield return new WaitForSeconds(actionTime);
        mainSprite.spriteName = "peaBoss_2";
        yield return new WaitForSeconds(actionTime);
        UpdateSpriteByBlockType();
    }

    IEnumerator CoScaleAction()
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        mainSprite.transform.DOScale(1.2f, actionTime).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.4f);
        mainSprite.transform.DOScale(1.0f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
        MakePixelPerfect(mainSprite);
    }

    public void InitBlock(int count)
    {
        isActive = (count == 1) ? false : true;

        ManagerBlock.instance.listPeas.Add(this);
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return false;
        return !isActive;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_1;
    }
}
