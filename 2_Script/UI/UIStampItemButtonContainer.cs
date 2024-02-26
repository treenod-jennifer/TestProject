using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIStampItemButtonContainer : MonoBehaviour
{
    public static UIStampItemButtonContainer instance = null;

    public UIStampItemButtonColor editBtnColor;
    public UIStampItemButtonText editBtnText;
    public UIStampItemButtonTransform editBtnTransform;

    public BoxCollider deselectCollider;

    private UIItemStamp editItem;
    private Action<UIItemStamp> callbackBtnHandler;
    private List<BoxCollider> colliderList = new List<BoxCollider>();

    private int defaultIndex = 0;
    private List<UIStampItemButton> defaultBtnList = new List<UIStampItemButton>();

    //---------------------------------------------------------------------------
    void Awake ()
    {
        instance = this;
    }

    void OnDestroy ()
    {
        this.StopCoroutine( this.IsAutoDeselctItemButton() );
        DestroyImmediate( editBtnColor );
        DestroyImmediate( editBtnText );
        DestroyImmediate( editBtnTransform );
    }

    //---------------------------------------------------------------------------
    IEnumerator IsAutoDeselctItemButton ()
    {
        while ( true )
        {
            bool isDeselect = true;

            if ( UIStampItemButton.curSelectItemButton != null )
            {
                int length = this.colliderList.Count;

                for ( int i = 0; i < length; i++ )
                {
                    if ( this.IsEnterTouchEvent( false ) )
                    {
                        if ( this.IsClickEventObject( this.colliderList[i], UIStampItemButton.curSelectItemButton.actionObjItem ) )
                        {
                            isDeselect = false;
                        }
                    }
                }

                // 충돌했을때
                if ( this.IsEnterTouchEvent( false ) && this.IsClickEventObject( this.deselectCollider, UIStampItemButton.curSelectItemButton.actionObjItem ) == false
                                                    && isDeselect )
                {
                    UIStampItemButton.curSelectItemButton.isSelected = true;
                    UIStampItemButton.curSelectItemButton.DoDeselectButtonData();
                    UIStampItemButton.curSelectItemButton = null;
                }

                if ( defaultBtnList[defaultIndex] != UIStampItemButton.curSelectItemButton )
                {
                    this.SetDefaultBtnData();
                }

            }
            else
            {

                int length = this.colliderList.Count;

                if ( this.IsEnterTouchEvent( false ) && this.IsClickEventObject( this.deselectCollider, null )
                    && isDeselect )
                {
                    if ( UIStampItemButton.curSelectItemButton == null && defaultBtnList[defaultIndex] != null
                        && defaultBtnList[defaultIndex].gameObject.activeInHierarchy == true)
                    {
                        UIStampItemButton.curSelectItemButton = defaultBtnList[defaultIndex];
                        UIStampItemButton.curSelectItemButton.isSelected = false;
                        UIStampItemButton.curSelectItemButton.StartButtonActionEvent();
                        UIStampItemButton.curSelectItemButton.selectTexture.gameObject.SetActive( true );

                        this.SetDefaultBtnData();
                    }
                }
            }

            yield return null;
        }
        yield break;
    }

    //---------------------------------------------------------------------------
    public void SetDefaultBtnData ()
    {
        int length = this.defaultBtnList.Count;

        if ( ( length - 1 ) <= defaultIndex )
        {
            this.defaultIndex = 1;
        }
        else
        {
            this.defaultIndex += 1;
        }
    }

    public void SetColorPadData (List<UIItemColor> colorList)
    {
        int length = colorList.Count;
        for ( int i = 0; i < length; i++ )
        {
            this.colliderList.Add( colorList[i].GetComponent<BoxCollider>() );
        }
    }

    public void DeleteColorPadData (List<UIItemColor> colorList)
    {
        int length = colorList.Count;
        for ( int i = 0; i < length; i++ )
        {
            this.colliderList.Remove( colorList[i].GetComponent<BoxCollider>() );
        }
    }

    public void InitContainerData (UIItemStamp stampItem, Action<UIItemStamp> callbackBtnHandler)
    {
        this.callbackBtnHandler = callbackBtnHandler;

        this.deselectCollider = stampItem.stampTexture.GetComponent<BoxCollider>();

        GameObject actionObj = stampItem.stampText.gameObject;
        GameObject actionRotateObj = stampItem.rootLabel;

        this.editBtnColor.InitializeData( actionObj, stampItem.data.fontColor, SetColorPadData, DeleteColorPadData );
        this.colliderList.Add( this.editBtnColor.GetComponent<BoxCollider>() );

        this.editBtnText.InitializeData( actionObj, stampItem.data );
        this.colliderList.Add( this.editBtnText.GetComponent<BoxCollider>() );

        this.editBtnTransform.InitializeData( actionObj, actionRotateObj, stampItem.data );
        this.colliderList.Add( this.editBtnTransform.GetComponent<BoxCollider>() );

        this.editItem = stampItem;

        defaultBtnList.Add( this.editBtnText );
        defaultBtnList.Add( this.editBtnTransform );

        this.StartCoroutine( this.IsAutoDeselctItemButton() );
    }

    public void GetEditStampResult ()
    {
        this.editBtnText.GetStringEditState( ( UIStampItemButtonText.EditWorkState state, string originValue ) => {
            switch ( state )
            {
                case UIStampItemButtonText.EditWorkState.Complete:
                    this.SetStampItem(true);
                    // 우선 스크린 캡처 
                    // TODO : 추후 랜더 스크린캡처시 삭제 수정
                    break;
                case UIStampItemButtonText.EditWorkState.Empty:
                    this.editItem.stampText.text = originValue;
                    ManagerUI._instance.ClosePopUpUI();
                    break;
                default : 
                    break;
            }


        });
    }

    public void CheckNgWord(Action<bool> callback)
    {
        this.editBtnText.GetStringEditState((UIStampItemButtonText.EditWorkState state, string originValue) => {
            switch (state)
            {
                case UIStampItemButtonText.EditWorkState.Complete:
                    string text = editBtnText.GetItemInputText();

                    bool isResult = true;
                    int stampIndex = UIPopupSendItemToSocial._instance.StampIndex;

                    ServerAPI.FilterStampNGWord(stampIndex, text, (Protocol.NGWordResp ngWordResp)
                    =>
                    {
                        if (ngWordResp.changed)
                        {
                            isResult = true;
                            this.editItem.stampText.text = originValue;
                        }
                        else
                        {
                            isResult = false;
                        }

                        string clippedString = Global.ClipString(originValue);
                        if (clippedString != originValue)
                        {
                            isResult = true;

                            this.editItem.stampText.text = clippedString;
                        }

                        callback(isResult);
                    });

                    break;
                case UIStampItemButtonText.EditWorkState.Empty:
                    this.editItem.stampText.text = originValue;
                    ManagerUI._instance.ClosePopUpUI();
                    callback(false);
                    break;
                default:
                    break;
            }


        });
    }

    public void SetStampItem (bool isHandler = false)
    {
        string text = editBtnText.GetItemInputText();
        this.editItem.data.TextOrKey = text;

        this.editItem.data.fontColor = editBtnColor.GetColorData();
        this.editItem.data.textLocalPosition = editBtnTransform.GetTransformData().localPosition / 1.758f;
        this.editItem.data.textLocalRotation = editBtnTransform.GetTransformData().eulerAngles;

        UIStampItemButtonTransform.LabelScaleData scaleData = editBtnTransform.GetLabelScaleData();
        this.editItem.data.textSize = ( int ) ( scaleData.fontSize / 1.758 );
        this.editItem.data.textWidgetHeight = ( int ) ( scaleData.height / 1.758f );
        this.editItem.data.textWidgetWidth = ( int ) ( scaleData.width / 1.758f );

        this.callbackBtnHandler( editItem );
        if ( isHandler )
        {
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    public void ResetData (Stamp originData)
    {
        this.editItem.InitData( originData );
    }

    //-----------------------------임시로 넣어놓음-------------------------------
    protected bool IsEnterTouchEvent (bool isMultiTouchEvent)
    {
        int eventCount = ( isMultiTouchEvent ) ? 1 : 0;

#if UNITY_EDITOR
        if ( Input.GetMouseButtonDown( eventCount ) )
        {
            return true;
        }

#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR  
        if (Input.touchCount == ( eventCount + 1 ) )
        {
            return true;
        }
#endif
        return false;
    }

    protected bool IsClickEventObject (Collider collider, GameObject actionObjItem, bool isMultiTouch = false)
    {
        if ( this.GetTouchCount() >= 1 )
        {
            Vector3[] touchPos;
            if ( isMultiTouch && actionObjItem != null )
            {
                touchPos = this.GetWorldToMultiTouchPos( actionObjItem ).ToArray();
            }
            else
            {
                touchPos = new Vector3[] { ManagerUI._instance._camera.ScreenToWorldPoint( Input.mousePosition ) };
            }

            bool confirmValue = true;
            // 터치가 맞는지 아닌지 검사 
            for ( int i = 0; i < touchPos.Length; i++ )
            {
                Vector3 screenPos = ManagerUI._instance._camera.WorldToScreenPoint( touchPos[i] );
                RaycastHit[] hits = Physics.RaycastAll( ManagerUI._instance._camera.ScreenPointToRay( screenPos ).origin, new Vector3( 0f, 0f, 100f ) );
                Debug.DrawRay( ManagerUI._instance._camera.ScreenPointToRay( screenPos ).origin, ManagerUI._instance._camera.ScreenPointToRay( screenPos ).direction * 10, Color.yellow );

                if ( touchPos.Length == 1 )
                {
                    if ( this.IsTouchCollider( hits, collider ) )
                    {
                        return true;
                    }
                }
                else
                {
                    if ( this.IsTouchCollider( hits, collider ) == false )
                    {
                        confirmValue = false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            if ( confirmValue && touchPos.Length != 1 )
            {
                return true;
            }
        }
        return false;
    }

    protected bool IsTouchCollider (RaycastHit[] hits, Collider collider)
    {
        for ( int i = 0; i < hits.Length; i++ )
        {
            RaycastHit hit = hits[i];

            if ( hit.collider == collider )
            {
                return true;
            }
        }
        return false;
    }

    protected int GetTouchCount ()
    {
#if UNITY_EDITOR
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            return 1;
        }
        else if ( Input.GetMouseButtonDown( 1 ) )
        {
            return 2;
        }
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR           
        if( Input.touchCount == 1 )
        {
            return 1;
        }
        else if(Input.touchCount > 1)
        {
            return 2;
        }
#endif
        return 0;
    }

    /// <summary>
    /// 현재 터치하고 있는 Position을 World 좌표로 받아옴
    /// 에디터에서 받아오는 Position Index : 
    /// *_ 0 : 편집중인 오브젝트 Position
    /// *_ 1 : 마우스 Position
    /// </summary>
    /// <returns></returns>
    protected List<Vector3> GetWorldToMultiTouchPos (GameObject ationObj)
    {
        List<Vector3> resultPos = new List<Vector3>();
#if UNITY_EDITOR
        resultPos.Add( ationObj.transform.position );
        resultPos.Add( ManagerUI._instance._camera.ScreenToWorldPoint( Input.mousePosition ) );

#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR  
        int touchCount = Input.touchCount;
        for(int i = 0; i < touchCount; i++)
        {
            resultPos.Add( ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( i ).position ) );
        }
#endif
        return resultPos;
    }


}
