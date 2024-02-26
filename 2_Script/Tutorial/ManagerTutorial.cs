using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PokoAddressable;
using UnityEngine.AddressableAssets;

public enum TutorialType
{
    None = 0,
    TutorialLobbyMission,
    TutorialDiaryMission,
    TutorialQuestComplete,
    TutorialGetPlusHousing,
    TutorialGetMaterial,
    TutorialGetSticker,
    TutorialEventRanking,
    TutorialStageRanking,
    TutorialMoleCatch,
    TutorialCoinStageOutgame,
    TutorialCapsuleGacha,
    TutorialBingoEvent_EventOpen,
    TutorialBingoEvent_FirstLineComplete,
    TutorialTreasureHunt,
    TutorialAntiqueStoreEvent_EventOpen,
    TutorialLobbyRenewal,
    TutorialGroupRanking,

    TutorialGame2Match = 101,
    TutorialGameLineBomb,   //102
    TutorialGameCircleBomb, //103
    TutorialGameRainbow,    //104
    TutorialBombXBomb,      //105
    TutorialReadyItem,      //106
    TutorialLava,           //107
    TutorialInGameItem,     //108
    
    TutorialReadyScoreUp,   //109
    TutorialBlockBlack,     //110
    TutorialNet,            //111
    TurorialIceApple,       //112
    TutorialStone,          //113
    TutorialDynamite,       //114
    TutorialPowerHammer,    //115
    TutorialDecoIce,        //116
    TutorialFixDecoIce,     //117
    TutorialGrassFenceBlock,//118
    TutorialCarpet,         //119
    TutorialBigJewel,       //120
    TutorialFireWork,       //121
    TutorialGameRule_CoinStage,//122
    TutorialColorFlowerPot_Little, //123
    TutorialSodaJelly,      //124
    TutorialCountCrack,     //125
    TutorialPea,            //126
    TutorialBox,            //127
    TutorialPlant,          //128
    TutorialCrack,          //129
    TutorialKey,            //130
    TutorialCrossHammer,    //131
    TutorialJewel,          //132
    TutorialApple,          //133
    TutorialDig,            //134
    TutorialStatue,         //135
    TutorialReadyItem_Ingame,   //136
    TutorialReadyScoreUp_Ingame,//137
    TutorialFlowerInk,      //138
    TutorialSpaceShip,      //139
    TutorialBlockGenerator,      //140
    TutorialPeaBoss,       //141
    TutorialRandomBox,       //142
    TutorialFenceBlock,       //143
    TutorialHeart1,       //144
    TutorialHeart2,       //145
    
    //우선 사용
    TutorialBread,  //146
    TutorialWaterBomb,//147
    TutorialEMPTY_148,//148
    TutorialEMPTY_149,//149
    TutorialEMPTY_150,//150

    TutorialRainbowHammer, //151
    TutorialPaint, //152
    TutorialClover, //153
    TutorialCannon1, //154
    TutorialCannon2, //155
    
    TutorialReadyRandomBomb, //156

    TutorialGame2Match_Adventure = 201,
    TutorialUseSkill_Adventure,
    TutorialGameRule_Adventure,
    TutorialMakeBomb_Adventure,
    TutorialHealItem_Adventure,
    TutorialPotion_Adventure,
    TutorialSkillItem_Adventure,

    TutorialStart_Adventure,
    TutorialAttribute_Adventure,
    TutorialGacha_Adventure,
    TutorialChallenge_Adventure,

    TutorialColorBrush, //212
    
    TutorialTest,
    TutorialTest_CountCrack,
}

public enum BirdPositionType
{
    none,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public enum BirdAnimationType
{
    NONE,
    IDLE_LOOP,
    IDLE_DOWN,
    IDLE_UP,
    Clear_S,
    Clear_L,
}

public class ManagerTutorial : MonoBehaviour
{
    public static ManagerTutorial _instance = null;
    [System.NonSerialized]
    public bool _playing = false;

    [System.NonSerialized]
    public Transform _transform = null;
    public GameObject _objBlindPanel;
    public GameObject _objBlind;
    public GameObject _objTextbox;

