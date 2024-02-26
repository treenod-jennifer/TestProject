using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameInfo
{
    public Texture tex;
    public float interval;
}

public class APNGInfo : IEnumerable<FrameInfo>
{
    private FrameInfo[] frameInfos;

    public int Length { get { return frameInfos.Length; } }

    public bool IsSimplePNG { get { return frameInfos.Length == 1; } }

    public APNGInfo(FrameInfo[] frameInfos)
    {
        this.frameInfos = frameInfos;
    }

    public FrameInfo this[int i]
    {
        get
        {
            if(i < frameInfos.Length)
            {
                return frameInfos[i];
            }
            else
            {
                return null;
            }
        }
    }

    public void UnLoad()
    {
        foreach(var frame in frameInfos)
        {
            UnityEngine.Object.Destroy(frame.tex);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return frameInfos.GetEnumerator();
    }

    public IEnumerator<FrameInfo> GetEnumerator()
    {
        return frameInfos.GetEnumerator() as IEnumerator<FrameInfo>;
    }
}


public static class TextureUtility
{
    public static int GetHeight(this Texture tData)
    {
        Texture2D texture2D = tData as Texture2D;

        Color[] colorData = texture2D.GetPixels();

        for (int y = texture2D.height - 1; y >= 0; y--)
        {
            bool isAlpha = true;
            for (int x = 0; x < texture2D.width; x++)
            {
                if (colorData[x + y * texture2D.width].a != 0)
                {
                    isAlpha = false;
                    break;
                }
            }

            if (!isAlpha)
                return y;
        }

        return 0;
        //Sprite sp = Sprite.Create(temp, new Rect(Vector2.zero, Vector2.one * 255), Vector2.zero);
        //Debug.Log(sp.textureRect);
    }

    public static int GetGround(this Texture tData)
    {
        Texture2D texture2D = tData as Texture2D;

        Color[] colorData = texture2D.GetPixels();

        for (int y = 0; y < texture2D.height; y++)
        {
            bool isAlpha = true;
            for (int x = 0; x < texture2D.width; x++)
            {
                if (colorData[x + y * texture2D.width].a != 0)
                {
                    isAlpha = false;
                    break;
                }
            }

            if (!isAlpha)
                return y;
        }

        return 0;
    }

    public static int GetCenter(this Texture tData)
    {
        int height = GetHeight(tData);
        int ground = GetGround(tData);

        return Mathf.RoundToInt((height + ground) * 0.5f);
    }

    //public static float GetHeight(this GameObject tData)
    //{
    //    var tt = tData.GetComponent<Renderer>();

    //    return tt.bounds.size.y;
    //}

    public static int GetLeft(this Texture tData)
    {
        Texture2D texture2D = tData as Texture2D;

        Color[] colorData = texture2D.GetPixels();

        for (int x = 0; x < texture2D.width; x++)
        {
            bool isAlpha = true;
            for (int y = texture2D.height - 1; y >= 0; y--)
            {
                if (colorData[x + y * texture2D.width].a != 0)
                {
                    isAlpha = false;
                    break;
                }
            }

            if (!isAlpha)
                return x;
        }

        return 0;
    }

    public static int GetRight(this Texture tData)
    {
        Texture2D texture2D = tData as Texture2D;

        Color[] colorData = texture2D.GetPixels();

        for (int x = texture2D.width - 1; x >= 0; x--)
        {
            bool isAlpha = true;
            for (int y = texture2D.height - 1; y >= 0; y--)
            {
                if (colorData[x + y * texture2D.width].a != 0)
                {
                    isAlpha = false;
                    break;
                }
            }

            if (!isAlpha)
                return x;
        }

        return 0;
    }

    public static int GetWidth(this Texture tData)
    {
        int left = GetLeft(tData);
        int right = GetRight(tData);

        return right - left;
    }

