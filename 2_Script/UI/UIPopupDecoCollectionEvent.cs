using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIPopupDecoCollectionEvent : UIPopupBase
{
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel text_Collect;
    [SerializeField] private UILabel text_EndCount;
    [SerializeField] private UILabel text_Timer;
    [SerializeField] private UIUrlTexture texture_InfoBg;
    [SerializeField] private UIUrlTexture texture_TitleBg;
    [SerializeField] private GenericReward rewardIcon;
    [SerializeField] private GameObject rewardRoot;
    [SerializeField] private GameObject obj_Check;
    [SerializeField] private GameObject obj_GetButton;
    [SerializeField] private GameObject obj_GetButtonLock;

    private Reward _reward = new Reward();
    private int _eventIndex = 0;
    private int _getCount = 0;
    private int _endCount = 0;
    private bool _isGetReward = false;
    private long _endTs = 0;

    private bool isCanTouch = true;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }
    
    public void InitData()
    {
        SetDefaultData();
        // 상단 정보 관련 데이터 세팅 (데코 배경 텍스쳐, 타이머)
        SetInfoData();
        // 하단 진행도 관련 데이터 세팅 (진행도, 보상)
        SetBottomData();
    }

    private void SetDefaultData()
    {
        _eventIndex = ManagerDecoCollectionEvent.DecoCollectionEventIndex;
        _endTs = ManagerDecoCollectionEvent.DecoCollectionEventEndTs;
        _reward = ManagerDecoCollectionEvent.DecoCollectionReward;
        _getCount = ManagerDecoCollectionEvent.DecoCollectCount;
        _endCount = ManagerDecoCollectionEvent.DecoMaxCount;
        _isGetReward = ManagerDecoCollectionEvent.IsGetReward;
    }

    private void SetInfoData()
    {
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "DecoCollection/", $"declEventPopup.png", (texture) =>
        {
            texture_InfoBg.mainTexture = texture;
        });
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "DecoCollection/", $"ev_decl_title.png", (texture) =>
        {
            texture_TitleBg.mainTexture = texture;
        });
        
        text_Timer.text = Global.GetTimeText_MMDDHHMM_Plus1(_endTs);
    }

    private void SetBottomData()
    {
        if (ManagerDecoCollectionEvent.instance != null)
        {
            progressBar.value = (float)_getCount / (float)_endCount;
            text_Collect.text = _getCount.ToString();
            text_EndCount.text = $"/{ _endCount }";
            obj_GetButtonLock.SetActive(_getCount >= _endCount ? false : true);
            obj_Check.SetActive(_isGetReward);
            obj_GetButton.gameObject.SetActive(!_isGetReward);
            rewardRoot.SetActive(_reward != null);
            if (_reward != null)
                rewardIcon.SetReward(_reward);
        }
        else
        {
            progressBar.value = 0;
            text_Collect.text = " ";
            text_EndCount.text = " ";
            obj_Check.SetActive(false);
            obj_GetButton.gameObject.SetActive(false);
            rewardRoot.SetActive(false);
        }
    }
    
    private void RecvGetReward(DecoCollectionEventGetRewardResp resp)
    {
        if (resp.IsSuccess && resp.result)
        {
            ManagerUI._instance.SyncTopUIAssets();
            ManagerUI._instance.UpdateUI();
            InitData();
            if (resp.clearReward != null)
            {
                ManagerUI._instance.OpenPopupGetRewardAlarm
                (Global._instance.GetString("n_m_1"),
                    null,
                    (resp.clearReward));
            }

            foreach (var reward in resp.clearReward.mailReceived)
            {
                // 보상 수령 관련 그로시 로그 전송
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type, reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_DECO_COLLECTION_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_DECO_COLLECTION_REWARD,
                    $"DECOCOLLECTION_REWARD_GET_{ManagerDecoCollectionEvent.DecoCollectionEventIndex}"
                );
            }
        }
        isCanTouch = true;
    }

    #region 클릭 이벤트 (데코 리스트 버튼, 보상 수령 버튼)

    private void OnClickDecoListdButton()
    {
        if (isCanTouch == false)
            return;
        var popup = ManagerUI._instance.OpenPopup<UIPopupDecoCollectionList>();
        popup.Init(_endTs, texture_TitleBg.mainTexture);
    }

    private void OnClickGetRewardButton()
    {
        if (isCanTouch == false)
            return;
        if (_endCount <= _getCount && !_isGetReward)
        {
            isCanTouch = true;
            ServerAPI.DecoCollectionGetReward(_eventIndex, RecvGetReward);
        }
    }

    #endregion
}