    public UITexture _spriteFinger;

    [HideInInspector] public Texture _textureFingerNormal;
    [HideInInspector] public Texture _textureFingerPush;
    
    [SerializeField] private AssetReference _assetFingerNormal;
    [SerializeField] private AssetReference _assetFingerPush;
    
    [System.NonSerialized]
    public TutorialBase _current = null;
    [System.NonSerialized]
    public LAppModelProxy birdLive2D = null;

    public BirdPositionType birdType = BirdPositionType.none;

    private Vector3 topLeftOffset = new Vector3(150f, -230f, 0f);
    private Vector3 topRightOffset = new Vector3(-150f, -230f, 0f);
    private Vector3 bottomLeftOffset = new Vector3(150f, 230f, 0f);
    private Vector3 bottomRightOffset = new Vector3(-150f, 230f, 0f);

    //파랑새 사운드 자동으로 전환되도록 저장하는 변수.
    private bool isSoundChange = true;

    #region 튜토리얼 델리게이트
    //게임 오브젝트
    public delegate GameObject GetGameObjectDelegate();
    public delegate List<GameObject> GetListGameObjectDelegate();

    //인게임 블럭
    public delegate List<BlockBase> GetListBlockBaseDelegate();

    //인게임 데코
    public delegate List<DecoBase> GetListDecoBaseDelegate();

    //커스텀 블라인드 데이터
    public delegate List<CustomBlindData> GetCustomBlindDataDelegate();

    //bool형 값
    public delegate bool GetBoolDelegate();
    #endregion

    void Awake()
    {
        _instance = this;
        _transform = transform;
    }
	// Use this for initialization
	void Start ()
    {
        //텍스쳐 로드
        this.gameObject.AddressableAssetLoad<Texture>(_assetFingerNormal, (x) => _textureFingerNormal = x);
        this.gameObject.AddressableAssetLoad<Texture>(_assetFingerPush, (x) => _textureFingerPush = x);
        
        _spriteFinger.color = new Color(1f, 1f, 1f, 0f);
        //"나가기" 팝업이 있다면 해당 팝업 삭제.
        if (ManagerUI._instance.CheckExitUI() == true)
            ManagerUI._instance.ClosePopUpUI();
    }

    /// <summary>
    /// 튜토리얼 실행.
    /// </summary>
    /// <param name="in_type">튜토리얼 타입</param>
    /// <param name="anotherPrefab">같은 튜토리얼에서 확장 된 프리팹을 사용할 때</param>
    public static void PlayTutorial(TutorialType in_type, string anotherPrefab = "")
    {
        if (!Global._optionTutorialOn)
        {
            return;
        }
        
        if(_instance == null)
        {
            GameObject obj = Resources.Load("Tutorial/ManagerTutorial") as GameObject;
            Instantiate(obj, Vector3.zero, Quaternion.identity);
        }
        ManagerTutorial._instance._playing = true;

        GameObject objT = Resources.Load("Tutorial/" + in_type + anotherPrefab) as GameObject;
        if (objT == null)
            return;

        ManagerTutorial._instance._current = Instantiate(objT, Vector3.zero, Quaternion.identity).GetComponent<TutorialBase>();
        ManagerTutorial._instance._current._tutorialType = in_type;

        ManagerTutorial._instance._current._transform.parent = ManagerTutorial._instance._transform;
        ManagerTutorial._instance._current._transform.localPosition = Vector3.zero;
        ManagerTutorial._instance._current._transform.localScale = Vector3.one;

        string growthyName = "TUTORIAL" + ((int)in_type).ToString() + "_S";
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEventTutorial(growthyName);

        //튜토리얼 재생.
        ManagerTutorial._instance._current.PlayTutorial();
    }
    
    public BlindTutorial MakeBlind(Transform in_parent)
    {
        BlindTutorial obj = Instantiate(_objBlind).GetComponent<BlindTutorial>();
        obj._transform.parent = in_parent;
        obj._transform.localPosition = Vector3.zero;
        obj._transform.localScale = Vector3.one;

        return obj;
    }

