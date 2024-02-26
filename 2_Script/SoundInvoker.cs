using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInvoker : MonoBehaviour {

    [SerializeField] float delay;
    [SerializeField] AudioLobby audioLobby = AudioLobby.NO_SOUND;
    [SerializeField] AudioInGame audioIngame = AudioInGame.APPLE;

    [SerializeField] bool playIngameSound = true;

    // Use this for initialization
    void Start () {
       

    }

    private void OnEnable()
    {
        StartCoroutine(Play());
        
    }

    IEnumerator Play()
    {
        yield return new WaitForSeconds(delay);
        if (playIngameSound)
            ManagerSound.AudioPlay(audioIngame);
        else
            ManagerSound.AudioPlay(audioLobby);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
