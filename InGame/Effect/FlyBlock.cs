using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBlock : MonoBehaviour
{
    public UISprite mainSprite;

    Vector3 StartPos;

    public void initBlock(Vector3 tempStartPos, string tempName = null)
    {
        StartPos = tempStartPos;
        //mainSprite.spriteName = tempName;
    }


    float speedRatio = -0.8f;
    float velocity = 0.08f;
    const float MAX_SPEED = 4f;
    float _timer = 0f;

    IEnumerator Start()
    {
        transform.position = StartPos;
        BlockBomb tempBomb = null;

        while (true)
        {
            _timer += Global.deltaTimePuzzle * 1.5f;

            if (Vector3.Distance(transform.position, Vector3.zero) < 0.001f)
            {
                    if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode == AdventureMode.ORIGIN)
                    {
                        //BlockMatchManager.instance.CheckMatchPangBlock(targetBlock, false);
                       tempBomb = BlockMaker.instance.MakeBombBlock(null, 0, 0, BlockBombType.ADVENTURE_BOMB, BlockColorType.D);
                    }                
                break;
            }

            speedRatio += velocity;
            speedRatio = speedRatio > MAX_SPEED ? MAX_SPEED : speedRatio;

            transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, speedRatio * Global.deltaTimePuzzle);

            if (speedRatio < 0f)
                transform.position += Vector3.right * 0.5f * speedRatio * Global.deltaTimePuzzle;

            yield return null;
        }

        while (tempBomb != null)
            yield return null;

        Destroy(gameObject);
    }
}
