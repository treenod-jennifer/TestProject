using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerTurnRelay : MonoBehaviour
{
    public enum BONUSITEM_TYPE
    {
        ADD_TURN,
        EVENTPOINT,
        LINE_BOMB,
        CIRCLE_BOMB,
        RAINBOW_BOMB,
    }

    public class TurnRelayIngame
    {
        #region 인게임 컨텐츠 데이터
        public float LuckRatio { get; set; } = 0f;
        public List<int> listTurnBonus = new List<int>();
        public List<int> listEventPoint_WaveClear = new List<int>();
        public List<int> listEventPoint_BonusItem = new List<int>();
        #endregion

        #region 인게임 유저 데이터(인게임 클리어/실패 후 사라지는 데이터)
        //유저가 소지하고 있는 인게임 아이템
        public Dictionary<ManagerTurnRelay.BONUSITEM_TYPE, int> dicBonusItem_Gain
                = new Dictionary<ManagerTurnRelay.BONUSITEM_TYPE, int>();

        //보너스 스테이지 리스트
        public List<int> listBonusStage = new List<int>();

        //누적 유저가 선택한 보너스 아이템 카운트
        public List<int> listTotalBonusItem_Select = new List<int>();

        //누적 폭탄 생성 카운트
        public List<int> listTotalMakeBombCount = new List<int>();

        //누적 인게임 아이템 사용 카운트
        public List<int> listTotalUseIngameItemCount = new List<int>();

        //게임 시작 시, 사용한 레디 아이템 카운트
        public List<int> listUseReadyItem = new List<int>();

        public int RemainTurn { get; set; } = 0;    //이전 스테이지에서 이월된 턴
        public int IngameEventPoint { get; set; } = 0;    //1웨이브부터 현재 웨이브까지 획득한 이벤트 포인트
        public int CurrentWave { get; set; } = 1;  //진행중인 웨이브
        public long TotalPlayTime { get; set; } = 0; //누적 플레이 타임
        public int TotalContinueCount { get; set; } = 0;   //누적 컨티뉴 카운트
        public int BonusEventPoint { get; set; } = 0;   //보너스 아이템 선택으로 증가된 이벤트 포인트
        public BONUSITEM_TYPE SelectBonusItemType { get; set; } = BONUSITEM_TYPE.ADD_TURN;   //선택한 보너스 아이템
        public bool IsPlayEnd { get; set; } = false;    //플레이 종료될 때, 유예기간 지났는지
        public bool IsSaveWave { get; set; } = false;    //실패 팝업에서 재시작 버튼 클릭으로 시작한 웨이브인지 (웨이브 재시작 시 바로 초기화)
        public int SaveWaveCount { get; set; } = 0;    //해당 게임에서 광고지면으로 재시작 진행한 횟수
        #endregion

        #region 초기화 관련
        public void SyncFromServerContentsData_Ingame()
        {
            CdnTurnRelayEvent cdnTurnRelayEvent = ServerContents.TurnRelayEvent;
            LuckRatio = cdnTurnRelayEvent.luckyRatio;
            listTurnBonus = new List<int>(cdnTurnRelayEvent.waveTurnBonus);
            listEventPoint_WaveClear = new List<int>(cdnTurnRelayEvent.score);
            listEventPoint_BonusItem = new List<int>(cdnTurnRelayEvent.bonusScore);
            SaveWaveCount = 0;
        }

        public void SyncFromServerUserData_Ingame()
        {
            if (ServerRepos.UserTurnRelayEvent == null)
                return;

            ServerUserTurnRelayEvent userData = ServerRepos.UserTurnRelayEvent;

            //소지 아이템 초기화
            dicBonusItem_Gain.Clear();
            foreach (var item in userData.itemCurrent)
            {
                if (item.Value > 0)
                    dicBonusItem_Gain.Add((ManagerTurnRelay.BONUSITEM_TYPE)item.Key, item.Value);
            }

            listTotalBonusItem_Select = new List<int>(userData.bonusItemSelected);
            listBonusStage = new List<int>(userData.bonusStage);
            listTotalMakeBombCount = new List<int>(userData.usedItems);
            listTotalUseIngameItemCount = new List<int>(userData.usedIngameItems);
            listUseReadyItem = new List<int>(userData.itemStartSelected);

            RemainTurn = userData.carryOverTurn;
            IngameEventPoint = userData.scoreStacked;
            CurrentWave = userData.stage;
            CurrentWave = userData.stage;
            TotalPlayTime = userData.playSec;
        }

        /// <summary>
        /// 턴 릴레이 인게임 데이터 초기화
        /// (처음부터 게임 시작했을 때, 초기화 시켜줌)
        /// </summary>
        public void InitIngameData_GameStart(Dictionary<READY_ITEM_TYPE, READY_ITEM_STATE> dicSelectData_IngameItem)
        {
            InitIngameUserData();

            //스테이지 준비창에서 선택한 아이템 추가
            foreach (var data in dicSelectData_IngameItem)
            {
                //사과 아이템은 진입하자마자 사용됨
                if (data.Key == READY_ITEM_TYPE.ADD_TURN)
                {
                    RemainTurn += 3;
                }
                else if (data.Key == READY_ITEM_TYPE.DOUBLE_ADD_TURN)
                {
                    RemainTurn += 6;
                }
            }
        }

        public void InitIngameUserData(bool isSaveWave = false)
        {
            SyncFromServerUserData_Ingame();
            BonusEventPoint = 0;
            IsPlayEnd = false;
            IsSaveWave = isSaveWave;
        }
        #endregion

        #region 인게임 턴 관련
        /// <summary>
        /// 해당 웨이브의 보너스 턴 가져오는 함수
        /// </summary>
        public int GetBonuseTurn_AtWave(int wave = 0)
        {
            int waveIdx = (wave == 0) ? (CurrentWave - 1) : (wave - 1);

            //확인하는 웨이브의 보너스 턴 데이터가 없으면 반환.
            if (listTurnBonus.Count <= waveIdx)
                return 0;

            return listTurnBonus[waveIdx];
        }

        #endregion

        #region 이벤트 전용 게임 아이템 관련
        /// <summary>
        /// 유저가 소지하고 있는 인게임 아이템 카운트 변경
        /// </summary>
        public void SetData_DicIngameItemGainCount(ManagerTurnRelay.BONUSITEM_TYPE itemType, int itemCount = 1)
        {
            if (dicBonusItem_Gain.ContainsKey(itemType) == false)
                dicBonusItem_Gain.Add(itemType, itemCount);
            else
                dicBonusItem_Gain[itemType] += itemCount;
        }

        /// <summary>
        /// 유저가 소지하고 있는 인게임 아이템 카운트 가져오기
        /// </summary>
        public int GetData_DicIngameItemCount(ManagerTurnRelay.BONUSITEM_TYPE itemType)
        {
            return (dicBonusItem_Gain.ContainsKey(itemType) == false) ? 0 : dicBonusItem_Gain[itemType];
        }

        /// <summary>
        /// 각 레디 아이템 타입에 해당하는 폭탄 타입 가져오는 함수
        /// </summary>
        public BlockBombType GetItemBombType(ManagerTurnRelay.BONUSITEM_TYPE itemType)
        {
            switch (itemType)
            {
                case ManagerTurnRelay.BONUSITEM_TYPE.LINE_BOMB:
                    return BlockBombType.LINE;
                case ManagerTurnRelay.BONUSITEM_TYPE.CIRCLE_BOMB:
                    return BlockBombType.BOMB;
                case ManagerTurnRelay.BONUSITEM_TYPE.RAINBOW_BOMB:
                    return BlockBombType.RAINBOW;
                default:
                    return BlockBombType.NONE;
            }
        }
        #endregion

        #region 레디 아이템 관련
        public int GetUseReadyItemCount(int index)
        {
            if (listUseReadyItem.Count <= index)
                return 0;
            return listUseReadyItem[index];
        }
        #endregion

        #region 인게임 아이템 및 폭탄 생성 관련
        //누적 생성한 폭탄 카운트에 현재 생성량 더하기
        public void AddTotalMakeBombCount()
        {
            if (ManagerBlock.instance == null)
                return;

            for (int i = 0; i < ManagerBlock.instance.creatBombCount.Length; i++)
            {
                if (ManagerTurnRelay.turnRelayIngame.listTotalMakeBombCount.Count <= i)
                    continue;

                int itemCount = ManagerBlock.instance.creatBombCount[i].Value;
                ManagerTurnRelay.turnRelayIngame.listTotalMakeBombCount[i] += itemCount;
            }
        }

        //누적 생성한 폭탄 카운트 가져오기
        public int GetTotalMakeBombCount(int bombIdx)
        {
            if (ManagerTurnRelay.turnRelayIngame.listTotalMakeBombCount.Count <= bombIdx)
                return 0;

            return ManagerTurnRelay.turnRelayIngame.listTotalMakeBombCount[bombIdx];
        }

        //누적 사용한 인게임 카운트에 현재 사용량 더하기
        public void AddTotalUseIngameItemCount()
        {
            for (int i = 0; i < GameItemManager.useCount.Length; i++)
            {
                if (ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount.Count <= i)
                    continue;

                int itemCount = GameItemManager.useCount[i];
                ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount[i] += itemCount;
            }
        }

        //누적 사용한 인게임 아이템 카운트 가져오기
        public int GetTotalUseIngameItemCount(int itemIdx)
        {
            if (ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount.Count <= itemIdx)
                return 0;

            return ManagerTurnRelay.turnRelayIngame.listTotalUseIngameItemCount[itemIdx];
        }
        #endregion

        #region 보너스 스테이지 관련
        /// <summary>
        /// 해당 웨이브가 럭키 보너스가 적용된 웨이브인지 검사하는 함수
        /// </summary>
        public bool IsLuckyWave(int wave = 0)
        {
            int waveIdx = (wave == 0) ? (CurrentWave - 1) : (wave - 1);

            if (listBonusStage.Count > waveIdx && listBonusStage[waveIdx] == 1)
                return true;
            else
                return false;
        }
        #endregion

        #region 이벤트 포인트 관련
        /// <summary>
        /// 해당 웨이브에서 획득할 수 있는 포인트
        /// (합계 포인트, 추가로 획득한 포인트)
        /// </summary>
        public (int, int) GetEventPoint_AtWave(int wave = 0)
        {
            if (wave == 0)
                wave = CurrentWave;

            int waveIdx = wave - 1;

            //확인하는 웨이브의 점수 데이터가 없으면 반환.
            if (listEventPoint_WaveClear.Count <= waveIdx)
                return (0, 0);

            bool isLuckyStage = (IsLuckyWave(wave) == true);

            //해당 스테이지를 클리어 했을 때 획득할 수 있는 포인트
            int defaultPoint = listEventPoint_WaveClear[waveIdx];

            //배수 포인트 적용된 스테이지라면 추가된 포인트량 저장
            int bonusPoint = (isLuckyStage == false) ?
                0 : (int)(defaultPoint * LuckRatio) - defaultPoint;

            //현재 진행중인 스테이지라면, 게임시작 시 선택한 보너스 점수도 합산
            if (wave == CurrentWave)
                bonusPoint += BonusEventPoint;

            //해당 스테이지에서 총 얻을 수 있는 포인트의 합계
            int totalPoint = defaultPoint + bonusPoint;

            return (totalPoint, bonusPoint);
        }

        public int GetBonusEventPoint_AtWave(int wave = 0)
        {
            int waveIdx = (wave == 0) ? (CurrentWave - 1) : (wave - 1);

            //확인하는 웨이브의 보너스 턴 데이터가 없으면 반환.
            if (listEventPoint_BonusItem.Count <= waveIdx)
                return 0;

           return listEventPoint_BonusItem[waveIdx];
        }

        public void SetBonusEventPoint()
        {
            BonusEventPoint = GetBonusEventPoint_AtWave();
        }
        #endregion
        
        public bool IsTurnRelayRestart()
        {
            if (Global.GameType != GameType.TURN_RELAY)
            {
                return false;
            }

            if (turnRelayIngame == null || turnRelayIngame.CurrentWave < 2)
            {
                return false;
            }

            if (GetEventState() != EventState.RUNNING)
            {
                return false;
            }

            if (!AdManager.ADCheck(AdManager.AdType.AD_22))
            {
                return false;
            }

            return true;
        }
    }
}
