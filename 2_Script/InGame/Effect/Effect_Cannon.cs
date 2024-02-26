using System.Collections;
using UnityEngine;

public enum EEffectCannonLine
{
    eHLeft,
    eHRight,
    eVUp,
    eVDown,
}

public class Effect_Cannon : MonoBehaviour
{
    [System.NonSerialized] public EEffectCannonLine _type = EEffectCannonLine.eHLeft;
    public Transform _transform;
    public UITexture _sprite;
    public Vector3 _scale = new Vector3(1f, 1f, 1f);

    [SerializeField] private GameObject[] tailEffectObj1;
    [SerializeField] private UITexture[] tailEffectTexture1;

    private const int SCALE_RATIO = 3;

    private void Awake()
    {
        _transform = transform;
    }

    public void SetOpacity(float opacity)
    {
        _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, opacity);

        for (int i = 0; i < tailEffectTexture1.Length; i++)
        {
            tailEffectTexture1[i].color = new Color(tailEffectTexture1[i].color.r, tailEffectTexture1[i].color.g, tailEffectTexture1[i].color.b, opacity);
        }
    }

    private IEnumerator Start()
    {
        foreach (var obj in tailEffectObj1)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        _sprite.depth = (int) GimmickDepth.DECO_BASE;

        Vector3 pos = _transform.localPosition;
        Vector3 dir = Vector3.left;

        if (_type == EEffectCannonLine.eHLeft)
        {
            dir = Vector3.left;
        }
        else if (_type == EEffectCannonLine.eHRight)
        {
            dir = Vector3.right;
        }
        else if (_type == EEffectCannonLine.eVUp)
        {
            _transform.Rotate(0f, 0f, 90f);
            dir = Vector3.up;
        }
        else if (_type == EEffectCannonLine.eVDown)
        {
            _transform.Rotate(0f, 0f, 90f);
            dir = Vector3.down;
        }

        _transform.localPosition = pos;
        _sprite.gameObject.SetActive(true);

        float timer = 0f;

        foreach (var obj in tailEffectObj1)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        while (true)
        {
            timer += Global.deltaTimePuzzle;

            pos = _transform.localPosition + dir * Global.deltaTimePuzzle * 2100f;
            _transform.localPosition = pos;

            if (_transform.localScale.x < 1)
            {
                _transform.localScale = new Vector3(_scale.x * (1 + timer * SCALE_RATIO), _scale.y * (1 - timer * 1f), _scale.y);
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