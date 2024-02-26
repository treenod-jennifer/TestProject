using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PLANT_TYPE
{
    NORMAL, //0
    APPLE,  //1
    KEY,    //2
    ICE_APPLE,  //3
    LINE,       //4
    CIRCLE,         //5
    CANDY,      //6
    DUCK,       //7

    LINE_V,       //8
    LINE_H,       //9

    EVENT,      //10
    WATER,      //11
    COIN,       //12

    HealPotion,     //13
    SkillPotion,    //14

    COIN_BAG,    //15
    SPACESHIP,      //16
}

public class BlockPlant : BlockBase
{
    public PLANT_TYPE plantType = PLANT_TYPE.NORMAL;

    public int EventIndex = 0;

    public UISprite plantPlace_Sprite;

    public int AppleCount = 0;

    public override bool IsDigTarget()
    {
        return plantType == PLANT_TYPE.KEY;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsBlockType()
    {
        return false;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return true;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsThisBlockHasPlace()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        if (lifeCount <= 0) return;

        switch (plantType)
        {
            case PLANT_TYPE.WATER:
            case PLANT_TYPE.NORMAL:
            case PLANT_TYPE.CANDY:
                if (colorType == BlockColorType.NONE)
                    mainSprite.spriteName = "plant" + lifeCount;
                else if(colorType == BlockColorType.C)                
                    mainSprite.spriteName = "plant" + lifeCount + "_yellow";                
                else if (colorType == BlockColorType.E)
                    mainSprite.spriteName = "plant" + lifeCount + "_blue";
                break;
            case PLANT_TYPE.APPLE:
                mainSprite.spriteName = "plantApple" + lifeCount;
                break;
            case PLANT_TYPE.ICE_APPLE:
                mainSprite.spriteName = "plant" + lifeCount + "_iceApple";
                break;
            case PLANT_TYPE.KEY:
                mainSprite.spriteName = "plantKey" + lifeCount;
                break;
            case PLANT_TYPE.LINE:
            case PLANT_TYPE.LINE_V:
            case PLANT_TYPE.LINE_H:
                mainSprite.spriteName = "plant" + lifeCount + "_Line";
                break;
            case PLANT_TYPE.CIRCLE:
                mainSprite.spriteName = "plant" + lifeCount + "_Circle";
                break;
            case PLANT_TYPE.DUCK:
                mainSprite.spriteName = "plant" + lifeCount + "_Duck";
                break;
            case PLANT_TYPE.COIN:
                mainSprite.spriteName = "plant" + lifeCount + "_coin";
                break;
            case PLANT_TYPE.HealPotion:
                mainSprite.spriteName = "plantPotionHeal" + lifeCount;
                break;
            case PLANT_TYPE.SkillPotion:
                mainSprite.spriteName = "plantPotionSkill" + lifeCount;
                break;
            case PLANT_TYPE.COIN_BAG:
                string number = "3";
                if (AppleCount < 10)
                    number = "1";
                else if (10 <= AppleCount && AppleCount < 20)
                    number = "2";
                mainSprite.spriteName = string.Format("plant{0}_wallet_0{1}", lifeCount, number);
                break;
            case PLANT_TYPE.SPACESHIP:
                mainSprite.spriteName = "plant" + lifeCount + "_Spider";
                break;
        }

		mainSprite.cachedTransform.localPosition = (lifeCount < 3) ? new Vector3(0f, 2f, 0f) : Vector3.zero;
        mainSprite.transform.localScale = Vector3.one;
        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    public override void SetMainSpriteDepth()
    {
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE;
    }

    public override void UpdateBlock()
    {
        _waitCount++;
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
            if(lifeCount > 1) ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (pangIndex == uniqueIndex) return;

        if (colorType != BlockColorType.NONE && colorType != splashColorType) return;   //컬러매치

        pangIndex = uniqueIndex;
        Pang();
    }

    void Start()
    {
        Board back = PosHelper.GetBoard(indexX, indexY);
        if (back.IsActiveBoard)
        {
            plantPlace_Sprite.spriteName = "plantPlace";
            plantPlace_Sprite.depth = (int)GimmickDepth.DECO_GROUND;
            MakePixelPerfect(plantPlace_Sprite);
        }
        else
        {
            Destroy(plantPlace_Sprite.gameObject);
        }
    }

    IEnumerator CoPang()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        UpdateSpriteByBlockType();

        timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        MakePixelPerfect(mainSprite);
    }

    IEnumerator CoPangFinal()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();

        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(_transform.position, 80);


        if (plantPlace_Sprite != null)
            Destroy(plantPlace_Sprite.gameObject);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if(back != null) back.SetUnderBoard();

        float timer = 0f;

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        if (plantType == PLANT_TYPE.APPLE)
        {
            BlockBase blockApple = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.APPLE, BlockColorType.NONE, AppleCount);
            ManagerBlock.boards[indexX, indexY].Block = blockApple;

            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.APPLE]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.APPLE]--;
        }
        else if (plantType == PLANT_TYPE.ICE_APPLE)
        {
            BlockBase blockApple = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ICE_APPLE, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockApple;

            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ICE_APPLE]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ICE_APPLE]--;
        }
        else if (plantType == PLANT_TYPE.KEY)
        {
            BlockBase blockKey = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.KEY, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockKey;

            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.KEY]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.KEY]--;
        }
        else if (plantType == PLANT_TYPE.LINE)
        {
            NormalBlock lineBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = lineBlock;
            int randomLine = GameManager.instance.GetIngameRandom(0, 2);
            lineBlock.bombType = randomLine == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;
            lineBlock.makeLineBomb = true;
            lineBlock.InitBlock(type);
        }
        else if (plantType == PLANT_TYPE.LINE_V)
        {
            NormalBlock lineBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = lineBlock;
            lineBlock.bombType =  BlockBombType.LINE_V;
            lineBlock.makeLineBomb = true;
            lineBlock.InitBlock(type);
        }
        else if (plantType == PLANT_TYPE.LINE_H)
        {
            NormalBlock lineBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = lineBlock;
            lineBlock.bombType = BlockBombType.LINE_H;
            lineBlock.makeLineBomb = true;
            lineBlock.InitBlock(type);
        }
        else if (plantType == PLANT_TYPE.CIRCLE)
        {
            NormalBlock bombBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = bombBlock;
            bombBlock.bombType = BlockBombType.BOMB;
            bombBlock.makeLineBomb = true;
            bombBlock.InitBlock(type);
        }
        else if (plantType == PLANT_TYPE.CANDY)
        {
            BlockBase blockApple = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.CANDY, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockApple;
        }
        else if (plantType == PLANT_TYPE.DUCK)
        {
            BlockBase blockApple = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.DUCK, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockApple;
        }
        else if (plantType == PLANT_TYPE.WATER)
        {
            DecoInfo decoWater = new DecoInfo();
            decoWater.BoardType = (int)BoardDecoType.WATER;
            DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(ManagerBlock.boards[indexX, indexY], indexX, indexY, decoWater);
            ManagerBlock.instance.GetWater = true;
            ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);
        }
        else if (plantType == PLANT_TYPE.COIN)
        {
            BlockBase blockCoin = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.BLOCK_COIN, BlockColorType.NONE);
            ManagerBlock.boards[indexX, indexY].Block = blockCoin;
        }
        else if (plantType == PLANT_TYPE.HealPotion)
        {
            BlockBase blockHealPotion = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ADVENTURE_POTION_HEAL, BlockColorType.NONE, 1);
            ManagerBlock.boards[indexX, indexY].Block = blockHealPotion;

            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]--;
        }
        else if (plantType == PLANT_TYPE.SkillPotion)
        {
            BlockBase blockSkillPotion = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ADVENTURE_POTION_SKILL, BlockColorType.NONE, 1);
            ManagerBlock.boards[indexX, indexY].Block = blockSkillPotion;
            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]--;
        }
        else if (plantType == PLANT_TYPE.COIN_BAG)
        {
            BlockBase blockCoinBag = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.COIN_BAG, BlockColorType.NONE, AppleCount);
            ManagerBlock.boards[indexX, indexY].Block = blockCoinBag;
            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.COIN_BAG]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.COIN_BAG]--;
        }
        else if(plantType == PLANT_TYPE.SPACESHIP)
        {
            BlockBase blockSpaceship = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.SPACESHIP, BlockColorType.NONE, AppleCount);
            ManagerBlock.boards[indexX, indexY].Block = blockSpaceship;
            ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.SPACESHIP]--;
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.SPACESHIP]--;
            blockSpaceship.isMoveDownThisTurn = true;
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

        //아래에 석상있는지 체크후 석상작동
        if (back != null)
            back.CheckStatus();
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        yield return CoPangFinal();
        yield return null;
    }
}