    // 외각선 필터
    public static Texture2D GetAlphaEdgeTexture(this Texture2D tex)
    {
        int[][] sobelx = {new int[] {-1, 0, 1},
                          new int[] {-2, 0, 2},
                          new int[] {-1, 0, 1}};

        int[][] sobely = {new int[] {-1, -2, -1},
                          new int[] { 0, 0, 0},
                          new int[] { 1, 2, 1}};

        var ret = new Texture2D(tex.width, tex.height, tex.format, false);

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                if (i < 1 || i >= tex.width - 1 ||
                     j < 1 || j >= tex.height - 1)
                {
                    ret.SetPixel(i, j, new Color(0f, 0f, 0f, 0f));
                    continue;
                }

                float dx = 0;
                float dy = 0;
                for (int k = 0; k < 3; ++k)
                {
                    for (int l = 0; l < 3; ++l)
                    {
                        var pixel = tex.GetPixel(i + k - 1, j + l - 1);
                        dx += pixel.a * sobelx[k][l];
                        dy += pixel.a * sobely[k][l];
                    }
                }

                float color = Mathf.Sqrt((dx * dx) + (dy * dy)) > 0.5f ? 1.0f : 0f;

                ret.SetPixel(i, j, new Color(color, color, color, color > 0.5f ? 1.0f : 0f));
            }
        }
        ret.Apply();

