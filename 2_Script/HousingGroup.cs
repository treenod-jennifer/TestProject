using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGroup : MonoBehaviour {

    public List<GameObject> objectList = new List<GameObject>();
    int housingIdx = 0;
    [Header("Wait오브젝트에서는 꺼주세요")]
    [SerializeField] bool cloneObjectAtLoad = true;

    public int HousingIdx {
        set
        {
            housingIdx = value;
            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null) continue;
                var objEvent = objectList[i].GetComponent<ObjectEvent>();
                if( objEvent != null)
                    objEvent.housingIdx = housingIdx;
            }
        }
        get { return housingIdx; }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitInstantiate()
    {
        // 번들에 넣어둘 때는 개별 오브젝트가 아니라 링크만 걸어서 넣어두도록 할거니
        // 실제 instantiate 처음 할 때 이런식으로 개별분리처리

        if (this.cloneObjectAtLoad == false)
            return;
        for (int i = 0; i < objectList.Count; ++i)
        {
            if( objectList[i] == null )
            {
                Debug.Log("ERR");
            }

            this.objectList[i] = Instantiate(objectList[i]);
            objectList[i].transform.parent = this.transform;
        }
    }

    public void ApplyTransforms(HousingGroup origin)
    {
        for(int i = 0; i < origin.objectList.Count; ++i)
        {
            this.objectList[i].transform.position = origin.objectList[i].transform.position;
            this.objectList[i].transform.localScale = origin.objectList[i].transform.localScale;
            this.objectList[i].transform.rotation= origin.objectList[i].transform.rotation;
        }
    }

    public void SetActive(bool active)
    {
        for (int i = 0; i < objectList.Count; ++i)
        {
            this.objectList[i].gameObject.SetActive(active);
        }

    }
}
