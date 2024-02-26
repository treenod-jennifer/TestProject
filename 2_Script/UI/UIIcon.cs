using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SideIcon
{
    public static class Maker
    {
        public static T MakeIcon<T>(UIItemScrollbar scrollBar, System.Action<T> init) where T : Icon, new()
        {
            UIIcon originPrefab = Resources.Load<UIIcon>($"UIPrefab/UIIcon");
            UIIcon uiIcon = Object.Instantiate(originPrefab, scrollBar.IconRoot);

            T icon = new T();

            icon.UIIcon = uiIcon;

            init(icon);

            scrollBar.AddIcon(icon);

            return icon;
        }
    }

    public class UIIcon : MonoBehaviour
    {
        [SerializeField] private UIUrlTexture texture;
        [SerializeField] private UILabelPlus  label;
        [SerializeField] private GameObject   newIcon;
        [SerializeField] private UIIconBubble bubble;

        public UIUrlTexture Texture { get { return texture; } }

        public UILabelPlus Label { get { return label; } }

        public GameObject NewIcon { get { return newIcon; } }
        
        public UIIconBubble Bubble { get { return bubble; } }
        
        public event System.Action OnClickEvent;
        public event System.Action OnEnableEvent;

        public void OnIconClick()
        {
            if (ManagerLobby._instance == null || !ManagerLobby._instance.IsLobbyComplete)
                return;
            
            OnClickEvent?.Invoke();
        }

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }
        
        public void UpdateNewIcon(string _uniqueKey)
        {
            NewIcon.SetActive(UIItemScrollbar.newChecker.IsNew(_uniqueKey));
        }
    }

    public abstract class Icon
    {
        private UIIcon uiIcon;

        public UIIcon UIIcon 
        {
            protected get
            {
                return uiIcon;
            }
            set
            {
                uiIcon = value;
                uiIcon.OnClickEvent += () =>
                {
                    if (UniqueKey.Contains("IconEventQuest"))
                    {
                        UIItemScrollbar.newChecker.SetNew(UniqueKey);
                        uiIcon.UpdateNewIcon(UniqueKey);
                    }

                    OnIconClick();
                };
                uiIcon.OnEnableEvent += () => 
                {
                    if (UniqueKey.Contains("IconEventQuest"))
                        uiIcon.UpdateNewIcon(UniqueKey);
                };
                ManagerLobby._instance.OnEventHighlight += () =>
                {
                    OnEventHighlight();
                };
            }
        }

        public virtual int? PriorityIcon { get; } = null;

        public GameObject GameObject { get { return UIIcon.gameObject; } }

        protected UIUrlTexture Texture { get { return UIIcon.Texture; } }

        protected UILabelPlus Label { get { return UIIcon.Label; } }

        public abstract void OnIconClick();

        public virtual ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.NONE;
        }

        /// <summary>
        /// 해당 아이콘을 클릭 했는지 확인을 위해, 아이콘별 고유한 Key값을 설정해 주어야 합니다. (4.11.0 빌드 기준 사용하지 않고, 정의만 해줌.)
        /// </summary>
        public abstract string UniqueKey { get; }
        
        //이벤트 아이콘 강조 이펙트 사용 여부
        protected virtual bool UseIconEffect { get; }
        
        public bool IsActiveNewIcon()
        {
            return UIIcon.NewIcon.activeSelf;
        }

        public bool IsActiveBubble()
        {
            return UIIcon.Bubble != null && UIIcon.Bubble.gameObject.activeSelf;
        }
        
        public UIIconBubble GetBubble()
        {
            return UIIcon.Bubble;
        }
        
        protected virtual void OnEventHighlight()
        {
            if (UseIconEffect == false)
                return;
            
            if (ManagerUI._instance.ScrollbarRight.CheckIconPosition(this))
                return;
            
            ManagerUI._instance.SpawnEffect(uiIcon.transform, Texture);
        }
    }

    /// <summary>
    /// 커스텀 아이콘을 만들기 위한 베이스 클래스 입니다.
    /// </summary>
    /// <typeparam name="T">아이콘의 초기화에 필요한 데이터 형태 입니다.</typeparam>
    public abstract class Icon<T> : Icon
    {
        //PlayerPrefs에 들어있는 값을 참조합니다.
        public abstract void Init(T bannerData);

        //PlayerPrefs와 무관하게 껐다 켭니다.
        public void SetNewIcon(bool activeNewIcon)
        {
            UIIcon.NewIcon.SetActive(activeNewIcon);
        }
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

    #region 커스텀 아이콘 정의

    public class IconStageEvent : Icon<CdnEventChapter>
    {
        private CdnEventChapter eventData;

        public override string UniqueKey => $"IconStageEvent_{eventData.index}";
        
        protected override bool UseIconEffect => true;

        public override void Init(CdnEventChapter eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_stage_{eventData.index}.png");

            EndTsTimer.Run(Label, eventData.endTs);
            if(ManagerEventStage.instance != null)
                SetNewIcon(ManagerEventStage.instance.IsPlayCurrentVersion() == false);
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.EVENT_STAGE;
        }
        
        public override void OnIconClick()
        {
            ManagerEventStage.instance.OnEventIconClick();
        }
    }

    public class IconPokoFlowerEvent : Icon<CdnPokoFlowerEvent>
    {
        private CdnPokoFlowerEvent eventData;

        public override string UniqueKey => $"IconPokoFlowerEvent_{eventData.event_index}";

        public override void Init(CdnPokoFlowerEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_flower_{eventData.resource_index}.png");

            EndTsTimer.Run(Label, eventData.end_ts,Global.GetLeftTimeText_DDHHMMSS);
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.POKO_FLOWER;
        }
        
        public override void OnIconClick()
        {
            ManagerPokoFlowerEvent.instance.OnEventIconClick();
        }
    }

    public class IconSpecialEvent : Icon<int>
    {
        private int specialEventIndex;

        public override string UniqueKey => $"IconSpecialEvent_{specialEventIndex}";

        public override void Init(int specialEventIndex)
        {
            this.specialEventIndex = specialEventIndex;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_special_{specialEventIndex}.png");

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
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.SPECIAL_EVENT;
        }
        
        public override void OnIconClick()
        {
            ManagerSpecialEvent.instance.OnEventIconClick();
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

    public class IconAlphabetEvent : Icon<int>
    {
        int currentGruop = 0;

        public override string UniqueKey => $"IconAlphabetEvent_{ManagerAlphabetEvent.instance.eventIndex}";

        public override void Init(int currentGroup)
        {
            currentGruop = currentGroup;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_alphabet_{ManagerAlphabetEvent.instance.resourceIndex}.png");

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
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.ALPHABET;
        }
        
        public override void OnIconClick()
        {
            ManagerAlphabetEvent.instance.OnEventIconClick();
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

    public class IconTurnRelayEvent : Icon<CdnTurnRelayEvent>
    {
        private CdnTurnRelayEvent eventData;

        public override string UniqueKey => $"IconTurnRelayEvent_{eventData.eventIndex}";

        public override void Init(CdnTurnRelayEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_turn_relay_{eventData.resourceIndex}.png");

            var eventState = ManagerTurnRelay.GetEventState();
            switch (eventState)
            {
                case ManagerTurnRelay.EventState.BEFORE_REWARD: SetTextBeforeReward(); break;
                case ManagerTurnRelay.EventState.REWARD:        SetTextReward(); break;
                default:                                        SetTextRunning(); break;
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

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.TURN_RELAY;
        }
        
        public override void OnIconClick()
        {
            ManagerTurnRelay.instance.OnEventIconClick();
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

    public class IconStageChallenge : Icon<CdnStageChallenge>
    {
        private CdnStageChallenge eventData;

        public override string UniqueKey => $"IconStageChallengeEvent_{eventData.eventIndex}_{ManagerStageChallenge.instance.EventStepIndex}";

        public override void Init(CdnStageChallenge eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_stagechallenge.png");

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

            //아이콘 텍스트 변경
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
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.STAGE_CHALLENGE;
        }
        
        public override void OnIconClick()
        {
            ManagerStageChallenge.instance.OnEventIconClick();
        }
    }

    public class IconMoleCatchEvent : Icon<UseResourceEvent>
    {
        private UseResourceEvent eventData;

        public override string UniqueKey => $"IconMoleCatchEvent_{eventData.eventIndex}";

        public override void Init(UseResourceEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_mole_catch_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs);
            if(ManagerMoleCatch.instance != null)
                SetNewIcon(ManagerMoleCatch.instance.IsPlayCurrentVersion() == false);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.MOLE_CATCH_EVENT;
        }
        
        public override void OnIconClick()
        {
            ManagerMoleCatch.instance.OnEventIconClick();
        }
    }

    public class IconStickerEvent : Icon<NormalEvent>
    {
        private NormalEvent eventData;

        public override string UniqueKey => $"IconStickerEvent_{eventData.eventIndex}";

        public override void Init(NormalEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"icon_line_stamp_{eventData.eventIndex}");
            EndTsTimer.Run(Label, eventData.endTs, (endTs) =>
            {
                string text;

                if (ServerRepos.UserEventStickers.Count > 0 && ServerRepos.UserEventStickers[0].state != 0)
                {
                    text = $"{Global._instance.GetString("li_ev_2")}";
                }
                else
                {
                    text = Global.GetLeftTimeText(endTs);
                }

                return text;
            });
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            //추후 통합 이벤트 팝업에 출력되도록 개선된다면 작업 필요 (라인 스탬프)
            return ManagerIntegratedEvent.IntegratedEventType.NONE;
        }

        public override void OnIconClick()
        {
            ManagerStickerEvent.instance.OnEventIconClick(eventData.eventIndex);
        }
    }

    public class IconCoinBonusStageEvent : Icon<CoinBonusStageEvent>
    {
        private CoinBonusStageEvent eventData;

        public override string UniqueKey => $"IconCoinBonusStageEvent_{eventData.eventIndex}";

        public override void Init(CoinBonusStageEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_coinstage.png");

            if (eventData.playCount < eventData.maxPlayCount)
            {
                Label.text = $"{(eventData.maxPlayCount - eventData.playCount)} / {eventData.maxPlayCount}";
            }
            else
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
            if(ManagerCoinBonusStage.instance != null && ManagerCoinBonusStage.instance != null)
                SetNewIcon(ManagerCoinBonusStage.instance.IsPlayCurrentVersion() == false);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.COIN_BONUS;
        }
        
        public override void OnIconClick()
        {
            ManagerCoinBonusStage.instance.OnEventIconClick();
        }
    }

    public class IconWorldRankingEvent : Icon<CdnWorldRank>
    {
        private CdnWorldRank eventData;

        public override string UniqueKey => $"IconWorldRankingEvent_{eventData.eventIndex}";

        protected override bool UseIconEffect => true;

        public override void Init(CdnWorldRank eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_worldrank_{eventData.resourceId}.png");

            var eventState = ManagerWorldRanking.GetEventState();
            switch (eventState)
            {
                case ManagerWorldRanking.EventState.BEFORE_REWARD: SetTextBeforeReward(); break;
                case ManagerWorldRanking.EventState.REWARD: SetTextReward(); break;
                case ManagerWorldRanking.EventState.EVENT_END: SetTextEventEnd(); break;
                default: SetTextRunning(); break;
            }

            if(ManagerWorldRanking.instance != null)
                SetNewIcon(ManagerWorldRanking.instance.IsPlayCurrentVersion() == false);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.WORLD_RANKING;
        }
        
        public override void OnIconClick()
        {
            ManagerWorldRanking.instance.OnEventIconClick();
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
            SetTextTime(Global._instance.GetString("i_rk_2"), eventData.endTs, SetTextEventEnd);
        }

        private void SetTextEventEnd()
        {
            Label.text = Global._instance.GetString("i_rk_3");
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

    public class IconAdventureEvent : Icon<CdnEventAdventure>
    {
        private CdnEventAdventure eventData;

        public override string UniqueKey => $"IconAdventureEvent_{eventData.event_idx}";

        public override void Init(CdnEventAdventure eventData)
        {
            this.eventData = eventData;
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_adventure_{eventData.event_idx}.png");

            EndTsTimer.Run(Label, eventData.end_ts);
            if(ManagerAdventureEvent.instance != null)
                SetNewIcon(ManagerAdventureEvent.instance.IsPlayCurrentVersion() == false);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.ADVENTURE_EVENT;
        }
        
        public override void OnIconClick()
        {
            ManagerAdventureEvent.instance.OnEventIconClick();
        }
    }

    public class IconCaosuleGachaEvent : Icon<CdnCapsuleGacha>
    {    
        private CdnCapsuleGacha eventData;

        public override string UniqueKey => $"IconCaosuleGachaEvent_{eventData.eventIndex}";

        public override void Init(CdnCapsuleGacha eventData)
        {
            this.eventData = eventData;
            
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_capsule_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.CAPSULE_GACHA;
        }
        
        public override void OnIconClick()
        {
           ManagerCapsuleGachaEvent.Instance.OnEventIconClick();
        }
    }

    public class IconStageAssistMissionEvent : Icon<CdnStageAssistMissionEvent>
    {
        private CdnStageAssistMissionEvent eventData;
        public override string UniqueKey => $"IconStageAssistMissionEvent_{0}";

        public override void Init(CdnStageAssistMissionEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_relay_mission_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs, Global.GetLeftTimeText_DDHHMMSS);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.STAGE_ASSIST_MISSION_EVENT;
        }
        
        public override void OnIconClick()
        {
            ManagerStageAssistMissionEvent.Instance.OnEventIconClick();
        }
    }

    public class IconLoginADBonus : Icon<CdnLoginAdBonus>
    {
        private CdnLoginAdBonus eventData;

        public override string UniqueKey => $"IconLoginADBonusEvent_{eventData.type}";

        public override void Init(CdnLoginAdBonus eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_login_ad_{eventData.type}.png");

            EndTsTimer.Run(Label, ManagerLoginADBonus.Instance.GetUserLoginADBonus(eventData.type).endTs,
                Global.GetLeftTimeText_DDHHMMSS);
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return eventData.type switch
            {
                1 => ManagerIntegratedEvent.IntegratedEventType.LOGIN_AD_BONUS_NRU,
                2 => ManagerIntegratedEvent.IntegratedEventType.LOGIN_AD_BONUS_CBU,
                _ => ManagerIntegratedEvent.IntegratedEventType.NONE
            };
        }
        
        public override void OnIconClick()
        {
            ManagerLoginADBonus.Instance.OnEventIconClick(eventData.type);
        }
    }

    public class IconDecoCollectionEvent : Icon<CdnDecoCollectionEvent>
    {
        private CdnDecoCollectionEvent eventData;

        public override string UniqueKey => $"IconDecoCollectionEvent_{eventData.eventIndex}";

        public override void Init(CdnDecoCollectionEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_deco_collection.png");

            if ( !ManagerDecoCollectionEvent.IsGetReward )
            {
                Label.text = $"{ManagerDecoCollectionEvent.DecoCollectCount}/{eventData.maxCount}";
            }
            else
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.DECO_COLLECTION;
        }
        
        public override void OnIconClick()
        {
            ManagerDecoCollectionEvent.instance.OnEventIconClick();
        }

        public void RefreshCollectCount()
        {
            if ( !ManagerDecoCollectionEvent.IsGetReward )
            {
                Label.text = $"{ManagerDecoCollectionEvent.DecoCollectCount}/{eventData.maxCount}";
            }
            else
            {
                Label.text = Global._instance.GetString("li_ev_2");
            }
        }
    }
    
    public class IconEventQuest : Icon<List<QuestGameData>>
    {
        // ManagerEventQuest의 eventQuestList는 매번 갱신되지만 갱신된 해당 데이터는 dataList에 반영되지 않음
        // dataList의 데이터는 시간 등을 매번 체크해서 타이머나 시스템 팝업에 반영하기 때문에 ManagerEventQuest.eventQuestList와 동일하게 제거되면 관련 체크가 불가능함
        private List<QuestGameData> dataList;
        public override string UniqueKey => $"IconEventQuest_{dataList[0].index}";
        public override void Init(List<QuestGameData> dataList)
        {
            this.dataList = dataList;
            
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_eventQuest.png");
            SetNewIcon();

            EndTsTimer.Run(Label, dataList[0].valueTime1);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.EVENT_QUEST;
        }
        
        public override void OnIconClick()
        {
            ManagerEventQuest.instance.OnEventIconClick();
        }

        public void OnIconClick_QuestAlarm()
        {
            UIItemScrollbar.newChecker.SetNew(UniqueKey);
            OnIconClick();
        }

        public void SetNewIcon(bool getReward = false)
        {
            bool canReward = false;

            var enumerator_Q = ManagerEventQuest.eventQuestList.GetEnumerator();
            List<QuestGameData> dataList = new List<QuestGameData>();
            while (enumerator_Q.MoveNext())
            {
                QuestGameData qData = enumerator_Q.Current;
                //끝나는 시간이 없거나, 이벤트가 끝나는 시간이 현재시간보다 더 많이 남았을 경우 리스트에 추가.
                if (qData.valueTime1 >= Global.GetTime())
                {
                    dataList.Add(enumerator_Q.Current);
                }
            }

            foreach (var item in dataList)
                if (item.state == QuestState.Completed)
                    canReward = true;
            
            if (getReward)
            {
                if (!canReward)
                    UIItemScrollbar.newChecker.SetNew(UniqueKey);
            }
            else
            {
                if (canReward)
                    UIItemScrollbar.newChecker.DeleteNew(UniqueKey);
            }
            UIIcon.UpdateNewIcon(UniqueKey);
        }

        public void SetText()
        {
            if (ManagerEventQuest.eventQuestList != null && ManagerEventQuest.eventQuestList.Count == 0)
            {
                if (Global.LeftTime(dataList[0].valueTime1) > 0)
                {
                    Label.text = Global._instance.GetString("btn_16");
                    EndTsTimer.Stop(Label);
                }
            }
        }
    }
    
    public class IconEndContentsEvent : Icon<CdnEndContentsEvent>
    {
        private CdnEndContentsEvent eventData;

        public override string UniqueKey => $"IconEndContentsEvent_{eventData.eventIndex}";

        public override void Init(CdnEndContentsEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_end_contents_{eventData.resourceIndex}.png");
            Label.text = $"{ManagerEndContentsEvent.instance.EventAP}/{eventData.maxAp}";
            if (ManagerEndContentsEvent.instance != null)
            {
                if (ManagerEndContentsEvent.instance.EventAP > 0)
                    ManagerEndContentsEvent.instance.CheckTimer();
                else
                    EndTsTimer.Run(Label, ManagerEndContentsEvent.instance.ApChargeAt, null, () =>
                    {
                        ManagerEndContentsEvent.instance.SyncFromDummyUserData_AP();
                        RefreshCollectCount(1);
                        ManagerEndContentsEvent.instance.CheckTimer();
                    });
            }

        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.END_CONTENTS;
        }
        
        public override void OnIconClick()
        {
            ManagerEndContentsEvent.instance.OnEventIconClick();
        }

        public void RefreshCollectCount(int ap)
        {
            Label.text = $"{ap}/{eventData.maxAp}";
        }
    }
    
    public class IconTreasureHunt : Icon<CdnTreasureHunt>
    {
        private CdnTreasureHunt eventData;

        public override string UniqueKey => $"IconTreasureHuntEvent_{eventData.eventIndex}";

        public override void Init(CdnTreasureHunt eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_treasure_{eventData.resourceIndex}.png");
            EndTsTimer.Run(Label, eventData.endTs);
            if(ManagerTreasureHunt.instance != null)
                SetNewIcon(ManagerTreasureHunt.instance.IsPlayCurrentVersion() == false);
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.TREASURE_HUNT;
        }
        
        public override void OnIconClick()
        {
            ManagerTreasureHunt.instance.OnEventIconClick();
        }
    }

    public class IconBingoEvent : Icon<CdnBingoEvent>
    {
        private CdnBingoEvent eventData;
        
        public override string UniqueKey => $"IconBingoEvent_{eventData.eventIndex}";

        public override void Init(CdnBingoEvent eventData)
        {
            this.eventData = eventData;
            
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_bingo_event_{eventData.resourceIndex}.png");

            if (ManagerBingoEvent.instance != null)
            {
                if (ManagerBingoEvent.IsActiveEvent())
                {
                    EndTsTimer.Run(Label, ManagerBingoEvent.instance.EndTs);
                    SetNewIcon(ManagerBingoEvent.instance.IsPlayCurrentVersion() == false);
                }
            }
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.BINGO_EVENT;
        }
        
        public override void OnIconClick()
        {
           ManagerBingoEvent.instance.OnEventIconClick();
        }
    }
    
    public class IconGuideEvent : Icon<LoginCdn>
    {
        public override string UniqueKey => "IconGuideEvent";

        public override void Init(LoginCdn eventData)
        {
            Label.transform.parent.gameObject.SetActive(false);
            
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"icon_guide_{eventData.tutorialCollaboResource}.png");
        }

        public override void OnIconClick()
        {
            ManagerUI._instance.OpenPopup<UIPopupGuideEvent>((popup)=>popup.InitPopup());
        }
    }
    
    public class IconAntiqueStore : Icon<CdnAntiqueStore>
    {
        private CdnAntiqueStore eventData;
        
        public override string UniqueKey => $"IconAntiqueStore_{eventData.eventIndex}";

        public override void Init(CdnAntiqueStore eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/",
                $"e_icon_antique_store_{(ManagerAntiqueStore.IsSpecialEventCheck() ? $"2" : $"1")}_{ServerContents.AntiqueStore.resourceIndex}.png");

            if (ManagerAntiqueStore.instance != null)
            {
                if (ManagerAntiqueStore.CheckStartable())
                {
                    EndTsTimer.Run(Label, ServerContents.AntiqueStore.endTs);
                }
            }
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.ANTIQUE_STORE;
        }

        public override void OnIconClick()
        {
            ManagerAntiqueStore.instance.OnEventIconClick();
        }
    }

    public class IconCriminalEvent : Icon<CdnCriminalEvent>
    {
        private CdnCriminalEvent eventData;

        public override string UniqueKey => $"IconCriminalEvent_{eventData.vsn}";

        public override void Init(CdnCriminalEvent eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_criminal_{eventData.resourceIndex}.png");
            EndTsTimer.Run(Label, eventData.endTs);
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.CRIMINAL_EVENT;
        }

        public override void OnIconClick()
        {
            ManagerCriminalEvent.instance.OnEventIconClick();
        }
    }

    public class IconAtelier : Icon<CdnAtelier>
    {
        public override string UniqueKey => $"IconAtelier_{ServerContents.Atelier.vsn}";

        public override void Init(CdnAtelier eventData)
        {
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_atelier_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, eventData.endTs, Global.GetLeftTimeText_DDHHMMSS);
            
            if(ManagerAtelier.instance != null)
            {
                SetNewIcon(ManagerAtelier.instance.IsPlayCurrentVersion() == false);
            }
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType() => ManagerIntegratedEvent.IntegratedEventType.ATELIER;

        public override void OnIconClick() => ManagerAtelier.instance.OnEventIconClick();
    }

    
    public class IconNoyBoostEvent : Icon<CdnNoyBoostEvent>
    {
        public override string UniqueKey => $"IconNoyBoostEvent_{ManagerNoyBoostEvent.instance.EventIndex}";

        public override void Init(CdnNoyBoostEvent eventData)
        {
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "e_icon_noy_boost.png");

            EndTsTimer.Run(Label, eventData.endTs);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType()
        {
            return ManagerIntegratedEvent.IntegratedEventType.NOY_BOOST;
        }

        public override void OnIconClick()
        {
            ManagerNoyBoostEvent.instance.OnEventIconClick();
        }
    }
    
    public class IconSpaceTravel : Icon<CdnSpaceTravel>
    {
        public override string UniqueKey => $"IconSpaceTravel_{ManagerSpaceTravel.instance.EventIndex}";

        public override void Init(CdnSpaceTravel eventData)
        {
            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_space_travel_{eventData.resourceIndex}.png");
            if (ManagerSpaceTravel.instance != null)
            {
                SetNewIcon(ManagerSpaceTravel.instance.IsPlayCurrentVersion() == false);
            }

            EndTsTimer.Run(Label, eventData.endTs, Global.GetLeftTimeText_DDHHMMSS);
        }
        
        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType() => ManagerIntegratedEvent.IntegratedEventType.SPACE_TRAVEL;
        public override void OnIconClick() => ManagerSpaceTravel.instance.OnEventIconClick();
    }

    public class IconGroupRanking : Icon<CdnGroupRanking>
    {
        private CdnGroupRanking _eventData;

        public override string UniqueKey => $"IconGroupRanking_{_eventData.eventIndex}";

        public override void Init(CdnGroupRanking eventData)
        {
            this._eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_icon_grouprank_{eventData.resourceId}.png");

            UpdateIconState();
        }

        public override ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType() => ManagerIntegratedEvent.IntegratedEventType.GROUP_RANKING;

        public override void OnIconClick() => ManagerGroupRanking.instance.OnEventIconClick();

        public void UpdateIconState()
        {
            var eventState = ManagerGroupRanking.GetEventState();
            SetTimeLabel(eventState);
            SetBubble(eventState);
        }

        private void SetTimeLabel(ManagerGroupRanking.EventState state)
        {
            switch (state)
            {
                case ManagerGroupRanking.EventState.NEED_PARTICIPATE:
                case ManagerGroupRanking.EventState.RUNNING:
                    EndTsTimer.Run(Label, _eventData.deadlineTs, null, SetRunningTimerOver);
                    break;
                case ManagerGroupRanking.EventState.REWARD:
                    SetRunningTimerOver();
                    break;
                case ManagerGroupRanking.EventState.EVENT_END:
                    SetEndTimer();
                    break;
            }
        }

        private void SetRunningTimerOver()
        {
            if (ServerRepos.UserGroupRanking.isParticipationRewardReceived)
            {
                EndTsTimer.Run(Label, _eventData.endTs, null, SetEndTimer);
                SetBubble(ManagerGroupRanking.EventState.REWARD);
            }
            else
            {
                SetEndTimer();
            }
        }
        
        private void SetEndTimer()
        {
            Label.text = "00:00:00";
            SetBubble(ManagerGroupRanking.EventState.EVENT_END);
        }

        private void SetBubble(ManagerGroupRanking.EventState state)
        {
            switch (state)
            {
                case ManagerGroupRanking.EventState.NEED_PARTICIPATE:
                    UIIcon.Bubble.gameObject.SetActive(true);
                    UIIcon.Bubble.SetBubble(_eventData.participationReward);
                    break;
                case ManagerGroupRanking.EventState.RUNNING:
                    UIIcon.Bubble.gameObject.SetActive(true);
                    UIIcon.Bubble.SetBubble(Global._instance.GetString("icon_19"));
                    break;
                case ManagerGroupRanking.EventState.REWARD:
                    UIIcon.Bubble.gameObject.SetActive(true);
                    UIIcon.Bubble.SetBubble(Global._instance.GetString("icon_20"));
                    break;
                default:
                    UIIcon.Bubble.gameObject.SetActive(false);
                    break;
            }
        }
    }
    
    #endregion
}