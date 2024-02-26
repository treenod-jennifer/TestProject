using System.Collections.Generic;
using UnityEngine;

public class UIItemSpaceTravelStage : MonoBehaviour
{
    private UIPopUpSpaceTravel.SpaceTravelStage _stageData;

    // 일반 스테이지
    [SerializeField] private UISprite _normalStageSprite;
    [SerializeField] private UILabel  _normalStageIndexLabel;

    // 보상 스테이지 (별 모양, 3의 배수 스테이지)
    [SerializeField] private GameObject    _starStageLightSprite;
    [SerializeField] private UISprite      _starStageSprite;
    [SerializeField] private GameObject    _starStageGetBtn;
    [SerializeField] private GameObject    _starStageRewardRoot;
    [SerializeField] private GenericReward _starStageRewardItem;

    // 라인 오브젝트
    [SerializeField] private Transform _lineTransform;

    // 아틀라스 할당 스프라이트
    [SerializeField] private List<UISprite> _spriteList;

    public void InitStage(UIPopUpSpaceTravel.SpaceTravelStage stageData, Color color)
    {
        foreach (var item in _spriteList) item.atlas = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;

        _stageData = stageData;

        // 일반 스테이지 세팅
        if ((stageData.stageIndex + 1) % 3 > 0)
        {
            _normalStageSprite.atlas      = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
            _normalStageSprite.spriteName = stageData.stageType <= ManagerSpaceTravel.StageType.CLOSE ? "stage_not_clear" : "stage_clear";
            _normalStageSprite.gameObject.SetActive(true);
            _starStageSprite.gameObject.SetActive(false);
            _normalStageIndexLabel.text = (stageData.stageIndex + 1).ToString();
        }
        // 보상 스테이지 세팅
        else
        {
            _starStageSprite.atlas      = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
            _starStageSprite.spriteName = stageData.stageType <= ManagerSpaceTravel.StageType.CLOSE ? "not_clear" : "clear";
            _normalStageSprite.gameObject.SetActive(false);
            _starStageSprite.gameObject.SetActive(true);
            _starStageSprite.color = color;
            SetStageHighlight();
            SetStageReward();
        }
    }

    public void InitLine(Transform next)
    {
        if (next != null)
        {
            _lineTransform.gameObject.SetActive(true);
            var angle    = Vector2.Angle(Vector2.right, next.localPosition - transform.localPosition);
            var rotation = new Vector3(0, 0, angle                         - 180f);
            _lineTransform.localRotation = Quaternion.Euler(rotation);
        }
        else
        {
            _lineTransform.gameObject.SetActive(false);
        }
    }

    private void SetStageReward()
    {
        // 마지막 스테이지 or 보상이 없거나 이미 받은 상태라면 보상 아이콘을 보여주지 않음
        if (_stageData.rewardList       == null                                     ||
            _stageData.rewardList.Count == 0                                        ||
            _stageData.stageIndex       == ManagerSpaceTravel.instance.MaxStage - 1 ||
            _stageData.stageType        == ManagerSpaceTravel.StageType.REWARDED)
        {
            _starStageRewardRoot.SetActive(false);
        }
        else
        {
            foreach (var reward in _stageData.rewardList) NGUITools.AddChild(_starStageRewardRoot, _starStageRewardItem.gameObject).GetComponent<GenericReward>().SetReward(reward);
        }

        _starStageGetBtn.SetActive(_stageData.stageType == ManagerSpaceTravel.StageType.CLEAR && _stageData.rewardList != null && _stageData.rewardList.Count > 0 && _stageData.stageIndex < ManagerSpaceTravel.instance.MaxStage - 1);
    }

    public void SetStageHighlight()
    {
        var isMaxStage = _stageData.stageIndex == ManagerSpaceTravel.instance.MaxStage - 1;
        var isNotReward = ManagerSpaceTravel.instance.GetStageType(_stageData.stageIndex) != ManagerSpaceTravel.StageType.REWARDED;
        _starStageLightSprite.SetActive(isMaxStage && isNotReward);
    }

    public void OnClickRewardButton()
    {
        if (UIPopUpSpaceTravel.instance.bCanTouch == false)
        {
            return;
        }

        UIPopUpSpaceTravel.instance.bCanTouch = false;
        var rewardIndex = _stageData.stageIndex / 3;
        ServerAPI.SpaceTravelGetReward(rewardIndex, (resp) =>
        {
            if (resp.IsSuccess)
            {
                ManagerSpaceTravel.instance.SyncFromServerUserData();
                ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.reward);
                _starStageRewardRoot.SetActive(false);
                _starStageGetBtn.SetActive(false);
                UIPopUpSpaceTravel.instance.PostRewardItem();
                ManagerUI._instance.SyncTopUIAssets();
                ManagerUI._instance.UpdateUI();

                if (resp.reward != null)
                {
                    if (resp.reward.directApplied != null)
                    {
                        foreach (var reward in resp.reward.directApplied)
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog((int) reward.Key, reward.Value.valueDelta,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SPACE_TRAVEL_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SPACE_TRAVEL_REWARD,
                            $"SPACE_TRAVEL_REWARD_{ManagerSpaceTravel.instance.EventIndex}_{rewardIndex + 1}");
                        }
                    }

                    if (resp.reward.mailReceived != null)
                    {
                        foreach (var reward in resp.reward.mailReceived)
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(reward.type, reward.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SPACE_TRAVEL_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SPACE_TRAVEL_REWARD,
                            $"SPACE_TRAVEL_REWARD_{ManagerSpaceTravel.instance.EventIndex}_{rewardIndex + 1}");
                        }
                    }
                }
            }
            UIPopUpSpaceTravel.instance.bCanTouch = true;
        });
    }
}