using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectMaterialFly : MonoBehaviour {

    public RawImage _meshRenderer;
    [System.NonSerialized]
    public Transform _transform;
    

    void Awake()
    {
        _transform = transform;
    }
	// Use this for initialization
	IEnumerator Start () {


        float timer = 0f;
        float pp = Mathf.PI * 1.5f;

        Vector3 targetS = new Vector3(3f, 8f, 3f) + _transform.position;
        Vector3 startP = _transform.position;
        while(true)
        {
            timer += Global.deltaTime * 1.2f;

            float ratio = 1f + Mathf.Sin(pp + Mathf.Clamp01(timer) * Mathf.PI * 0.5f);

            Vector3 targetP = UIButtonMaterial._instance.transform.position;
            targetP = CameraController._instance.GetWorldPosFromScreen(targetP + new Vector3(50f,110f,0f),0.8f);


            _meshRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - ratio  * 2f));
            targetP = Vector3.Lerp(targetS, targetP, Mathf.Clamp01(ratio * 2f));
            _transform.position = Vector3.Lerp(_transform.position, targetP, ratio);
            
            yield return null;

            if (ratio >= 0.5f)
                break;
        }
        UIButtonMaterial._instance.AddCount();
        ManagerSound.AudioPlay(AudioLobby.liftup_shovel);
        Destroy(gameObject);
	}
	
}
