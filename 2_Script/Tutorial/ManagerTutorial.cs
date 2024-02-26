using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TutorialType
{
    None = 0,
    TutorialLobbyMission,
    TutorialDiaryMission,
    TutorialQuestComplete,
    TutorialGetPlusHousing,
    TutorialGetMaterial,
    TutorialGetSticker,

    TutorialGame2Match = 101,
    TutorialGameLineBomb,   //101
    TutorialGameCircleBomb, //102
    TutorialGameRainbow,    //103
    TutorialBombXBomb,      //104
    TutorialReadyItem,      //105
    TutorialLava,           //106
    TutorialInGameItem,     //107
    
    TutorialReadyScoreUp,   //108
    TutorialBlockBlack,     //109
}

public enum BirdPositionType
{
    none,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}


public class ManagerTutorial : MonoBehaviour {

    public static ManagerTutorial _instance = null;
    [System.NonSerialized]
    public bool _playing = false;

    [System.NonSerialized]
    public Transform _transform = null;
    public GameObject _objBlindPanel;
    public GameObject _objBlind;
    public GameObject _objTextbox;

    public UITexture _spriteFinger;
    public Texture _textureFingerNormal;
    public Texture _textureFingerPush;

    [System.NonSerialized]
    public TutorialBase _current = null;
    [System.NonSerialized]
    public LAppModelProxy birdLive2D = null;

    private BirdPositionType birdType = BirdPositionType.none;

    private Vector3 topLeftOffset = new Vector3(150f, -230f, 0f);
    private Vector3 topRightOffset = new Vector3(-150f, -230f, 0f);
    private Vector3 bottomLeftOffset = new Vector3(150f, 230f, 0f);
    private Vector3 bottomRightOffset = new Vector3(-150f, 230f, 0f);

    void Awake()
    {
        _instance = this;
        _transform = transform;
    }
	// Use this for initialization
	void Start ()
    {
        _spriteFinger.color = new Color(1f, 1f, 1f, 0f);
        //"나가기" 팝업이 있다면 해당 팝업 삭제.
        if (ManagerUI._instance.CheckExitUI() == true)
            ManagerUI._instance.ClosePopUpUI();
    }

    public static void PlayTutorial(TutorialType in_type)
    {
        if(_instance == null)
        {
            GameObject obj = Resources.Load("Tutorial/ManagerTutorial") as GameObject;
            Instantiate(obj, Vector3.zero, Quaternion.identity);
        }
        ManagerTutorial._instance._playing = true;
        //ManagerLobby._instance._state = TypeLobbyState.Tutorial;

        GameObject objT = Resources.Load("Tutorial/" + in_type) as GameObject;
        ManagerTutorial._instance._current = Instantiate(objT, Vector3.zero, Quaternion.identity).GetComponent<TutorialBase>();
        ManagerTutorial._instance._current._tutorialType = in_type;

        ManagerTutorial._instance._current._transform.parent = ManagerTutorial._instance._transform;
        ManagerTutorial._instance._current._transform.localPosition = Vector3.zero;
        ManagerTutorial._instance._current._transform.localScale = Vector3.one;


        string growthyName = "TUTORIAL" + ((int)in_type).ToString() + "_S";
        //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEventTutorial(growthyName);
        Debug.Log(growthyName);
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
            obj.transform.localPosition = new Vector3((pos.x + 270f), (pos.y - 230f), 0);
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
            obj.transform.localPosition = new Vector3((pos.x + 270f), (pos.y + 230f), 0);
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
            birdLive2D = Instantiate(ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.BlueBird].obj).GetComponent<LAppModelProxy>();
            birdLive2D.transform.parent = _transform;
            birdLive2D.transform.localScale = Vector3.one * 300f;
            birdLive2D._CubismRender.SortingOrder = 51;
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

    public IEnumerator OutBlueBird(float actionTime)
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
        birdLive2D = null;
    }

    public void SetBirdAnimation(string aniName)
    {
        birdLive2D.setAnimation(false, aniName);
    }
}
