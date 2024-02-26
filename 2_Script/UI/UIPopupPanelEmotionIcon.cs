using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIPopupPanelEmotionIcon : UIPopupBase 
{
    public static UIPopupPanelEmotionIcon _instance = null;
    //public List<UIItemEmotionIcon> curEmotionIconList = new List<UIItemEmotionIcon>();
    public UIPanel emoticonPanel;

    private Action _callbackHandler;

    public GameObject objEmotionIcon;
    public BoxCollider popupCollider;

    private int curStage = 0;

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            emoticonPanel.useSortingOrder = true;
            emoticonPanel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    IEnumerator IsClosePopup ()
    {
        while(true)
        {
            if ( this.IsClickEventObject( popupCollider ) == false )
            {
                if( Input.touchCount != 0 || Input.GetMouseButton (0))
                {
                    if ( UIPopupClear._instance.bCanTouch == false )
                        yield return null;
                    else
                    { 
                        Debug.Log( "this.DestroyPopupObject()" );
                        this.DestroyPopupObject();
                        break;
                    }
                }
            }

            yield return null;
        }
        yield return null;
    }

    // Use this for initialization
    void Awake ()
    {
        if( _instance == null )
        { 
            _instance = this;
        }
    }
    
    void OnDestroy ()
    {
        _instance = null;
    }
        
    private bool IsClickEventObject ( Collider collider )
    {
        if ( Input.touchCount >= 1  || Input.GetMouseButton(0) )
        {
            Vector3[] touchPos = this.GetWorldToMultiTouchPos().ToArray();
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

    protected List<Vector3> GetWorldToMultiTouchPos ()
    {
        List<Vector3> resultPos = new List<Vector3>();
#if UNITY_EDITOR
        resultPos.Add( ManagerUI._instance._camera.ScreenToWorldPoint( Input.mousePosition ) );
#elif !UNITY_EDITOR  
        int touchCount = Input.touchCount;
        for(int i = 0; i < touchCount; i++)
        {
            resultPos.Add( ManagerUI._instance._camera.ScreenToWorldPoint( Input.GetTouch( i ).position ) );
        }
#endif
        return resultPos;
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

    public void InitPopup (System.Action callbackHandler)
    {
        this._callbackHandler = callbackHandler;

        this.StartCoroutine( "IsClosePopup" );
        this.curStage = UIPopupReady.stageIndex;
        this.RefreshEmotionData();

        UIPopupClear._instance.bCanTouch = true;
    }

    public void RefreshEmotionData ()
    {
        string idArticle = string.Format( "Stage_{0:D3}", ( curStage ) );
        /*
#if !UNITY_EDITOR
        SDKReviewBoardManager._instance.GetArticleByIDOfSpecific( "Stage", idArticle, false, (SDKBoardSpecificArticleInfo articleInfo) =>
        {
            string id = "";
            if ( PlayerPrefs.HasKey( string.Format( "{0}Stage EmotionID", curStage ) ) )
            {
                id = PlayerPrefs.GetString( string.Format( "{0}Stage EmotionID", this.curStage ) );

                for ( int i = 0; i < this.curEmotionIconList.Count; i++ )
                {
                    if ( this.curEmotionIconList[i].emotionId.ToString() == id )
                    {
                        this.curEmotionIconList[i].check.gameObject.SetActive( true );
                    }
                    else
                    {
                        this.curEmotionIconList[i].check.gameObject.SetActive( false );
                    }
                }
            }

        } );
#elif UNITY_EDITOR
        string id = "";
        if ( PlayerPrefs.HasKey( string.Format( "{0}Stage EmotionID", curStage ) ) )
        {
            id = PlayerPrefs.GetString( string.Format( "{0}Stage EmotionID", this.curStage ) );

            for ( int i = 0; i < this.curEmotionIconList.Count; i++ )
            {
               if ( this.curEmotionIconList[i].emotionId.ToString() == id )
                {
                    this.curEmotionIconList[i].check.gameObject.SetActive( true );
                }
                else
                {
                    this.curEmotionIconList[i].check.gameObject.SetActive( false );
                }
            }
        }
#endif 
*/
    }
    
    public void DeselectIconList ()
    {/*
        string id = "";
        if ( PlayerPrefs.HasKey( string.Format( "{0}Stage EmotionID", this.curStage ) ) )
        {
            id = PlayerPrefs.GetString( string.Format( "{0}Stage EmotionID", this.curStage ) );

            for ( int i = 0; i < curEmotionIconList.Count; i++ )
            {
                if ( this.curEmotionIconList[i].emotionId.ToString() != id )
                {
                    this.curEmotionIconList[i].check.gameObject.SetActive( false );
                }
                else
                {
                    this.curEmotionIconList[i].check.gameObject.SetActive( true );
                }
            }
        }
        else
        {
            for ( int i = 0; i < curEmotionIconList.Count; i++ )
            {
                this.curEmotionIconList[i].check.gameObject.SetActive( false );
            }
        }
*/
    }



    public void DestroyPopupObject ()
    {/*
        if ( UIPopupClear._instance.bCanTouch == false )
        return;

        for ( int i = 0; i < this.curEmotionIconList.Count; i++ )
        {
            UIItemEmotionIcon data = this.curEmotionIconList[i];
            this.curEmotionIconList[i] = null;
            DestroyImmediate( data.gameObject );
        }

        this.curEmotionIconList.Clear();

        ManagerUI._instance.ClosePopUpUI();*/
    }

}
