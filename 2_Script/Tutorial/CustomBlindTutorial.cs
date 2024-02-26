using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// 인게임 블럭의 커스텀 블라인드 영역(인덱스로 찾을 때 사용)
/// </summary>
public class CustomBlindAreaData_Block_Index
{
    public int x1 = 0;
    public int y1 = 0;
    public int x2 = -1;
    public int y2 = -1;
    public int sizeX = 0;
    public int sizeY = 0;
    public int offsetX = 0;
    public int offsetY = 0;
}

[System.Serializable]
/// <summary>
/// 인게임 블럭의 커스텀 블라인드 영역(블럭 설정으로 찾을 때 사용)
/// </summary>
public class CustomBlindAreaData_Block_BlockType
{
    public FindBlockData findBlockData;
    public int sizeX = 0;
    public int sizeY = 0;
    public int offsetX = 0;
    public int offsetY = 0;
}

/// <summary>
/// pos : 알파들어갈 위치
/// wdith, height : 알파 영역 사이즈
/// </summary>
public class CustomBlindData
{
    public Vector3 pos;
    public int width;
    public int height;

    public CustomBlindData(Vector3 position, int alphaWidth, int alphaHeight)
    {
        this.pos = position;
        this.width = alphaWidth;
        this.height = alphaHeight;
    }
}

public class CustomBlindTutorial : MonoBehaviour
{
    public UITexture blind;

    //터치가능 영역 데이터.
    private List<CustomBlindData> listTouchData = new List<CustomBlindData>();
    //터치했을 때 실행될 액션.
    private System.Action touchAction = null;

    /// <summary>
    ///  커스텀 블라인드 텍스쳐를 생성해주는 함수
    ///  width, height : 화면에 출력될 텍스쳐 사이즈
    ///   textureSize : 생성할 텍스쳐 사이즈(2의 배수)
    /// </summary>
    public void MakeBlindTexture(int width, int height, int textureSize = 512)
    {
        Texture2D blindTexture = new Texture2D(textureSize, textureSize);
        Color fillColor = Color.black;
        Color[] fillPixels = new Color[blindTexture.width * blindTexture.height];

        for (int i = 0; i < fillPixels.Length; i++)
        {
            fillPixels[i] = fillColor;
        }
        blindTexture.SetPixels(fillPixels);
        blindTexture.wrapMode = TextureWrapMode.Clamp;

        blind.mainTexture = blindTexture;
        blind.width = width;
        blind.height = height;
    }

    /// <summary>
    /// 커스텀 블라인드 텍스쳐를 설정해주는 함수
    /// listCustomData : 알파 영역에 대한 데이터 리스트
    /// fillColor : 채우고자 하는 색상
    /// textureSize : 생성할 텍스쳐 사이즈(2의 배수)
    /// </summary>
    public void FillColorAtCustomBlindTexture(List<CustomBlindData> listCustomData, Color fillColor, int textureSize = 512)
    {
        int width = blind.width;
        int height = blind.height;

        //비율(생성된 텍스쳐 사이즈/ 화면에 출력될 텍스쳐 사이즈)
        float ratioX = ((float)textureSize / width);
        float ratioY = ((float)textureSize / height);

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        Texture2D blindTexture = blind.mainTexture as Texture2D;
        for (int i = 0; i < listCustomData.Count; i++)
        {
            CustomBlindData aData = listCustomData[i];
            //'알파 위치의 픽셀값 / 알파 사이즈' 계산
            int pixelX = (int)((halfW + aData.pos.x) * ratioX);
            int pixelY = (int)((halfH + aData.pos.y) * ratioY);
            int aWidth = (int)((aData.width * 0.5f) * ratioX);
            int aHeight = (int)((aData.height * 0.5f) * ratioY);

            //계산한 영역 알파로 칠해줌.
            for (int x = pixelX - aWidth; x < pixelX + aWidth; x++)
            {
                for (int y = pixelY - aHeight; y < pixelY + aHeight; y++)
                {
                    blindTexture.SetPixel(x, y, fillColor);
                }
            }
        }
        blindTexture.Apply();
    }

