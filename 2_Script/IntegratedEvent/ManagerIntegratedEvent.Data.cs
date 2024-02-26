using System.Linq;

public class IntegratedEventInfo
{
    public bool   enable;
    public bool   isNew;
    public string icon;
    public long   endTs;
    
    private ManagerIntegratedEvent.IntegratedEventType type;

    public IntegratedEventInfo(Protocol.IntegratedBannerInfo cdn)
    {
        type   = ManagerIntegratedEvent.GetEventType(cdn.etype);
        icon   = cdn.name;
        endTs  = cdn.endts;
        enable = ManagerIntegratedEvent.CreatedLobbyIcon(type) && IsEndTimeOver(cdn.endts) == false;
        isNew  = cdn.isNew;
    }

    public void OpenEventPopup()
    {
        ManagerIntegratedEvent.OpenEventPopup(type);
    }

    private bool IsEndTimeOver(long endTs)
    {
        // -1일 경우 배너 표기(엔드 컨텐츠)
        return (endTs > -1) && (Global.LeftTime(endTs) <= 0);
    }
    
    public ManagerIntegratedEvent.IntegratedEventType GetIntegratedEventType() => type;
}

/// <summary>
/// 새로운 이벤트가 추가 될 경우 해당 이벤트 관련 처리 추가.
/// IntegratedEventType 추가, OpenEventPopup 추가
/// </summary>
public partial class ManagerIntegratedEvent
{
    // 이벤트 타입 추가 (서버에서 내려주는 이벤트 타입, 순서 변경하면 안됨, 새로 추가되는 이벤트는 마지막에)
    public enum IntegratedEventType
    {
        NONE = -1,
        EVENT_STAGE,                //이벤트 스테이지
        SPECIAL_EVENT,              //블록 모으기
        NOY_BOOST,                  //노이의 마법 페스티벌 (부스팅 이벤트)
        WORLD_RANKING,              //월드랭킹
        POKO_FLOWER,                //에코피 이벤트
        VENDER,                     //사용하지 않는 이벤트
        COIN_BONUS,                 //코인 스테이지
        ADVENTURE_EVENT,            //탐험모드 이벤트 스테이지
        ALPHABET,                   //알파벳 모으기
        CAPSULE_GACHA,              //캡슐토이
        TURN_RELAY,                 //금붕어잡기
        STAGE_ASSIST_MISSION_EVENT, //릴레이 미션
        LOGIN_AD_BONUS_NRU,         //NRU 로그인 보너스
        LOGIN_AD_BONUS_CBU,         //CBU 로그인 보너스
        DECO_COLLECTION,            //데코 컬렉션
        TREASURE_HUNT,              //포코타와 보물찾기
        BINGO_EVENT,                //스탬프 빙고
        END_CONTENTS,               //포코타의 비밀의 섬
        EVENT_QUEST,                //이벤트 퀘스트
        STAGE_CHALLENGE,            //마유지의 도전장
        ANTIQUE_STORE,              //앤티크 공방
        CRIMINAL_EVENT,             //코코의 수사일지
        MOLE_CATCH_EVENT,           //두더지 잡기
        GROUP_RANKING,              //그룹랭킹
        ATELIER,                    //퍼즐 명화
        SPACE_TRAVEL,               //포코타의 우주여행
    }

    public static IntegratedEventType GetEventType(int type)
    {
        return (IntegratedEventType)type;
    }

    /// <summary>
    /// 이벤트 아이콘 생성 여부
    /// </summary>
    public static bool CreatedLobbyIcon(IntegratedEventType type)
    {
        return ManagerUI._instance.ScrollbarRight.icons.Any(icon => icon.GetIntegratedEventType() == type);
    }

