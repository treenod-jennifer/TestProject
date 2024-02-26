using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTitle : MonoBehaviour//MonoSingletonOnlyScene<SceneTitle>
{

    static public SceneTitle instance = null;

    // 여기 텍스처는 리소스 폴더에서 읽어서 출력,, 씬 벗어날때 읽은 텍스처 꼭 해제
    // 추가로 이벤트에 따라 텍스처를 로딩해서 타이틀씬 바뀌게도..
    // 
    public UITexture _textureTitle;
    public UITexture _textureBG;
    public UILabel _labelLoad;
    public UILabel _labelLoadS;
    public UILabel _versionText;
    public AudioClip _audioStart;
    public UIPokoButton _btnStart;
    public SceneTitleBtnContainer _btnContainer;

    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    static public bool _showStartButton = false; // 기본이 false로   .. ReBoot ()시 true로 만들어줌
    static public bool _isDirectAccessLogin = false;

    static public int resourceProgress = 0;

    /// <summary>
    /// 타이틀 씬에서 Global.reboot시 씬 전환하면서 이전 SceneTitle의 파괴(OnDestroy)가 신규 SceneTitle의 생성(Awake)이후에 됨. 그래서 reboot후에 insance가 null이 되어버리기 때문에 크래시가 발생함
    /// 그래서 Global.reboot이 SceneTitle을 파괴할경우 플래그를 주어 Ondestory에서 instance를 null하는 작업을 못하게함
    /// </summary>
    private bool _aleadyReleased = false;
    
    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        //_textureTitle.transform.localScale = Vector3.zero;
        _labelLoad.gameObject.SetActive(true);
        _labelLoadS.gameObject.SetActive(true);

        if ( Application.platform != RuntimePlatform.Android )
        {
            // Debug.Log(" 아이폰X 대응 " + (float)Screen.height / (float)Screen.width);
            if ( ( float ) Screen.height / ( float ) Screen.width > 2f || ( float ) Screen.width / ( float ) Screen.height > 2f )
            {
                for ( int i = 0; i < iphoneXWidgetY.Count; i++ )
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 70;
                }
            }
        }
    }

    public void DestroySeneTitle()
    {
        this._aleadyReleased = true;
        Destroy(this.gameObject);
        instance = null;
    }

    void OnDestroy()
    {
        if (this._aleadyReleased == false)
        { 
            instance = null; 
        }
    }

    public void SetLoginButton(SceneTitleBtnContainer.BtnContainerState buttonState, System.Action<bool> callbackHandler)
    {
        //버튼이 안나와야 할 경우
        if (buttonState == SceneTitleBtnContainer.BtnContainerState.None)
        {
            this._labelLoad.gameObject.SetActive(true);
            this._labelLoadS.gameObject.SetActive(true);
        }
        else
        {
            this._labelLoad.gameObject.SetActive(false);
            this._labelLoadS.gameObject.SetActive(false);
        }
        this._btnContainer.gameObject.SetActive(true);
        this._btnContainer.InitData(buttonState, callbackHandler);
    }

	// Use this for initialization
    IEnumerator Start()
    {
        bool useDefaultTitle = true;
        if( PlayerPrefs.HasKey("Title_Ver") )
        {
            int title_ver = PlayerPrefs.GetInt("Title_Ver");

            if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
            {
#if UNITY_EDITOR
                string path = "Assets/5_OutResource/title/TitlePrefab.prefab";
                GameObject org = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (org)
                {
                    var titleVerData = org.GetComponent<TitleData>();
                    if (titleVerData && titleVerData.version == title_ver)
                    {
                        GameObject go = Instantiate(org);
                        go.transform.parent = this.gameObject.transform;

                        go.transform.position = Vector3.zero;
                        go.transform.localScale = Vector3.one;
                        useDefaultTitle = false;
                    }
                }
#endif
            }
            else
            {
                var bundle = TitleBundle.GetLocalBundle(true);
                if (bundle)
                {
                    GameObject org = bundle.LoadAsset<GameObject>("TitlePrefab");
                    if (org)
                    {
                        var titleVerData = org.GetComponent<TitleData>();
                        if (titleVerData && titleVerData.version == title_ver)
                        {
                            GameObject go = Instantiate(org);
                            go.transform.parent = this.gameObject.transform;

                            go.transform.position = Vector3.zero;
                            go.transform.localScale = Vector3.one;
                            useDefaultTitle = false;
                        }
                    }
                }
            }
        }

        if (useDefaultTitle)
        {
            var go = Instantiate(Resources.Load<GameObject>("UIPrefab/DefaultTitle"));
            go.transform.parent = this.gameObject.transform;

            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }

        Debug.Log( "================= Manager Position 0 : " + ManagerUI._instance.gameObject.transform.position + " ================= " );
        {
            CameraEffect staticCamera = CameraEffect.MakeScreenEffect();
            staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.4f, true);
        }
        /*
        //앱 버전 표시.
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
        {
            _versionText.text = "v" + NetworkSettings.Instance.gameAppVersion;
            if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.DevServer)
                _versionText.text += " d";
            else if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LiveQAServer ||
                NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.Pub_QAServer)
                _versionText.text += " a";
        }
        else
        {
            _versionText.gameObject.SetActive(false);
        }
        */
        yield return new WaitForSeconds(0.2f);

        ManagerSound.AudioPlay(_audioStart);

        yield return new WaitForSeconds(0.5f);
        resourceProgress = 0;
        ManagerData._instance.StartNetwork();


        yield return new WaitForSeconds(0.5f);
        

        // 데이타 완전이 받을때까지 대기 
        while (true)    // 로딩중 Reboot시  ManagerData가 NULL이어서 처리
        {
            if (Global._pendingReboot) {
                yield break; // 
            }
           if (ManagerData._instance._state == DataLoadState.eComplete)
                break;
            yield return null;
        }

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Reset();
        watch.Start();


        string sceneName = "Lobby";
        if (Global.join) {
            sceneName = "Intro";
          //  Debug.Log("** GAME Global.join (SceneTitle) : " + ServerRepos.User.loginTs + "  " + Time.time);
        }
        yield return null;
        SceneLoading._plaeaseWait = true;
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        //Application.LoadLevelAsync("Lobby");
        float progress = 0f;
        while (!async.isDone)
        {
            progress = async.progress * 100.0f;
            if (progress >= 0.9f)
                break;
            yield return null;
        }

        //Debug.LogError("dddddddddddddddd 3");
        yield return new WaitForSeconds(0.1f);
        async.allowSceneActivation = true;
        if (!Global.join)
        {
            while (true)
            {
                if (SceneLoading._plaeaseWait == false)
                    break;
                yield return null;
            }
        }
       // Debug.LogError("dddddddddddddddd 2");

        watch.Stop();
        //Global.Log("Lobby " + (float)watch.ElapsedMilliseconds / 1000f);
        {
     // /      CameraEffect staticCamera = CameraEffect.MakeScreenEffect(1);
      //      staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0f), Color.black, 0.1f, false);
        }
        Destroy(gameObject);
        
 //       yield return new WaitForSeconds(0.3f);
        

     /*   if(ManagerUI._instance != null)
        {
            Debug.Log( "================= Manager Position 1 : " + ManagerUI._instance.gameObject.transform.position + " ================= " );
        }*/
    }
	
	// Update is called once per frame
	void Update () {

        string text = "LOADING";

        if (ManagerData._instance == null)
            return;

        if (ManagerData._instance._state == DataLoadState.eLineInitialize || ManagerData._instance._state == DataLoadState.eStart)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "ポコタと合流中";
            else
                text = "INITIALIZING";
        }
        else if (ManagerData._instance._state == DataLoadState.eLineLogin)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "まゆじの眉毛を整え中";
            else
                text = "LOGIN";
        }
        else if (ManagerData._instance._state == DataLoadState.eLineUserData)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "まゆじが花の帽子を探し中";
            else
                text = "LOADING USERINFO";
        }
        else if (ManagerData._instance._state == DataLoadState.eLineFriendsData || ManagerData._instance._state == DataLoadState.eLineInviteFriendsData || ManagerData._instance._state == DataLoadState.eLineRankingData || ManagerData._instance._state == DataLoadState.eLineProfileData)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "まゆじが小言をつぶやき中";
            else
                text = "LOADING FRIENDS";
            /*  }
              else if (ManagerData._instance._state == DataLoadState.eLineRankingData)
              {
                  text = "LOADING RANKINGS";*/
        }
        else if (ManagerData._instance._state == DataLoadState.eUserLogin || ManagerData._instance._state == DataLoadState.eGameData)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "ポコタ逃走中";
            else
                text = "LOADING GAMEINFO";
        }
        else if (ManagerData._instance._state == DataLoadState.eResourceData)
        {
            
            if (Global._systemLanguage == CurrLang.eJap)
                text = "ポコタ捜索中" + resourceProgress + "%";
            else
                text = "LOADING RESOURCE " + resourceProgress + "%";

        }
        else if (ManagerData._instance._state == DataLoadState.eComplete)
        {
            if (Global._systemLanguage == CurrLang.eJap)
                text = "ポコパンタウンに向かい中";
            else
                text = "MAKE WORLD";
        }


        if (ManagerData._instance._state != DataLoadState.eResourceData)
        {
            for (int i = 0; i < (int)Mathf.PingPong(Time.time * 7f, 4); i++)
                text += ".";
        }
        
        _labelLoad.text = text;
        _labelLoadS.text = text;
	}

}
