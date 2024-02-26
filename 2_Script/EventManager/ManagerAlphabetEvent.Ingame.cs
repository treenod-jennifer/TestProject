using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAlphabetEvent : MonoBehaviour
{
    public class AlphabetIngame
    {
        public class AlphabetData
        {
            public int index = 0;               //현재 알파벳 인덱스
            public int getCount_All = 0;        //지금까지 모은 수
            public int getCount_Current = 0;    //현재 스테이지에서 모은 수
        }

        #region 일반 알파벳
        //알파벳 등장 확률
        public int AppearProb_N { get; set; }

        //알파벳 화면 제한 수
        public int LimitScreenCount_N { get; set; }

        //중복 블럭 획득 시, 변환될 코인
        public int CoinCount_N { get; set; }

        //현재 진행중인 그룹의 알파벳 데이터
        public List<AlphabetData> listAlphabetData_N = null;

        //인게임 내에서 등장할 알파벳 목록
        private List<int> listAppearAlphabet_N = null;

        //현재 화면에 등장한 알파벳 수
        public int screenCount_N = 0;

        #endregion

        #region 스페셜 알파벳
        //중복 블럭 획득 시, 변환될 코인
        public int CoinCount_S { get; set; }

        //인게임 내에서 등장할 알파벳
        public int Alphabet_S { get; set; }

        //스페셜 알파벳이 등장할 턴.
        public int AppearMoveCount_S { get; set; }

        //스페셜 알파벳 정보.
        public AlphabetData alphabetData_S = null;
        #endregion

        //알파벳 이벤트 적용된 상태인지.
        public bool IsStage_ApplyAlphabetEvent { get; set; }

        /// <summary>
        /// 알파벳 모드 인게임 데이터 초기화.
        /// </summary>
        public void InitIngameData_Default()
        {
            if (IsStage_ApplyAlphabetEvent == false)
                return;
            IsStage_ApplyAlphabetEvent = false;
            InitNormalData_Default();
            InitSpecialData_Default();
        }

        /// <summary>
        /// 인게임 시작 시, 인게임 관련된 내용 초기화 시켜주는 함수.
        /// </summary>
        public void InitIngameData()
        {
            IsStage_ApplyAlphabetEvent = true;
            SyncFromServerAlphabetData_Normal();
            SyncFromServerAlphabetData_Special();
        }

        #region 일반 알파벳 데이터 초기화
        private void SyncFromServerAlphabetData_Normal()
        {
            InitNormalData_Default();
            InitNormalData_ServerData();
        }

        /// <summary>
        /// 일반 블럭 데이터 초기화
        /// </summary>
        private void InitNormalData_Default()
        {
            AppearProb_N = 0;
            LimitScreenCount_N = 0;
            CoinCount_N = 0;
            listAppearAlphabet_N = null;
            listAlphabetData_N = null;
            screenCount_N = 0;
        }

        /// <summary>
        /// 일반 블럭 데이터 서버 설정으로 데이터 초기화
        /// </summary>
        private void InitNormalData_ServerData()
        {
            if (ManagerAlphabetEvent.instance.isUser_normalComplete == true)
                return;

            //게임 데이터
            AppearProb_N = (int)ServerContents.AlphabetEvent.appearRatio[instance.currentGroup - 1];
            LimitScreenCount_N = ServerContents.AlphabetEvent.appearMaxCount[instance.currentGroup - 1];
            CoinCount_N = ServerContents.AlphabetEvent.coinExchange[instance.currentGroup - 1];

            //유저 데이터
            if (ServerRepos.UserAlphabetEvents != null)
            {
                listAppearAlphabet_N = ServerRepos.UserAlphabetEvents.alphabetList;
            }

            InitArrAlphabetData_N();
        }

        private void InitArrAlphabetData_N()
        {
            string key = instance.currentGroup.ToString();
            int[][] arrDatas = ServerContents.AlphabetEvent.alphabetRatio[key];
            int alphabetCount = arrDatas.Length;

            listAlphabetData_N = new List<AlphabetData>();
            for (int i = 0; i < alphabetCount; i++)
            {
                AlphabetData addData = new AlphabetData();

                //해당 알파벳의 인덱스 설정
                addData.index = arrDatas[i][0];

                //유저가 현재까지 모은 알파벳 카운트 받아옴.
                if (ServerRepos.UserAlphabetEvents != null)
                {
                    int listIdx = instance.currentGroup - 1;
                    if (i < ServerRepos.UserAlphabetEvents.alphabetStatus[listIdx].Count)
                        addData.getCount_All = ServerRepos.UserAlphabetEvents.alphabetStatus[listIdx][i];   
                }

                //스테이지에서 모은 알파벳 카운트는 0으로 초기화.
                addData.getCount_Current = 0;

                listAlphabetData_N.Add(addData);
            }
        }
        #endregion

        #region 스페셜 알파벳 데이터 초기화
        private void SyncFromServerAlphabetData_Special()
        {
            InitSpecialData_Default();
            if (instance.IsExistSpecialBlock() == true)
                InitSpecialData_ServerData();
        }

        /// <summary>
        /// 스페셜 블럭 데이터 초기화
        /// </summary>
        private void InitSpecialData_Default()
        {
            CoinCount_S = 0;
            Alphabet_S = 0;
            AppearMoveCount_S = 0;
            alphabetData_S = null;
        }

        /// <summary>
        /// 스페셜 블럭이 등장하는 스테이지에서의 설정 - 서버 설정으로 데이터 초기화
        /// </summary>
        private void InitSpecialData_ServerData()
        {
            if (instance.isUser_specialComplete == true)
                return;

            //게임 데이터
            int specialGroupIdx = ServerContents.AlphabetEvent.coinExchange.Length - 1;
            CoinCount_S = ServerContents.AlphabetEvent.coinExchange[specialGroupIdx];

            //등장할 알파벳 설정
            if (ServerRepos.UserAlphabetEvents != null)
            {
                Alphabet_S = ServerRepos.UserAlphabetEvents.specialAlphabet;
            }

            //등장할 턴 설정
            InitAppearMoveCount_S();

            //등장하는 알파벳 정보 설정
            InitAlphabetData_S();
        }

        private void InitAppearMoveCount_S()
        {   
            if (Alphabet_S == 0)
            {   //현재 맵에서 등장하는 알파벳이 없다면, 턴은 0으로 설정
                AppearMoveCount_S = 0;
            }
            else
            {
                if (ManagerBlock.instance.stageInfo.gameMode == (int)GameMode.LAVA)
                {
                    AppearMoveCount_S = 1;
                }
                else
                {
                    //등장할 수 있는 최대 카운트 : (기본 턴수 * 20 / 100) 턴 수 후 / 게임 종료 전 5턴
                    int maxCount = ManagerBlock.instance.stageInfo.turnCount - (int)(ManagerBlock.instance.stageInfo.turnCount * 0.2f);
                    int minCount = 5;

                    //턴 증가 레디 아이템을 사용한 상태라면, 등장 턴수에 해당 턴 수 만큼 추가.
                    if (Global.GameInstance.CanUseReadyItem(0) == true && UIPopupReady.readyItemUseCount[0].Value == 1)
                    {
                        maxCount += 3;
                        minCount += 3;
                    }
                    else if (Global.GameInstance.CanUseReadyItem(6) == true && UIPopupReady.readyItemUseCount[6].Value == 1)
                    {
                        maxCount += 6;
                        minCount += 6;
                    }

                    //맵 자체의 턴이 적어서 등장할 수 있는 턴 계산이 안 될 경우 minCount에 등장하도록 설정.
                    if (maxCount <= minCount)
                        AppearMoveCount_S = minCount;
                    else
                        AppearMoveCount_S = Random.Range(minCount, (maxCount + 1));
                }
            }
        }

        private void InitAlphabetData_S()
        {
            alphabetData_S = new AlphabetData();

            //해당 알파벳의 인덱스 설정
            alphabetData_S.index = Alphabet_S;
            
            //유저가 현재까지 모은 알파벳 카운트 받아옴.
            if (ServerRepos.UserAlphabetEvents != null)
            {
                int findIndex = instance.listAlphabetIndex_Special.FindIndex(x => x == Alphabet_S);
                if(findIndex != -1)
                {
                    alphabetData_S.getCount_All = ServerRepos.UserAlphabetEvents.specialStatus[findIndex];
                }
            }

            //스테이지에서 모은 알파벳 카운트는 0으로 초기화.
            alphabetData_S.getCount_Current = 0;
        }
        #endregion

        #region 알파벳 이름 가져오기
        /// <summary>
        /// 현재 그룹의 알파벳 리스트에서 인덱스 값의 알파벳 블럭 이름 가져오기 - 일반 알파벳
        /// </summary>
        public string GetAlphabetSpriteName_AtListData_N(int listIndex)
        {
            if (listAlphabetData_N == null || listAlphabetData_N.Count <= listIndex)
                return "";

            bool isAlreadyGet = (listAlphabetData_N[listIndex].getCount_All > 0);
            int alphabetIdx = listAlphabetData_N[listIndex].index;
            return instance.GetAlphabetSpriteName_N(alphabetIdx, isAlreadyGet);
        }
        #endregion

        #region 알파벳 생성 관련
        /// <summary>
        /// 알파벳 생성 가능한 상태인지 검사 - 일반 알파벳 
        /// </summary>
        public bool IsCanAppearAlphabetBlock_N()
        {
            //다음에 등장할 수 있는 알파벳이 없으면 생성 불가.
            if (listAppearAlphabet_N == null || listAppearAlphabet_N.Count == 0)
                return false;

            //화면수 제한이 걸렸으면 알파벳 생성 불가.
            if (screenCount_N >= LimitScreenCount_N)
                return false;

            //등장할 수 있는 확률로 생성 여부 결정.
            return (AppearProb_N > Random.Range(0, 1000));
        }

        /// <summary>
        /// 다음에 생성 가능한 알파벳 반환 - 일반 알파벳
        /// </summary>
        public int GetNextAlphabetIdx()
        {
            if (listAppearAlphabet_N == null || listAppearAlphabet_N.Count == 0)
                return 0;

            //리스트에 가장 앞에 위치한 알파벳을 반환, 반환된 알파벳은 리스트에서 제거해줌.
            int index = listAppearAlphabet_N[0];
            listAppearAlphabet_N.RemoveAt(0);
            return index;
        }

        /// <summary>
        /// 알파벳 생성 가능한 상태인지 검사 - 스페셜 알파벳 
        /// </summary>
        public bool IsCanAppearAlphabetBlock_S(int moveCount)
        {
            //다음에 등장할 수 있는 알파벳이 없으면 생성 불가.
            if (Alphabet_S == 0)
                return false;

            //등장할 수 있는 턴이 0이면 생성 불가.
            if (AppearMoveCount_S == 0)
                return false;

            //모드에 따라 출력 조건이 다름.
            if (GameManager.gameMode == GameMode.LAVA)
            {
                return 5 > Random.Range(0, 100);
            }
            else
            {
                return (AppearMoveCount_S >= moveCount);
            }
        }
        #endregion

        #region 플레이 시 등장하는 알파벳 검사 관련
        /// <summary>
        /// 현재 스테이지에서 등장하는 알파벳 이름 가져오기 - 스페셜 알파벳
        /// </summary>
        public string GetAppearAlphabetSpriteName_S(bool isOn = true)
        {
            if (alphabetData_S == null)
                return "";

            return instance.GetAlphabetSpriteName_S(alphabetData_S.index);
        }
        #endregion

        #region 플레이 중 획득한 알파벳 검사 관련
        /// <summary>
        /// 최근 플레이한 스테이지에서 획득한 알파벳 카운트를 가져오기 - 일반 알파벳
        /// </summary>
        public List<int> GetListCurrentGainCount_All_N()
        {
            List<int> listCount = new List<int>();
            if (listAlphabetData_N != null)
            {
                for (int i = 0; i < listAlphabetData_N.Count; i++)
                {
                    listCount.Add(listAlphabetData_N[i].getCount_Current);
                }
            }
            return listCount;
        }

        /// <summary>
        /// 최근 플레이한 스테이지에서 신규로 획득한 알파벳 종류가 몇 개인지 가져오기 - 일반 알파벳
        /// </summary>
        public int GetAlphabetKindsCount_NewGain_N()
        {
            int newCount = 0;
            if (listAlphabetData_N != null)
            {
                for (int i = 0; i < listAlphabetData_N.Count; i++)
                {
                    AlphabetData data = listAlphabetData_N[i];
                    if (data.getCount_Current > 0 && data.getCount_All == data.getCount_Current)
                        newCount++;
                }
            }
            return newCount;
        }

        /// <summary>
        /// 최근 플레이한 스테이지에서 알파벳을 획득했는지 검사 - 스페셜 알파벳
        /// </summary>
        public bool IsGainAlphabet_S()
        {
            if (alphabetData_S == null || alphabetData_S.getCount_Current == 0)
                return false;
            return true;
        }

        /// <summary>
        /// 최근 플레이한 스테이지에서 신규로 알파벳을 획득했는지 검사 - 스페셜 알파벳
        /// </summary>
        public bool IsGainNewAlphabet_S()
        {
            if (alphabetData_S == null || alphabetData_S.getCount_Current == 0 
                || alphabetData_S.getCount_All > alphabetData_S.getCount_Current)
                return false;
            return true;
        }
        #endregion

        #region 알파벳 블럭 획득 처리
        /// <summary>
        /// 알파벳 획득 처리
        /// </summary>
        public void GainCollectItem_Alphabet(int index, bool isNormalAlphabet, Vector3 startPos)
        {
            if (isNormalAlphabet == true)
                GainCollectItem_AlphabetNormal(index, startPos);
            else
                GainCollectItem_AlphabetSpecial(startPos);

        }

        //일반 알파벳 획득 처리
        private void GainCollectItem_AlphabetNormal(int index, Vector3 startPos)
        {
            if (listAlphabetData_N == null)
                return;

            int findIndex = -1;

            //검사하는 알파벳의 인덱스를 가진 상황에서 획득한 카운트가 없는 인덱스부터 받아옴.
            for (int i = 0; i < listAlphabetData_N.Count; i++)
            {
                if (listAlphabetData_N[i].index == index && listAlphabetData_N[i].getCount_All == 0)
                {
                    findIndex = i;
                    break;
                }
            }

            //획득한 카운트가 모두 있다면, 가장 앞에 위치한 알파벳의 인덱스를 가져옴.
            if (findIndex == -1)
                findIndex = listAlphabetData_N.FindIndex(x => x.index == index);

            //해당하는 알파벳이 없으면 검사하지 않음.
            if (findIndex == -1)
                return;

            AlphabetData findData = listAlphabetData_N[findIndex];

            if (findData.getCount_All == 0)
            {   // 획득한 적 없는 아이템은 UI로 날아가는 연출.
                string imageName = instance.GetAlphabetSpriteName_N(findData.index);
                Vector3 uiPos = GameUIManager.instance.listAlphabetUI_N[findIndex].alphabetObj.transform.position;
                InGameEffectMaker.instance.MakeAlphabetEventEffect(startPos, uiPos, imageName, true,
                    () => UpdateData_AlphabetNormal(findData, findIndex, imageName));
            }
            else
            {   // 획득한 적 있는 아이템은 코인으로 변환.
                InGameEffectMaker.instance.MakeFlyCoin(startPos, CoinCount_N);
            }
            UpdateAlphabetData_N(findData);
        }

        private void UpdateData_AlphabetNormal(AlphabetData data, int uiIndex, string spriteName)
        {
            GameUIManager.instance.RefreshAlphabetEvent_Normal(uiIndex, spriteName);

            //현재 진행중인 그룹의 모든 일반 알파벳을 다 모은 경우, UI 연출 재생
            bool isCompleteAll = true;
            for (int i = 0; i < listAlphabetData_N.Count; i++)
            {
                if (listAlphabetData_N[i].getCount_All == 0)
                {
                    isCompleteAll = false;
                    break;
                }
            }

            if (isCompleteAll == true)
                GameUIManager.instance.ActionAlphabetEvent_AllCollect_Normal();
        }

        private void UpdateAlphabetData_N(AlphabetData data)
        {
            data.getCount_All++;
            data.getCount_Current++;
            screenCount_N--;
        }

        //스페셜 알파벳 획득 처리
        private void GainCollectItem_AlphabetSpecial(Vector3 startPos)
        {
            if (alphabetData_S == null)
                return;

            if (alphabetData_S.getCount_All == 0)
            {   // 획득한 적 없는 아이템은 UI로 날아가는 연출.
                string imageName = GetAppearAlphabetSpriteName_S();
                Vector3 uiPos = GameUIManager.instance.textureAlphabet_S.gameObject.transform.position;
                InGameEffectMaker.instance.MakeAlphabetEventEffect(startPos, uiPos, imageName, false,
                    () => UpdateData_AlphabetSpecial(imageName));
            }
            else
            {
                // 획득한 적 있는 아이템은 코인으로 변환.
                InGameEffectMaker.instance.MakeFlyCoin(startPos, CoinCount_S);
            }
            UpdateAlphabetData_S();
        }

        private void UpdateData_AlphabetSpecial(string spriteName)
        {
            GameUIManager.instance.RefreshAlphabetEvent_Special(spriteName);

            //모든 스페셜 알파벳을 다 모은 경우, UI 연출 재생
            if (instance.listCollectCount_Special == null)
                return;

            int specialIdx = instance.listAlphabetIndex_Special.FindIndex(x => x == Alphabet_S);
            if (specialIdx != -1 && instance.listCollectCount_Special != null)
            {
                bool isCompleteAll = true;
                for (int i = 0; i < instance.listCollectCount_Special.Count; i++)
                {
                    if (instance.listCollectCount_Special[i] == 0 && i != specialIdx)
                    {   
                        isCompleteAll = false;
                        break;
                    }
                }

                if (isCompleteAll == true)
                    GameUIManager.instance.ActionAlphabetEvent_AllCollect_Special();
            }
        }

        private void UpdateAlphabetData_S()
        {
            if (alphabetData_S == null)
                return;

            alphabetData_S.getCount_All++;
            alphabetData_S.getCount_Current++;
        }
        #endregion
    }
}
