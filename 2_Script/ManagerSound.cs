using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum AudioPriority
{
    Top,
    Middle,     
    Low,
};

public enum IngameBgmEnum
{
    INGAME,
    INGAME_ADV,
    INGAME_COIN,
    INGAME_COIN_FEVER,
    INGAME_COIN_HURRY,
    INGAME_WORLD_RANK,    

    COUNT,
}

public enum LobbyBgmEnum
{
    LOBBY,
    LOBBY_ADV,
    LOBBY_MOLE,
    LOBBY_SPACE,
    COUNT,
}


public class ManagerSound : MonoBehaviour
{
    public static ManagerSound _instance = null;

    public AudioBankLobby _bankLobby;
    public AudioBankInGame _bankInGame;

    [EnumNamedArray(typeof(IngameBgmEnum))]
    public List<AudioSource> _ingameBgmList = new List<AudioSource>((int)IngameBgmEnum.COUNT);
    [EnumNamedArray(typeof(LobbyBgmEnum))]
    public List<AudioSource> _lobbyBgmList = new List<AudioSource>((int)LobbyBgmEnum.COUNT);

    [EnumNamedArray(typeof(IngameBgmEnum))]
    public List<AudioClip> _ingameBgmDefaultClipList = new List<AudioClip>((int)IngameBgmEnum.COUNT);
    [EnumNamedArray(typeof(LobbyBgmEnum))]
    public List<AudioClip> _lobbyBgmDefaultClipList = new List<AudioClip>((int)LobbyBgmEnum.COUNT);

    static AudioSource effSoundSource = null;

    [System.NonSerialized]
    SoundPack externalSoundPack = null;

    [System.NonSerialized]
    public float _lastAudioPlayTime = 0.0f;
    [System.NonSerialized]
    public List<AudioClip> _listAudioClip = new List<AudioClip>();

    [System.NonSerialized]
    public float _listPangTime = 0.0f;
    public List<AudioClip> _listPangAudioClip = new List<AudioClip>();

    Dictionary<AudioInGame, List<float>> _listClassOfClip = new Dictionary<AudioInGame, List<float>>();

    public LobbySpeechBank _lobbySpeechBank = new LobbySpeechBank();

    static bool muted = false;

