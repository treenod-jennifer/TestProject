using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemBGController : MonoBehaviour {

    [SerializeField] private UIBasicSprite[] bgs;
    [SerializeField] private UIItemCloud[] clouds;

    private int selectBGIndex = 0;

    private bool isChanging = false;
    public void ChangeBG(int index)
    {
        if (!isChanging &&index != selectBGIndex && bgs.Length > index)
        {
            StartCoroutine(CoChangeBG(index));

            foreach(var cloud in clouds)
            {
                cloud.ChangeCloud(index);
            }
        }
    }

    [SerializeField][Range(0.1f, 10.0f)] float changeTime = 1.0f;
    private IEnumerator CoChangeBG(int index)
    {
        isChanging = true;

        float totalTime = 0.0f;

        bgs[selectBGIndex].depth = 0;
        bgs[index].depth = -1;
        bgs[index].alpha = 1.0f;

        while (bgs[selectBGIndex].alpha != 0.0f)
        {
            totalTime += Global.deltaTimePuzzle;

            bgs[selectBGIndex].alpha = Mathf.Lerp(1.0f, 0.0f, totalTime / changeTime);

            yield return null;
        }

        selectBGIndex = index;

        isChanging = false;
    }

    public void testc()
    {
        ChangeBG(1);
    }
}
