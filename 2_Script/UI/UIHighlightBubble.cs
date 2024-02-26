using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHighlightBubble : MonoBehaviour
{
    public IEnumerator CoBubbleImageChange(params GameObject[] objects)
    {
        int changeValue = 0;

        while (true)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (changeValue == i)
                {
                    objects[i].SetActive(true);
                    continue;
                }
                objects[i].SetActive(false);
            }

            changeValue = (changeValue + 1) % objects.Length;

            yield return new WaitForSeconds(1f);
        }
    }
}