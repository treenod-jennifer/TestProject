using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum GROUND_TYPE
{
    NORMAL,
    JEWEL,
    LINE,
    CIRCLE,
    APPLE,
    KEY,
    LINE_V,
    LINE_H,
    ICE_APPLE,
    DUCK,
    CANDY,
    BlOCKBLACK,
}

public class BlockGround : BlockBase
{
    public GROUND_TYPE groundType = GROUND_TYPE.NORMAL;

    bool isDepome = false;
    public int appleCount = 1;

    public UIBlockSprite _bgSprite;
    public UIBlockSprite _bgSprite2;

    public UIBlockSprite _castle;

    public bool isCanShining = true;

    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;

        if(_bgSprite != null) _bgSprite.enabled = setEnabled;
        if(_bgSprite2 != null) _bgSprite2.enabled = setEnabled;
        if(_castle != null) _castle.enabled = setEnabled;
    }

    public override void DestroyBlockData(int adType = -1)
    {
        //전체수/화면수에 영향 주지 않는 경우 확인
        if (ManagerBlock.instance.CheckExcept_AdjustBlock(type, (int)groundType) == false)
        {
            return;
        }

        base.DestroyBlockData((int)adType);
    }

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool isCanPangByRainbow()
    {
        return false;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsDigTarget()
    {
        if (type == BlockType.GROUND_JEWEL || type == BlockType.GROUND_KEY) return true;

        return groundType == GROUND_TYPE.JEWEL || groundType == GROUND_TYPE.KEY;
    }

    public override bool IsDepomerBlock()
    {
        return isDepome;
    }

    public override bool IsCanCoverIce()
    {
        if (type == BlockType.GROUND_JEWEL && lifeCount <= 1)
            return true;
        return false;
    }

    public override bool IsCoverCarpet()
    {
        if (type == BlockType.GROUND_JEWEL && lifeCount <= 1)
            return false;
        return true;
    }
    
    public void SetEffectHide(bool isHide)
    {
        isCanShining = isHide;
    }

    public override void SetRandomBoxHide(bool isHide)
    {
        base.SetRandomBoxHide(isHide);
        SetEffectHide(isHide);
    }

    public override void SetHideBeforeCloverPang()
    {
        mainSprite.alpha = 1;
        SetSpriteEnabled(true);
    }

    public override void SetCloverHide(bool isHide)
    {
        // Wait 상태 진입 후 클로버 아래에 진입해 있으면 사라지는 애니메이션 재생
        if (!isHide && mainSprite.enabled)
        {
            Board board = PosHelper.GetBoardSreeen(indexX, indexY);
            bool isPang = false;
            foreach (var item in board.DecoOnBoard)
                if (item is Clover && (item as Clover).isPang) return;
            
            DOTween.Sequence()
                .Append(DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0, 0.2f))
                .OnComplete(() => SetSpriteEnabled(false));
        }
        // Move 상태라면 무조건 보이게끔 활성화
        else
        {
            mainSprite.alpha = 1;
            SetSpriteEnabled(isHide);
        }

        SetEffectHide(isHide);
    }


    public override void SetMainSpriteDepth()
    {
         mainSprite.depth = (int)GimmickDepth.BLOCK_GROUND + 1;
        if (_castle != null) _castle.depth = (int)GimmickDepth.BLOCK_GROUND + 2;
        if (_bgSprite != null) _bgSprite.depth = (int)GimmickDepth.BLOCK_GROUND;

        if (groundType == GROUND_TYPE.JEWEL && lifeCount <= 1)mainSprite.depth = (int)GimmickDepth.BLOCK_BASE;       

    }

    public override void MakeDummyImage(Vector3 tartgetPos)
    {
        base.MakeDummyImage(tartgetPos);

        if (_castle != null)
        {
            UIBlockSprite castleSprite = NGUITools.AddChild(gameObject, BlockMaker.instance.blockDummySpriteObj).GetComponent<UIBlockSprite>();
            castleSprite.spriteName = _castle.spriteName;
            castleSprite.depth = _castle.depth;
            MakePixelPerfect(castleSprite);
            castleSprite.cachedTransform.localPosition = tartgetPos;
            dummySpriteList.Add(castleSprite.customFill);
        }

        if (_bgSprite != null)
        {
            UIBlockSprite bgSpriteSprite = NGUITools.AddChild(gameObject, BlockMaker.instance.blockDummySpriteObj).GetComponent<UIBlockSprite>();
            bgSpriteSprite.spriteName = _bgSprite.spriteName;
            bgSpriteSprite.depth = _bgSprite.depth;
            MakePixelPerfect(bgSpriteSprite);
            bgSpriteSprite.cachedTransform.localPosition = tartgetPos;
            dummySpriteList.Add(bgSpriteSprite.customFill);

            UIBlockSprite lowBgSprite = bgSpriteSprite.gameObject.GetComponentInChildren<UIBlockSprite>();
            if (lowBgSprite != null) dummySpriteList.Add(lowBgSprite.customFill);
        }
    }

    public override void SpriteRatio(float ratio, float vertRatio = 0)
    {
        if (_castle != null)
        {
            _castle.customFill.verticalRatio = vertRatio;
            _castle.customFill.blockRatio = ratio;
        }
        if (_bgSprite != null)
        {
            _bgSprite.customFill.verticalRatio = vertRatio;
            _bgSprite.customFill.blockRatio = ratio;

            _bgSprite2.customFill.verticalRatio = vertRatio;
            _bgSprite2.customFill.blockRatio = ratio;
        }
    }

    public override void UpdateSpriteByBlockType()
    {
        string castleSpriteName = "";

        switch (groundType)
        {
            case GROUND_TYPE.JEWEL:
                if (lifeCount <= 1)
                {
                    if (_bgSprite != null) Destroy(_bgSprite.gameObject);
                    if (_castle != null) Destroy(_castle.gameObject);

                    mainSprite.spriteName = "blockGround_Jewel";
                    isDepome = true;
                }
                else
                {
                    castleSpriteName = string.Format("ingame_castle_{0}_Jewel", (lifeCount - 1));
                }
                break;

            case GROUND_TYPE.APPLE:
                castleSpriteName = string.Format("ingame_castle_{0}_Apple", lifeCount);
                break;

            case GROUND_TYPE.ICE_APPLE:
                castleSpriteName = string.Format("ingame_castle_{0}_IceApple", lifeCount);
                break;

            case GROUND_TYPE.KEY:
                castleSpriteName = string.Format("ingame_castle_{0}_Key", lifeCount);
                break;

            case GROUND_TYPE.LINE:
            case GROUND_TYPE.LINE_V:
            case GROUND_TYPE.LINE_H:
                castleSpriteName = string.Format("ingame_castle_{0}_Line", lifeCount);
                break;

            case GROUND_TYPE.CIRCLE:
                castleSpriteName = string.Format("ingame_castle_{0}_Circle", lifeCount);
                break;
            case GROUND_TYPE.BlOCKBLACK:
                castleSpriteName = string.Format("ingame_castle_{0}_Blockblack", lifeCount);
                break;
            case GROUND_TYPE.DUCK:
                castleSpriteName = string.Format("ingame_castle_{0}_Duck", lifeCount);
                break;
            default:
                if (_castle != null)
                {
                    if (lifeCount == 1)
                        _castle.color = new Color(1, 1, 1, 0.75f);
                    else
                        _castle.color = Color.white;
                    castleSpriteName = string.Format("ingame_castle_{0}", lifeCount);
                }
                break;
        }

        _castle.spriteName = castleSpriteName;

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
        if (_castle != null) MakePixelPerfect(_castle);
        if (_bgSprite != null) MakePixelPerfect(_bgSprite);
    }

    public override IEnumerator CoFlashBlock_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => mainSprite.color, x => mainSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => _bgSprite.color, x => _bgSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => _bgSprite2.color, x => _bgSprite2.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => _castle.color, x => _castle.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        mainSprite.color = Color.white;
        _bgSprite.color = Color.white;
        _bgSprite2.color = Color.white;
        _castle.color = Color.white;
        yield return null;
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {       
        if (mBombEffect) return;
        if (pangIndex == uniqueIndex) return;
        if (state == BlockState.PANG) return;

        pangIndex = uniqueIndex;

        if (blockDeco != null)
        {
            if (bombEffect) return;

            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }

        state = BlockState.PANG;
        _pangRemoveDelay = 0.5f;
        Pang();        
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        lifeCount--;
        IsSkipPang = true;

        if(groundType != GROUND_TYPE.JEWEL || lifeCount > 0)
        InGameEffectMaker.instance.MakeGroundEffect(_transform.position);


        if (lifeCount <= 0)
        {            
            StartCoroutine(CoPangFinal());
        }
        else
        {
            StartCoroutine(CoPang());
        }
    }


    IEnumerator CoPang()
    {
        if (groundType == GROUND_TYPE.JEWEL && lifeCount > 1) ManagerSound.AudioPlayMany(AudioInGame.Release_Jewelry1);
        else if (groundType == GROUND_TYPE.JEWEL) ManagerSound.AudioPlayMany(AudioInGame.Release_Jewelry2);
        else ManagerSound.AudioPlayMany(AudioInGame.BREAK_SOIL_POT1);

        //이펙트
        //Instantiate(ManagerBlock.instanse._effectPlant, _transform.position - ManagerBlock.instanse._groundMoveTransform.position, Quaternion.identity);
        //InGameEffectMaker.instance.MakeCastleEffect(transform.position);
        UpdateSpriteByBlockType();

       // state = BlockState.WAIT;
      //  IsSkipPang = false;

        yield return null;
        /*
        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        UpdateSpriteByBlockType();
        */
        
        float timer = 0f;
        while (timer < 0.3f)
        {
            //mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        if (targetPos != Vector3.zero && _transform.localPosition != targetPos)
        {
            state = BlockState.MOVE;
        }
        else
        {
            state = BlockState.WAIT;
        }
        IsSkipPang = false;
        yield return null;
        
    }

    IEnumerator CoPangFinal()
    {
        if (groundType == GROUND_TYPE.JEWEL) ManagerSound.AudioPlayMany(AudioInGame.Get_Jewelry);
        else ManagerSound.AudioPlayMany(AudioInGame.BREAK_SOIL_POT1);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;


        if (_bgSprite != null) Destroy(_bgSprite.gameObject);
        //InGameEffectMaker.instance.MakeCastleEffect(transform.position);

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        BlockBase makeBlock = null;
        
        //목표날리기
        if (groundType == GROUND_TYPE.JEWEL)
        {
            ManagerSound.AudioPlayMany(AudioInGame.KEY);
            InGameEffectMaker.instance.MakeGetJewelEffect(_transform.position);

            for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
            {
                if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.JEWEL)
                {
                    InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.JEWEL);
                    break;
                }
            }

            ManagerBlock.instance.AddScore(500);
            InGameEffectMaker.instance.MakeScore(_transform.position, 500);

            ManagerBlock.instance.creatBlockTypeTurnCount[(int)BlockType.GROUND_JEWEL] = GameManager.instance.touchCount;
        }
        else if (groundType == GROUND_TYPE.APPLE)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.APPLE, BlockColorType.NONE, appleCount);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
        }
        else if (groundType == GROUND_TYPE.LINE)
        {
            makeBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
            int randomLine = GameManager.instance.GetIngameRandom(0, 2);
            makeBlock.bombType = randomLine == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;
        }
        else if (groundType == GROUND_TYPE.ICE_APPLE)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ICE_APPLE, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
        }
        else if (groundType == GROUND_TYPE.LINE_V)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
            makeBlock.bombType = BlockBombType.LINE_V;
        }
        else if (groundType == GROUND_TYPE.LINE_H)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
            makeBlock.bombType = BlockBombType.LINE_H;
        }
        else if (groundType == GROUND_TYPE.CIRCLE)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
            makeBlock.bombType = BlockBombType.BOMB;
        }
        else if (groundType == GROUND_TYPE.KEY)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.KEY, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
        }
        else if (groundType == GROUND_TYPE.DUCK)
        {
            BlockBase blockDuck = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.DUCK, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockDuck;
        }
        else if (groundType == GROUND_TYPE.CANDY)
        {
            BlockBase blockCandy = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.CANDY, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockCandy;
        }
        else if (groundType == GROUND_TYPE.BlOCKBLACK)
        {
            makeBlock = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.BLOCK_BLACK, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = makeBlock;
        }
        else
        {
            ManagerBlock.instance.AddScore(80);
            InGameEffectMaker.instance.MakeScore(_transform.position, 80);
        }

        DestroyBlockData();

        if (makeBlock != null)
        {
            //흙에서 생성된 블럭은 출발에서 생성된 블럭과 동일하게 동작("화면수에 미포함" 기능이 동작하지 않도록 처리)
            makeBlock.isMakeByStart = true;
            
            if(makeBlock.type == BlockType.NORMAL)
            {
                BlockType tempType = BlockType.START_Bomb;

                switch (makeBlock.bombType)
                {
                    case BlockBombType.LINE:
                    case BlockBombType.LINE_H:
                    case BlockBombType.LINE_V:
                        tempType = BlockType.START_Line;
                        break;
                }
                ManagerBlock.instance.liveBlockTypeCount[(int)tempType]++;
                ManagerBlock.instance.totalCreatBlockTypeCount[(int)tempType]++;

                //출발에서 생성된 폭탄과 동일하게 초기화해줌
                NormalBlock temp = makeBlock as NormalBlock;
                temp.InitBlock();
            }
        }

        if (groundType != GROUND_TYPE.APPLE
            && groundType != GROUND_TYPE.LINE
            && groundType != GROUND_TYPE.LINE_V
            && groundType != GROUND_TYPE.LINE_H
            && groundType != GROUND_TYPE.CIRCLE
            && groundType != GROUND_TYPE.KEY
            && groundType != GROUND_TYPE.ICE_APPLE
            && groundType != GROUND_TYPE.DUCK
            && groundType != GROUND_TYPE.CANDY
            && groundType != GROUND_TYPE.BlOCKBLACK
            )
            ManagerBlock.boards[indexX, indexY].Block = null;

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        yield return CoPangFinal();
    }

    // Move 상태이고 맵 내에 클로버가 있다면 항상 보여주도록 처리
    public void SetMoveState()
    {
        if (ManagerBlock.instance.listClover.Count <= 0)
            return;
        if (mainSprite.enabled)
            return;
        
        SetCloverHide(true);
    }
    
    // Wait 상태 진입 시 클로버에 가려져 있다면 기믹 이미지 비활성화
    public override void SetStateWait()
    {
        if (ManagerBlock.instance.listClover.Count <= 0)
            return;
        if (!mainSprite.enabled)
            return;
        
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);

        if (board.HasDecoHideBlock())
            SetCloverHide(false);
    }
    
    float shineTimer = -1f;
    public override void UpdateBlock()
    {
        MoveBlock();

        if (state == BlockState.MOVE)
            SetMoveState();

        if (state == BlockState.WAIT && groundType == GROUND_TYPE.JEWEL && lifeCount == 1 && isCanShining == true && PosHelper.InExistScreen(indexX, indexY))
        { 

            if(shineTimer == -1f)
            {
                shineTimer = (float)(Random.Range(4, 8))*0.5f;
            }
            else if (0 >= shineTimer)
            {
                InGameEffectMaker.instance.MakeJewelShineEffect(_transform);
                shineTimer = (float)(Random.Range(5, 10))*0.5f;
            }
            else
            {
                shineTimer -= Global.deltaTimePuzzle;
            }
        }
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (groundType == GROUND_TYPE.JEWEL) // 보석일 경우 우선순위
        {
            if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.JEWEL))
                return FireWorkRank.RANK_1;
            else
                return FireWorkRank.RANK_NONE;
        }
        else //그 외의 모든 흙의 우선순위
        {
            return FireWorkRank.RANK_3;
        }
    }

    public override bool IsBlockCheckKeyTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        if (groundType == GROUND_TYPE.JEWEL && lifeCount <= 1) //보석일 경우
            return false;
        else
            return true;
    }

    public override bool EventAction()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        
        bool isRandomBox = false;
        foreach (var item in board.BoardOnNet)
        {
            if (item is RandomBox)
            {
                isRandomBox = true;
                break;
            }
        }

        // 낙엽 or 랜덤박스 : 이펙트 및 오브젝트 비활성화
        if (board.HasDecoHideBlock() || isRandomBox)
            SetCloverHide(false);
        // 랜덤박스를 제외한 INet 기믹 (물, 잡기돌) : 이펙트 비활성화
        else if (board.HasDecoCoverBlock())
            SetEffectHide(false);
        // 이외 (아무 기믹에도 영향받지 않는 상태) : 이펙트 및 오브젝트 활성화
        else
            SetCloverHide(true);
        
        return false;
    }
}
