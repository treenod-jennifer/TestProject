using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyEvent : MonoBehaviour
{
    [System.Serializable]
    public class ScoreModeRewardLine
    {
        public UISprite spriteRewardLine;
        public UILabel textRewardCount;
    }

    private TextMeshGlobalString title = null;
    private TextMeshGlobalString Title{
        get
        {
            if(title == null)
            {
                title = transform.GetComponentInChildren<TextMeshGlobalString>(true);
            }
            
            return title;
        }
    }

    public List<UITexture> _step = new List<UITexture>();
    public List<UITexture> _star = new List<UITexture>();
    public List<GameObject> _free = new List<GameObject>();
    public Transform giftRoot;
    public Vector3 _offsetPoint = Vector3.zero;

    public UITexture _texturePoint;
    public UITexture _texturePointShadow;

    public Texture _textureStepOn;
    public Texture _textureStepOff;

    public GameObject collectRoot;
    public GameObject waveRoot;
    public UILabel waveLabel;
    public UILabel waveLabelShadow;
    public UIUrlTexture collectObj;
    public UILabel getCountLabel;
    public UILabel getCountLabelShadow;
    public UILabel maxcountLabel;
    public UILabel maxcountLabelShadow;

    #region 스코어 모드
    public GameObject scoreRoot;
    public UILabel[] getScoreBadgeLabel;
    public UILabel[] maxScoreBadgeLabel;

    public UIProgressBar scoreProgressBar;
    public List<ScoreModeRewardLine> listScoreRewardLine;
    public UILabel maxBadgeText;

    public NGUIAtlas scoreModeAtlas;
    public UISprite[] scoreModeUseAtlasSprite;
    #endregion

    public TypeCharacterType live2dCharacter = TypeCharacterType.Boni;
    public Vector3 live2dSize = new Vector3(-300f, 300f, 300f);
    public Vector3 live2dOffset = Vector3.zero;
    public AudioLobby readyVoice = AudioLobby.m_boni_tubi;
    public AudioLobby moveVoice = AudioLobby.m_bird_hehe;
    public AudioLobby failVoice = AudioLobby.m_bird_hansum;

    private void Awake()
    {
        Title?.SetStringKey($"title_ev_{Global.eventIndex}");
    }
}
