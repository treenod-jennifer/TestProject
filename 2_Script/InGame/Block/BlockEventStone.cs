using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEventStone : BlockBase
{
    public UIBlockUrlTexture eventBlockTexture;
    public UISprite plantPlace_Sprite;
    public int eventBlockIndex = 0;

    void Start()
    {
        Board back = PosHelper.GetBoard(indexX, indexY);
        if (back.IsActiveBoard && lifeCount > 0)
        {
            plantPlace_Sprite.spriteName = "plantPlace";
            plantPlace_Sprite.depth = (int)GimmickDepth.DECO_GROUND;
            MakePixelPerfect(plantPlace_Sprite);
            _isCanMove = false;
        }
        else
        {
            if (plantPlace_Sprite != null)
                Destroy(plantPlace_Sprite.gameObject);
        }
    }

    public override void UpdateSpriteByBlockType()
    {
        int collectEventType = ManagerBlock.instance.stageInfo.collectEventType;
        if (GameManager.instance.state == GameState.EDIT)
        {
            collectEventType = 101; //더미 이미지
        }

        string blockName = "stone" + lifeCount + "_mt_" + collectEventType;

        if (lifeCount <= 0)
            blockName = "block_mt_" + collectEventType;

        eventBlockTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", blockName);
        if (lifeCount >= 3)
        {
            eventBlockTexture.cachedTransform.localPosition = new Vector3(0, 14, 0);
        }
        else
        {
            eventBlockTexture.cachedTransform.localPosition = Vector3.zero;
        }
        SetMainSpriteDepth();
        eventBlockTexture.MakePixelPerfect();
    }

    public override void SetMainSpriteDepth()
    {
        if (lifeCount >= 3)
        {
            eventBlockTexture.depth = (int)GimmickDepth.BLOCK_BASE + indexY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT;
        }
        else
        {
            eventBlockTexture.depth = (int)GimmickDepth.BLOCK_BASE;
        }
    }

    public override bool IsCanPang() { return true; }
    public override bool IsBlockType()
    {
        if (lifeCount <= 0) return true;
        return false;
    }

    bool _isCanMove = false;
    public override bool IsCanMove()
    {
        return _isCanMove;
    }

    public override void UpdateBlock()
    {
        if (IsCanMove()) MoveBlock();
        else _waitCount++;
    }

    public override bool IsPangExtendable()
    {
        if (lifeCount == 0)return true;
        return false;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        if (lifeCount <= 0)
            return false;
        return true;
    }

    public override bool IsCoverCarpet()
    {
        if (lifeCount <= 0)
            return false;
        return true;
    }

    public override bool IsThisBlockHasPlace()
    {
        return true;
    }

    public override bool IsDestroyBlockAtStageClear()
    {
        return true;
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (lifeCount == 2) return;
        if (mBombEffect) return;
        if (pangIndex == uniqueIndex) return;
        if (state == BlockState.PANG) return;

        pangIndex = uniqueIndex;
        Pang(splashColorType, true);
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (!PangByBomb)
        {
            state = BlockState.WAIT;
            return;
        }

        if (lifeCount < 0)
        {
            return;
        }

        lifeCount--;

        if (lifeCount < 0)
        {
            IsSkipPang = true;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);

            ManagerBlock.instance.materialCollectPos |= (1 << eventBlockIndex);
        }
        else
        {
            StartCoroutine(CoPang());
            if (lifeCount > 1) ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
    }

    IEnumerator CoPang()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            eventBlockTexture.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        if (lifeCount <= 0)
        {
            if (plantPlace_Sprite != null)
                Destroy(plantPlace_Sprite.gameObject);

            Board back = PosHelper.GetBoardSreeen(indexX, indexY);
            if (back != null && back.BoardOnGrass.Count > 0)
            {
                foreach (var tempGrass in back.BoardOnGrass)
                {
                    Grass grassA = tempGrass as Grass;
                    grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                    grassA.BGSprite.depth = grassA.uiSprite.depth + 1;
                }
            }

            //아래에 석상있는지 체크후 석상작동
            if (back != null)
                back.CheckStatus();
        }

        UpdateSpriteByBlockType();

        timer = 0f;
        while (timer < 1f)
        {
            eventBlockTexture.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        //여기서 부터 이동가능
        if (lifeCount <= 0)
            _isCanMove = true;

        eventBlockTexture.MakePixelPerfect();
        eventBlockTexture.transform.localScale = Vector3.one;
    }

    IEnumerator CoPangFinal()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(_transform.position, 80);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;

        while (timer < 1f)
        {
            eventBlockTexture.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            eventBlockTexture.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        if (plantPlace_Sprite != null)
            Destroy(plantPlace_Sprite.gameObject);

        IsSkipPang = false;

        ManagerBlock.boards[indexX, indexY].Block = null;
        ManagerBlock.boards[indexX, indexY].TempBlock = null;


        if (back != null && back.BoardOnGrass.Count > 0)
        {
            foreach (var tempGrass in back.BoardOnGrass)
            {
                Grass grassA = tempGrass as Grass;
                grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                grassA.BGSprite.depth = grassA.uiSprite.depth + 1;
            }
        }
        //획득한 메터리얼 게임매니져에 전송
        //이미지날리기
        InGameEffectMaker.instance.MakeUrlEffect(transform.position, GameUIManager.instance.specialEventObj.transform.position, "plant0_mt_" + ManagerBlock.instance.stageInfo.collectEventType, endEffectMove);


        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

    void endEffectMove()
    {
        ManagerBlock.instance.materialCollectEvent++;
        GameUIManager.instance.RefreshMaterial();
    }

    public override void SpriteRatio(float ratio, float vertRatio = 0)
    {
        if (eventBlockTexture != null)
        {
            eventBlockTexture.customFill.verticalRatio = vertRatio;
            eventBlockTexture.customFill.blockRatio = ratio;
        }
    }

    public override void MakeDummyImage(Vector3 tartgetPos)
    {
        int tempCount = lifeCount;
        if (lifeCount < 0)
            tempCount = 0;

        UIBlockUrlTexture dummyMainSprite = NGUITools.AddChild(gameObject, eventBlockTexture.gameObject).GetComponent<UIBlockUrlTexture>();
        string blockName = "plant" + tempCount + "_mt_" + ManagerBlock.instance.stageInfo.collectEventType;
        dummyMainSprite.SettingTextureScale(78, 78);
        dummyMainSprite.LoadCDN(Global.gameImageDirectory, "IconEvent/", blockName);

        dummyMainSprite.depth = mainSprite.depth;
        dummyMainSprite.MakePixelPerfect();
        dummyMainSprite.cachedTransform.localPosition = tartgetPos;

        dummySpriteList.Add(dummyMainSprite.customFill);
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (lifeCount > 0)
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_4;
    }
}
