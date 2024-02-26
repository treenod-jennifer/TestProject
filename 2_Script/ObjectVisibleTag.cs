using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectVisibleTagType
{
    NONE,
    PRESENT_BOX_NPC_SPACE,
    ADVENTURE_NPC_SPACE,
    MOLE_CATCH_NPC_SPACE,
    POKOFLOWER_NPC_SPACE,
    TURNRELAY_NPC_SPACE,
    CAPSULE_TOY_NPC_SPACE,
    NPC_SPACE_UNUSED_2,
    NPC_SPACE_UNUSED_3,
    NPC_SPACE_UNUSED_4,
}


public class ObjectVisibleTag : MonoBehaviour {

    [SerializeField]
    ObjectVisibleTagType _objectTag;

    // Use this for initialization
    void Start () {
        ManagerArea._instance.objVisibleTagMgr.RegisterObject(_objectTag, this.gameObject);
	}

    private void OnDestroy()
    {
        ManagerArea._instance.objVisibleTagMgr.UnregisterObject(_objectTag, this.gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

public class ObjectVisibleTagManager
{
    Dictionary<ObjectVisibleTagType, HashSet<GameObject>> taggedObjects = new Dictionary<ObjectVisibleTagType, HashSet<GameObject>>();

    Dictionary<ObjectVisibleTagType, bool> orders = new Dictionary<ObjectVisibleTagType, bool>();

    internal void RegisterObject(ObjectVisibleTagType t, GameObject g)
    {
        HashSet<GameObject> objSet = null;
        if(taggedObjects.ContainsKey(t))
        {
            objSet = taggedObjects[t];
        }
        else
        {
            objSet = new HashSet<GameObject>();
            taggedObjects.Add(t, objSet);
        }

        if (!objSet.Contains(g))
        {
            objSet.Add(g);

            if( orders.ContainsKey(t) )
            {
                g.SetActive(orders[t]);
            }
        }
    }

    internal void UnregisterObject(ObjectVisibleTagType t, GameObject gameObject)
    {
        if (taggedObjects.ContainsKey(t))
        {
            taggedObjects[t].Remove(gameObject);

            if( taggedObjects[t].Count <= 0)
            {
                taggedObjects.Remove(t);
            }

        }
    }

    internal void SetActiveToObjects(ObjectVisibleTagType t, bool onOff )
    {
        if ( orders.ContainsKey(t) )
        {
            if(orders[t] != onOff)
            {
                if( taggedObjects.ContainsKey(t) )
                {
                    ApplyVisibleToList(onOff, taggedObjects[t]);
                    orders.Remove(t);   // 기존에 내려진 오더가 있었는데 다른거 = 오더 취소로 간주
                    return;
                }
                // 다른 오더인데, 오브젝트가 하나도 없는 경우
            }
            // 같은 오더인 경우, 무시한다
            return;
        }
        else
        {
            orders.Add(t, onOff);
            if (taggedObjects.ContainsKey(t))
            {
                ApplyVisibleToList(onOff, taggedObjects[t]);
                return;
            }
        }
    }

    void ApplyVisibleToList(bool b, HashSet<GameObject> l)
    {
        var e = l.GetEnumerator();
        while(e.MoveNext())
        {
            e.Current.SetActive(b);
        }
    }
}