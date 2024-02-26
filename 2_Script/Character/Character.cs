using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;



public enum TypeCharacterType
{
    None = -1,
    Boni = 0,
    BlueBird,
    Coco,
    Pang,
    Jeff,
    Zelly,
    Aroo,
    Alphonse,
    Mai,
	Kiri,
    ANIMAL_010,
    ANIMAL_011,
    ANIMAL_012,
    ANIMAL_013,
    ANIMAL_014,
    ANIMAL_015,
    ANIMAL_016,
    ANIMAL_017,
    ANIMAL_018,
    ANIMAL_019,
    ANIMAL_020,
    ANIMAL_021,
    ANIMAL_022,
    ANIMAL_023,
    ANIMAL_024,
    ANIMAL_025,
    ANIMAL_026,
    ANIMAL_027,
    ANIMAL_028,
    ANIMAL_029,
    ANIMAL_030,     
    ANIMAL_031,
    ANIMAL_032,
    ANIMAL_033,
    ANIMAL_034,
    ANIMAL_035,
    ANIMAL_036,
    ANIMAL_037,
    ANIMAL_038,
    ANIMAL_039,
    ANIMAL_040,
    ANIMAL_041,
    ANIMAL_042,
    ANIMAL_043,
    ANIMAL_044,
    ANIMAL_045,
    ANIMAL_046,
    ANIMAL_047,
    ANIMAL_048,
    ANIMAL_049,
    ANIMAL_100 = 100,

    COLLABO_START = 1000,
    COLLABO_BONO_Bonobono_1001,
    COLLABO_1002,
    COLLABO_1003,
    COLLABO_1004,
    COLLABO_1005,
    COLLABO_1006,
    COLLABO_1007,
    COLLABO_1008,
    COLLABO_1009,
    COLLABO_1010,
    COLLABO_1011,
    COLLABO_1012,
    COLLABO_1013,
    COLLABO_1014,
    COLLABO_1015,
    COLLABO_1016,
    COLLABO_1017,
    COLLABO_1018,
    COLLABO_1019,
    COLLABO_1020,
    COLLABO_1021,
    COLLABO_1022,
    COLLABO_1023,
    COLLABO_1024,
    COLLABO_1025,
    COLLABO_1026,
    COLLABO_1027,
    COLLABO_1028,
    COLLABO_1029,
    COLLABO_1030,
    COLLABO_1031,
    COLLABO_1032,
    COLLABO_1033,
    COLLABO_1034,
    COLLABO_1035,
    COLLABO_1036,
    COLLABO_1037,
    COLLABO_1038,
    COLLABO_1039,
    COLLABO_1040,
    COLLABO_1041,
    COLLABO_1042,
    COLLABO_1043,
    COLLABO_1044,
    COLLABO_1045,
    COLLABO_1046,
    COLLABO_1047,
    COLLABO_1048,
    COLLABO_1049,
    collabo_1050,
    collabo_1051,
    collabo_1052,
    collabo_1053,
    collabo_1054,
    collabo_1055,
    collabo_1056,
    collabo_1057,
    collabo_1058,
    collabo_1059,
    collabo_1060,
    collabo_1061,
    collabo_1062,
    collabo_1063,
    collabo_1064,
    collabo_1065,
    collabo_1066,
    collabo_1067,
    collabo_1068,
    collabo_1069,
    collabo_1070,
    collabo_1071,
    collabo_1072,
    collabo_1073,
    collabo_1074,
    collabo_1075,
    collabo_1076,
    collabo_1077,
    collabo_1078,
    collabo_1079,
    collabo_1080,
    collabo_1081,
    collabo_1082,
    collabo_1083,
    collabo_1084,
    collabo_1085,
    collabo_1086,
    collabo_1087,
    collabo_1088,
    collabo_1089,
    collabo_1090,
    collabo_1091,
    collabo_1092,
    collabo_1093,
    collabo_1094,
    collabo_1095,
    collabo_1096,
    collabo_1097,
    collabo_1098,
    collabo_1099,
    collabo_1100,
    collabo_1101,
    collabo_1102,
    collabo_1103,
    collabo_1104,
    collabo_1105,
    collabo_1106,
    collabo_1107,
    collabo_1108,
    collabo_1109,
    collabo_1110,
    collabo_1111,
    collabo_1112,
    collabo_1113,
    collabo_1114,
    collabo_1115,
    collabo_1116,
    collabo_1117,
    collabo_1118,
    collabo_1119,
    collabo_1120,
    collabo_1121,
    collabo_1122,
    collabo_1123,
    collabo_1124,
    collabo_1125,
    collabo_1126,
    collabo_1127,
    collabo_1128,
    collabo_1129,
    collabo_1130,
    collabo_1131,
    collabo_1132,
    collabo_1133,
    collabo_1134,
    collabo_1135,
    collabo_1136,
    collabo_1137,
    collabo_1138,
    collabo_1139,
    collabo_1140,
    collabo_1141,
    collabo_1142,
    collabo_1143,
    collabo_1144,
    collabo_1145,
    collabo_1146,
    collabo_1147,
    collabo_1148,
    collabo_1149,
    collabo_1150,
    collabo_1151,
    collabo_1152,
    collabo_1153,
    collabo_1154,
    collabo_1155,
    collabo_1156,
    collabo_1157,
    collabo_1158,
    collabo_1159,
    collabo_1160,
    collabo_1161,
    collabo_1162,
    collabo_1163,
    collabo_1164,
    collabo_1165,
    collabo_1166,
    collabo_1167,
    collabo_1168,
    collabo_1169,
    collabo_1170,
    collabo_1171,
    collabo_1172,
    collabo_1173,
    collabo_1174,
    collabo_1175,
    collabo_1176,
    collabo_1177,
    collabo_1178,
    collabo_1179,
    collabo_1180,
    collabo_1181,
    collabo_1182,
    collabo_1183,
    collabo_1184,
    collabo_1185,
    collabo_1186,
    collabo_1187,
    collabo_1188,
    collabo_1189,
    collabo_1190,
    collabo_1191,
    collabo_1192,
    collabo_1193,
    collabo_1194,
    collabo_1195,
    collabo_1196,
    collabo_1197,
    collabo_1198,
    collabo_1199,
    collabo_1200,

