using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class CharacterMotionblurData
{
    public float _duration = 1f;
    public int _frameIndex = 0;
    public Vector3 _start;
    public Vector3 _end;
    public float _angle = 0f;
    public bool _invert = false;
}
public class ActionCharacterMotionblur : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public Transform _rootTransform;
    public float _size = 10f;
    public List<CharacterMotionblurData> _data = new List<CharacterMotionblurData>();
    public ParticleSystem _particle = null;
    public Vector3 _particleOffset = Vector3.zero;
    public bool _enableColliderEvent = false;
    Character character = null;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        StartCoroutine(CoAnimation());
    }
    IEnumerator CoAnimation()
    {

        GameObject sprite = Instantiate<GameObject>(ManagerLobby._instance._objCharacterMotionblur[(int)_type]); 
       /* if (_type == TypeCharacterType.Boni)
            sprite = Instantiate<GameObject>(ManagerLobby._instance._objCharacterMotionblurBoni);
        else if (_type == TypeCharacterType.BlueBird)
            sprite = Instantiate<GameObject>(ManagerLobby._instance._objCharacterMotionblurBlueBird);
        else if (_type == TypeCharacterType.Coco)
            sprite = Instantiate<GameObject>(ManagerLobby._instance._objCharacterMotionblurCoco);*/
        character = ManagerLobby._instance.GetCharacter(_type);


        sprite.transform.parent = transform;
        sprite.transform.rotation = Global.billboardRotation;
        sprite.transform.localScale = Vector3.one * _size;

        Material mat = sprite.GetComponent<MeshRenderer>().material;
        if (_particle != null)
        {
            _particle.gameObject.SetActive(true);
            _particle.Play(true);
        }

        for (int i = 0; i < _data.Count; i++)
        {
            float timer = _data[i]._duration;
            if (_data[i]._invert)
                sprite.transform.localScale = new Vector3(-1f, 1f, 1f) * _size;
            else
                sprite.transform.localScale = Vector3.one * _size;
            sprite.transform.rotation = Quaternion.Euler(50f, -45f, _data[i]._angle);
            while (timer>0f)
            {
                timer -= Global.deltaTimeLobby;

                mat.SetFloat("_Frame", (int)ManagerLobby._instance.motionblurCurveFrameList[_data[i]._frameIndex].Evaluate(1f - timer / _data[i]._duration));

                sprite.transform.position = _rootTransform.position + Vector3.Lerp(_data[i]._start,_data[i]._end, ManagerLobby._instance.motionblurCurveMove.Evaluate(1f - timer/_data[i]._duration));

                if (character != null && _enableColliderEvent)
                    character.ColliderUpdate(sprite.transform.position);

                if (_particle != null)
                    _particle.transform.position = sprite.transform.position + _particleOffset;
                yield return null;
            }
        }

        if (_particle != null)
            _particle.Stop(true);
        Destroy(sprite);
        bActionFinish = true;
        yield return null;
    }

    void OnDrawGizmosSelected()
    {

        if (_rootTransform != null)
        {

            for (int i = 0; i < _data.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(_rootTransform.position + _data[i]._start, _rootTransform.position + _data[i]._end);
                Gizmos.DrawWireSphere(_rootTransform.position + _data[i]._start, 0.4f);

                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(_rootTransform.position + _data[i]._end, 0.4f);
            }
        }

    }
}
