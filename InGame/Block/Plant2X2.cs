using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2X2 : BlockBase
{
    public int index;

    public List<Board> blockBoardList = new List<Board>();
    public UISprite plantPlace_Sprite;

    public List<UISprite> listSprite = new List<UISprite>();

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsBlockType()
    {
        return false;
    }

    public override void UpdateSpriteByBlockType()
    {
        if (lifeCount <= 0) return;

        mainSprite.spriteName = "plant2";// + lifeCount;
        SetMainSpriteDepth();
        foreach (var tempBoard in listSprite)
        {
            tempBoard.depth = mainSprite.depth;
            tempBoard.MakePixelPerfect();
        }
    }

    public void initBlock()
    {
        Vector3 centerPos = new Vector3();
        foreach (var tempBoard in blockBoardList)
        {
            centerPos += PosHelper.GetPosByIndex(tempBoard.indexX, tempBoard.indexY);
        }
        centerPos = centerPos * 0.25f;
        _transform.localPosition = centerPos;

        plantPlace_Sprite.cachedTransform.localScale = Vector3.one * 2f;

        UpdateSpriteByBlockType();
    }
    int checkWaitFrame = 0;
    public override void UpdateBlock()
    {
        if (checkWaitFrame != Time.frameCount)
        {
            checkWaitFrame = Time.frameCount;
            _waitCount++;
        }
    }

    void Start()
    {
        Board back = PosHelper.GetBoard(indexX, indexY);
        if (back.IsActiveBoard)
        {
            plantPlace_Sprite.spriteName = "plantPlace";
            plantPlace_Sprite.depth = (int)GimmickDepth.DECO_GROUND;
            //plantPlace_Sprite.MakePixelPerfect();
        }
        else
        {
            Destroy(plantPlace_Sprite.gameObject);
        }
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        lifeCount--;

        if (lifeCount <= 0 && state != BlockState.PANG)
        {
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
        }
        else
        {
            StartCoroutine(CoPang());
            if (lifeCount > 1) ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang();
    }

    IEnumerator CoPang()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            listSprite[lifeCount].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            //transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            foreach (var tempBoard in listSprite)
                tempBoard.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        UpdateSpriteByBlockType();
        listSprite[lifeCount].enabled = false;

        timer = 0f;
        while (timer < 1f)
        {
            //transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            foreach (var tempBoard in listSprite)
                tempBoard.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);

            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
    }

    IEnumerator CoPangFinal()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        if (plantPlace_Sprite != null)
            Destroy(plantPlace_Sprite.gameObject);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;

        while (timer < 1f)
        {
            transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
        ManagerBlock.boards[indexX, indexY].Block = null;
        ManagerBlock.boards[indexX, indexY].TempBlock = null;

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }
}
