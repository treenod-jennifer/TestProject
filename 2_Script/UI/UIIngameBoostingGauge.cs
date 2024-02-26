using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class UIIngameBoostingItem
{
    public UISprite iconItem;
}

public class UIIngameBoostingGauge : MonoBehaviour
{
    [SerializeField]
    private UIProgressBar progressBar;
    [SerializeField]
    private GameObject arrowObj;
    [SerializeField]
    private List<UIIngameBoostingItem> listBoostingItem;

    private int boostingStep = 0;

#region 초기화
    public void InitBoostingGauge()
    {
        boostingStep = GameManager.instance.boostingStep - 1;
        
        InitProgressBar();
        InitBoostingItem();
        InitBoostingArrowPosition();
    }

    private void InitProgressBar()
    {
        float defalutValue = 0.54f;
        float offset = 0.155f;

        //프로그레스 바 = 기본value + (step * offset);
        float targetValue = defalutValue + (boostingStep * offset);
        progressBar.value = targetValue;
    }

    private void InitBoostingItem()
    {
        for (int i = 0; i < listBoostingItem.Count; i++)
        {
            bool bActive = (boostingStep <= i) ? false : true;
            SetItem(listBoostingItem[i], bActive);
        }
    }

    private void SetItem(UIIngameBoostingItem boostingItem, bool isActive)
    {
        Color color = Color.white;
        if(isActive == false)
            color = new Color(172f / 255f, 172f / 255f, 172f / 255f);

        boostingItem.iconItem.color = color;
    }

    private void InitBoostingArrowPosition()
    {
        float defalutPosX = -10f;
        float offsetX = 55f;

        //화살표 위치 = 기본pos + (step * offset);
        Vector3 targetPos = new Vector3(defalutPosX + (boostingStep * offsetX), 30f, 0f);
        arrowObj.transform.localPosition = targetPos;
    }
    #endregion

    #region 감소 연출
    public void DecreaseBoostingAction()
    {
        if (boostingStep == 0)
            return;
        StartCoroutine(CoDecreaseBoostingAction());
    }

    private IEnumerator CoDecreaseBoostingAction()
    {   
        yield return new WaitForSeconds(0.2f);

        //프로그레스 바
        float defalutValue = 0.54f;
        DOTween.To(() => progressBar.value, x => progressBar.value = x, defalutValue, 0.3f).SetEase(Ease.Linear);

        //화살표
        float defalutPosX = -10f;
        arrowObj.transform.DOLocalMoveX(defalutPosX, 0.3f).SetEase(Ease.Linear);

        //마지막 활성화 됐었던 아이템부터 차례로 비활성화.
        for (int i = boostingStep; i > 0; i--)
        {
            SetItem(listBoostingItem[i - 1], false);
        }
        yield return null;
    }
    #endregion
}
