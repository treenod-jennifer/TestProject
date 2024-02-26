using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEnableTimer : MonoBehaviour {
    [SerializeField] EnableTimeInfo[] objects;

	// Use this for initialization
	void Start () {
        StartCoroutine(CoTimer());
	
	}
	
	// Update is called once per frame
	void Update () {

       
		
	}

    IEnumerator CoTimer()
    {
        float t = 0f;

        bool allActive = false;
        while (allActive == false)
        {
            t += Time.deltaTime;
            allActive = true;
            for (int i = 0; i < objects.Length; ++i)
            {
                if (t >= objects[i].time)
                {
                    if (objects[i].obj.activeInHierarchy == false)
                        objects[i].obj.SetActive(true);
                }
                else
                    allActive = false;
            }
            
            yield return new WaitForSeconds(0.01f);
        }
        yield break;
    }

    
}

[System.Serializable]
class EnableTimeInfo
{
    public float time;
    public GameObject obj;
}