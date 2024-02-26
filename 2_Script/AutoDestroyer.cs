using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyer : MonoBehaviour
{
    private List<UnityEngine.Object> destroyObjects = new List<UnityEngine.Object>();

    private void AddDestroyObject(UnityEngine.Object resource)
    {
        destroyObjects.Add(resource);
    }

    private void OnDestroy()
    {
        foreach(var obj in destroyObjects)
        {
            if(obj != null)
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// 타겟이 제거될 때 리소스를 자동으로 메모리에서 해제합니다.
    /// </summary>
    public static void SetDestroy(GameObject target, UnityEngine.Object resource)
    {
        AutoDestroyer autoDestroyer = target.GetComponent<AutoDestroyer>();

        if(autoDestroyer == null)
        {
            autoDestroyer = target.AddComponent<AutoDestroyer>();
        }

        autoDestroyer.AddDestroyObject(resource);
    }
}
