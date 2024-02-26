using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIStampItemButton : UIPokoButton
{
    public static UIStampItemButton curSelectItemButton = null;

    public UITexture selectTexture;

    public GameObject actionObjItem;

    public bool isSelected = false;
    public bool isClickBtnInSide = false;

    void Start ()
    {
        this.isSelected = false;
        this.isClickBtnInSide = false;
        this.InitEventData();
    }

    //---------------------------------------------------------------------------
    private void DoSelectButtonData ()
    {
        // 기존에 선택된 버튼이 있다면
        if ( curSelectItemButton != null)
        {
            curSelectItemButton.DoDeselectButtonData();          
            if( curSelectItemButton == this )
            {
                curSelectItemButton = null;
                return;
            }
            else
            { 
                curSelectItemButton = null;
            }
        }

        // 버튼을 눌렀는데 선택 상태가 아니라면
        if ( this.isSelected == false )
        {       
            // 선택완료
            this.isSelected = true;
   
            // 버튼 이벤트 시작
            this.StartButtonActionEvent();
            this.selectTexture.gameObject.SetActive( true );
            // 현재 버튼 값을 넣어준다.
            curSelectItemButton = this;
        }
    }

    protected abstract void InitEventData ();
    protected abstract void DestroyButtonActionEvent ();
    public abstract void StartButtonActionEvent ();

    public void DoDeselectButtonData ()
    {
        // 버튼 선택처리
        this.isSelected = false;
        this.selectTexture.gameObject.SetActive( false );
        this.DestroyButtonActionEvent();
    }

    
  
}
