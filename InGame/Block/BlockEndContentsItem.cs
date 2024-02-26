using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockEndContentsItem : BlockBase
{
    [SerializeField]
    private GameObject itemRoot;
    [SerializeField]
    private UISprite itemImage;
    [SerializeField]
    private UILabel itemCountText;
    [SerializeField]
    private TweenPosition itemImageTween;

    private int itemCount = 1;

    private void Start()
    {
        if (ManagerEndContentsEvent.instance != null)
        {
            NGUIAtlas EndContentsAtlas = ManagerEndContentsEvent.instance.endContentsPack_Ingame.IngameAtlas;
            mainSprite.atlas = EndContentsAtlas;
            itemImage.atlas = EndContentsAtlas;
        }
    }

    public override bool IsCanPang()
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

    public override bool IsDestroyBlockAtStageClear()
    {
        return true;
    }

    public IEnumerator CoStartEffect()
    {
        itemImageTween.enabled = false;
        itemImage.spriteName = "endContents1";

        int itemImageDepth = itemImage.depth;
        int itemCountTextDepth = itemCountText.depth;

        itemImage.depth = (int) GimmickDepth.FX_EFFECTBASE;
        itemCountText.depth = (int) GimmickDepth.FX_EFFECTBASE + 1;

        float originY = itemImage.transform.position.y;

        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);

        itemImage.transform.DOMoveY(originY + 0.04f, actionTime).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(actionTime);

        itemImage.transform.DOMoveY(originY, actionTime).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.1f);
        itemImage.transform.DOScale(0.8f, actionTime).SetEase(Ease.InOutElastic);
        yield return new WaitForSeconds(actionTime);
        
        itemImage.spriteName = "endContents2";
        itemImageTween.enabled = true;
        itemImage.transform.DOScale(1f, actionTime);

        itemImage.depth = itemImageDepth;
        itemCountText.depth = itemCountTextDepth;

    }


    public override void UpdateBlock()
    {
        if (state == BlockState.WAIT)
            _waitCount++;
    }

    public override void UpdateSpriteByBlockType()
    {
        SetMainSpriteDepth();
        mainSprite.MakePixelPerfect();
        itemImage.MakePixelPerfect();
        itemImage.depth = mainSprite.depth + 1;
        itemCountText.depth = mainSprite.depth + 2;
        itemCountText.text = itemCount.ToString();

        //기믹 획득 직전 마지막 카운트 표시
        if (lifeCount <= 1)
        {
            itemCountText.color = new Color(1f, 80f / 255f, 80f / 255f);
            itemCountText.effectColor = new Color(100f / 255f, 0f, 0f);
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount < 1)
            return;
        lifeCount--;

        //연출
        CollectAction();
    }

    private void CollectAction()
    {
        //사운드 출력
        ManagerSound.AudioPlayMany(AudioInGame.WORLDRANKITEM_PANG);

        itemRoot.SetActive(false);
        itemRoot.transform.localScale = new Vector3(1f, 0f, 1f);

        //아이템 획득 이펙트
        StartCoroutine(CoMakeEffect());

        if (lifeCount < 1 && state != BlockState.PANG)
        {
            StartCoroutine(CoPangFinal());
        }
        else
        {
            itemCount++;
            UpdateSpriteByBlockType();
            StartCoroutine(CoPang());
        }
    }

    private IEnumerator CoMakeEffect()
    {
        int itemCnt = itemCount;
        int effectCnt = (itemCnt > 5) ? 5 : itemCnt;

        int count = (itemCnt > 5) ? (itemCnt / 5) : 1;
        for (int i = 0; i < effectCnt; i++)
        {
            int addCount = count;

            //맨 마지막 이펙트는 나머지까지 전부 합산한 카운트.
            if (itemCnt > 5 && (i + 1) >= effectCnt)
                addCount += itemCnt % 5;

            InGameEffectMaker.instance.MakeFlyEndContentsItem
                (transform.position, GameUIManager.instance.collectItemSprite.transform.position, () => { GetCollectItem(addCount); });
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void GetCollectItem(int itemCnt)
    {
        GameUIManager.instance.CollectAction_EndContentsItem(itemCnt);
    }

    private IEnumerator CoPang()
    {
        yield return new WaitForSeconds(0.5f);
        itemRoot.SetActive(true);
        itemRoot.transform.localScale = Vector3.one;

        float timer = 0f;
        while (timer < 1f)
        {
            itemRoot.transform.localScale = new Vector3(itemRoot.transform.localScale.x, ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer), itemRoot.transform.localScale.z);
            timer += Global.deltaTimePuzzle * 3f;
            yield return null;
        }
    }

    private IEnumerator CoPangFinal()
    {
        //이펙트 생성
        InGameEffectMaker.instance.MakeEffectWorldRankItemPang(_transform.position);

        //이펙트 생성 끝날 때 까지 대기
        int effectCnt = (itemCount > 5) ? 5 : itemCount;
        float waitTime = effectCnt > 0 ? (effectCnt - 1) * 0.15f : 0f;
        yield return new WaitForSeconds(waitTime);

        mainSprite.transform.DOScale(0f, 0.3f).SetEase(Ease.InOutBack);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0.5f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();
        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null)
            back.SetUnderBoard();
        PangDestroyBoardData();
        if (back != null)
            back.CheckStatus();
    }

    public void InitBlock(int life)
    {
        lifeCount = life;
        ManagerBlock.instance.AddEndContents(this);
    }
    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        return true;
    }
}
