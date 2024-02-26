using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
[System.Serializable]
public class ChatCameraPlayerTargetInfo
{
    public float fPlayerTargetDelay = -1f;
    public bool _OnPlayerTarget = true;
    public TypeCharacterType _ePcType = TypeCharacterType.Boni;
    public float _fPlayerTargetDuration = 1f;
    public float _fPlayerTargetInfluence = 1f;
    public bool _OnlyPlayerTarget = true;
}

[System.Serializable]
public class ChatCameraTargetInfo
{
    public float fTargetDelay = -1f;
    public bool _OnTarget = true;
    public sTriggerGameObjectInfo _sChatCameraTargetObjectInfo;
    public Transform _targetTransform;
    public float _fDuration = 1f;
    public float _ftargetInfluence = 1f;
    public bool _OnlyTarget = true;
}

[System.Serializable]
public class ChatCameraShakeInfo
{
    public float fShakeDelay = -1f;
    public CameraShakeInfo _cameraShakeInfo;
}

[System.Serializable]
public class ChatCameraZoomInfo
{
    public float fZoomDelay = -1f;
    public float fZoomAmount = 1f;
    public float fDuration = 0;
    public EaseType easeType = EaseType.Linear;
}
*/
[System.Serializable]
public struct sSceneDataObjectInfo
{
    public int              _nChapterIndex;
 //   public eChapterDataType _eChapterDataType;
    public int              _nSceneIndex;
    public MissionData _SDObject;
}

[System.Serializable]
public struct sMissionDataObjectInfo
{
    public int              _nChapterIndex;
//    public eChapterDataType _eChapterDataType;
    public int              _nSceneIndex;
    public MissionData _MDObject;
}

[System.Serializable]
public class sChatMission
{
    public Transform _targetTranform = null;
    public Vector3 _offset = Vector3.zero;
    public int _missionIndex;
}

public enum TypeChatBoxType
{
    NormalRight,
    NormalLeft,
    NormalMiddle,
    IconTexture,
    Mission,
    Quest,
    Notice,
    Monologue
}

public enum TypeChatEmoticon
{
    none,
    angry,
    blinding,
    confusion,
    exclamation,
    love,
    question,
}

[System.Serializable]
public class ChatData
{
    public string tempTitle;
    public string userInputBoxFunction = "";
    public string stringKey = "";
    //[TextArea]
    //public string strChat;
    //text color값 설정 : [c][" + EncodeColor24(c) + "]" + text + "[-][/c]
    //ex : 판매의 시작은 [c][f3790d]팻말[-][/c]부터지!

    /*
    public string strCharacterSpriteName_0 = "boni_a001";
    public float fSpriteSize_0 = 1f;
    public string strCharacterSpriteName_1 = "koko_a001";
    public float fSpriteSize_1 = 1f;
    */
    public bool bChatColor = false;

    public bool bCharPositionChange = false;

    public TypeCharacterType character_0 = TypeCharacterType.Boni;
    public string characterMotion_0 = "";
    public string characterExpression_0 = "";   //스파인으로 변경되면서 사용하지 않음
    public TypeChatEmoticon emoticon_0 = TypeChatEmoticon.none;
    public string chatBack_0 = "";

    public TypeCharacterType character_1 = TypeCharacterType.Boni;
    public string characterMotion_1 = "";
    public string characterExpression_1 = "";   //스파인으로 변경되면서 사용하지 않음
    public TypeChatEmoticon emoticon_1 = TypeChatEmoticon.none;
    public string chatBack_1 = "";

    public TypeChatBoxType boxType = TypeChatBoxType.NormalLeft;

    public TriggerConditionChating conditionData = null;
    public TriggerConditionChating waitConditionData = null;
//    public bool _bShowCloseBtn = false;
//    public bool _bShowNextBtn = true;
 //   public AudioInfo _audioInfo;
 //   public bool bCameraReset = false;
    //public bool bKeepChatMode = false;
  //  public sSceneDataObjectInfo _sPlayTriggerSDObjectInfo;
  //  public SceneData _playTriggerData = null;
 //   public sSceneDataObjectInfo _sPlaySceneSDObjectInfo;
    //public SceneData _playSceneData = null;
    //public sMissionDataObjectInfo[] _sAddmissionObjectInfos;
    public List<sChatMission> addMission = new List<sChatMission>();

    //페이드 기능 관련.
 //   public float fadeDelayTime = -1f;
  //  public string backgroundName = null;
  //  public bool bTurnBackground = false;

    //사진 팝업 기능 관련.
  //  public float photoDelayTime = -1f;
 //   public string photoName = null;

    public string chatBackCenter = "";
    //카메라 기능 관련.
    //  public ChatCameraPlayerTargetInfo   _playerTargerInfo;
    //  public ChatCameraTargetInfo         _targerInfo;
    // public ChatCameraShakeInfo          _shakeInfo;
    //  public ChatCameraZoomInfo           _zoomInfo;
    public bool bDeleteChat = false;

    public List<int> continueTargetMissionClear = new List<int>();

}

public class ChatGameData : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Comment", order = 1000)]
    [MultiLineString(100.0f, order = 1000)]
    public string comment = "";
#endif
    
    public List<ChatData> _listChatData;
    public float chatDelayTime = 0f;

    private void Start()
    {
        foreach(var chatData in _listChatData)
        {
            PreDownloader.AddItem(GetFullPath(chatData.chatBack_0));
            PreDownloader.AddItem(GetFullPath(chatData.chatBack_1));
            PreDownloader.AddItem(GetFullPath(chatData.chatBackCenter));
        }
    }

    public static string GetFullPath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }
        else
        {
            return $"CachedResource/Chat/{fileName}";
        }
    }
}
