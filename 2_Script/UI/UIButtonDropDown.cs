using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonDropDown : MonoBehaviour
{
    public enum ExpansionState
    {
        Expansion,
        Reduction,
        Transforming
    }

    [Header("Animation Control")]
    [SerializeField] private float speed = 1.0f;

    [Header("Linked Object")]
    [SerializeField] private UIPanel list_Panel;
    [SerializeField] private Transform listRoot;
    [SerializeField] private Transform listEdge;

    [SerializeField] private Transform triangle;

    [Header("Value")]
    [SerializeField] private int expansionHeight;
    private float reductionHeight;

    //드롭다운 박스가 펼쳐졌을때와 닫혔을때의 이벤트
    public event System.Action<ExpansionState> OnClickDropDownBtnEvent;
    public event System.Action PostExpansionEvent;
    public event System.Action PostReductionEvent;

    private ExpansionState state = ExpansionState.Reduction;

    private void Awake()
    {
        reductionHeight = listRoot.localPosition.y;
    }

    public void ButtonCall()
    {
        OnClickDropDownBtnEvent?.Invoke(state);

        if (state == ExpansionState.Reduction)
            Expansion();
        else if (state == ExpansionState.Expansion)
            Reduction();
    }
    private void Expansion()
    {
        state = ExpansionState.Transforming;
        StartCoroutine(CoTransforming(expansionHeight));
    }

    public void Reduction()
    {
        if (state == ExpansionState.Expansion)
        {
            state = ExpansionState.Transforming;
            StartCoroutine(CoTransforming((int)reductionHeight));
        }
    }

    public void SetHeightByDynamic(int height)
    {
        expansionHeight = height;
    }
    
    private IEnumerator CoTransforming(int transforHeight)
    {
        float totalTime = 0.0f;

        float startHegiht = listRoot.localPosition.y;
        float endHeight = transforHeight;

        while (totalTime < 1.0f)
        {
            totalTime += Time.unscaledDeltaTime * speed;

            float height = Mathf.Lerp(startHegiht, endHeight, totalTime);
            list_Panel.SetRect(0.0f, height * 0.5f - 2, list_Panel.width, height * -1 + 4);
            listRoot.localPosition = new Vector3(listRoot.localPosition.x, height, listRoot.localPosition.z);

            if (transforHeight == expansionHeight)
            {
                if(listEdge != null)
                    listEdge.localPosition = new Vector3(listEdge.localPosition.x, Mathf.Lerp(15.0f, 0.0f, totalTime), listEdge.localPosition.z);
                if(triangle != null)
                    triangle.localEulerAngles = new Vector3(triangle.localEulerAngles.x, triangle.localEulerAngles.y, Mathf.Lerp(180.0f, 0.0f, totalTime));
            }
            else
            {
                if (listEdge != null)
                    listEdge.localPosition = new Vector3(listEdge.localPosition.x, Mathf.Lerp(0.0f, 15.0f, totalTime), listEdge.localPosition.z);
                if (triangle != null)
                    triangle.localEulerAngles = new Vector3(triangle.localEulerAngles.x, triangle.localEulerAngles.y, Mathf.Lerp(0.0f, 180.0f, totalTime));
            }

            yield return null;
        }

        if (transforHeight == expansionHeight)
        {
            state = ExpansionState.Expansion;
            PostExpansionEvent?.Invoke();
        }
        else
        { 
            state = ExpansionState.Reduction;
            PostReductionEvent?.Invoke();
        }
    }
}
