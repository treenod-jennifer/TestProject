using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol;
using UnityEngine;
using ServiceSDK;

public class UIItemCriminalEvent : MonoBehaviour
{
    [SerializeField] private GenericReward reward;
    [SerializeField] private UILabel stageLabel;
    [SerializeField] private GameObject receiveObj;
    [SerializeField] private GameObject clearObj;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private GameObject selectObj;

    private UIPopUpCriminalEvent.StageData stageData;

    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprCriminalList;

    private void OnDestroy()
    {
        for (var i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas = null;
        }
    }

    private void Start()
    {
        //번들 Atlas 세팅
        for (int i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas =
                ManagerCriminalEvent.instance.criminalEventPack.AtlasUI;
        }
    }

    public void InitData(UIPopUpCriminalEvent.StageData stageData)
    {
        this.stageData = stageData;
        reward.SetReward(stageData.reward);
        stageLabel.text = stageData.stageIndex.ToString();
        selectObj.SetActive(ManagerCriminalEvent.instance.GetEventStep() == stageData.dataIndex);

        // 미 클리어
        if (ManagerCriminalEvent.instance.IsStageClear(stageData.dataIndex) == false)
        {
            reward.gameObject.SetActive(true);
            receiveObj.SetActive(false);
            clearObj.SetActive(false);
            lockObj.SetActive(true);
        }
        // 보상 미수령
        else if (!stageData.isGetReward)
        {
            reward.gameObject.SetActive(true);
            receiveObj.SetActive(true);
            clearObj.SetActive(false);
            lockObj.SetActive(false);
        }
        // 보상 수령
        else
        {
            reward.gameObject.SetActive(true);
            reward.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f));
            SetRewardTextColor();
            
            reward.EnableInfoBtn(false);
            reward.btnTweenHolder.transform.localScale = Vector3.one;

