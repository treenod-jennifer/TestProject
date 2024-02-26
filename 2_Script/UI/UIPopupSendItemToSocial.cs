using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Scripting;
using Newtonsoft.Json;

/// <summary>
/// 소셜 또는 갤러리로 캡처한 아이템텍스처를 전송한다.
/// </summary> 

public class UIPopupSendItemToSocial : UIPopupBase
{
    public static UIPopupSendItemToSocial _instance = null;
    
    //---------------------------------------------------------------------------
    // 스트링 수정 데이터 // Texture2D 임시 데이터 
    // TODO : 랜더 텍스처 적용시 Texture2D 제외
    public Action<UIItemStamp> _callbackBtnHandler; 
    public Action<Stamp> _callbackResetHandler;

    //public UIEditStampContainer editItem;
    public GameObject rootEditItem;

    // 라벨 제한 구역
    public Transform upPos;
    public Transform downPos;
    public Transform leftPos;
    public Transform rightPos;

    public UILabel title; 
    public UILabel titleS;

    public GameObject[] activeButtons;
    public GameObject[] inactiveButtons;

    private bool isClicked = false;

    public UIStampItemButtonContainer buttonConatiner;
    public BoxCollider editBoxCollider;
    [NonSerialized]
    public UIItemStamp item;


    //---------------------------------------------------------------------------
    private Stamp originData;
    private int stampIndex = 0;
    public int StampIndex { get { return stampIndex; } }
    //---------------------------------------------------------------------------
    void Awake ()
    {
        _instance = this;
        isClicked = false; 
    }

    //---------------------------------------------------------------------------
    public void InitPopup ( UIItemStamp stampData, Stamp originData, Action<UIItemStamp> _callbackBtnHandler, System.Action<Stamp> _callbackResetHandler,  bool closePopup = false , int tempStampIndex = 0)
    {
        this._callbackBtnHandler = _callbackBtnHandler;
        this._callbackResetHandler = _callbackResetHandler;
        this.InitStampObject( stampData, _callbackBtnHandler );

        this.originData = originData;
        this.title.text = Global._instance.GetString( "p_es_1" );
        this.titleS.text = Global._instance.GetString( "p_es_1" );
        this.stampIndex = tempStampIndex;

        //스탬프 edit 버튼 활성화 관련.
        bool[] bActives = stampData.data.enableEditButton;
        SettingButtonActiveState(bActives);
    }

    public void InitStampObject(UIItemStamp stamp, Action<UIItemStamp> _callbackHandler)
    {
        if (this.item != null)
        {
            DestroyImmediate(this.item);
        }

        this.item = Instantiate(stamp);
        this.item.transform.parent = this.rootEditItem.transform;
        this.item.transform.localScale = new Vector3(1f, 1f, 1f);
        this.item.transform.localPosition = new Vector3(44f, 251f, 0f);
        // NoneSerialize 라서 강제 세팅 해줌
        this.item.InitData(stamp.data);

        UIWidget textureWidget = this.item.stampTexture.GetComponent<UIWidget>();
        textureWidget.width = (int)(stamp.data.textureWidgetWidth * 1.758f);
        textureWidget.height = (int)(stamp.data.textureWidgetHeight * 1.758f);
        this.item.stampTexture.transform.localPosition = new Vector3(-44f, -22f, 0f) +
            new Vector3(stamp.data.textureLocalPosition.x, stamp.data.textureLocalPosition.y * 1.758f, stamp.data.textureLocalPosition.z);

        DestroyImmediate(this.item.stampTexture.GetComponent<BoxCollider2D>());
        this.editBoxCollider = this.item.stampTexture.gameObject.AddComponent<BoxCollider>();
        this.editBoxCollider.size = new Vector3(450, 450);
        this.editBoxCollider.center = new Vector3(2.75f, -4.2f, 1f);
        this.item.name = "UIItemStamp";

        this.item.stampText.transform.localPosition *= 1.758f;

        UIPanel panle = this.item.GetComponent<UIPanel>();
        panle.depth = 9;
        panle.clipRange = new Vector4(-42.47f, -25.03f, 450f, 450f);

        this.item.data.textSize = (int)(this.item.stampText.fontSize * 1.758f);
        this.item.data.textWidgetWidth = (int)(this.item.data.textWidgetWidth * 1.758f);
        this.item.data.textWidgetHeight = (int)(this.item.data.textWidgetHeight * 1.758f);
        this.item.data.textLocalPosition *= 1.758f;

        this.buttonConatiner.InitContainerData(this.item, _callbackHandler);
    }


    /// <summary>
    /// 아이템 텍스처를 라인으로 전송한다.
    /// </summary>
    public void OnClickBtnOK ()
    {
        if( isClicked == false )
        {
            isClicked = true;

            this.buttonConatiner.CheckNgWord((result) => {
                if(!result)
                {
                    this.buttonConatiner.GetEditStampResult();

                    //그로씨추가
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.EDIT,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.STAMP,
                                      "Stamp" + stampIndex,
                                      "Stamp",
                                     1,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EDIT_STAMP
                          );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
                else
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();

                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_7"), false);
                    popup.FunctionSetting(1, "submitFailCallback", this.gameObject);

                }
                });

            
        }
    }

    private void submitFailCallback()
    {
        isClicked = false;
        UIStampItemButton.curSelectItemButton?.StartButtonActionEvent();
    }

    // X버튼 눌렀을 시
    public void OnClickBtnInitialize ()
    {
        if ( isClicked == false )
        {
            isClicked = true;
            this._callbackResetHandler( this.originData );
            ManagerUI._instance.ClosePopUpUI();
        }   
    }

    private void SettingButtonActiveState(bool[] bActive)
    {
        for (int i = 0; i < activeButtons.Length; i++)
        {
            activeButtons[i].SetActive(bActive[i]);
            inactiveButtons[i].SetActive(!bActive[i]);
        }
    }
}
