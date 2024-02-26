using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stamp : MonoBehaviour
{
    public string name = "";                   // fileName
    public Texture stampTexture;               // fileTexture // TODO: 추후 지워짐
    public string text = "";                  // fileEditText
    public Vector3 textLocalPosition;          // textEditPosition
    public Vector3 textLocalRotation;          // textEditRotation
    public int textureWidgetWidth;
    public int textureWidgetHeight;
    public Vector3 textureLocalPosition;
    public int textSize = 28;                   // textSize
    public int maxLines = 1;
    public Color fontColor = Color.white;                  // textFontColor
    public UIWidget.Pivot textPivot;           // textPivot;
    public int textWidgetWidth = 256;                // textWidgetWidth;
    public int textWidgetHeight = 32;               // textWidgetHeight;
    public float textAspectRatio = 0.01f;                   // textMaxWidth;
    public UIWidget.AspectRatioSource keepAspectRatio = UIWidget.AspectRatioSource.Free;
    public NGUIText.Alignment textAlignment;  // textAlignment;
    public UILabel.Overflow textOverflow;      // textOverflow;
    public UILabel.Effect textEffect;        // textEffect (Shadow or OutLine);
    public Color textEffectColor  = new Color(0f,0f,0f,0.3f);              // textEffectColor; 
    public bool[] enableEditButton = new bool[3] { true, true, true };   // 버튼 사용여부(글수정, 컬러수정, 위치수정).
    public int textLimit = 20;      //텍스트 최대 길이.
    public long startTime = 0;      //이벤트 시작 시간.
    public long endTime = 0;        //이벤트 종료 시간.

    public void Clone(Stamp data)
    {
        this.name = data.name;
        this.stampTexture = data.stampTexture;               // fileTexture // TODO: 추후 지워짐
        this.textureWidgetWidth = data.textureWidgetWidth;
        this.textureWidgetHeight = data.textureWidgetHeight;
        this.textureLocalPosition = data.textureLocalPosition;
        this.text =  data.text;                  // fileEditText
        this.textLocalPosition = data.textLocalPosition;          // textEditPosition
        this.textLocalRotation = data.textLocalRotation;          // textEditRotation
        this.textSize = data.textSize;                   // textSize
        this.maxLines = data.maxLines;
        this.fontColor = data.fontColor;                  // textFontColor
        this.textPivot = data.textPivot;           // textPivot;
        this.textWidgetWidth = data.textWidgetWidth;                // textWidgetWidth;
        this.textWidgetHeight = data.textWidgetHeight;               // textWidgetHeight;
        this.textAspectRatio = data.textAspectRatio;               
        this.keepAspectRatio = data.keepAspectRatio;
        this.textAlignment = data.textAlignment;  // textAlignment;
        this.textOverflow = data.textOverflow;      // textOverflow;
        this.textEffect = data.textEffect;        // textEffect (Shadow or OutLine);
        this.textEffectColor= data.textEffectColor;              // textEffectColor; 
        this.enableEditButton = data.enableEditButton;  // 버튼 사용여부(글수정, 컬러수정, 위치수정).
        this.textLimit = data.textLimit;    //텍스트 최대 길이.
        this.startTime = data.startTime;    //이벤트 시작 시간.
        this.endTime = data.endTime;        //이벤트 종료 시간.
    }
    /*
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}*/
}
