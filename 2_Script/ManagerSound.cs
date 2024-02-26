using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AudioPriority
{
    Top,
    Middle,     
    Low,
};


public class ManagerSound : MonoBehaviour
{
    public static ManagerSound _instance = null;

    public AudioBankLobby _bankLobby;
    public AudioBankInGame _bankInGame;

    // 자주 쓰는 BGM은 메모리 고정
    public AudioSource _bgmInGame;
    public AudioSource _bgmLobby;

    [System.NonSerialized]
    public float _lastAudioPlayTime = 0.0f;
    [System.NonSerialized]
    public List<AudioClip> _listAudioClip = new List<AudioClip>();

    [System.NonSerialized]
    public float _listPangTime = 0.0f;
    public List<AudioClip> _listPangAudioClip = new List<AudioClip>();

    Dictionary<AudioInGame, List<float>> _listClassOfClip = new Dictionary<AudioInGame, List<float>>();

    public LobbySpeechBank _lobbySpeechBank = new LobbySpeechBank();

    void Awake()
    {
        _instance = this;
    }

	void Start () {

        for (int i = 0; i < (int)AudioInGame.Count; i++)
            _listClassOfClip.Add((AudioInGame)i, new List<float>());
	}

    static public void AudioPlay(AudioClip in_audio, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect)
            return;

