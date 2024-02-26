using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Video;

public class ProcAnimIntroSubTitle : MonoBehaviour 
{
    [System.Serializable]
    public class SubTitleAnimData
    {
        public int index;
        public string textKey;
        public float timeDelay;
        public float timeDestroy;
        public Vector2 position;      
    }

    public float durationStartAlpha = 0.4f;
    public float durationEndAlpha = 0.2f;
    
    public UILabel subTitleDescription;
    public UILabel subTitleDescriptionS;
    public Transform _subTitleRoot;

    public GameObject editRoot;
    public UILabel curMovieTime;
    public UILabel subTitleCurShowTime;

    public List<SubTitleAnimData> subTitleAnimDataList = new List<SubTitleAnimData>();

    // -----------------------------------------------------------------------------------
    [SerializeField]
    private bool isShowTestMode = false;

    private int curIndex = 0;
    private string curStringData = "";

    private VideoPlayer curPlayer;

    private SubTitleAnimData animData;

    // -----------------------------------------------------------------------------------
    IEnumerator ProcAnimIntro ()
    {
        int Count = this.subTitleAnimDataList.Count;
        
        animData = this.subTitleAnimDataList[this.curIndex];
      
        this.subTitleDescriptionS.text = "";

        //if ( Screen.width > 492 && animData != null )
        //{
        //    Debug.Log( ( ( Screen.width - 492 ) * 0.5f ).ToString() );
        //    this._subTitleRoot.localPosition = new Vector3( animData.position.x, animData.position.y + ( 0.5f * ( Screen.width - 492 ) ), 0 );
        //    this.subTitleDescription.fontSize = 32 + ( int ) ( 0.05f * ( Screen.width - 492 ) );
        //    this.subTitleDescriptionS.fontSize = 32 + ( int ) ( 0.05f * ( Screen.width - 492 ) );
        //}

        this.StartAutoTextFadeInOut( this.durationEndAlpha );

        while ( true )
        {
            if ( this.curPlayer.time >= animData.timeDelay )
            {
                this.subTitleDescription.text = Global._instance.GetString(animData.textKey);
                this.subTitleDescriptionS.text = this.subTitleDescription.text;

                this.subTitleCurShowTime.text = string.Format("TextTime : {0}", animData.timeDelay);
                this._subTitleRoot.localPosition = animData.position;
                //if ( Screen.width > 492 && animData != null )
                //{
                //    Debug.Log( ( ( Screen.width - 492 ) * 0.5f ).ToString() );
                //    this._subTitleRoot.localPosition = new Vector3( animData.position.x, animData.position.y + ( 0.5f * ( Screen.width - 492 ) ), 0 );
                //    this.subTitleDescription.fontSize = 32 + ( int ) ( 0.05f * ( Screen.width - 492 ) );
                //    this.subTitleDescriptionS.fontSize = 32 + ( int ) ( 0.05f * ( Screen.width - 492 ) );
                //}
                yield return null;

                this.StartAutoTextFadeInOut( this.durationStartAlpha );


                if ( Count - 1 > this.curIndex )
                {
                    animData = this.subTitleAnimDataList[++this.curIndex];
                }
                else
                {
                    break;
                }
            }
            yield return null;
        }
    }

    IEnumerator ProcAnimIntroDestroy ()
    {
        int Count = this.subTitleAnimDataList.Count;
        int index = 0;

        SubTitleAnimData animData = this.subTitleAnimDataList[index];
    
        while ( true )
        {
            if ( this.curPlayer.time >= animData.timeDestroy )
            {
                this.StartAutoTextFadeInOut( this.durationEndAlpha );

                if ( Count - 1 > index )
                {
                    animData = this.subTitleAnimDataList[++index];
                }
                else
                {
                    break;
                }
            }
            yield return null;
        }
    }

    IEnumerator UpdateTime ()
    {
        while ( true )
        {
            yield return new WaitForFixedUpdate();

            if(this.curPlayer != null)
            { 
                this.curMovieTime.text = string.Format( "MovieTime : {0:F2} ", this.curPlayer.time );
            }
        }
    }

    // -----------------------------------------------------------------------------------
    void Awake ()
    {
        // 현재 동영상 시간 설정
        if ( this.isShowTestMode )
        {
            this.editRoot.gameObject.SetActive( true );
            this.StartCoroutine( this.UpdateTime() );
        }
        else
        {
            this.editRoot.gameObject.SetActive( false );
        }
    }

    private void StartAutoTextFadeInOut ( float duration )
    {
        {
            TweenAlpha alphaEvent = this.subTitleDescription.GetComponent<TweenAlpha>();
            this.subTitleDescription.color = new Color(1, 1, 1, alphaEvent.to);
            alphaEvent.duration = duration;
            float temp = alphaEvent.from;
            alphaEvent.from = alphaEvent.to;
            alphaEvent.to = temp;
            alphaEvent.PlayForward();
        }

        {
            TweenAlpha alphaEvent = this.subTitleDescriptionS.GetComponent<TweenAlpha>();
            this.subTitleDescriptionS.color = new Color( 1, 1, 1, alphaEvent.to );
            alphaEvent.duration = duration;
            float temp = alphaEvent.from;
            alphaEvent.from = alphaEvent.to;
            alphaEvent.to = temp;
            alphaEvent.PlayForward();
        }
    }

    // -----------------------------------------------------------------------------------
    public void InitData ( VideoPlayer player )
    {
        this.curPlayer = player;

        this.curIndex = 0;
        this.StartCoroutine( this.ProcAnimIntro() );
        this.StartCoroutine( this.ProcAnimIntroDestroy() );

    }

    public void SetDataOfSubString ( string fileName )
    {
        if( 0 <=(this.curIndex - 1) )
        { 
            SubTitleAnimData data = this.subTitleAnimDataList[this.curIndex - 1];
            this.curStringData += string.Format( "index: {0}, startTime: {1}, position: {2}, description: {3} \n",
                this.curIndex, this.curPlayer.time, this._subTitleRoot.localPosition, Global._instance.GetString(animData.textKey));


            System.IO.File.WriteAllText( Application.dataPath + "/" + fileName + ".txt", this.curStringData );
        }
    }

    public void EndOfSubstring ()
    {
        this.subTitleDescription.gameObject.SetActive(false);
        this.subTitleDescriptionS.gameObject.SetActive(false);
    }
}
