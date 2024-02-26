using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LINKER_TYPE
{
    NORMAL,
    BOMB,
    BLACK,
}

public class BlockLinker : MonoBehaviour {

    public BlockBase blockA = null;
    public BlockBase blockB = null;

    public UISprite linkerSprite;

    public UISprite linkerSpriteFR;
    public UISprite linkerSpriteRE;


    public int linkerDirectionA = 0; // 1 up, 2 right, 3 down, 4 left
    public int linkerDirectionB = 0; // 1 up, 2 right, 3 down, 4 left

    public LINKER_TYPE linkerType = LINKER_TYPE.NORMAL;

    public void Setlinker(LINKER_TYPE Type)
    {
        linkerType = Type;
        linkerSprite.transform.localPosition = new Vector3(0, 0, 0);

        if (linkerDirectionA == 2 || linkerDirectionB == 2)
        {
            linkerSprite.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));

            if (linkerType == LINKER_TYPE.BOMB)
            {
                linkerSprite.enabled = true;

                linkerSpriteFR.enabled = false;
                linkerSpriteRE.enabled = false;


                linkerSprite.spriteName = "Linker_bomb";
                linkerSprite.depth = (int)GimmickDepth.BLOCK_BASE - 3;
                MakePixelPerfect(linkerSprite);
            }
            else if (linkerType == LINKER_TYPE.BLACK)
            {
                linkerSprite.enabled = true;

                linkerSpriteFR.enabled = false;
                linkerSpriteRE.enabled = false;

                linkerSprite.spriteName = "Linker_black";
                linkerSprite.depth = (int)GimmickDepth.BLOCK_BASE + 5;
                MakePixelPerfect(linkerSprite);
            }
            else
            {
                linkerSpriteFR.enabled = true;
                linkerSpriteRE.enabled = true;


                linkerSprite.enabled = false;

                string linkerName = "Linker_";

                linkerSpriteFR.spriteName = linkerName + blockA.GetColorTypeString() + "_Right_FR";
                linkerSpriteRE.spriteName = linkerName + blockA.GetColorTypeString() + "_Right";

                Board boardB = PosHelper.GetBoard(blockB.indexX, blockB.indexY, 0, 0);

                int maxDepth = Mathf.Max(blockA.mainSprite.depth, blockB.mainSprite.depth);
                int minDepth = Mathf.Min(blockA.mainSprite.depth, blockB.mainSprite.depth);

                if (maxDepth - minDepth > 10)
                {
                    linkerSpriteFR.depth = minDepth + 1;
                    linkerSpriteRE.depth = minDepth - 1;
                }
                else if (blockA is BlockDynamite)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }
                else if (blockB is BlockDynamite)
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else if (blockA is BlockFireWork && blockB is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth - 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 3;
                }
                else if (blockA is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else if (blockB is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }
                else if (boardB != null && boardB.HasDecoCoverBlock())
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }

