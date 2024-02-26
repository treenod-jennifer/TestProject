using ServiceSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerCinemaBox : MonoBehaviour
{
    public static ManagerCinemaBox _instance = null;
    public UISprite _boxUp;
    public UISprite _boxDown;
    public GameObject _skipButton;
    public AnimationCurve _curve;

    public GameObject skippingRoot;
    public UITexture[] skippingTextures;

    public System.Action completeCinemaCallback = null;

    public static bool IsActive
    {
        get
        {
            if (_instance == null)
            {
                return false;
            }

            return _instance.isActive;
        }
    }

    private bool isActive = false;
    readonly float _maxSize = 430f;
    float size = 0f;
    bool _soundPlay = true;
    [System.NonSerialized]
    string sceneIdentifier;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            Init();

            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }
    }

    private void SceneManager_sceneUnloaded(Scene arg0)
    {
        if (arg0.name == "Lobby")
        {
            _instance?.Init();
        }
    }

    private void Init()
    {
        skippableScene = true;
        skipPressed = false;
        skipEnded = false;
        _boxUp.gameObject.SetActive(false);
        _boxDown.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if( _instance == this )
        {
            _instance = null;
            SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
        }
            
    }
    // Use this for initialization
    void Start () {
        _boxUp.gameObject.SetActive(false);
        _boxDown.gameObject.SetActive(false);

        _boxUp.color = Color.black;
        _boxDown.color = Color.black;
	}

    public void OnBox(string sceneId, float in_speed = 1f, bool in_sound = true, bool skippableScene = true)
    {
        this.sceneIdentifier = sceneId;
        _soundPlay = in_sound;
        this.skippableScene = skippableScene;

        var achieve = new GrowthyCustomLog_Achievement 
            (
            tag: GrowthyCustomLog_Achievement.Code_L_TAG.SCENE,
            cat: GrowthyCustomLog_Achievement.Code_L_CAT.SCENE_START,
            anm: sceneIdentifier,
            arlt: GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

        StartCoroutine(CoOnBox(in_speed));
    }

    public IEnumerator OffBox(float in_speed)
    {
        yield return CoOffBox(in_speed);
    }

    IEnumerator CoOnBox(float in_speed)
    {
        skipPressed = false;
        skipEnded = false;
        isActive = true;

        float timer = 0f;
        _boxUp.gameObject.SetActive(true);
        _boxDown.gameObject.SetActive(true);
        _skipButton.SetActive(false);

        if (_soundPlay)
            ManagerSound.AudioPlay(AudioLobby.LetterBox);
     
        while (true)
        {
            if (!_boxUp.gameObject.active)
            {
                _boxUp.gameObject.SetActive(true);
                _boxDown.gameObject.SetActive(true);
            }

            size = _curve.Evaluate(timer) * _maxSize;

            _boxUp.SetDimensions(1000, (int)size);
            _boxDown.SetDimensions(1000, (int)size);

            timer += Global.deltaTimeLobby * in_speed;
            if (timer > 1f)
            {
                _boxUp.SetDimensions(1000, (int)_maxSize);
                _boxDown.SetDimensions(1000, (int)_maxSize);
                break;
            }
            yield return null;
        }
        
        if ( skippableScene )
            _skipButton.SetActive(true);
    }

    IEnumerator CoOffBox(float in_speed)
    {
        float timer = 0f;
        _boxUp.gameObject.SetActive(true);
        _boxDown.gameObject.SetActive(true);
        _skipButton.SetActive(false);
        isActive = false;

        while (true)
        {

            size = _curve.Evaluate(timer) * _maxSize;

            _boxUp.SetDimensions(1000, (int)(_maxSize - size));
            _boxDown.SetDimensions(1000, (int)(_maxSize - size));

            timer += Global.deltaTimeLobby * in_speed;
            if (timer > 1f)
            {
                _boxUp.SetDimensions(1000, 0);
                _boxDown.SetDimensions(1000, 0);
                break;
            }
            yield return null;
        }
        
        //레터박스 완전히 닫힌 후, 스킵 UI 사라지도록 수정.
        if (skipPressed)
        {
            skipPressed = false;

            while (skipEnded == false)
            {
                yield return new WaitForSeconds(0.1f);
            }
            skipEnded = false;
        }

        if (completeCinemaCallback != null)
        {
            completeCinemaCallback();
            completeCinemaCallback = null;
        }
        _boxUp.gameObject.SetActive(false);
        _boxDown.gameObject.SetActive(false);
    }

    public bool skippableScene = true;
    bool skipPressed = false;
    bool skipEnded = false;
    Coroutine skipProc = null;

    public void OnClickSkip()
    {
        if (skipPressed == true)
            return;
        skipPressed = true;
        _skipButton.SetActive(false);

        var achieve = new GrowthyCustomLog_Achievement
            (
            tag: GrowthyCustomLog_Achievement.Code_L_TAG.SCENE,
            cat: GrowthyCustomLog_Achievement.Code_L_CAT.SCENE_SKIP,
            anm: sceneIdentifier,
            arlt: GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

        StartCoroutine(CoSkipProc());
    }
    
    IEnumerator CoSkipProc()
    {
        ManagerSound._instance.Mute();

        skippingRoot.SetActive(true);

        Global.timeScaleLobby = 100f;
        Global.timeScaleLobbySpine = 100f;

        float frm = 0f;
        while (skipPressed)
        {
            if (UIChat._instance != null)
            {
                UIChat._instance.Skip();
            }

            for(int i = 0; i < skippingTextures.Length; ++i)
            {
                bool on = i < frm;

                if (skippingTextures[i].gameObject.activeSelf != on)
                    skippingTextures[i].gameObject.SetActive(on);
            }

            frm += 0.8f;
            if ((int)frm > skippingTextures.Length + 1)
                frm = 0f;

            yield return new WaitForSeconds(0.1f);
        }

        Global.timeScaleLobby = 1f;
        Global.timeScaleLobbySpine = 1f;

        skippingRoot.SetActive(false);

        ManagerSound._instance.UnMute();
        skipEnded = true;
        yield break;
    }

    public void SkipEmergencyStop()
    {
        skipPressed = false;
        skippableScene = false;
        _skipButton.SetActive(false);
    }
}
