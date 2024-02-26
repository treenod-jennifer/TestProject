using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum sandDirection
{
    NONE,
    UP,
    RIGHT,
    DOWN,
    LEFT,

    RIGHT_DOWN,
    DOWN_LEFT,
    LEFT_UP,
    UP_RIGHT,

    LEFT_DOWN,
    DOWN_RIGHT,
    RIGHT_UP,
    UP_LEFT,    
}


public class SandBelt : DecoBase, IMover
{
    const string Straight_Sprite = "sandBelt_Straight";
    const string Round_Sprite = "sandBelt_Round";

    public BlockDirection direction = BlockDirection.NONE; //나가는방향
    public sandDirection sandDir = sandDirection.NONE;

    public UISprite sandWave1;
    public UISprite sandWave2;

    public bool startSand = false;
    public Board exitBoard;
    public int sandIndex = 0;

    public void MoveSandBelt(float ratio, float ratio2)
    {
        float sandRatio = ratio;// (1 - Mathf.Cos(Mathf.PI * ratio)) * 0.5f;      //ratio;// Mathf.Sin(Mathf.PI * ratio*0.5f);
        float sandRatio2 = (ratio2 + 0.5f) % 1f; //ratio

        switch (sandDir)
        {
            case sandDirection.UP:
                sandWave1.transform.localPosition = Vector3.Lerp(new Vector3(0, -39, 0), new Vector3(0, 39, 0), sandRatio);
                sandWave2.transform.localPosition = Vector3.Lerp(new Vector3(0, -39, 0), new Vector3(0, 39, 0), sandRatio2);
                break;
            case sandDirection.DOWN:
                sandWave1.transform.localPosition = Vector3.Lerp(new Vector3(0, 39, 0), new Vector3(0, -39, 0), sandRatio);
                sandWave2.transform.localPosition = Vector3.Lerp(new Vector3(0, 39, 0), new Vector3(0, -39, 0), sandRatio2);
                break;

            case sandDirection.RIGHT:
                sandWave1.transform.localPosition = Vector3.Lerp(new Vector3(-39, 0, 0), new Vector3(39, 0, 0), sandRatio);
                sandWave2.transform.localPosition = Vector3.Lerp(new Vector3(-39, 0, 0), new Vector3(39, 0, 0), sandRatio2);
                break;
            case sandDirection.LEFT:
                sandWave1.transform.localPosition = Vector3.Lerp(new Vector3(39, 0, 0), new Vector3(-39, 0, 0), sandRatio);
                sandWave2.transform.localPosition = Vector3.Lerp(new Vector3(39, 0, 0), new Vector3(-39, 0, 0), sandRatio2);
                break;

            case sandDirection.UP_LEFT:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, 90), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, 90), sandRatio2);
                break;
            case sandDirection.RIGHT_DOWN:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 0), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 0), sandRatio2);
                break;

            case sandDirection.RIGHT_UP:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 180), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 180), sandRatio2);
                break;

            case sandDirection.DOWN_LEFT:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -90), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -90), sandRatio2);
                break;

            case sandDirection.DOWN_RIGHT:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, 90), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, 90), sandRatio2);
                break;
            case sandDirection.LEFT_UP:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 0), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 0), sandRatio2);
                break;

            case sandDirection.UP_RIGHT:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, -90), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, -90), sandRatio2);
                break;
            case sandDirection.LEFT_DOWN:
                sandWave1.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 180), sandRatio);
                sandWave2.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 180), sandRatio2);
                break;
        }

        if (exitBoard != null)
        {
            sandWave1.color = new Color(1, 1, 1, Mathf.Cos(Mathf.PI * sandRatio*0.5f));
            sandWave2.color = new Color(1, 1, 1, Mathf.Cos(Mathf.PI * sandRatio2*0.5f));
        }

        if (startSand)
        {
            sandWave1.color = new Color(1, 1, 1, Mathf.Sin(Mathf.PI * sandRatio * 0.5f));
            sandWave2.color = new Color(1, 1, 1, Mathf.Sin(Mathf.PI * sandRatio2 * 0.5f));
        }
    }

    public void SetSandSprite(string sandName, bool vert, bool hori,bool rotate= false)
    {
        uiSprite.spriteName = sandName;
        uiSprite.depth = (int)GimmickDepth.DECO_GROUND;
        MakePixelPerfect(uiSprite);
        MakePixelPerfect(sandWave1);
        MakePixelPerfect(sandWave2);

        float rotateZ = rotate ? 90f : 0f;

        uiSprite.cachedTransform.localScale = new Vector3(vert ? -1 : 1, hori ? -1 : 1, 1);
        uiSprite.gameObject.transform.localEulerAngles = new Vector3(0, 0, rotateZ);
    }

    public override void Init()
    {
       // GameObject obj = NGUITools.AddChild(gameObject, BoardDecoMaker.instance.arrowPrefab);
       // obj.transform.localEulerAngles = new Vector3(0, 0, -90 * ((int)direction - 1));

        if (IsGetSand(PosHelper.GetBoard(inX, inY, -1, 0)))
        {
            if (IsGetSand(PosHelper.GetBoard(inX, inY, 1, 0)))
            {
                if (direction == BlockDirection.RIGHT) sandDir = sandDirection.RIGHT;
                else if (direction == BlockDirection.LEFT) sandDir = sandDirection.LEFT;

                SetSandSprite(Straight_Sprite, false, false, true);
            }
            else if (IsGetSand(PosHelper.GetBoard(inX, inY, 0, 1)))
            {
                if (direction == BlockDirection.LEFT) sandDir = sandDirection.UP_LEFT;
                else if (direction == BlockDirection.DOWN) sandDir = sandDirection.RIGHT_DOWN;
                SetSandSprite(Round_Sprite, false, false);
            }
            else if (IsGetSand(PosHelper.GetBoard(inX, inY, 0, -1)))
            {
                if (direction == BlockDirection.LEFT) sandDir = sandDirection.DOWN_LEFT;
                else if (direction == BlockDirection.UP) sandDir = sandDirection.RIGHT_UP;
                SetSandSprite(Round_Sprite, false, true);
            }
            else
            {
                if (direction == BlockDirection.RIGHT) sandDir = sandDirection.RIGHT;
                else if (direction == BlockDirection.LEFT) sandDir = sandDirection.LEFT;
                SetSandSprite(Straight_Sprite, false, false, true);
            }
            
        }
        else if (IsGetSand(PosHelper.GetBoard(inX, inY, 0, -1)))
        {
            if (IsGetSand(PosHelper.GetBoard(inX, inY, 0, 1)))
            {
                if (direction == BlockDirection.UP) sandDir = sandDirection.UP;
                else if (direction == BlockDirection.DOWN) sandDir = sandDirection.DOWN;
                SetSandSprite(Straight_Sprite, false, false);
            }
            if (IsGetSand(PosHelper.GetBoard(inX, inY, 1, 0)))
            {
                if (direction == BlockDirection.RIGHT) sandDir = sandDirection.DOWN_RIGHT;
                else if (direction == BlockDirection.UP) sandDir = sandDirection.LEFT_UP;
                SetSandSprite(Round_Sprite, true, true);
            }
            else
            {
                if (direction == BlockDirection.UP) sandDir = sandDirection.UP;
                else if (direction == BlockDirection.DOWN) sandDir = sandDirection.DOWN;
                SetSandSprite(Straight_Sprite, false, false);
            }
        }
        else if (IsGetSand(PosHelper.GetBoard(inX, inY, 1, 0)))
        {
            if (IsGetSand(PosHelper.GetBoard(inX, inY, 0, 1)))
            {
                if (direction == BlockDirection.RIGHT) sandDir = sandDirection.UP_RIGHT;
                else if (direction == BlockDirection.DOWN) sandDir = sandDirection.LEFT_DOWN;
                SetSandSprite(Round_Sprite, true, false);
            }
            else
            {
                if (direction == BlockDirection.RIGHT) sandDir = sandDirection.RIGHT;
                else if (direction == BlockDirection.LEFT) sandDir = sandDirection.LEFT;
                SetSandSprite(Straight_Sprite, false, false, true);
            }
        }

        if(sandDir == sandDirection.NONE)
        {
            if (direction == BlockDirection.DOWN) sandDir = sandDirection.DOWN;
            else if (direction == BlockDirection.UP) sandDir = sandDirection.UP;
            else if (direction == BlockDirection.LEFT) sandDir = sandDirection.LEFT;
            else if (direction == BlockDirection.RIGHT) sandDir = sandDirection.RIGHT;
        }


        if (exitBoard != null)
        {
            GameObject potalObj = NGUITools.AddChild(gameObject, BoardDecoMaker.instance.SandPotalPrefab);
            potalObj.transform.localEulerAngles = new Vector3(0, 0, -90 * ((int)(direction + 2 - 1) % 4));

            foreach (var deco in exitBoard.BoardOnMover)
            {
                SandBelt tempSand = deco as SandBelt;
                if(tempSand != null)
                {
                    tempSand.startSand = true;
                    GameObject potal2Obj = NGUITools.AddChild(tempSand.gameObject, BoardDecoMaker.instance.SandPotalPrefab);
                    potal2Obj.transform.localEulerAngles = new Vector3(0, 0, -90 *  ((int)(tempSand.direction - 1)));
                }
            }
        }

        switch (sandDir)
        {
            case sandDirection.UP:
                sandWave1.transform.localPosition = new Vector3(0, 39, 0);
                sandWave2.transform.localPosition = new Vector3(0, 39, 0);
                break;
            case sandDirection.DOWN:
                sandWave1.transform.localPosition = new Vector3(0, -39, 0);
                sandWave1.transform.localScale = new Vector3(1, -1, 1);

                sandWave2.transform.localPosition = new Vector3(0, -39, 0);
                sandWave2.transform.localScale = new Vector3(1, -1, 1);
                break;

            case sandDirection.RIGHT:
                sandWave1.transform.localPosition = new Vector3(39, 0, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, -90);

                sandWave2.transform.localPosition = new Vector3(39, 0, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, -90);
                break;
            case sandDirection.LEFT:
                sandWave1.transform.localPosition = new Vector3(-39, 0, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, 90);

                sandWave2.transform.localPosition = new Vector3(-39, 0, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, 90);
                break;

            case sandDirection.UP_LEFT:
                sandWave1.pivot = UIWidget.Pivot.Left;
                sandWave1.transform.localPosition = new Vector3(-39, -39, 0);

                sandWave2.pivot = UIWidget.Pivot.Left;
                sandWave2.transform.localPosition = new Vector3(-39, -39, 0);
                break;
            case sandDirection.RIGHT_DOWN:
                sandWave1.pivot = UIWidget.Pivot.Left;
                sandWave1.transform.localPosition = new Vector3(-39, -39, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, 90);
                sandWave1.transform.localScale = new Vector3(1, -1, 1);

                sandWave2.pivot = UIWidget.Pivot.Left;
                sandWave2.transform.localPosition = new Vector3(-39, -39, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, 90);
                sandWave2.transform.localScale = new Vector3(1, -1, 1);
                break;

            case sandDirection.RIGHT_UP:
                sandWave1.pivot = UIWidget.Pivot.Right;
                sandWave1.transform.localPosition = new Vector3(-39, 39, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, 90);
                sandWave1.transform.localScale = new Vector3(1, -1, 1);

                sandWave2.pivot = UIWidget.Pivot.Right;
                sandWave2.transform.localPosition = new Vector3(-39, 39, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, 90);
                sandWave2.transform.localScale = new Vector3(1, -1, 1);
                break;
            case sandDirection.DOWN_LEFT:
                sandWave1.pivot = UIWidget.Pivot.Left;
                sandWave1.transform.localPosition = new Vector3(-39, 39, 0);
                sandWave1.transform.localScale = new Vector3(1, -1, 1);

                sandWave2.pivot = UIWidget.Pivot.Left;
                sandWave2.transform.localPosition = new Vector3(-39, 39, 0);
                sandWave2.transform.localScale = new Vector3(1, -1, 1);
                break;

            case sandDirection.DOWN_RIGHT:
                sandWave1.pivot = UIWidget.Pivot.Right;
                sandWave1.transform.localPosition = new Vector3(39, 39, 0);
                sandWave1.transform.localScale = new Vector3(1, -1, 1);

                sandWave2.pivot = UIWidget.Pivot.Right;
                sandWave2.transform.localPosition = new Vector3(39, 39, 0);
                sandWave2.transform.localScale = new Vector3(1, -1, 1);
                break;
            case sandDirection.LEFT_UP:
                sandWave1.pivot = UIWidget.Pivot.Right;
                sandWave1.transform.localPosition = new Vector3(39, 39, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, 90);

                sandWave2.pivot = UIWidget.Pivot.Right;
                sandWave2.transform.localPosition = new Vector3(39, 39, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, 90);
                break;

            case sandDirection.UP_RIGHT:
                sandWave1.pivot = UIWidget.Pivot.Right;
                sandWave1.transform.localPosition = new Vector3(39, -39, 0);

                sandWave2.pivot = UIWidget.Pivot.Right;
                sandWave2.transform.localPosition = new Vector3(39, -39, 0);
                break;
            case sandDirection.LEFT_DOWN:
                sandWave1.pivot = UIWidget.Pivot.Left;
                sandWave1.transform.localPosition = new Vector3(39, -39, 0);
                sandWave1.transform.localEulerAngles = new Vector3(0, 0, 90);

                sandWave2.pivot = UIWidget.Pivot.Left;
                sandWave2.transform.localPosition = new Vector3(39, -39, 0);
                sandWave2.transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
        }

        //sandWave1.color = new Color(1, 1, 1, 0);
    }

    bool IsGetSand(Board GetNearBoard)
    {
        if (GetNearBoard != null)
        {
            foreach (var sand in GetNearBoard.DecoOnBoard)
            {
                SandBelt tempSand = sand as SandBelt;
                if (tempSand != null && tempSand.sandIndex == sandIndex)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override bool EventAction()
    {
        if (board.Block == null) return false;

        _waitCount++;
        //board.Block.state = BlockState.MOVE_OTHER;
        board.Block._velocity = 0;
        board.Block.mainSprite.cachedTransform.localScale = Vector3.one;

        ManagerBlock.instance.blockMove = false;
        ManagerBlock.instance.creatBlock = false;

        if (exitBoard != null)
        {
            bool differDir = false;

            tempBlock = board.Block;
            tempBlock.RemoveLinkerNoReset();

            tempBlock.RemoveTempBombType();

            //exitboard의 방향이 다를경우
            foreach(var temDeco in exitBoard.DecoOnBoard)
                if (temDeco is SandBelt)
                {
                    SandBelt tempSand = temDeco as SandBelt;
                    if (tempSand.direction != direction)
                    {
                        differDir = true;
                        OutDircteion = tempSand.direction;
                    }
                }

            if (differDir)
            {
                dummySrite = NGUITools.AddChild(GameUIManager.instance.groundAnchor, tempBlock.mainSprite.gameObject).GetComponent<UIBlockSprite>();
                dummySrite.spriteName = tempBlock.mainSprite.spriteName;

                if (OutDircteion == BlockDirection.LEFT)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX + 1, exitBoard.indexY);
                    tempBlock.mainSprite.customFill.verticalRatio = 1;
                }
                else if (OutDircteion == BlockDirection.RIGHT)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX - 1, exitBoard.indexY);
                    tempBlock.mainSprite.customFill.verticalRatio = -1;
                }
                else if (OutDircteion == BlockDirection.DOWN)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY - 1);
                    tempBlock.mainSprite.customFill.blockRatio = 1;
                }
                else if (OutDircteion == BlockDirection.UP)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY + 1);
                    tempBlock.mainSprite.customFill.blockRatio = 1;
                }
                
                if (direction == BlockDirection.LEFT)
                {
                    dummySrite.transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    dummySrite.customFill.verticalRatio = 0;
                    targetPosDummy = PosHelper.GetPosByIndex(inX-1, inY);
                }
                else if (direction == BlockDirection.RIGHT)
                {
                    dummySrite.transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    dummySrite.customFill.verticalRatio = 0;
                    targetPosDummy = PosHelper.GetPosByIndex(inX +1, inY);
                }
                else if (direction == BlockDirection.DOWN)
                {
                    dummySrite.transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    dummySrite.customFill.blockRatio = 0;
                    targetPosDummy = PosHelper.GetPosByIndex(inX, inY+1);
                }
                else if (direction == BlockDirection.UP)
                {
                    dummySrite.transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    dummySrite.customFill.verticalRatio = 0;
                    targetPosDummy = PosHelper.GetPosByIndex(inX, inY-1);
                }
                
                targetPos = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY);
                board.Block = null;
                StartCoroutine(CoPotalBeltActionDir());
            }
            else
            {
                dummySrite = NGUITools.AddChild(tempBlock.gameObject, tempBlock.mainSprite.gameObject).GetComponent<UIBlockSprite>();
                dummySrite.spriteName = tempBlock.mainSprite.spriteName;

                if (direction == BlockDirection.LEFT)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX + 1, exitBoard.indexY);
                    tempBlock.mainSprite.customFill.verticalRatio = 1;
                    dummySrite.cachedTransform.localPosition = (PosHelper.GetPosByIndex(inX, inY) - PosHelper.GetPosByIndex(exitBoard.indexX + 1, exitBoard.indexY));
                    dummySrite.customFill.verticalRatio = 0;
                }
                else if (direction == BlockDirection.RIGHT)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX - 1, exitBoard.indexY);
                    tempBlock.mainSprite.customFill.verticalRatio = -1;
                    dummySrite.cachedTransform.localPosition = (PosHelper.GetPosByIndex(inX, inY) - PosHelper.GetPosByIndex(exitBoard.indexX - 1, exitBoard.indexY));
                    dummySrite.customFill.verticalRatio = 0;
                }
                if (direction == BlockDirection.DOWN)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY - 1);
                    tempBlock.mainSprite.customFill.blockRatio = 1;
                    dummySrite.cachedTransform.localPosition = (PosHelper.GetPosByIndex(inX, inY) - PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY - 1));
                    dummySrite.customFill.blockRatio = 0;
                }
                else if (direction == BlockDirection.UP)
                {
                    tempBlock._transform.localPosition = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY + 1);
                    tempBlock.mainSprite.customFill.blockRatio = 1;
                    dummySrite.cachedTransform.localPosition = (PosHelper.GetPosByIndex(inX, inY) - PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY + 1));
                    dummySrite.customFill.verticalRatio = 0;
                }

                targetPos = PosHelper.GetPosByIndex(exitBoard.indexX, exitBoard.indexY);
                board.Block = null;
                StartCoroutine(CoPotalBeltAction());
            }
        }
        else
        {
            targetBoard = PosHelper.GetBoardByDir(inX, inY, direction);
            tempBlock = board.Block;
            targetPos = PosHelper.GetPosByIndex(targetBoard.indexX, targetBoard.indexY);
            StartCoroutine(CoBeltAction());
            board.Block = null;
        }

        return true;
    }

    BlockDirection OutDircteion = BlockDirection.NONE;

    Board targetBoard;
    BlockBase tempBlock;
    Vector3 targetPos;
    Vector3 targetPosDummy;
    UIBlockSprite dummySrite;

    IEnumerator CoPotalBeltActionDir()
    {
        float _velocity = MAX_VELOCITY;//0
        while (true)
        {
            _velocity += Global.deltaTimePuzzle * ManagerBlock.NORMAL_SPEED;
            _velocity = (_velocity > MAX_VELOCITY) ? MAX_VELOCITY : _velocity;

            if (Vector3.Distance(tempBlock._transform.localPosition, targetPos) < 1f)
            {
                tempBlock.indexX = exitBoard.indexX;
                tempBlock.indexY = exitBoard.indexY;

                exitBoard.Block = tempBlock;

                tempBlock._transform.localPosition = targetPos;
                tempBlock.targetPos = targetPos;

                tempBlock.mainSprite.customFill.verticalRatio = 0;
                tempBlock.mainSprite.customFill.blockRatio = 0;

                if(dummySrite != null)Destroy(dummySrite.gameObject);

                yield return null;
                break;
            }

            tempBlock._transform.localPosition = Vector3.MoveTowards(tempBlock._transform.localPosition, targetPos, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * MOVE_SPPED);
            if (dummySrite != null) dummySrite.transform.localPosition = Vector3.MoveTowards(dummySrite.transform.localPosition, targetPosDummy, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * MOVE_SPPED);

            float distance = 0;

            if (OutDircteion == BlockDirection.LEFT)
            {
                distance = targetPos.x - tempBlock._transform.localPosition.x;
                tempBlock.mainSprite.customFill.verticalRatio = -distance / ManagerBlock.BLOCK_SIZE;
            }
            else if (OutDircteion == BlockDirection.RIGHT)
            {
                distance = targetPos.x - tempBlock._transform.localPosition.x;
                tempBlock.mainSprite.customFill.verticalRatio = -distance / ManagerBlock.BLOCK_SIZE;
            }
            else if (OutDircteion == BlockDirection.DOWN)
            {
                distance = targetPos.y - tempBlock._transform.localPosition.y;
                tempBlock.mainSprite.customFill.blockRatio = -distance / ManagerBlock.BLOCK_SIZE;
            }
            else if (OutDircteion == BlockDirection.UP)
            {
                distance = targetPos.y - tempBlock._transform.localPosition.y;
                tempBlock.mainSprite.customFill.blockRatio = -1 + distance / ManagerBlock.BLOCK_SIZE;
            }

            distance = Mathf.Abs(distance);

            if (distance <= 0)
            {
                if (dummySrite != null) Destroy(dummySrite.gameObject);
            }

            if (dummySrite != null)
            {
                if (direction == BlockDirection.LEFT)
                {
                    dummySrite.customFill.verticalRatio = -1 + distance / ManagerBlock.BLOCK_SIZE;
                }
                else if (direction == BlockDirection.RIGHT)
                {
                    dummySrite.customFill.verticalRatio = 1 - distance / ManagerBlock.BLOCK_SIZE;
                }
                else if (direction == BlockDirection.DOWN)
                {
                    dummySrite.customFill.blockRatio = -distance / ManagerBlock.BLOCK_SIZE;
                }
                else if (direction == BlockDirection.UP)
                {
                    dummySrite.customFill.blockRatio = 1 + distance / ManagerBlock.BLOCK_SIZE;
                }
            }



            yield return null;
        }
        tempBlock.state = BlockState.WAIT;
        _waitCount--;
        yield return null;
    }



    IEnumerator CoPotalBeltAction()
    {
        float _velocity = 0;
        while (true)
        {
            _velocity += Global.deltaTimePuzzle * ManagerBlock.NORMAL_SPEED;
            _velocity = (_velocity > MAX_VELOCITY) ? MAX_VELOCITY : _velocity;


            if (Vector3.Distance(tempBlock._transform.localPosition, targetPos) < 1f)
            {
                tempBlock.indexX = exitBoard.indexX;
                tempBlock.indexY = exitBoard.indexY;

                exitBoard.Block = tempBlock;

                tempBlock._transform.localPosition = targetPos;
                tempBlock.targetPos = targetPos;

                tempBlock.mainSprite.customFill.verticalRatio = 0;
                tempBlock.mainSprite.customFill.blockRatio = 0;

                if (dummySrite != null) Destroy(dummySrite.gameObject);

                yield return null;
                break;
            }

            tempBlock._transform.localPosition = Vector3.MoveTowards(tempBlock._transform.localPosition, targetPos, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * MOVE_SPPED);

            if (direction == BlockDirection.LEFT)
            {
                float distance = targetPos.x - tempBlock._transform.localPosition.x;
                tempBlock.mainSprite.customFill.verticalRatio = - distance / ManagerBlock.BLOCK_SIZE;

                if (dummySrite != null) { dummySrite.customFill.verticalRatio = -1 - distance / ManagerBlock.BLOCK_SIZE; }
            }
            else if (direction == BlockDirection.RIGHT)
            {
                float distance = targetPos.x - tempBlock._transform.localPosition.x;
                tempBlock.mainSprite.customFill.verticalRatio =- distance / ManagerBlock.BLOCK_SIZE;

                if (dummySrite != null) { dummySrite.customFill.verticalRatio = 1 - distance / ManagerBlock.BLOCK_SIZE;
                }
            }
            else if (direction == BlockDirection.DOWN)
            {
                float distance = targetPos.y - tempBlock._transform.localPosition.y;
                tempBlock.mainSprite.customFill.blockRatio = -distance / ManagerBlock.BLOCK_SIZE;

                
                if (dummySrite != null) {
                    dummySrite.customFill.blockRatio = distance / ManagerBlock.BLOCK_SIZE;
                    if (distance / ManagerBlock.BLOCK_SIZE == 0) Destroy(dummySrite.gameObject);
                }
            }
            else if (direction == BlockDirection.UP)
            {
                float distance = targetPos.y - tempBlock._transform.localPosition.y;
                tempBlock.mainSprite.customFill.blockRatio = -1 + distance / ManagerBlock.BLOCK_SIZE ;

                if (dummySrite != null) { dummySrite.customFill.blockRatio = 1 - distance / ManagerBlock.BLOCK_SIZE; }
            }

            yield return null;
        }
        tempBlock.state = BlockState.WAIT;
        _waitCount--;
        yield return null;
    }

    const float MAX_VELOCITY = Mathf.PI * 0.5f;
    const float MOVE_SPPED = 500f;

    IEnumerator CoBeltAction()
    {
        float _velocity = 0;
        while (true)
        {
            _velocity += Global.deltaTimePuzzle * ManagerBlock.NORMAL_SPEED;
            _velocity = (_velocity > MAX_VELOCITY) ? MAX_VELOCITY : _velocity;

            if (Vector3.Distance(tempBlock._transform.localPosition, targetPos) < 1f)
            {
                tempBlock.indexX = targetBoard.indexX;
                tempBlock.indexY = targetBoard.indexY;

                targetBoard.Block = tempBlock;
                tempBlock.targetPos = targetPos;
                tempBlock._transform.localPosition = targetPos;                
                yield return null;
                break;
            }
            tempBlock._transform.localPosition = Vector3.MoveTowards(tempBlock._transform.localPosition, targetPos, Mathf.Sin(_velocity) * Global.deltaTimePuzzle * MOVE_SPPED);

            yield return null;
        }
        tempBlock.state = BlockState.WAIT;
        _waitCount--;
        yield return null;
    }
}
