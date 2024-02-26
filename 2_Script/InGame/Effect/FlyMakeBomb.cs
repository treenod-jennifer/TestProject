using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMakeBomb : MonoBehaviour 
{
    [System.NonSerialized]
    public BlockBombType changeBlockType = BlockBombType.NONE;
    [System.NonSerialized]
    public BlockBase targetBlock = null;
    public UISprite mainSprite;
    [System.NonSerialized]
    public Vector3 StartPos;

    public void initBlock(BlockBombType tempType, BlockBase tempTargetBlock, Vector3 tempStartPos)
    {
        string ColorName = "";
        switch (tempTargetBlock.colorType)
        {
            case BlockColorType.A:
                ColorName = "Rabbit";
                break;
            case BlockColorType.B:
                ColorName = "Bear";
                break;
            case BlockColorType.C:
                ColorName = "Kangaroo";
                break;
            case BlockColorType.D:
                ColorName = "Tiger";
                break;
            case BlockColorType.E:
                ColorName = "Wolf";
                break;
            case BlockColorType.F:
                ColorName = "Wildpig";
                break;
        }
        switch (tempType)
        {
            case BlockBombType.RAINBOW:
                mainSprite.spriteName = "ToyRainbow_" + ColorName;
                break;
            case BlockBombType.BOMB:
            case BlockBombType.HALF_BOMB:
                mainSprite.spriteName = "Toy_bomb";
                break;
            case BlockBombType.LINE_H:
                mainSprite.spriteName = "Toy_lineH";
                break;
            case BlockBombType.LINE_V:
                mainSprite.spriteName = "Toy_lineV";
                break;
        }

        mainSprite.depth = (int)GimmickDepth.FX_FLYEFFECT;
        mainSprite.MakePixelPerfect();

        changeBlockType = tempType;
        targetBlock = tempTargetBlock;
        transform.position = tempStartPos;
        StartPos = tempStartPos;
        mainSprite.MakePixelPerfect();
    }



    float speedRatio = -0.8f;
    float velocity = 0.08f;
    const float MAX_SPEED = 4f;
    float _timer = 0f;

    IEnumerator Start()
    {
        transform.position = StartPos;

        while (true)
        {
            _timer += Global.deltaTimePuzzle * 1.5f;

            if (targetBlock == null)// || changeBlockType == BlockBombType.NONE || targetBlock.bombType != BlockBombType.NONE)
            {
                //ManagerBlock.instance.ResetMakeRandomBombBlock();
                break; 
            }

            if (Vector3.Distance(transform.position, targetBlock.transform.position) < 0.001f)
            {
                if (targetBlock != null && ManagerBlock.instance.state != BlockManagrState.STOP)
                {
                    targetBlock.bombType = changeBlockType;
                    //if (randomLine == 0) lineBlock.bombType = BlockBombType.LINE_V;
                    //else lineBlock.bombType = BlockBombType.LINE_H;
                    // else if (UIPopupReady.readyItemUseCount[4].Value == 1) lineBlock.bombType = BlockBombType.BOMB;
                    // else if (UIPopupReady.readyItemUseCount[5].Value == 1) lineBlock.bombType = BlockBombType.RAINBOW;
                    ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
                    targetBlock.JumpBlock();
                    targetBlock.Destroylinker();
                    targetBlock.IsSkipPang = false;
                    targetBlock.isSkipDistroy = false;
                }

                break;
            }

            speedRatio += velocity;
            speedRatio = speedRatio > MAX_SPEED ? MAX_SPEED : speedRatio;

            transform.position = Vector3.MoveTowards(transform.position, targetBlock.transform.position, speedRatio * Global.deltaTimePuzzle);

            if (speedRatio < 0f)
                transform.position += Vector3.right * 0.5f * speedRatio * Global.deltaTimePuzzle;

            yield return null;
        }

        Destroy(gameObject);
    }

    /*
    IEnumerator Start()
    {
        float flyTimer = 0f;
        while (flyTimer< 1)
        {
            if (targetBlock == null || changeBlockType == BlockBombType.NONE || targetBlock.bombType != BlockBombType.NONE)
                break;

            flyTimer += Global.deltaTimePuzzle*1.5f;

            float ratioY = ManagerBlock.instance._curveRankFly.Evaluate(flyTimer);
            float posX = Mathf.Lerp(StartPos.x, targetBlock.transform.position.x, flyTimer);
            float posY = Mathf.Lerp(StartPos.y, targetBlock.transform.position.y, flyTimer);// StartPos.y + (targetBlock.transform.position.y - StartPos.y) * ratioY;//2 * flyTimer - flyTimer * flyTimer);

            transform.position = new Vector3(posX, posY, 0);
            yield return null;
        }
        Destroy(gameObject);
        yield return null;
    }
    */
    /*
    void Update()
    {
        if (targetBlock != null)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetBlock.transform.localPosition, Global.deltaTimePuzzle * 900f);

            if (Vector3.Distance(targetBlock.transform.localPosition, transform.localPosition) < 2)
            {
                if (changeBlockType != BlockBombType.NONE)
                {
                    targetBlock.bombType = changeBlockType; 
                }
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
     */
}
