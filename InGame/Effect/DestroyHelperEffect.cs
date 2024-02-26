using UnityEngine;

public class DestroyHelperEffect : MonoBehaviour {

    public float _delayDestroy = 0f;
    public float _delayParentRelease = 0f;
    public float _scaleTimer = 0;

    [System.NonSerialized]
    public bool isDestoryByEvent = false;

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

    void Update()
    {
        if (isDestoryByEvent == true)
            return;

        timer += Global.deltaTimePuzzle;
        if (_delayDestroy != 0f)
        {
            if (timer >= _delayDestroy)
            {
                timer = 0f;

                //gameObject.Recycle();
                Destroy(gameObject);
                return;
            }

            if (_scaleTimer != 0f)
            {
                if (timer >= _scaleTimer)
                {
                    _transform.localScale = Vector3.one * (1-(timer - _scaleTimer) / (_delayDestroy - _scaleTimer));
                }
            }
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
                Destroy(gameObject);              
                return;
            }


    }

    public void DestroyWithoutTimer()
    {
        if (isDestoryByEvent == true)
        {
            Destroy(gameObject);
            return;
        }
    }
}
