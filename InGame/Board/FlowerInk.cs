using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlowerInk : DecoBase
{
    [SerializeField]
    private UISprite pangEffect;

    public BlockColorType colorType = BlockColorType.NONE;

    private const string spriteName = "FlowerInk_";

    public override void SetSprite()
    {
        switch (colorType)
        {
            case BlockColorType.A:
                uiSprite.spriteName = string.Format("{0}A", spriteName);
                break;
            case BlockColorType.B:
                uiSprite.spriteName = string.Format("{0}B", spriteName);
                break;
            case BlockColorType.C:
                uiSprite.spriteName = string.Format("{0}C", spriteName);
                break;
            case BlockColorType.D:
                uiSprite.spriteName = string.Format("{0}D", spriteName);
                break;
            case BlockColorType.E:
                uiSprite.spriteName = string.Format("{0}E", spriteName);
                break;
        }
        uiSprite.depth = (int)GimmickDepth.DECO_AREA;
        MakePixelPerfect(uiSprite);
    }

    public override void RemoveDeco()
    {
        board.RemoveDeco(this);
        _listBoardDeco.Remove(this);
        ManagerBlock.instance.listObject.Remove(gameObject);
        ManagerBlock.instance.listFlowerInk.Remove(this);
        StartCoroutine(CoRemoveAction());
    }

    private IEnumerator CoRemoveAction()
    {
        uiSprite.transform.DOScale(1.1f, 0.2f);
        yield return new WaitForSeconds(0.3f);
        uiSprite.transform.DOScale(0f, 1.5f).SetEase(Ease.OutQuad);
        DOTween.ToAlpha(() => uiSprite.color, x => uiSprite.color = x, 0f, 1.2f);
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        Destroy(this);
    }

    public bool IsCanAutoMatch()
    {
        //스크린 범위를 넘어간 데코는 자동매치 동작하지 않음.
        if (inX < GameManager.MinScreenX || inX > GameManager.MaxScreenX || inY < GameManager.MinScreenY || inY > (GameManager.MaxScreenY - 1))
            return false;

        //그 외의 자동매치 불가능한 조건 검사.
        if (board.BoardOnNet.Count > 0 || board.Block == null || board.Block.type != BlockType.NORMAL || board.BoardOnHide.Count > 0
            || board.Block.IsNormalBlock() == false || board.Block.colorType != this.colorType || board.Block.blockDeco != null)
            return false;

        return true;
    }

    public void PangMatchBlock()
    {
        if (IsCanAutoMatch() == false)
            return;

        //스코어
        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(transform.position, 80);

        //이펙트
        InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.FLOWER_INK, board.Block.colorType);

        //획득 될 블럭에 모으기 아이템 있었다면 획득시켜줌
        board.Block.GetCollectEventBlock();

        //블럭 지우기
        board.Block.PangDestroyBoardData();

        //데코 연출
        StartCoroutine(CoPangAction());
    }

    private IEnumerator CoPangAction()
    {
        transform.DOPunchScale(Vector3.one * -0.3f, 0.3f, 2);
        pangEffect.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        pangEffect.gameObject.SetActive(false);

        //사운드 출력
        ManagerSound.AudioPlayMany(AudioInGame.FLOWERINK_PANG);
    }

    public override bool IsTarget_DigMode()
    {   
        return true;
    }

    public override bool IsTarget_LavaMode()
    {
        return true;
    }
}
