using System.Collections;
using UnityEngine;

public class UIWorldRankAdBubble : MonoBehaviour
{
    [SerializeField] private GameObject[] activeObj;
    private Coroutine actionRoutine = null;

    private void Start()
    {
        StartCoroutine(CoIconChangeAction());
    }

    public void OnEnable()
    {
        for (int i = 0; i < activeObj.Length; i++)
            activeObj[i].SetActive(false);
        actionRoutine = StartCoroutine(CoIconChangeAction());
    }

    private void OnDisable()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }
    }

    IEnumerator CoIconChangeAction()
    {
        int index = 0;

        while (this.gameObject.activeInHierarchy)
        {
            activeObj[index].gameObject.SetActive(true);
            
            yield return new WaitForSeconds(1f);
            
            if (index >= activeObj.Length - 1)
            {
                activeObj[activeObj.Length - 1].gameObject.SetActive(false);
                index = 0;
            }
            else
            {
                activeObj[index].gameObject.SetActive(false);
                index++;
            }
        }
    }
}
