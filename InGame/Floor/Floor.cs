using System.Collections.Generic;
using UnityEngine;

public class Floor : UIWidget
{
    #region original_uisprite
    public enum Type
    {
        Simple,
        Sliced,
        Tiled,
        Filled,
        Advanced,
        Block,
    }

    [HideInInspector]
    [SerializeField]
    NGUIAtlas mAtlas;

    [HideInInspector]
    [SerializeField]
    string mSpriteName;

    [HideInInspector]
    [SerializeField]
    Type mType = Type.Simple;

    [HideInInspector]
    [SerializeField]
    bool mInvert = false;

    protected UISpriteData mSprite;
    protected Rect mInnerUV = new Rect();
    protected Rect mOuterUV = new Rect();

    bool mSpriteSet = false;

    static Vector2[] mTempPos = new Vector2[4];
    static Vector2[] mTempUVs = new Vector2[4];

    float offset = 0.5f / 1024f;

    void Awake()
    {   
        map_width = GameManager.MAX_X;
        map_height = GameManager.MAX_Y;

        width = (int)(map_width * ManagerBlock.BLOCK_SIZE);
        height = (int)(map_height * ManagerBlock.BLOCK_SIZE);

        offset = 0.5f / material.mainTexture.width;

        depth = 0;
    }

    public virtual Type type
    {
        get
        {
            return mType;
        }
        set
        {
            if (mType != value)
            {
                mType = value;
                MarkAsChanged();
            }
        }
    }

    public override Material material { get { return (mAtlas != null) ? mAtlas.spriteMaterial : null; } }

    public NGUIAtlas atlas
    {
        get
        {
            return mAtlas;
        }
        set
        {
            if (mAtlas != value)
            {
                RemoveFromPanel();

                mAtlas = value;
                mSpriteSet = false;
                mSprite = null;

                if (string.IsNullOrEmpty(mSpriteName))
                {
                    if (mAtlas != null && mAtlas.spriteList.Count > 0)
                    {
                        SetAtlasSprite(mAtlas.spriteList[0]);
                        mSpriteName = mSprite.name;
                    }
                }

                if (!string.IsNullOrEmpty(mSpriteName))
                {
                    string sprite = mSpriteName;
                    mSpriteName = "";
                    spriteName = sprite;
                    MarkAsChanged();
                }
            }
        }
    }


