using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BoardDecoType
{
    NONE,
    NET,
    CARCK1,
    KEY_EXIT,
    START,
    POTAL_IN,
    POTAL_OUT,
    LINK_SIDE_BLOCK,
    SIDE_BLOCK,
    GRASS,
    STATUE,
    ARROW,
    MAP_DECO,
    ICE,    //블럭데코
    SAND_BELT,
    LAVA,
    WATER,
    FENCEBLOCK,
    GRASSFENCEBLOCK,
    CARPET,
    COUNT_CRACK,
    FLOWER_INK,
    SPACESHIP_EXIT,
    BLOCK_GENERATOR,
    RANDOM_BOX,
    CLOVER,
}

public enum BoardDecoOrder
{
    CLOVER,
    RANDOM_BOX,
    END,
}

public class DecoBase : MonoBehaviour {

    public int inX;
    public int inY;

    public static List<DecoBase> _listBoardDeco;
    public static List<DecoBase> _listBoardDecoTemp;
    public static int _waitCount = 0;

    public UIBlockSprite uiSprite;
    public Board board;

    public int pangIndex = -1;

    [System.NonSerialized]
    public BoardDecoOrder boardDecoOrder = BoardDecoOrder.END;

    int mLifeCount = 1;
    public int lifeCount
    {
        get { return mLifeCount; }
        set
        {
            mLifeCount = value;
            SetSprite();
        }
    }

    void Start()
    {
        //Init();
    }


    public virtual void SetSprite()
    {

    }

    public virtual void SetSpriteEnabled(bool setEnabled)
    {
        uiSprite.enabled = setEnabled;
    }

    /*
    public virtual bool SetPang(int uniquePang = 0 , BlockColorType colorType = BlockColorType.NONE)
    {
        return true;
    }
    */

    public void DestroySelf()
    {
        Destroy(gameObject);
        Destroy(this);
    }

    public virtual bool IsInterruptBlockEvent()
    {   //블럭 이벤트를 막는지 검사.
        return false;
    }

    public virtual bool IsInterruptBlockColorChange()
    {   //블럭의 컬러 변경을 막는지 검사.
        return false;
    }

    public virtual bool IsCanPangBlockBoardIndex(int indexX, int indexY, BlockDirection bombDirection)
    {   //현재 보드 인덱스 읽어와서 블럭이 터질 수 있는지 검사.
        return true;
    }

    public virtual bool SetDirectionPang(Board checkBoard = null, int bombX = 0, int bombY = 0, int uniquePang = 0, BlockDirection bombDirection = BlockDirection.NONE, List<BombAreaInfo> infoList = null, bool bNoneUseDirection = false)
    {   //폭탄 방향 검사해서 현재 데코가 터질수 있는지 검사 후 제거됨
        return false;
    }

    public virtual bool IsDisturbDeco_ByBomb(Board checkBoard = null, int bombX = 0, int bombY = 0, int uniquePang = 0, BlockDirection bombDirection = BlockDirection.NONE, List<BombAreaInfo> infoList = null, bool bNoneUseDirection = false)
    {   //폭탄 방향 검사해서 현재 테코가 폭탄 효과를 막는지 검사 //터짐X
        return false;
    }

    public virtual bool IsCanPang_ByHeart()
    {
        //하트 기믹이 지나가면 사라지는 데코인지 검사 //제거되는 것이 기본.
        return true;
    }

    public virtual bool SetSplashPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {
        return false;
    }

    public virtual bool IsCanHasBlock()
    {
        return false;
    }

    public virtual bool IsCanFlow()
    {
        return true;
    }

    public virtual bool IsWarpBlock()
    {
        return false;
    }

    public virtual bool IsMakerBlock()
    {
        return false;
    }
    public virtual bool IsDisturbPangByRainbowBomb()
    {
        return true;
    }

    public virtual bool IsCanFill()
    {
        if (IsWarpBlock()) return false;
        if (!IsCanFlow()) return false;

        return true;
    }
    
    public virtual bool IsPangDirectionExtendable(BlockDirection bombDirection, int indexX, int indexY)
    {  //폭탄 방향으로 블럭들 팡이 퍼질 수 있는 지 검사.
        return true;
    }

    public virtual bool IsCoverStatue() //석상위에 깔리는 데코인지.
    {
        return false;
    }

    public virtual bool IsCoverCarpet() //카펫위에 덮이는 블럭인지(카펫을 감추는 블럭인지)
    {
        return false;
    }

    public virtual void Init()
    {
    }

    public virtual void UpdateDeco()
    {

    }

    public virtual bool EventAction()
    {
        return false;
    }

    public virtual void MakeBlockAction() { }

    public virtual bool GetTargetBoard(BlockType blockType)
    {
        return false;
    }

    public virtual void RemoveDeco()
    {
        board.RemoveDeco(this);
        _listBoardDeco.Remove(this);
        ManagerBlock.instance.listObject.Remove(gameObject);
        Destroy(gameObject);
        Destroy(this);
    }

    public IEnumerator CoPang()
    {
        //이펙트
        //Instantiate(ManagerBlock.instanse._effectPlant, _transform.position - ManagerBlock.instanse._groundMoveTransform.position, Quaternion.identity);

        float timer = 0f;
        while (timer < 1f)
        {
            uiSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        SetSprite();

        timer = 0f;
        while (timer < 1f)
        {
            uiSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
    }

    public virtual IEnumerator CoPangFinal()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            uiSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            uiSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
        RemoveDeco();
        yield return null;
    }

    public virtual void MakeFieldEffect()
    {
        return;
    }

    //땅파기 모드에서 목표로 사용되는 데코인지.
    public virtual bool IsTarget_DigMode()
    {
        return false;
    }

    //용암 모드에서 목표로 사용되는 데코인지.
    public virtual bool IsTarget_LavaMode()
    {
        return false;
    }

    public virtual IEnumerator CoFlashDeco_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => uiSprite.color, x => uiSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        uiSprite.color = Color.white;
        yield return null;
    }

    public virtual IEnumerator CoFlashDeco_Alpha(int actionCount, float actionTIme, float waitTime)
    {
        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.ToAlpha(() => uiSprite.color, x => uiSprite.color = x, 0, aTime).SetLoops(2, LoopType.Yoyo);
            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        uiSprite.color = new Color(uiSprite.color.r, uiSprite.color.g, uiSprite.color.b, 1f);
        yield return null;
    }

    protected void MakePixelPerfect(UISprite sprite, float offset = 1.25f)
    {
        sprite.MakePixelPerfect();
        sprite.width = Mathf.RoundToInt(sprite.width * offset);
        sprite.height = Mathf.RoundToInt(sprite.height * offset);
    }

    public virtual void SetMainSpriteDepth()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_BASE;
    }
}