                MakePixelPerfect(linkerSpriteFR);
                MakePixelPerfect(linkerSpriteRE);
            }
        }
        else if (linkerDirectionA == 3 || linkerDirectionB == 3)
        {
            if (linkerType == LINKER_TYPE.BOMB)
            {
                linkerSprite.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                linkerSprite.enabled = true;

                linkerSpriteFR.enabled = false;
                linkerSpriteRE.enabled = false;

                linkerSprite.spriteName = "Linker_bomb";
                linkerSprite.depth = (int)GimmickDepth.BLOCK_BASE - 3;
                MakePixelPerfect(linkerSprite);
            }
            else if (linkerType == LINKER_TYPE.BLACK)
            {
                linkerSprite.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                linkerSprite.transform.localPosition = new Vector3(0, -4, 0);
                linkerSprite.enabled = true;

                linkerSpriteFR.enabled = false;
                linkerSpriteRE.enabled = false;

                linkerSprite.spriteName = "Linker_black";
                linkerSprite.depth = (int)GimmickDepth.BLOCK_BASE + 5;
                MakePixelPerfect(linkerSprite);
            }
            else
            {
                linkerSprite.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                linkerSprite.enabled = false;

                string linkerName = "Linker_";

                linkerSpriteFR.enabled = true;
                linkerSpriteRE.enabled = true;

                linkerSpriteFR.spriteName = linkerName + blockA.GetColorTypeString() + "_Down_FR";
                linkerSpriteRE.spriteName = linkerName + blockA.GetColorTypeString() + "_Down";

                Board boardB = PosHelper.GetBoard(blockB.indexX, blockB.indexY, 0, 0);

                int maxDepth = Mathf.Max(blockA.mainSprite.depth, blockB.mainSprite.depth);
                int minDepth = Mathf.Min(blockA.mainSprite.depth, blockB.mainSprite.depth);

                if (maxDepth - minDepth > 10)
                {
                    linkerSpriteFR.depth = minDepth + 1;
                    linkerSpriteRE.depth = minDepth - 1;
                }
                else if (blockA is BlockDynamite)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }
                else if (blockB is BlockDynamite)
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else if (blockA is BlockFireWork && blockB is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth - 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 3;
                }
                else if (blockA is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else if (blockB is BlockFireWork)
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }
                else if (boardB != null && boardB.HasDecoCoverBlock())
                {
                    linkerSpriteFR.depth = blockB.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockB.mainSprite.depth - 1;
                }
                else
                {
                    linkerSpriteFR.depth = blockA.mainSprite.depth + 1;
                    linkerSpriteRE.depth = blockA.mainSprite.depth - 1;
                }

                MakePixelPerfect(linkerSpriteFR);
                MakePixelPerfect(linkerSpriteRE);
            }
        }

        if (blockA != null && blockB != null)
        {
            Vector3 dir = (blockA._transform.localPosition - blockB._transform.localPosition).normalized;
            transform.localPosition = (blockA._transform.localPosition + blockB._transform.localPosition) * 0.5f;
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
        }
    }

    
    public void SetSpriteAlpha(float alphaColor)
    {
        if (linkerSprite != null) linkerSprite.color = new Color(linkerSprite.color.r, linkerSprite.color.g, linkerSprite.color.b, alphaColor);
        if (linkerSpriteFR != null) linkerSpriteFR.color = new Color(linkerSpriteFR.color.r, linkerSpriteFR.color.g, linkerSpriteFR.color.b, alphaColor);
        if (linkerSpriteRE != null) linkerSpriteRE.color = new Color(linkerSpriteRE.color.r, linkerSpriteRE.color.g, linkerSpriteRE.color.b, alphaColor);
    }

    public void SetSpriteColor(Color _color)
    {
        if (linkerSprite != null) linkerSprite.color = _color;
        if (linkerSpriteFR != null) linkerSpriteFR.color = _color;
        if (linkerSpriteRE != null) linkerSpriteRE.color = _color;
    }

    public void SetSpriteColor_Float(float _color)
    {
        Color color = new Color(_color, _color, _color);
        if (linkerSprite != null) linkerSprite.color = color;
        if (linkerSpriteFR != null) linkerSpriteFR.color = color;
        if (linkerSpriteRE != null) linkerSpriteRE.color = color;
    }


    [SerializeField]
    private bool destroyLinker = false;
    private float destroyTimer = 0;

    const float LINKER_DESTROY_TIME = 8;

    void Update()
    {
         if (destroyLinker)
        {
            destroyTimer += Global.deltaTimePuzzle * LINKER_DESTROY_TIME;
            {
                if (linkerSpriteFR != null) linkerSpriteFR.color = new Color(1, 1, 1, 1 - destroyTimer*1.5f);
                if (linkerSpriteRE != null) linkerSpriteRE.color = new Color(1, 1, 1, 1 - destroyTimer*1.5f);

                linkerSpriteFR.depth = (int)GimmickDepth.BLOCK_BASE + 5;
                linkerSpriteRE.depth = (int)GimmickDepth.BLOCK_BASE + 5;
            }

            if (destroyTimer > 1)
            {
                DestroyLinker();
            }
        }
        else
        {
            if (blockA == null || blockB == null)
            {
                destroyLinker = true;
                destroyTimer = 0;
                return;
            }

            if (Vector3.Distance(blockA._transform.localPosition, blockB._transform.localPosition) > 78 * ManagerBlock.linkerDistanceRatio)
            {
                destroyLinker = true;
                destroyTimer = 0;
                return;
            }
        }

        if (blockA != null && blockB != null)
        {
            Vector3 dir = (blockA._transform.localPosition - blockB._transform.localPosition).normalized;
            transform.localPosition = (blockA._transform.localPosition + blockB._transform.localPosition) * 0.5f;
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            float distanceRatio = Vector3.Distance(blockA._transform.localPosition, blockB._transform.localPosition) / 78f;
            transform.localScale = new Vector3(1, distanceRatio, 1);


            if ((blockA != null && blockA.IsBombBlock()) || (blockB != null && blockB.IsBombBlock()))
            {
                float ratio = Mathf.Sin(ManagerBlock.instance.BlockTime * 10);
                transform.localScale = new Vector3(1 + 0.2f * ratio, (distanceRatio - 0.2f), 1);

                linkerSprite.transform.localPosition =new Vector3(0, 0, 0);
            }
        }

    }

    public void DestroyLinker()
    {
        if (GameManager.instance.state == GameState.GAMECLEAR ||
            GameManager.instance.state == GameState.GAMEOVER ||
            GameManager.instance.state == GameState.EDIT)
        {
            Destroy(this);
            Destroy(gameObject);
            return;
        }

        if (blockA != null)
        {
            blockA.Removelinker(this);
           
            //if (blockA.state == BlockState.WAIT) 
            BlockMatchManager.instance.CheckBlockLinkToItem(blockA);
            //blockA.expectType = BlockBombType.NONE;
        }
        if (blockB != null)
        {
            blockB.Removelinker(this);
      
           //blockB.expectType = BlockBombType.NONE;
           // if (blockB.state == BlockState.WAIT) 
           BlockMatchManager.instance.CheckBlockLinkToItem(blockB);
        }

        Destroy(gameObject);
    }


    public void DestroyLinkerNoReset()
    {
        if (blockA != null)
        {
            blockA.Removelinker(this);
        }
        if (blockB != null)
        {
            blockB.Removelinker(this);
        }

        Destroy(gameObject);
    }


    bool onLavaWarring = false;
    IEnumerator CoWarring()
    {
        while (onLavaWarring)
        {
            linkerSprite.color = new Color(0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 1); //linkerSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);
            linkerSpriteFR.color = new Color(0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 1); //linkerSpriteA.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);
            linkerSpriteRE.color = new Color(0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 0.75f + Mathf.Sin(Time.time * 5f) * 0.25f, 1); //linkerSpriteB.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);
            yield return null;
        }
        linkerSprite.color = Color.white;
        linkerSpriteFR.color = Color.white;
        linkerSpriteRE.color = Color.white;
        yield return null;
    }

    public void OnLavaWarring(bool onWarring = true)
    {
        if (!onLavaWarring && onWarring)
        {
            onLavaWarring = onWarring;
            StartCoroutine(CoWarring());
        }
        onLavaWarring = onWarring;
    }

    private void MakePixelPerfect(UISprite sprite)
    {
        sprite.MakePixelPerfect();

        sprite.width = Mathf.RoundToInt(sprite.width * 1.25f);
        sprite.height = Mathf.RoundToInt(sprite.height * 1.25f);
    }
}
