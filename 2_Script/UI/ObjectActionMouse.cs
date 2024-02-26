using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectActionMouse : ObjectActionBase 
{
    protected enum EditorPositionData
    {
        StartPos = 0,
        EndPos = 1,
    }

    protected bool IsExitTouchEvent ( bool isMultiTouchEvent )
    {
        int eventCount = ( isMultiTouchEvent ) ? 1 : 0;

#if UNITY_EDITOR
        if ( Input.GetMouseButtonUp( eventCount ) )
        {
            return true;
        }
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR  
        if (Input.touchCount < (eventCount + 1) )
        {
            return true;
        }
#endif
        return false;
    }

    protected bool IsEnterTouchEvent ( bool isMultiTouchEvent )
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

    protected bool IsTouchCollider ( RaycastHit[] hits, Collider collider )
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
    protected List<Vector3> GetWorldToMultiTouchPos ( GameObject ationObj )
    {
        List<Vector3> resultPos = new List<Vector3>();
#if UNITY_EDITOR
        resultPos.Add( actionObj.transform.position );
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

    protected List<Vector3> GetScreenTouchPos ( GameObject ationObj )
    {
        List<Vector3> resultPos = new List<Vector3>();
#if UNITY_EDITOR
        resultPos.Add( ManagerUI._instance._camera.WorldToScreenPoint(actionObj.transform.position) );
        resultPos.Add( Input.mousePosition );

#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR  
        int touchCount = Input.touchCount;
        for(int i = 0; i < touchCount; i++)
        {
            resultPos.Add( Input.GetTouch( i ).position );
        }
#endif
        return resultPos;
    }

    protected bool IsClickEventObject ( Collider collider )
    {
        if ( this.GetTouchCount() >= 1 )
        {
            Vector3[] touchPos = this.GetWorldToMultiTouchPos( actionObj ).ToArray();
            bool confirmValue = true;
            // 터치가 맞는지 아닌지 검사 
            for ( int i = 0; i < touchPos.Length; i++ )
            {
                Vector3 screenPos = ManagerUI._instance._camera.WorldToScreenPoint( touchPos[i] );
                RaycastHit[] hits = Physics.RaycastAll( ManagerUI._instance._camera.ScreenPointToRay( screenPos ), 100.0f );

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
                }
            }

            if ( confirmValue && touchPos.Length != 1 )
            {
                return true;
            }
        }
        return false;
    }
}