    public BlindTutorial MakeBlindPanel(Transform in_parent)
    {
        BlindTutorial obj = Instantiate(_objBlindPanel).GetComponent<BlindTutorial>();
        obj._transform.parent = in_parent;
        obj._transform.localPosition = Vector3.zero;
        obj._transform.localScale = Vector3.one;
        return obj;
    }

    public TextboxTutorial MakeTextbox(BirdPositionType type, string in_message, float actionTime)
    {
        TextboxTutorial obj = Instantiate(_objTextbox).GetComponent<TextboxTutorial>();
        obj._transform.parent = _transform;
        obj.transform.localScale = Vector3.one;

        bool bFlip = false;
        if (type == BirdPositionType.TopLeft)
        {
            Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
            obj.transform.localPosition = new Vector3((pos.x + 262f), (pos.y - 230f), 0);
        }
        else if (type == BirdPositionType.TopRight)
        {
            Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
            obj.transform.localPosition = new Vector3((pos.x - 420f), (pos.y - 230f), 0);
            bFlip = true;
        }
        else if (type == BirdPositionType.BottomLeft)
        {
            Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
            obj.transform.localPosition = new Vector3((pos.x + 262f), (pos.y + 230f), 0);
        }
        else if (type == BirdPositionType.BottomRight)
        {
            Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
            obj.transform.localPosition = new Vector3((pos.x - 420f), (pos.y + 230f), 0);
            bFlip = true;
        }
        obj.InitTextBox(bFlip, in_message, actionTime);
      
        return obj;
    }

    public bool SettingBlueBird(BirdPositionType type, float actionTime)
    {
        if (birdType == type || type == BirdPositionType.none)
            return false;

        if (birdLive2D == null)
        {
            birdLive2D = LAppModelProxy.MakeLive2D(_transform.gameObject, TypeCharacterType.BlueBird);
            birdLive2D.transform.localScale = Vector3.one * 300f;
            birdLive2D.SetSortOrder(51);
        }
        Ease bluebirdEase = Ease.OutQuart;
        bool bTurn = false;

        #region 파랑새 현재 위치타입 : none 일 때 설정.
        if (birdType == BirdPositionType.none)
        {
            if (type == BirdPositionType.TopLeft)
            {
                Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
                birdLive2D.transform.localPosition = new Vector3((pos.x - 150f), (pos.y - 150f), 0);
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topLeftOffset.x), (pos.y + topLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
                birdLive2D.transform.localScale = new Vector3(birdLive2D.transform.localScale.x * -1, birdLive2D.transform.localScale.y, birdLive2D.transform.localScale.z);
            }
            else if (type == BirdPositionType.TopRight)
            {
                Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
                birdLive2D.transform.localPosition = new Vector3((pos.x + 150f), (pos.y - 150f), 0);
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topRightOffset.x), (pos.y + topRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomLeft)
            {
                Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
                birdLive2D.transform.localPosition = new Vector3((pos.x - 150f), (pos.y + 150f), 0);
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomLeftOffset.x), (pos.y + bottomLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
                birdLive2D.transform.localScale = new Vector3(birdLive2D.transform.localScale.x * -1, birdLive2D.transform.localScale.y, birdLive2D.transform.localScale.z);
            }
            else if (type == BirdPositionType.BottomRight)
            {
                Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
                birdLive2D.transform.localPosition = new Vector3((pos.x + 150f), (pos.y + 150f), 0);
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomRightOffset.x), (pos.y + bottomRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
        }
        #endregion 파랑새 현재 위치타입 : none 일 때 설정.

        #region 파랑새 현재 위치타입 : TopLeft 일 때 설정.
        else if (birdType == BirdPositionType.TopLeft)
        {
            if (type == BirdPositionType.TopRight)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topRightOffset.x), (pos.y + topRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomLeft)
            {
                Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomLeftOffset.x), (pos.y + bottomLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomRight)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomRightOffset.x), (pos.y + bottomRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
        }
        #endregion 파랑새 현재 위치타입 : TopLeft 일 때 설정

        #region 파랑새 현재 위치타입 : TopRight 일 때 설정.
        else if (birdType == BirdPositionType.TopRight)
        {
            if (type == BirdPositionType.TopLeft)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topLeftOffset.x), (pos.y + topLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomLeft)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomLeftOffset.x), (pos.y + bottomLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomRight)
            {
                Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomRightOffset.x), (pos.y + bottomRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
        }
        #endregion 파랑새 현재 위치타입 : TopRight 일 때 설정.

