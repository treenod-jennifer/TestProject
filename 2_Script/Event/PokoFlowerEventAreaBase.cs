using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokoFlowerEventAreaBase : AreaBase, IEventLobbyObject
{
    static public PokoFlowerEventAreaBase _instance = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    [SerializeField] private string textFileName;

    public ObjectEvent _touchTarget;

    [System.NonSerialized]
    public EventObjectUI _uiEvent = null;

    [System.NonSerialized]
    public bool callbackInstalled = false;

    public override bool IsEventArea()
    {
        return true;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        InitSceneDatas();
    }
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }


    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }
    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);
        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }
	// Use this for initialization
	void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }

        SetProgressCount();
    }

    public void SetProgressCount()
    {
        if (_uiEvent != null)
        {
            var userBlossomData = ServerRepos.UserPokoFlowerEvent;
            if (userBlossomData != null)
            {
                _uiEvent._uiLevel.font = ManagerFont._instance.GetFont("LILITAONE-REGULAR");
                _uiEvent._uiLevel.text = userBlossomData.progress + "/" + userBlossomData.target;
                _uiEvent.gameObject.SetActive(ManagerPokoFlowerEvent.IsRewardExhausted() == false);
            }
        }
    }
    
    void Update()
    {
        if( !callbackInstalled && ManagerLobby._instance.IsCharacterExist(TypeCharacterType.ANIMAL_010))
        {
            //에코피 터치시 PokoFlowerPopup 오픈 기능 추가
            CharacterNPC pokoFlowerCharacter = ManagerLobby._instance.GetCharacter(TypeCharacterType.ANIMAL_010) as CharacterNPC;
            if (pokoFlowerCharacter != null)
            {
                pokoFlowerCharacter.SetTapCallBack(() =>
                    ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ManagerPokoFlowerEvent.PokoFlowerEventIndex))
                );
                callbackInstalled = true;
            }
        }
    }

    void onTouch()
    {
        if( ServerContents.PokoFlowerEvent == null || ServerRepos.UserPokoFlowerEvent == null)
        {
            return;
        }

        ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(ManagerPokoFlowerEvent.PokoFlowerEventIndex));
    }

    public GameEventType GetEventType()
    {
        return GameEventType.POKOFLOWER;
    }

    public void Invalidate()
    {
    }

    public void TriggerSetting()
    {
        if (ServerContents.PokoFlowerEvent == null || ServerRepos.UserPokoFlowerEvent == null)
        {
            return;
        }

        switch (ManagerPokoFlowerEvent.EventLevelChanged())
        {
            case 0:
                {
                    _listSceneDatas[0].state = TypeSceneState.Active;
                    if (_listSceneDatas.Count > 1)
                        _listSceneDatas[1].state = TypeSceneState.Active;

                }
                break;
            case 1:
                {
                    _listSceneDatas[0].state = TypeSceneState.Wait;
                    if (_listSceneDatas.Count > 1)
                        _listSceneDatas[1].state = TypeSceneState.Wait;
                }
                break;
            case 2:
                {
                    _listSceneDatas[0].state = TypeSceneState.Finish;
                    if (_listSceneDatas.Count > 1)
                        _listSceneDatas[1].state = TypeSceneState.Wait;
                }
                break;


        }

    }
}
