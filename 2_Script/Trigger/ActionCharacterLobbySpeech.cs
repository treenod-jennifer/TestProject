using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LobbySpeech
{
    public string _text = "";
    public AudioLobby audio = AudioLobby.DEFAULT_AUDIO;
}

public class ActionCharacterLobbySpeech : ActionBase
{
    [SerializeField]
    TypeCharacterType _type = TypeCharacterType.Boni;

    [SerializeField]
    List<LobbySpeech> _speechData = new List<LobbySpeech>();

    [SerializeField]
    int clearMissionMin = 0;

    [SerializeField]
    int clearMissionMax = 0;

    [System.NonSerialized]
    public Dictionary<string, string> _stringBank = null;
    

    public void Awake()
    {
    }

    public override void DoAction()
    {
        if (_stateType == TypeTriggerState.BehaviorGlobal)
            _stringBank = Global._instance._stringData;
        else
        {
            if (transform.parent.parent.parent.GetComponent<AreaBase>() != null)
                _stringBank = transform.parent.parent.parent.GetComponent<AreaBase>()._stringData;
        }

        LobbySpeechData speechData = new LobbySpeechData() { missionMin = this.clearMissionMin, missionMax = this.clearMissionMax };

        for(int i = 0; i < _speechData.Count; ++i)
        {
            speechData.speeches.Add(new LobbySpeech() { _text = GetString(_speechData[i]._text), audio = _speechData[i].audio });
        }
        ManagerSound._instance._lobbySpeechBank.SetLobbySpeechData(this._type, speechData);
    }
    public string GetString(string in_key)
    {
        if (!_stringBank.ContainsKey(in_key))
            return in_key + ": string empty";
        else
            return _stringBank[in_key];
    }
    
}

public class LobbySpeechData
{
    public int missionMin = 0;
    public int missionMax = 0;

    public List<LobbySpeech> speeches = new List<LobbySpeech>();
}



public class LobbySpeechBank
{
    Dictionary<int, LobbySpeechData> bank = new Dictionary<int, LobbySpeechData>();

    public LobbySpeech GetSpeech(TypeCharacterType t)
    {
        if (!bank.ContainsKey((int)t))
            return null;

        var speechData = bank[(int)t];
        if (speechData == null)
            return null;

        if (speechData.missionMin == speechData.missionMax && speechData.missionMin == 0)
        {
            // OK
        }
        else
        {
            if (speechData.missionMin != 0 && ManagerData._instance._missionData[speechData.missionMin].state != TypeMissionState.Clear)
                return null;

            if (speechData.missionMax != 0 && ManagerData._instance._missionData[speechData.missionMax].state == TypeMissionState.Clear)
                return null;
        }

        return speechData.speeches[Random.Range(0, speechData.speeches.Count)];
    }

    public void SetLobbySpeechData(TypeCharacterType t, LobbySpeechData data)
    {
        if (bank.ContainsKey((int)t))
            bank.Remove((int)t);
        bank.Add((int)t, data);
    }
    
}