    ANIMAL_CLONE_START = 10000,
    ANIMAL_10001,
    ANIMAL_10002,
    ANIMAL_10003,
    ANIMAL_10004,
    ANIMAL_10005,
    ANIMAL_10006,
    ANIMAL_10007,
    ANIMAL_10008,
    ANIMAL_10009,
    ANIMAL_10010,
    ANIMAL_10011,
    ANIMAL_10012,
    ANIMAL_10013,
    ANIMAL_10014,
    ANIMAL_10015,
    ANIMAL_10016,
    ANIMAL_10017,
    ANIMAL_10018,
    ANIMAL_10019,
    ANIMAL_10020,
    ANIMAL_10021,
    ANIMAL_10022,
    ANIMAL_10023,
    ANIMAL_10024,
    ANIMAL_10025,
    ANIMAL_10026,
    ANIMAL_10027,
    ANIMAL_10028,
    ANIMAL_10029,
    ANIMAL_10030,
    ANIMAL_10031,
    ANIMAL_10032,
    ANIMAL_10033,
    ANIMAL_10034,
    ANIMAL_10035,
    ANIMAL_10036,
    ANIMAL_10037,
    ANIMAL_10038,
    ANIMAL_10039,
    ANIMAL_10040,
    ANIMAL_10041,
    ANIMAL_10042,
    ANIMAL_10043,
    ANIMAL_10044,
    ANIMAL_10045,
    ANIMAL_10046,
    ANIMAL_10047,
    ANIMAL_10048,
    ANIMAL_10049,
}

public enum TypeCharacterDir
{
    None,
    Right,
    Left,
}

public class Character : MonoBehaviour {

