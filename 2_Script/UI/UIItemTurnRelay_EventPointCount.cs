using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemTurnRelay_EventPointCount : MonoBehaviour
{
    public UILabel[] labelEventPoint;

    [SerializeField] private GameObject objEffectBG;
    [SerializeField] private GameObject objEffectLight;

    [SerializeField] private Color fontColor_Default = Color.white;

    public void InitEventPointUI(int defaultPoint, bool isShowEffect = false)
    {
        //디폴트 텍스트 설정
        string countText = defaultPoint.ToString();

        //텍스트 출력
        for (int i = 0; i < labelEventPoint.Length; i++)
        {
            labelEventPoint[i].color = fontColor_Default;
            labelEventPoint[i].text = countText;
        }

        //이펙트 설정
        SetEffect(isShowEffect);
    }

    public void SetEventPointText(int defaultPoint)
    {  
        //디폴트 텍스트 설정
        string countText = defaultPoint.ToString();

        //텍스트 출력
        for (int i = 0; i < labelEventPoint.Length; i++)
            labelEventPoint[i].text = countText;
    }

    public void SetTextAction(float actionTime)
    {
        labelEventPoint[0].transform.DOPunchScale(new Vector3(0.3f, -0.1f, 0f), actionTime);
    }

    private void SetEffect(bool isShowEffect)
    {
        objEffectBG.SetActive(isShowEffect);
        objEffectLight.SetActive(isShowEffect);
    }

    public Transform GetTextTr()
    {
        return labelEventPoint[0].transform;
    }
}
