using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_RainbowLine : MonoBehaviour
{
    [System.NonSerialized]
    public Transform _transform;
    [System.NonSerialized]
    public Vector3 _startPos;
    [System.NonSerialized]
    public Vector3 _endPos;

    float timer = 0;
    public int type = 0;
    public float speed =0.005f;

    public BlockBase targetBlock = null;
    public int pangIndex = -1;

    public UISprite _rainbowUI;
    public BlockBombType bombType = BlockBombType.NONE;

    public GameObject trailObj;
    public GameObject traiRoot;
    public AnimationCurve curveAni;
    public BlockBase tempBlock;

    public float waitTimer = 0;
    float waitPangTime = 0.3f;
    public int curveDir = 1;

    public bool HasCarpet = false;

    //인게임 아이템으로 블럭이 제거되는 경우에만
    public bool pangByIngameItem_RainbowBombHammer = false;

    void Awake()
    {
        _transform = transform;
    }

    IEnumerator Start()
    {     
        timer = 0;
        trailObj.SetActive(true);
        float heightY = trailObj.transform.localPosition.x;

        _startPos = trailObj.transform.localPosition;
        _endPos = new Vector3(-trailObj.transform.localPosition.x, trailObj.transform.localPosition.y, 0);

        yield return null;

        float ratioSpped = 0;

        while (timer < 1)
        {
            timer += Global.deltaTimePuzzle * speed *0.75f;
            if (timer > 1f) timer = 1;

            ratioSpped = curveAni.Evaluate(timer);          
            traiRoot.transform.localEulerAngles = new Vector3(0,0, 180* (ratioSpped));
            yield return null;
        }

        bool isCanPangBlock = false;
        Board tempBoard = null;

        if (tempBlock != null)
        {
            tempBlock.RemoveLinkerNoReset();

            if (HasCarpet)
            {
                tempBlock.CoverBlockWithCarpet();
            }

            tempBoard = PosHelper.GetBoard(tempBlock.indexX, tempBlock.indexY);

            //블럭팡이 가능한지 확인.
            if (tempBoard != null)
            {
                isCanPangBlock = (tempBoard.HasDecoHideBlock() == false && tempBoard.HasDecoCoverBlock() == false
                    && (tempBlock.blockDeco == null || tempBlock.blockDeco.IsInterruptBlockSelect() == false));
            }

            if (bombType != BlockBombType.NONE)
            {
                if (isCanPangBlock == true)
                {
                    ManagerBlock.instance.PangColorBlock(tempBlock.colorType, transform.position);

                    if (tempBlock is NormalBlock)
                    {
                        if (bombType == BlockBombType.LINE)
                        {
                            int randLineType = GameManager.instance.GetIngameRandom(0, 2);
                            tempBlock.bombType = randLineType == 0 ? BlockBombType.LINE_H : BlockBombType.LINE_V;
                        }
                        else
                        {
                            tempBlock.bombType = bombType;
                            if (pangByIngameItem_RainbowBombHammer == true) tempBlock.rainbowBombHammerUse = true;
                        }  
                    }                  
                }
                else
                {
                    InGameEffectMaker.instance.MakeRainbowTargetEffect(tempBlock._transform.position);
                    InGameEffectMaker.instance.MakeRainbowLight(tempBlock.gameObject, tempBlock.GetRainbowEffectSpriteName());
                }
            }
            else
            {
                InGameEffectMaker.instance.MakeRainbowTargetEffect(tempBlock._transform.position);
                InGameEffectMaker.instance.MakeRainbowLight(tempBlock.gameObject, tempBlock.GetRainbowEffectSpriteName());

                //점수추가
                InGameEffectMaker.instance.MakeScore(tempBlock._transform.position, 80);
                ManagerBlock.instance.AddScore(80);
            }
        }

        yield return null;

        timer = 0;
        while (timer < waitPangTime + waitTimer)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        if(tempBlock != null)
        {
            if (HasCarpet && tempBlock.hasCarpet == true)
            {
                tempBoard.MakeCarpet(0f);
            }

            tempBlock._pangRemoveDelay = 0.1f;
            tempBlock.IsSkipPang = false;
            tempBlock.isRainbowBomb = false;

            if (isCanPangBlock == true && tempBlock.lifeCount <= 1)
            {
                tempBlock.byRainbowBomb = (bombType == BlockBombType.NONE);
            }

            tempBlock.BlockPang(pangIndex, BlockColorType.NONE, bombType != BlockBombType.NONE);

            trailObj.SetActive(false);
        }


        Destroy(gameObject);
        yield return null;
    }

}