    public static Character _boni {
        get { return ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni); } }

    [System.NonSerialized]
    public Transform _transform = null;
    //[System.NonSerialized]
    public TypeCharacterType _type = TypeCharacterType.None;
    
    public GameObject _model = null;
    public Animation _animation = null;
    public ParticleSystem _runDust = null;
    public SkinnedMeshRenderer _rendererModel = null;
    public MeshRenderer _rendererShadow = null;

    public List<AudioClip> tapSound = new List<AudioClip>();

    [System.NonSerialized]
    public GameObject _model_ex = null;     // 확장 모델
    [System.NonSerialized]
    public Animation _animation_ex = null;  // 확장 모션


    [System.NonSerialized]
    public Seeker _pathFind;
    [System.NonSerialized]
    public Path _path = null;
    [System.NonSerialized]
    public int _pathIndex = 0;
    
    private int _pathDirCalculateFrame = -1;
    private Vector3 _pathDIr = Vector3.zero;
    private float _pathLen;

    private int _targetDirCalculateFrame = -1;
    private Vector3 _targetUnitDIr = Vector3.forward;
    private float _targetUnitLen;

    [System.NonSerialized]
    public AICharacter _ai = new AICharacter();

    readonly public float nextWaypointDistance = 0.1f;

    [System.NonSerialized]
    public Vector3 _targetPos = Vector3.zero; // 타겟 위치
    [System.NonSerialized]
    public Vector3 _velocity = Vector3.zero;
    [System.NonSerialized]
    public float _speed = 1f;
    [System.NonSerialized]
    public string _startMotion = null;

    public float _heightOffset = 0.0f;

    [System.NonSerialized]
    public int costumeId = 0;


    public virtual void Awake()
    {
        _transform = transform;
        _pathFind = GetComponent<Seeker>();
    }

    // 리소스를 모두 읽고 ManualyStart를 불려줘야 ai등등 움직이기 시작,, 
    public virtual void ManualyStart()
    {

    }
    public void ChangeState(AIStateID in_stateID, string in_animationName = null)
    {
        AIChangeCommand command = new AIChangeCommand();
        command._state = in_stateID;
        command._animationName = in_animationName;
        _ai.ChangeState(command);
    }
	// Use this for initialization
	void Start () {
        SetAlpha(1f);

	}
  /*  public virtual void OnPathComplete(Path _p)
    {
        if ((_path.vectorPath[_path.vectorPath.Count - 1] - _targetPos).sqrMagnitude > 0.1f)
            _targetPos = _path.vectorPath[_path.vectorPath.Count - 1];
        
    }*/
    public void StopPath()
    {
        _path = null;
        _targetPos = _transform.position;
    }

    OnPathDelegate _pathCallback = null;
    Rect pachScreenRect = new Rect(-50f, -50f, Screen.width + 100f, Screen.height + 100f);
   // List<Vector3> pppp = new List<Vector3>();

    public void StartPath(Vector3 in_target, OnPathDelegate callback = null,bool bScreenStart = false)
    {
        _targetPos = in_target;
        if (bScreenStart)
        {
            Vector3 screenPos = CameraController._instance.WorldToScreen(_transform.position);
            if (pachScreenRect.Contains(screenPos))
                bScreenStart = false;
        }

        if (bScreenStart)
        {
            _pathCallback = callback;
            _path = _pathFind.StartPath(_transform.position, in_target, CallbackScreenStartPath);
        }
        else
        {
            _path = _pathFind.StartPath(_transform.position, in_target, callback);
        }
        _pathIndex = 1;
    }
    void CallbackScreenStartPath(Path _p)
    {
        if( _path == null )
        {
            if (_p == null)
                return;
            _path = _p;
        }
        
        // 보니가 화면 밖에 있다면 루프돌면서 가까운곳으로 순간이동

      //  pppp.Clear();
        for (int i = _path.vectorPath.Count - 1; i >= 0; i--)
        {
            float moveTimer = 0f;
            Vector3 pos = _path.vectorPath[i];
            Vector3 target = _transform.position;
            if (i >= 1)
                target = _path.vectorPath[i - 1];

            Vector3 dir = _path.vectorPath[i] - target;
            dir.Normalize();

            while (true)
            {
                pos += -dir * 0.5f;
                //pppp.Add(pos);

                if ((pos - target).magnitude < 0.6f)
                    break;


                Vector3 screenPos = CameraController._instance.WorldToScreen(pos);
                if (!pachScreenRect.Contains(screenPos))
                {
                    _pathIndex = i;
                    _transform.position = pos;
                    CalculatePathDIrLen();

                    if (_pathCallback != null)
                        _pathCallback(_p);
                    return;
                }
            }
        }

        if (_pathCallback != null)
            _pathCallback(_p);
    }
   /* void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < pppp.Count; i++)
        {
            Gizmos.DrawWireSphere(pppp[i], 0.3f);
        }
    }*/
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (_path != null)
        for (int i = 0; i < _path.vectorPath.Count; i++)
        {
            Gizmos.DrawSphere(_path.vectorPath[i], 0.3f);
        }
            Gizmos.DrawSphere(_path.vectorPath[_path.vectorPath.Count - 1], 0.3f);
        Gizmos.DrawWireSphere(_targetPos, 1f);
    }*/
    public void CalculateTargetDIrLen()
    {
        if (_targetPos == Vector3.zero)
            return;

        _targetDirCalculateFrame = Time.frameCount;
        Vector3 vecSoruce = new Vector3(_transform.position.x, 0f, _transform.position.z);
        _targetUnitDIr = _targetPos - vecSoruce;
        _targetUnitLen = _targetUnitDIr.magnitude;
        _targetUnitDIr.Normalize();
    }
    public Vector3 GetTargetDir(bool in_reset = false)
    {
        if (_targetDirCalculateFrame == Time.frameCount && !in_reset)
            return _targetUnitDIr;
        CalculateTargetDIrLen();
        return _targetUnitDIr;
    }
    public float GetTargetLen(bool in_reset = false)
    {
        if (_targetDirCalculateFrame == Time.frameCount && !in_reset)
            return _targetUnitLen;
        CalculateTargetDIrLen();
        return _targetUnitLen;
    }

    public Vector3 GetPathDir(bool in_reset = false)
    {
        if (_pathDirCalculateFrame == Time.frameCount && !in_reset)
            return _pathDIr;
        CalculatePathDIrLen();
        return _pathDIr;
    }

    public float GetPathLen(bool in_reset = false)
    {
        if (_pathDirCalculateFrame == Time.frameCount && !in_reset)
            return _pathLen;
        CalculatePathDIrLen();
        return _pathLen;
    }
    public void CalculatePathDIrLen()
    {
        if (_path == null)
            return;
  //      Debug.Log("CalculatePathDIrLen " + _path.vectorPath.Count + "  " + _pathIndex);

        if (_path.vectorPath.Count <= _pathIndex)
            return;
      //  if (_path.vectorPath.Count <= _pathIndex)
      //      return;
        _pathDirCalculateFrame = Time.frameCount;

        

        _pathDIr = _path.vectorPath[_pathIndex] - _transform.position;
        _pathDIr.y = 0f;
        _pathLen = _pathDIr.magnitude;
        _pathDIr.Normalize();
    }
    
    public void MoveUpdate()
    {
        if (_ai.GetStateID() == AIStateID.eMove)
        {
            if (_path != null)
            {
                if (_path.vectorPath.Count > _pathIndex)
                {
                    float leftMoveDist = _speed * Global.deltaTimeLobby;

                    bool needPathLenRefresh = false;
                    while( GetPathLen(needPathLenRefresh) < leftMoveDist)
                    {
                        float stride = GetPathLen();
                        _transform.position = _path.vectorPath[_pathIndex];
                        _pathIndex++;
                        leftMoveDist -= stride;
                        needPathLenRefresh = true;

                        if (_pathIndex >= _path.vectorPath.Count)
                        {
                            _targetPos = _transform.position;
                            _path = null;
                            ChangeState(AIStateID.eIdle);
                            break;
                        }
                    }

                    if( _path != null )
                    {
                        if (GetPathLen(needPathLenRefresh) < nextWaypointDistance)
                        {
                            _pathIndex++;

                            if (_pathIndex >= _path.vectorPath.Count)
                            {
                                _targetPos = _transform.position;
                                _path = null;
                                ChangeState(AIStateID.eIdle);
                            }
                        }
                        else
                        {
                            _velocity = GetPathDir() ;

                            //_controller.Move(_velocity * Global.deltaTimeLobby);

                            _transform.position += _velocity * leftMoveDist;

                            if (_transform.position.y > 0 || _transform.position.y < 0)
                                _transform.position = new Vector3(_transform.position.x, 0f, _transform.position.z);
                        }
                    }
                }
                else if (_path.vectorPath.Count != 0)
                {
                    _targetPos = _transform.position;
                    _path = null;
                    ChangeState(AIStateID.eIdle);
                }
            }
            
        }

    }
    // 아직 불안
    public void SetAlpha(float in_alpha)
    {
        _rendererModel.sharedMaterial.SetColor("_MainColor", new Color(1f, 1f, 1f, in_alpha));
        _rendererShadow.material.SetColor("_MainColor", new Color(1f, 1f, 1f, in_alpha));
    }
    Collider[] colliderList;
    public List<Collider> colliderBank = new List<Collider>();
    // 컬라이드 이벤트
    public void ColliderUpdate(Vector3 in_pos)
    {
        colliderList = Physics.OverlapSphere(in_pos + new Vector3(-0.4f, 0f, 0.4f), 1f, Global.eventObjectMask);
        if (colliderList.Length > 0)
        {
            for (int j = 0; j < colliderList.Length; j++)
            {
                bool newColl = true;
                for (int i = 0; i < colliderBank.Count; i++)
                {
                    if (colliderBank[i] == colliderList[j])
                    {
                        newColl = false;
                        break;
                    }
                }
                //send
                if (newColl)
                {
                    if (colliderList[j].transform.parent != null)
                    {
                        //Debug.Log("CharacterBoni " + colliderList[j].name);
                        ObjectEvent eventObj = colliderList[j].transform.parent.GetComponent<ObjectEvent>();
                        if (eventObj != null)
                            eventObj.OnEnterCharacter(this);
                    }


                    colliderBank.Add(colliderList[j]);
                }
            }
        }
        else
            colliderBank.Clear();
    }

    float lastLobbySpeechTime = 0.0f;

    virtual public void OnTap()
    {
        ClickBlocker.Make();

        TouchAction();
        //Debug.Log("OnTap");
    }

    public void TouchAction()
    {
        var speech = ManagerSound._instance._lobbySpeechBank.GetSpeech(_type);
        if (speech != null)
        {
            MakeSpeech(speech._text, speech.audio);
        }
        else
            PlayDefaultSound();


        TouchBounce();
    }


    public void MakeSpeech(string text, AudioLobby sound, bool isAIChat = false, float speechTime = 1.5f, bool ignoreSpeechInterval = false)
    {
        if( SceneLoading.IsSceneLoading )
            return;

        bool playDefaultSound = false;
        
        if (ignoreSpeechInterval || lastLobbySpeechTime + 2.0f <= Time.realtimeSinceStartup)
        {
            lastLobbySpeechTime = Time.realtimeSinceStartup;

            if (isAIChat == false)
            {
                var lobbyChat = UILobbyChat.MakeLobbyChat(this.transform, text, speechTime);
                lobbyChat.heightOffset = GetBubbleHeightOffset();
            }
            else
            {
                var lobbyChat = UILobbyRewardChat.MakeLobbyChat(this.transform, text, speechTime);
                lobbyChat.heightOffset = GetBubbleHeightOffset();
            }

            switch (sound)
            {
                case AudioLobby.DEFAULT_AUDIO:
                    playDefaultSound = true;
                    break;
                case AudioLobby.NO_SOUND:
                    break;
                default:
                    ManagerSound.AudioPlay(sound);
                    break;
            }
        }
        else
            playDefaultSound = true;

        if (playDefaultSound)
        {
            PlayDefaultSound();
        }
    }

    void PlayDefaultSound()
    {
        if (tapSound != null && tapSound.Count > 0)
            ManagerSound.AudioPlay(tapSound[Random.Range(0, tapSound.Count)]);
    }

    public void TouchBounce()
    {
        if (_model != null)
            StartCoroutine(CoTouch());
    }

    IEnumerator CoTouch()
    {
        
        if (_model.transform.localScale.x<0f)
            _model.transform.localScale = new Vector3(-1f,1f,1f);
        else
            _model.transform.localScale = Vector3.one;

        float timer = 1f;
        while (timer > 0f)
        {
            timer -= Global.deltaTime * 1.3f;

            if (_model.transform.localScale.x < 0f)
                _model.transform.localScale = new Vector3(-1f, 1f + (timer * Mathf.Sin(20f * Time.time) * 0.2f), 1f);
            else
                _model.transform.localScale = new Vector3(1f, 1f + (timer * Mathf.Sin(20f * Time.time) * 0.2f), 1f);

            yield return null;
        }

        if (_model.transform.localScale.x < 0f)
            _model.transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            _model.transform.localScale = Vector3.one;
        yield return null;
    }

    public float GetBubbleHeightOffset()
    {
        return _heightOffset;
    }

    public void SetFallbackSound()
    {
        if (tapSound.Count == 0)
        {
            if (_type == TypeCharacterType.Coco)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_coco]);
            else if (_type == TypeCharacterType.Pang)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_peng]);
            else if (_type == TypeCharacterType.Jeff)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_jeff]);
            else if (_type == TypeCharacterType.Zelly)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_jelly]);
            else if (_type == TypeCharacterType.Aroo)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_aroo]);
            else if (_type == TypeCharacterType.Alphonse)
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.m_alphonse_hello]);
            else
                tapSound.Add(ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_Other]);
        }


    }

    //public void Sp
    // Update is called once per frame
    //	void Update () {
    //     _ai.AIUpdate();
    //}
}
