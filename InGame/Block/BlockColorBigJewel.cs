using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockColorBigJewel : BlockBase
{
    public List<Board> blockBoardList = new List<Board>();
    public int index;

    private List<BlockColorType> listColorCount = new List<BlockColorType>();
    private List<BlockColorType> listColorType = new List<BlockColorType>();

    [SerializeField] private List<UISprite> listSprite = new List<UISprite>();
    [SerializeField] private UISprite sprDisabled;
    
    public override bool IsDigTarget()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsBlockType()
    {
        return false;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsCanMakeBombFieldEffect()
    {
        return false;
    }

    //외부에서 블럭을 터트릴때 //폭발을 막는지 안막는지 체크
    public override bool BlockPang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false, bool isPangByPowerBomb = false)
    {
        for (int i = 0; i < blockBoardList.Count; i++)
        {
            for (int j = 0; j < blockBoardList[i].BoardOnHide.Count; j++)
            {
                blockBoardList[i].BoardOnHide[j].SetHideDecoPang();
                return IsPangExtendable();
            }
        }
        
        if (IsSkipPang)
            return IsPangExtendable();

        if (isRainbowBomb)
            return IsPangExtendable();

        if (state == BlockState.PANG)
            return IsPangExtendable();

        if (pangIndex == tempPangIndex) return IsPangExtendable();
        if (pangIndex > tempPangIndex) return IsPangExtendable();

        pangIndex = tempPangIndex;

        Board getBoardB = PosHelper.GetBoardSreeen(indexX, indexY);
        if (getBoardB == null)
        {
            normalPangDelay = false;
            return IsPangExtendable();
        }

        bool interruptExtendPangB = IsPangExtendable();

        //화단에 폭탄을 맞았을 때 돌아가는데 그 부분은 클로버가 없으므로 
        //HasDecoCoverBlock안에 있는 Clover 동작하는 부분을 동작하지 않도록 처리
        if (getBoardB.HasDecoHideBlock(true, tempPangIndex, pangColorType))
        {
            normalPangDelay = false;
            return interruptExtendPangB;
        }
        else if (getBoardB.HasDecoCoverBlock(true, tempPangIndex, pangColorType))
        {
            normalPangDelay = false;
            return interruptExtendPangB;
        }

        if (blockDeco == null)
        {
            if (normalPangDelay == false)
            {
                Pang(pangColorType, PangByBomb);
            }
            return interruptExtendPangB;
        }

        if (blockDeco.DecoPang(tempPangIndex, pangColorType))
        {
            normalPangDelay = false;
        }
        else
        {
            if (normalPangDelay == false)
            {
                Pang(pangColorType, PangByBomb);
            }
        }
        return interruptExtendPangB;
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (lifeCount <= 0 || state == BlockState.PANG)
            return;

        if (byRainbow == false && pangIndex == uniqueIndex)
            return;

        if (splashColorType == BlockColorType.NONE)
            return;

        for (int i = 0; i < blockBoardList.Count; i++)
        {
            if (blockBoardList[i].DecoOnBoard
                .Find((DecoBoard) => DecoBoard.boardDecoOrder == BoardDecoOrder.CLOVER))
            {
                return;
            }
        }

        pangIndex = uniqueIndex;
        Pang(splashColorType);
    }

    bool playCoPang = false;
    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (!PangByBomb && checkColorData(pangColorType) == 0)
            return;

        int spriteIndex = checkColorData(pangColorType);

        if (PangByBomb)
        {
            BlockColorType randomColor = GetRandomColor();
            spriteIndex = DeleteColorData(randomColor);
        }
        else
        {
            DeleteColorData(pangColorType);
        }

        lifeCount--;

        if (lifeCount <= 0)
        {
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal(spriteIndex - 1));
        }
        else
        {
            state = BlockState.WAIT;
            StartCoroutine(CoPang(spriteIndex - 1));
            ManagerSound.AudioPlayMany(AudioInGame.FLOWERPOT_1PANG);
        }
    }


    IEnumerator CoPang(int spriteIndex) //
    {
        InGameEffectMaker.instance.MakeFlowerBedFlower(listSprite[spriteIndex].transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            listSprite[spriteIndex].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        UpdateSpriteByBlockType();
        playCoPang = false;

        timer = 0f;
        while (timer < 1f)
        {
            listSprite[spriteIndex].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
    }

    bool isTarget = false;

    IEnumerator CoPangFinal(int spriteIndex)
    {
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.FLOWER_POT)
            {
                isTarget = true;
                break;
            }
        }

        float timer = 0f;
        while (timer < 1f)
        {
            listSprite[spriteIndex].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        UpdateSpriteByBlockType();
        playCoPang = false;

        bool makeEffect = false;
        timer = 0f;
        while (timer < 1f)
        {
            listSprite[spriteIndex].transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;

            if (timer > 0.5f && makeEffect == false)
            {
                makeEffect = true;
                InGameEffectMaker.instance.MakeFlowerBedPang(_transform.position);
            }

            yield return null;
        }
        
        if (isTarget)
            InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.FLOWER_POT);

        ManagerBlock.instance.AddScore(1000);
        InGameEffectMaker.instance.MakeScore(transform.position, 1000);
        ManagerSound.AudioPlayMany(AudioInGame.FLOWERPOT_DESTROY);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        timer = 0f;
        while (timer < 1f)
        {
            transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
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
        
        PangDestroyBoardData();
    }

    public override void UpdateSpriteByBlockType()
    {
        SetMainSpriteDepth();

        for (int i=0; i<listColorCount.Count; i++)
        {
            if (i < listColorCount.Count)
            {
                listSprite[i].gameObject.SetActive(true);
                listSprite[i].spriteName = GetBlockSpriteName(listColorCount[i], listColorType[i]);
                listSprite[i].depth = mainSprite.depth + 1 + i;
                MakePixelPerfect(listSprite[i]);
            }
        }
        
        sprDisabled.depth = mainSprite.depth + listSprite.Count + 1;
    }

    public void initBlock(int tempType, int[] subColors)
    {
        //if (colorCount.Count == 0)

        listColorCount.Clear();
        listColorType.Clear();

        {
            int life = 0;

            for (int i=0; i<subColors.Length; i++)
            {
                if (subColors[i] > 0 && subColors[i] < 10)
                {
                    life++;
                    listColorCount.Add((BlockColorType)subColors[i]);
                    listColorType.Add((BlockColorType)subColors[i]);
                }
                else if (subColors[i] == 0)
                {
                    life++;
                    BlockColorType temp = BlockMaker.instance.GetBlockRandomTypeAtMakeMap();

                    listColorCount.Add(temp);
                    listColorType.Add(temp);
                }
                else if (subColors[i] == 10)
                {
                    BlockColorType temp = BlockMaker.instance.GetBlockRandomTypeAtMakeMap();

                    listColorCount.Add(BlockColorType.NONE);
                    listColorType.Add(temp);
                }
                else
                {
                    listColorCount.Add(BlockColorType.NONE);
                    listColorType.Add((BlockColorType)(subColors[i] % 10));
                }
            }

            lifeCount = life;
        }

        Vector3 centerPos = new Vector3();
        foreach (var tempBoard in blockBoardList)
        {
            centerPos += PosHelper.GetPosByIndex(tempBoard.indexX, tempBoard.indexY);
        }
        centerPos = centerPos * 0.25f;
        _transform.localPosition = centerPos;

        UpdateSpriteByBlockType();
        HideDecoRemoveAction();
    }

    protected int checkWaitFrame = 0;
    public override void UpdateBlock()
    {
        if (state == BlockState.WAIT)
        {
            //1x1 사이즈를 넘는 기믹들은 카운트 계산을 한 프레임을 저장해 중복해서 waitCount가 증가되지 않도록 함.
            if (checkWaitFrame != Time.frameCount)
            {
                checkWaitFrame = Time.frameCount;
                _waitCount++;
            }
        }
    }

    private string GetBlockSpriteName(BlockColorType blockType, BlockColorType blockTypeA)
    {
        if (blockType == BlockColorType.NONE)
            return "flower_02_" + ManagerBlock.instance.GetColorTypeString(blockTypeA);
        else
            return "flower_01_" + ManagerBlock.instance.GetColorTypeString(blockTypeA);
    }

    int checkColorData(BlockColorType deleteColor)
    {
        for (int i = 0; i < listColorCount.Count; i++)
        {
            if (listColorCount[i] == deleteColor)            
                return i+1;            
        }
        return 0;
    }

    private int DeleteColorData(BlockColorType deleteColor)
    {
        int contains = 0;
        for (int i = 0; i < listColorCount.Count; i++)
        {
            if (listColorCount[i] == deleteColor)
            {
                listColorCount[i] = BlockColorType.NONE;
                contains = i + 1;
                break;
            }
        }
        return contains;
    }

    //폭탄으로 터졌을때 제거할 색상을 선택하는 기능
    private BlockColorType GetRandomColor()
    {
        foreach(var color in listColorCount)
        {
            if (color != BlockColorType.NONE)
                return color;
        }

        return BlockColorType.NONE;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        for (int i = 0; i < blockBoardList.Count; i++)
        {
            if (blockBoardList[i].HasDecoHideBlock())
                return FireWorkRank.RANK_NONE;
        }
        
        if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.FLOWER_POT))
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }

    public override void HideDecoRemoveAction()
    {
        int cloverCount = 0;
        
        for (int i = 0; i < blockBoardList.Count; i++)
        {
            if (blockBoardList[i].HasDecoHideBlock())
            {
                cloverCount++;   
            }
        }

        IsDisabled = cloverCount > 0; 
        
        sprDisabled.gameObject.SetActive(IsDisabled);
    }
}
