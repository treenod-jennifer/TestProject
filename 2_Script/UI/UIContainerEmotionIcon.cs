using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContainerEmotionIcon : MonoBehaviour 
{
    public static UIContainerEmotionIcon _instance = null;

    private List<UIItemEmotionIcon> curEmotionData = new List<UIItemEmotionIcon>();
    private Dictionary<EmotionID, UIItemEmotionIcon> emotionIconlist = new Dictionary<EmotionID,UIItemEmotionIcon>();
    
    public GameObject objEmotionIcon;
    public GameObject rootEmotionIcon;

    private string idArticle;
    public void InitData ( string idArticle )
    {
        if (Global.SkipStageComment() == false)
        {
            _instance = this;
            this.idArticle = idArticle;
            this.StartCoroutine(this.StartProcess());
        }
    }

    private IEnumerator StartProcess ()
    {
        while(true)
        {
            if( SDKReviewBoardManager._instance !=  null )
            {
                break;
            }

            yield return null;
        }

        this.RefreshEmotionData();
    }

    private void RefreshEmotionData  ()
    {
#if !UNITY_EDITOR
        SDKReviewBoardManager._instance.GetArticleByIDOfSpecific( "Stage", this.idArticle, false, (SDKBoardSpecificArticleInfo articleInfo) =>
        {
            EmotionCountData emotionData = ( articleInfo.emotionCounts != null ) ? articleInfo.emotionCounts : new EmotionCountData();

            if ( this.emotionIconlist.Count == 0 )
            { 
                this.AddEmotionData( EmotionID.like, EmotionTYPE.POSITIVE, emotionData.like, () => {}  );
                this.AddEmotionData( EmotionID.dislike, EmotionTYPE.NEGATIVE, emotionData.dislike, () => {} );
                this.AddEmotionData( EmotionID.sorrow, EmotionTYPE.POSITIVE, emotionData.sorrow, () => {} );
            }   

            // 정렬
            this.SortEmotionData( emotionData );
        } );
#elif UNITY_EDITOR
        string key = string.Format("EmotionCountData_Stage{0}", GameManager.instance.CurrentStage);
        string jsonEmotionValue = "";
        EmotionCountData emotionData;
        if ( PlayerPrefs.HasKey( key ) )
        {
            jsonEmotionValue = PlayerPrefs.GetString( key );
            emotionData = JsonFx.Json.JsonReader.Deserialize<EmotionCountData>( jsonEmotionValue );
        }
        else
        {
            emotionData = new EmotionCountData(true);
        }

        if ( this.emotionIconlist.Count == 0 )
        {
            this.AddEmotionData( EmotionID.like, EmotionTYPE.POSITIVE, emotionData.like, () => { } );
            this.AddEmotionData( EmotionID.dislike, EmotionTYPE.NEGATIVE, emotionData.dislike, () => { } );
            this.AddEmotionData( EmotionID.sorrow, EmotionTYPE.POSITIVE, emotionData.sorrow, () => { } );
        }

        // 정렬
        this.SortEmotionDataInEditor( emotionData );
#endif
    }

    private void SortEmotionData ( EmotionCountData countData )
    {
        this.curEmotionData.Clear();
        int index = 0;
        for ( int i = 0; i < 3; i++ )
        {
            EmotionID id = ( EmotionID )i ;
            EmotionTYPE type = ( EmotionTYPE )i;

            int count = GetCountData( id.ToString(), countData );

            if ( count != 0 )
            {
                this.emotionIconlist[id].count.text = ( count > 999 ) ? "999+" : count.ToString();
                this.emotionIconlist[id].gameObject.SetActive( true );
                this.emotionIconlist[id].gameObject.transform.parent = this.rootEmotionIcon.transform;
                UIWidget labelWidget = this.emotionIconlist[id].count.GetComponent<UIWidget>();

                if ( index == 0 )
                {
                    //offset = -61 + ( -labelWidget.width );// : ( -labelWidget.width ); // 0이 아닐때 
                    this.rootEmotionIcon.transform.localPosition = new Vector3( ( -labelWidget.width / 2 ), 0f, 0f );
                    this.emotionIconlist[id].gameObject.transform.localPosition = Vector3.zero + new Vector3( -10f, 0f, 0f );
                }
                else
                {
                    this.emotionIconlist[id].gameObject.transform.localPosition = new Vector3( ( ( -70 * index ) + ( index * ( -labelWidget.width / 2 ) ) ) + -10f, 0f, 0f );
                }

                this.curEmotionData.Add( this.emotionIconlist[id] );

                string countText = ( count > 999 ) ? "999+" : count.ToString();
                this.curEmotionData[index].InitEmotionData( id, type, countText, () => { } );
                index += 1;
            }
            else
            {
                this.emotionIconlist[id].gameObject.SetActive( false );
            }
        }
    }

    #region
//    int count = 0;
//    void Update()
//    {
//#if !UNITY_EDITOR
//        if( Input.touchCount == 1 )
//        {
//            count += 1;
//            this.RefreshEmotionData();
//        }
//        else if (Input.touchCount == 2)
//        {
//            if(count > 999)
//            {`
//                count = 999;
//            }
//            count -= 1;
//            this.RefreshEmotionData();
//        }
//        else if (Input.touchCount == 3)
//        {
//            count = 100;
//            this.RefreshEmotionData();
//        }
//        else if ( Input.touchCount == 4 )
//        {
//            count = 9999;
//            this.RefreshEmotionData();
//        }

//#elif UNITY_EDITOR
//        if (Input.GetMouseButton(0))
//        {
//            count += 1;
//            this.RefreshEmotionData();
//        }
//        else if ( Input.GetMouseButtonDown( 1 ) )
//        {
//            if(count > 999)
//            {
//                count = 999;
//            }
//            count -= 1;
//            this.RefreshEmotionData();
//        }
//#endif
//    }
    #endregion

    private void SortEmotionDataInEditor ( EmotionCountData countData )
    {
        // 임시로 emotionData 에디터에서는 로컬에 저장 
        this.curEmotionData.Clear();
        string key = string.Format("EmotionCountData_Stage{0}", GameManager.instance.CurrentStage);
        string jsonEmotionValue = "";

        SDKEmotionData[] emotionData = new SDKEmotionData[] { new SDKEmotionData( EmotionID.like, EmotionTYPE.POSITIVE, countData.like, 0), 
            new SDKEmotionData( EmotionID.dislike, EmotionTYPE.NEGATIVE, countData.dislike, 0), 
            new SDKEmotionData( EmotionID.sorrow, EmotionTYPE.POSITIVE, countData.sorrow, 0) };

        int length = emotionData.Length;     
        int index = 0;
        for ( int i = 0; i < length; i++ )
        {
            EmotionID id     = ( EmotionID ) System.Enum.Parse( typeof( EmotionID ), emotionData[i].id );
            EmotionTYPE type = ( EmotionTYPE ) System.Enum.Parse( typeof( EmotionTYPE ), emotionData[i].type );

            int count =  System.Convert.ToInt32(emotionData[i].count);
            if ( count != 0 )
            {
                this.emotionIconlist[id].count.text = ( count > 999 ) ? "999+" : emotionData[i].count;
                this.emotionIconlist[id].gameObject.SetActive( true );
                this.emotionIconlist[id].gameObject.transform.parent = this.rootEmotionIcon.transform;
                UIWidget labelWidget = this.emotionIconlist[id].count.gameObject.GetComponent<UIWidget>();
                
                if(index == 0)
                {
                    //offset = -61 + ( -labelWidget.width );// : ( -labelWidget.width ); // 0이 아닐때 
                    this.rootEmotionIcon.transform.localPosition = new Vector3( (-labelWidget.width / 2), 0f, 0f );
                    this.emotionIconlist[id].gameObject.transform.localPosition = Vector3.zero + new Vector3(-10f, 0f, 0f);
                }
                else
                {
                    this.emotionIconlist[id].gameObject.transform.localPosition = new Vector3(( (-70 * index) + ( index * ( -labelWidget.width / 2 )) ) + -10f, 0f, 0f );
                }

                this.emotionIconlist[id].gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                this.curEmotionData.Add( this.emotionIconlist[id] );

                string countText = ( count > 999 ) ? "999+": count.ToString();
                this.curEmotionData[index].InitEmotionData( id, type, countText, () => { } );
                index += 1;
            }
            else
            {
                this.emotionIconlist[id].gameObject.SetActive( false );
            }
        }
    }

    private int GetCountData ( string id , EmotionCountData countData )
    {
        if ( id == EmotionID.like.ToString() )
        {
           return countData.like;
        }
        else if ( id == EmotionID.dislike.ToString() )
        {
            return countData.dislike;
        }
        else 
        {
            return countData.sorrow;
        }
    }

    private void AddEmotionData ( EmotionID id, EmotionTYPE type, int count, System.Action callbackHandler )
    {
        if( this.emotionIconlist.ContainsKey(id) )
        {
            
            string countText = ( count > 999 ) ? "999+" : count.ToString();
            countText =  "999+";
            this.emotionIconlist[id].count.text = countText;
            this.emotionIconlist[id].InitEmotionData( id, type, countText, callbackHandler );
        }
        else
        {
            string countText = ( count > 999 ) ? "999+" : count.ToString();
            countText = "999+";
            UIItemEmotionIcon emotionIcon = NGUITools.AddChild( this.gameObject, this.objEmotionIcon ).GetComponent<UIItemEmotionIcon>();
            emotionIcon.InitEmotionData( id, type, countText, callbackHandler );
            emotionIcon.transform.localScale = new Vector3(1f, 1f, 1f);
            this.emotionIconlist.Add( id, emotionIcon );
        }
    }

    public void SelectEmotionIconHandler ()
    {
        this.RefreshEmotionData ();
    }

    private void OnClickUpdateEmotionData ()
    {
        if( UIPopupPanelEmotionIcon._instance == null )
        {
            if ( UIPopupClear._instance.bCanTouch == false )
                return;
            UIPopupClear._instance.bCanTouch = false;

            ManagerUI._instance.OpenPopupPanelEmotionIcon( this.SelectEmotionIconHandler, this.gameObject );
        }
    }
}
