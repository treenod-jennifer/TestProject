using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyEvent : MonoBehaviour
{
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
    public UIUrlTexture collectObj;
    public UILabel getCountLabel;
    public UILabel getCountLabelShadow;
    public UILabel maxcountLabel;
    public UILabel maxcountLabelShadow;

    public TypeCharacterType live2dCharacter = TypeCharacterType.Boni;
    public Vector3 live2dSize = new Vector3(-300f, 300f, 300f);
    public Vector3 live2dOffset = Vector3.zero;
    public AudioLobby readyVoice = AudioLobby.m_boni_tubi;
    public AudioLobby moveVoice = AudioLobby.m_bird_hehe;
    public AudioLobby failVoice = AudioLobby.m_bird_hansum;
}
