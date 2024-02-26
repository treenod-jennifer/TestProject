using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCloud : MonoBehaviour {

    [SerializeField] private UIBasicSprite[] clouds;
    [SerializeField] private int leftBound;
    [SerializeField] private int rightBound;
    [SerializeField][Range(0.0f, 100.0f)] private float moveSpeed = 1.0f;
    [SerializeField] private bool isLeftMove = true;

    private int selectCloudIndex = 0;

    private void Update()
    {
        transform.localPosition = transform.localPosition + (isLeftMove ? Vector3.left : Vector3.right) * Global.deltaTimePuzzle * moveSpeed;

        if(isLeftMove && transform.localPosition.x < leftBound)
        {
            transform.localPosition = new Vector3(rightBound, transform.localPosition.y, transform.localPosition.z);
        }
        else if(!isLeftMove && transform.localPosition.x > rightBound)
        {
            transform.localPosition = new Vector3(leftBound, transform.localPosition.y, transform.localPosition.z);
        }
    }

    private bool isChanging = false;
    public void ChangeCloud(int index)
    {
        if(!isChanging && index != selectCloudIndex && clouds.Length > index)
            StartCoroutine(CoChangeCloud(index));
    }

    [SerializeField][Range(0.1f, 10.0f)] float changeTime = 1.0f;
    private IEnumerator CoChangeCloud(int index)
    {
        isChanging = true;

        float totalTime = 0.0f;
        float alpha = 0.0f;

        while (alpha <= 1.0f)
        {
            totalTime += Global.deltaTimePuzzle;

            alpha = Mathf.Lerp(0.0f, 1.0f, totalTime / changeTime);

            clouds[selectCloudIndex].alpha = 1.0f - alpha;
            clouds[index].alpha = alpha;

            yield return null;
        }

        selectCloudIndex = index;

        isChanging = false;
    }
}
