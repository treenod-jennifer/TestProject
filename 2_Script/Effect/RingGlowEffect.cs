using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingGlowEffect : MonoBehaviour
{
    public UITexture _MainTexture;

    public float _effectScale = 0.45f;
    public int _timeScale = 3;
    float effectTimer = 0;

	IEnumerator Start () 
    {
        _MainTexture.transform.localScale = Vector3.zero;
        _MainTexture.alpha = 0;

        while (effectTimer < 1)
        {
            effectTimer += Global.deltaTimeLobby*3f;

            _MainTexture.transform.localScale = Vector3.one*_effectScale*effectTimer;
            _MainTexture.alpha = effectTimer;
            yield return null;
        }

        while (effectTimer < 1.5f)
        {
            effectTimer += Global.deltaTimeLobby*3f;

            _MainTexture.transform.localScale = Vector3.one * _effectScale * effectTimer;
            _MainTexture.alpha = 1 - (effectTimer- 1)*2  ;
            yield return null;
        }

        ManagerObjectPool.Recycle(gameObject);
        yield return null;
    }
	

}
