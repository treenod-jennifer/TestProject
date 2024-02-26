using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 스크린 캡처 매니저
/// </summary>
/// 
public class ManagerScreenCapture : MonoBehaviour
{
    public static ManagerScreenCapture _instance = null;

    // 캡처 결과 전달해줄 콜백 함수
    private Action<byte[]> callbackTextureDelegate;

    public Camera uiCamera;
    
    public GameObject itemRoot;
    private UIItemStamp item;

    public UIPanel bgPanel;

    void Awake ()
    {
        _instance = this;
    }

    /// <summary>
    /// 수정된 스템프 이미지 캡처
    /// </summary>
    /// <returns></returns>
    IEnumerator SaveImageCaptureTest ()
    {
        yield return new WaitForEndOfFrame();

        int resWidth = this.uiCamera.pixelWidth;
        int resHeight = this.uiCamera.pixelHeight;

        // RenderTexture 생성
        BoxCollider2D objCollider = this.item.stampTexture.GetComponent<BoxCollider2D>();
        Rect drawScaleRect = PokoMath.GUIRectWithObject2D( objCollider, this.uiCamera );
        RenderTexture rt = new RenderTexture( 512, ( int ) 512, 50 );
        this.uiCamera.targetTexture = rt;

        // RenderTexture 저장을 위한 Texture2D 생성
        Texture2D screenShot = new Texture2D( 512, 512, TextureFormat.ARGB32, false );
        this.uiCamera.Render();

        yield return new WaitForEndOfFrame();

        //카메라에 RenderTexture 할당
        ////RenderTexture.active에 설정된 RenderTexture를 Read 한다.
        RenderTexture.active = rt;
        Vector3 ScreenPos = this.uiCamera.WorldToScreenPoint( new Vector3( this.item.captureRectData.capturePos.position.x, this.item.captureRectData.capturePos.position.y ) );
        screenShot.ReadPixels( new Rect( 0, 0, 512, 512 ), 0, 0 );
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        this.callbackTextureDelegate( bytes );

#if UNITY_EDITOR
        System.IO.File.WriteAllBytes( Application.persistentDataPath + "/capture7.png", bytes );
#endif
        RenderTexture.active = null;
        uiCamera.targetTexture = null;
        Destroy( rt );

        DestroyImmediate( this.item.gameObject );
        // 우선 없앰
        DestroyImmediate( this.gameObject );
        yield break;
    }

    public void SaveImageCapture ( UIItemStamp itemData, bool isGallery, bool isSetUpData, Action<byte[]> callbackTextureDelegate )
    {
        bgPanel.SetRect( 0, 0, 512f, 512f );
        
        float size = (isGallery) ? 2f : 1.2f;

        this.item = Instantiate( itemData );
        item.transform.parent = this.itemRoot.transform;
        item.transform.localPosition = new Vector3( 0f, 0f, 0f );
        item.transform.localScale = new Vector3( 1f, 1f, 1f );

        UIWidget widgetItem = item.stampTexture.GetComponent<UIWidget>();
        widgetItem.width = ( int ) ( widgetItem.width * size );
        widgetItem.height = ( int ) ( widgetItem.height * size );

        item.stampText.transform.localPosition = itemData.stampText.transform.localPosition * size;
        item.stampText.fontSize = ( int ) ( itemData.stampText.fontSize * size );

        UIPanel stickerPanel = item.GetComponent<UIPanel>();
        stickerPanel.softBorderPadding = false;
        stickerPanel.alwaysOnScreen = true;
        stickerPanel.SetRect( 0, 0, widgetItem.width, widgetItem.height );

        item.gameObject.layer = 13;
        Transform[] child = item.GetComponentsInChildren<Transform>();
        for ( int i = 0; i < child.Length; i++ )
        {
            child[i].gameObject.layer = 13;
        }

        this.callbackTextureDelegate = callbackTextureDelegate;
        this.StartCoroutine( this.SaveImageCaptureTest() );
    }

}
