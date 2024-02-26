using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEffectBombLine
{
    eHLeft,
    eHRight,
    eVUp,
    eVDown,
}

public class Effect_Line : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;

    public Transform TrailTransform;

    public UITexture _sprite;
    [System.NonSerialized]
    public EEffectBombLine _type = EEffectBombLine.eHLeft;
    [System.NonSerialized]
    public Vector3 _scale = new Vector3(1f, 1f, 1f);

    public int arrowDepth = 0;
    public bool crossLine = false;

    public GameObject[] tailEffectObj1;
    public UITexture[] tailEffectTexture1;
    public TrailRenderer trailRenderer;

    public ParticleSystem particleObj;


    int scaleRatio = 3;
    float scaleRatioW = 1;

    public void SetOpacity(float opacity)
    {
        _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, opacity);

        for(int i = 0; i< tailEffectTexture1.Length;i++)
        {
            tailEffectTexture1[i].color = new Color(tailEffectTexture1[i].color.r, tailEffectTexture1[i].color.g, tailEffectTexture1[i].color.b, opacity);
        }

        if(trailRenderer != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                trailRenderer.colorGradient.colorKeys,
                new GradientAlphaKey[] { new GradientAlphaKey(opacity, 0.0f), new GradientAlphaKey(0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }

    void Awake()
    {
        _transform = transform;
    }

    IEnumerator Start()
    {
        foreach(var obj in tailEffectObj1)
        {
            if(obj != null)
            obj.SetActive(false);
        }
        

        _sprite.depth = (int)GimmickDepth.DECO_BASE;
        if (crossLine)
        {
            scaleRatio = 2;
            scaleRatioW = 0.8f;
        }


         Vector3 pos = _transform.localPosition;

        Vector3 dir = Vector3.left;
        if (_type == EEffectBombLine.eHLeft)
        {
            dir = Vector3.left;
        }
        else if (_type == EEffectBombLine.eHRight)
        {
            dir = Vector3.right;
        }
        else if (_type == EEffectBombLine.eVUp)
        {
            if(particleObj != null)
            {
                particleObj.startRotation = ManagerBlock.PI90;
            }

            _transform.Rotate(0f, 0f, 90f);
            dir = Vector3.up;
        }
        else if (_type == EEffectBombLine.eVDown)
        {
            if (particleObj != null)
            {
                particleObj.startRotation = ManagerBlock.PI90;
            }

            _transform.Rotate(0f, 0f, 90f);
            dir = Vector3.down;
        }

        _transform.localPosition = pos;
        _transform.localScale = _scale;
        float timer = 0f;

        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle;

            if (timer > 0.2f) timer = 0.2f;
            _transform.localScale = new Vector3(_scale.x * (1 - timer * scaleRatio), _scale.y * (1 + timer * 1f* scaleRatioW), _scale.y);

            pos = _transform.localPosition - dir * Global.deltaTimePuzzle * 10;
            _transform.localPosition = pos;
            yield return null;
        }
        
        timer = 0f;
        
        while (timer < 0.2f)
        {
            timer += Global.deltaTimePuzzle * 1.5f;

            if (timer > 0.2f)timer = 0.2f;
            _transform.localScale = new Vector3(_scale.x * (1 - (0.2f + timer*0.2f) * scaleRatio), _scale.y * (1 + 0.2f * 1f * scaleRatioW), _scale.y);

            pos = _transform.localPosition - dir * Global.deltaTimePuzzle * 10;
            _transform.localPosition = pos;
            yield return null;
        }
        
        timer = 0f;

        foreach (var obj in tailEffectObj1)
        {
            if(obj != null)
            obj.SetActive(true);
        }

        while (true)
        {
            timer += Global.deltaTimePuzzle;

            pos = _transform.localPosition + dir * Global.deltaTimePuzzle * 2100f;
            _transform.localPosition = pos;

            if(_transform.localScale.x < 1)
            {
                _transform.localScale = new Vector3(_scale.x * (1 + timer * scaleRatio), _scale.y * (1 - timer * 1f), _scale.y);
            }
            else
            {
                _transform.localScale = _scale;
            }
            
            if (pos.x > 2000f || pos.x < -2000f)
                Destroy(gameObject);
            else if (pos.y > 2000f || pos.y < -2000f)
                Destroy(gameObject);

            yield return null;
        }
    }
}