    /// <summary>
    /// 배너 클릭 시 동작하는 함수 추가 (IEventBase.OnEventIconClick 함수 연결)
    /// </summary>
    public static void OpenEventPopup(IntegratedEventType type)
    {
        switch (type)
        {
            case IntegratedEventType.EVENT_STAGE:
                if (ManagerEventStage.instance != null)
                {
                    ManagerEventStage.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.SPECIAL_EVENT:
                if (ManagerSpecialEvent.instance != null)
                {
                    ManagerSpecialEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.WORLD_RANKING:
                if (ManagerWorldRanking.instance != null)
                {
                    ManagerWorldRanking.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.POKO_FLOWER:
                if (ManagerPokoFlowerEvent.instance != null)
                {
                    ManagerPokoFlowerEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.COIN_BONUS:
                if (ManagerCoinBonusStage.instance != null)
                {
                    ManagerCoinBonusStage.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.ADVENTURE_EVENT:
                if (ManagerAdventureEvent.instance != null)
                {
                    ManagerAdventureEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.ALPHABET:
                if (ManagerAlphabetEvent.instance != null)
                {
                    ManagerAlphabetEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.CAPSULE_GACHA:
                if (ManagerCapsuleGachaEvent.Instance != null)
                {
                    ManagerCapsuleGachaEvent.Instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.TURN_RELAY:
                if (ManagerTurnRelay.instance != null)
                {
                    ManagerTurnRelay.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.STAGE_ASSIST_MISSION_EVENT:
                if (ManagerStageAssistMissionEvent.Instance != null)
                {
                    ManagerStageAssistMissionEvent.Instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.LOGIN_AD_BONUS_NRU:
                if (ManagerLoginADBonus.Instance != null)
                {
                    ManagerLoginADBonus.Instance.OnEventIconClick(1);
                }

                break;
            case IntegratedEventType.LOGIN_AD_BONUS_CBU:
                if (ManagerLoginADBonus.Instance != null)
                {
                    ManagerLoginADBonus.Instance.OnEventIconClick(2);
                }

                break;
            case IntegratedEventType.DECO_COLLECTION:
                if (ManagerDecoCollectionEvent.instance != null)
                {
                    ManagerDecoCollectionEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.TREASURE_HUNT:
                if (ManagerTreasureHunt.instance != null)
                {
                    //튜토리얼에서 로비 특정 구역으로 이동하는 부분이 있음. 튜토리얼 출력이 가능할 경우 이벤트 통합 팝업 닫히도록 추가.
                    if (Global._optionTutorialOn && ManagerTreasureHunt.instance.IsShowTutorial() &&
                        UIPopupIntegratedEvent._instance != null)
                    {
                        ManagerUI._instance.ClosePopUpUI(UIPopupIntegratedEvent._instance);
                    }

                    ManagerTreasureHunt.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.BINGO_EVENT:
                if (ManagerBingoEvent.instance != null)
                {
                    ManagerBingoEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.END_CONTENTS:
                if (ManagerEndContentsEvent.instance != null)
                {
                    ManagerEndContentsEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.EVENT_QUEST:
                if (ManagerEventQuest.instance != null)
                {
                    ManagerEventQuest.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.STAGE_CHALLENGE:
                if (ManagerStageChallenge.instance != null)
                {
                    ManagerStageChallenge.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.ANTIQUE_STORE:
                if (ManagerAntiqueStore.instance != null)
                {
                    ManagerAntiqueStore.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.CRIMINAL_EVENT:
                if (ManagerCriminalEvent.instance != null)
                {
                    ManagerCriminalEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.MOLE_CATCH_EVENT:
                if (ManagerMoleCatch.instance != null)
                {
                    ManagerMoleCatch.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.NOY_BOOST:
                if (ManagerNoyBoostEvent.instance != null)
                {
                    ManagerNoyBoostEvent.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.GROUP_RANKING:
                if (ManagerGroupRanking.instance != null && ManagerGroupRanking.isEventOn)
                {
                    ManagerGroupRanking.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.SPACE_TRAVEL:
                if (ManagerSpaceTravel.instance != null)
                {
                    ManagerSpaceTravel.instance.OnEventIconClick();
                }

                break;
            case IntegratedEventType.ATELIER:
                if (ManagerAtelier.instance != null)
                {
                    ManagerAtelier.instance.OnEventIconClick();
                }

                break;
        }
    }
}