using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ServiceSDK
{
    /// <summary>
    /// 이곳에 있는 클래스들은 그로씨 데이터용 클래스입니다.
    /// JSON으로 Serialize되는 클래스이니 JSON으로 변환되지 않아야 할 변수들은 public으로 하지 않는것이 좋습니다.
    /// </summary>

#region User Inflow Index
    public enum GROWTHY_INFLOW_VALUE
    {
        SPLASH,
        LANG_SEL_S,
        LANG_SEL_E,
        SDK_INIT_S,
        SDK_INIT_E,
        SDK_LOGIN_S,
        SDK_LOGIN_E,
        SDK_NOTICE_S,
        SDK_NOTICE_E,
        CDN_DOWNLOAD_S,
        CDN_DOWNLOAD_E,
        SDK_PROFILE_S,
        SDK_PROFILE_E,
        GAME_LOGIN_S,
        GAME_LOGIN_E,
        SDK_GET_FRIENDS_S,
        SDK_GET_FRIENDS_E,
        INTRO_S,
        INTRO_E,
        NICKNAME_S,
        NICKNAME_E,
        GET_RANKING_S,
        GET_RANKING_E,
        TUTORIAL_S,  //튜토리얼 전
        TUTORIAL_E,  //튜토리얼 후
    }
#endregion

#region USERINFO
    
    /// <summary>
    /// USERINFO 그로씨 데이터
    /// </summary>
    public class GrowthyUserInfo
    {
        /// <summary>
        /// 캐릭터 데이터 관리용 컨테이너
        /// </summary>
        private Dictionary<string, GrowthyCharacterInfo> _characters = new Dictionary<string, GrowthyCharacterInfo>();

        /// <summary>
        /// 아이템 데이터 관리용 컨테이너
        /// </summary>
        private List<GrowthyItemInfo> _items = new List<GrowthyItemInfo>();

        public string L_SVR      = string.Empty; //서버이름(포코퍼즐 사용안함)
        public string L_MID      = string.Empty; //유저 ID
        public int    L_ULV      = 0;            //유저 계정 레벨(포코퍼즐 사용안함)
        public long   L_PCASH    = 0;            //유상 1차코인(다이아)
        public long   L_FCASH    = 0;            //무상 1차코인(다이아)
        public long   L_PGOLD    = 0;            //유상 2차코인(골드)
        public long   L_FGOLD    = 0;            //무상 2차코인(골드)
        public int    L_FPNT     = 0;            //우정포인트(포코퍼즐 사용안함)
        public long   L_APNT     = 0;            //활동포인트(클로버)
        public int    L_FRN      = 0;            //게임 내 전체친구 보유수(포코퍼즐 사용안함)
        public int    L_LFRN     = 0;            //게임 내 LINE친구 보유수
        public string L_TUTORIAL = string.Empty; //현재 진행중인 튜토리얼 단계(포코퍼즐 사용안함)
        public string L_GID      = string.Empty; //길드 ID(포코퍼즐 사용안함)
        public int    L_GLV      = 0;            //길드 레벨(포코퍼즐 사용안함)
        public string L_GSTAT    = "NONE";       //길드 상태(포코퍼즐 사용안함)
        public string L_CDT = string.Empty;      //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS)
        
        public long   L_NUM1     = 0;            //포코퍼즐 전용 컬럼. 스타 갯수
        public long   L_NUM2     = 0;            //포코퍼즐 전용 컬럼. 스타 갯수
        public long   L_NUM3     = 0;            //포코퍼즐 전용 컬럼. 스타 갯수
        public long   L_NUM4     = 0;            //포코퍼즐 전용 컬럼. 스타 갯수
        public long   L_NUM5     = 0;            //포코퍼즐 전용 컬럼. 스타 갯수        
        public long   L_NUM6     = 0;            //Ranking Point 총 보유량
        public long   L_NUM7     = 0;            //포코꽃 총 보유량
        public long   L_NUM8     = 0;            //탐험날개 총 보유량
        public long   L_NUM9     = 0;            //성장구슬
        public long   L_NUM10    = 0;            //월드 랭킹 토큰
        
        public string L_STR1 = string.Empty;     //마지막미션 단계?
        public string L_STR2 = string.Empty;     //탐험모드 stage
        public string L_STR3 = string.Empty;     //유저 세그먼트
        public string L_STR4 = string.Empty;     //유저 언어서비스 선택사항
        public string L_STR5 = string.Empty;     //튜토리얼 on/off
        
        public List<Dictionary<string, object>> L_STR6 = new List<Dictionary<string, object>>();     // 통상 이벤트 관련 정보 ( 1. 엔드 컨텐츠 : 이벤트 노출 여부 (NONE, VIEW, ABLE) )
        public List<Dictionary<string, object>> L_STR7 = new List<Dictionary<string, object>>();     // 이벤트 재화 총 보유량 ( 1. 엔드 컨텐츠 재화 (진주조개) )

        public List<GrowthyCharacterInfo> L_CHAR; //캐릭터 정보(이 내용은 오직 json으로 Serialize하기 위한 용도. 데이터 비교용이 아님. 비교하려면 characters를 사용. 포코퍼즐에서는 사용하지 않음)
        public List<GrowthyItemInfo>      L_ITEM; //아이템 정보(이 내용은 오직 json으로 Serialize하기 위한 용도. 데이터 비교용이 아님. 비교하려면 items를 사용)

        public Dictionary<string, GrowthyCharacterInfo> GetCharacterDic()
        {
            return _characters;
        }
        
        public List<GrowthyItemInfo> GetItemDic()
        {
            return _items;
        }
        
        public Dictionary<string, object> GetEventDictionary(string eventName, object value)
        {
            return new Dictionary<string, object> {{"L_NAME", eventName}, {"L_VALUE", value}};;
        }
    }

    /// <summary>
    /// USERINFO에 L_CHAR에 들어갈 데이터 (사용 안함)
    /// </summary>
    public class GrowthyCharacterInfo
    {
        public enum Code_L_STAT
        {
            OWN,            //보유
            EQUIP,          //장착
            DELETE,         //삭제
        }

        public string L_SVR  = string.Empty;               //서버이름
        public string L_MID  = string.Empty;               //유저 ID
        public string L_CUID = string.Empty;               //캐릭터 UID(UID가 없으면 ID넣어도 됨)
        public string L_CID  = string.Empty;               //캐릭터 ID
        public string L_LEV  = string.Empty;               //레벨
        public long   L_EXP  = 0;                          //경험치
        public string L_GRD  = string.Empty;               //등급
        public string L_CAT  = string.Empty;               //캐릭터 직업
        public string L_MCAT = string.Empty;               //캐릭터 속성
        public string L_STAT = Code_L_STAT.OWN.ToString(); //캐릭터 상태
        public int    L_HP   = 0;                          //HP
        public int    L_OFP  = 0;                          //공격력
        public int    L_DFP  = 0;                          //방어력
        public int    L_LV   = 0;                          //강화 레벨
        public string L_CDT  = string.Empty;               //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS)
    }

    /// <summary>
    /// USERINFO에 L_ITEM에 들어갈 데이터
    /// </summary>
    public class GrowthyItemInfo
    {
        public enum Code_L_STAT
        {
            OWN,            //보유
            EQUIP,          //장착
            DELETE,         //삭제
        }

        public enum Code_L_CAT
        {
            DECO,           //하우징
            MATERIAL,       //로비 재료
            CONSUMPTION_INGAME,    //소모성 아이템
            CONSUMPTION_LOBBY
        }

        public string L_SVR  = string.Empty;               //서버이름(사용안함)
        public string L_MID  = string.Empty;               //유저 ID
        public string L_IUID = string.Empty;               //아이템 UID(UID가 없으면 ID넣어도 됨)
        public string L_IID  = string.Empty;               //아이템 ID(분류-대분류-소분류로 정의. 예를들면 하우징인덱스가 1이고 모델인덱스가 2인 하우징이라면 HOUSING-1-2. 인덱스가 1인 로비 재료라면 MATERIAL-1. 소분류가 없으면 제외)
        public string L_LEV  = string.Empty;               //레벨(갯수로 사용)
        public int    L_EXP  = 0;                          //경험치(포코퍼즐 사용안함)
        public string L_GRD  = string.Empty;               //등급(포코퍼즐 사용안함)
        public string L_CAT  = string.Empty;               //분류(하우징, 로비재료)
        public string L_STAT = Code_L_STAT.OWN.ToString(); //캐릭터 상태
        public string L_CID  = string.Empty;               //아이템 장착캐릭터 정보 L_STAT이 EQUIP인것만 적용(포코퍼즐 사용안함)
        public int    L_CNT  = 0;                          //아이템 보유수
        public string L_CDT  = string.Empty;               //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS)



        public GrowthyItemInfo(string itemUID, string itemID, Code_L_CAT cat, Code_L_STAT stat, int count)
        {
            this.L_MID = ServiceSDK.ServiceSDKManager.instance.GetUserKey();
            this.L_IUID = itemUID;
            this.L_IID = itemID;
            this.L_CAT = cat.ToString();
            this.L_STAT = stat.ToString();
            this.L_CNT = count;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
        }
    }

#endregion

    
#region CUSTOM LOG
    public class GrowthyCustomLog_Money
    {
        //재화 로그시 재화 유형
        public enum Code_L_TAG
        {
            FC,          //1차 재화 //다이아 
            SC,          //2차 재화 //코인
            AP_CLOVAR,   //클로버
            AP_STAR,     //스타(미션용)
            RP,          //랭킹포인트
            WA,          //탐험의 날개
            GB,          //성장구슬
            GT,          //가챠권
            RT,          // 랭킹토큰
            EC,          // 엔드컨텐츠 재화
            AT_CAPSULEMEDAL, // 캡슐가챠용 메달
            AS,          // 앤틱 스토어 재화
        }

        //재화 로그시 변경 상세 사유 코드
        public enum Code_L_MRSN
        {
            G_BUY_AT_SHOP,                      //재화를 상점에서 구매                       //프로덕션코드
            G_BUY_BY_1ST,                       //재화를 1차 코인을 사용하여 구매            //프로덕션코드
            G_BUY_BY_2ND,                       //재화를 2차 코인을 사용하여 구매            //프로덕션코드
            G_BUY_BY_AP,                        //재화를 활동포인트로 구매                   //프로덕션코드
            G_FREE_CHARGE,                      //무료 충전으로 재화 획득
            G_GAME_PLAY,                        //게임 플레이로 재화 획득
            G_FIRST_USE_REWARD,                 //게임 첫 이용 보상으로 재화 획득
            G_ATTENDANCE_REWARD,                //출석 보상으로 재화 획득
            G_REVIER_REWARD,                    //리뷰 보상으로 재화 획득
            G_SOCIAL_REWARD,                    //소셜 활동 보상으로 재화 획득               //소셜 액션 네임
            G_LEVEL_UP_REWARD,                  //레벨업 보상으로 재화 획득                  //에프터 레벌
            G_RANKING_REWARD,                   //랭킹 보상으로 재화 획득                    //랭킹
            G_EVENT_REWARD,                                                                 //이벤트이름
            G_OPERATOR_REWARD,                  //운영자에게 지급받아 재화 획득
            G_COUPON_REGISTER,                  //쿠폰 등록으로 재화 획득                    //쿠폰이름
            G_AUTO_REGENERATION,                //자동 생성으로 인한 재화의 증가
            G_QUEST_REWARD,                     //퀘스트, 미션 보상으로 재화 획득             //퀘스트이름
            G_SHARE_STAMP,                      //스템프 공유로 재화 획득
            G_PRESENT_BOX,
            G_ADVENTURE_PLAY,                   //탐험모드에서 획득
            G_GACHA,                            //갓차에서 얻어지는 보너스
            G_WORLDRANK_REWARD,                 //월드랭킹 최종보상
            G_RANKSHOP_BUY,                     // 랭킹샵 구매
            G_AD_VIEW,                          // 광고시청으로 획득
            G_OFFERWALL_REWARD,                  // 오퍼월 광고 시청으로 획득
            G_POKOFLOWER_SHARE,                 // 에코피이벤트에서 친구가 보내준 아이템 수령
            G_WORLDRANK_BONUS,                  // 월드랭킹 보너스에서 획득
            G_LOBBY_ANIMAL_GIFT,                // 로비동물에게서 획득
            G_EP_REWARD,                        // 에피소드 보상에서 획득
            G_TURN_RELAY_PLAY,                  // 턴 릴레이 플레이 보상
            G_TURN_RELAY_COOP_RANK_REWARD,      // 턴 릴레이 협동 랭킹보상
            G_TURN_RELAY_COOP_ACHIEVE_REWARD,   // 턴 릴레이 협동 달성보상
            G_TURN_RELAY_SUBMISSION_REWARD,     // 턴 릴레이 서브미션 보상
            G_CAPSULE_GACHA,                    // 캡슐가챠를 통해 획득
            G_CBU_SUPPLY,                       // cbu 보상
            G_WELCOME_MISSION_REWARD,           // 웰컴 미션 보상
            G_DECO_COLLECTION_REWARD,           // 데코 컬렉션 보상

            G_STAGE_ASSIST_MISSION_REWARD,      // 통상 스테이지 플레이 독려 보상
            G_BUY_COIN_STASH,                   // 플레이 누적형 구매유도 상품 구매로 인한 획득 (COIN)
            G_DIA_STASH,                        // 플레이 누적형 구매유도 상품 구매로 인한 획득 (DIA)
            G_STAGE_CHALLENGE_REWARD,           // 마유지의 도전장 보상으로 획득
            G_WELCOME_BACK_MISSION_REWARD,      // cbu 케어개편 보상
            G_DAY_CLEAR_REWARD,                 // 하루 마무리 보상
            G_ADTYPE_LOGIN_REWARD,              // AD 타입의 로그인 이벤트에서 획득
            G_PREMIUM_PASS_REWARD,              // 권리형 패스에서 획득
            G_SPECIALSHOPITEM_REWARD,           // 상점 내 스페셜 아이템을 통해 획득
            G_REWARD_URL,                       // 리워드 URL 보상 획득
            G_END_CONTENTS_SHOP_PURCHASE,       // 엔드 컨텐츠 상점에서 아이템 획득
            G_BINGO_LINE_REWARD,                // 빙고 이벤트 라인 보상
            G_BINGO_BONUS_REWARD,               // 빙고 이벤트 보너스 보상
            G_TREASURE_HUNT_CLEAR_REWARD,       // 보물찾기 이벤트 스테이지 클리어 보상
            G_TREASURE_HUNT_MAP_REWARD,         // 보물찾기 이벤트 데코 GET 보상
            
            G_GROUP_RANKING_REWARD,             // 그룹랭킹 참가 보상
            
            G_LOGIN_ACCUMULATE_REWARD,          // 로그인 보너스 리뉴얼 보상 : 누적
            G_LOGIN_CONTINUOUS_REWARD,          // 로그인 보너스 리뉴얼 보상 : 연속
            
            G_SPACE_TRAVEL_REWARD,              // 우주여행 이벤트 보상
            
            U_BUY_GOLD,                         //골드 구매에 재화 사용                          //프로덕션코드
            U_BUY_AP_CLOVAR,                    //클로버 구매에 재화 사용                         //프로덕션코드
            U_BUY_AP_STAR,                      //별 구매에 재화 사용                               //프로덕션코드
            U_GAME_PLAY,                        //게임 플레이에 재화 사용                       
            U_GAME_PLAY_FREE,                   //게임 플레이에 재화 사용                       
            U_BUY_ITEM_DECO,                    //아이템 구매에 재화 사용                        //아이템이름
            U_USE_GACHA,                        //가차 실행에 재화 사용                                //프로덕션코드
            U_QUEST_PLAY,                       //퀘스트, 미션 플레이에 재화 사용                    //퀘스트이름
            U_SPEED_UP_PLAY,                    //스피드업 게임플레이에 재화 사용         //추가        
            U_BUY_ITEM_MATERIAL,                //MATERIAL 아이템 구매에 재화 사용        //추가        //아이템이름
            U_BUY_ITEM_CONSUMPTION,             //CONSUMPTION 아이템 구매에 재화 사용      //추가       //아이템이름
            U_OPEN_PRESENT_BOX,                 //선물상자 즉시열기에 재화 사용          //추가
            U_REDUCE_TIME_MISSION,              //시간이 걸리는 데일리 미션의 시간 단축에 재화 사용  //추가    //퀘스트이름
            U_CONTINUE_PLAY,
            U_BUY_PRESENT_BOX,
            U_BUY_NAMETITLE,                    //L_MRSN_DTL 구매한호칭
            U_LEVELUP_CHARACTER,                //동물 레벨업에 사용
            U_BUY_GACHA,                        //가챠티켓 구매에 사용
            U_BUY_WING,                         //모험날개구매
            U_ADVENTURE_PLAY,                   //모험모드 플레이시 사용
            U_ADVENTURE_PLAY_FREE,              //모험모드 무료플레이
            U_BUY_BOMB_BOX,                     //폭탄상자 구매에 사용
            U_WORLDRANK_SHOP,                   // 월드랭킹 샵에서 구매에 사용
            U_BUY_PACKAGE,                      // 다이아 패키지 구매에 사용
            U_EXPIRED_MAIL,                     // 우편함 내부에서 수령기간 만료됨
            U_USE_CAPSULE_GACHA,                // 캡슐가챠에 토큰(메달)사용
            U_EXPIRED_CAPSULE_MEDAL,            // 캡슐가챠 상품 갱신으로 인해 이전토큰 전부 만료처리됨
            U_BUY_TURNRELAY_AP,                 // 턴 릴레이 AP 구매용으로 사용

            U_BUY_COIN_STASH,                   // 플레이 누적형 구매유도 상품 구매에 사용
            U_BUY_SPECIALSHOPITEM,              // 상점 내 스페셜 아이템 구매 시 사용
            
            U_END_CONTENTS_SHOP_PURCHASE,       // 엔드 컨텐츠 상점 구매 시 사용 (진주조개)
            U_END_CONTENTS_CHANGEMAP,           // 엔드컨텐츠 맵 변경 시 사용
            U_END_CONTENTS_CHARGE_AP,           // 엔드컨텐츠 AP 충전 시 사용

            U_BINGO_RESET,                      // 빙고 이벤트 슬롯 리셋
            
            U_BUY_LUCKY_ROULETTE,               // 럭키룰렛 구매
            G_LUCKY_ROULETTE_REWARD,            // 럭키룰렛 보상
            G_LUCKY_ROULETTE_COMPLETE_REWARD,   // 럭키룰렛 보상
            
            G_ATELIER_REWARD,                   //퍼즐 명화 보상

            G_BOOST_REWARD,
            U_BOOST_REWARD,
            
            U_ANTIQUE_STORE,                    //앤틱 스토어 상품 구매 시 사용 (앤틱스토어 주화)
            G_ANTIQUE_STORE,                    //앤틱 스토어 보너스 보상 획득 시 사용
            G_ADVENTURE_PASS_REWARD,              // 탐험 패스에서 획득
            G_SINGLE_ROUND_EVENT_REWARD,        // 수문장 크루그 성공 후 리워드 획득 시 사용
            
            G_CRIMINAL_EVENT,                   //코코의 수사일지 GET 보상
            G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,   //코코의 수사일지 최종 보상
            
            G_BUY_NRU_DIA_SHOP,                 // 다이아 상점에서 NRU 다이아 구매
            
            NULL
        }

        public string L_TAG = string.Empty; //Code_L_TAG.FC.ToString();             //재화 유형
        public string L_ULV = string.Empty;             //유저 레벨
        public string L_MRSN = string.Empty;//Code_L_MRSN.G_BUY_AT_SHOP.ToString();            //재화 변경 사유 코드
        public string L_MRSN_DTL = string.Empty;
        public long L_PMONEY_C = 0;                     //유상 재화 변동량
        public long L_FMONEY_C = 0;                     //무상 재화 변동량
        public long L_PMONEY_A = 0;                     //변동 후 유상 재화 잔액
        public long L_FMONEY_A = 0;                     //변동 후 무상 재화 잔액
        public string L_CDT = string.Empty;             //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS)
        public string L_STR1 = string.Empty;            //Gacha의 종류 1. Normal, 2. Premium
        public string L_STR2 = string.Empty;            // GameMode

        public GrowthyCustomLog_Money(Code_L_TAG tag, Code_L_MRSN code, long paidMoney, long freeMoney, long postPaidMoney, long postFreeMoney, string mrsn_DTL = null, string str1 = null)
        {
            this.L_TAG = tag.ToString();
            this.L_ULV = (SDKGameProfileManager._instance.GetMyProfile().stage - 1).ToString();
            this.L_MRSN = code.ToString();
            this.L_MRSN_DTL = mrsn_DTL;
            this.L_PMONEY_C = paidMoney;
            this.L_FMONEY_C = freeMoney;
            this.L_PMONEY_A = postPaidMoney;
            this.L_FMONEY_A = postFreeMoney;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
            this.L_STR1 = str1;
            this.L_STR2 = Global.GameInstance.GetGrowthyGameMode().ToString();
        }
    }

    public class GrowthyCustomLog_Social
    {
        public enum Code_L_TAG
        {
            SOCIAL,
        }
        public enum Code_L_RSN
        {
            SEND_INVITE,             //친구에게 초대 보내기
            ACCEPT_INVITE,          //친구 초대 수락
            SHOW_OFF,               //친구에게 자랑하기
            HELP_FRIEND,            //친구에게 도움주기
            GET_HELP,               //친구에게 도움받기
            SEND_PRESNET,           //친구에게 선물 보내기
            RECEIVE_PRESENT,        //친구에게 선문 받기
            SEND_CLOVAR,            //친구에게 활동포인트(클로버)보내기
            RECEIVE_CLOVAR,         //친구에게 활동포이트(클로버)받기
            SEND_MATERIAL,          //재료보내기
            RECEIVE_MATERIAL,       //재료 보내기
            SEND_STAMP,             //스탬프 보내기
            REQUEST_CLOVAR,         //활동포인트 요청
            SEND_WAKEUP,            //휴면 친구에게 깨우기 메세지 보내기
            RESPOND_WAKEUP,         //깨우기 응답
            RECEIVE_WAKEUP_REWARD,  //친구가 응답한 경우 받는 보상
            ADD_LINE_OA,            // 라인 공식계정 친구추가
            REQ_GAME_FRIEND,        // 게임친구 신청
            ACCEPT_GAME_FRIEND,     // 게임친구 수락
            REJECT_GAME_FRIEND,     // 게임친구 거절
            DELETE_GAME_FRIEND,     // 게임친구 삭제
            SHARE_POKOFLOWER_EVENT_REWARD   // 친구에게 에코피 이벤트 보상 공유
        }

        public string L_TAG = Code_L_TAG.SOCIAL.ToString();
        public string L_ULV = string.Empty;
        public string L_RSN = Code_L_RSN.SEND_INVITE.ToString();
        public string L_FRN_MID = string.Empty;                       //Growthy SDK Development Document
        public string L_CDT = string.Empty;
        public string L_STR1 = string.Empty;

        public GrowthyCustomLog_Social(string lastStage, Code_L_RSN socialType, string friendUid, string detailString1 = "")
        {
            this.L_TAG = Code_L_TAG.SOCIAL.ToString();
            this.L_ULV = (SDKGameProfileManager._instance.GetMyProfile().stage - 1).ToString();
            this.L_RSN = socialType.ToString();
            this.L_FRN_MID = friendUid;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
            this.L_STR1 = detailString1;
        }
    }

    public class GrowthyCustomLog_ADDOA
    {
        public enum Code_L_TAG
        {
            POPUP_SHOW,
            POPUP_CLOSE,
            ADD_OA,
        }
        public enum Code_L_STR1
        {
            AFTER_TUTORIAL,
            OA_NOT_FOLLOW,
            OUT_OF_STAMINA
        }

        public string L_LOGTYPE = "ADD_OA";
        public string L_TAG = Code_L_TAG.POPUP_SHOW.ToString();
        public string L_ULV = string.Empty;
        public string L_CDT = string.Empty;
        public string L_STR1 = string.Empty;
        public string L_NUM1 = string.Empty;


        public GrowthyCustomLog_ADDOA(Code_L_TAG actionTag, Code_L_STR1 popupWakedPhase, int gazedTime = -1)
        {
            this.L_TAG = actionTag.ToString();
            this.L_ULV = (SDKGameProfileManager._instance.GetMyProfile().stage - 1).ToString();
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
            this.L_STR1 = popupWakedPhase.ToString();

            this.L_NUM1 = gazedTime == -1 ? "" : gazedTime.ToString();
        }
    }


    public class GrowthyCustomLog_ITEM
    {
        public enum Code_L_TAG
        {
            CHANGE,                     //아이템 변동 (획득/소진)
            EDIT,                       //스템프,   수정될때마다 로그남김
            SYNTHESIS                   //재료아이템을 사용하여 데코아이템 획득
        }

        public enum Code_L_ICAT
        {
            CONSUMPTION_INGAME,                //소비아이템 _ 인게임
            CONSUMPTION_LOBBY,                   //소비아이템 _ 로비 //레디아이템
            MATERIAL,                       //재료아이템
            DECO,                       //데코아이템
            STAMP,                       //스티커      //스티커(스탬프) -> Edit만 가능하며, 업데이트로 모든 유저에게 일괄 스탬프 획득인 경우 획득 로그를 남기지 않는다
            GIFTBOX,                    //선물BOX
            COSTUME,
            ANIMAL,
            ANIMAL_OVERLAP_TICKET
        }

        public enum Code_L_RSN
        {
            G_SOCIAL_REWARD,            //소셜보상으로 인한 획득
            G_BUY_ITEM,                 //아이템구매하여 획득
            //G_MONSTER_KILL,             //제거
            G_GACHA,                    //가챠 보상을 통한 획득                      //가차 상품의 상품명을 L_RSN_DTL 컬럼에 넣어주세요
            G_GACHA_TICKET,             //가챠 티켓을 통한 획득                      //가차 상품의 상품명을 L_RSN_DTL 컬럼에 넣어주세요
            G_SYNTHESIS_RESULT,         //아이템 합성으로 획득
            G_EVENT_ADMIN,              //이벤트 지급
            G_GAME_PLAY,                //플레이를 하여 아이템 획득                    //스테이지 번호를 L_RSN_DTL 컬럼에 넣어주세요
            G_QUEST_REWARD,             //퀘스트, 미션, 업적보상으로 아이템 획득        //미션명/퀘스트명/업적명을 L_RSN_DTL 컬럼에 넣어주세요
            G_EDIT_STAMP,               //스템프 수정
            G_SHARE_STAMP,              //스템프 공유
            G_PRESENT_BOX,              //선물상자로 아이템 획득
            G_EVENT_LOGIN,              //로그인 이벤트로 아이템 획득                   //이벤트명을 L_RSN_DTL 컬럼에 넣어주세요
            G_EVENT_FLOWER,             //Flower 이벤트로 아이템 획득                    //이벤트명을 L_RSN_DTL 컬럼에 넣어주세요
            G_EVENT_OTHER,              //로그인, Flower 이외의 이벤트로 아이템 획득       //이벤트명을 L_RSN_DTL 컬럼에 넣어주세요
            G_TOWN_GIFT,                //?새로 만들어짐
            G_RANKING_REWARD,           //랭킹 보상으로 아이템 획득                        //Rank #을 L_RSN_DTL 컬럼에 넣어주세요
            G_LEVEL_UP_REWARD,
            G_ADVENTURE_PLAY,           //탐험모드
            G_FIRST_USE_REWARD,         //
            G_POKOFLOWER_SHARE,         // 에코피이벤트에서 친구가 보내준 아이템 수령
            G_WORLDRANK_BONUS,          // 월드랭킹 보너스에서 획득
            G_LOBBY_ANIMAL_GIFT,        // 로비동물에게서 획득
            G_EP_REWARD,                // 에피소드 보상에서 획득

            U_USE_ITEM,                 //아이템 사용으로 소진
            U_USE_FREE_ITEM,            // 무료 아이템 소진
            U_DELETE_ITEM,              //아이템 삭제로 소진
            U_SYNTHESIS_MATERIAL,        //아이템 합성으로 소진(재료 아이템)

            G_OVERLAP_TICKET_CHANGE,    //더 이상 뽑을 수 없는 가챠티켓을 중첩티켓으로 교환
            U_OVERLAP_TICKET,           //중첩티켓으로 특정 동물을 획득

            G_BOOST_REWARD,
            U_BOOST_REWARD,

            G_MATERIAL_BOX,             //재료상자로 인해 얻은 재료
            G_MATERIAL_RANKUP,          //재료교환은로 얻은 재료
            U_MATERIAL_RANKUP,          //재교교환으로 사용된 재료
            G_BOMB_BOX,                 //폭탄상자에서 얻은 폭탄 및 보너스 재료
            G_ANIMAL_TO_OVERLAP_TICKET, //최대중첩인 경우 동물티켓에서 중첩티켓으로 교환
            G_AD_VIEW,                  // 광고 뷰로 인한 아이템 획득
            G_RANKSHOP_BUY,             // 랭킹샵에서 황금나뭇잎과 아이템을 교환
            
            U_EXPIRED_MAIL,                     // 우편함 내부에서 수령기간 만료됨
            G_CAPSULE_GACHA,                    // 캡슐가챠를 통해 획득
            G_TURN_RELAY_COOP_RANK_REWARD,      // 턴 릴레이 협동 랭킹보상
            G_TURN_RELAY_COOP_ACHIEVE_REWARD,   // 턴 릴레이 협동 달성보상
            G_TURN_RELAY_SUBMISSION_REWARD,     // 턴 릴레이 서브미션 보상
            G_CBU_SUPPLY,                       // CBU 로그인 지원품
            G_WELCOME_MISSION_REWARD,           // 웰컴 미션 보상
            G_STAGE_ASSIST_MISSION_REWARD,      // 통상 스테이지 플레이 독려기능 보상
            G_STAGE_CHALLENGE_REWARD,           // 마유지의 도전장 보상으로 획득
            G_WELCOME_BACK_MISSION_REWARD,      // cbu 케어개편 보상
            G_TUTORIAL,                         // 인게임 아이템 튜토리얼
            G_ADTYPE_LOGIN_REWARD,              // AD 타입의 로그인 이벤트에서 획득
            G_DECO_COLLECTION_REWARD,           // 데코 컬렉션 이벤트 보상
            G_PREMIUM_PASS_REWARD,              // 권리형 패스에서 획득
            G_SPECIALSHOPITEM_REWARD,           // 상점 내 스페셜 아이템을 통해 획득
            G_REWARD_URL,                       // RewardURL 보상 획득
            G_END_CONTENTS_SHOP_PURCHASE,        // 엔드컨텐츠 상점에서 아이템을 교환
            G_BINGO_LINE_REWARD,                // 빙고 이벤트 라인 보상
            G_BINGO_BONUS_REWARD,               // 빙고 이벤트 보너스 보상
            G_LOGIN_ACCUMULATE_REWARD,              // 로그인 보너스 리뉴얼 : 누적
            G_LOGIN_CONTINUOUS_REWARD,              // 로그인 보너스 리뉴얼 : 연속
            G_TREASURE_HUNT_CLEAR_REWARD,       // 보물찾기 이벤트 스테이지 클리어 보상
            G_TREASURE_HUNT_MAP_REWARD,         // 보물찾기 이벤트 데코 GET 보상
            G_ANTIQUE_STORE,                    //앤틱 스토어 상점에서 아이템을 교환
            G_ADVENTURE_PASS_REWARD,            // 탐험 패스에서 획득
            G_SINGLE_ROUND_EVENT_REWARD,        // 수문장 크루그 이벤트 보상
            G_LUCKY_ROULETTE_REWARD,            // 럭키룰렛 이벤트 보상
            G_LUCKY_ROULETTE_COMPLETE_REWARD,   // 럭키룰렛 이벤트 최종 보상
            G_CRIMINAL_EVENT,                   //코코의 수사일지 GET 보상
            G_CRIMINAL_EVENT_ALL_STAGE_CLEAR,   //코코의 수사일지 최종 보상
            G_GROUP_RANKING_REWARD,             // 그룹랭킹 보상
            G_SPACE_TRAVEL_REWARD,              // 우주여행 이벤트 보상
            G_ATELIER_REWARD,                   //퍼즐 명화 보너스 보상
            NULL
        }

        public string L_TAG = Code_L_TAG.CHANGE.ToString();                                                                         //Not Null
        public string L_ULV = string.Empty;                             //유저레벨                                      //Not Null
        public string L_ICAT = Code_L_ICAT.MATERIAL.ToString();         //아이템카테고리                                //Not Null                           
        public string L_ITEM = string.Empty;                         //아이템 ID       //게임내 아이템의 ID               //Not Null
        public string L_ITEM_NM = string.Empty;                         //아이템 영문이름                                  //Not Null
        public int L_ITEM_C = 0;                         //아이템의 수량                                   //Not Null                        
        public string L_RSN = Code_L_RSN.G_QUEST_REWARD.ToString();         //이벤트 사유 코드
        public string L_RSN_DTL = string.Empty;                             //아이템 이벤트 상세 사유 코드
        public string L_CDT = string.Empty;             //디바이스시간                                                     //Not Null       
        public string L_STR1 = string.Empty;

        public GrowthyCustomLog_ITEM(Code_L_TAG tag, Code_L_ICAT cat, string item, string item_nm, int item_c, Code_L_RSN rsn, string LRsnDtl = null)
        {
            this.L_TAG = tag.ToString();
            this.L_ULV = (SDKGameProfileManager._instance.GetMyProfile().stage - 1).ToString();
            this.L_ICAT = cat.ToString();             //업적카테고리
            this.L_ITEM = item.ToString();      //업적이름
            this.L_ITEM_NM = item_nm.ToString();           //업적달성결과
            this.L_ITEM_C = item_c;           //업적달성결과
            this.L_RSN = rsn.ToString();
            this.L_RSN_DTL = LRsnDtl;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
            // this.L_STR1 = (Global.GameType.Equals(GameType.NORMAL) && ManagerOvisBoostEvent.instance != null && ManagerOvisBoostEvent.instance.IsActiveUser())
            //     ? ManagerOvisBoostEvent.instance.GetBoostStep().ToString() : "0";        // 오비스 부스트 관련 레디 아이템 로그로, 현재 사용되지 않음.
        }
    }

    public class GrowthyCustomLog_PLAYSTART
    {
        /// <summary>
        /// 플레이 이벤트 유형
        /// </summary>
        public enum Code_L_TAG
        {
            PLAYSTART,
        }

        public string L_TAG   = Code_L_TAG.PLAYSTART.ToString(); //Not Null
        public ulong  L_PID   = ulong.MinValue;                  //Not Null
        public string L_ULV   = string.Empty;                    //게임(스테이지) 시작 시점의 이용자 Stage //Not Null
        public string L_GMOD  = "NORMAL";
        public string L_STG   = string.Empty; //플레이한 스테이지                      //Not Null
        public string L_REP   = "N";          //최초플레이 여부 Y,N
        public string L_CHAR  = string.Empty; //플레이에 사용한 동물리스트
        public string L_ITEM  = string.Empty; //플레이 중 사용한 아이템 리스트
        public string L_CDT   = string.Empty; //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS) //Not Null
        
        public string L_STR7  = string.Empty; //부스팅 레벨
        public string L_STR10 = string.Empty; //playstart Detail Info
        
        public int    L_NUM1  = 0;            //해당 스테이지를 플레이했던 횟수 (이번 플레이 미포함)
        public int    L_NUM2  = 0;            //L_GMOD : EVENT 인 경우 이벤트 인덱스//L_GMOD : NORMAL & L_STR5 <> N 인 경우 랭킹 모드 그룹//L_GMOD : ADVENTURE인 경우 챕터

        public GrowthyCustomLog_PLAYSTART(ulong pid, string ulv, string stg, string gmod,
            bool rep, string character, string item, int num1, int num2, string str7 = null, string detailInfo = null)
        {
            this.L_TAG = Code_L_TAG.PLAYSTART.ToString();
            this.L_PID = pid;
            this.L_ULV = ulv;

            this.L_GMOD = gmod;
            this.L_STG = stg;

            this.L_REP = rep ? "Y" : "N";
            this.L_CHAR = character;
            this.L_ITEM = item;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();

            this.L_NUM1 = num1;
            this.L_NUM2 = num2;

            this.L_STR7 = str7;
            this.L_STR10 = detailInfo;
        }
    }
    
    public class GrowthyCustomLog_PLAYEND
    {
        public class CLASS_L_ITEM     //플레이중 사용한 아이템 리스트
        {
            public string L_CAT = Code_L_CAT.CONSUMPTION_INGAME.ToString();     //아이템 카테고리
            public string L_IID = string.Empty;                                 //아이템 ID
            public int L_CNT = 0;                           //아이템 사용 갯수
        }

        public class CLASS_L_ANIMAL     //플레이중 사용한 동물
        {
            public int L_CID = 0;     //동물번호
            public int L_LEV = 0;                                 //레벨
            public int L_CNT = 0;                           //중첩
        }

        public enum Code_L_TAG
        {
            PLAYEND,    //게임종료
        }

        public enum Code_L_GMOD
        {
            TUTORIAL,
            NORMAL,
            _EVENT,  // EVENT는 더이상 나오면 안됨
            EVENT_FAILRESET,
            EVENT_COLLECT,
            EVENT_SCORE,
            RANKING,
            ADVENTURE,
            MOLE,
            COINBONUS,
            ADVENTURE_EVENT,
            WORLD_RANKING,
            TURN_RELAY,
            END_CONTENTS,
            BINGO,
            TREASURE_HUNT,
            SPACE_TRAVEL,
            ATELIER_SCORE,
            ATELIER_COLLECT,
        }

        public enum Code_L_PWIN
        {
            WIN,            //승리
            LOSE,           //패배
            DRAW,
        }

        public enum Code_L_PWIN_DTL
        {
            NULL,
            STAR1,
            STAR2,
            STAR3,
            STAR4,
            STAR5,
        }

        public enum Code_L_CAT
        {
            CONSUMPTION_INGAME,                     //소비아이템_인게임
            CONSUMPTION_LOBBY,                      //소비 아이템_로비
            MATERIAL,
            DECO,
            AD_ITEM_LOBBY,
        }

        public string L_TAG = Code_L_TAG.PLAYEND.ToString();                            //Not Null
        public ulong L_PID = ulong.MinValue;        //플레이아이디                        //Not Null
        public string L_ULV = string.Empty;             //종료시점 유저 스테이지          //Not Null
        public string L_ULV_S = string.Empty;           //시작시점 유저 스테이지
        public string L_GMOD = Code_L_GMOD.NORMAL.ToString();
        public string L_STG = string.Empty;             //플레이한 스테이지                             //Not Null
        public string L_PWIN = Code_L_PWIN.WIN.ToString();            //플레이한 결과                     //Not Null
        public string L_PWIN_DTL = string.Empty;         //플레이 상세결과             //Not Null

        public int L_SCR = 0;            //플레이 점수                                                   //Not Null
       // public int L_CASH_G = 0;         //획득 캐시
        public int L_GOLD_G = 0;         //획득 코인
        public long L_PTM = 0;           //플레이시간                                                    //Not Null
        public string L_REP = "N";          //최초플레이 여부 Y,N
        public string L_IREP = "N";          //플레이중 이어하기 아이템 사용여부 //Y,N

        public string L_CHAR = string.Empty; //동물리스트
        public string L_ITEM = string.Empty; //public List<CLASS_L_ITEM> L_ITEM = new List<CLASS_L_ITEM>(); //아이템
        public string L_CDT  = string.Empty; //클라이언트 디바이스 기준 시간(YYYYMMDD HH22MISS)                  //Not Null

        public int L_NUM1 = 0; // 남은턴
        public int L_NUM2 = 0; // 획득한 별
        public int L_NUM3 = 0; // 이벤트스테이지 인덱스
        public int L_NUM4 = 0; // play count
        public int L_NUM5 = 0; // seed
        public int L_NUM6 = 0; // clover
        public int L_NUM7 = 0; // used Turn
        public int L_NUM8 = 0; // continue Reconfirm
        
        public string L_STR1  = string.Empty; // 미션완료 여부        //Y,N
        public string L_STR2  = string.Empty; // 챕터미션 완료 여부    //Y,N
        public string L_STR3  = string.Empty; // 스테이지 리뷰 평점    //1,2,3,4,5
        public string L_STR4  = string.Empty; // 남은스테이지목표
        public string L_STR5  = string.Empty; // 랭킹모드 참여 및 프로필 동의 여부   //동의Y_AGREE //참가하나 동의하지않은경우 Y_DISAGREE
        public string L_STR6  = string.Empty; // 최종클리어 여부
        public string L_STR7  = string.Empty; // 부스팅 레벨
        public string L_STR8  = string.Empty; // 클리어등급 최초획득여부
        public string L_STR9  = string.Empty; // 시드타입
        public string L_STR10 = string.Empty; // playend Detail Info

        public GrowthyCustomLog_PLAYEND(ulong pid, string lastStage, string beforeLastStage, string currentStage, Code_L_GMOD stageType, Code_L_PWIN win, Code_L_PWIN_DTL star,
            int score, int getCoin, long playTime, bool firstPlay, bool continuePlay, int leftTurn, string useItemList, int getHeart, bool clearStageMission,
            bool clearChapterMission, string reviewCount = null, int tempEventIndex = 0, string remainGoals = null, string rankMode = null, string ecopi = null,
            string boostLevel = null, string firstFlowerLevel = null, int levelAdjusted = 0, int usedTurn = 0, int continueReconfirm = 0, string detailInfo = null)
        {
            var seedOrAdjust = ServerRepos.IngameSeed <= 0 ? 0 : ServerRepos.IngameSeed;
            if(seedOrAdjust == 0 && ServerRepos.IngameLevelAdjust != 0)
            {
                seedOrAdjust = -1 * ServerRepos.IngameLevelAdjust;
            }

            this.L_TAG = Code_L_TAG.PLAYEND.ToString();
            this.L_PID = pid;
            this.L_ULV = lastStage;
            this.L_ULV_S = beforeLastStage;

            this.L_GMOD = stageType.ToString();
            this.L_STG = currentStage;
            this.L_PWIN = win.ToString();
            this.L_PWIN_DTL = star != Code_L_PWIN_DTL.NULL ? star.ToString(): string.Empty;

            this.L_SCR = score;
            this.L_GOLD_G = getCoin;
            this.L_PTM = playTime;

            this.L_REP = firstPlay ? "Y" : "N";
            this.L_IREP = continuePlay ? "Y" : "N";
            this.L_ITEM = useItemList;
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();

            this.L_NUM1 = leftTurn;
            this.L_NUM2 = getHeart;
            this.L_NUM3 = tempEventIndex;
            this.L_NUM4 = Global.GameInstance.GetGrowthyPlayCount();
            this.L_NUM5 = seedOrAdjust;
            this.L_NUM6 = ServerRepos.User.AllClover;
            this.L_NUM7 = usedTurn;
            this.L_NUM8 = continueReconfirm;

            this.L_STR1  = clearStageMission ? "Y" : "N";
            this.L_STR2  = clearChapterMission ? "Y" : "N";
            this.L_STR3  = reviewCount;
            this.L_STR4  = remainGoals;
            this.L_STR5  = rankMode;
            this.L_STR6  = ecopi;
            this.L_STR7  = boostLevel;
            this.L_STR8  = firstFlowerLevel;
            this.L_STR9  = ServerRepos.IngameSeedType.ToString();
            this.L_STR10 = detailInfo;
        }
    }
    
    public class GrowthyCustomLog_Achievement
    {
        public enum Code_L_TAG
        {
            QUEST,
            MISSION,
            NAMETITLE,
            EVENT_MODE,
            STAGE,
            AD,
            LINK,
            PACKAGE_MISSION,
            PACKAGE,
            HOUSING,
            COSTUME,
            APP_REVIEW,
            CAPSULE_GACHA,
            SCENE,
            PREMIUM_PASS_REWARD,
            LOGIN_EVENT,
            TREASURE_HUNT,
            DIA_STASH,
            ANTIQUE_STORE,
            ADVENTURE_PASS_REWARD,
            CRIMINAL_EVENT,
            OFFERWALL,
            GROUP_RANK,
            COMPLETE_PACK
        }

        public enum Code_L_CAT
        {
            NORMAL,
            MAIN,
            DAILY,
            DECO,
            EVENT,
            HOBBY_TIME,
            //SEASON,
            //COLLABO,
            RANK,
            ECOPI,  //에코피
            POKOFLOWER_EVENT,
            ADVENTURE,  //탐험
            ADVENTURE_CHALLENGE,
            WHAC_A_MOLE,    //두더지
            COLLECT_MATERIAL,   //
            BOOSTING,
            ALPHABET_EVENT,
            ADVENTURE_EVENT,
            AD_VIEW, // 광고시청
            ENTRY_QUEST,
            WORLD_RANKING,
            LAST_FLOWER_RANK,
            TIME_REDUCE_BOX,        // 박스에 시간감소처리
            SPECIAL_EVENT,          // 모으기 이벤트
            TURN_RELAY_EVENT,       // 턴 릴레이 이벤트
            OPEN_LINK,              // 링크 클릭
            REWARD_URL,             // 딥링크로 게임시작
            WELCOME_MISSION,        // 웰컴미션
            SPOT_DIA_SUGGEST,       // 스팟다이아 유저에게 제안하는 순간
            COIN_STASH_STATE_CHANGED,   //코인스태시 상태변경
            STAGE_ASSIST_MISSION_CLEAR, // 통상스테이지 독려이벤트 미션 클리어
            STAGE_CHALLENGE_CLEAR,      // 마유지의 도전장 클리어
            CHANGE_HOUSING,         // 하우징 변경
            CHANGE_COSTUME,         // 코스튬 변경
            WELCOME_BACK_MISSION,
            APP_REVIEW_ACTION,
            CAPSULE_GACHA,
            PACKAGE_SUGGEST,        //로비 진입시 패키지 강제 노출
            SCENE_START,
            SCENE_SKIP,
            DECO_COLLECTION,
            G_PREMIUM_PASS,         //프리미엄 패스권을 구매했을 때
            G_PREMIUM_PASS_REWARD,  //프리미엄 패스 보상을 받았을 때
            BINGO_REWARD,           //빙고 이벤트 보상을 받았을 때
            EVENT_FAILRESET,        //연속모드 광고 지면 추가 광고창 출력 시
            LOGIN_EVENT_REWARD,
            LOGIN_EVENT_POPUP_OPEN,
            TREASURE_HUNT_MAP_REWARD,
            DIA_STASH_STEP,         //다이아 주머니 인게임 클리어 이후 다이아 적립될 때
            DIA_STASH_BUY,         //다이아 주머니 구매했을 때
            NPU_CONTINUE_AD,
            NPU_SPOT_PACKAGE,
            ANTIQUE_STORE_GET_BONUS,   //엔틱스토어 보상 획득
            SINGLE_ROUND_EVENT,   //수문장 크루그 보상 획득
            G_ADVENTURE_PASS,         //탐험 패스권을 구매했을 때
            G_ADVENTURE_PASS_REWARD,  //탐험 패스 보상을 받았을 때
            CRIMINAL_EVENT_STAGE_CLEAR,
            OFFERWALL_SELECT,        //오퍼월 배너 클릭
            COINBONUSSTAGE,          //코인 보너스 스테이지 이벤트
            COMPLETE_PACK_GET_REWARD, //컴플리트 팩 보상 획득 
            GROUP_RANK_GET_REWARD, //그룹랭킹 보상 획득
            SPOT_CLOVER_PACKAGE,   //스팟 클로버 패키지
            AD_SHOW,               // 광고 노출 (모듈화 따로 없이 필요한 곳에서 직접 사용)
            SPACE_TRAVEL_SELECT,   // 우주 여행 아이템 선택
            SPACE_TRAVEL_USE,      // 우주 여행 아이템 사용
            ATELIER_PUZZLE_REWARD, //퍼즐 명화 보상
        }

        public enum Code_L_ARLT
        {
            SUCCESS,
            FAIL,
            FIRST_CLEAR,
        }
        
        public string L_TAG = Code_L_TAG.QUEST.ToString();
        public string L_ULV = string.Empty;             //이용자 레벨
        public string L_CAT = Code_L_CAT.NORMAL.ToString();          //업적카테고리
        public string L_ANM = string.Empty;             //업적이름
        public string L_ARLT = Code_L_ARLT.SUCCESS.ToString();            //업적달성결과
        public string L_CDT = string.Empty;             //디바이스시간
        public int L_NUM1 = 0;                          //FIRST_CLEAR클리어시 스테이지 도전횟수
        public string L_STR1 = string.Empty;

        public GrowthyCustomLog_Achievement(Code_L_TAG tag, Code_L_CAT cat, string anm, Code_L_ARLT arlt, int num1 = 0, string str1 = null)
        {
            this.L_TAG = tag.ToString();
            this.L_ULV = (SDKGameProfileManager._instance.GetMyProfile().stage - 1).ToString();
            this.L_CAT = cat.ToString();             //업적카테고리
            this.L_ANM = anm;      //업적이름
            this.L_ARLT = arlt.ToString();           //업적달성결과
            this.L_CDT = ServiceSDKManager.instance.GetTimeString();
            this.L_NUM1 = num1;
            this.L_STR1 = str1;
        }        
    }

    public class GrowthyCusmtomLogHelper
    {
        public static void SendGrowthyLog(int rewardType, int rewardCount, GrowthyCustomLog_Money.Code_L_MRSN moneyMRSN, GrowthyCustomLog_ITEM.Code_L_RSN itemRSN, string QuestName)
        {
            if (rewardType == (int)RewardType.clover)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                moneyMRSN,
                0,
                rewardCount,
                0,
                (int)(ServerRepos.User.AllClover),
                QuestName
                );

                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int)RewardType.cloverFreeTime)
            {
                var freeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag:        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat:        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item:       $"CLOVER_{rewardCount / 60}m",
                    item_nm:    $"CLOVER_{rewardCount / 60}m",
                    item_c:     1,
                    rsn:        itemRSN,
                    LRsnDtl:    QuestName
                );

                var docMoney = JsonConvert.SerializeObject(freeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docMoney);

                var usefreeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag:        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat:        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item:       $"CLOVER_{rewardCount / 60}m",
                    item_nm:    $"CLOVER_{rewardCount / 60}m",
                    item_c:     -1,
                    rsn:        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                    LRsnDtl:    QuestName
                );
                var doc1 = JsonConvert.SerializeObject(usefreeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
            }
            else if (rewardType == (int)RewardType.wingFreetime)
            {
                var freeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item: $"WING_{rewardCount / 60}m",
                    item_nm: $"WING_{rewardCount / 60}m",
                    item_c: 1,
                    rsn: itemRSN,
                    LRsnDtl: QuestName
                );

                var docMoney = JsonConvert.SerializeObject(freeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docMoney);

                var usefreeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item: $"WING_{rewardCount / 60}m",
                    item_nm: $"WING_{rewardCount / 60}m",
                    item_c: -1,
                    rsn: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                    LRsnDtl: QuestName
                );
                var doc1 = JsonConvert.SerializeObject(usefreeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
            }
            else if (rewardType == (int)RewardType.readyItem3_Time ||
                     rewardType == (int)RewardType.readyItem4_Time || 
                     rewardType == (int)RewardType.readyItem5_Time || 
                     rewardType == (int)RewardType.readyItem6_Time || 
                     rewardType == (int)RewardType.readyItemBomb_Time)
            {
                string itemPrefix = "";
                switch( rewardType )
                { 
                    case (int)RewardType.readyItem3_Time:       itemPrefix = "readyItem3_"; break;
                    case (int)RewardType.readyItem4_Time:       itemPrefix = "readyItem4_"; break;
                    case (int)RewardType.readyItem5_Time:       itemPrefix = "readyItem5_"; break;
                    case (int)RewardType.readyItem6_Time:       itemPrefix = "readyItem6_"; break;
                    case (int)RewardType.readyItemBomb_Time:    itemPrefix = "readyItem_AllBomb_"; break;
                }

                var freeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item: $"{itemPrefix}{rewardCount / 60}m",
                    item_nm: $"{itemPrefix}{rewardCount / 60}m",
                    item_c: 1,
                    rsn: itemRSN,
                    LRsnDtl: QuestName
                );

                var docMoney = JsonConvert.SerializeObject(freeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docMoney);

                var usefreeTimeClover = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    tag: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    item: $"{itemPrefix}{rewardCount / 60}m",
                    item_nm: $"{itemPrefix}{rewardCount / 60}m",
                    item_c: -1,
                    rsn: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                    LRsnDtl: QuestName
                );
                var doc1 = JsonConvert.SerializeObject(usefreeTimeClover);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
            }
            else if (rewardType == (int)RewardType.coin)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                moneyMRSN,
                0,
                rewardCount,
                (int)(ServerRepos.User.coin),
                (int)(ServerRepos.User.fcoin),
                QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int)RewardType.star)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                moneyMRSN,
                0,
                rewardCount,
                0,
                (int)(ServerRepos.User.fcoin),
                QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int)RewardType.jewel)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                moneyMRSN,
                0,
                rewardCount,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int)RewardType.stamp)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.STAMP,
                      "Stamp" + rewardCount.ToString(),
                      "Stamp",
                      1,
                      itemRSN,
                      QuestName
                  );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType > (int)RewardType.material)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                      "MATERIAL_" + ((int)rewardType - (int)RewardType.material).ToString(),
                      "material",
                      rewardCount,
                      itemRSN,
                    QuestName
                  );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType == (int)RewardType.costume)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.COSTUME,
                      "COSTUME" + rewardCount,
                      "COSTUME" + rewardCount,
                      1,
                      itemRSN,
                    QuestName
                  );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType == (int)RewardType.boxBig || rewardType == (int)RewardType.boxMiddle || rewardType == (int)RewardType.boxSmall)
            {
                var growthyBox = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.GIFTBOX,
                      "GIFTBOX_" + ((RewardType)rewardType).ToString(),
                      "GIFTBOX_" + ((RewardType)rewardType).ToString(),
                      1,
                     itemRSN,
                      QuestName
                  );
                var docBox = JsonConvert.SerializeObject(growthyBox);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docBox);
            }
            else if (rewardType >= (int)RewardType.readyItem1 && rewardType <= (int)RewardType.readyItem6)
            {
                string r = "ReadyItem" + ((READY_ITEM_TYPE)(rewardType - (int)RewardType.readyItem1)).ToString();

                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    ((RewardType)rewardType).ToString(),
                    r,
                    rewardCount,
                    itemRSN,
                    QuestName
                );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType >= (int)RewardType.ingameItem1 && rewardType <= (int)RewardType.ingameItem5)
            {
                string r;
                if (rewardType == (int)RewardType.ingameItem5)
                {
                    r = "InGameItem5";
                }
                else
                {
                    r = "InGameItem" + ((GameItemType)(rewardType - (int)RewardType.ingameItem1 + (int)GameItemType.HAMMER)).ToString();
                }
                
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                    ((RewardType)rewardType).ToString(),
                    r,
                    rewardCount,
                    itemRSN,
                    QuestName
                );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType == (int)RewardType.wing)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var rewardWing = new ServiceSDK.GrowthyCustomLog_Money
                                         (
                                           ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.WA,
                                           moneyMRSN,
                                           0,
                                           rewardCount,
                                           0,
                                           (int)(ServerRepos.User.AllWing),
                                          QuestName
                                         );
                var docReward = JsonConvert.SerializeObject(rewardWing);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docReward);
            }
            else if (rewardType == (int)RewardType.expBall)
            {
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var growthyExp = new ServiceSDK.GrowthyCustomLog_Money
                                    (
                                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.GB,
                                        moneyMRSN,
                                        0,
                                        rewardCount,
                                        0,
                                        (int)(ServerRepos.User.expBall),
                                        QuestName
                                    );
                var docExp = JsonConvert.SerializeObject(growthyExp);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docExp);
            }
            else if (rewardType == (int)RewardType.gachaTicket)
            {
                int useCount = 1;
                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.U_USE_GACHA || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.U_EXPIRED_MAIL)
                    useCount = -1;

                if (moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PLAY || moneyMRSN == GrowthyCustomLog_Money.Code_L_MRSN.G_GAME_PLAY)
                {
                    QuestName = Global.GameInstance.GetGrowthyGameMode().ToString();
                }

                var rewardTicket = new ServiceSDK.GrowthyCustomLog_Money
                  (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.GT,
                    moneyMRSN,
                    0,
                    useCount,
                    0,
                    0,
                   QuestName,
                   rewardCount.ToString()
                  );
                var DocTicket = JsonConvert.SerializeObject(rewardTicket);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", DocTicket);
            }
            else if (rewardType == (int)RewardType.animal)
            {
                var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                      "Animal_" + rewardCount.ToString(),
                      "Animal",
                     1,
                    itemRSN,
                   QuestName
                  );
                var DocTicket = JsonConvert.SerializeObject(rewardTicket);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);
            }
            else if (rewardType == (int)RewardType.housing)
            {
                int housingIdx = (int)(rewardCount / 10000);
                int modelIdx = (int)(rewardCount % 10000);
                //그로씨
                string fileName = string.Format("h_{0}_{1}", housingIdx, modelIdx);
                string ItemName = string.Format("DECO-{0}-{1}", housingIdx, modelIdx);
                
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                              ItemName,//plusData.housingIndex.ToString(),
                              fileName,
                              1,
                              itemRSN,
                              QuestName
                          );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            else if (rewardType == (int)RewardType.toy)
            {
                var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                      "POKOYURA_" + rewardCount.ToString(),
                      "POKOYURA_" + rewardCount.ToString(),
                     1,
                    itemRSN,
                   QuestName
                  );
                var DocTicket = JsonConvert.SerializeObject(rewardTicket);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);
            }
            else if (rewardType == (int)RewardType.animalOverlapTicket)
            {
                Debug.Log(
                    "rewardType : " + rewardType + "\n" +
                    "rewardCount : " + rewardCount + "\n" +
                    "moneyMRSN : " + moneyMRSN + "\n" +
                    "itemRSN : " + itemRSN + "\n" +
                    "QuestName : " + QuestName
                );

                var item = new GrowthyCustomLog_ITEM
                (
                    tag: GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    cat: GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL_OVERLAP_TICKET,
                    item: "OverlapTicket" + rewardCount.ToString(),
                    item_nm: "OverlapTicket" + rewardCount.ToString(),
                    item_c: 1,
                    rsn: itemRSN,
                    LRsnDtl: QuestName
                );

                var jsonData = JsonConvert.SerializeObject(item);
                ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", jsonData);
            }
            else if (rewardType == (int) RewardType.rankToken)
            {
                // 랭킹토큰(황금나뭇잎)의 경우, 획득처는 월드랭킹 보상/ 사용처는 월드랭킹샵으로 한정된다

                int token = 0;
                ServerUserTokenAsset userTokenAsset = null;
                if (ServerRepos.UserTokenAssets.TryGetValue(1, out userTokenAsset))
                {
                    token = userTokenAsset.amount;
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                    tag:            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.RT,
                    code:           moneyMRSN,
                    paidMoney:      0,
                    freeMoney:      rewardCount,
                    postPaidMoney:  0,
                    postFreeMoney:  token,
                    mrsn_DTL:       QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int)RewardType.capsuleGachaToken)
            {
                int token = 0;
                ServerUserTokenAsset userTokenAsset = null;
                if (ServerRepos.UserTokenAssets.TryGetValue(2, out userTokenAsset))
                {
                    token = userTokenAsset.amount;
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                    tag: ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AT_CAPSULEMEDAL,
                    code: moneyMRSN,
                    paidMoney: 0,
                    freeMoney: rewardCount,
                    postPaidMoney: 0,
                    postFreeMoney: token,
                    mrsn_DTL: QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (rewardType == (int) RewardType.revivalAndHeal)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                    "ingameItem5",
                    "InGameItemHEAL_ONE_ANIMAL",
                    rewardCount,
                    itemRSN,
                    QuestName
                );
                var docStamp = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType == (int)RewardType.skillCharge)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                    "ingameItem6",
                    "InGameItemSKILL_HAMMER",
                    rewardCount,
                    itemRSN,
                    QuestName
                );

                var docStamp = JsonConvert.SerializeObject(useReadyItem);

                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp);
            }
            else if (rewardType == (int)RewardType.endContentsToken)
            {
                int token = 0;
                ServerUserTokenAsset userTokenAsset = null;
                if (ServerRepos.UserTokenAssets.TryGetValue(3, out userTokenAsset))
                {
                    token = userTokenAsset.amount;
                }

                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                    tag: ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.EC,
                    code: moneyMRSN,
                    paidMoney: 0,
                    freeMoney: rewardCount,
                    postPaidMoney: 0,
                    postFreeMoney: token,
                    mrsn_DTL: QuestName
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
        }

        public static void SendStartGrowthyLog()
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();
            var gmod = Global.GameInstance.GetGrowthyGameMode();
            if (gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_FAILRESET ||
                gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_COLLECT ||
                gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT_SCORE)
            {
                gmod = GrowthyCustomLog_PLAYEND.Code_L_GMOD._EVENT;
            }

            var playCount = Global.GameInstance.GetGrowthyPlayCount();
            if (gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.WORLD_RANKING ||
                gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.END_CONTENTS)
            {
                playCount += 1;
            }
            
            var boostingLevel = "0";
            if (gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL && ManagerNoyBoostEvent.instance != null && ManagerNoyBoostEvent.instance.IsActiveUser())
            {
                boostingLevel = ManagerNoyBoostEvent.instance.GetBoostStep().ToString();
            }

            var playStart = new GrowthyCustomLog_PLAYSTART
            (
                myProfile.userID,
                (myProfile.stage - 1).ToString(),
                Global.GameInstance.GetGrowthyStageIndex(),
                gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD._EVENT ? "EVENT" : gmod.ToString(),
                Global.GameInstance.IsFirstPlay(),
                GetUseAnimalData(gmod),
                GetUseReadyItemData(gmod),
                playCount,
                Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
                boostingLevel,
                "[0]"
            );

            var doc = JsonConvert.SerializeObject(playStart);
            doc = doc.Replace("\"[0]\"", Global.GameInstance.GetGrowthy_PLAYSTART_DETAILINFO());
            ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYSTART", doc);
        }

        private static string GetUseReadyItemData(GrowthyCustomLog_PLAYEND.Code_L_GMOD gmod)
        {
            List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM> itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
            
            for (int i = 0; i < 6; i++)
            {
                if (!Global.GameInstance.CanUseReadyItem(i))
                {
                    continue;
                }
                
                if (UIPopupReady.readyItemUseCount.Length > i && 
                    UIPopupReady.readyItemUseCount[i].Value > 0)
                {
                    var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                    {
                        L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
                        L_IID = ((READY_ITEM_TYPE)i).ToString(),
                        L_CNT = UIPopupReady.readyItemUseCount[i].Value
                    };
                    itemList.Add(readyItem);
                }
            }
            
            //더블레디 아이템(기존에 서버에서 남기는 값과 동일하게 처리하기 위해)
            for (int i = 6; i < 8; i++)
            {
                if (!Global.GameInstance.CanUseReadyItem(i))
                {
                    continue;
                }
                
                if (UIPopupReady.readyItemUseCount.Length > i && 
                    UIPopupReady.readyItemUseCount[i].Value > 0)
                {
                    var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                    {
                        L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                        L_IID = "InGameItem" + (i + 3),
                        L_CNT = UIPopupReady.readyItemUseCount[i].Value
                    };
                    itemList.Add(readyItem);
                }
            }
            
            if (GameManager.instance != null && Global.GameInstance.GetTurnCount_UseAD_AddTurn() > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.AD_ITEM_LOBBY.ToString(),
                    L_IID = "AD_ADD_TURN",
                    L_CNT = 1
                };
                itemList.Add(readyItem);
            }

            return JsonConvert.SerializeObject(itemList);
        }
        private static string GetUseAnimalData(GrowthyCustomLog_PLAYEND.Code_L_GMOD gmod)
        {
            //사용동물
            List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL> animalList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL>();

            if (gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE ||
                gmod == GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE_EVENT)
            {
                for (int i = 0; i < 3; i++)
                {
                    var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);

                    var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL
                    {
                        L_CID = animalData.idx,
                        L_LEV = animalData.level,
                        L_CNT = animalData.overlap
                    };
                    animalList.Add(readyItem);
                }
            }

            return JsonConvert.SerializeObject(animalList);
        }
    }

    #endregion
}