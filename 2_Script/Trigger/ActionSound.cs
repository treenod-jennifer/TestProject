using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionSound : ActionBase
{
    public AudioLobby _clip = AudioLobby.CreateMission;
    public bool _loop = false;
    public float _loopDuration = 1f;
    public AudioClip _dataClip = null;

    public override void DoAction()
    {
        if (!Global._optionSoundEffect)
            return;

        if (_loop)
            StartCoroutine(CoAction());
        else
            ManagerSound.AudioPlay(_clip);
    }
    IEnumerator CoAction()
    {
        GameObject obj = new GameObject("sound");
        AudioSource audio = obj.AddComponent<AudioSource>();
        if (_dataClip == null)
            audio.clip = ManagerSound._instance._bankLobby._audioList[(int)_clip];
        else
            audio.clip = _dataClip;
        audio.loop = true;
        audio.Play();

        obj.transform.parent = transform;
        obj.transform.position = Vector3.zero;


        float timer = _loopDuration;
        while (timer>=0f)
        {
            timer -= Global.deltaTimeLobby;

            yield return null;
        }
        audio.Stop();
        yield return null;
        Destroy(obj);
    }
}
