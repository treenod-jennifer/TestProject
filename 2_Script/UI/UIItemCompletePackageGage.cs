using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCompletePackageGage : MonoBehaviour
{
    [SerializeField] private UIWidget gage_Back;
    [SerializeField] private UIProgressBar gage_Bar;

    [SerializeField] private UILabel count;
    [SerializeField] private UILabel countMax;

    [SerializeField] private Transform[] bounderyList;

    private int maxCount;

    public void InitGage(int maxCount, int count)
    {
        this.maxCount = maxCount;
        countMax.text = "/" + maxCount.ToString();
        SetBoundery(maxCount);

        this.count.text = count.ToString();
        SetGage(count);
    }

    public void SetGage(int count)
    {
        this.count.text = count.ToString();

        gage_Bar.value = (float)count / maxCount;
    }

    private void SetBoundery(int maxCount)
    {
        if (maxCount <= 0)
            return;

        int bounderySize = Mathf.RoundToInt(gage_Back.width / maxCount);

        for (int i=0; i<bounderyList.Length; i++)
        {
            if(i < maxCount - 1)
            {
                bounderyList[i].localPosition = Vector3.right * ((i + 1) * bounderySize);
                bounderyList[i].gameObject.SetActive(true);
            }
            else
            {
                bounderyList[i].gameObject.SetActive(false);
            }
        }
    }
}
