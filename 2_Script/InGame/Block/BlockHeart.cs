using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BlockHeart : BlockBase
{
    const int GAUGEMAXCOUNT = 2;

    [SerializeField]
    public Transform heartActionRoot;

    [SerializeField]
    public UIBlockSprite heartGaugeSprite;

    [SerializeField]
    public UIBlockSprite heartGaugeBaseSprite;

    [SerializeField]
    public GameObject HeartWayObj;

    [SerializeField]
    public UILabel gauge_moveCount;

    [SerializeField]
    public GameObject effectShineObj;

    //하트 길
    private List<HeartWayBoardData> listHeartWayDatas = new List<HeartWayBoardData>();
    private List<HeartWayImage> listHeartWayObject = new List<HeartWayImage>();

    private class HeartWayBoardData
    {
        public HeartWayData data = null;    //하트길 데이터
        public BlockDirection moveDir = BlockDirection.NONE;    //이동할 방향

        public HeartWayBoardData(HeartWayData _data, BlockDirection _dir)
        {
            data = _data;
            moveDir = _dir;
        }
    }

    #region 이동 관리 변수
    //현재 턴에서의 상태 관련
    public enum HeartMoveState
    {
        NONE,   //기본 상태
        MOVE,   //배터리 이동함
        RESET,  //게이지 리셋
        STOP,   //이동 불가 상태(연출등장 후 END로 전환)
        END,    //이번 턴에서 이동 종료한 상태(연출도 등장하지 않음)
    }
    public HeartMoveState moveState = HeartMoveState.NONE;
    public HeartMoveState MoveState
    {
        get { return moveState; }
        set
        {
            if (moveState == value)
                return;

            if (value == HeartMoveState.STOP && moveState == HeartMoveState.MOVE)
            {   //이전에 이동한 기믹이 이번턴에 멈췄다면, 게이지 리셋
                moveState = HeartMoveState.RESET;
            }
            else
                moveState = value;
        }
    }
    #endregion

    //자리를 바꿀 블럭
    public BlockBase moveToNextBlock;

    public int heartIndex = 0;

    #region 게이지 및 칸 수 관련 변수
    //게이지 UI 높이
    private const int GAUGE_SPRITE_HEIGHT = 30;

    //게이지 UI 1개당 높이
    private int gaugeBarStep = 0;

    //게이지 스택
    private int _gaugeCount = 0;
    private int gaugeCount
    {
        get
        {
            return _gaugeCount;
        }
        set
        {
            if (_gaugeCount != value)
                _gaugeCount = value;
        }
    }

    //누적형 이동카운트
    private int _accMoveCount = 0;
    private int accMoveCount
    {
        get
        {
            return _accMoveCount;
        }
        set
        {
            _accMoveCount = value;
        }
    }

    //게이지 및 카운트 추가 연출 루틴
    private Coroutine gaugeActionRotine = null;
    private Coroutine countActionRotine = null;
    #endregion

    //같은 컬러타입이 있어서 우선순위에서 밀려난 경우
    public bool isActiveHeart_ByColorType = false;

    public override bool isCanPangByRainbow()
    {
        return false;
    }

    public override bool IsStopEvnetAtDestroyIce()
    {
        return true;
    }
    public override bool IsStopEvnetAtDestroyNet()
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
    public override bool IsStopEventAtMoveGround()
    {
        return true;
    }
    public override bool IsCanMakeCarpet()
    {
        return true;
    }
    public override bool IsCanMakeCarpetByBomb()
    {
        return true;
    }
    public override bool IsCanCoveredCarpet()
    {
        return true;
    }

    public override bool IsCanMakeBombFieldEffect()
    {
        return false;
    }

    public override bool IsDigTarget()
    {
        return true;
    }

    public override bool IsTarget_LavaMode()
    {
        return true;
    }

    public override void SetRandomBoxHide(bool isHide)
    {
        SetSpriteEnabled(isHide);
    }

    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
    }

    public override void SetMainSpriteDepth()
    {
        heartGaugeBaseSprite.depth = (int)GimmickDepth.BLOCK_BASE;
        heartGaugeSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE + 2;
        gauge_moveCount.depth = (int)GimmickDepth.BLOCK_BASE + 3;
    }

    public override void DestroyBlockData(int adType = -1)
    {
        //길 제거
        foreach (var item in listHeartWayObject)
            Destroy(item.gameObject);
        listHeartWayObject.Clear();
        listHeartWayDatas.Clear();
        
        listHeartWayDatas.Clear();
        RemoveHeartHomeByHeartIndex();
        base.DestroyBlockData((int)adType);
    }

    public void CheckCarpet()
    {
        //블럭 이동 후, 현재 보드에 카펫이 있다면, 해당 블럭에 카펫 생성
        Board tempBoard = PosHelper.GetBoard(indexX, indexY);
        if (tempBoard != null && tempBoard.IsExistCarpetAndCanExpand())
        {
            CoverBlockWithCarpet();
        }
    }

    public void InitBlock(int idx)
    {
        heartIndex = idx;
        ShowGaugeCountAction(gaugeCount, false);
        effectShineObj.gameObject.SetActive(false);

        ManagerBlock.instance.AddHeart(this);
        SetSprite();

        if (GameManager.instance.state == GameState.EDIT)
            gauge_moveCount.enabled = false;
    }

    public void InitHeartMoveState()
    {
        MoveState = HeartMoveState.NONE;
    }

    /// <summary>
    /// 하트 이동 상태 설정해주는 함수
    /// </summary>
    public void SetHeartMoveState()
    {
        #region 현재 블럭에 대한 이동 가능 검사
        if (listHeartWayDatas.Count <= 0 || listHeartWayDatas[0].moveDir == BlockDirection.NONE)
        {
            MoveState = HeartMoveState.END;
            return;
        }
        else if (indexX < GameManager.MinScreenX || indexX >= GameManager.MaxScreenX || indexY < GameManager.MinScreenY || indexY >= GameManager.MaxScreenY)
        { //화면에 출력되는 기믹인지
            MoveState = HeartMoveState.END;
            return;
        }
        else if (blockDeco != null && blockDeco.IsInterruptBlockSelect() == true)
        {   //얼음 기믹이 존재하는지
            MoveState = HeartMoveState.END;
            return;
        }
        else if (CheckMoveCount() == false)
        {   //이동 가능한 칸 수가 있는지
            MoveState = HeartMoveState.END;
            return;
        }
        #endregion

        BlockDirection nextDirection = listHeartWayDatas[0].moveDir;
        #region 이동할 다음 보드에 대한 검사
        Board nextBoard = PosHelper.GetBoardByDir(indexX, indexY, nextDirection);
        if (nextBoard == null)
        {
            MoveState = HeartMoveState.END;
            return;
        }
        else if (nextBoard.indexX < GameManager.MinScreenX || nextBoard.indexX >= GameManager.MaxScreenX || nextBoard.indexY < GameManager.MinScreenY || nextBoard.indexY >= GameManager.MaxScreenY)
        {   //다음 보드가 화면에 출력되는 범위인지
            MoveState = HeartMoveState.END;
            return;
        }
        else if (nextBoard.HasDecoCoverBlock())
        {
            MoveState = HeartMoveState.STOP;
            return;
        }
        else if (!nextBoard.IsCanMove())
        {
            MoveState = HeartMoveState.STOP;
            return;
        }
        else if (!nextBoard.IsActiveBoard)
        {
            MoveState = HeartMoveState.STOP;
            return;
        }
        else if (nextBoard.HasDecoHideBlock())
        {   //클로버 기믹이 존재하는지
            MoveState = HeartMoveState.STOP;
            return;
        }
        #endregion

        //하트 기믹이 다음 방향으로 움직일 수 있는지
        //(맵 이동 후, 게이지 리셋되기 위해 보드검사 다음에 실행)
        if (IsCanChangePositionWithoutMove(true, nextDirection, false) == false)
        {
            MoveState = HeartMoveState.STOP;
            return;
        }

        #region 위치교체할 다음 블럭에 대한 검사
        BlockBase nextBlock = PosHelper.GetBlockScreenByDir(indexX, indexY, nextDirection);
        if (nextBlock != null)
        {
            if (nextBlock.type == BlockType.HEART_HOME)
            {   //이동할 위치가 하트 홈일 경우 처리
                BlockHeartHome home = nextBlock as BlockHeartHome;
                if (home.HasListHeartIndex(heartIndex) == false)
                {   //다른 하트홈일 때 처리
                    MoveState = HeartMoveState.STOP;
                    return;
                }
            }
            else
            {
                if (nextBlock.IsCanChangePosition(false) == false)
                {   //다음 블럭과 위치 이동 불가할 경우.
                    MoveState = HeartMoveState.STOP;
                    return;
                }
            }
        }
        #endregion

        MoveState = HeartMoveState.MOVE;
    }

    /// <summary>
    /// 이동 가능한 칸수가 있는지 검사
    /// </summary>
    public bool CheckMoveCount()
    {
        if (accMoveCount <= 0)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 하트 기믹 이미지 설정
    /// </summary>
    public void SetSprite()
    {
        if (isActiveHeart_ByColorType == true)
        {
            mainSprite.spriteName = "blockHeart" + (int)colorType;
        }
        else
        {
            mainSprite.spriteName = "blockHeart_disabled";
        }
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE + 2;
        mainSprite.MakePixelPerfect();

        heartGaugeBaseSprite.spriteName = "blockHeartGauge";
        heartGaugeBaseSprite.depth = (int)GimmickDepth.BLOCK_BASE;
        heartGaugeSprite.spriteName = "blockHeartGauge";
        heartGaugeSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
        gauge_moveCount.depth = (int)GimmickDepth.BLOCK_BASE + 3;
    }

    public bool CheckNextBlock()
    {
        BlockDirection thisDirection = listHeartWayDatas[0].moveDir;
        //해당 방향으로 움직일 수 있는지
        if (IsCanChangePositionWithoutMove(true, thisDirection, false) == false)
        {
            return false;
        }

        BlockBase nextBlock = PosHelper.GetBlockScreenByDir(indexX, indexY, thisDirection);
        if (nextBlock == null)
        {
            return true;
        }

        if (nextBlock.type == BlockType.HEART_HOME)
        {
            if (nextBlock.IsCanChangePositionWithoutMove(false) == false)
            {
                return false;
            }

            BlockHeartHome home = nextBlock as BlockHeartHome;

            if (home.HasListHeartIndex(heartIndex) == false)
            {
                return false;
            }
        }
        else if (nextBlock.IsCanChangePosition(false) == false)
        {
            return false;
        }

        if (nextBlock.type == BlockType.HEART)
        {
            return false;
        }

        return true;
    }

    //교체될 최상위 다음기믹값(moveToNextBlock) 설정
    public void SetMoveToNextBlock()
    {
        if (listHeartWayDatas.Count <= 0 || listHeartWayDatas[0].moveDir == BlockDirection.NONE)
        {
            return;
        }

        BlockDirection thisDirection = listHeartWayDatas[0].moveDir;

        BlockBase upBlock = PosHelper.GetBlockScreenByDir(indexX, indexY, thisDirection);
        if (upBlock == null) return;

        //교체되지 않고 이동도 안되는 경우
        if (upBlock.type == BlockType.HEART
            || upBlock.type == BlockType.SODAJELLY
            || upBlock.type == BlockType.ColorBigJewel)
        {
            return;
        }
        else
        {
            moveToNextBlock = upBlock;
        }
    }

    public IEnumerator CoMoveAction()
    {
        switch (MoveState)
        {
            case HeartMoveState.MOVE:
                SetMoveToNextBlock();
                yield return CoMoveToWayHeart();
                break;
            case HeartMoveState.RESET:
                MoveState = HeartMoveState.END;
                InitGaugeAndCount();
                ShowMoveCountAction();
                ShowGaugeCountAction(gaugeCount, false);
                ShowGaugeResetAction(true);
                break;
            case HeartMoveState.STOP:
                MoveState = HeartMoveState.END;
                ShowGaugeResetAction(false);
                break;
        }
    }

    /// <summary>
    /// 하트 이동 연출
    /// </summary>
    public IEnumerator CoMoveToWayHeart() 
    {
        if (listHeartWayDatas.Count <= 0 || listHeartWayDatas[0].moveDir == BlockDirection.NONE)
            yield break;

        BlockDirection thisDirection = listHeartWayDatas[0].moveDir;
        if (moveToNextBlock != null && moveToNextBlock.type == BlockType.HEART_HOME)
        {   //하트 끝 기믹인 경우
            BlockHeartHome tempHeartHome = moveToNextBlock as BlockHeartHome;
            tempHeartHome.RemoveHeartIndex(heartIndex);

            if (tempHeartHome.GetListHeartIndexCount() <= 0)
            {
                //하트 끝 기믹까지 제거함
                MoveHeart(thisDirection, moveToNextBlock);
                TweenAlpha.Begin(mainSprite.gameObject, 2f, 0f);

                //하트 마지막 길 삭제
                listHeartWayDatas.RemoveAt(0);
                StartCoroutine(CoRemoveHeartWay());
                yield return tempHeartHome.CoPangImmediately();
            }
            else
            {
                //하트 끝 기믹을 다른 하트 기믹과 공유중
                switch (thisDirection)
                {
                    case BlockDirection.DOWN:
                        MoveToDirWithoutBlockSwitch(0, 1, 0.5f);
                        break;
                    case BlockDirection.LEFT:
                        MoveToDirWithoutBlockSwitch(-1, 0, 0.5f);
                        break;
                    case BlockDirection.RIGHT:
                        MoveToDirWithoutBlockSwitch(1, 0, 0.5f);
                        break;
                    case BlockDirection.UP:
                        MoveToDirWithoutBlockSwitch(0, -1, 0.5f);
                        break;
                }
                TweenAlpha.Begin(mainSprite.gameObject, 2f, 0f);

                //하트 마지막 길 삭제
                listHeartWayDatas.RemoveAt(0);
                StartCoroutine(CoRemoveHeartWay());
                yield return tempHeartHome.CoGetHeartEffect();
            }
        }
        else
        {
            MoveHeart(thisDirection, moveToNextBlock);
            //하트 길 삭제
            yield return CoRemoveHeartWay();
            listHeartWayDatas.RemoveAt(0);
        }

        //블럭 이동 후, 해당 보드에 카펫이 설치되어야 한다면 카펫 생성
        if (hasCarpet == true)
        {
            Board carpetBoard = PosHelper.GetBoard(indexX, indexY);
            carpetBoard.MakeCarpet();
        }
        else
        {
            CheckCarpet();
        }

        //이동칸 수 감소 및 연출
        accMoveCount--;
        ShowMoveCountAction();

        //다음 블럭 초기화
        moveToNextBlock = null;

        //이동할 칸 수가 0이 되었을 때의 설정
        if (accMoveCount <= 0)
        {
            gaugeCount = 0;
            ShowGaugeCountAction(gaugeCount, true);
            InGameEffectMaker.instance.MakeEffectBatteryCastEffect(effectShineObj.transform.position);
            effectShineObj.SetActive(false);
        }

        //블럭 이동 시간동안 대기
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.02f));

        if (listHeartWayDatas.Count == 0 || listHeartWayDatas[0].moveDir == BlockDirection.NONE)
        {
            lifeCount--;
            Pang();
        }
    }

    public void MoveHeart(BlockDirection _direction, BlockBase _block)
    {
        //이동시킬 속도 관련 변수(숫자가 클수록 빨라짐)
        float speedRatio = 0.5f;
        if (_block != null)
        {
            switch (_direction)
            {
                case BlockDirection.DOWN:
                    _block.MoveBlockToTargetPosition(0, -1, speedRatio);
                    MoveBlockToTargetPosition(0, 1, speedRatio);
                    break;
                case BlockDirection.LEFT:
                    _block.MoveBlockToTargetPosition(1, 0, speedRatio);
                    MoveBlockToTargetPosition(-1, 0, speedRatio);
                    break;
                case BlockDirection.RIGHT:
                    _block.MoveBlockToTargetPosition(-1, 0, speedRatio);
                    MoveBlockToTargetPosition(1, 0, speedRatio);
                    break;
                case BlockDirection.UP:
                    _block.MoveBlockToTargetPosition(0, 1, speedRatio);
                    MoveBlockToTargetPosition(0, -1, speedRatio);
                    break;
            }
        }
        else
        {
            //다음 블럭이 빈 블럭인 경우
            switch (_direction)
            {
                case BlockDirection.DOWN:
                    MoveBlockToTargetPosition(0, 1, speedRatio, false);
                    break;
                case BlockDirection.LEFT:
                    MoveBlockToTargetPosition(-1, 0, speedRatio, false);
                    break;
                case BlockDirection.RIGHT:
                    MoveBlockToTargetPosition(1, 0, speedRatio, false);
                    break;
                case BlockDirection.UP:
                    MoveBlockToTargetPosition(0, -1, speedRatio, false);
                    break;
            }
        }
    }

    public IEnumerator CoSetActiveHeartImage_ByColorType(bool isActive, bool isShowAction)
    {
        isActiveHeart_ByColorType = isActive;
        if (isShowAction == true)
        {
            float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
            heartActionRoot.transform.DOPunchScale(new Vector3(1.1f, 1.1f, 0f), actionTime);
            InGameEffectMaker.instance.MakeEffectWorldRankItemPang(heartActionRoot.position);
            yield return new WaitForSeconds(actionTime);
        }
        SetSprite();
    }

    public override void UpdateBlock()
    {
        MoveBlock();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        StartCoroutine(CoPangFinal());
    }

    private IEnumerator CoPangFinal()
    {
        IsSkipPang = true;
        isSkipDistroy = true;
        DestroyBlockData();

        ManagerBlock.instance.RemoveHeart(this);
        PangDestroyBoardData();

        //다음 하트 활성화
        ManagerBlock.instance.SetActiveNextHeart(colorType);
        yield return null;
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return false;
        else
            return true;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }

    #region 게이지, 칸 수 관련 함수
    public void InitGaugeAndCount()
    {
        gaugeCount = 0;
        accMoveCount = 0;
    }

    /// <summary>
    /// 게이지 추가하는 함수
    /// </summary>
    public void AddMoveGaugeCount(int count)
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);

        if (board == null) return;

        //게이지 추가를 방해하는 기믹이 있다면 동작하지 않음
        bool hasInterruptDeco = board.DecoOnBoard.FindIndex(x => x.IsInterruptBlockEvent() == true) > -1;
        if (hasInterruptDeco == true || (blockDeco != null && blockDeco.IsInterruptBlockEvent() == true))
            return;

        //게이지 카운트 추가, 이동 카운트 계산
        gaugeCount += count;
        accMoveCount = (int)(gaugeCount / GAUGEMAXCOUNT);

        //게이지 추가 연출
        ShowGaugeCountAction(gaugeCount, true);

        //이동할 수 있는 카운트가 있는지에 따라 처리 
        if (accMoveCount > 0)
        {
            //이펙트 연출
            if (effectShineObj.activeInHierarchy == false)
            {
                InGameEffectMaker.instance.MakeEffectBatteryCastEffect(effectShineObj.transform.position);
                effectShineObj.SetActive(true);
            }

            //카운트 숫자 연출
            if (accMoveCount > 5)
                ShowMoveCountAction(true);
            else
                ShowMoveCountAction();
        }
    }

    #region 이동 카운트 추가/감소 연출
    private void ShowMoveCountAction(bool isOverAction = false)
    {
        if (countActionRotine != null)
        {
            StopCoroutine(countActionRotine);
            countActionRotine = null;
            gauge_moveCount.transform.DOKill();
            gauge_moveCount.transform.localScale = Vector3.one;
        }
        countActionRotine = StartCoroutine(CoAddMoveCountAction(isOverAction));
    }

    private IEnumerator CoAddMoveCountAction(bool isOverAction = false, float _actionTime = 0.3f)
    {
        float actionScale = 1.05f;
        float actionTime = _actionTime;

        //더 크게 출력하는 경우
        if (isOverAction)
        {
            actionScale = 1.3f;
            actionTime = 0.5f;
        }

        gauge_moveCount.transform.DOPunchScale(new Vector3(actionScale, actionScale, 0f), actionTime, 1);
        gauge_moveCount.text = accMoveCount.ToString();
        yield return new WaitForSeconds(actionTime);
    }
    #endregion

    #region 게이지 추가 연출
    private void ShowGaugeCountAction(int count, bool isAction = true)
    {
        if (gaugeActionRotine == null)
        {
            gaugeActionRotine = StartCoroutine(CoGaugeCountAction(count, isAction));
        }
    }

    private IEnumerator CoGaugeCountAction(int count, bool isAction)
    {
        heartGaugeSprite.enabled = true;

        //남아있는 카운트 없으면, 게이지 이미지 보이지 않도록 설정
        if (count == 0)
        {
            heartGaugeSprite.enabled = false;
            gaugeBarStep = 0;
        }
        //게이지 하나 채워진 경우
        else if (count == 1
            && gaugeBarStep < GAUGE_SPRITE_HEIGHT / 2)
        {
            gaugeBarStep = GAUGE_SPRITE_HEIGHT / 2;
            heartGaugeSprite.height = gaugeBarStep;
        }
        //게이지 2개 이상 채워진 경우
        else if (count >= 2
            && gaugeBarStep < GAUGE_SPRITE_HEIGHT)
        {
            if (isAction == true)
            {
                float actionTime = ManagerBlock.instance.GetIngameTime(0.12f);

                gaugeBarStep = GAUGE_SPRITE_HEIGHT / 2;
                heartGaugeSprite.height = gaugeBarStep;
                yield return new WaitForSeconds(actionTime);

                gaugeBarStep = GAUGE_SPRITE_HEIGHT;
                heartGaugeSprite.height = gaugeBarStep;
            }
            else
            {
                gaugeBarStep = GAUGE_SPRITE_HEIGHT;
                heartGaugeSprite.height = gaugeBarStep;
            }
        }

        gaugeActionRotine = null;
    }
    #endregion

    #endregion

    #region 하트 움직임 연출 관련 코드
    /// <summary>
    ///  게이지 리셋되는 연출
    /// </summary>
    public void ShowGaugeResetAction(bool isReset)
    {
        if (listHeartWayDatas.Count > 0 && listHeartWayDatas[0].moveDir != BlockDirection.NONE)
        {
            BlockDirection wayDir = listHeartWayDatas[0].moveDir;
            if (wayDir == BlockDirection.DOWN || wayDir == BlockDirection.UP)
                StartCoroutine(CoGaugeResetAction_Vertical(wayDir, isReset));
            else if (wayDir == BlockDirection.LEFT || wayDir == BlockDirection.RIGHT)
                StartCoroutine(CoGaugeResetAction_Horizon(wayDir, isReset));
        }
    }

    public IEnumerator CoGaugeResetAction_Vertical(BlockDirection moveDir, bool isReset)
    {
        float yOffset = (moveDir == BlockDirection.UP) ? -1f : 1f;

        //찌글
        float actionTime = ManagerBlock.instance.GetIngameTime(0.08f);
        heartActionRoot.DOLocalMoveY(-25f * yOffset, actionTime);
        heartActionRoot.DOScale(new Vector3(1.15f, 0.7f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //튕겨나기
        actionTime = ManagerBlock.instance.GetIngameTime(0.15f);
        heartActionRoot.transform.DOLocalMoveY(10f * yOffset, actionTime);
        heartActionRoot.DOScale(new Vector3(0.95f, 1f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //배터리 소모되는 연출
        if (isReset == true)
        {
            actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
            ManagerSound.AudioPlay(AudioInGame.DYNAMITE_MATCH);
            InGameEffectMaker.instance.MakeEffectBatterySparkEffect(heartActionRoot.position);
            effectShineObj.SetActive(false);
            yield return new WaitForSeconds(actionTime);
        }
        else
        {
            ManagerSound.AudioPlay(AudioInGame.DYNAMITE_COUNT_DOWN);
        }

        //돌아오는 연출
        actionTime = ManagerBlock.instance.GetIngameTime(0.1f);
        heartActionRoot.transform.DOLocalMoveY(-5f * yOffset, actionTime);
        heartActionRoot.DOScale(new Vector3(1.15f, 0.7f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //원래 상태로 돌아옴
        heartActionRoot.transform.DOLocalMoveY(0f, actionTime);
        heartActionRoot.DOScale(new Vector3(1f, 1f, 0f), actionTime);
    }

    public IEnumerator CoGaugeResetAction_Horizon(BlockDirection moveDir, bool isReset)
    {
        float yOffset = (moveDir == BlockDirection.RIGHT) ? -1f : 1f;

        //찌글
        float actionTime = ManagerBlock.instance.GetIngameTime(0.08f);
        heartActionRoot.DOLocalMoveX(-25f * yOffset, actionTime);
        heartActionRoot.DOScale(new Vector3(0.7f, 1.15f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //튕겨나기
        actionTime = ManagerBlock.instance.GetIngameTime(0.15f);
        heartActionRoot.transform.DOLocalMove(new Vector3(-5f * yOffset, 10f, 0f), actionTime);
        heartActionRoot.DOScale(new Vector3(1f, 0.95f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //배터리 소모되는 연출
        if (isReset == true)
        {
            actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
            ManagerSound.AudioPlay(AudioInGame.DYNAMITE_MATCH);
            InGameEffectMaker.instance.MakeEffectBatterySparkEffect(heartActionRoot.position);
            effectShineObj.SetActive(false);
            yield return new WaitForSeconds(actionTime);
        }
        else
        {
            ManagerSound.AudioPlay(AudioInGame.DYNAMITE_COUNT_DOWN);
        }

        //돌아오는 연출
        actionTime = ManagerBlock.instance.GetIngameTime(0.1f);
        heartActionRoot.transform.DOLocalMove(new Vector3(0f, -5f, 0f), actionTime);
        heartActionRoot.DOScale(new Vector3(1.15f, 0.7f, 0f), actionTime);
        yield return new WaitForSeconds(actionTime);

        //원래 상태로 돌아옴
        heartActionRoot.transform.DOLocalMoveY(0f, actionTime);
        heartActionRoot.DOScale(new Vector3(1f, 1f, 0f), actionTime);
    }
    #endregion

    #region 하트 길 데이터 관련
    /// <summary>
    /// 하트 길 오브젝트 생성
    /// </summary>
    public void MakeHeartWay()
    {
        //길 데이터 생성
        InitListHeartWayData();

        //생성된 길 데이터를 바탕으로 길 오브젝트 생성
        InitListHeartWayObject();
    }

    private void InitListHeartWayData()
    {
        Vector2Int? prevIdx = null;
        Vector2Int? curIdx = null;
        Vector2Int? nextIdx = null;

        listHeartWayDatas.Clear();
        int gimmickIndex = ManagerBlock.instance.stageInfo.listBlockInfo.FindIndex(x => x.gimmickType == (int)BlockType.HEART);
        if (gimmickIndex > -1)
        {
            if (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] is GimmickInfo_BlockHeart == false)
                return;

            List<HeartWayInfo> tempInfolist = (ManagerBlock.instance.stageInfo.listBlockInfo[gimmickIndex] as GimmickInfo_BlockHeart).listHeartWayInfo;
            if (tempInfolist != null && tempInfolist.Count > 0)
            {
                int tempInfoindex = tempInfolist.FindIndex(x => x.heartIndex == heartIndex);
                if (tempInfoindex > -1 && tempInfolist[tempInfoindex].listHeartWayData != null && tempInfolist[tempInfoindex].listHeartWayData.Count > 0)
                {
                    for (int i = 0; i < tempInfolist[tempInfoindex].listHeartWayData.Count; i++)
                    {
                        prevIdx = (i == 0) ? new Vector2Int(indexX, indexY) : curIdx;
                        HeartWayData tempData_1 = tempInfolist[tempInfoindex].listHeartWayData[i];
                        curIdx = new Vector2Int(tempData_1.indexX, tempData_1.indexY);
                        BlockDirection curDir = PosHelper.GetDir(prevIdx.Value.x, prevIdx.Value.y, curIdx.Value.x, curIdx.Value.y);
                        listHeartWayDatas.Add(new HeartWayBoardData(tempData_1, curDir));
                    }
                }
            }
        }
        //순서대로 길 정렬
        listHeartWayDatas.Sort((HeartWayBoardData a, HeartWayBoardData b) => a.data.wayCount.CompareTo(b.data.wayCount));
    }

    private void InitListHeartWayObject()
    {
        if (listHeartWayDatas.Count <= 0)
            return;

        //길 이미지의 방향
        BlockDirection prevDirection = BlockDirection.NONE;
        BlockDirection curDirection = BlockDirection.NONE;
        BlockDirection nextDirection = BlockDirection.NONE;

        Vector2Int? prevIdx = null;
        Vector2Int? curIdx = null;
        Vector2Int? nextIdx = null;

        int dataCount = listHeartWayDatas.Count;
        List<Vector2Int> listPrevWayIdx = new List<Vector2Int>();
        for (int i = 0; i < dataCount; i++)
        {
            //현재, 이전, 다음 데이터 저장
            prevIdx = (i == 0) ? new Vector2Int(indexX, indexY) : curIdx;
            curIdx = new Vector2Int(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY);
            if ((i + 1) >= dataCount)
                nextIdx = null;
            else
                nextIdx = new Vector2Int(listHeartWayDatas[i + 1].data.indexX, listHeartWayDatas[i + 1].data.indexY);

            //현재, 이전, 다음 길이 향하는 방향 저장
            prevDirection = (prevIdx == null) ? BlockDirection.NONE : curDirection;
            curDirection = PosHelper.GetDir(prevIdx.Value.x, prevIdx.Value.y, curIdx.Value.x, curIdx.Value.y);
            nextDirection = (nextIdx == null) ? BlockDirection.NONE :
                PosHelper.GetDir(curIdx.Value.x, curIdx.Value.y, nextIdx.Value.x, nextIdx.Value.y);

            if (nextDirection != BlockDirection.NONE && curDirection == nextDirection)
            {   //현재와 다음 길이 같은 방향의 길이라면, 현재 길을 리스트에 추가시키고 다음 보드 검사
                listPrevWayIdx.Add(curIdx.Value);
                continue;
            }
            else
            {
                //길 방향이 변경되었다면, 이전 위치에 이어진 직선길 오브젝트 생성
                if (listPrevWayIdx.Count > 0)
                {
                    InstantiateHeartWay(listPrevWayIdx, HeartWayImage.WAYSPRITE_DIR.SPRITE_STRAIGHT, prevDirection, prevDirection);
                }

                List<Vector2Int> listCurWayIdx = new List<Vector2Int>();
                listCurWayIdx.Add(curIdx.Value);
                if (nextIdx == null)
                {   //다음 길이 없다면, 현재 위치가 골로 향하는 직선길 생성
                    InstantiateHeartWay(listCurWayIdx, HeartWayImage.WAYSPRITE_DIR.END, curDirection, curDirection);
                }
                else
                {   //다음 길이 있다면, 다음 길의 위치를 받아와 꺾인 곡선길 생성
                    BlockDirection spriteDirection = GetSpriteDirection(curDirection, nextDirection);
                    InstantiateHeartWay(listCurWayIdx, HeartWayImage.WAYSPRITE_DIR.SPRITE_CURVE, spriteDirection, nextDirection);
                }

                //저장하고 있던 이전 데이터 초기화
                listPrevWayIdx.Clear();
            }
        }

        if (listHeartWayObject.Count > 0)
            listHeartWayObject[0].SetStartWay();
    }

    /// <summary>
    /// 이전/이후 상태에 따른 방향정보를 가져오는 함수
    /// </summary>
    private BlockDirection GetSpriteDirection(BlockDirection currentDir, BlockDirection nextDir)
    {
        BlockDirection moveDir = BlockDirection.NONE;
        if (currentDir == nextDir || nextDir == BlockDirection.NONE)
        {   //이전/이후가 같은 상태이거나 다음 위치가 길 마지막인 경우.
            moveDir = currentDir;
        }
        else if ((currentDir == BlockDirection.DOWN && nextDir == BlockDirection.RIGHT)
            || (currentDir == BlockDirection.LEFT && nextDir == BlockDirection.UP))
        {
            moveDir = BlockDirection.DOWN_RIGHT;
        }
        else if ((currentDir == BlockDirection.UP && nextDir == BlockDirection.RIGHT)
            || (currentDir == BlockDirection.LEFT && nextDir == BlockDirection.DOWN))
        {
            moveDir = BlockDirection.UP_RIGHT;
        }
        else if ((currentDir == BlockDirection.DOWN && nextDir == BlockDirection.LEFT)
            || (currentDir == BlockDirection.RIGHT && nextDir == BlockDirection.UP))
        {
            moveDir = BlockDirection.DOWN_LEFT;
        }
        else if ((currentDir == BlockDirection.UP && nextDir == BlockDirection.LEFT)
            || (currentDir == BlockDirection.RIGHT && nextDir == BlockDirection.DOWN))
        {
            moveDir = BlockDirection.UP_LEFT;
        }
        return moveDir;
    }

    /// <summary>
    /// 하트 길 생성 및 초기화
    /// </summary>
    private void InstantiateHeartWay(List<Vector2Int> listIdx, HeartWayImage.WAYSPRITE_DIR wState, BlockDirection sDir, BlockDirection mDir)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.groundAnchor, HeartWayObj);
        ManagerBlock.instance.listObject.Add(obj);

        Vector2Int lastIdx = listIdx[listIdx.Count - 1];
        obj.transform.localPosition = PosHelper.GetPosByIndex(lastIdx.x, lastIdx.y);

        HeartWayImage heartWayObject = obj.GetComponent<HeartWayImage>();
        heartWayObject.InitHeartWay(listIdx, wState, sDir, mDir);

        listHeartWayObject.Add(heartWayObject);
    }

    public int GetHeartWayCount()
    {
        return listHeartWayDatas.Count;
    }

    //화면에 출력되는 길이 있는지
    public bool HasHeartWayOnScreen()
    {
        foreach (var way in listHeartWayDatas)
        {
            if (PosHelper.InExistScreen(way.data.indexX, way.data.indexY))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 툴에서 길에 관한 가이드 출력
    /// </summary>
    public void MakeGuide_Edit()
    {
        int tempX = indexX;
        int tempY = indexY;
        bool isConnected = true;

        List<GameObject> listGuideObj = new List<GameObject>();
        if (EditManager.instance.dicGimmickGuide.ContainsKey(type) == false)
            EditManager.instance.dicGimmickGuide.Add(type, listGuideObj);

        for (int i = 0; i < listHeartWayDatas.Count; i++)
        {
            HeartWayData way = listHeartWayDatas[i].data;
            Vector3 pos = PosHelper.GetPosByIndex(way.indexX, way.indexY);

            //길 번호 출력
            UILabel label = MakeLabel(EditManager.instance.mEditAnchor, way.wayCount.ToString(), pos);
            listGuideObj.Add(label.gameObject);

            //하트 길이 연결되었는지 확인하기
            int diff = System.Math.Abs(way.indexX - tempX) + System.Math.Abs(way.indexY - tempY);
            GameObject obj = null;
            if (isConnected == true && diff == 1)
            {
                tempX = way.indexX;
                tempY = way.indexY;
                isConnected = true;
                obj = MakeFrame(GameUIManager.instance.groundAnchor, Color.blue, pos);
            }
            else
            {
                isConnected = false;
                obj = MakeFrame(GameUIManager.instance.groundAnchor, Color.red, pos);
            }

            if (obj != null)
                listGuideObj.Add(obj);
        }
        EditManager.instance.dicGimmickGuide[type].AddRange(listGuideObj);
    }

    public IEnumerator CoRemoveHeartWay()
    {
        if (listHeartWayObject.Count <= 0)
            yield break;

        //블럭이 이동하는 동안 길 사이즈 조절
        StartCoroutine(listHeartWayObject[0].CoRemoveHeartWay());
        yield return new WaitUntil(() => state != BlockState.MOVE_OTHER);

        //남은 길이 없다면 해당 길 오브젝트 삭제
        if (listHeartWayObject[0].listWayIdx.Count <= 0)
        {
            Destroy(listHeartWayObject[0].gameObject);
            listHeartWayObject.RemoveAt(0);

            //다음 하트길의 길이를 배터리 아래로 덮일수 있도록 조절
            if (listHeartWayObject.Count > 0)
            {
                listHeartWayObject[0].SetStartWay();
            }
        }
    }

    private GameObject MakeFrame(GameObject obj, Color color, Vector3 pos)
    {
        UISprite uiSprite = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockFrameSpriteObj).GetComponent<UISprite>();
        uiSprite.depth = (int)GimmickDepth.UI_LABEL + 1;
        uiSprite.color = color;
        uiSprite.transform.localPosition = pos;
        return uiSprite.gameObject;
    }

    private UILabel MakeLabel(GameObject obj, string text, Vector3 tr)
    {
        if (GameManager.instance.state != GameState.EDIT) return null;

        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = new Color(124f / 255f, 40f / 255f, 40f / 255f);
        uiLabel.color = Color.white;
        uiLabel.text = text;
        uiLabel.fontSize = 25;

        uiLabel.transform.localPosition = tr;
        return uiLabel;
    }

    public List<Vector2Int> GetListHeartWayPos()
    {
        List<Vector2Int> listHeartWayPos = new List<Vector2Int>();
        for (int i = 0; i < listHeartWayDatas.Count; i++)
        {
            Vector2Int vecPos = new Vector2Int(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY);
            listHeartWayPos.Add(vecPos);
        }
        return listHeartWayPos;
    }
    #endregion

    #region 하트 끝 관련

    public void RemoveHeartHome(bool removeOnly)
    {
        for (int i = 0; i < listHeartWayDatas.Count; i++)
        {
            BlockInfo removeInfo = null;

            //내 하트 홈만 삭제하려는 경우
            if (removeOnly)
            {
                BlockHeartHome tempHome = ManagerBlock.instance.listHeartHome.Find(x => x.indexX == listHeartWayDatas[i].data.indexX && x.indexY == listHeartWayDatas[i].data.indexY);
                if (tempHome != null && tempHome.HasListHeartIndex(heartIndex) == true)
                {
                    tempHome.RemoveHeartIndex(heartIndex);
                    if (tempHome.GetListHeartIndexCount() <= 0)
                    {
                        removeInfo = ManagerBlock.instance.GetBlockInfo(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY);
                    }
                }
            }
            else
            {
                //상관없이 길 위에 설치된 하트홈 등 관련 내용 전체 삭제하는 경우
                removeInfo = ManagerBlock.instance.GetBlockInfo(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY);
            }

            if (removeInfo != null && removeInfo.type == (int)BlockType.HEART_HOME)
            {
                removeInfo.type = (int)BlockType.NORMAL;
                removeInfo.colorType = (int)BlockColorType.RANDOM;
                removeInfo.index = 0;
                removeInfo.count = 0;
                removeInfo.subType = 0;

                Board getBoard = PosHelper.GetBoard(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY);
                if (getBoard.Block != null)
                {
                    getBoard.Block.DestroyBlockData();
                    getBoard.Block.PangDestroyBoardData();
                    getBoard.Block = BlockMaker.instance.MakeBlockBase(listHeartWayDatas[i].data.indexX, listHeartWayDatas[i].data.indexY, BlockType.NORMAL, BlockColorType.NONE);
                }
            }
        }
    }

    public void MakeHeartHome_LastHeartWay()
    {
        HeartWayData endOfWay = GetLastHeartWay();
        if (endOfWay != null)
        {
            int endIndexX = endOfWay.indexX;
            int endIndexY = endOfWay.indexY;
            BlockInfo blockInfo = ManagerBlock.instance.GetBlockInfo(endIndexX, endIndexY);

            blockInfo.type = (int)BlockType.HEART_HOME;

            Board getBoard = PosHelper.GetBoard(endIndexX, endIndexY);
            if (getBoard.Block != null)
            {
                getBoard.Block.DestroyBlockData();
                getBoard.Block.PangDestroyBoardData();
            }
            getBoard.Block = BlockMaker.instance.MakeBlockBase(endIndexX, endIndexY, (BlockType)blockInfo.type, (BlockColorType)blockInfo.colorType, blockInfo.count, blockInfo.index, blockInfo.subType);
        }
    }

    public HeartWayData GetLastHeartWay()
    {
        int dataCount = listHeartWayDatas.Count;
        if (dataCount > 0)
            return listHeartWayDatas[dataCount - 1]?.data;
        return null;
    }

    public void RemoveHeartHomeByHeartIndex()
    {
        ManagerBlock.instance.SetHeartHomeIndex();
    }

    #endregion

    #region 튜토리얼 이동 카운트 연출

    //이동 카운트 추가 연출(연속)
    public IEnumerator TutorialStart_ShowMoveCountAction()
    {
        while (true)
        {
            countActionRotine = StartCoroutine(CoAddMoveCountAction(_actionTime: 0.7f));
            yield return countActionRotine;
        }
    }

    //이동 카운트 추가 연출 제거
    public void TutorialStop_ShowMoveCountAction()
    {
        if (countActionRotine != null)
        {
            StopCoroutine(countActionRotine);
            countActionRotine = null;
            gauge_moveCount.transform.DOKill();
            gauge_moveCount.transform.localScale = Vector3.one;
        }
    }

    #endregion
}
