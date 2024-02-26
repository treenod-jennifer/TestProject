using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStampItemButtonColor : UIStampItemButton
{
    private PopupInputColorPad colorPad;

    public GameObject colorPadRoot;
    public UILabel actionObj;
    
    public Animation animEvent;

    System.Action<List<UIItemColor>> callbackHandler;
    System.Action<List<UIItemColor>> callbackEndHandler;

    //---------------------------------------------------------------------------
    protected override void InitEventData ()
    {
        this.selectTexture.gameObject.SetActive( false );
    }

    public override void StartButtonActionEvent ()
    {
        this.colorPad = ManagerUI._instance.OpenPopupInputColorPad( this.SetColorObjectItem, callbackHandler );
        this.colorPad.transform.parent = this.colorPadRoot.transform;
        this.colorPad.transform.localPosition = Vector3.zero;

        this.callbackHandler( colorPad.colorDataList );
    }

    protected override void DestroyButtonActionEvent ()
    {
        if(this.colorPad != null)
        { 
            // 추후 작성
            this.callbackEndHandler( colorPad.colorDataList );
            ManagerUI._instance.ClosePopUpUI();
        }

    }

    //---------------------------------------------------------------------------    
    private void SetColorObjectItem (Color color)
    {
        this.actionObj.color = color;
        this.animEvent.Play();
    }

    public Color GetColorData ()
    {
        return this.actionObj.color;
    }
    
    //---------------------------------------------------------------------------
    public void InitializeData ( GameObject actionObj, Color color, System.Action<List<UIItemColor>> dataHandler, System.Action<List<UIItemColor>> dataEndHandler)
    {
       this.actionObj = actionObj.GetComponent<UILabel>();
       this.DestroyButtonActionEvent();
       this.actionObj.color = new Color( color.r, color.g, color.b );
       this.callbackHandler = dataHandler;
       this.callbackEndHandler = dataEndHandler;
    }

}
