using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIItemEmotionIcon : MonoBehaviour 
{
    public EmotionID emotionId;
    public EmotionTYPE emotionType;
    public UISprite texture;
    public UISprite check;
    public UILabel  count;

    private Action callbackHandler;

    private int curStage = 0;
    private string originKey;
    public Animation animation;

    private string loadKEYEmotionID        = "{0}Stage EmotionID";
    private string loadKeyEmotionsCountData = "EmotionCountData_Stage{0}";

    public void Start()
    {
        this.curStage = GameManager.instance.CurrentStage;
        this.loadKEYEmotionID = string.Format( "{0}Stage EmotionID", this.curStage );
        this.loadKeyEmotionsCountData = string.Format( "EmotionCountData_Stage{0}", this.curStage);
    }

    public void InitEmotionData ( EmotionID id, EmotionTYPE type, string emotionCount, Action callbackHandler )
    {
        this.emotionId = id;
        this.emotionType = type;
        this.count.text = emotionCount;
        this.callbackHandler = callbackHandler;

        this.SetIconTexture();
    }


    public void OnClickEmotionAdd ()
    {
        this.originKey = "";
        bool isEmotionAdd = false;
        // 트윈 애니메이션
        this.ProcAnimationStart();
        // 만약 키가 있다면 삭제
        if ( PlayerPrefs.HasKey( this.loadKEYEmotionID ) )
        {
            this.originKey = PlayerPrefs.GetString( this.loadKEYEmotionID );
            isEmotionAdd = ( this.originKey == this.emotionId.ToString() ) ? false : true;
        }
        else // 없다면 추가
        {
            this.originKey = EmotionID.emotionCount.ToString();
            isEmotionAdd = true;
        }

#if UNITY_EDITOR
        this.AddInEditor( isEmotionAdd );
#elif !UNITY_EDITOR
        this.AddInDevice( isEmotionAdd );
#endif
    }

    private void AddInEditor ( bool isEmotionAdd )
    {
        EmotionCountData data;

        if ( isEmotionAdd )
        {
            PlayerPrefs.SetString( this.loadKEYEmotionID, this.emotionId.ToString() );
            if ( PlayerPrefs.HasKey( this.loadKeyEmotionsCountData ) )
            {
                data = JsonFx.Json.JsonReader.Deserialize<EmotionCountData>( PlayerPrefs.GetString( this.loadKeyEmotionsCountData ) );
                if( this.originKey != EmotionID.emotionCount.ToString() )
                { 
                    data = this.SetCountData( data, ( EmotionID ) System.Enum.Parse( typeof( EmotionID ), this.originKey ), false );
                }
            }
            else
            {
                data = new EmotionCountData(true);
            }

            data = this.SetCountData( data, this.emotionId, true );
            PlayerPrefs.SetString( this.loadKeyEmotionsCountData, JsonFx.Json.JsonWriter.Serialize( data ) );
        }
        else
        {
            PlayerPrefs.DeleteKey( this.loadKEYEmotionID );

            if ( PlayerPrefs.HasKey( this.loadKeyEmotionsCountData ) )
            {
                data = JsonFx.Json.JsonReader.Deserialize<EmotionCountData>( PlayerPrefs.GetString( this.loadKeyEmotionsCountData ) );
                data = this.SetCountData( data, ( EmotionID ) System.Enum.Parse( typeof( EmotionID ), this.originKey ), false );
            }
            else
            {
                data = new EmotionCountData( true );
            }

            PlayerPrefs.SetString( this.loadKeyEmotionsCountData,  JsonFx.Json.JsonWriter.Serialize( data ) );
        }

        DelegateHelper.SafeCall( () =>
        {
            UIContainerEmotionIcon._instance.SelectEmotionIconHandler();
            UIPopupPanelEmotionIcon._instance.DeselectIconList();
        } );
    }

    private void AddInDevice ( bool isEmotionAdd )
    {
        string idArticle = ReviewBoard.GetArticleID( this.curStage );

        if ( isEmotionAdd )
        {
            if(emotionId == 0) GameManager.instance.stageReview = "Good";
            else if (emotionId == (EmotionID)1) GameManager.instance.stageReview = "tired";
            else if (emotionId == (EmotionID)2) GameManager.instance.stageReview = "surprise";

            SDKReviewBoardManager._instance.AddReactionOfArticle( "Stage", idArticle, this.emotionId.ToString(), () =>
            {
                PlayerPrefs.SetString( this.loadKEYEmotionID, this.emotionId.ToString() );

                DelegateHelper.SafeCall( () =>
                {
                    UIContainerEmotionIcon._instance.SelectEmotionIconHandler();
                    UIPopupPanelEmotionIcon._instance.DeselectIconList();
                } );
            } );

        }
        else
        {
            SDKReviewBoardManager._instance.DeleteReactionOfArticle( "Stage", idArticle, () =>
            {
                PlayerPrefs.DeleteKey( this.loadKEYEmotionID );

                DelegateHelper.SafeCall( () =>
                {
                    UIContainerEmotionIcon._instance.SelectEmotionIconHandler();
                    UIPopupPanelEmotionIcon._instance.DeselectIconList();
                } );

            } );
        }
    }

    private void SetIconTexture ()
    {
        if(this.emotionId == EmotionID.like) // like
        {
            this.texture.spriteName = "ressult_emoticon_01";
        }
        else if(this.emotionId == EmotionID.dislike) // hate
        {
            this.texture.spriteName = "ressult_emoticon_04";
        }
        else if (this.emotionId == EmotionID.sorrow) // 무서워
        {
            this.texture.spriteName = "ressult_emoticon_03";
        }
    }

    private EmotionCountData SetCountData ( EmotionCountData data, EmotionID id, bool isAdd )
    {
        int count = ( isAdd ) ? 1 : -1;

        if ( id == EmotionID.like )
        {
            data.like += count;
        }
        else if (id == EmotionID.dislike)
        {
            data.dislike += count;
        }
        else if (id == EmotionID.sorrow)
        {
            data.sorrow += count;
        }

        return data;
    }

    public void ProcAnimationStart ()
    {
        if ( animation.isPlaying == false )
        {
            animation.Play();
        }
        else if( animation.isPlaying )
        {
            animation.Stop();
        }
    }
}
