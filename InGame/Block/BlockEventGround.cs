using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockEventGround : BlockBase
{
    public UIBlockSprite _bgSprite;
    public UIBlockSprite _bgSprite2;

    public UIBlockUrlTexture eventBlockTexture;
    public int eventBlockIndex = 0;

    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
        eventBlockTexture.enabled = setEnabled;
        if (_bgSprite != null) _bgSprite.enabled = setEnabled;
        if (_bgSprite2 != null) _bgSprite2.enabled = setEnabled;
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

    public override bool IsCanCoverIce()
    {
        return false;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override void SetMainSpriteDepth()
    {
        mainSprite.depth = (int)GimmickDepth.BLOCK_GROUND + 1;
        if (lifeCount < 1)
            eventBlockTexture.depth = (int)GimmickDepth.BLOCK_BASE;
        else
            eventBlockTexture.depth = (int)GimmickDepth.BLOCK_GROUND + 2;
        if (_bgSprite != null) _bgSprite.depth = (int)GimmickDepth.BLOCK_GROUND;
    }

    public override void MakeDummyImage(Vector3 tartgetPos)
    {
        base.MakeDummyImage(tartgetPos);

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
        int collectEventType = ManagerBlock.instance.stageInfo.collectEventType;
        if (GameManager.instance.state == GameState.EDIT)
        {
            collectEventType = 101; //더미 이미지
        }

        string blockName = "ground" + lifeCount + "_mt_" + collectEventType;

        if (lifeCount <= 0)
        {
            blockName = "block_mt_" + collectEventType;
            mainSprite.enabled = false;
            _bgSprite.enabled = false;
            _bgSprite2.enabled = false;
        }
        eventBlockTexture.Load(Global.gameImageDirectory, "IconEvent/", blockName);
        eventBlockTexture.MakePixelPerfect();

        MakePixelPerfect(mainSprite);
        if (_bgSprite != null) MakePixelPerfect(_bgSprite);
        if (_bgSprite2 != null) MakePixelPerfect(_bgSprite2);
        SetMainSpriteDepth();
        
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
            DOTween.To(() => eventBlockTexture.color, x => eventBlockTexture.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }
        mainSprite.color = Color.white;
        _bgSprite.color = Color.white;
        _bgSprite2.color = Color.white;
        eventBlockTexture.color = Color.white;
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
        InGameEffectMaker.instance.MakeGroundEffect(_transform.position);

        if (lifeCount < 0)
        {
            StartCoroutine(CoPangFinal());
            ManagerBlock.instance.materialCollectPos |= (1 << eventBlockIndex);
        }
        else
        {
            StartCoroutine(CoPang());
        }
    }

    IEnumerator CoPang()
    {
        ManagerSound.AudioPlayMany(AudioInGame.BREAK_SOIL_POT1);
        UpdateSpriteByBlockType();
        yield return null;

        float timer = 0f;
        while (timer < 0.3f)
        {
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
        ManagerSound.AudioPlayMany(AudioInGame.BREAK_SOIL_POT1);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;

        if (_bgSprite != null) Destroy(_bgSprite.gameObject);

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        //획득한 메터리얼 게임매니져에 전송
        InGameEffectMaker.instance.MakeUrlEffect(transform.position, GameUIManager.instance.specialEventObj.transform.position, "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType, endEffectMove);

        DestroyBlockData();
        PangDestroyBoardData();
    }

    void endEffectMove()
    {
        ManagerBlock.instance.materialCollectEvent++;
        GameUIManager.instance.RefreshMaterial();
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (lifeCount > 0)
            return FireWorkRank.RANK_3;
        else
            return FireWorkRank.RANK_4;
    }

    public override bool IsBlockCheckKeyTypeAtFireWork()
    {
        return true;
    }
}