        AudioSource.PlayClipAtPoint(in_audio, Vector3.zero);
    }
    static public void AudioPlayMany(AudioInGame in_audio)
    {

        if (!Global._optionSoundEffect)
            return;

        List<float> list = ManagerSound._instance._listClassOfClip[in_audio];
        if (list.Count == 0)
            list.Add(Time.time);
        else
        {
            // 제일 최근 추가한 같은 그룹의 오디오를 찾아서 매우 최근에 추가된 이력이 없다면 추가
            if(Time.time - list[list.Count - 1]>0.2f)
                list.Add(Time.time + Random.value * 0.2f);
        }
    }
    static public void AudioPlayClass(AudioInGame in_audio, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect)
            return;

        AudioClip audioData = ManagerSound._instance._bankInGame._audioList[(int)in_audio];
        if (audioData == null)
            return;

        if (Time.time - ManagerSound._instance._lastAudioPlayTime < 0.07f && ManagerSound._instance._listAudioClip.Count == 0)
        {
            ManagerSound._instance._listAudioClip.Add(audioData);
        }
        else
        {
            if (ManagerSound._instance._listAudioClip.Count == 0)
            {
                ManagerSound._instance._listAudioClip.Add(audioData);
            }
            else if (ManagerSound._instance._listAudioClip[ManagerSound._instance._listAudioClip.Count - 1] != audioData)
            {
                ManagerSound._instance._listAudioClip.Add(audioData);
            }
        }
    }

    static public void AudioPlay(AudioInGame in_audio, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect)
            return;
        if (ManagerSound._instance._bankInGame._audioList[(int)in_audio] == null)
        {
            Debug.Log(in_audio + " 사운드가 없어요.");
            return;
        }

        AudioSource.PlayClipAtPoint(ManagerSound._instance._bankInGame._audioList[(int)in_audio], Vector3.zero);
    }

    static public void AudioPlay(AudioLobby in_audio, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect)
            return;

        if (ManagerSound._instance._bankLobby._audioList[(int)in_audio] == null)
        {
            Debug.Log(in_audio + " 사운드가 없어요.");
            return;
        }
        AudioSource.PlayClipAtPoint(ManagerSound._instance._bankLobby._audioList[(int)in_audio], Vector3.zero);
    }

    static public void AudioPlayPang(int count)
    {
        if (!Global._optionSoundEffect)return;

        //카운트 2~4까지 
        //  _listPangAudioClip
        /*
        0.22
        0.22
        0.2 
        0.2
        0.15
        0.18
        0.16
        0.14
        0.15
        0.33
        */
        ManagerSound._instance._listPangAudioClip.Clear();

        if (count > 10) count = 10;

        for (int i = 0; i < count; i++)
        {
            ManagerSound._instance._listPangAudioClip.Add(ManagerSound._instance._bankInGame._audioList[40 + i]);
        }
    }

    public void GetAudioFTP()
    {
        //사운드뱅크더미만들기
        GameObject lobbyDummy = Instantiate(_bankLobby.gameObject);
        lobbyDummy.transform.parent = gameObject.transform;

        _bankLobby = lobbyDummy.GetComponent<AudioBankLobby>();

        GameObject inGameDummy = Instantiate(_bankInGame.gameObject);
        inGameDummy.transform.parent = gameObject.transform;
        _bankInGame = inGameDummy.GetComponent<AudioBankInGame>();

        StartCoroutine(CoDownAllFile());
    }

    IEnumerator CoDownAllFile()
    {
        for(int i = 0; i < (int)AudioInGame.Count; i++)
        {
            doneDownAudio = false;
            StartCoroutine(CoDownLoadInGameAudio((AudioInGame)i));
            yield return null;

            while (!doneDownAudio)
                yield return null;
        }

        for (int i = 0; i < (int)AudioLobby.Count; i++)
        {
            doneDownAudio = false;
            StartCoroutine(CoDownLoadLobbyAudio((AudioLobby)i));
            yield return null;

            while (!doneDownAudio)
                yield return null;
        }

        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popupWarning.InitSystemPopUp("System", "사운드 받음", false, null);

        yield return null;
    } 

    bool doneDownAudio = false;

    IEnumerator CoDownLoadInGameAudio(AudioInGame typeName)
    {
        WWW www = new WWW(Global._cdnAddress + "Sound/" + typeName.ToString() + ".wav");
        yield return www;

        while (!www.isDone)
            yield return null;

        if(www.error == null)
        {
            ManagerSound._instance._bankInGame._audioList[(int)typeName] = www.GetAudioClip(false);
        }
        doneDownAudio = true;
        yield return null;
    }

    IEnumerator CoDownLoadLobbyAudio(AudioLobby typeName)
    {
        WWW www = new WWW(Global._cdnAddress + "Sound/" + typeName.ToString() + ".wav");
        yield return www;

        while (!www.isDone)
            yield return null;

        if (www.error == null)
        {
            ManagerSound._instance._bankLobby._audioList[(int)typeName] = www.GetAudioClip(false);
        }
        doneDownAudio = true;
        yield return null;
    }


    void Update ()
    {
        if (_listAudioClip.Count > 0)
        {
            if (Time.time - _lastAudioPlayTime >= 0.07f)
            {
                if (_listAudioClip[0] != null)
                {
                    AudioSource.PlayClipAtPoint(_listAudioClip[0], Vector3.zero);
                    _lastAudioPlayTime = Time.time;
                    _listAudioClip.RemoveAt(0);
                }
            }
        }

        if (_listPangAudioClip.Count > 0)
        {
            if (Time.time - _listPangTime >= 0.1f)
            {
                AudioSource.PlayClipAtPoint(_listPangAudioClip[0], Vector3.zero);
                _listPangTime = Time.time;
                _listPangAudioClip.RemoveAt(0);
            }
        }

        foreach (var item in ManagerSound._instance._listClassOfClip)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                if (item.Value[i] < Time.time)
                {
                    AudioSource.PlayClipAtPoint(ManagerSound._instance._bankInGame._audioList[(int)item.Key], Vector3.zero);
                    item.Value.RemoveAt(0);
                    break;
                }
            }
        }

	}


    public void PlayBGM()
    {
        if (!Global._optionBGM)
            return;

        if (SceneManager.GetActiveScene().name == "InGame")
        {
            if(EditManager.instance == null)
            _bgmInGame.Play();
        }
        else
            _bgmLobby.Play();
    }
    public void SetTimeBGM(float in_time)
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            _bgmInGame.time = in_time;
        else
            _bgmLobby.time = in_time;
    }
    public void PauseBGM()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            _bgmInGame.Pause();
        else
            _bgmLobby.Pause();
    }
    public void UnPauseBGM()
    {
        if (!Global._optionBGM)
            return;

        if (SceneManager.GetActiveScene().name == "InGame")
            _bgmInGame.UnPause();
        else
            _bgmLobby.UnPause();
    }
    public void StopBGM()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            _bgmInGame.Stop();
        else
            _bgmLobby.Stop();
    }
    public void VolumeBGM(float in_volume )
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            _bgmInGame.volume = in_volume;
        else
            _bgmLobby.volume = in_volume;
    }


}
