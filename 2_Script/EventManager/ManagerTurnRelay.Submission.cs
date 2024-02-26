using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerTurnRelay : MonoBehaviour
{
    public enum SubmissionType
    {
        SUBMISSION_CLEAR,       // 서브미션 클리어 갯수
        CLEAR_WAVE_1 = 1,       // 1웨이브 클리어
        CLEAR_WAVE_2,
        CLEAR_WAVE_3,
        CLEAR_WAVE_4,
        CLEAR_WAVE_5,
        // 혹시 모르는 기묘한 상황에 대비해서 사이를 비워둔다
        // 갑자기 100웨이브 하고싶다고 하면...
        CLEAR_ALL_WAVE = 1000,  // 모든웨이브 클리어
        PLAY_COUNT,             // 플레이 횟수
        READYITEM_ADDTURN,      // 레디아이템 선택 - 사과
        READYITEM_LINE,         // 레디아이템 - 라인
        READYITEM_DOUBLE,       // 레디아이템 - 더블폭탄
        READYITEM_RAINBOW,      // 레디아이템 - 레인
        READYITEM_ANY,          // 레디아이템 - 아무꺼나
        TOTAL_SCORE,            // 전체 스코어 (붕어)

        SELITEM_ADDTURN,        // 보너스아이템 선택 - 사과
        SELITEM_LINE,           // 보너스아이템 - 라인
        SELITEM_DOUBLE,         // 보너스아이템 - 더블폭탄
        SELITEM_RAINBOW,        // 보너스아이템 - 레인
        SELITEM_SCORE,          // 보너스아이템 - 붕어

    }

    public enum SubmissionState
    {
        INVALID_STATE = -1, // 이건 뭔가 에러...
        INCOMPLETED = 0,
        COMPLETED,
        REWARD_FINISHED
    }

    public class SubmissionData
    {
        public int idx = 0;
        public SubmissionType type = SubmissionType.SUBMISSION_CLEAR;
        public SubmissionState state = SubmissionState.INVALID_STATE;
        public Reward reward = new Reward();
        public int progress = 0;
        public int targetCount = 0;
    }

    public class TurnRelaySubmission
    {
        //서브미션 데이터
        public List<SubmissionData> listSubMission = new List<SubmissionData>();

        #region 초기화 관련
        public void SyncFromServerUserData()
        {
            listSubMission.Clear();

            for (int i = 0; i < ServerContents.TurnRelayEvent.submissionList.Count; i++)
            {
                SubmissionData submissionData = new SubmissionData();
                submissionData.idx = i;

                //서브미션 타입 및 타겟 카운트 추가
                TVPair targetData = ServerContents.TurnRelayEvent.submissionList[i];
                submissionData.type = (SubmissionType)targetData.type;
                submissionData.targetCount = targetData.value;

                //서브미션 보상 추가
                if (ServerContents.TurnRelayEvent.submissionRewards.Count > i)
                {
                    TVPair pair = ServerContents.TurnRelayEvent.submissionRewards[i];
                    submissionData.reward = new Reward()
                    {
                        type = pair.type,
                        value = pair.value
                    };
                }

                //진행한 카운트 추가
                if (ServerRepos.UserTurnRelayEvent.submissionProgress.Count > i)
                    submissionData.progress = ServerRepos.UserTurnRelayEvent.submissionProgress[i];

                //서브미션 상태 추가
                if (ServerRepos.UserTurnRelayEvent.submissionRewardFlag.Count > i)
                    submissionData.state = (SubmissionState)ServerRepos.UserTurnRelayEvent.submissionRewardFlag[i];

                //리스트에 추가
                listSubMission.Add(submissionData);
            }
        }
        #endregion

        public int GetSubmissionCount()
        {
            return listSubMission.Count;
        }

        public SubmissionData GetSubmission(int idx)
        {
            if (listSubMission.Count <= idx)
                return null;

            return listSubMission[idx];
        }

        public Reward GetSubmissionReward(int idx)
        {
            if (listSubMission.Count <= idx)
                return null;

            return listSubMission[idx].reward;
        }

        public SubmissionState GetSubmissionState(int idx)
        {
            if (listSubMission.Count <= idx)
                return SubmissionState.INVALID_STATE;

            return listSubMission[idx].state;
        }

        //받을 수 있는 보상카운트 가져오는 함수
        public int GetCanReciveRewardCount()
        {
            int count = 0;
            for (int i = 0; i < listSubMission.Count; i++)
            {
                if (listSubMission[i].state == SubmissionState.COMPLETED)
                    count++;
            }
            return count;
        }

        public int GetRecivedRewardCount()
        {
            int count = 0;
            for (int i = 0; i < listSubMission.Count; i++)
            {
                if (listSubMission[i].state == SubmissionState.REWARD_FINISHED)
                    count++;
            }
            return count;
        }
    }

}
