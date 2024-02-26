using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Rainbow_Line_move : MonoBehaviour
{
    [System.NonSerialized]
    public Transform _transform;

    [System.NonSerialized]
    public Vector3 _startPos;
    [System.NonSerialized]
    public Vector3 _endPos;

    float timer = 0;
    public int type = 0;
    float speed = 0.8f;

    public BlockBase targetBlock = null;
    public int pangIndex = -1;

    public BlockBombType bombType = BlockBombType.NONE;
    
    void Awake()
    {
        _transform = transform;
    }

    IEnumerator Start()
    {
        _transform.localPosition = _startPos;

        float posX = _startPos.x;
        float posY = _startPos.y;

        while (true)
        {
            timer += Global.deltaTimePuzzle * speed;

            float ratioY = Mathf.Sin(ManagerBlock.PI90 * timer);

            if(type == 0)
            {
                posX = Mathf.Lerp(_transform.localPosition.x, _endPos.x, timer);
                posY = Mathf.Lerp(_transform.localPosition.y, _endPos.y, ratioY);
            }
            else
            {
                posX = Mathf.Lerp(_transform.localPosition.x, _endPos.x, ratioY);
                posY = Mathf.Lerp(_transform.localPosition.y, _endPos.y, timer);
            }

            //_transform.localPosition = Vector3.Lerp(_transform.localPosition, _endPos, timer);
            _transform.localPosition = new Vector3(posX, posY, 0);

            if (Vector3.Distance(_transform.localPosition, _endPos) < 2f)
            {
                if(targetBlock != null)
                {
                    if (bombType != BlockBombType.NONE)
                    {
                        targetBlock.bombType = bombType;
                        targetBlock.RemoveLinkerNoReset();
                    }

                    InGameEffectMaker.instance.MakeRainbowLight(targetBlock.gameObject, targetBlock.GetRainbowEffectSpriteName());
                    targetBlock.BlockPang(pangIndex, BlockColorType.NONE, true);
                    targetBlock.RemoveLinkerNoReset();

                }

                Destroy(gameObject);
            }
            yield return null;
        }
    }

}