    public string spriteName
    {
        get
        {
            return mSpriteName;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(mSpriteName)) return;

                mSpriteName = "";
                mSprite = null;
                mChanged = true;
                mSpriteSet = false;
            }
            else if (mSpriteName != value)
            {
                mSpriteName = value;
                mSprite = null;
                mChanged = true;
                mSpriteSet = false;
            }
        }
    }

    public void InitFloorSprite()
    {
        //커스텀 배경 사용하는 이미지들에 커스텀 배경 설정
        NGUIAtlas targetAtlas = ManagerUIAtlas.CheckAndApplyIngameBGAtlas();
        if (targetAtlas != null)
            mAtlas = targetAtlas;
    }

    public UISpriteData GetAtlasSprite()
    {
        if (!mSpriteSet) mSprite = null;

        if (mSprite == null && mAtlas != null)
        {
            if (!string.IsNullOrEmpty(mSpriteName))
            {
                UISpriteData sp = mAtlas.GetSprite(mSpriteName);
                if (sp == null) return null;
                SetAtlasSprite(sp);
            }

            if (mSprite == null && mAtlas.spriteList.Count > 0)
            {
                UISpriteData sp = mAtlas.spriteList[0];
                if (sp == null) return null;
                SetAtlasSprite(sp);

                if (mSprite == null)
                {
                    return null;
                }
                mSpriteName = mSprite.name;
            }
        }
        return mSprite;
    }

    protected void SetAtlasSprite(UISpriteData sp)
    {
        mChanged = true;
        mSpriteSet = true;

        if (sp != null)
        {
            mSprite = sp;
            mSpriteName = mSprite.name;
        }
        else
        {
            mSpriteName = (mSprite != null) ? mSprite.name : "";
            mSprite = sp;
        }
    }

    /*
    public override Vector4 drawingDimensions
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = -offset.x * mWidth;
            float y0 = -offset.y * mHeight;
            float x1 = x0 + mWidth;
            float y1 = y0 + mHeight;

            if (GetAtlasSprite() != null)
            {
                int padLeft = mSprite.paddingLeft;
                int padBottom = mSprite.paddingBottom;
                int padRight = mSprite.paddingRight;
                int padTop = mSprite.paddingTop;

                int w = mSprite.width + padLeft + padRight;
                int h = mSprite.height + padBottom + padTop;

                if (w > 0 && h > 0)
                {
                    if ((w & 1) != 0) ++padRight;
                    if ((h & 1) != 0) ++padTop;

                    float px = (1f / w) * mWidth;
                    float py = (1f / h) * mHeight;

                    x0 += padLeft * px;
                    x1 -= padRight * px;
                    y0 += padBottom * py;
                    y1 -= padTop * py;
                }
                else
                {
                    x0 += padLeft;
                    x1 -= padRight;
                    y0 += padBottom;
                    y1 -= padTop;
                }
            }

            Vector4 br = border * atlas.pixelSize;

            float fw = br.x + br.z;
            float fh = br.y + br.w;

            float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
            float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
            float vz = Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
            float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

            return new Vector4(vx, vy, vz, vw);
        }
    }*/


    protected override void OnInit()
    {
        base.OnInit();
    }


    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (mChanged || !mSpriteSet)
        {
            mSpriteSet = true;
            mSprite = null;
            mChanged = true;
        }
    }


    #endregion

    float spriteWidth = ManagerBlock.BLOCK_SIZE;
    float spriteHeight = ManagerBlock.BLOCK_SIZE;
    float rate_x;
    float rate_y;

    //타일의 수.
    //int map_width = 9;
    //int map_height = 10;

    //int map_width = Global.blockMaxWCount;
    //int map_height = Global.blockMaxHCount;
    int map_width;
    int map_height;

    //int map_width = Global.blockMaxWCount + 2;
    //int map_height = Global.blockMaxHCount + 2;

    int max_map_size;

    public int MapWidth
    {
        set
        {
            map_width = value;
        }
        get
        {
            return map_width;
        }
    }
    public int MapHeight
    {
        set
        {
            map_height = value;
        }
        get
        {
            return map_height;
        }
    }
    public int MapSize
    {
        get
        {
            //return map_width * map_height;
            return GameManager.MAX_X * GameManager.MAX_Y;
        }
    }





    public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> colors)
    {
        Texture tex = mainTexture;

        for (int i = GameManager.MAX_X; i < MapSize; i++)
        {
            //스프라이트 이름을 가져 옴.
            mSprite = atlas.GetSprite(FloorManager.instance.array_tile[i]._sprite_name);
            if (mSprite == null)
                continue;

            //x축은 맞으나, y축이 문제.
            mOuterUV.Set(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
            mInnerUV.Set(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
                mSprite.width - mSprite.borderLeft - mSprite.borderRight,
                mSprite.height - mSprite.borderBottom - mSprite.borderTop);

            mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
            mInnerUV = NGUIMath.ConvertToTexCoords(mInnerUV, tex.width, tex.height);

            DrawFunc(verts, uvs, colors, i);
        }
    }

    void DrawFunc(List<Vector3> verts, List<Vector2> uvs, List<Color> cols, int index)
    {
        //x축. y축.
        int x = index % map_width;
        int y = (MapSize - index - 1) / map_width;      //왼쪽 위가 (0, 0) 일 경우.

        Texture tex = material.mainTexture;     //아틀라스의 메인 텍스쳐.Tile        
        if (tex == null) return;                //아틀라스가 없으면 종료.

        //아틀라스의 (시작지점 X, 시작지점 Y, 실제 width, 실제 hegiht) 값 가져옴.
        Vector4 dr = drawingDimensions;

        //이미지의 원래 사이즈
        int sizeX = (mSprite.width + mSprite.paddingLeft + mSprite.paddingRight);
        int sizeY = (mSprite.height + mSprite.paddingTop + mSprite.paddingBottom);
        Vector2 size = new Vector2(sizeX, sizeY);
        size *= atlas.pixelSize;

        Color colF = color;
        colF.a = finalAlpha;
        Color32 col = atlas.premultipliedAlpha ? NGUITools.ApplyPMA(colF) : colF;

        //시작 위치
        //스프라이트를 만들 때 좌측과 하단을 시작으로 이미지를 붙이기 때문에, 알파 영역까지 더하려면 좌측과 하단의 padding값을 더해줘야 함)
        float x0 = dr.x + mSprite.paddingLeft;
        float y0 = dr.y + mSprite.paddingBottom;

        //u0 : 0, v0 : 0,
        float u0 = mInnerUV.xMin;     //아틀라스 내 이미지 위치. 
        float v0 = mInnerUV.yMin;     //아틀라스 내 이미지 위치.

        x0 += size.x * x;
        y0 += size.y * y;

        float y1 = y0 + mSprite.height;
        float v1 = mInnerUV.yMax;
        float x1 = x0 + mSprite.width;
        float u1 = mInnerUV.xMax;

        verts.Add(new Vector3(x0, y0));
        verts.Add(new Vector3(x0, y1));
        verts.Add(new Vector3(x1, y1));
        verts.Add(new Vector3(x1, y0));

        u0 = TileUtility.uvCeil(u0);
        u1 = TileUtility.uvFloor(u1);
        v0 = TileUtility.uvCeil(v0);
        v1 = TileUtility.uvFloor(v1);

        uvs.Add(new Vector2(u0 + offset, v0 + offset));
        uvs.Add(new Vector2(u0 + offset, v1 - offset));
        uvs.Add(new Vector2(u1 - offset, v1 - offset));
        uvs.Add(new Vector2(u1 - offset, v0 + offset));

        cols.Add(col);
        cols.Add(col);
        cols.Add(col);
        cols.Add(col);
    }

    public void Change()
    {
        mChanged = true;
    }

}
