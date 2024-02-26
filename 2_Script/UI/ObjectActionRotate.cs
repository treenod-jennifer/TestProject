using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectActionRotate : ObjectActionMouse
{
    float pinchTurnRatio = Mathf.PI / 2;
    float minTurnAngle = 0;
    float pinchRatio = 1;
    float minPinchDistance = 0;
    //float panRatio = 1;
    //float minPanDistance = 0;

    public float turnAngle;
    public float turnAngleDelta;
    public float pinchDistanceDelta;
    public float pinchDistance;


    //---------------------------------------------------------------------------
    protected override IEnumerator ProcCompuseActionEvent (GameObject actionObj, BoxCollider editCollider)
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
                this.StartCoroutine ( this.OnActionEvent() );
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
        Vector3[] originTouchPos = this.GetScreenTouchPos( actionObj ).ToArray();
        pinchDistance = pinchDistanceDelta = 0;
        turnAngle = turnAngleDelta = 0;
        Quaternion desiredRotation = actionObj.transform.rotation;

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
                    // ...각도확인 ...
                    turnAngle = Angle( touch1.position, touch2.position );

                    float prevTurn = Angle( touch1.position - touch1.deltaPosition,
                        touch2.position - touch2.deltaPosition );

                    this.turnAngleDelta = Mathf.DeltaAngle( prevTurn, turnAngle );

                    if ( Mathf.Abs( this.turnAngleDelta ) > minTurnAngle )
                    {
                        this.turnAngleDelta *= pinchTurnRatio;
                    }
                    else
                    {
                        turnAngle = this.turnAngleDelta = 0;
                    }

                    if ( Mathf.Abs( this.turnAngleDelta ) > 0 )
                    { 
                        // rotate
                        Vector3 rotationDeg = Vector3.zero;
                        rotationDeg.z = this.turnAngleDelta;
                        desiredRotation *= Quaternion.Euler( rotationDeg );
                    }
                    actionObj.transform.rotation = desiredRotation;
                }   
            } 
            yield return null;
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

    private float Angle ( Vector2 pos1, Vector2 pos2 )
    {
        Vector2 from = pos2 - pos1;
        Vector2 to = new Vector2(1,0);

        float result = Vector2.Angle (from, to);
        Vector3 cross = Vector3.Cross(from, to);

        if(cross.z > 0)
        {
            result = 360f - result;
        }
        return result;
    }

}
