using System.Collections.Generic;
using UnityEngine;

namespace SideBanner
{
    public static class Maker
    {
        public static T MakeBanner<T>(UIItemSidebar sidebar, System.Action<T> init) where T : Banner, new()
        {
            UIBanner originPrefab = Resources.Load<UIBanner>($"UIPrefab/UIBanner");
            UIBanner uiBanner = Object.Instantiate(originPrefab, sidebar.BannerRoot);

            T banner = new T();

            banner.UIBanner = uiBanner;

            init(banner);

            sidebar.AddBanner(banner);

            return banner;
        }
    }

    public class UIBanner : MonoBehaviour
    {
        [SerializeField] private UIUrlTexture texture;
        [SerializeField] private UILabelPlus label;
        [SerializeField] private GameObject newIcon;

        public UIUrlTexture Texture { get { return texture; } }

        public UILabelPlus Label { get { return label; } }

        public GameObject NewIcon { get { return newIcon; } }

        public event System.Action OnClickEvent;
        public event System.Action OnEnableEvent;

        public void OnBannerClick()
        {
            OnClickEvent?.Invoke();
        }

        private void Start()
        {
            OnEnableEvent?.Invoke();
        }

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }

        public void UpdateNewIcon(string _uniqueKey)
        {
            NewIcon.SetActive(UIItemSidebar.newChecker.IsNew(_uniqueKey));
        }
    }

    public abstract class Banner
    {
        private UIBanner uiBanner;

        public UIBanner UIBanner 
        {
            protected get
            {
                return uiBanner;
            }
            set
            {
                uiBanner = value;
                uiBanner.OnClickEvent += () =>
                {
                    UIItemSidebar.newChecker.SetNew(UniqueKey);
                    uiBanner.UpdateNewIcon(UniqueKey);

                    OnBannerOpen();
                };
                uiBanner.OnEnableEvent += () => 
                {
                    uiBanner.UpdateNewIcon(UniqueKey);
                };
            }
        }

        public virtual int? PriorityBanner { get; } = null;

        public GameObject GameObject { get { return UIBanner.gameObject; } }

        protected UIUrlTexture Texture { get { return UIBanner.Texture; } }

        protected UILabelPlus Label { get { return UIBanner.Label; } }

        public abstract void OnBannerOpen(bool autoOpen = false);

        /// <summary>
        /// 해당 배너를 클릭 했는지 확인을 위해, 배너별 고유한 Key값을 설정해 주어야 합니다.
        /// </summary>
        public abstract string UniqueKey { get; }

        public virtual bool IsPriorityPackage()
        {
            return false;
        }
    }

    /// <summary>
    /// 커스텀 배너를 만들기 위한 베이스 클래스 입니다.
    /// </summary>
    /// <typeparam name="T">배너의 초기화에 필요한 데이터 형태 입니다.</typeparam>
    public abstract class Banner<T> : Banner
    {
        public abstract void Init(T bannerData);
    }

    #region 커스텀 데이터 정의

    public class NormalEvent
    {
        public readonly int eventIndex;
        public readonly long endTs;

        public NormalEvent(int eventIndex, long endTs)
        {
            this.eventIndex = eventIndex;
            this.endTs = endTs;
        }
    }

    public class UseResourceEvent : NormalEvent
    {
        public readonly int resourceIndex;

        public UseResourceEvent(int eventIndex, int resourceIndex, long endTs) : base(eventIndex, endTs)
        {
            this.resourceIndex = resourceIndex;
        }
    }

    public class CoinBonusStageEvent
    {
        public readonly int eventIndex;
        public readonly int playCount;
        public readonly int maxPlayCount;

        public CoinBonusStageEvent(int eventIndex, int playCount, int maxPlayCount)
        {
            this.eventIndex = eventIndex;
            this.playCount = playCount;
            this.maxPlayCount = maxPlayCount;
        }
    }

    #endregion

    #region 커스텀 배너 정의

    public class BannerStageEvent : Banner<CdnEventChapter>
    {
        private CdnEventChapter eventData;

        public override string UniqueKey => $"BannerStageEvent_{eventData.index}";

        public override void Init(CdnEventChapter eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_stage_{eventData.index}.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (UIPopupReady._instance != null)
            {
                return;
            }

            if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
            {
                if (ServerRepos.EventChapters.groupState > ServerContents.EventChapters.counts.Count)
                {
                    if (UIPopupSystem._instance == null)
                    {
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        string title = Global._instance.GetString("p_t_4");
                        popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_4"), false);
                        popup.SetResourceImage("Message/happy1");
                        popup.SortOrderSetting();
                    }
                    return;
                }
            }

            CdnEventChapter cdnData = ServerContents.EventChapters;

            int stageIndex = ServerRepos.EventChapters.stage;

            if (stageIndex > cdnData.counts[cdnData.counts.Count - 1])
            {
                if (eventData.index == Global.eventIndex)
                    stageIndex = Global.stageIndex;
                else
                    stageIndex = cdnData.counts[cdnData.counts.Count - 1];
            }

            Global.SetGameType_Event(eventData.index, stageIndex);

            ManagerUI._instance.OpenPopupReadyStageEvent();
        }
    }

    public class BannerPokoFlowerEvent : Banner<CdnPokoFlowerEvent>
    {
        private CdnPokoFlowerEvent eventData;

        public override string UniqueKey => $"BannerPokoFlowerEvent_{eventData.event_index}";

        public override void Init(CdnPokoFlowerEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_flower_{eventData.resource_index}.png");

            EndTsTimer.Run(Label, eventData.end_ts);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopup<UIPopupPokoFlowerEvent>((popup) => popup.InitData(eventData.event_index));
        }
    }

    public class BannerSpecialEvent : Banner<int>
    {
        private int specialEventIndex;

        public override string UniqueKey => $"BannerSpecialEvent_{specialEventIndex}";

        public override void Init(int specialEventIndex)
        {
            this.specialEventIndex = specialEventIndex;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_special_{specialEventIndex}.png");

            int collectCount = GetCollectCount(specialEventIndex);
            int allCount = GetAllCount(specialEventIndex);

            if(collectCount < allCount)
            {
                Label.text = $"{collectCount}/{allCount}";
            }
            else
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopupSpecialEvent(specialEventIndex, true);
        }

        private int GetCollectCount(int specialEventIndex)
        {
            int index = ServerRepos.UserSpecilEvents.FindIndex((specilEvent) => specilEvent.eventIndex == specialEventIndex);

            if (index != -1)
            {
                return ServerRepos.UserSpecilEvents[index].progress;
            }
            else
            {
                return 0;
            }
        }

        private int GetAllCount(int specialEventIndex)
        {
            int sectionCount = ServerContents.SpecialEvent[specialEventIndex].sections.Count;
            int allItemCount = ServerContents.SpecialEvent[specialEventIndex].sections[sectionCount - 1];

            return allItemCount;
        }
    }

    public class BannerAlphabetEvent : Banner<int>
    {
        int currentGruop = 0;

        public override string UniqueKey => $"BannerAlphabetEvent_{ManagerAlphabetEvent.instance.eventIndex}";

        public override void Init(int currentGroup)
        {
            currentGruop = currentGroup;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_alphabet_{ManagerAlphabetEvent.instance.resourceIndex}.png");

            if (ManagerAlphabetEvent.instance.isUser_eventComplete == true)
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
            else 
            {
                //일반 알파벳 카운트 표시
                if (ManagerAlphabetEvent.instance.isUser_normalComplete == false)
                {
                    int allCount = GetAllCount_Normal();
                    int collectCount = GetCollectCount_Normal();
                    Label.text = $"{collectCount}/{allCount}";
                }

                //스페셜 알파벳 카운트 표시
                else
                {
                    int allCount = ManagerAlphabetEvent.instance.listAlphabetIndex_Special.Count;
                    int collectCount = GetCollectCount_Special();
                    Label.text = $"{collectCount}/{allCount}";
                }
            }
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopupAlphabetEvent();
        }

        private int GetAllCount_Normal()
        {
            if (ManagerAlphabetEvent.instance.dicAlphabetIndex_Normal.ContainsKey(currentGruop) == false)
                return 0;

            return ManagerAlphabetEvent.instance.dicAlphabetIndex_Normal[currentGruop].Count;
        }

        private int GetCollectCount_Normal()
        {
            if (ManagerAlphabetEvent.instance.dicCollectCount_Normal == null)
                return 0;

            if (ManagerAlphabetEvent.instance.dicCollectCount_Normal.ContainsKey(currentGruop) == false)
                return 0;

            int count = 0;
            List<int> listTemp = ManagerAlphabetEvent.instance.dicCollectCount_Normal[currentGruop];
            for (int i = 0; i < listTemp.Count; i++)
            {
                if (listTemp[i] > 0)
                    count++;
            }
            return count;
        }

        private int GetCollectCount_Special()
        {
            if (ManagerAlphabetEvent.instance.listCollectCount_Special == null)
                return 0;

            int count = 0;
            for (int i = 0; i < ManagerAlphabetEvent.instance.listCollectCount_Special.Count; i++)
            {
                if (ManagerAlphabetEvent.instance.listCollectCount_Special[i] > 0)
                    count++;
            }
            return count;
        }
    }

    public class BannerTurnRelayEvent : Banner<CdnTurnRelayEvent>
    {
        private CdnTurnRelayEvent eventData;

        public override string UniqueKey => $"BannerTurnRelayEvent_{eventData.eventIndex}";

        public override void Init(CdnTurnRelayEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_turn_relay_{eventData.resourceIndex}.png");

            var eventState = ManagerTurnRelay.GetEventState();
            switch (eventState)
            {
                case ManagerTurnRelay.EventState.BEFORE_REWARD: SetTextBeforeReward(); break;
                case ManagerTurnRelay.EventState.REWARD: SetTextReward(); break;
                default: SetTextRunning(); break;
            }
        }

        //플레이 중 타이머
        private void SetTextRunning()
        {
            long playEndTs = eventData.startTs + eventData.runningPeriod;
            SetTextTime(playEndTs, SetTextBeforeReward);
        }

        //보상 집계 기간 중 타이머
        private void SetTextBeforeReward()
        {
            long beforeRewardEndTs = eventData.startTs + eventData.runningPeriod + eventData.countingPeriod;
            SetTextTime(Global._instance.GetString("p_tr_m_3"), beforeRewardEndTs, SetTextReward);
        }

        //보상 수령 기간 중 타이머
        private void SetTextReward()
        {
            long eventEndTs = eventData.startTs + eventData.runningPeriod + eventData.countingPeriod + eventData.rewardPeriod;
            SetTextTime(Global._instance.GetString("p_tr_m_4"), eventEndTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerTurnRelay.OpenTurnRelay();
        }

        private void SetTextTime(long endTs, System.Action timeOutCallback = null)
        {
            EndTsTimer.Run(
                target: Label,
                endTs: endTs,
                timeOutAction: timeOutCallback
            );
        }

        private void SetTextTime(string title, long endTs, System.Action timeOutCallback = null)
        {
            EndTsTimer.Run(
                target: Label,
                endTs: endTs,
                overrideTextFunc: (ts) => $"{title}({Global.GetLeftTimeText(endTs)})",
                timeOutAction: timeOutCallback
            );
        }
    }

    public class BannerStageChallenge : Banner<CdnStageChallenge>
    {
        private CdnStageChallenge eventData;

        public override string UniqueKey => $"BannerStageChallengeEvent_{eventData.eventIndex}_{ManagerStageChallenge.instance.EventStepIndex}";

        public override void Init(CdnStageChallenge eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_stagechallenge.png");

            SetText(SetTextTimeOut);
        }

        private void SetTextTimeOut()
        {
            SetText();
        }

        private void SetText(System.Action action = null)
        {
            //서버 데이터 동기화
            ManagerStageChallenge.instance.InitData();

            //배너 텍스트 변경
            if(ManagerStageChallenge.instance.IsEndStageChallenge == false)
            {
                long endTs = ManagerStageChallenge.instance.EndTs;
                long day = ManagerStageChallenge.instance.EventStepIndex + 1;
                string txt = Global._instance.GetString("p_msc_6");
                txt = txt.Replace("[n]", day.ToString());
                EndTsTimer.Run(
                    target: Label,
                    endTs: endTs,
                    overrideTextFunc: (ts) => txt,
                    timeOutAction: action
                );
            }

            //배너 알림 설정
            UIBanner.UpdateNewIcon(UniqueKey);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopupStageChallenge();
        }
    }

    public class BannerMoleCatchEvent : Banner<UseResourceEvent>
    {
        private UseResourceEvent eventData;

        public override string UniqueKey => $"BannerMoleCatchEvent_{eventData.eventIndex}";

        public override void Init(UseResourceEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_mole_catch_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopupMoleCatch();
        }
    }

    public class BannerStickerEvent : Banner<NormalEvent>
    {
        private NormalEvent eventData;

        public override string UniqueKey => $"BannerStickerEvent_{eventData.eventIndex}";

        public override void Init(NormalEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"line_stamp_{eventData.eventIndex}");
            EndTsTimer.Run(Label, eventData.endTs, (endTs) =>
            {
                string text;

                if (ServerRepos.UserEventStickers.Count > 0 && ServerRepos.UserEventStickers[0].state != 0)
                {
                    //언제 완료 상태로 바뀌는지 알 수 없다고 합니다. 그래서 타이머가 돌아가면서 실시간으로 체크 할 수 있도록 처리하였습니다.
                    text = $"{Global._instance.GetString("li_ev_2")}({Global.GetLeftTimeText(endTs)})";
                }
                else
                {
                    text = Global.GetLeftTimeText(endTs);
                }

                return text;
            });
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerUI._instance.OpenPopupMissionStampEvent(eventData.eventIndex);
        }
    }

    public class BannerCoinBonusStageEvent : Banner<CoinBonusStageEvent>
    {
        private CoinBonusStageEvent eventData;

        public override string UniqueKey => $"BannerCoinBonusStageEvent_{eventData.eventIndex}";

        public override void Init(CoinBonusStageEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_coinstage.png");

            if (eventData.playCount < eventData.maxPlayCount)
            {
                Label.text = $"{(eventData.maxPlayCount - eventData.playCount)} / {eventData.maxPlayCount}";
            }
            else
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            Global.SetGameType_CoinBonusStage(ServerRepos.UserCoinBonusStage.event_index, ServerRepos.UserCoinBonusStage.current_stage);
            UIBanner.StartCoroutine(ManagerUI._instance.CoCheckStageDataBeforeOpenPopUpCoinStageReady());
        }
    }

    public class BannerWorldRankingEvent : Banner<CdnWorldRank>
    {
        private CdnWorldRank eventData;

        public override string UniqueKey => $"BannerWorldRankingEvent_{eventData.eventIndex}";

        public override void Init(CdnWorldRank eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_worldrank_{eventData.resourceId}.png");

            var eventState = ManagerWorldRanking.GetEventState();
            switch (eventState)
            {
                case ManagerWorldRanking.EventState.BEFORE_REWARD: SetTextBeforeReward(); break;
                case ManagerWorldRanking.EventState.REWARD: SetTextReward(); break;
                default: SetTextRunning(); break;
            }
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            ManagerWorldRanking.OpenWorldRanking();
        }

        private void SetTextRunning()
        {
            SetTextTime(eventData.deadlineTs, SetTextBeforeReward);
        }

        private void SetTextBeforeReward()
        {
            SetTextTime(Global._instance.GetString("i_rk_1"), eventData.rewardTs, SetTextReward);
        }

        private void SetTextReward()
        {
            SetTextTime(Global._instance.GetString("i_rk_2"), eventData.endTs);
        }

        private void SetTextTime(long endTs, System.Action timeOutCallback = null)
        {
            EndTsTimer.Run(
                target: Label,
                endTs: endTs,
                timeOutAction: timeOutCallback
            );
        }

        private void SetTextTime(string title, long endTs, System.Action timeOutCallback = null)
        {
            EndTsTimer.Run(
                target: Label,
                endTs: endTs,
                overrideTextFunc: (ts) => $"{title}({Global.GetLeftTimeText(endTs)})",
                timeOutAction: timeOutCallback
            );
        }
    }

    public class BannerAdventureEvent : Banner<CdnEventAdventure>
    {
        private CdnEventAdventure eventData;

        public override string UniqueKey => $"BannerAdventureEvent_{eventData.event_idx}";

        public override void Init(CdnEventAdventure eventData)
        {
            this.eventData = eventData;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_adventure_{eventData.event_idx}.png");

            EndTsTimer.Run(Label, eventData.end_ts);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            UIButtonAdventureEvent.OpenEvent();
        }
    }

    public class BannerCaosuleGachaEvent : Banner<CdnCapsuleGacha>
    {    
        private CdnCapsuleGacha eventData;

        public override string UniqueKey => $"BannerCaosuleGachaEvent_{eventData.eventIndex}";

        public override void Init(CdnCapsuleGacha eventData)
        {
            this.eventData = eventData;
            
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_capsule_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(ManagerCapsuleGachaEvent.CheckStartable())
                ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>();
            else
            {
                ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
                    {
                        popup.SortOrderSetting();
                        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
                    });
            }
        }
    }

    public class BannerWelcomeEvent : Banner<CdnWelcomeMission>
    {
        private CdnWelcomeMission eventData;

        public override int? PriorityBanner => 0;
        public override string UniqueKey => $"BannerWelcomeEvent_{eventData.resourceIndex}";

        public override void Init(CdnWelcomeMission eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_welcome_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, ManagerWelcomeEvent.welcomeEndTs());
        }
        
        public override bool IsPriorityPackage()
        {
            if (ManagerWelcomeEvent.CheckStartable())
            {
                if (ManagerWelcomeEvent.isBuyVipPass && ManagerWelcomeEvent.IsBuyVipPass() == false)
                {
                    return false;
                }
                else
                {
                    return ManagerWelcomeEvent.IsBuyVipPass() == false;
                }
            }
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (ManagerWelcomeEvent.CheckStartable())
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopUpWelcomeMission>();
                popup.bAutoOpen = autoOpen;
            }
        }
    }

    public class BannerWelcomeBackEvent : Banner<ServerUserWelcomeBackMission>
    {
        private ServerUserWelcomeBackMission eventData;

        public override int? PriorityBanner => 1;
        public override string UniqueKey => $"BannerWelcomeBackEvent_{eventData.startTs}";

        public override void Init(ServerUserWelcomeBackMission eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_welcome_back_{eventData.contentsType}.png");

            EndTsTimer.Run(Label, ManagerWelcomeBackEvent.GetUserWelcomeBackMission().endTs);
        }
        
        public override bool IsPriorityPackage()
        {
            if (ManagerWelcomeBackEvent.IsActiveEvent())
            {
                if (ManagerWelcomeBackEvent._instance.buyPassNeedReboot && ManagerWelcomeBackEvent.GetUserWelcomeBackMission().buyPass == 0)
                {
                    return false;
                }
                else
                {
                    return ManagerWelcomeBackEvent.GetUserWelcomeBackMission().buyPass > 0 == false;
                }
            }
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (ManagerWelcomeBackEvent.IsActiveEvent())
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopUpWelcomeBackMission>();
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_68"), false);
                popup.SortOrderSetting();
                return;
            }
        }
    }

    public class BannerCoinStashEvent : Banner<CdnCoinStashEvent>
    {
        private CdnCoinStashEvent eventData;

        public override int? PriorityBanner => 2;
        public override string UniqueKey => $"BannerCoinStashEvent_{eventData.resourceIndex}";

        public override void Init(CdnCoinStashEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop/", $"cs_{eventData.resourceIndex}_banner.png");

            EndTsTimer.Run(Label, eventData.end_ts,Global.GetLeftTimeText_DDHHMMSS);
        }
        
        public override bool IsPriorityPackage()
        {
            if (ManagerCoinStashEvent.CheckStartable())
                return true;
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(ManagerCoinStashEvent.CheckStartable())
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupCoinStashEvent>();
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_17"), false);
                systemPopup.SortOrderSetting();
            }
        }
    }

    public class BannerStageAssistMissionEvent : Banner<CdnStageAssistMissionEvent>
    {
        private CdnStageAssistMissionEvent eventData;
        public override string UniqueKey => $"BannerStageAssistMissionEvent_{0}";

        public override void Init(CdnStageAssistMissionEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_relay_mission_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(ManagerStageAssistMissionEvent.CheckStartable())
            {
                ManagerUI._instance.OpenPopup<UIPopUpStageAssistMissionEvent>();
            }
            else
            {
                UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false);
                systemPopup.SortOrderSetting();
            }
        }
    }

    public class BannerLoginADBonus : Banner<CdnLoginAdBonus>
    {
        private CdnLoginAdBonus eventData;

        public override string UniqueKey => $"BannerLoginADBonusEvent_{eventData.type}";

        public override void Init(CdnLoginAdBonus eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_login_ad_{eventData.type}.png");

            EndTsTimer.Run(Label, ManagerLoginADBonus.Instance.GetUserLoginADBonus(eventData.type).endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(ManagerLoginADBonus.CheckStartable(eventData.type))
                ManagerUI._instance.OpenPopup<UIPopupLoginADBonus>((popup) => popup.InitData(eventData.type));
            else
            {
                UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_41"), false);
                systemPopup.SortOrderSetting();
            }
        }
    }

    public class BannerNormalPackage : Banner<ManagerUI.PackageShowData>
    {
        private ManagerUI.PackageShowData bannerData;

        public override int? PriorityBanner => 2;
        public override string UniqueKey => $"BannerNormalPackage_{bannerData.packageData.idx}";

        public override void Init(ManagerUI.PackageShowData bannerData)
        {
            this.bannerData = bannerData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop", $"{bannerData.packageData.image}_banner.png");
            EndTsTimer.Run(Label, bannerData.expireTs);
        }
        
        public override bool IsPriorityPackage()
        {
            if (Global.LeftTime(bannerData.expireTs) > 0)
                return true;
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (Global.LeftTime(bannerData.expireTs) > 0)
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupPackage>((popup) => popup.InitPopUp(bannerData.packageData));
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                OpenPopupExpirationPackage();
            }
        }

        /// <summary>
        /// 기간 종료 안내 팝업
        /// </summary>
        public static void OpenPopupExpirationPackage()
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) => popup.InitSystemPopUp(
                name: Global._instance.GetString("p_t_4"), 
                text: Global._instance.GetString("n_b_17"), 
                useButtons: false
            ));
        }
    }

    public class BannerCompletePackage : Banner<CdnCompletePackage>
    {
        private CdnCompletePackage _bannerData;

        public override int? PriorityBanner => 2;

        public override string UniqueKey => $"BannerCompletePackage_{_bannerData.idx}";

        public override void Init(CdnCompletePackage bannerData)
        {
            _bannerData = bannerData;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop", $"cp_{bannerData.idx}_{bannerData.userSegment}_banner.png");
            EndTsTimer.Run(Label, bannerData.expired_at,Global.GetLeftTimeText_DDHHMMSS);
        }
        
        public override bool IsPriorityPackage()
        {
            if (Global.LeftTime(_bannerData.expired_at) > 0)
            {
                //패키지 모두 구매
                for (var i = 0; i < _bannerData.packages.Count; ++i)
                {
                    //complete 패키지가 아닌경우 continue
                    if (ServerContents.Packages.ContainsKey(_bannerData.packages[i]) == false)
                    {
                        continue;
                    }

                    //컴플리트 패키지 중 하나라도 유저 데이터에 포함되지 않았다면(유저가 구매하지 않았다면) true -> 노출
                    var packageData = ServerContents.Packages[_bannerData.packages[i]];
                    if (ServerRepos.UserShopPackages.Exists(x => x.idx == packageData.idx) == false)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (Global.LeftTime(_bannerData.expired_at) > 0)
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupCompletePackage>((popup) => popup.InitPopUp(_bannerData));
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                BannerNormalPackage.OpenPopupExpirationPackage();
            }
        }
    }

    public class BannerRandomBoxADPackage : Banner<BannerTypeDatas.Package_RandomBox_ShowAD>
    {
        private BannerTypeDatas.Package_RandomBox_ShowAD bannerData;

        public override int? PriorityBanner => 100;

        public override string UniqueKey => $"BannerPackageRandomBox";
        public string checkClickKey => $"BannerPackageRandomBox_clicked"; // 한 번이라도 클릭했는지?

        public bool isCanShowAD => AdManager.ADCheck(bannerData.adType);

        public override void Init(BannerTypeDatas.Package_RandomBox_ShowAD bannerData)
        {
            this.bannerData = bannerData;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop/", $"ad_randombox_{bannerData.resourceIdx}.png");
            if (AdManager.ADCheck(bannerData.adType))
            {
                // 광고 시청 가능한 시점에 도달하면 1회 UniqueKey를 Checker에서 삭제 ( 00:00:00 되는 시점에 맞춰 느낌표 표시 위함 )
                if (PlayerPrefs.GetInt(checkClickKey) != 1)
                {
                    UIItemSidebar.newChecker.DeleteNew(UniqueKey);
                    UIBanner.UpdateNewIcon(UniqueKey);
                }
                Label.text = Global._instance.GetString("li_pack_2");
            }
            else
            {
                int index = ServerRepos.UserAdInfos.FindIndex(i => i.adType == (int)bannerData.adType);
                if (ServerRepos.UserAdInfos[index].usedCount >= ServerContents.AdInfos[(int) bannerData.adType].dailyLimit)
                {
                    Label.text = Global._instance.GetString("li_pack_4");
                    return;
                }

                EndTsTimer.Run(Label, bannerData.expiredTime, (endTs) =>
                {
                    if (AdManager.ADCheck(bannerData.adType) || Global.LeftTime(endTs) <= 0)
                    {
                        if (PlayerPrefs.GetInt(checkClickKey) != 1)
                        {
                            UIItemSidebar.newChecker.DeleteNew(UniqueKey);
                            UIBanner.UpdateNewIcon(UniqueKey);
                        }
                        return Global._instance.GetString("li_pack_2");
                    }
                    else
                        return Global.GetLeftTimeText(endTs);
                });
            }
        }
        

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (AdManager.ADCheck(bannerData.adType))
            {
                // 해당 광고 시청 가능한 시점에 최초 클릭, checkClickKey 등록
                PlayerPrefs.SetInt(checkClickKey, 1);
                ManagerUI._instance.OpenPopup<UIPopupADView>(
                    (popup) =>
                    { 
                        popup.SetPackageRandomBox(ServerContents.AdInfos[(int)AdManager.AdType.AD_13].rewards, 
                            () =>
                            {
                                // 상품을 수령해서 다음 시간대로 넘어가는 경우 checkClickKey (최초 클릭한 기록) 삭제
                                ManagerUI._instance.PackageUpdate();
                                PlayerPrefs.DeleteKey(checkClickKey);
                            });
                    });
            }
            else
            {
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_69"), false);
                popup.SortOrderSetting();
            }
        }
    }
    
    public class BannerSelectPackage : Banner<CdnSelectPackage>
    {
        private CdnSelectPackage bannerData;

        public override int? PriorityBanner => 2;

        public override string UniqueKey => $"BannerSelectPackage_{bannerData.idx}";

        public override void Init(CdnSelectPackage bannerData)
        {
            this.bannerData = bannerData;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop", $"select_{bannerData.assetVersion}_banner.png");
            EndTsTimer.Run(Label, ServerRepos.UserSelectPackage.expiredAt,Global.GetLeftTimeText_DDHHMMSS);
        }
        
        public override bool IsPriorityPackage()
        {
            if (Global.LeftTime(ServerRepos.UserSelectPackage.expiredAt) > 0)
                return true;
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(Global.LeftTime(ServerRepos.UserSelectPackage.expiredAt) > 0)
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupSelectPackage>((popup) =>
                {
                    popup.InitPopUp(bannerData);
                });
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                BannerNormalPackage.OpenPopupExpirationPackage();
            }
        }
    }
    
    public class BannerCBUPackage : Banner<ManagerUI.PackageShowData>
    {
        private ManagerUI.PackageShowData bannerData;

        public override int? PriorityBanner => 2;

        public override string UniqueKey => $"BannerCBUPackage_{bannerData.packageData.idx}";

        public override void Init(ManagerUI.PackageShowData bannerData)
        {
            this.bannerData = bannerData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop", $"{bannerData.packageData.image}_banner.png");
            EndTsTimer.Run(Label, bannerData.expireTs);
        }
        
        public override bool IsPriorityPackage()
        {
            if (Global.LeftTime(bannerData.expireTs) > 0)
                return true;
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (Global.LeftTime(bannerData.expireTs) > 0)
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopupNewPackage>((popup) => popup.InitPopup(bannerData.packageData));
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                BannerNormalPackage.OpenPopupExpirationPackage();
            }
        }
    }
    
    public class BannerDiaStashEvent : Banner<CdnDiaStashEvent>
    {
        private CdnDiaStashEvent eventData;
        public override int? PriorityBanner => 2;
        public override string UniqueKey => $"BannerDiaStashEvent_{eventData.eventIndex}";

        public override void Init(CdnDiaStashEvent eventData)
        {
            this.eventData = eventData;

            //배너 이미지 세팅.
            SetBannerTexture();

            EndTsTimer.Run(Label, eventData.end_ts);

            UIBanner.OnClickEvent += () =>
            {
                if (UIBanner.NewIcon.activeInHierarchy) return;
                
                if(ManagerDiaStash.CheckStartable())
                    UIBanner.NewIcon.SetActive(ManagerDiaStash.instance.IsFullDia());
            };
            
            UIBanner.OnEnableEvent += () =>
            {
                if (UIBanner.NewIcon.activeInHierarchy) return;
                
                if(ManagerDiaStash.CheckStartable())
                    UIBanner.NewIcon.SetActive(ManagerDiaStash.instance.IsFullDia());
            };
        }
        
        public override bool IsPriorityPackage()
        {
            if (ManagerDiaStash.CheckStartable())
                return true;
            return false;
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if(ManagerDiaStash.CheckStartable())
            {
                var popup = ManagerUI._instance.OpenPopup<UIPopUpDiaStashEvent>((popup) => popup.InitData());
                popup.bAutoOpen = autoOpen;
            }
            else
            {
                UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_17"), false);
                systemPopup.SortOrderSetting();
            }
        }

        void SetBannerTexture()
        {
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop/", $"ds_{eventData.resourceIndex}_{ManagerDiaStash.instance.GetPackageGrade()}_banner.png");
        }
    }
    
    public class BannerLuckyRoulette : Banner<CdnLuckyRoulette>
    {
        private CdnLuckyRoulette eventData;

        public override int? PriorityBanner => 2;

        public override string UniqueKey => $"BannerLuckyRoulette_{eventData.vsn}";

        public override void Init(CdnLuckyRoulette bannerData)
        {
            this.eventData = bannerData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "Shop", $"roulette_{bannerData.resourceIndex}_banner.png");
            EndTsTimer.Run(Label, bannerData.endTs);
        }

        public override void OnBannerOpen(bool autoOpen = false)
        {
            if (Global.LeftTime(eventData.endTs) > 0)
            {
                ManagerUI._instance.OpenPopupLuckyRoulette();
            }
            else
            {
                ManagerUI._instance.OpenPopupEventOver();
            }
        }
    }

    #endregion
}