        #region 파랑새 현재 위치타입 : BottomLeft 일 때 설정.
        else if (birdType == BirdPositionType.BottomLeft)
        {
            if (type == BirdPositionType.TopLeft)
            {
                Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topLeftOffset.x), (pos.y + topLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.TopRight)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topRightOffset.x), (pos.y + topRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomRight)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomRightOffset.x), (pos.y + bottomRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
        }
        #endregion 파랑새 현재 위치타입 : BottomLeft 일 때 설정.

        #region 파랑새 현재 위치타입 : BottomLeft 일 때 설정.
        else if (birdType == BirdPositionType.BottomRight)
        {
            if (type == BirdPositionType.TopLeft)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topLeftOffset.x), (pos.y + topLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.TopRight)
            {  
                Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + topRightOffset.x), (pos.y + topRightOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
            else if (type == BirdPositionType.BottomLeft)
            {
                bTurn = true;
                Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
                birdLive2D.transform.DOLocalMove(new Vector3((pos.x + bottomLeftOffset.x), (pos.y + bottomLeftOffset.y), 0), actionTime).SetEase(bluebirdEase);
            }
        }
        #endregion 파랑새 현재 위치타입 : BottomLeft 일 때 설정.
        
        birdType = type;
        return bTurn;
    }

    public void BlueBirdTurn()
    {
        birdLive2D.transform.localScale = new Vector3(birdLive2D.transform.localScale.x * -1, birdLive2D.transform.localScale.y, birdLive2D.transform.localScale.z);
    }

    public IEnumerator OutBlueBird(float actionTime, bool bDeleteBird = true)
    {   
        if (birdType == BirdPositionType.TopLeft)
        {
            Vector3 pos = ManagerUI._instance.anchorTopLeft.transform.localPosition;
            birdLive2D.transform.DOLocalMove(new Vector3((pos.x - 150f), (pos.y - 150f), 0), actionTime);
        }
        else if (birdType == BirdPositionType.TopRight)
        {
            Vector3 pos = ManagerUI._instance.anchorTopRight.transform.localPosition;
            birdLive2D.transform.DOLocalMove(new Vector3((pos.x + 150f), (pos.y - 150f), 0), actionTime);
        }
        else if (birdType == BirdPositionType.BottomLeft)
        {
            Vector3 pos = ManagerUI._instance.anchorBottomLeft.transform.localPosition;
            birdLive2D.transform.DOLocalMove(new Vector3((pos.x - 150f), (pos.y + 150f), 0), actionTime);
        }
        else if (birdType == BirdPositionType.BottomRight)
        {
            Vector3 pos = ManagerUI._instance.anchorBottomRight.transform.localPosition;
            birdLive2D.transform.DOLocalMove(new Vector3((pos.x + 150f), (pos.y + 150f), 0), actionTime);
        }

        yield return new WaitForSeconds(actionTime);

        if (bDeleteBird == true)
        {
            birdLive2D = null;
        }
        else
        {
            birdType = BirdPositionType.none;
            BlueBirdTurn();
        }
    }

    public void SetBirdAnimation(string aniName, bool isLoop = false)
    {
        birdLive2D.SetAnimation(aniName, isLoop);
    }

    public void SetBirdAnimation(string aniName, string loopAniName)
    {
        birdLive2D.SetAnimation(aniName, loopAniName);
    }

    public void PlayBirdSound()
    {
        AudioLobby clip = (isSoundChange == true) ? AudioLobby.m_bird_hehe : AudioLobby.m_bird_aham;
        isSoundChange = !isSoundChange;
        ManagerSound.AudioPlay(clip);
    }
}
