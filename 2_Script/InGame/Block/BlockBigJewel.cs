using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBigJewel : BlockBase
{
    PLANT_TYPE plantType = PLANT_TYPE.NORMAL;
    const int MAX_LIFE = 6;
    public int index;

    public List<Board> blockBoardList = new List<Board>();
    public UISprite plantPlace_Sprite;
    //public List<UISprite> listSprite = new List<UISprite>();

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
        SetMainSpriteDepth();
       /* foreach (var tempBoard in listSprite)
        {
            tempBoard.depth = mainSprite.depth;
            tempBoard.MakePixelPerfect();
        }*/

        mainSprite.spriteName = "StoneJewel" + lifeCount;
        mainSprite.MakePixelPerfect();
    }

    public void initBlock(int tempType)
    {
        plantType = (PLANT_TYPE)tempType;

        Vector3 centerPos = new Vector3();
        foreach (var tempBoard in blockBoardList)
        {
            centerPos += PosHelper.GetPosByIndex(tempBoard.indexX, tempBoard.indexY);
        }
        centerPos = centerPos * 0.25f;
        _transform.localPosition = centerPos;

        plantPlace_Sprite.cachedTransform.localScale = Vector3.one;// *2f;

        UpdateSpriteByBlockType();

        /*
        if (lifeCount < MAX_LIFE)
        {
            for (int i = lifeCount; i < MAX_LIFE; i++)
            {
                listSprite[i].enabled = false;
            }
        }        
        */
    }
    protected int checkWaitFrame = 0;
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
        }
        else
        {
            Destroy(plantPlace_Sprite.gameObject);
        }
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        bool hasColor = false;
        //UISprite tempPangSprite = listSprite[lifeCount - 1];

        lifeCount--;

        if (lifeCount <= 0 && state != BlockState.PANG)
        {
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
            FlyTarget.flyTargetCount++;
            InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.FLOWER_POT);
        }
        else
        {
            if (playCoPang) return;

            playCoPang = true;
            StartCoroutine(CoPang());
            if (lifeCount > 1) ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
    }

    bool playCoPang = false;

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;
        
        if (pangIndex == uniqueIndex) return;
        
        if (lifeCount <= 0 || state == BlockState.PANG)
            return;

        pangIndex = uniqueIndex;
        Pang(splashColorType);
    }

    IEnumerator CoPang() //
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            //listSprite[lifeCount].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            //transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            //foreach (var tempBoard in listSprite)
            //    tempBoard.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }


        /*
            for (int i = lifeCount - 1; i < MAX_LIFE; i++)
            {
                listSprite[i].enabled = false;
            }
        */

        UpdateSpriteByBlockType();
        //tempSprite.enabled = false;
        playCoPang = false;

        timer = 0f;
        while (timer < 1f)
        {
            //transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
          //  foreach (var tempBoard in listSprite)
          //      tempBoard.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);

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

        if (plantType == PLANT_TYPE.WATER)
        {
            foreach (var tempBoard in blockBoardList)
            {
                DecoInfo decoWater = new DecoInfo();
                decoWater.BoardType = (int)BoardDecoType.WATER;
                DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(tempBoard, tempBoard.indexX, tempBoard.indexY, decoWater);
            }

            ManagerBlock.instance.GetWater = true;
            ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);
        }
        else
        {
            ManagerBlock.boards[indexX, indexY].Block = null;
            ManagerBlock.boards[indexX, indexY].TempBlock = null;
        }

        if (back != null && back.BoardOnGrass.Count > 0)
        {
            foreach (var tempGrass in back.BoardOnGrass)
            {
                Grass grassA = tempGrass as Grass;
                grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                grassA.BGSprite.depth = grassA.uiSprite.depth + 1;
            }
        }

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.FLOWER_POT))
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }
}
