using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : AreaBase
{
    public TextAsset _naviData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    [SerializeField] private string textFileName;

    public Transform _defaultCharacterPosition = null;
    public Vector2 _defaultCharacterOffset;
    public Transform _defaultCameraPosition = null;
    public Vector2 _defaultCameraOffset;
    public float _defaultZoom = 0f;

    public List<TriggerScene> _extendTrigger = new List<TriggerScene>();

    public static int globalAreaOrder = 1;
    [System.NonSerialized]
    public int areaOrder = 0;

    public ObjectEvent[] _touchTarget;

    [SerializeField] public List<GameObject> listSceneObject;

    void Awake()
    {
        InitSceneDatas();

        for (int i = 0; i < _extendTrigger.Count; i++)
        {
            if (_extendTrigger[i] != null)
            {
                _extendTrigger[i]._triggerWait.gameObject.SetActive(false, true);
                _extendTrigger[i]._triggerWakeUp.gameObject.SetActive(false, true);
                _extendTrigger[i]._triggerActive.gameObject.SetActive(false, true);
                _extendTrigger[i]._triggerFinish.gameObject.SetActive(false, true);
            }
        }

        var lobbyFocus = gameObject.GetComponent<LobbyEntryFocus>();
        if (lobbyFocus == null)
        {
            if(_defaultCameraPosition != null || _defaultCharacterPosition != null)
            {
                lobbyFocus = gameObject.AddMissingComponent<LobbyEntryFocus>();
                lobbyFocus.focusData = new FocusData()
                {
                    camTransform = _defaultCameraPosition,
                    camOffset = _defaultCameraOffset,
                    charPos = _defaultCharacterPosition,
                    charOffset = _defaultCharacterOffset,
                    zoom = _defaultZoom
                };

                lobbyFocus.bubbleDisplayAt = LobbyEntryFocus.BubbleDisplayOption.TOP_PRIORITY_EVERY;
                lobbyFocus.focusPriority = 1000 * areaOrder;
            }
            areaOrder = globalAreaOrder;
            globalAreaOrder++;
        }
    }

    void Start()
    {
        if (_touchTarget != null)
        {
            for (int i = 0; i < _touchTarget.Length; i++)
            {
                _touchTarget[i]._onTouch = (() => onTouch());
            }
        }

        if (_naviData != null && LobbyBehavior._instance != null)
            AstarPath.active.data.DeserializeGraphs(_naviData.bytes);

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }

        //오픈된 에리어 정보 추가
        if (openAreaIndex > -1 && ManagerLobby._instance.listOpenAreaIndex.FindIndex(x => x == openAreaIndex) == -1)
            ManagerLobby._instance.listOpenAreaIndex.Add(openAreaIndex);
    }

    public void SetAllTriggerState(TypeSceneState in_state)
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
                _listSceneDatas[i].state = in_state;
        }
    }

    void onTouch()
    {
        EventLobbyObjectOption.NotEventPopup();
    }

    public override void TriggerStart()
    {
        TriggerStart_Internal();
    }

    void OnDrawGizmosSelected()
    {
        if (_defaultCharacterPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_defaultCharacterPosition.position + new Vector3(_defaultCharacterOffset.x, _defaultCharacterOffset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }

        if (_defaultCameraPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_defaultCameraPosition.position + new Vector3(_defaultCameraOffset.x, _defaultCameraOffset.y), 0.5f);
        }
    }
    //public 
}
