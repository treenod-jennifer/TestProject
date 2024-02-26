using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UIPopupSingleRoundEvent : UIPopupBase
{
    public static UIPopupSingleRoundEvent instance;
    
    private StageMapData tempData;
    
    [SerializeField] private UILabel labelTimer;
    [SerializeField] private UILabel labelContent;
    
    [SerializeField] private UIUrlTexture texture;
    [FormerlySerializedAs("reward")] [SerializeField] private GenericReward genericReward;

    public Action callBackButtonCancel;
    
    public void Init(StageMapData tempData)
    {
        this.tempData = tempData;
        
        //타이머
        EndTsTimer.Run(labelTimer, ServerContents.SingleRound.endTs);
        
        texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "e_single_round_popup");

        //리워드
        Reward reward = ManagerSingleRoundEvent.instance.GetReward();
        if (reward != null)
        {
            genericReward.SetReward(reward);
            string text = Global._instance.GetString("p_guard_5").Replace("[0]", RewardHelper.GetRewardName((RewardType)reward.type, reward.value));
            labelContent.text = text.Replace("[1]", reward.value.ToString());
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        texture.mainTexture = null;
        instance            = null;
    }

    //닫았을 때 직전 데이터로 교체하는 콜백을 받아서 처리
    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        callBackButtonCancel?.Invoke();
    }

    private void OnClickButton1()
    {
        if (bCanTouch == false)
            return;
        
        ClosePopUp(callback:() =>
        {
            Global.GameInstance.StartCoroutine(
                Global.GameInstance.CoOnPopupOpen_Ready(tempData, _callbackOpen, _callbackClose, callBackButtonCancel));
        });
    }
}
