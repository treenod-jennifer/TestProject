using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIPopupGetRewardAlarm : UIPopupBase
{
    [SerializeField] private UILabel labelMessage;    
    [SerializeField] private Transform trItemRewardRoot;
    [SerializeField] private List<UIItemGetRewardAlarm> listItemGetRewardAlarm;
    [SerializeField] private GameObject btnOk;
    [SerializeField] private GameObject btnAd;
    [SerializeField] private UIGrid grid;
    [SerializeField] private List<GameObject> listReward;
    [SerializeField] private GenericReward reward;

    public override void OpenPopUp(int depth)
    {
        //팝업 오픈 연출이 모두 재생된 뒤, 클릭 가능하도록 변경.
        this.bCanTouch = false;
        uiPanel.depth = depth;
    }

    public void InitPopup(string textMsg, Protocol.AppliedRewardSet rewardSet, float delay = 0f)
    {
        OpenPopupAnim(delay);

        btnOk.SetActive(false);
        labelMessage.text = textMsg;

        int uiIdx = 0;
        int rewardCnt = 0;

        //즉시지급 보상 UI 설정
        if (rewardSet.directApplied != null)
        {
            rewardCnt += rewardSet.directApplied.Count;
            foreach (var item in rewardSet.directApplied)
            {
                listItemGetRewardAlarm[uiIdx].Init(item.Key, item.Value);
                uiIdx++;
            }
        }

        //메일로 들어가는 보상 UI 설정
        if (rewardSet.mailReceived != null)
        {
            rewardCnt += rewardSet.mailReceived.Length;
            for (int i = 0; i < rewardSet.mailReceived.Length; i++)
            {
                listItemGetRewardAlarm[uiIdx].Init(rewardSet.mailReceived[i]);
                uiIdx++;
            }
        }

        //아이템 위치 및 UI 설정
        trItemRewardRoot.localPosition = new Vector3(-85f * (rewardCnt - 1), 0f, 0f);
        if (listItemGetRewardAlarm.Count > rewardCnt)
        {
            for (int i = rewardCnt; i < listItemGetRewardAlarm.Count; i++)
            {
                listItemGetRewardAlarm[i].gameObject.SetActive(false);
            }
        }
        StartCoroutine(CoAction());
    }

    private void OpenPopupAnim(float delay)
    {
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);
        if (delay > 0f)
            StartCoroutine(CoAppearPopUp(delay));
        else
        {
            if (mainSprite != null)
            {
                mainSprite.transform.localScale = Vector3.one * 0.2f;
                mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
                mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
                DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
            }
        }
    }

    public IEnumerator CoAppearPopUp(float delay = 0f)
    {
        bCanTouch = false;
        uiPanel.alpha = 0.01f;
        yield return new WaitForSeconds(delay);
        uiPanel.alpha = 1f;
        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.2f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }
        bCanTouch = true;
    }

    [HideInInspector] public bool IsADBonus = false;
    [HideInInspector] public Reward rewardAD = new Reward();

    private IEnumerator CoAction()
    {
        yield return new WaitForSeconds(0.8f);
        this.bCanTouch = true;
        btnOk.SetActive(true);
        btnAd.SetActive(IsADBonus);

        if (IsADBonus)
        {
            StartCoroutine(CoRewardLoop());
            reward.SetReward(rewardAD);
        }

        grid.Reposition();
    }

    private IEnumerator CoRewardLoop()
    {
        bool changeValue = false;

        while (btnAd.activeInHierarchy)
        {
            listReward[0].SetActive(changeValue);
            listReward[1].SetActive(!changeValue);

            changeValue = !changeValue;

            yield return new WaitForSeconds(1f);
        }
    }

    void OnClickADBtn()
    {
        _callbackEnd += () =>
        {
            UIPopupLoginADBonus._instance.ShowAD();
        };

        ClosePopUp();
    }
}
