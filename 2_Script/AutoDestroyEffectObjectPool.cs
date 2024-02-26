using UnityEngine;
using System.Collections;

public class AutoDestroyEffectObjectPool : MonoBehaviour {
    public float _delayDestroy = 0f;
    public float _delayParentRelease = 0f;


    float timer = 0f;
    ParticleSystem _particleSystem;
    Transform _transform;
    void Start()
    {
        _transform = transform;
        _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
            _particleSystem = GetComponentInChildren<ParticleSystem>();
    }
	// Update is called once per frame
	void Update () {

        timer += Global.deltaTime;
        if (_delayDestroy!=0f)
            if (timer >= _delayDestroy)
            {
                timer = 0f;
                //PokoObjectPool.instance.Recycle(gameObject);
                gameObject.Recycle();
                return;
                //Destroy(gameObject);
            }

        if (_delayParentRelease != 0f)
        {
            if (timer >= _delayParentRelease)
            {
                if (_transform.parent != null)
                    _transform.parent = null;
            }
        }

        if (_particleSystem != null)
            if (!_particleSystem.IsAlive(true))
            {
                timer = 0f;
                //PokoObjectPool.instance.Recycle(gameObject);
                if (ManagerObjectPool._instance != null)
                {
                    gameObject.Recycle();
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
                //Destroy(gameObject);
            }
	}
}
