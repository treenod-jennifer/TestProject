using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[System.Serializable]
public class SpeechBubbleList
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public string _text = "";
    public float _dulation = 1.5f;
    public float _wait = 0.5f;
    public bool _playSound = false;
} 


public class ActionCharacterSpeechBubble : ActionBase
{
    public bool _abortAction = false;
    public bool _conditionWaitOn = false;
    [SerializeField]
    bool _randomSpeechMode = false;
    public float _startDelay = 0.0f;
    public List<SpeechBubbleList> _stringData = new List<SpeechBubbleList>();

    //public  string _strChat = "";
    [System.NonSerialized]
    public Dictionary<string, string> _stringBank = null;

    UILobbyChat _lobbyChat = null;

    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }

    public override void AbortAction()
    {
        this._abortAction = true;

        if (_lobbyChat != null)
            _lobbyChat._dulation = 0f;
    }

    public override void DoAction()
    {
        _abortAction = false;
        if (_stateType == TypeTriggerState.BehaviorGlobal)
            _stringBank = Global._instance._stringData;
        else
        {
            if (transform.parent.parent.parent.GetComponent<AreaBase>() != null)
                _stringBank = transform.parent.parent.parent.GetComponent<AreaBase>()._stringData;
        }

        //base.DoAction();

        //Debug.Log("ActionCharacterSpeechBubble _ DoAction"  + transform.parent.parent.name);

        StartCoroutine(CoDoAction());
        
    }
    public string GetString(string in_key)
    {
        if (!_stringBank.ContainsKey(in_key))
            return in_key + ": string empty";
        else
            return _stringBank[in_key];
    }
    IEnumerator CoDoAction()
    {
        if (_abortAction)
            yield break;

        timer = _startDelay;
        while (timer > 0f)
        {
            if (_abortAction)
            {
                yield break;
            }

            timer -= Global.deltaTimeLobby;
            yield return null;
        }

        if ( _randomSpeechMode )
        {
            var selected = _stringData[Random.Range(0, _stringData.Count)];

            float localTimer = selected._dulation + selected._wait;
            Character character = ManagerLobby._instance.GetCharacter(selected._type);

            _lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, GetString(selected._text), selected._dulation);
            _lobbyChat.heightOffset = character.GetBubbleHeightOffset();
            if (selected._playSound)
                AudioPlay(selected._type);
            while (localTimer > 0f)
            {
                if (_abortAction)
                {
                    break;
                }

                localTimer -= Global.deltaTimeLobby;
                yield return null;
            }

        }
        else
        {
            for (int i = 0; i < _stringData.Count; i++)
            {
                float localTimer = _stringData[i]._dulation + _stringData[i]._wait;
                Character character = ManagerLobby._instance.GetCharacter(_stringData[i]._type);

                _lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, GetString(_stringData[i]._text), _stringData[i]._dulation);
                _lobbyChat.heightOffset = character.GetBubbleHeightOffset();
                if (_stringData[i]._playSound)
                    AudioPlay(_stringData[i]._type);
                while (localTimer > 0f)
                {
                    if (_abortAction)
                    {
                        break;
                    }

                    localTimer -= Global.deltaTimeLobby;
                    yield return null;
                }
            }
        }
        
        bActionFinish = true;
        yield return null;
    }

    void AudioPlay(TypeCharacterType in_ch)
    {
        AudioClip a = ManagerCharacter._instance.GetChatSound(in_ch);
        if (a != null)
        {
            ManagerSound.AudioPlay(a);
        }
    }
}