    public void FillColorAtCustomBlindTexture(CustomBlindData aData, Color fillColor, int textureSize = 512)
    {
        int width = blind.width;
        int height = blind.height;

        //비율(생성된 텍스쳐 사이즈/ 화면에 출력될 텍스쳐 사이즈)
        float ratioX = ((float)textureSize / width);
        float ratioY = ((float)textureSize / height);

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        Texture2D blindTexture = blind.mainTexture as Texture2D;
        //'알파 위치의 픽셀값 / 알파 사이즈' 계산
        int pixelX = (int)((halfW + aData.pos.x) * ratioX);
        int pixelY = (int)((halfH + aData.pos.y) * ratioY);
        int aWidth = (int)((aData.width * 0.5f) * ratioX);
        int aHeight = (int)((aData.height * 0.5f) * ratioY);

        //계산한 영역 알파로 칠해줌.
        for (int x = pixelX - aWidth; x < pixelX + aWidth; x++)
        {
            for (int y = pixelY - aHeight; y < pixelY + aHeight; y++)
            {
                blindTexture.SetPixel(x, y, fillColor);
            }
        }
        blindTexture.Apply();
    }

    public void SetAlphaCustomBlind(float in_alpha)
    {
        Color color = new Color(0f, 0f, 0f, Mathf.Clamp(in_alpha, 0.002f, 1f));
        blind.color = color;
    }

    #region 블라인드 터치 설정
    /// <summary>
    /// 터치 될 수 있는 영역과, 해당 영역을 터치했을 때 발생할 이벤트 설정
    /// </summary>
    public void SetTouchData(List<CustomBlindData> listTouch = null, System.Action action = null)
    {
        if (listTouch == null || action == null)
            return;

        listTouchData = new List<CustomBlindData>(listTouch);
        touchAction = action;
    }

    public void ResetTouchData()
    {
        listTouchData.Clear();
        touchAction = null;
    }

    private void OnPress(bool isPressed)
    {
        if (isPressed == false || listTouchData.Count == 0 || touchAction == null)
            return;

        Camera camera = GameUIManager.instance.GetComponentInChildren<Camera>();
        //마우스 포지션
        Vector3 touchPos = camera.ScreenToWorldPoint(Global._touchPos);

        #region 로컬 포지션 변수
        Vector3 minPosX_local = Vector3.zero;
        Vector3 maxPosX_local = Vector3.zero;
        Vector3 minPosY_local = Vector3.zero;
        Vector3 maxPosY_local = Vector3.zero;
        #endregion

        #region 월드 포지션 변수
        float minPosX = 0f;
        float maxPosX = 0f;
        float minPosY = 0f;
        float maxPosY = 0f;
        #endregion

        for (int i = 0; i < listTouchData.Count; i++)
        {
            float width = listTouchData[i].width * 0.5f;
            float height = listTouchData[i].height * 0.5f;

            //로컬 포지션
            minPosX_local = new Vector3(listTouchData[i].pos.x - width, listTouchData[i].pos.y, listTouchData[i].pos.z);
            maxPosX_local = new Vector3(listTouchData[i].pos.x + width, listTouchData[i].pos.y, listTouchData[i].pos.z);
            minPosY_local = new Vector3(listTouchData[i].pos.x, listTouchData[i].pos.y - height, listTouchData[i].pos.z);
            maxPosY_local = new Vector3(listTouchData[i].pos.x, listTouchData[i].pos.y + height, listTouchData[i].pos.z);

            ////월드 포지션으로 변환
            minPosX = transform.TransformVector(minPosX_local).x;
            maxPosX = transform.TransformVector(maxPosX_local).x;
            minPosY = transform.TransformVector(minPosY_local).y;
            maxPosY = transform.TransformVector(maxPosY_local).y;

            if ((minPosX < touchPos.x && touchPos.x < maxPosX) && (minPosY < touchPos.y && touchPos.y < maxPosY))
            {
                touchAction();
            }
        }
    }
    #endregion
}
