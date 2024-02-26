using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockPaint : BlockBase
{
    [SerializeField] private UISprite effectSprite;

    //블럭이 터지길 대기하는 상태(이벤트 상태에서 폭발함)
    [HideInInspector] public bool isStateWaitPang = false;

    private Sequence waitSequence = null;
    private List<BlockBase> listChangeBlock = new List<BlockBase>();

    public override bool IsCanMove()
    {
        //터짐 대기 상태에서는 움직이지 않음
        return !isStateWaitPang;
    }

    public override bool IsCanPang()
    {
        return !isStateWaitPang;
    }

    public override bool isCanPangByRainbow()
    {
        return false;
    }

    public override void UpdateBlock()
    {
        if (isStateWaitPang == false)
        {
            if (IsCanMove())
            {
                MoveBlock();
            }
            else
            {
                _waitCount++;
            }
        }
        else
        {   //이동 중 대기상태가 되었을 때,보드 위치까지 이동하도록 설정
            if (this.state == BlockState.MOVE_OTHER)
            {
                targetPos = PosHelper.GetPosByBlock(this);
            }
            MoveBlock();
        }
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = string.Format("Paint_{0}_{1}", GetColorTypeString(), lifeCount < 1 ? 1 : lifeCount);
        SetMainSpriteDepth();
        effectSprite.depth = mainSprite.depth + 1;
        mainSprite.transform.localScale = Vector3.one;
        MakePixelPerfect(mainSprite);
        MakePixelPerfect(effectSprite);
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
        if (lifeCount <= 0 || isStateWaitPang == true)
            return;

        lifeCount--;
        if (lifeCount <= 0)
        {
            isStateWaitPang = true;
            StartAction_WaitPang();

            //blockPang에서 강제로 상태를 Pang으로 변경했을 때 상태를 원래대로 돌리기 위해 사용
            if (state == BlockState.PANG)
            {
                IsSkipPang = true;
                if (targetPos != Vector3.zero && _transform.localPosition != targetPos)
                {
                    state = BlockState.MOVE_OTHER;
                }
                else
                {
                    state = BlockState.WAIT;
                }
            }
        }
        else
        {
            IsSkipPang = true;
            state = BlockState.PANG;
            StartCoroutine(CoPang());
        }
    }

    public IEnumerator CoPang()
    {
        ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_1PANG);
        UpdateSpriteByBlockType();

        //이펙트
        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;

            if (timer > 0.3f && state == BlockState.PANG)
            {
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
        mainSprite.transform.localScale = Vector3.one;
    }

    public IEnumerator CoPangFinal()
    {
        waitSequence.Kill();

        //이펙트 출력
        ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_1PANG);
        InGameEffectMaker.instance.MakeEffectPaintPangEffect(_transform.position, colorType);

        //점수 출력
        AddScore();

        //블럭 컬러 변경 및 제거 연출 출력
        bool isActionEnd = false;
        //StartCoroutine(CoChangeColorNearByBlock());

        ChangeColorNearByBlock();
        StartCoroutine(CoPangAction(() => isActionEnd = true));

        //모든 연출이 끝날 때까지 대기
        if (listChangeBlock.Count > 0)
        {
            float actionTime = ManagerBlock.instance.GetIngameTime(0.45f + (0.02f * listChangeBlock.Count));
            yield return new WaitForSeconds(actionTime);
        }
        yield return new WaitUntil(() => isActionEnd == true);

        //블럭 제거
        IsSkipPang = false;
        ManagerBlock.instance.RemovePaint(this);
        DestroyBlockData();
        PangDestroyBoardData();
    }

    private IEnumerator CoPangAction(System.Action endAction)
    {
        mainSprite.transform.DORotate(new Vector3(0, 0, -360f), ManagerBlock.instance.GetIngameTime(0.5f), RotateMode.FastBeyond360);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));

        mainSprite.transform.DOScale(1.2f, ManagerBlock.instance.GetIngameTime(0.2f)).SetEase(Ease.InOutBack);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.15f));

        InGameEffectMaker.instance.MakeBlockPangEffect(_transform.position);
        endAction?.Invoke();

        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0, ManagerBlock.instance.GetIngameTime(0.05f));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.05f));
    }

    private void ChangeColorNearByBlock()
    {
        //컬러 변경이 가능한 블럭을 리스트에 추가
        AddListCanChangeBlock(BlockDirection.UP);
        AddListCanChangeBlock(BlockDirection.UP_RIGHT);
        AddListCanChangeBlock(BlockDirection.RIGHT);
        AddListCanChangeBlock(BlockDirection.DOWN_RIGHT);
        AddListCanChangeBlock(BlockDirection.DOWN);
        AddListCanChangeBlock(BlockDirection.DOWN_LEFT);
        AddListCanChangeBlock(BlockDirection.LEFT);
        AddListCanChangeBlock(BlockDirection.UP_LEFT);

        //컬러 변경 적용
        float actionTime = ManagerBlock.instance.GetIngameTime(0.02f);
        for (int i = 0; i < listChangeBlock.Count; i++)
        {
            StartCoroutine(CoChangeBlockColor(listChangeBlock[i]));
        }
    }

    private IEnumerator CoChangeColorNearByBlock()
    {
        //컬러 변경이 가능한 블럭을 리스트에 추가
        AddListCanChangeBlock(BlockDirection.UP);
        AddListCanChangeBlock(BlockDirection.UP_RIGHT);
        AddListCanChangeBlock(BlockDirection.RIGHT);
        AddListCanChangeBlock(BlockDirection.DOWN_RIGHT);
        AddListCanChangeBlock(BlockDirection.DOWN);
        AddListCanChangeBlock(BlockDirection.DOWN_LEFT);
        AddListCanChangeBlock(BlockDirection.LEFT);
        AddListCanChangeBlock(BlockDirection.UP_LEFT);

        //순차적으로 컬러 변경 적용
        float actionTime = ManagerBlock.instance.GetIngameTime(0.02f);
        for (int i = 0; i < listChangeBlock.Count; i++)
        {
            StartCoroutine(CoChangeBlockColor(listChangeBlock[i]));
            yield return new WaitForSeconds(actionTime);
        }
    }

    public void AddListCanChangeBlock(BlockDirection dir)
    {
        //블럭의 컬러 변화를 막는 데코가 있다면 검사하지 않음
        Board tempBoard = PosHelper.GetBoardByDir(indexX, indexY, dir);
        if (tempBoard != null && tempBoard.DecoOnBoard.FindIndex(x => x.IsInterruptBlockColorChange() == true) == -1)
        {
            BlockBase tempBlock = PosHelper.GetBlockScreenByDir(indexX, indexY, dir);
            if (tempBlock != null
                && tempBlock.type == BlockType.NORMAL && tempBlock.IsNormalBlock()
                && tempBlock.blockDeco == null && tempBlock.state != BlockState.PANG)
                listChangeBlock.Add(tempBlock);
        }
    }

    public IEnumerator CoChangeBlockColor(BlockBase tempBlock)
    {
        InGameEffectMaker.instance.MakeEffectFlyColorChangeEffect(this._transform.localPosition, tempBlock._transform.localPosition, colorType);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.45f));
        tempBlock.colorType = colorType;
        tempBlock.RemoveLinkerNoReset();
        tempBlock.MakeLinkerByManager();
    }

    public override IEnumerator CoPangThatCannotRemoveFromEventState()
    {
        if (isStateWaitPang == false)
            yield break;

        //이펙트 출력
        ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_1PANG);
        InGameEffectMaker.instance.MakeEffectPaintPangEffect(_transform.position, colorType);

        //점수 출력
        AddScore();

        //블럭 제거
        yield return CoPangImmediately();
    }

    private void AddScore()
    {
        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(_transform.position, 80);
    }

    private void StartAction_WaitPang()
    {
        waitSequence = DOTween.Sequence()
            .Append(mainSprite.transform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.3f))
            .Join(DOTween.ToAlpha(() => effectSprite.color, x => effectSprite.color = x, 1f, 0.3f));

        waitSequence.SetLoops(-1, LoopType.Yoyo);
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {   //터지길 대기하는 상태에서는 벌집 우선순위 제외
        return isStateWaitPang;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_1;
    }
}
