using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLastLoginADBonus : MonoBehaviour
{
    [SerializeField] private List<UILabel> labelDay;
    [SerializeField] private GameObject objComplete;
    [SerializeField] private List<GenericReward> reward;
    [SerializeField] private GameObject lastRewardObj;
    [SerializeField] private UIGrid gridRewardRoot;

    LoginADBonusData loginADBonusData;

    public void InitData(LoginADBonusData loginADBonusData)
    {
        this.loginADBonusData = loginADBonusData;

        for (int i = 0; i < labelDay.Count; i++)
        {
            labelDay[i].text = $"Day{loginADBonusData.day}";
        }

        SetReward();

        SetComplete();
    }

    void SetComplete()
    {
        if (loginADBonusData.action)
        {
            objComplete.SetActive(false);

            //연출
            UIPopupLoginADBonus._instance._callbackOpen += () => StartCoroutine(CoPlayAction());
        }
        else
        {
            if (loginADBonusData.complete)
                objComplete.SetActive(true);
            else
                objComplete.SetActive(false);
        }
    }

    void SetReward()
    {
        //마지막 Day 보상 2개 (기본보상 + 추가보상)
        for (int i = 0; i < 2; i++)
        {
            reward[i].SetReward(loginADBonusData.rewards[i]);
        }

        //마지막 Day 특별 보상은 없을 수도 있음
        if(loginADBonusData.rewards.Count == 3)
        {
            lastRewardObj.SetActive(true);
            lastRewardObj.GetComponentInChildren<GenericReward>().SetReward(loginADBonusData.rewards[2]);
        }

        gridRewardRoot.Reposition();
    }

    public IEnumerator CoPlayAction()
    {
        bool isGetReward = false;

        ManagerUI._instance.OpenPopup<UIPopupGetRewardAlarm>((popup) =>
        {
            ManagerUI._instance.SyncTopUIAssets();

            popup.InitPopup(Global._instance.GetString("n_s_46"), loginADBonusData.rewardSet, 0f);
            popup.IsADBonus = true;
            popup.rewardAD = loginADBonusData.rewards[loginADBonusData.rewards.Count - 1];

        }, () => { isGetReward = true; });

        //보상 팝업 종료될 때까지 대기.
        yield return new WaitUntil(() => isGetReward == true);

        yield return UIPopupLoginADBonus._instance.PlayClear(objComplete.transform.position, () => objComplete.SetActive(true));
    }
}