        return ret;
    }

    // 텍스쳐 축소. 축소필터 적용해야되나 싶은데...
    public static Texture2D GetDownscaleTexture(this Texture2D tex, float ratio)
    {
        int width = (int)(ratio * tex.width);
        int height = (int)(ratio * tex.height);

        var ret = new Texture2D(width, height, tex.format, false);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int x = (int)(i * 1f / ratio);
                int y = (int)(j * 1f / ratio);
                Color pixel = new Color(0f, 0f, 0f, 0f);

                int pixelCount = 0;
                for (int k = -1; k < 2; ++k)
                {
                    for (int l = -1; l < 2; ++l)
                    {
                        if (i + k < 0 || j + l < 0 || i + k > tex.width || j + l > tex.height)
                            continue;
                        pixel += tex.GetPixel(x + k, y + l);
                        pixelCount++;
                    }
                }

                if (pixelCount > 0)
                {
                    pixel /= (float)pixelCount;
                    ret.SetPixel(i, j, pixel);
                }
            }
        }
        ret.Apply();

        return ret;
    }

    // 블러필터
    // 근데 이거 샘플레인지 5~6 넘어가면 눈에띄게 느려서 알고리즘 교체해야될거같음
    public static Texture2D Blur(this Texture2D tex, int sampleRangeX, int sampleRangeY)
    {
        int width = tex.width;
        int height = tex.height;

        var ret = new Texture2D(width, height, tex.format, false);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = new Color(0f, 0f, 0f, 0f);

                float alphaSum = 0f;
                int pixelCnt = 0;
                for (int k = -sampleRangeX; k < sampleRangeX + 1; ++k)
                {
                    for (int l = -sampleRangeY; l < sampleRangeY + 1; ++l)
                    {
                        if (i + k < 0 || j + l < 0 || i + k > tex.width || j + l > tex.height)
                            continue;
                        var p = tex.GetPixel(i + k, j + l);
                        float a = p.a;
                        p = p * p.a;
                        p.a = a;
                        pixel += p;
                        pixelCnt++;
                        alphaSum += a;
                    }
                }

                // 알파하고 칼라채널 처리를 따로해줘야 이미지 모퉁이가 시커매지지 않음
                if (pixelCnt > 0 && alphaSum > 0f)
                {
                    pixel.a /= (float)pixelCnt;
                    pixel.r /= alphaSum;
                    pixel.g /= alphaSum;
                    pixel.b /= alphaSum;
                    ret.SetPixel(i, j, pixel);
                }
                else
                {
                    ret.SetPixel(i, j, pixel);
                }
            }
        }
        ret.Apply();

        return ret;
    }

    // 실루엣 텍스쳐 만들기
    public static Texture2D GetShadowTexture(this Texture2D tex)
    {
        int width = tex.width;
        int height = tex.height;

        var ret = new Texture2D(width, height, tex.format, false);

        var transBlack = new Color(0f, 0f, 0f, 0f);
        var pixels = tex.GetPixels32();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                if (pixels[i + j * width].a > 0f)
                {
                    ret.SetPixel(i, j, Color.white);
                }
                else
                    ret.SetPixel(i, j, transBlack);
            }
        }
        ret.Apply();

        return ret;
    }

    public static Texture2D GetGrayscale(this Texture2D tex)
    {
        int width = tex.width;
        int height = tex.height;

        var ret = new Texture2D(width, height, tex.format, false);

        var transBlack = new Color(0f, 0f, 0f, 0f);
        var pixels = tex.GetPixels();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // 사람의 색 민감도에 따른 색상가중치 들어간 그레이스케일
                float colorVal = pixels[i + j * width].r * 0.2126f + pixels[i + j * width].g * 0.7152f + pixels[i + j * width].b * 0.0722f;
                var pixel = new Color(colorVal, colorVal, colorVal, pixels[i + j * width].a);

                ret.SetPixel(i, j, pixel);

            }
        }
        ret.Apply();

        return ret;
    }

    // 크롭은 top-bottom 구하는 부분을 새로 짜야될듯
    public static Texture2D Crop(this Texture2D tex)
    {
        var pixels = tex.GetPixels32();

        int right = tex.GetRight();
        int left = tex.GetLeft();

        int top = 0;
        {
            int yCursor = tex.height;
            while(yCursor --> 0)
            {
                bool isAlpha = true;
                for (int x = 0; x < tex.width; x++)
                {
                    if (pixels[x + yCursor * tex.width].a != 0)
                    {
                        isAlpha = false;
                        break;
                    }
                }

                if (!isAlpha)
                    top = yCursor;
            }

        }

        int bottom = 0;
        {
            for (int y = 0; y < tex.height; y++)
            {
                bool isAlpha = true;
                for (int x = 0; x < tex.width; x++)
                {
                    if (pixels[x + y * tex.width].a != 0)
                    {
                        isAlpha = false;
                        break;
                    }
                }

                if (!isAlpha)
                    bottom = y;
            }

        }

        int width = right - left;
        int height = bottom - top;

        var ret = new Texture2D(width, height, tex.format, false);

        var transBlack = new Color(0f, 0f, 0f, 0f);


        {
            int x = 0;
            int y = 0;
            for (int i = left; i < right; i++)
            {
                y = 0;
                for (int j = top; j < bottom; j++)
                {
                    ret.SetPixel(x, y, pixels[i + tex.width * j]);
                    y++;
                }
                x++;
            }
        }
        
        ret.Apply();

        return ret;
    }

    //static public Texture2D GetReadable(this Texture2D tex)
    //{
    //    RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
    //    Graphics.Blit(tex, tmp);
    //    RenderTexture previous = RenderTexture.active;
    //    RenderTexture.active = tmp;
    //    Texture2D myTexture2D = new Texture2D(tex.width, tex.height);
    //    myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
    //    myTexture2D.Apply();
    //    RenderTexture.active = previous;
    //    RenderTexture.ReleaseTemporary(tmp);

    //    return myTexture2D;
    //}

    static public int CopyFromTexture( this Texture2D dest, Color32[] fromData, int fromWidth, int startX, int endX, int startY, int endY)
    {
        bool wrapTop= true;
        bool wrapBottom = true;
        bool wrapLeft = true;
        bool wrapRight = true;


        Color32[] pixels = new Color32[dest.width * dest.height];
        for(int i = startX; i < endX; ++i)
        {
            for(int j = startY; j < endY; ++j)
            {
                int x = i - startX;
                int y = j - startY;
                var destIdx = x + y * dest.width;
                var srcIdx = i + fromWidth * j;

                try
                {
                    pixels[destIdx] = fromData[srcIdx];

                    if (i == startX && fromData[srcIdx].a == 0)
                        wrapLeft = false;
                    if (i == endX - 1 && fromData[srcIdx].a == 0)
                        wrapRight = false;
                    if (j == startY && fromData[srcIdx].a == 0)
                        wrapTop = false;
                    if (j == endY - 1 && fromData[srcIdx].a == 0)
                        wrapBottom = false;
                }
                catch(System.Exception e)
                {
                    Debug.Log("Error");
                }
                
            }
        }

        int ret = (wrapTop ? (1 << 1) : 0) | (wrapBottom ? 1 << 2 : 0) | (wrapLeft ? 1 << 3 : 0) | (wrapRight ? 1 << 4 : 0);

        dest.SetPixels32(pixels);
        dest.Apply();

        return ret;
    }

    static public Texture2D WrapTexture(this Texture2D org, int wrapSize)
    {
        int wrappedWidth = (org.width + wrapSize * 2);
        int wrappedHeight = (org.height + wrapSize * 2);
        var newTex = new Texture2D(wrappedWidth, wrappedHeight, org.format, false);
        Color32[] pixels = new Color32[wrappedHeight * wrappedWidth];
        var orgPixels = org.GetPixels32();

        for (int x = 0; x < wrappedWidth; x++)
        {
            for(int y = 0; y < wrappedHeight; ++y)
            {
                int sampleX = x - wrapSize;
                int sampleY = y - wrapSize;

                if (sampleX < 0)
                    sampleX = 0;
                else if (sampleX >= org.width)
                    sampleX = org.width - 1;

                if (sampleY < 0)
                    sampleY = 0;
                else if (sampleY >= org.height)
                    sampleY = org.height - 1;

                pixels[x + wrappedWidth * y] = orgPixels[sampleX + sampleY * org.width];
            }
        }
        newTex.SetPixels32(pixels);
        newTex.Apply();

        return newTex;
    }

    static public void CopyFromTexture(this Texture2D dest, Texture2D fromTex, int startX, int endX, int startY, int endY)
    {
        var orgAtlasPixels = fromTex.GetPixels32();
        dest.CopyFromTexture(orgAtlasPixels, fromTex.width, startX, endX, startY, endY);
    }

    static public APNGInfo ParseAPNGFrames(LibAPNG.APNG apng)
    {
        if (apng == null )  // apng가 아니라 일반 png인지 먼저 판정
        {
            return null;
        }

        //이미지 생성시에만 사용하고 생성 후 삭제할 텍스쳐 리스트
        List<Texture2D> listTempTextures = new List<Texture2D>();

        int width = (int)apng.DefaultImage.fcTLChunk.Width;
        int height = (int)apng.DefaultImage.fcTLChunk.Height;

        FrameInfo[] frameSequence = new FrameInfo[apng.Frames.Length];
        
        Texture2D mainFrame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mainFrame.LoadImage(apng.DefaultImage.GetStream().ToArray());
        listTempTextures.Add(mainFrame);

        var mainFramePixels = mainFrame.GetPixels32();

        List<Texture2D> rawTextures = new List<Texture2D>();
        for (int i = 0; i < apng.Frames.Length; ++i)
        {
            var frame = apng.Frames[i];
            Texture2D tex = new Texture2D((int)frame.fcTLChunk.Width, (int)frame.fcTLChunk.Height);
            tex.LoadImage(frame.GetStream().ToArray());
            rawTextures.Add(tex);
            listTempTextures.Add(tex);
        }

        Color32[] blackPixels = new Color32[width * height];
        for (int i = 0; i < width * height; ++i)
        {
            blackPixels[i] = new Color32(0, 0, 0, 0);
        }

        for (int i = 0; i < apng.Frames.Length; ++i)
        {
            var frame = apng.Frames[i];
            Texture2D frameTex = new Texture2D(width, height, TextureFormat.RGBA32, false);            

            // 작은 프레임 때문에 기본색 나오는거 제거
            if (width != frame.fcTLChunk.Width || height != frame.fcTLChunk.Height)
            {
                frameTex.SetPixels32(mainFramePixels);
            }

            if (frame.fcTLChunk.BlendOp == LibAPNG.BlendOps.APNGBlendOpOver && i > 0)
            {
                Texture2D tmpTexture = new Texture2D(width, height);
                listTempTextures.Add(tmpTexture);

                int prevFrameIdx = (i - 1) % apng.Frames.Length;
                if (apng.Frames[prevFrameIdx].fcTLChunk.DisposeOp == LibAPNG.DisposeOps.APNGDisposeOpNone)
                {
                    if (width != frame.fcTLChunk.Width || height != frame.fcTLChunk.Height)
                    {
                        tmpTexture.SetPixels32(mainFramePixels);
                    }

                    int yOffset = (int)(height - (apng.Frames[prevFrameIdx].fcTLChunk.Height + apng.Frames[prevFrameIdx].fcTLChunk.YOffset));
                    var pixels = rawTextures[prevFrameIdx].GetPixels32();
                    tmpTexture.SetPixels32((int)apng.Frames[prevFrameIdx].fcTLChunk.XOffset, yOffset, (int)apng.Frames[prevFrameIdx].fcTLChunk.Width, (int)apng.Frames[prevFrameIdx].fcTLChunk.Height, pixels);
                    tmpTexture.Apply();
                }
                else if (apng.Frames[prevFrameIdx].fcTLChunk.DisposeOp == LibAPNG.DisposeOps.APNGDisposeOpBackground)
                {
                    tmpTexture.SetPixels32(blackPixels);
                }
                else if (apng.Frames[prevFrameIdx].fcTLChunk.DisposeOp == LibAPNG.DisposeOps.APNGDisposeOpPrevious)
                {
                    if (i > 1)
                    {
                        int ppFrmIdx = (i - 2) % apng.Frames.Length;

                        if (width != frame.fcTLChunk.Width || height != frame.fcTLChunk.Height)
                        {
                            tmpTexture.SetPixels32(mainFramePixels);
                            tmpTexture.Apply();
                        }

                        int yOffset = (int)(height - (apng.Frames[ppFrmIdx].fcTLChunk.Height + apng.Frames[ppFrmIdx].fcTLChunk.YOffset));
                        var pixels = rawTextures[ppFrmIdx].GetPixels32();
                        tmpTexture.SetPixels32((int)apng.Frames[ppFrmIdx].fcTLChunk.XOffset, yOffset, (int)apng.Frames[ppFrmIdx].fcTLChunk.Width, (int)apng.Frames[ppFrmIdx].fcTLChunk.Height, pixels);
                        tmpTexture.Apply();
                    }
                    else
                    {
                        tmpTexture.SetPixels32(mainFramePixels);
                    }
                }
                {
                    int yOffset = (int)(height - (frame.fcTLChunk.Height + frame.fcTLChunk.YOffset));
                    var pixels = rawTextures[i].GetPixels32();
                    frameTex.SetPixels32((int)frame.fcTLChunk.XOffset, yOffset, (int)frame.fcTLChunk.Width, (int)frame.fcTLChunk.Height, pixels);
                    frameTex.Apply();
                }


                if (tmpTexture != null)
                {
                    var thisFramePixels = frameTex.GetPixels32();
                    var tmpPixels = tmpTexture.GetPixels32();
                    Color32[] newPixels = new Color32[width * height];

                    for (int p = 0; p < thisFramePixels.Length; ++p)
                    {
                        Color c = new Color(thisFramePixels[p].r / 255f, thisFramePixels[p].g / 255f, thisFramePixels[p].b / 255f, thisFramePixels[p].a / 255f);
                        Color c2 = new Color(tmpPixels[p].r / 255f, tmpPixels[p].g / 255f, tmpPixels[p].b / 255f, tmpPixels[p].a / 255f);
                        Color32 newPixel = c * c.a + (1 - c.a) * c2;
                        newPixels[p] = newPixel;
                    }

                    frameTex.SetPixels32(newPixels);
                    frameTex.Apply();
                }
            }
            else
            {
                int yOffset = (int)(height - (frame.fcTLChunk.Height + frame.fcTLChunk.YOffset));

                var pixels = rawTextures[i].GetPixels32();
                frameTex.SetPixels32((int)frame.fcTLChunk.XOffset, yOffset, (int)frame.fcTLChunk.Width, (int)frame.fcTLChunk.Height, pixels);
                frameTex.Apply();
            }

            frameTex.wrapMode = TextureWrapMode.Clamp;

            var newFrame = new FrameInfo()
            {
                tex = frameTex,
                interval = frame.fcTLChunk.DelayNum / (float)frame.fcTLChunk.DelayDen
            };

            frameSequence[i] = newFrame;
        }

        //apng 이미지 로드 시 생성한 텍스쳐 제거.
        foreach (var texture in listTempTextures)
        {
            Object.DestroyImmediate(texture, true);
        }

        return new APNGInfo(frameSequence);
    }

    static public void CombineAlphaChannel(this Texture2D dest, Texture2D srcAlphaTex)
    {
        int destWidth = dest.GetWidth();
        int destHeight = dest.GetHeight();
        int srcWidth = srcAlphaTex.GetWidth();
        int srcHeight = srcAlphaTex.GetHeight();

        float xRatio = (float)srcWidth / (float)destWidth;
        float yRatio = (float)srcHeight / (float)destHeight;


        var destPixels = dest.GetPixels32();
        var srcPixels = srcAlphaTex.GetPixels32();
        Color32[] pixels = new Color32[dest.width * dest.height];
        for (int i = 0; i < destWidth; ++i)
        {
            for (int j = 0; j < destHeight; ++j)
            {
                pixels[i + j * destWidth] = destPixels[i + j * destWidth];
                int sampleX = (int)(i * xRatio);
                int sampleY = (int)(j * yRatio);

                int srcPixelIdx = (int)(sampleX + sampleY * srcWidth);
                //if (srcPixelIdx >= srcPixels.Length)
                //    srcPixelIdx = srcPixels.Length - 1;
                
                //pixels[i + j * destWidth].a = (byte)(srcAlphaTex.GetPixel((int)(i * xRatio), (int)(j * yRatio)).a * byte.MaxValue);
                pixels[i + j * destWidth].a = srcPixels[srcPixelIdx].a;
            }

            dest.SetPixels32(pixels);
            dest.Apply();
        }
    }
}
