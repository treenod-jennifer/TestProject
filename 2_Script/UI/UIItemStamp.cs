using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using System;
using System.Linq;

[System.Serializable]
public class CaptureRectData 
{
    public BoxCollider2D objCollider;
    public Transform capturePos;
}

public class UIItemStamp : MonoBehaviour
{
    // baseStampData
    [NonSerialized]
    public Stamp data;

    public GameObject rootLabel;
    public UITexture stampTexture;
    public UILabel stampText;

    public CaptureRectData captureRectData;

    // CaptureComplete SignindicateDraw --------
    private Rect drawScaleRect;
    private Texture drawTexture;
    //------------------------------------------
    private Texture2D imgTextureByte;
    private bool isImgSettingComplete;

    //StampData item;
    //지금은 사용안하는 코드(무한 스크롤 사용할 때).
    public void UpdateData ( ServerUserStamp stampData )
    {   
        if (stampData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        // item = stampData;
    }

    public void InitData( Stamp in_data )
    {
        // 기본 데이터 받아옴
        this.data = transform.GetComponent<Stamp>();
        if (data == null)
        {
            this.data = this.gameObject.AddComponent<Stamp>(); //Instantiate<Stamp>( in_data );
        }

        // 기본 데이터 세팅
        this.data.Clone(in_data);
        this.stampTexture.mainTexture = this.data.stampTexture;

        UIWidget textureWidget = this.stampTexture.GetComponent<UIWidget>();
        textureWidget.width  = data.textureWidgetWidth;
        textureWidget.height = data.textureWidgetHeight;
        
        this.stampTexture.transform.localPosition = this.data.textureLocalPosition;

        this.stampText.fontSize = this.data.textSize;
        this.stampText.color = this.data.fontColor;
        this.stampText.maxLineCount = this.data.maxLines;

        //스템프 기본 위젯 정보
        UIWidget stampWidget = this.stampText.GetComponent<UIWidget>();
        stampWidget.pivot = this.data.textPivot;
        stampWidget.width = this.data.textWidgetWidth;
        stampWidget.height = this.data.textWidgetHeight;
        stampWidget.aspectRatio = this.data.textAspectRatio;
        stampWidget.keepAspectRatio = this.data.keepAspectRatio;

        this.stampText.alignment = this.data.textAlignment;
        this.stampText.overflowMethod = this.data.textOverflow;
        this.stampText.effectStyle = UILabel.Effect.Shadow;
        this.stampText.effectColor = new Color( this.stampText.color.r - ( 150f / 255f ), this.stampText.color.g - ( 150f / 255f ), this.stampText.color.b - ( 150f / 255f ), 57f / 255f );
        this.stampText.effectDistance = new Vector2( 1.34f, 1.22f );
        this.stampText.text = this.data.Text;
        this.stampText.MakePixelPerfect();
        this.stampText.transform.localPosition = this.data.textLocalPosition;
        this.stampText.transform.eulerAngles = this.data.textLocalRotation;
    }  

    /// <summary>
    /// 텍스트 수정 이벤트
    /// </summary>
    public void OnClickBtnEdit ()
    {
        //ManagerUI._instance.OpenPopupInputText( this.SetStampStringHandler );
    }

    /// <summary>
    /// 라인 및, 갤러리 사진 저장 팝업 이벤트
    /// </summary>
    public void OnClickBtnSend ()
    {
        this.isImgSettingComplete = false;
        this.StartCoroutine( this.SaveImageCapture() );
    }

    /// <summary>
    /// 수정된 스템프 이미지 캡처
    /// </summary>
    /// <returns></returns>
    IEnumerator SaveImageCapture ()
    {
        yield return new WaitForEndOfFrame();

        this.captureRectData.objCollider = this.stampTexture.GetComponent<BoxCollider2D>();
        this.drawScaleRect = PokoMath.GUIRectWithObject2D( this.captureRectData.objCollider, ManagerUI._instance._camera );

        imgTextureByte = new Texture2D( ( int ) this.drawScaleRect.width, ( int ) this.drawScaleRect.height, TextureFormat.RGB24, true );
        Vector3 ScreenPos = ManagerUI._instance._camera.WorldToScreenPoint( new Vector3( this.captureRectData.capturePos.position.x, this.captureRectData.capturePos.position.y ) );
        imgTextureByte.ReadPixels( new Rect( ScreenPos.x, ScreenPos.y,
                                          this.drawScaleRect.width, this.drawScaleRect.height), 0, 0, true );
        imgTextureByte.Apply();
        
        this.isImgSettingComplete = true;
        yield break;
    }
}
