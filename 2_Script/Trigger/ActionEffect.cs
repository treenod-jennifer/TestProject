using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;



[System.Serializable]
public class ActionEffectList
{
    public ParticleSystem _particle;
    public bool _create = false;
    public float _delay = 0f;
    public float _delayStop = 0f;

    public TypeCharacterType _followType = TypeCharacterType.None;
    public Vector3 _followOffset = Vector3.one;
    public Transform _rootTransform = null;
    public Vector3 _offset = Vector3.one;
    public int _staticSource = 0;

    [System.NonSerialized]
    public bool play = false;

    [System.NonSerialized]
    public bool stop = false;
    [System.NonSerialized]
    public float timer = 0f;
    [System.NonSerialized]
    public ParticleSystem _localParticle = null;

    
} 

public class ActionEffect : ActionBase
{
    public bool _conditionWaitOn = false;
    public List<ActionEffectList> _animation = new List<ActionEffectList>();
    

    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //base.DoAction();
        for (int i = 0; i < _animation.Count; i++)
        {
            if (_animation[i]._particle == null)
            {
                if (_animation[i]._staticSource > 0)
                {
                    _animation[i]._create = true;
                    _animation[i]._particle = ManagerLobby._instance._staticEffectObj[_animation[i]._staticSource - 1];
                }
            }
        }

  //      Debug.Log("CoEffect _ DoAction");
        StartCoroutine(CoEffect());
    }
    IEnumerator CoEffect()
    {
        float timer = 0f;
        while (true)
        {

            bool endWork = true;
            bool endStop = true;

            for (int i = 0; i < _animation.Count; i++)
            {
                Character character = ManagerLobby._instance.GetCharacter(_animation[i]._followType);
                


                if (!_animation[i].play)
                {
                    _animation[i].timer += Global.deltaTimeLobby;
                    if (_animation[i].timer >= _animation[i]._delay)
                    {
                        _animation[i].play = true;

                        if (_animation[i]._particle != null)
                        {

                            if (_animation[i]._create)
                            {
                                _animation[i]._localParticle = Instantiate(_animation[i]._particle.gameObject).GetComponent<ParticleSystem>();
                                _animation[i]._localParticle.gameObject.SetActive(true);
                                _animation[i]._localParticle.gameObject.AddComponent<EffectDestroy>();
                                _animation[i]._localParticle.transform.parent = _animation[i]._particle.transform.parent;
                                _animation[i]._localParticle.transform.localPosition = _animation[i]._particle.transform.position;
                                _animation[i]._localParticle.transform.localScale = _animation[i]._particle.transform.localScale;
                            }
                            else
                            {
                                _animation[i]._particle.gameObject.SetActive(true);
                                _animation[i]._localParticle = _animation[i]._particle;
                            }

                            _animation[i]._localParticle.Play(true);
                        }
                    }

                    endWork = false;
                }
                if (_animation[i].play && _animation[i]._localParticle != null)
                {
                    if (character != null)
                        _animation[i]._localParticle.transform.position = character._transform.position + _animation[i]._followOffset;
                    else if (_animation[i]._rootTransform != null)
                        _animation[i]._localParticle.transform.position = _animation[i]._rootTransform.position + _animation[i]._offset;
                }
                


                if (_animation[i]._delayStop > 0f)
                    if (_animation[i].play && !_animation[i].stop)
                    {
                        _animation[i].timer += Global.deltaTimeLobby;
                        if (_animation[i].timer >= _animation[i]._delay + _animation[i]._delayStop)
                        {
                            _animation[i].stop = true;
                            if (_animation[i]._localParticle != null)
                            {
                                _animation[i]._localParticle.Stop(true);
                             //   _animation[i]._particle.Stop(true);
                            }
                        }
                        endStop = false;
                    }
            }


            if (endWork && endStop)
            {
                bActionFinish = true;
                break;
            }
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {

        if (_animation != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < _animation.Count; i++)
            {

                if (_animation[i]._rootTransform != null)
                {
                    Gizmos.DrawWireSphere(_animation[i]._rootTransform.position + _animation[i]._offset, 0.3f);
                    PokoUtil.drawString("<" + i + ">", _animation[i]._rootTransform.position + _animation[i]._offset, 0f, 0f, Color.magenta);
                }
                else
                {
                    Gizmos.DrawWireSphere(_animation[i]._particle.transform.position, 0.3f);
                    PokoUtil.drawString("<" + i + ">", _animation[i]._particle.transform.position, 0f, 0f, Color.magenta);
                }
            }
        }

    }
}
