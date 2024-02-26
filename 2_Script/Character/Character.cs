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
                    if (GetPathLen() < nextWaypointDistance)
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
                        _velocity = GetPathDir() * _speed;

                        //_controller.Move(_velocity * Global.deltaTimeLobby);

                        _transform.position += _velocity * Global.deltaTimeLobby;

                        if (_transform.position.y > 0 || _transform.position.y < 0)
                            _transform.position = new Vector3(_transform.position.x, 0f, _transform.position.z);
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

    public void OnTap()
    {
        bool playDefaultSound = false;

        var speech = ManagerSound._instance._lobbySpeechBank.GetSpeech(_type);
        if (speech != null && lastLobbySpeechTime + 2.0f <= Time.realtimeSinceStartup)
        {
            lastLobbySpeechTime = Time.realtimeSinceStartup;

            var lobbyChat = UILobbyChat.MakeLobbyChat(this.transform, speech._text, 1.5f);
            lobbyChat.heightOffset = GetBubbleHeightOffset();

            switch (speech.audio)
            {
                case AudioLobby.DEFAULT_AUDIO:
                    playDefaultSound = true;
                    break;
                case AudioLobby.NO_SOUND:
                    break;
                default:
                    ManagerSound.AudioPlay(speech.audio);
                    break;
            }
        }
        else
            playDefaultSound = true;

        if( playDefaultSound)
        {
            if (tapSound != null && tapSound.Count > 0)
                ManagerSound.AudioPlay(tapSound[Random.Range(0, tapSound.Count)]);
        }

        if (_model != null)
            StartCoroutine(CoTouch());
        //Debug.Log("OnTap");
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
        switch (this._type)
        {
            case TypeCharacterType.Alphonse:
                return 2.0f;
        }
        return 0.0f;
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
