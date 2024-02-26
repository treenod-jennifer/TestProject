using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureBGSelecter : MonoBehaviour
{
    [SerializeField] private bool autoEnable = true;
    [SerializeField] private GameObject normalObject;
    [SerializeField] private GameObject eventObject;

    private void Awake()
    {
        if (autoEnable)
        {
            GetAdventureObject().SetActive(true);
        }
    }

    public T GetAdventureObject<T>()
    {
        GameObject obj = GetAdventureObject();

        return obj.GetComponent<T>();
    }

    public GameObject GetAdventureObject()
    {
        if (Global.GameType == GameType.ADVENTURE_EVENT)
        {
            return eventObject;
        }
        else
        {
            return normalObject;
        }
    }
}
