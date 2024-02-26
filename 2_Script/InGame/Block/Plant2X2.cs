using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2X2 : BlockBase
{
    enum PLANT_DESTROY_TYPE
    {
        NORMAL,
        COLOR,
    }

    PLANT_TYPE plantType = PLANT_TYPE.NORMAL;
    const int MAX_LIFE = 7;
    public int index;
    PLANT_DESTROY_TYPE destroyType = PLANT_DESTROY_TYPE.NORMAL;

    [SerializeField]
    int[] colorCount = new int[]{ 0, 0, 0, 0, 0 };

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

        //mainSprite.spriteName = "plant2";// + lifeCount;
        SetMainSpriteDepth();
        foreach (var tempBoard in listSprite)
        {
            tempBoard.depth = mainSprite.depth;
            tempBoard.MakePixelPerfect();
        }
    }

    public void initBlock(int tempType, int[] tempSubTarget)
    {
        //colorCount = new int[] { 0, 0, 0, 0, 0 };

        plantType = (PLANT_TYPE)tempType;

        for (int i = 0; i < 5; i++)
        colorCount[i] = tempSubTarget[i];

       
        foreach(var temp in tempSubTarget)
        {
            if (temp > 0) 
            {
                destroyType = PLANT_DESTROY_TYPE.COLOR;
                break;
            }
        }

        Vector3 centerPos = new Vector3();
        foreach (var tempBoard in blockBoardList)
        {
            centerPos += PosHelper.GetPosByIndex(tempBoard.indexX, tempBoard.indexY);
        }
        centerPos = centerPos * 0.25f;
        _transform.localPosition = centerPos;

        plantPlace_Sprite.cachedTransform.localScale = Vector3.one * 2f;

        UpdateSpriteByBlockType();

        if(destroyType == PLANT_DESTROY_TYPE.COLOR)
        {
            foreach (var temp in listSprite)
                temp.enabled = false;

            int tempCount = 0;
            for (int j = 0; j < colorCount.Length; j++)
            {
                for (int i = 0; i < colorCount[j]; i++)
                {
                    listSprite[tempCount].enabled = true;
                    listSprite[tempCount].spriteName = "plant2_" + ManagerBlock.instance.GetColorTypeString((BlockColorType)(j + 1));
                    tempCount++;

                    if (tempCount >= listSprite.Count)
                        break;
                }

                if (tempCount >= listSprite.Count)
                    break;
            }

            lifeCount = tempCount;
        }
        else
        {
            if (lifeCount < MAX_LIFE)
            {
                for (int i = lifeCount; i < MAX_LIFE; i++)
                {
                    listSprite[i].enabled = false;
                }
            }
        }

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
        bool hasColor = false;
        UISprite tempPangSprite = listSprite[lifeCount-1];

        //폭탄폭발일때는?
        if (destroyType == PLANT_DESTROY_TYPE.COLOR)
        {
            if (pangColorType == BlockColorType.NONE)
            {
                for (int j = 0; j < colorCount.Length; j++)
                {
                    if (colorCount[j] > 0)
                    {
                        colorCount[j]--;
                        foreach (var temp in listSprite)
                        {
                            if (temp.enabled && temp.spriteName == "plant2_" + ManagerBlock.instance.GetColorTypeString((BlockColorType)(j + 1)))
                            {
                                tempPangSprite = temp;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            else
            {             

                string spriteName = "plant2_" + ManagerBlock.instance.GetColorTypeString(pangColorType);
                colorCount[(int)pangColorType - 1]--;

                foreach (var temp in listSprite)
                {
                    if (temp.enabled && temp.spriteName == spriteName)
                    {
                        tempPangSprite = temp;
                        hasColor = true;
                        break;
                    }
                }

                if (hasColor == false)
                    return;
            }
        }


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

        if (destroyType == PLANT_DESTROY_TYPE.COLOR)
        {
            if (splashColorType == BlockColorType.NONE)
                return;

            if (colorCount[(int)splashColorType - 1] <= 0)
            {
                return;
            }
        }

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
            foreach (var tempBoard in listSprite)
                tempBoard.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        if (destroyType == PLANT_DESTROY_TYPE.COLOR)
        {
            foreach (var temp in listSprite)
                temp.enabled = false;

            int tempCount = 0;
            for (int j = 0; j < colorCount.Length; j++)
            {
                for (int i = 0; i < colorCount[j]; i++)
                {
                    listSprite[tempCount].enabled = true;
                    //listSprite[tempCount].spriteName = "plant2_" + ManagerBlock.instance.GetColorTypeString((BlockColorType)(j + 1));
                    tempCount++;

                    if (tempCount >= listSprite.Count)
                        break;
                }

                if (tempCount >= listSprite.Count)
                    break;
            }
        }
        else
        {
            for (int i = lifeCount-1; i < MAX_LIFE; i++)
            {
                listSprite[i].enabled = false;
            }            
        }

        UpdateSpriteByBlockType();
        //tempSprite.enabled = false;
        playCoPang = false;

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
}