            receiveObj.SetActive(false);
            clearObj.SetActive(true);
            lockObj.SetActive(false);
        }
    }

    public void OnClickReceiveButton()
    {
        if (!UIPopUpCriminalEvent.instance.bCanTouch)
            return;

        UIPopUpCriminalEvent.instance.bCanTouch = false;

        NetworkLoading.MakeNetworkLoading(1f);
        
        // Server API 호출
        ServerAPI.CriminalEventGetReward(stageData.dataIndex, (resp) =>
        {
            NetworkLoading.EndNetworkLoading();
            
            if (resp.IsSuccess)
            {
                //그로시
                {
                    if(resp.reward.directApplied != null)
                    {
                        foreach (var reward in resp.reward.directApplied)
                        {
                            // 구매한 아이템 기록
                            GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)reward.Key,
                                reward.Value.valueDelta,
                                GrowthyCustomLog_Money.Code_L_MRSN.G_CRIMINAL_EVENT,
                                GrowthyCustomLog_ITEM.Code_L_RSN.G_CRIMINAL_EVENT,
                                $"{ServerContents.CriminalEvent.vsn}_CRIMINAL_EVENT"
                            );
                        }
                    }
                    
                    if(resp.reward.mailReceived != null)
                    {
                        foreach (var reward in resp.reward.mailReceived)
                        {
                            // 구매한 아이템 기록
                            GrowthyCusmtomLogHelper.SendGrowthyLog(
                                reward.type,
                                reward.value,
                                GrowthyCustomLog_Money.Code_L_MRSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                GrowthyCustomLog_ITEM.Code_L_RSN.G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,
                                $"{ServerContents.CriminalEvent.vsn}_CRIMINAL_EVENT"
                            );
                        }
                    }
                }

                StartCoroutine(OnReceiveReward(resp.reward));
            }
        });
    }

    private void SetRewardTextColor()
    {
        Color textColor = reward.rewardCount[0].color;
        Color effectColor = reward.rewardCount[0].effectColor;


        reward.SetTextColor(new Color(textColor.r * 0.4f, textColor.g * 0.4f, textColor.b * 0.4f));
        reward.SetEffectTextColor(new Color(effectColor.r * 0.4f, effectColor.g * 0.4f, effectColor.b * 0.4f));
    }

    #region 연출

    private IEnumerator OnReceiveReward(AppliedRewardSet reward)
    {
        bool isAnim = true;
        UISprite spriteClear = clearObj.GetComponent<UISprite>();
        UISprite spriteReceive = receiveObj.GetComponentInChildren<UISprite>();

        DOTween.Sequence().OnStart(() =>
            {
                UIPopUpCriminalEvent.instance.bCanTouch = false;
                clearObj.SetActive(true);
                clearObj.transform.localScale = Vector3.one * 2f;
                spriteClear.alpha = 0f;
                
                if(ManagerCriminalEvent.instance.IsGetAllReward())
                    UIPopUpCriminalEvent.instance.CocoBubbleDisabled();
                
                this.reward.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f));
                SetRewardTextColor();
                this.reward.EnableInfoBtn(false);
                this.reward.btnTweenHolder.transform.localScale = Vector3.one;
            })
            .OnComplete(() =>
            {
                UIPopUpCriminalEvent.instance.bCanTouch = true;
                receiveObj.SetActive(false);
                isAnim = false;
            })
            .Append(DOTween.ToAlpha(() => spriteReceive.color, x => spriteReceive.color = x, 0f, 0.3f))
            .Append(clearObj.transform.DOShakeRotation(0.5f, 10f, 10, 180f, false))
            .Join(clearObj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce))
            .Join(DOTween.ToAlpha(() => spriteClear.color, x => spriteClear.color = x, 1f, 0.5f));

        yield return new WaitUntil(() => !isAnim);

        ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, reward);
        ManagerCriminalEvent.instance.SyncFromServerUserData();
        
        //획득한 상품 ui에 갱신
        ManagerUI._instance.SyncTopUIAssets();
        
        //우편함으로 보상이 획득 될 때 우편함 갱신
        ManagerUI._instance.UpdateUI();

        UIPopUpCriminalEvent.instance.bCanTouch = true;
    }

    public void PostClearAnim(bool isLockActive, bool isTargetActive)
    {
        lockObj.SetActive(isLockActive);
        receiveObj.SetActive(!isLockActive);
        selectObj.SetActive(isTargetActive);
    }

    public IEnumerator CoClearStageAnim()
    {
        bool isAnim = true;
        UISprite spriteLock = lockObj.GetComponent<UISprite>();
        UISprite spriteReceive = receiveObj.GetComponentInChildren<UISprite>();
        UISprite spriteGlow = selectObj.GetComponent<UISprite>();
        DOTween.Sequence().OnStart(() =>
            {
                lockObj.SetActive(true);
                receiveObj.SetActive(true);
                selectObj.SetActive(true);
                receiveObj.transform.localScale = Vector3.zero;
                spriteReceive.alpha = 0f;
            })
            .OnComplete(() =>
            {
                selectObj.SetActive(false);
                lockObj.SetActive(false);
                isAnim = false;
            })
            .Append(lockObj.transform.DOShakeRotation(0.7f, 20f, 70, 180f, false))
            .Append(DOTween.ToAlpha(() => spriteLock.color, x => spriteLock.color = x, 0f, 0.3f))
            .Append(DOTween.ToAlpha(() => spriteReceive.color, x => spriteReceive.color = x, 1f, 0.3f))
            .Join(receiveObj.transform.DOScale(Vector3.one * 1f, 0.3f).SetEase(Ease.OutBounce))
            .Append(DOTween.ToAlpha(() => spriteGlow.color, x => spriteGlow.color = x, 0f, 0.3f));

        yield return new WaitUntil(() => !isAnim);
    }

    public IEnumerator CoSetStageAnim()
    {
        bool isPlay = true;
        UISprite spriteGlow = selectObj.GetComponent<UISprite>();
        DOTween.Sequence().OnStart(() =>
            {
                selectObj.SetActive(true);
                spriteGlow.alpha = 0f;
            })
            .OnComplete(() => { isPlay = false; })
            .Append(DOTween.ToAlpha(() => spriteGlow.color, x => spriteGlow.color = x, 1f, 0.3f));
        yield return new WaitUntil(() => isPlay);
    }

    #endregion
}