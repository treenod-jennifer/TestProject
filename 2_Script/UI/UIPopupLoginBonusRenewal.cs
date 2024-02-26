using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 로그인 이벤트 개선 메인 팝업
/// </summary>
public class UIPopupLoginBonusRenewal : UIPopupBase
{
    public static UIPopupLoginBonusRenewal instance = null;

    public enum LoginBonusRewardType
    {
        CUMULATIVE, // 누적
        CONTINUOUS  // 연속
    }

    [SerializeField] private GameObject popupRoot;
    
    [SerializeField] private List<UISprite> sprites;

    [SerializeField] private UILabel timeLabel;
    [SerializeField] private UILabel countLabel;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private List<LoginBonusCumulativeReward> cumulativeRewardItems;
    [SerializeField] private List<LoginBonusContinuousReward> continuousRewardItems;
    [SerializeField] private GameObject cumulativeCompleteSprite;
    [SerializeField] private GameObject continuousCompleteSprite;
    [SerializeField] private UISprite boniSprite;
    [SerializeField] private GameObject boni;

    private ServerUserLoginBonusRenewal _userLoginBonusData;
    private CdnLoginBonusRenewal data;

    public void InitPopup()
    {
        SetAtlas();

        _userLoginBonusData = ServerRepos.UserLoginBonusRenewal;
        data = ServerContents.LoginBonusRenewal;

        SetLabels();
        SetProgressBar();
        SetCumulativeLoginRewards();
        SetContinuousRewards();
        UpdateUI();
        StartCoroutine(CoStartBoniAnimation());
    }
    
    public override void OpenPopUp(int depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;
        
        popupRoot.transform.localScale = Vector3.one * 0.2f;
        popupRoot.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
        
        StartCoroutine(CoPostOpenPopup());
    }
    
    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
        
