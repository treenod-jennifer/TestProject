using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectActionScale : ObjectActionMouse
{
	private float originDistance;
	private float touchDistance;

    private bool isStartMouseDragCoroutine = false;

    float pinchRatio = 1;
    float minPinchDistance = 0f;

    float panRatio = 1;
    float minPanDistance = 0;

    public float turnAngle;
    public float turnAngleDelta;
    public float pinchDistanceDelta;
    public float pinchDistance;

    //---------------------------------------------------------------------------
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

            if ( this.IsEnterTouchEvent( true ) && this.IsClickEventObject( editCollider ) )
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
        UIWidget textWidget = actionObj.GetComponent<UIWidget>();
        Vector3[] touchPos = this.GetWorldToMultiTouchPos( actionObj ).ToArray();
        UILabel objText = actionObj.GetComponent<UILabel>();
        float pinchAmount = 0;

        while ( true )
        {
            if ( this.IsExitTouchEvent( true ) )
            {
                this.StartCoroutine( this.ProcCompuseActionEvent( actionObj, this.editCollider ) );
                break;
            }
            else
            { 
                if ( this.isEditComplete )
                {
                    break;
                }

                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if( touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved )
                {
                    pinchDistance = Vector2.Distance( touch1.position, touch2.position );
                    float prevDistance = Vector2.Distance( touch1.position - touch1.deltaPosition,
                                                          touch2.position - touch2.deltaPosition );
                    pinchDistanceDelta = pinchDistance - prevDistance;

                    if ( Mathf.Abs( pinchDistanceDelta ) > minPinchDistance )
                    {
                        pinchDistanceDelta *= pinchRatio;
                    }
                    else
                    {
                        pinchDistance = pinchDistanceDelta = 0;
                    }

                    if ( Mathf.Abs( this.pinchDistanceDelta ) > minPinchDistance )
                    { // zoom
                   
                        pinchAmount = this.pinchDistanceDelta;

                        textWidget.width = textWidget.width + ( int ) pinchAmount;
                        textWidget.height = textWidget.height + ( int ) pinchAmount;
                        objText.fontSize = objText.fontSize + ( int ) pinchAmount;
                    }

                    if ( objText.fontSize >= 254 )
                    {
                        objText.fontSize = 254;
                        textWidget.width = textWidget.width - ( int ) pinchAmount;
                        textWidget.height = textWidget.height - ( int ) pinchAmount;
                    }
                    else if ( objText.fontSize <= 30 )
                    {
                        objText.fontSize = 30;
                        textWidget.width = textWidget.width - ( int ) pinchAmount;
                        textWidget.height = textWidget.height - ( int ) pinchAmount;
                    }      
                }
            }
            yield return null;
        }
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

    public override void StopActionEvent (bool isCallback)
    {
        if ( isCallback )
        {
            this.StopActionEvent();
        }
        else
        {
            this.isEditComplete = true;
        }
    }
    //---------------------------------------------------------------------------
   
}