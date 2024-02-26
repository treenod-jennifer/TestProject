using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectActionMove : ObjectActionMouse 
{
    private bool isStartMouseDragCoroutine = false;
    private Vector3 originPos;

    private bool firstMove = true;
    bool isMouseDragTest = false;
   
    //---------------------------------------------------------------------------
    /// <summary>
    /// 1. Action이벤트가 일어났는지 아닌지 구분해줌
    /// </summary>
    /// <param name="actionObj"> 해당 오브젝트 </param>
    /// <param name="editCollider"> 터치가 되었는지 안되었는지 구분용 </param>
    /// <returns></returns>
    protected override IEnumerator ProcCompuseActionEvent ( GameObject actionObj, BoxCollider editCollider )
    {
        this.editCollider = editCollider;
        this.actionObj = actionObj;

        while ( true )
        {
            if ( this.isEditComplete )
            {
                break;
            }

            if (  Input.touchCount == 1 && this.IsClickEventObject( editCollider ) )
            {
                this.StartCoroutine( this.OnActionEvent() );
                break;
            }
            else if ( this.IsEnterTouchEvent( false ) && this.IsClickEventObject( editCollider ) == false )
            {
                this.StopActionEvent();
                break;
            }

            yield return null;
        }
    }

    protected override IEnumerator OnActionEvent ()
    {
        Vector3 touchPos = ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( 0 ).position );

        while ( true )
        {
            if ( Input.touchCount != 1 )
            { 
                this.StartCoroutine(this.ProcCompuseActionEvent( actionObj, this.editCollider ));
                this.firstMove = true;
                break;
            }
            else
            {

                if( this.isEditComplete )
                {
                    break;    
                }

                if ( this.IsEnterTouchEvent( false ) && this.IsClickEventObject( editCollider ) == false )
                {
                    this.StopActionEvent();
                    break;
                }

                if ( Input.GetTouch( 0 ).phase == TouchPhase.Moved )
                {
                    if ( firstMove )
                    {
                        originPos = ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( 0 ).position );
                        firstMove = false;
                    }

                    //actionObj.transform.position = pos;
                    if( Vector3.Distance(ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( 0 ).position ), originPos) > 0.01f )
                    {
                        touchPos = ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( 0 ).position ); //this.GetWorldToMultiTouchPos( actionObj ).ToArray();
                        Vector3 moveDirection = ( touchPos - originPos ).normalized;
                        float moveDistance = Vector3.Distance( touchPos, originPos );
                        actionObj.transform.position = Vector3.Lerp( actionObj.transform.position, actionObj.transform.position + ( moveDirection * moveDistance ), 0.9f );

                        originPos = ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( 0 ).position );

                        Vector3 pos = actionObj.transform.position;

                        if ( pos.x > UIPopupSendItemToSocial._instance.rightPos.position.x )
                        {
                            pos.x = UIPopupSendItemToSocial._instance.rightPos.position.x;
                        }
                        if ( pos.x < UIPopupSendItemToSocial._instance.leftPos.position.x )
                        {
                            pos.x = UIPopupSendItemToSocial._instance.leftPos.position.x;
                        }
                        if ( pos.y > UIPopupSendItemToSocial._instance.upPos.position.y )
                        {
                            pos.y = UIPopupSendItemToSocial._instance.upPos.position.y;
                        }
                        if ( pos.y < UIPopupSendItemToSocial._instance.downPos.position.y )
                        {
                            pos.y = UIPopupSendItemToSocial._instance.downPos.position.y;
                        }

                        actionObj.transform.position = pos;
                    }
                    else
                    {
                        originPos = touchPos;
                    }

                }

                yield return null;
            }
        }
        yield break;
    }

    //---------------------------------------------------------------------------
    protected override void InitActionData ()
    {
        this.isEditComplete = false;
    }

    public override void StopActionEvent ()
    {
        this.isEditComplete = true;
        this.StartOnFinishedHandler();
    }

    public override void StopActionEvent ( bool isCallback )
    {
        if ( isCallback )
        {
            this.StopActionEvent();
        }
        else
        {
            this.isEditComplete = true;
        }

        this.firstMove = true;
    }

    
}