    void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if(_instance == this)
            _instance = null;
    }

    void Start () {
        effSoundSource = gameObject.AddMissingComponent<AudioSource>();

        for (int i = 0; i < (int)AudioInGame.Count; i++)
            _listClassOfClip.Add((AudioInGame)i, new List<float>());
	}

    static public void AudioPlay(AudioClip in_audio, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect || muted)
            return;

        effSoundSource.PlayOneShot(in_audio);
        //AudioSource.PlayClipAtPoint(in_audio, Vector3.zero);
    }
    static public void AudioPlayMany(AudioInGame in_audio)
    {

        if (!Global._optionSoundEffect || muted)
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
        if (!Global._optionSoundEffect || muted)
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

    static public void AudioPlay(AudioInGame in_audio, float vol = 1.0f, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect || muted)
            return;
        var clip = ManagerSound._instance.GetAudioClip(in_audio);
        if (clip == null)
        {
            Debug.Log(in_audio + " 사운드가 없어요.");
            return;
        }
        effSoundSource.PlayOneShot(clip, vol);
        //AudioSource.PlayClipAtPoint(ManagerSound._instance._bankInGame._audioList[(int)in_audio], Vector3.zero, vol);
    }

    static public void AudioPlay(AudioLobby in_audio, float vol = 1.0f, AudioPriority in_priority = AudioPriority.Middle)
    {
        if (!Global._optionSoundEffect || muted)
            return;

        var clip = ManagerSound._instance.GetAudioClip(in_audio);
        if (clip == null)
        {
            Debug.Log(in_audio + " 사운드가 없어요.");
            return;
        }
        effSoundSource.PlayOneShot(clip, vol);
        //AudioSource.PlayClipAtPoint(ManagerSound._instance._bankLobby._audioList[(int)in_audio], Vector3.zero, vol);
    }

    public AudioClip GetAudioClip(AudioInGame in_audio)
    {
        List<AudioClip> targetClipList;

        if (ManagerSound._instance.externalSoundPack != null &&
            ManagerSound._instance.externalSoundPack.IngameSound.Count > (int)in_audio &&
            ManagerSound._instance.externalSoundPack.IngameSound[(int)in_audio] != null)
        {
            targetClipList = ManagerSound._instance.externalSoundPack.IngameSound;
        }
        else if (ManagerSound._instance._bankInGame._audioList.Count > (int)in_audio)
        {
            targetClipList = ManagerSound._instance._bankInGame._audioList;
        }
        else
        {
            return null;
        }

        return targetClipList[(int)in_audio];
    }

    public AudioClip GetAudioClip(AudioLobby in_audio)
    {
        List<AudioClip> targetClipList;

        int test = (int)in_audio;

        if (ManagerSound._instance.externalSoundPack != null &&
            ManagerSound._instance.externalSoundPack.LobbySound.Count > (int)in_audio &&
            ManagerSound._instance.externalSoundPack.LobbySound[(int)in_audio] != null)
        {
            targetClipList = ManagerSound._instance.externalSoundPack.LobbySound;
        }
        else if (ManagerSound._instance._bankLobby._audioList.Count > (int)in_audio)
        {
            targetClipList = ManagerSound._instance._bankLobby._audioList;
        }
        else
        {
            return null;
        }

        return targetClipList[(int)in_audio];
    }

    static public void AudioPlayPang(int count)
    {
        if (!Global._optionSoundEffect || muted) return;

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
            var clip = ManagerSound._instance.GetAudioClip((AudioInGame)(40 + i));

            ManagerSound._instance._listPangAudioClip.Add(clip);
        }
    }

    


    void Update ()
    {
        if (_instance == null)
            return;
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
                    AudioSource.PlayClipAtPoint(GetAudioClip(item.Key), Vector3.zero);
                    item.Value.RemoveAt(0);
                    break;
                }
            }
        }

	}

    public IEnumerator InitSoundPack()
    {
        if( this.externalSoundPack != null && externalSoundPack.packIndex != ServerRepos.LoginCdn.soundPackVer)
            RollbackSoundPack();

        if( ServerRepos.LoginCdn.soundPackVer > 0 && externalSoundPack == null)
        {
            if (Global.LoadFromInternal)
            {
                LoadFromInternal();
            }
            else
            {
                yield return LoadFromBundle();
            }

            ApplySoundPack();
        }
    }

    public void LoadFromInternal()
    {
        string path = $"Assets/5_OutResource/SoundPacks/soundpack_{ServerRepos.LoginCdn.soundPackVer}/soundPack.prefab";
#if UNITY_EDITOR
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return;
        }

        this.externalSoundPack = obj.GetComponent<SoundPack>();