        popupRoot.transform.DOScale(Vector3.zero, _mainTime).SetEase(Ease.InBack);

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(_mainTime, () =>
            PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            ManagerUI._instance._popupList.Remove(this);
            Destroy(gameObject);
        }));
    }
    
    private void SetAtlas()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            sprites[i].atlas = ManagerLoginBonusRenewal.Instance.atlas;
        }

        if (mainSprite != null)
        {
            mainSprite.atlas = ManagerLoginBonusRenewal.Instance.atlas;
        }
    }

    private void UpdateUI()
    {
        UpdateBoniPosition();
        UpdateAllComplete();
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        base.OnDestroy();
    }

    // 종료 기간, 누적 로그인 카운트 레이블 셋팅.
    private void SetLabels()
    {
        timeLabel.text = Global.GetTimeText_YYMMDDHHMM(data.endTs);
        countLabel.text = _userLoginBonusData.cumulativeLoginCount.ToString();
    }

    // 누적 보상 프로그래스바 셋팅.
    private void SetProgressBar()
    {
        int maxCount = data.cumulativeLoginRewardData[data.cumulativeLoginRewardData.Count - 1].targetCount;
        progressBar.value = Mathf.Clamp((float) _userLoginBonusData.cumulativeLoginCount / (float) maxCount, 0f, 1f);
    }

    // 누적 보상 아이템 셋팅.
    private void SetCumulativeLoginRewards()
    {
        int maxCount = data.cumulativeLoginRewardData[data.cumulativeLoginRewardData.Count - 1].targetCount;
        for (int i = 0; i < data.cumulativeLoginRewardData.Count; i++)
        {
            int targetCount = data.cumulativeLoginRewardData[i].targetCount;
            float x = (((float) targetCount / (float) maxCount) * 686f) - 342f;
            if (i < cumulativeRewardItems.Count && cumulativeRewardItems[i] != null)
            {
                // 마지막 보상의 경우 게이지 우측에 고정되기 때문에 위치 이동 X.
                if (i < data.cumulativeLoginRewardData.Count - 1)
                {
                    cumulativeRewardItems[i].gameObject.transform.localPosition = new Vector3(x, 216, 0);
                }

                cumulativeRewardItems[i].InitData(i + 1, targetCount, _userLoginBonusData.cumulativeLoginCount,
                    _userLoginBonusData.getCumulativeLoginReward[i], data.cumulativeLoginRewardData[i].adAvailable,
                    data.cumulativeLoginRewardData[i].reward);
            }
        }
    }

    // 연속 보상 아이템 셋팅.
    private void SetContinuousRewards()
    {
        for (int i = 0; i < data.continuousLoginRewards.Count; i++)
        {
            if (i < continuousRewardItems.Count && continuousRewardItems[i] != null)
            {
                continuousRewardItems[i].InitData(i + 1, _userLoginBonusData.continuousLoginCount,
                    _userLoginBonusData.continuousLoginMostCount, _userLoginBonusData.getContinuousLoginReward[i], data.continuousLoginRewards[i]);
            }
        }
    }

    // 모든 보상 획득 시 Complete 이미지 출력.
    private void UpdateAllComplete()
    {
        bool activeCumulativeComplete = true;
        foreach (var reward in _userLoginBonusData.getCumulativeLoginReward)
        {
            if (reward == false)
            {
                activeCumulativeComplete = false;
            }
        }

        cumulativeCompleteSprite.SetActive(activeCumulativeComplete);

        bool activeContinuousComplete = true;
        foreach (var reward in _userLoginBonusData.getContinuousLoginReward)
        {
            if (reward == false)
            {
                activeContinuousComplete = false;
            }
        }

        continuousCompleteSprite.SetActive(activeContinuousComplete);
    }

    // 누적 보상 보니 위치 셋팅.
    private void UpdateBoniPosition()
    {
        bool setBoni = false;
        for (int i = 0; i < _userLoginBonusData.getCumulativeLoginReward.Count; i++)
        {
            if (i < cumulativeRewardItems.Count && cumulativeRewardItems[i] != null)
            {
                if (_userLoginBonusData.getCumulativeLoginReward[i] == false && setBoni == false)
                {
                    boni.transform.SetParent(cumulativeRewardItems[i].boniPosition);
                    boni.transform.localPosition = Vector3.zero;
                    cumulativeRewardItems[i].SetEffect(true);
                    setBoni = true;
                }
                else
                {
                    cumulativeRewardItems[i].SetEffect(false);
                }
            }

            
        }

        if (setBoni == false)
        {
            // 모든 누적 보상 획득 시 마지막 보상에 보니가 위치하도록.
            boni.transform.SetParent(cumulativeRewardItems[cumulativeRewardItems.Count - 1].boniPosition);
            boni.transform.localPosition = Vector3.zero;
            cumulativeRewardItems[cumulativeRewardItems.Count - 1].SetEffect(true);
        }
    }

    // 누적 보상 보니 애니메이션.
    private IEnumerator CoStartBoniAnimation()
    {
        int count = 0;
        while (true)
        {
            if (count == 0)
            {
                boniSprite.spriteName = "stage_icon_boni_01";
            }
            else if (count == 2)
            {
                boniSprite.spriteName = "stage_icon_boni_02";
            }

            boniSprite.transform.localPosition = new Vector3(Mathf.Sin(Time.time * 15f) * 2f,
                Mathf.Abs(Mathf.Cos(Time.time * 15f) * 5f), 0f);

            if (count >= 4)
            {
                count = 0;
            }
            else
            {
                count++;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    // 보상 획득 서버 통신(누적, 연속).
    public void RequestGetReward(LoginBonusRewardType type, int index, Action onComplete, int targetCount = 0)
    {
        // 서버에서 처리를 0부터 시작하여 index -1 처리 추가.
        ServerAPI.LoginBonusRenewalGetReward((int) type, index - 1, resp =>
        {
            if (resp.IsSuccess == false) return;

            onComplete?.Invoke();

            _userLoginBonusData = resp.userLoginEventRenewal;
            UpdateUI();

            if (resp.reward.directApplied != null)
            {
                foreach (var reward in resp.reward.directApplied)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        rewardType: reward.Value.type,
                        rewardCount: reward.Value.valueDelta,
                        moneyMRSN: type == LoginBonusRewardType.CUMULATIVE
                            ? ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOGIN_ACCUMULATE_REWARD
                            : ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOGIN_CONTINUOUS_REWARD,
                        itemRSN: type == LoginBonusRewardType.CUMULATIVE
                            ? ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOGIN_ACCUMULATE_REWARD
                            : ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOGIN_CONTINUOUS_REWARD,
                        QuestName: index.ToString()
                    );
                }
            }

            if (resp.reward.mailReceived != null)
            {
                foreach (var reward in resp.reward.mailReceived)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        rewardType: reward.type,
                        rewardCount: reward.value,
                        moneyMRSN: type == LoginBonusRewardType.CUMULATIVE
                            ? ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOGIN_ACCUMULATE_REWARD
                            : ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOGIN_CONTINUOUS_REWARD,
                        itemRSN: type == LoginBonusRewardType.CUMULATIVE
                            ? ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOGIN_ACCUMULATE_REWARD
                            : ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOGIN_CONTINUOUS_REWARD,
                        QuestName: index.ToString()
                    );
                }
            }

            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.reward);

            if (ManagerUI._instance != null)
                ManagerUI._instance.SyncTopUIAssets();
            
            SendAchievementGrowthy_GetReward(type, index, targetCount);
        });
    }

    // 보상 획득 서버 통신(누적 광고).
    public void RequestGetAdReward(int index, int targetCount, Action onComplete)
    {
        // 서버에서 처리를 0부터 시작하여 index -1 처리 추가.
        ServerAPI.LoginBonusRenewalGetAdReward(index - 1, (resp) =>
        {
            if (resp.IsSuccess == false) return;

            onComplete?.Invoke();
            
            _userLoginBonusData = resp.userLoginEventRenewal;
            UpdateUI();
            
            if (resp.reward.directApplied != null)
            {
                foreach (var reward in resp.reward.directApplied)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        rewardType: reward.Value.type,
                        rewardCount: reward.Value.valueDelta,
                        moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AD_VIEW,
                        itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_AD_VIEW,
                        QuestName: "TYPE_20_REWARD"
                    );
                }
            }

            if (resp.reward.mailReceived != null)
            {
                foreach (var reward in resp.reward.mailReceived)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        rewardType: reward.type,
                        rewardCount: reward.value,
                        moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AD_VIEW,
                        itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_AD_VIEW,
                        QuestName: "TYPE_20_REWARD"
                    );
                }
            }

            //퀘스트 데이터 갱신
            QuestGameData.SetUserData();
            UIDiaryController._instance.UpdateQuestData(true);
            
            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.reward);

            if (ManagerUI._instance != null)
                ManagerUI._instance.SyncTopUIAssets();

            SendAchievementGrowthy_GetReward(LoginBonusRewardType.CUMULATIVE, index, targetCount);
        });
    }

    private void SendAchievementGrowthy_GetReward(LoginBonusRewardType type, int index, int targetCount)
    {
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.LOGIN_EVENT,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.LOGIN_EVENT_REWARD,
            type == LoginBonusRewardType.CUMULATIVE?
                $"LOGIN_ACCUMULATE_REWARD_{index}": $"LOGIN_CONTINUOUS_REWARD_{index}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
            type == LoginBonusRewardType.CUMULATIVE? targetCount : 0
        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }
}