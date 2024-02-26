using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPack : MonoBehaviour
{
    public int packIndex;

    [SerializeField][EnumNamedArray(typeof(IngameBgmEnum))]
    public List<AudioClip> IngameBGMList = new List<AudioClip>((int)IngameBgmEnum.COUNT);
    [SerializeField][EnumNamedArray(typeof(LobbyBgmEnum))]
    public List<AudioClip> LobbyBgmList = new List<AudioClip>((int)LobbyBgmEnum.COUNT);
    [SerializeField][EnumNamedArray(typeof(AudioInGame))]
    public List<AudioClip> IngameSound = new List<AudioClip>((int)AudioInGame.Count);
    [SerializeField][EnumNamedArray(typeof(AudioLobby))]
    public List<AudioClip> LobbySound = new List<AudioClip>((int)AudioLobby.Count);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
