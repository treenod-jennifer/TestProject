using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneStatue : DecoBase, Istatue 
{
    const string SPRITE_NAME = "StoneStatue";

    public List<Board> listBoards = new List<Board>();
    public int rotateDirection = 0;
    public int type = 0;

    public float statueScale = 1;

    public int index
    {
        set;
        get;
    }

    public void Init()
    {
        Vector3 centerVector3 = Vector3.zero;
        foreach (var backData in listBoards)
        {
            centerVector3 += PosHelper.GetPosByIndex(backData.indexX, backData.indexY);
        }
        centerVector3 = centerVector3 / listBoards.Count;
        transform.localPosition = centerVector3;
        SetSpriteSize();
        if (GameManager.instance.state == GameState.EDIT)
        {
            SetStatueText();
        }
    }

    public override void SetSprite()
    {
        uiSprite.spriteName = SPRITE_NAME;
        uiSprite.depth = (int)GimmickDepth.DECO_AREA;
        MakePixelPerfect(uiSprite);
    }

    public void AddBoard(Board tempBoard)
    {
        if (!listBoards.Contains(tempBoard))
            listBoards.Add(tempBoard);
    }

    bool isGet = false;
    public void CheckBoardHasGrass()
    {
        bool isGetStatue = true;

        //해당 보드의 데코검사.
        if (isGetStatue == true)
        {
            for (int i = 0; i < listBoards.Count; i++)
            {
                Board checkBoard = listBoards[i];
                //해당 보드의 블럭검사.
                if (checkBoard.Block != null && checkBoard.Block.IsCoverStatue() == true)
                {
                    isGetStatue = false;
                    break;
                }
                for (int j = 0; j < checkBoard.DecoOnBoard.Count; j++)
                {
                    //남아있는 데코 중, 현재 석판을 덮는 데코가 있으면 획득 안 함.
                    if (checkBoard.DecoOnBoard[j].IsCoverStatue() == true)
                    {
                        isGetStatue = false;
                        break;
                    }
                }
            }
        }

        if (isGet) return;

        if (isGetStatue)
        {
            isGet = true;
            FlyTarget.flyTargetCount++;
            StartCoroutine(CoGetStatueAnimation());
            //Debug.Log("석상획득" +index);
        }
    }

    IEnumerator CoGetStatueAnimation()
    {

        foreach (var board in listBoards)
        {
            board.RemoveDeco(this);
        }

        uiSprite.depth = (int)GimmickDepth.FX_FLYEFFECT;

        yield return null;

        float timer = 0;
        float speed = 4f;


        bool changeRotation = transform.localRotation == Quaternion.Euler(new Vector3(0, 0, 90));
        Vector3 scaleA = transform.localScale;

        float statueWaitTmer = 0;

        while(FlyStatue.flyStatueList.Count > 0)
        {
            if (statueWaitTmer > 0.6f)
            {
                statueWaitTmer = 0;
                break;
            }

            statueWaitTmer += Global.deltaTimePuzzle;
            yield return null;
        }

        ManagerBlock.instance.AddScore(1000);
        InGameEffectMaker.instance.MakeScore(transform.position, 1000);

        FlyStatue flyStatueObj = InGameEffectMaker.instance.MakeFlyStatue(transform.localPosition, transform.localEulerAngles.z, Vector3.one* statueScale);
        flyStatueObj.StartFly();
        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.STATUE);
        //Debug.Log("석상날림" + index);

        ManagerSound.AudioPlay(AudioInGame.GET_STONE_STATUE);


        for (int i = 0; i < listBoards.Count; i++)
        {            
                listBoards[i].movable = true;            
        }
        yield return null;

        RemoveDeco();
        yield return null;
    }

    public override void RemoveDeco()
    {
        _listBoardDeco.Remove(this);
        ManagerBlock.instance.listObject.Remove(gameObject);
        Destroy(gameObject);
        Destroy(this);
    }


    void SetSpriteSize()
    {
        int maxX = listBoards[0].indexX;
        int minX = listBoards[0].indexX;
        int maxY = listBoards[0].indexY;
        int minY = listBoards[0].indexY;

        for (int i = 0; i < listBoards.Count; i++)
        {
            for (int j = 0; j < listBoards.Count; j++)
            {
                if (listBoards[i].indexX > listBoards[j].indexX)
                {
                    if (minX > listBoards[j].indexX)
                    {
                        minX = listBoards[j].indexX;
                    }
                }
                if (listBoards[i].indexX < listBoards[j].indexX)
                {
                    if (maxX < listBoards[j].indexX)
                    {
                        maxX = listBoards[j].indexX;
                    }
                }

                if (listBoards[i].indexY > listBoards[j].indexY)
                {
                    if (minY > listBoards[j].indexY)
                    {
                        minY = listBoards[j].indexY;
                    }
                }
                if (listBoards[i].indexY < listBoards[j].indexY)
                {
                    if (maxY < listBoards[j].indexY)
                    {
                        maxY = listBoards[j].indexY;
                    }
                }
            }
        }

        if (maxX - minX > maxY - minY) //가로
        {
            if ((maxX - minX + 1) == 2)
            {
                uiSprite.spriteName = SPRITE_NAME + "2";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(1, 1.33f, 0);

                statueScale = 0.5f;
            }
            else if ((maxX - minX + 1) == 3)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);

                statueScale = 1f;
            }
            else if ((maxX - minX + 1) == 4)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(1.54f, 1.35f, 0);
                statueScale = 2f;
            }
            else if ((maxX - minX + 1) == 6)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(2f, 2f, 0);
                statueScale = 3f;
            }


            if (rotateDirection == 0)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            }
            else
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90)); }
        }
        else
        {
            if ((maxY - minY + 1) == 2)
            {
                uiSprite.spriteName = SPRITE_NAME + "2";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(1, 1.33f, 0);
                statueScale = 0.5f;
            }
            else if ((maxY - minY + 1) == 3)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);
                statueScale = 1f;
            }
            else if ((maxY - minY + 1) == 4)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(1.54f, 1.35f, 0);
                statueScale = 2f;
            }
            else if ((maxY - minY + 1) == 6)
            {
                uiSprite.spriteName = SPRITE_NAME + "3";
                MakePixelPerfect(uiSprite);
                transform.localScale = new Vector3(2f, 2f, 0);
                statueScale = 3f;
            }

            if (rotateDirection == 0)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0)); }
            else
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
            }
        }
    }

    void SetStatueText()
    {
        int maxX = listBoards[0].indexX;
        int minX = listBoards[0].indexX;
        int maxY = listBoards[0].indexY;
        int minY = listBoards[0].indexY;

        for (int i = 0; i < listBoards.Count; i++)
        {
            for (int j = 0; j < listBoards.Count; j++)
            {
                if (listBoards[i].indexX > listBoards[j].indexX)
                {
                    if (minX > listBoards[j].indexX)
                    {
                        minX = listBoards[j].indexX;
                    }
                }
                if (listBoards[i].indexX < listBoards[j].indexX)
                {
                    if (maxX < listBoards[j].indexX)
                    {
                        maxX = listBoards[j].indexX;
                    }
                }

                if (listBoards[i].indexY > listBoards[j].indexY)
                {
                    if (minY > listBoards[j].indexY)
                    {
                        minY = listBoards[j].indexY;
                    }
                }
                if (listBoards[i].indexY < listBoards[j].indexY)
                {
                    if (maxY < listBoards[j].indexY)
                    {
                        maxY = listBoards[j].indexY;
                    }
                }
            }
        }

        Board board = null;
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                board = PosHelper.GetBoard(i, j);
                if (board != null && CheckUpBlockOrDeco(board) == true)
                {
                    Vector3 targetPos = PosHelper.GetPosByIndex(i, j);
                    targetPos += new Vector3(-25f, 0f, 0f);
                    UILabel label = MakeLabel(GameUIManager.instance.groundAnchor, "S" + index, targetPos);
                    label.transform.parent = gameObject.transform;
                }
            }
        }
    }

    private bool CheckUpBlockOrDeco(Board board)
    {
        if (GameManager.instance.state != GameState.EDIT)
            return false;

        if (board == null)
            return false;
        
        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            if (board.DecoOnBoard[i] as Water != null || board.DecoOnBoard[i] as Grass != null
                || board.DecoOnBoard[i] as Crack != null || board.DecoOnBoard[i] as Carpet != null)
            {
                return true;
            }
        }

        if (board.Block != null && 
            (board.Block.blockDeco != null || board.Block.type == BlockType.STONE || board.Block.type == BlockType.GROUND
            || board.Block.type == BlockType.LITTLE_FLOWER_POT || board.Block.type == BlockType.SODAJELLY || board.Block.type == BlockType.BREAD
            || board.Block.type == BlockType.GROUND_JEWEL || board.Block.type == BlockType.GROUND_KEY || board.Block.type == BlockType.GROUND_BOMB
            || board.Block.type == BlockType.GROUND_APPLE || board.Block.type == BlockType.GROUND_ICE_APPLE || board.Block.type == BlockType.PLANT_ICE_APPLE
            || board.Block.type == BlockType.PLANT2X2 || board.Block.type == BlockType.ICE || board.Block.type == BlockType.PLANT))
            return true;
        return false;
    }

    private UILabel MakeLabel(GameObject obj, string text, Vector3 tr)
    {
        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = new Color(124f / 255f, 40f / 255f, 40f / 255f);
        uiLabel.color = new Color(1f, 208f / 255f, 56f / 255f);
        uiLabel.text = text;

        uiLabel.transform.localPosition = tr;
        return uiLabel;
    }
}
