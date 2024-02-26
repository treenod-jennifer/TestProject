using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockBread : BlockBase
{
    public enum BreadPangType
    {
        SPLASH,     //스플레시 효과로 터졌을 때
        BOMB,       //폭탄으로 인해 터졌을 때
        CHAIN      //체인효과로 터졌을 때
    }

    [SerializeField] private UISprite breadMold;
    [SerializeField] private UISprite breadMold2;
    [SerializeField] private UISprite spritePlace;

    //빵 기믹 획득되는 연출 동시에 호출 몇 번 됐는지 확인하는 용도
    private int spriteSyncCount = 0;

    private TARGET_TYPE tType = TARGET_TYPE.BREAD_1;
    private int groupIndex;

    //해당 기믹을 pang시키는 pangIdx 저장
    //(여러번의 폭발이 동시에 동작할 때, 같은 폭발처리를 여러번 하지 않기 위해 사용)
    private List<int> listPangIdx = new List<int>();

    void Start()
    {
        UpdateSpriteByBlockType();
    }

    public override bool IsCanPang()
    {
        return true;
    }
    
    public override bool IsRemoveHideDeco_AtBlockPang()
    {
        return true;
    }

    public override bool IsBlockType()
    {
        return false;
    }

    public override bool IsCanLink()
    {
        return false;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsThisBlockHasPlace()
    {
        return true;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return true;
    }

    public override bool IsTarget_LavaMode()
    {
        return true;
    }

    public override void UpdateBlock()
    {
        if (state == BlockState.WAIT)
            _waitCount++;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = string.Format("Bread_{0}", groupIndex);
        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);

        spritePlace.depth = (int)GimmickDepth.DECO_GROUND;

        breadMold.depth = (int)GimmickDepth.DECO_PLACE;
        breadMold2.depth = (int)GimmickDepth.DECO_PLACE;
        breadMold.gameObject.SetActive(true);

        breadMold.spriteName = "Bread_BG_0";
        MakePixelPerfect(breadMold);

        breadMold2.spriteName = "bread_bg2";
        MakePixelPerfect(breadMold2);
    }
    public void InitBlock(int group)
    {
        groupIndex = group;
        SetTargetType();
        ManagerBlock.instance.AddBlockBreadInDictionary(groupIndex, this);
    }

    public int GetGroupIndex()
    {
        return groupIndex;
    }

    private void SetTargetType()
    {
        switch (groupIndex)
        {
            case 1:
                tType = TARGET_TYPE.BREAD_1;
                break;
            case 2:
                tType = TARGET_TYPE.BREAD_2;
                break;
            case 3:
                tType = TARGET_TYPE.BREAD_3;
                break;
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (lifeCount < 1)
            return;

        if (mBombEffect)
            return;

        if (listPangIdx.FindIndex(x => x == uniqueIndex) > -1)
            return;

        AddPangIndex(uniqueIndex);
        pangIndex = uniqueIndex;
        PangBread(BreadPangType.SPLASH);
    }

    /// <summary>
    /// 연쇄적인 폭발을 시킬 때 사용하는 함수
    /// </summary>
    public void SetChainPang(int uniqueIndex)
    {
        if (lifeCount < 1)
            return;

        if (blockDeco != null)
            return;

        //빵 기믹의 경우, pang 리스트로 검사하기 때문에 해당 코드를 사용하지 않음
        //if (pangIndex == uniqueIndex)
        //    return;

        pangIndex = uniqueIndex;
        PangBread(BreadPangType.CHAIN);
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount < 1)
            return;

        if (listPangIdx.FindIndex(x => x == pangIndex) > -1)
            return;

        AddPangIndex(pangIndex);
        PangBread(PangByBomb == true ? BreadPangType.BOMB : BreadPangType.SPLASH);
    }

    /// <summary>
    /// 빵 실제로 터뜨리는 코드(팡인덱스 변경없이)
    /// </summary>
    public void PangBread(BreadPangType pangType)
    {
        if (ManagerBlock.instance.HasAchievedCollectTarget(tType) == true)
            CollectAction();

        if (lifeCount <= 1 && state != BlockState.PANG && ManagerBlock.instance.HasAchievedCollectTarget(tType) == false)
        {   //모든 목표를 다 모았을 때 제거되는 처리
            ManagerBlock.instance.PangBlockBread(groupIndex);
        }
        else
        {
            StartCoroutine(CoPang());
            //체인으로 인한 폭발이 아니라면, 연결되어 있는 빵들을 터뜨려주는 처리를 함
            if (pangType != BreadPangType.CHAIN)
            {
                StartCoroutine(CoPangConnectedBread());
            }
        }
    }
    
    public override void PangDestroyBoardData()
    {
        base.PangDestroyBoardData();
        
        if (ManagerBlock.instance.dicBlockBread.ContainsKey(groupIndex) == false)
            return;

        List<BlockBread> listBread = ManagerBlock.instance.dicBlockBread[groupIndex];
        if(listBread.Contains(this))
            listBread.Remove(this);
        
        if(ManagerBlock.instance.dicBlockBread[groupIndex].Count == 0) 
            ManagerBlock.instance.dicBlockBread.Remove(groupIndex);
    }

    private void CollectAction()
    {
        //사운드 출력
        ManagerSound.AudioPlayMany(AudioInGame.BREAK_2_JAR);

        //이펙트 출력
        InGameEffectMaker.instance.MakeFlyTarget(_transform.position, tType);

        //스코어 출력
        int score = 80;
        ManagerBlock.instance.AddScore(score);
        InGameEffectMaker.instance.MakeScore(transform.position, score);
    }

    /// <summary>
    /// 인접해있는 빵기믹을 터뜨림
    /// </summary>
    private IEnumerator CoPangConnectedBread()
    {
        //인접해있는 빵 기믹의 리스트를 설정
        List<BlockBread> listConnectedBread = new List<BlockBread>();
        GetListConnectedBread(listConnectedBread, pangIndex);

        //연쇄적으로 터질 수 있는 빵 기믹이 없다면 반환
        if (listConnectedBread.Count == 0)
            yield break;

        //맨 처음 터진 위치에서부터 가까이 있는 블럭 순으로 정렬
        SortedList<int, List<BlockBread>> dicPangBread = null;
        SortListConnectedBread_ByDistance(out dicPangBread, listConnectedBread);

        foreach (var item in dicPangBread)
        {
            yield return new WaitForSeconds(0.05f);
            List<BlockBread> listPangBread = new List<BlockBread>(item.Value);
            for (int i = 0; i < listPangBread.Count; i++)
            {
                listPangBread[i].SetChainPang(pangIndex);
            }
        }
    }

    private void GetListConnectedBread(List<BlockBread> listConnectedBread, int pangIdx)
    {
        List<BlockBread> listTemp = new List<BlockBread>();

        //상,하,좌,우 인접해있는 블럭 검사
        BlockBread tempBlock_Up = GetConnectedBread(BlockDirection.UP, listConnectedBread, pangIdx);
        if (tempBlock_Up != null) listTemp.Add(tempBlock_Up);

        BlockBread tempBlock_Right = GetConnectedBread(BlockDirection.RIGHT, listConnectedBread, pangIdx);
        if (tempBlock_Right != null) listTemp.Add(tempBlock_Right);

        BlockBread tempBlock_Down = GetConnectedBread(BlockDirection.DOWN, listConnectedBread, pangIdx);
        if (tempBlock_Down != null) listTemp.Add(tempBlock_Down);

        BlockBread tempBlock_Left = GetConnectedBread(BlockDirection.LEFT, listConnectedBread, pangIdx);
        if (tempBlock_Left != null) listTemp.Add(tempBlock_Left);

        if (listTemp.Count > 0)
            listConnectedBread.AddRange(listTemp);

        for (int i = 0; i < listTemp.Count; i++)
        {
            listTemp[i].AddPangIndex(pangIdx);
            listTemp[i].GetListConnectedBread(listConnectedBread, pangIdx);
        }
    }


    private void SortListConnectedBread_ByDistance(out SortedList<int, List<BlockBread>> dicPangBread, List<BlockBread> listConnectedBread)
    {
        dicPangBread = new SortedList<int, List<BlockBread>>();
        for (int i = 0; i < listConnectedBread.Count; i++)
        {   //현재 기믹과 떨어진 거리에 따라 리스트에 추가
            Vector2Int distanceIdx = new Vector2Int(Mathf.Abs(indexX - listConnectedBread[i].indexX), Mathf.Abs(indexY - listConnectedBread[i].indexY));
            int distance = distanceIdx.x + distanceIdx.y;

            if (dicPangBread.ContainsKey(distance) == false)
                dicPangBread.Add(distance, new List<BlockBread>());

            dicPangBread[distance].Add(listConnectedBread[i]);
        }
    }

    /// <summary>
    /// 현재 기믹에 인접한 있는 같은 타입의 기믹 반환
    /// </summary>
    public BlockBread GetConnectedBread(BlockDirection dir, List<BlockBread> listConnectedBread, int pangIdx)
    {
        Vector2Int offset = Vector2Int.zero;
        switch (dir)
        {
            case BlockDirection.UP:
                offset = new Vector2Int(0, -1);
                break;
            case BlockDirection.DOWN:
                offset = new Vector2Int(0, 1);
                break;
            case BlockDirection.RIGHT:
                offset = new Vector2Int(1, 0);
                break;
            case BlockDirection.LEFT:
                offset = new Vector2Int(-1, 0);
                break;
        }

        //인접한 보드가 있는지 검사
        Board tempBoard = PosHelper.GetBoardSreeen(indexX, indexY, offset.x, offset.y);
        if (tempBoard == null) return null;

        //인접한 보드에 빵 기믹이 있는지 검사
        BlockBase tempBlock = tempBoard.Block;
        if (tempBlock == null || tempBlock.type != BlockType.BREAD)
            return null;

        //인접한 블럭과 연결이 가능한 상태인지 검사
        if (ManagerBlock.instance.IsCanLinkSideBLock(indexX, indexY, offset.x, offset.y) == false)
            return null;

        //인접한 기믹을 터뜨릴 수 있는 상태인지 검사
        BlockBread tempBread = tempBlock.GetComponent<BlockBread>();
        if (tempBread == null || tempBread.groupIndex != groupIndex)
            return null;

        //이미 리스트에 추가된 기믹인지 검사
        if (listConnectedBread.FindIndex(x => x == tempBread) > -1)
            return null;

        //pangIndex가 동일한지 검사
        if (tempBread.IsExistPangIndex(pangIdx) == true)
            return null;

        return tempBread;
    }

    IEnumerator CoPang()
    {
        spriteSyncCount++;

        //빵틀연출
        StartCoroutine(CoPangEffect());

        //빵연출
        //DOScale 실행 도중에 또 호출되는 경우 제거
        mainSprite.transform.DOKill();
        mainSprite.transform.localScale = new Vector3(0f, 0f, 0f);
        mainSprite.transform.DOScale(Vector3.one, ManagerBlock.instance.GetIngameTime(1.5f)).SetEase(Ease.InOutExpo);
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(1.5f));

        //연출 연쇄될 때, 연출 끝나는 타이밍
        spriteSyncCount--;
        if (spriteSyncCount == 0)
        {
            breadMold2.gameObject.SetActive(false);
            breadMold.color = new Color(1f, 1f, 1f, 1f);
            mainSprite.transform.localScale = Vector3.one;
        }
    }

    //빵틀이 달궈지는 연출 (빨갛게 깜빡이고 페이드인 페이드아웃)
    IEnumerator CoPangEffect()
    {
        float twinkleTimer = 0.1f;

        breadMold2.gameObject.SetActive(true);
        breadMold2.color = new Color(1f, 1f, 1f, 0.1f);
        breadMold.color = new Color(1f, 1f, 1f, 0.9f);

        DOTween.ToAlpha(() => breadMold2.color, x => breadMold2.color = x, 1f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        DOTween.ToAlpha(() => breadMold.color, x => breadMold.color = x, 0f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(twinkleTimer));

        //김모락 이펙트 생성
        InGameEffectMaker.instance.MakeEffectBreadEffect(_transform.position);

        DOTween.ToAlpha(() => breadMold2.color, x => breadMold2.color = x, 0f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        DOTween.ToAlpha(() => breadMold.color, x => breadMold.color = x, 1f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(twinkleTimer));

        DOTween.ToAlpha(() => breadMold2.color, x => breadMold2.color = x, 1f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        DOTween.ToAlpha(() => breadMold.color, x => breadMold.color = x, 0f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(twinkleTimer));

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));

        DOTween.ToAlpha(() => breadMold2.color, x => breadMold2.color = x, 0f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        DOTween.ToAlpha(() => breadMold.color, x => breadMold.color = x, 1f, ManagerBlock.instance.GetIngameTime(twinkleTimer));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(twinkleTimer));
    }

    public void PangBlockBread()
    {
        if (lifeCount < 1 || state == BlockState.PANG)
            return;

        lifeCount--;
        state = BlockState.PANG;
        StartCoroutine(CoPangFinal());
    }

    IEnumerator CoPangFinal()
    {
        //이펙트 생성
        InGameEffectMaker.instance.MakeEffectWorldRankItemPang(_transform.position);

        //스케일 연출
        float waitTime = ManagerBlock.instance.GetIngameTime(0.3f);
        mainSprite.transform.DOScale(0f, 0.3f).SetEase(Ease.InOutBack);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0.5f, waitTime);
        yield return new WaitForSeconds(0.3f);

        //아래 설치된 기믹 처리
        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();
        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null)
            back.SetUnderBoard();

        //데이터 제거
        PangDestroyBoardData();
        
        if (back != null)
            back.CheckStatus();
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (ManagerBlock.instance.HasAchievedCollectTarget(tType) == true && state != BlockState.PANG)
            return false;
        else
            return true;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_1;
    }

    /// <summary>
    /// 이미 해당 팡인덱스로 블럭이 터지는 처리가 되고 있는지 검사
    /// </summary>
    public bool IsExistPangIndex(int pIdx)
    {
        return (listPangIdx.FindIndex(x => x == pIdx) > -1);
    }

    public void AddPangIndex(int pIdx)
    {
        if (listPangIdx.FindIndex(x => x == pIdx) > -1)
            return;
        listPangIdx.Add(pIdx);
    }

    public void ResetPangIndexList()
    {
        listPangIdx.Clear();
    }
}
