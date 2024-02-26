using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour
{
    public class EventData
    {
        public static bool GetActiveEvent_AdventureEvent()
        {
            return GetAdvEventIndex() != 0 && Global.LeftTime(GetAdvEventEndTS()) > 0;
        }

        public static long GetAdvEventEndTS()
        {
            return ServerContents.EventAdv.end_ts;
        }

        public static int GetAdvEventIndex()
        {
            if (ServerContents.EventAdv == null)
            {
                return 0;
            }
            else
            {
                return ServerContents.EventAdv.event_idx;
            }
        }

        public static int GetAdvEventStageCount()
        {
            if (ServerContents.EventAdvStages == null)
            {
                return 0;
            }
            else
            {
                return ServerContents.EventAdvStages.Count;
            }
        }

        public static bool IsAdvEventBonusAnimal(int animalIdx)
        {
            return ServerContents.EventAdv?.buff_animals?.Contains(animalIdx) ?? false;
        }

        public static int GetAdvEventBonusAnimalCount()
        {
            int count = 0;

            for (int i = 0; i < 3; i++)
            {
                int animalIndex = User.GetAnimalIdxFromDeck(1, i);
                if (IsAdvEventBonusAnimal(animalIndex))
                {
                    count++;
                }
            }

            return count;
        }

        public static int GetAdvEventBonus()
        {
            return ServerContents.EventAdv.buff_animal_percent;
        }

        public static int GetAdvEventBonus(int animalIdx)
        {
            if (IsAdvEventBonusAnimal(animalIdx))
            {
                return GetAdvEventBonus();
            }
            else
            {
                return 0;
            }
        }

        public static List<Reward> GetAdvEventRewards()
        {
            return ServerContents.EventAdv.rewards;
        }

        public static List<Reward> GetAdvEventBoxRewards(int stageIndex)
        {
            return ServerContents.EventAdvStages[stageIndex].box_reward_overview;
        }

        public static List<Reward> GetAdvEventStageRewards(int stageIndex)
        {
            return ServerContents.EventAdvStages[stageIndex].rewards;
        }

        public static StageInfo GetAdvEventStage(int stageIndex)
        {
            CdnEventAdventureStage cdnStage = ServerContents.EventAdvStages?[stageIndex];

            if (cdnStage == null) return null;

            StageInfo eventStage = new StageInfo();
            eventStage.idx = stageIndex;
            eventStage.mission = 0;
            eventStage.firstDropBoxRatio = cdnStage.first_drop_box_ratio;
            eventStage.normalDropBoxRatio = cdnStage.normal_drop_box_ratio;
            eventStage.dropBoxRatio = cdnStage.drop_boxes;
            eventStage.stageType = 0;
            eventStage.rewards = new StageReward()
            {
                exp = cdnStage.normal_exp,
                reward = null,
            };

            if (cdnStage.rewards.Count > 0)
            {
                eventStage.rewards.reward = cdnStage.rewards[0];
            }

            return eventStage;
        }

        public static string GetAdvEventMapFileName(int eventIndex, int stageIndex)
        {
            return "ae_" + eventIndex + "_" + stageIndex + ".xml";
        }

        public static string GetAdvEventTitleName(int eventIndex)
        {
            return "ev_a_title_" + eventIndex;
        }

        public static string GetAdvEventBonusListIconName(int eventIndex)
        {
            return "ev_a_list_" + eventIndex;
        }

        public static string GetAdvEventButtonName(int eventIndex)
        {
            return "ev_a_" + eventIndex;
        }

        #region StageManager
        private const string KEY = "Adv_Event_LastStage";

        public static void SetLastPlayStage()
        {
            SetLastPlayStage(Global.stageIndex);
        }

        public static void SetLastPlayStage(int stageIndex)
        {
            SetLastPlayStage(Global.eventIndex, stageIndex);
        }

        public static void SetLastPlayStage(int eventIndex, int stageIndex)
        {
            PlayerPrefs.SetString(KEY, eventIndex + "," + stageIndex + "," + IsAdvEventStageCleared(stageIndex).ToString());
        }

        public static int GetLastPlayStage()
        {
            if (PlayerPrefs.HasKey(KEY))
            {
                string[] data = PlayerPrefs.GetString(KEY).Split(',');
                int eventIndex = int.Parse(data[0]);

                if (eventIndex == Global.eventIndex)
                {
                    return int.Parse(data[1]);
                }
            }

            return -1;
        }

        public static bool IsAdvEventStageFirstCleard(int stageIndex)
        {
            if(stageIndex == GetLastPlayStage())
            {
                return IsAdvEventStageFirstCleard();
            }
            else
            {
                return false;
            }
        }

        public static bool IsAdvEventStageFirstCleard()
        {
            int lastStage = GetLastPlayStage();

            if (lastStage == -1)
            {
                return false;
            }

            string[] data = PlayerPrefs.GetString(KEY).Split(',');
            bool pastClearState = bool.Parse(data[2]);

            return IsAdvEventStageCleared(lastStage) && !pastClearState;
        }

        public static bool IsAdvEventStageCleared(int stageIndex)
        {
            ServerUserEventAdventureStage adventureEventStage = null;

            foreach (var stage in ServerRepos.UserEventAdventureStages)
            {
                if(stage.stage == stageIndex)
                {
                    adventureEventStage = stage;
                }
            }

            if (adventureEventStage != null)
            {
                return adventureEventStage.flag > 0;
            }
            else
            {
                return false;
            }
        }

        public static bool IsAdvEventStageBossStage(int stage)
        {
            return ServerContents.EventAdvStages.Count == stage;
        }

        public enum ClearState
        {
            NonClear,
            Clear,
            FirstClear
        }

        public static ClearState GetClearState(int stageIndex)
        {
            ClearState clearState;

            if (IsAdvEventStageCleared(stageIndex))
            {
                if (IsAdvEventStageFirstCleard(stageIndex))
                {
                    clearState = ClearState.FirstClear;
                }
                else
                {
                    clearState = ClearState.Clear;
                }
            }
            else
            {
                clearState = ClearState.NonClear;
            }

            return clearState;
        }

        /// <summary>
        /// 모든 이벤트 스테이지를 클리어 했지만, 기간이 지난 경우 보상지급을 위한 기능
        /// </summary>
        /// <returns></returns>
        public static IEnumerator RequestUnclaimedRewards()
        {
            int eventIndex = GetAdvEventIndex();
            if (eventIndex == 0)
                yield break;

            if (Global.LeftTime(GetAdvEventEndTS()) > 0)
                yield break;

            if (ServerRepos.UserEventAdventure.state != 1)
                yield break;

            //보상
            Protocol.AppliedRewardSet rewardSet = null;

            #region Call ServerAPI EventClearReward
            bool getRewardComplete = false;

            var arg = new Protocol.AdventureGetChapterClearRewardReq()
            {
                type = (int)GameType.ADVENTURE_EVENT,
                eventIdx = eventIndex
            };

            bool overdue = false;

            ServerAPI.GetEventAdventureClearReward
            (
                arg,
                (clearRewardResp) => 
                {
                    if (clearRewardResp.addMail == 0) overdue = true;

                    //보상 적용
                    rewardSet = clearRewardResp.clearReward;

                    getRewardComplete = true;
                }
            );

            yield return new WaitUntil(() => getRewardComplete);

            if (overdue) yield break;
            #endregion

            #region Popup Open GetReward
            //수령하지 않은 보상 지급 팝업
            if (rewardSet != null)
            {
                bool isGetReward = false;
                //ManagerUI._instance.OpenPopupGetRewardAlarm
                //    (Global._instance.GetString("adv_ev_4"),
                //    () => { isGetReward = true;},
                //    rewardSet);

                ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupGetRewardAlarm>((popup) =>
                popup.InitPopup(Global._instance.GetString("adv_ev_4"), rewardSet), () => { isGetReward = true; });

                //보상 팝업 종료될 때까지 대기.
                yield return new WaitUntil(() => isGetReward == true);
            }
            #endregion
        }
        #endregion
    }
}
