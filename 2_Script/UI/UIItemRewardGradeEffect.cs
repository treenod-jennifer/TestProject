using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemRewardGradeEffect : MonoBehaviour
{
    public GameObject objWhiteLight;
    public GameObject objYellowLight;
    public GameObject objGood;
    public GameObject objLucky;
    public GameObject objDefultLabel;
    public GameObject objLuckyLabel;

    public void SetGradeEffect(int RewardGrad)
    {
        objGood.SetActive(RewardGrad == 2 ? true : false);
        objLucky.SetActive(RewardGrad == 3 ? true : false);
        objDefultLabel.SetActive(RewardGrad != 3 ? true : false);
        objLuckyLabel.SetActive(RewardGrad == 3 ? true : false);
        objWhiteLight.SetActive(false);
        objYellowLight.SetActive(false);

        StartCoroutine(CoGradeEffect(RewardGrad));
    }

    IEnumerator CoGradeEffect(int RewardGrad)
    {
        yield return new WaitForSeconds(0.2f);

        objWhiteLight.SetActive(RewardGrad == 2 ? true : false);
        objYellowLight.SetActive(RewardGrad == 3 ? true : false);

        DOTween.To(() => 0f, x => objLucky.GetComponent<UILabel>().alpha = x, 1f, 0.3f);
        DOTween.To(() => 0f, x => objGood.GetComponent<UILabel>().alpha = x, 1f, 0.3f);

        if (RewardGrad == 3)
        {
            DOTween.To(() => 0f, x => objLuckyLabel.GetComponent<UILabel>().alpha = x, 1f, 0.15f);
            objLuckyLabel.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.15f).SetEase(Ease.OutSine);
        }
    }
}
