using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_BombAniamtion : MonoBehaviour
{
    public GameObject RayObj;
    public GameObject GlowObj;

    public GameObject insideObj;
    public GameObject[] bombEffectObj = new GameObject[2];
    public float waitTimer = 0.5f;

    private float timer = 0;

    IEnumerator Start()
    {
        while (timer < waitTimer)
        {
            timer += Global.deltaTimePuzzle;

            RayObj.transform.localEulerAngles = new Vector3(0,0,timer*200);

            yield return null;
        }

        RayObj.SetActive(false);
        bombEffectObj[0].SetActive(true);
        bombEffectObj[1].SetActive(true);

        yield return null;

        timer = 0;
        while (timer < 1.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        bombEffectObj[0].SetActive(false);
        bombEffectObj[1].SetActive(false);

        yield return null;

        Destroy(gameObject);
        yield return null;
    }


	void Update ()
	{

	}
}
