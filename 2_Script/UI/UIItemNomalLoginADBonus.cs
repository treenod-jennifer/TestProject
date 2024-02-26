using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemNomalLoginADBonus : MonoBehaviour
{
    [SerializeField] private List<UILabel> labelDay;
    [SerializeField] private GameObject objComplete;
    [SerializeField] private List<UISprite> sprBackGround;
    [SerializeField] private Color[] cColor;
    [SerializeField] private Color[] nColor;
    [SerializeField] private GenericReward nReward;
    [SerializeField] private GenericReward addReward;

    [SerializeField] Animation anim;

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
        if(loginADBonusData.action)
        {
            sprBackGround[0].color = nColor[0];
            sprBackGround[1].color = nColor[1];

            objComplete.SetActive(false);

            //연출
            UIPopupLoginADBonus._instance._callbackOpen += () => StartCoroutine(CoPlayAction());
        }
        else
        {
            if(loginADBonusData.complete)
            {
                sprBackGround[0].color = cColor[0];
                sprBackGround[1].color = cColor[1];

                objComplete.SetActive(true);
            }
            else
            {
                sprBackGround[0].color = nColor[0];
                sprBackGround[1].color = nColor[1];

                objComplete.SetActive(false);
            }
        }
    }

    void SetReward()
    {
        nReward.SetReward(loginADBonusData.rewards[0]);
        addReward.SetReward(loginADBonusData.rewards[1]);
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

        yield return UIPopupLoginADBonus._instance.PlayClear(objComplete.transform.position,
            () =>
            {
                objComplete.SetActive(true);
                SetCompleteBg();
            });
    }

    public void SetCompleteBg()
    {
        sprBackGround[0].color = cColor[0];
        sprBackGround[1].color = cColor[1];
    }
}
