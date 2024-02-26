using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public partial class ManagerTurnRelay : MonoBehaviour
{
    public enum CoopMissionState
    {
        INVALID_STATE = -1, // 이건 뭔가 에러...
        INCOMPLETED = 0,    // 미션에 도달하지 못한 상태
        RUNNING,            // 미션 진행중인 상태
        COMPLETED,          // 미션 완료한 상태 - 보상받기 전
        REWARD_FINISHED     // 미션 완료한 상태 - 보상받은 후
    }

    public class CoopMissionData
    {
        public int index = -1;
        public Reward rewardData = new Reward();
        public CoopMissionState state = CoopMissionState.INCOMPLETED;
        public int targetCount = 0;
        public int progress = 0;
    }

    public class TurnRelayCooperation
    {
        #region 컨텐츠 데이터
        //순위 별 받게되는 보상 리스트
        public List<Reward> listRankReward = new List<Reward>();
        #endregion

        #region 유저 데이터
        // 협동 미션 데이터
        public List<CoopMissionData> listCoopMissionData = new List<CoopMissionData>();

        //그룹에서 달성한 점수 합산
        public int groupTotalScore = 0;

        //그룹이 현재 도달한 미션 인덱스
        public int currentMissionIdx = 0;

        //내 점수를 제외한 그룹의 스코어(알림에서 사용)
        public int groupScore = -1;

        //내 점수(알림 및 인게임에서 사용)
        public int myScore = 0;
        #endregion

        #region 초기화 관련
        public void SyncFromServerContentsData()
        {
            listRankReward.Clear();
            for (int i=0; i< ServerContents.TurnRelayEvent.coopRankRewards.Count; i++)
            {
                TVPair tempData = ServerContents.TurnRelayEvent.coopRankRewards[i];
                Reward tempReward = new Reward()
                {
                    type = tempData.type,
                    value = tempData.value
                };
                listRankReward.Add(tempReward);
            };
        }

        public void SyncFromServerUserData()
        {
            InitCoopMission();
        }

        private void InitCoopMission()
        {
            listCoopMissionData.Clear();

            int totalTargetCount = 0;

            for (int i = 0; i < ServerContents.TurnRelayEvent.coopAchieveRewards.Count; i++)
            {
                CoopMissionData tempData = new CoopMissionData();

                //인덱스 설정
                tempData.index = i;

                //보상 데이터 추가
                TVPair pair = ServerContents.TurnRelayEvent.coopAchieveRewards[i];
                tempData.rewardData = new Reward()
                {
                    type = pair.type,
                    value = pair.value
                };

                int targetCount = (ServerContents.TurnRelayEvent.coopTargetCounts.Count <= i) ?
                    0 : ServerContents.TurnRelayEvent.coopTargetCounts[i];

                //타겟 카운트 추가
                tempData.targetCount = targetCount;

                bool isAchiveTarget = (groupTotalScore >= totalTargetCount + tempData.targetCount);

                //보상 상태 추가
                tempData.state = GetMissionState(i, isAchiveTarget);

                //보상 달성 단계 추가                
                tempData.progress = (isAchiveTarget == true) ?
                    tempData.targetCount : (groupTotalScore - totalTargetCount);

                totalTargetCount += tempData.targetCount;

                listCoopMissionData.Add(tempData);
            }

            InitCurrentCoopMissionIdx();
        }

        private CoopMissionState GetMissionState(int index, bool isAchiveTarget)
        {
            if (ServerRepos.UserTurnRelayEvent.coopAchRewardFlag.Count < index)
                return CoopMissionState.INVALID_STATE;

            int state = ServerRepos.UserTurnRelayEvent.coopAchRewardFlag[index];
            if (state == 0)
            {
                //미션 목표에 도달했지만, 보상을 받지 않은 상태
                if (isAchiveTarget == true)
                    return CoopMissionState.COMPLETED;

                //현재 진행중인 미션
                if (index == 0)
                {   //목표를 달성하지 않은 상태의 첫번째 미션이면, 현재 진행중인 미션
                    return CoopMissionState.RUNNING;
                }
                else
                {   //목표를 달성하지 않은 상태에서 이전 목표는 달성한 상태라면, 현재 진행중인 미션
                    CoopMissionState prevMissionState = (listCoopMissionData.Count <= (index - 1)) ?
                        CoopMissionState.INVALID_STATE : listCoopMissionData[index - 1].state;

                    if (prevMissionState == CoopMissionState.COMPLETED || prevMissionState == CoopMissionState.REWARD_FINISHED)
                    {
                        return CoopMissionState.RUNNING;
                    }
                }

                //미션에 도달하지 못한 상태
                return CoopMissionState.INCOMPLETED;
            }
            else
            {
                return CoopMissionState.REWARD_FINISHED;
            }
        }

        public void InitCurrentCoopMissionIdx()
        {
            //현재 미션 인덱스는 아직 보상을 받을 수 없는 상태의 가장 최근 미션
            currentMissionIdx = listCoopMissionData.FindIndex(x => x.state == CoopMissionState.RUNNING);

            //보상을 받을수 없는 상태의 미션이 없다면, 마지막 미션이 현재 미션
            if (currentMissionIdx == -1)
                currentMissionIdx = listCoopMissionData.Count - 1;
        }
        #endregion

        #region 협동 미션 관련 함수        
        public int GetCoopMissionCount()
        {
            return listCoopMissionData.Count;
        }

        //인덱스에 해당하는 미션 데이터 가져오기
        public CoopMissionData GetCoopMission(int idx)
        {
            if (listCoopMissionData.Count <= idx)
                return null;

            return listCoopMissionData[idx];
        }
        #endregion

        public Reward GetCoopReward(int idx)
        {
            if (idx < 0 || listRankReward.Count <= idx)
                return null;
            return listRankReward[idx];
        }

        public IEnumerator QueryGroupRanking(List<TurnRelayRankData> list)
        {
            var myRank = new MyRankQueryResult();
            yield return QueryMyEntry_Server(myRank);

            if (myRank.found == false)
            {
                Debug.Log("My Rank Not Found");
                yield break;
            }

            var groupSize = ServerContents.TurnRelayEvent.coopGroupSize;
            int groupStartRank = (((int)((myRank.rank - 1) / groupSize)) * groupSize);

            List<string> groupUserList = new List<string>();

            //같은 그룹에 있는 유저리스트를 가져옴
            yield return QueryUsers(ServerContents.TurnRelayEvent.tableIdEntry, groupStartRank, groupSize, groupUserList);

            //같은 그룹에 있는 유저들의 점수 정보를 가져옴
            yield return QueryRankingSpecificUsers(ServerContents.TurnRelayEvent.tableIdRank, groupUserList, list);

            //같은 그룹에 있는 유저들의 프로필 정보를 가져옴
            yield return QueryRankingProfiles(list);

            long totalScore = 0;
            foreach (var rankData in list)
            {
                if (rankData.userKey == SDKGameProfileManager._instance.GetMyProfile()._userKey)
                    myScore = (int)rankData.scoreValue;
                totalScore += rankData.scoreValue;
            }
            groupTotalScore = (int)totalScore;
            groupScore = (int)(totalScore - myScore);
        }

        #region 알람 데이터
        //알림을 위해서 랭킹 데이터만 가져옴
        public IEnumerator QueryGroupRanking_ForAlarm()
        {
            List<TurnRelayRankData> list = new List<TurnRelayRankData>();

            var myRank = new MyRankQueryResult();
            yield return QueryMyEntry_Server(myRank);

            if (myRank.found == false)
            {
                Debug.Log("My Rank Not Found");
                yield break;
            }

            var groupSize = ServerContents.TurnRelayEvent.coopGroupSize;
            int groupStartRank = (((int)((myRank.rank - 1) / groupSize)) * groupSize);

            List<string> groupUserList = new List<string>();

            //같은 그룹에 있는 유저리스트를 가져옴
            yield return QueryUsers(ServerContents.TurnRelayEvent.tableIdEntry, groupStartRank, groupSize, groupUserList);

            //같은 그룹에 있는 유저들의 점수 정보를 가져옴
            yield return QueryRankingSpecificUsers(ServerContents.TurnRelayEvent.tableIdRank, groupUserList, list);

            long totalScore = 0;
            foreach (var rankData in list)
            {
                if (rankData.userKey == SDKGameProfileManager._instance.GetMyProfile()._userKey)
                    myScore = (int)rankData.scoreValue;
                totalScore += rankData.scoreValue;
            }
            groupTotalScore = 0;
            groupScore = (int)(totalScore - myScore);
        }

        public int GetCanReciveRewardCount()
        {
            //현재 데이터를 검사해, 보상을 받을 수 있는 미션을 검사
            int canReciveRewardCnt = 0;
            int groupTotalScore = groupScore + myScore;
            int totalTargetCount = 0;
            for (int i = 0; i < listCoopMissionData.Count; i++)
            {
                CoopMissionData tempData = listCoopMissionData[i];
                bool isAchiveTarget = (groupTotalScore >= totalTargetCount + tempData.targetCount);
                if (isAchiveTarget == true && tempData.state != CoopMissionState.REWARD_FINISHED)
                    canReciveRewardCnt++;

                totalTargetCount += tempData.targetCount;
            }

            return canReciveRewardCnt;
        }
        #endregion

        #region 랭킹 쿼리함수들
        static public IEnumerator QueryMyEntry_Server(MyRankQueryResult res)  // 내 글로벌 랭킹 가져오기
        {
            res.found = false;

            bool ret = false;
            ServerAPI.TurnRelayGetMyEntry((resp)
                => {
                    if (resp.IsSuccess)
                    {
                        res.found = resp.found;
                        res.rank = resp.rank;
                        res.score = resp.score;
                    }
                    ret = true;
                }
            );

            yield return new WaitUntil(() => { return ret; });
        }

        private static IEnumerator QueryUsers(string factorId, int rankStart, int querySize, List<string> resultUserList)
        {
            Debug.Log($"QueryUsers: start {rankStart}, size {querySize}");

            bool isComplete = false;
            ServerAPI.QueryUsers(factorId, rankStart.ToString(), querySize.ToString(), resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    if (resp.data != null && resp.data.data != null)
                    {
                        foreach (var item in resp.data.data)
                        {
                            resultUserList.Add(item.userKey);
                        }
                    }
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        
        private static IEnumerator QueryRankingSpecificUsers(string factorId, List<string> userKeys, List<TurnRelayRankData> rankResult)
        {
            bool   isComplete        = false;
            string userKeyStringList = string.Join(",", userKeys.ToArray());
            ServerAPI.QueryRankingSpecificUsers(factorId, userKeyStringList, resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    if (resp.data != null && resp.data.data != null)
                    {
                        foreach (var item in resp.data.data)
                        {
                            rankResult.Add(new TurnRelayRankData(item));
                        }
                    }
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }

        public static IEnumerator QueryRankingProfiles(List<TurnRelayRankData> fillTargets)
        {
            List<string> userKeyList = new List<string>();
            for (int i = 0; i < fillTargets.Count; ++i)
            {
                userKeyList.Add(fillTargets[i].userKey);
            }

            bool profileRet1 = false;
            Debug.Log("ManagerTurnRelay.QueryRankingProfiles Start");
            yield return GetProfileList((Profile_PION[] profileList) =>
                {
                    Debug.Log("ManagerTurnRelay.GetProfileList.QueryRankingProfiles Callback" + profileList.Length);
                    profileRet1 = true;

                    if (profileList != null)
                    {
                        for (int i = 0; i < profileList.Length; ++i)
                        {
                            var score = fillTargets.Find((x) => x.userKey == profileList[i].userKey);
                            if (score != null)
                            {
                                score.alterPicture   = profileList[i].profile.alterPicture;
                                score.flower         = profileList[i].profile.flower;
                                score.rankEventPoint = profileList[i].profile.rankEventPoint;
                                score.photoUseAgreed = profileList[i].profile.isLineTumbnailUsed();
                                score.ingameName     = profileList[i].profile.name;
                                score.toy            = profileList[i].profile.toy;

                                profileList[i].profile.Log();
                            }
                        }
                    }
                },
                userKeyList.ToArray());

            while (profileRet1 == false)
            {
                yield return null;
            }

            yield break;
        }
        private static IEnumerator GetProfileList(System.Action<Profile_PION[]> callbackHandler, string[] userKeys)
        {
            bool   isComplete = false;
            string key        = NetworkUtil.CSV_FromArray(userKeys);
            string token      = ServiceSDK.ServiceSDKManager.instance.GetAccessToken();
            ServerAPI.GetProfileList(token, key, resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    DelegateHelper.SafeCall(callbackHandler, resp.data);
                }
                else if (resp.IsFailTridentAPI)
                {
                    // 라인 API 통신 중 에러 발생 시 경고 팝업만 출력.
                    TridentAPIErrorController.OpenErrorPopup(resp.lineStatusCode, resp.error);
                    DelegateHelper.SafeCall(callbackHandler, null);
                }
                isComplete = true;
            });

            yield return new WaitUntil(() => isComplete);
        }
        #endregion
    }

    public class TurnRelayRankData : ProfileTextureManager.IUserProfileData
    {
        public string ingameName = "";
        public string userKey;
        public long scoreValue;
        public int rankEventPoint;

        public int flower = 0;
        public int toy = 0;

        public long rank = 0;
        public string alterPicture;
        public bool photoUseAgreed;

        public long rankDiff = 0;
        public long scoreDiff = 0;

        //프로필 데이터 세팅
        public string _userKey      { get { return userKey; } }
        public string _alterPicture { get { return alterPicture; } }
        public string _pictureUrl   => string.Empty;

        public TurnRelayRankData()
        {
        }

        public TurnRelayRankData(Protocol.RankingData tridentScore)
        {
            if (tridentScore == null)
                return;

            this.scoreValue = (long)tridentScore.score;
            this.userKey = tridentScore.userKey;
            this.rank = tridentScore.rank;
        }

        public override string ToString()
        {
            return string.Format(
                "UserKey: {0}, scoreValue:{1} rank:{2} photoUseAgreed:{3}",
                userKey, scoreValue, rank, photoUseAgreed);
        }
    }

    public class MyRankQueryResult
    {
        public bool found = false;
        public long rank;
        public long score;
    }
}