#endif
    }

    public IEnumerator LoadFromBundle()
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string name = $"soundpack_{ServerRepos.LoginCdn.soundPackVer}";
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject obj = assetBundle.LoadAsset<GameObject>("soundPack");
                this.externalSoundPack = obj.GetComponent<SoundPack>();
            }
        }
        NetworkLoading.EndNetworkLoading();
    }



    public void ResetInGameBGMClip()
    {
        _ingameBgmList[(int)IngameBgmEnum.INGAME].clip = _ingameBgmDefaultClipList[(int)IngameBgmEnum.INGAME];
    }
   
    AudioSource GetCurrentAudioSource()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
        {
            switch(Global.GameSoundType)
            {
                case GameType.ADVENTURE:
                case GameType.ADVENTURE_EVENT:
                    return _ingameBgmList[(int)IngameBgmEnum.INGAME_ADV];
                case GameType.COIN_BONUS_STAGE:
                    {
                        if(  ManagerBlock.instance.isFeverTime() )
                        {
                            return _ingameBgmList[(int)IngameBgmEnum.INGAME_COIN_FEVER];

                        }
                        else
                        {
                            if (ManagerBlock.instance != null)
                            {
                                if( ManagerBlock.instance.bgmHurryOn)
                                    return _ingameBgmList[(int)IngameBgmEnum.INGAME_COIN_HURRY];

                            }
                            return _ingameBgmList[(int)IngameBgmEnum.INGAME_COIN];
                        }
                    }
                case GameType.WORLD_RANK:
                    return _ingameBgmList[(int)IngameBgmEnum.INGAME_WORLD_RANK];
                default:
                    return _ingameBgmList[(int)IngameBgmEnum.INGAME];

            }
        }
        else
        {
            if (Global.GameSoundType == GameType.ADVENTURE || Global.GameSoundType == GameType.ADVENTURE_EVENT)
                return _lobbyBgmList[(int)LobbyBgmEnum.LOBBY_ADV];
            else if (Global.GameSoundType == GameType.MOLE_CATCH && UIPopupMoleCatch._instance != null)
                return _lobbyBgmList[(int)LobbyBgmEnum.LOBBY_MOLE];
            else if (Global.GameSoundType == GameType.SPACE_TRAVEL && UIPopUpSpaceTravel.instance != null)
                return _lobbyBgmList[(int)LobbyBgmEnum.LOBBY_SPACE];
            else
                return _lobbyBgmList[(int)LobbyBgmEnum.LOBBY];
        }
    }



    public void PlayBGM()
    {
        if (!Global._optionBGM)
            return;

        var audioSrc = GetCurrentAudioSource();
        audioSrc.time = 0;
        audioSrc.Play();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip != null)
        {
            if (Global.GameSoundType == GameType.MOLE_CATCH)
            {
                _lobbyBgmList[(int)LobbyBgmEnum.LOBBY_MOLE].clip = clip;
            }

            if (Global.GameSoundType == GameType.NORMAL)
            {
                _lobbyBgmList[(int)LobbyBgmEnum.LOBBY].clip = clip;
            }
        }
        PlayBGM();
    }

    public void SetTimeBGM(float in_time)
    {
        var audioSrc = GetCurrentAudioSource();
        audioSrc.time = in_time;
    }
    public void PauseBGM()
    {
        var audioSrc = GetCurrentAudioSource();
        audioSrc.Pause();
    }
    public void UnPauseBGM()
    {
        if (!Global._optionBGM)
            return;

        var audioSrc = GetCurrentAudioSource();
        audioSrc.UnPause();
    }
    public void StopBGM()
    {
        var audioSrc = GetCurrentAudioSource();
        audioSrc.Stop();
    }
    public void VolumeBGM(float in_volume )
    {
        var audioSrc = GetCurrentAudioSource();
        audioSrc.volume = in_volume;
    }

    public void Mute()
    {
        muted = true;
        effSoundSource.Stop();
    }

    public void UnMute()
    {
        muted = false;
    }
    
    public static void StopEffectSound()
    {
        effSoundSource.Stop();
    }

    public void ApplySoundPack()
    {
        for( int i = 0; i < externalSoundPack.IngameBGMList.Count; ++i)
        {
            if( i < this._ingameBgmList.Count && externalSoundPack.IngameBGMList[i] != null)
            {
                _ingameBgmList[i].clip = externalSoundPack.IngameBGMList[i];
            }

        }

        for (int i = 0; i < externalSoundPack.LobbyBgmList.Count; ++i)
        {
            if (i < this._lobbyBgmList.Count && externalSoundPack.LobbyBgmList[i])
            {
                _lobbyBgmList[i].clip = externalSoundPack.LobbyBgmList[i];
            }

        }

        ManagerCharacter._instance.ReattachDefaultCharacterSound();
    }

    public void RollbackSoundPack()
    {
        externalSoundPack = null;

        for (int i = 0; i < _ingameBgmList.Count; ++i)
        {
            if (i < this._ingameBgmDefaultClipList.Count)
            {
                _ingameBgmList[i].clip = _ingameBgmDefaultClipList[i];
            }

        }

        for (int i = 0; i < _lobbyBgmList.Count; ++i)
        {
            if (i < this._lobbyBgmDefaultClipList.Count)
            {
                _lobbyBgmList[i].clip = _lobbyBgmDefaultClipList[i];
            }

        }

        ManagerCharacter._instance.ReattachDefaultCharacterSound();

    }


}
