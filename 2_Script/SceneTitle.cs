using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTitle : MonoBehaviour//MonoSingletonOnlyScene<SceneTitle>
{
    static public SceneTitle instance = null;

    // 여기 텍스처는 리소스 폴더에서 읽어서 출력,, 씬 벗어날때 읽은 텍스처 꼭 해제
    // 추가로 이벤트에 따라 텍스처를 로딩해서 타이틀씬 바뀌게도..

    public UILabel _labelLoad;
    public UILabel _labelLoadS;
    public UILabel _versionText;
    public UITexture _copyrightTexture;
    public UILabel _downloadLengthText;
    public AudioClip _audioStart;
    public AudioClip _audioLoop;
    public AudioSource _bgmAudioSource;
    public UIPokoButton _btnStart;
    public SceneTitleBtnContainer _btnContainer;

    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();
    public UIProgressBar _loadingProgress;
    public UILabel _loadingProgressLabel;

    static public bool _showStartButton = false; // 기본이 false로   .. ReBoot ()시 true로 만들어줌
    static public bool _isDirectAccessLogin = false;

    static public int resourceProgress = 0;
    static public long resourceDownloaded = 0;
    static public long resourceTotal = 0;

    [NonSerialized] float loadingProgress = 0.0f;
    [NonSerialized] float completeTimeElapsed = 0.0f;

    /// <summary>
    /// 타이틀 씬에서 Global.reboot시 씬 전환하면서 이전 SceneTitle의 파괴(OnDestroy)가 신규 SceneTitle의 생성(Awake)이후에 됨. 그래서 reboot후에 insance가 null이 되어버리기 때문에 크래시가 발생함
    /// 그래서 Global.reboot이 SceneTitle을 파괴할경우 플래그를 주어 Ondestory에서 instance를 null하는 작업을 못하게함
    /// </summary>
    private bool _aleadyReleased = false;

    private bool LobbyMakeWaitFlag = true;

    void Awake()
    {
        if(instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

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
    
    public void LobbyMakeWaitRelease()
    {
        LobbyMakeWaitFlag = false;
    }

    public void DestroySceneTitle()
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
            _labelLoad.gameObject.SetActive(true);
            _labelLoadS.gameObject.SetActive(true);
            _loadingProgress.gameObject.SetActive(true);
            _loadingProgressLabel.gameObject.SetActive(true);
        }
        else
        {
            _labelLoad.gameObject.SetActive(false);
            _labelLoadS.gameObject.SetActive(false);
            _loadingProgress.gameObject.SetActive(false);
            _loadingProgressLabel.gameObject.SetActive(false);
        }
        _btnContainer.gameObject.SetActive(true);
        _btnContainer.InitData(buttonState, callbackHandler);
    }

	// Use this for initialization
    IEnumerator Start()
    {
        if (LanguageUtility.SystemLanguage == SystemLanguage.Japanese)
        {
            int lastGuideTime = 0;
            float appearTime = 2;
            
            //기기 날짜를 월로 변경
            var now = DateTime.Now;
            int curGuideTime = now.Year * 100 + now.Month;

            if (PlayerPrefs.HasKey("LastProtectGuideMonth"))
                lastGuideTime = PlayerPrefs.GetInt("LastProtectGuideMonth");
            
            // 시간 만료 or 시간 미등록 
            if (!lastGuideTime.Equals(curGuideTime) || lastGuideTime == 0)
            {
                PlayerPrefs.SetInt("LastProtectGuideMonth", curGuideTime);
                
                GameObject loadObj = Resources.Load<GameObject>("UIPrefab/MinorProtectGuide");
                
                if (loadObj)
                {
                    GameObject obj = Instantiate(loadObj, this.gameObject.transform, true);
                    
                    obj.transform.position = Vector3.zero;
                    obj.transform.localScale = Vector3.one;

                    yield return new WaitForSecondsRealtime(appearTime);
                    Destroy(obj);
                }
            }
        }
        
        ManagerData.IsMinorPrefabLoading = true;
        loadingProgress = 0f;
        bool useDefaultTitle = true;
        float audioDelay = 0f;
        AudioClip audio = _audioStart;
        AudioClip bgm = _audioLoop;


        if (PlayerPrefs.HasKey("Title_Ver"))
        {
            int title_ver = PlayerPrefs.GetInt("Title_Ver");

            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path = $"Assets/5_OutResource/titles/title_v2_{title_ver}_{LanguageUtility.SystemCountryCodeForAssetBundle}/TitlePrefab.prefab";
                GameObject org = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (org)
                {
                    var titleVerData = org.GetComponent<TitleData>();
                    if (titleVerData && titleVerData.IsExpired() == false)
                    {
                        GameObject go = Instantiate(org, this.gameObject.transform, true);

                        go.transform.position = Vector3.zero;
                        go.transform.localScale = Vector3.one;
                        useDefaultTitle = false;

                        audioDelay = titleVerData.audioDelay;
                        if (titleVerData.audioStart != null)
                        {
                            audio = titleVerData.audioStart;
                        }
                        if (titleVerData.audioLoop != null)
                            bgm = titleVerData.audioLoop;

                        if(this._copyrightTexture != null)
                        {
                            if (titleVerData.copyright_Texture != null)
                            {
                                this._copyrightTexture.gameObject.SetActive(true);
                                this._copyrightTexture.mainTexture = titleVerData.copyright_Texture;
                                this._copyrightTexture.MakePixelPerfect();
                            }
                        }
                    }
                }
#endif
            }
            else
            {
                // 로컬에 캐싱되어있는 번들이 있는지 확인 후 해당 번들로 타이틀 출력.
                var bundle = TitleBundle.GetLocalBundle(title_ver, true);
                if (bundle)
                {
                    GameObject org = bundle.LoadAsset<GameObject>("TitlePrefab");
                    if (org)
                    {
                        var titleVerData = org.GetComponent<TitleData>();
                        if (titleVerData && titleVerData.IsExpired() == false)
                        {
                            GameObject go = Instantiate(org, this.gameObject.transform, true);

                            go.transform.position = Vector3.zero;
                            go.transform.localScale = Vector3.one;
                            useDefaultTitle = false;

                            audioDelay = titleVerData.audioDelay;
                            if (titleVerData.audioStart != null)
                            {
                                audio = titleVerData.audioStart;
                            }

                            if (titleVerData.audioLoop != null)
                                bgm = titleVerData.audioLoop;

                            if (this._copyrightTexture != null)
                            {
                                if (titleVerData.copyright_Texture != null)
                                {
                                    this._copyrightTexture.gameObject.SetActive(true);
                                    this._copyrightTexture.mainTexture = titleVerData.copyright_Texture;
                                    this._copyrightTexture.MakePixelPerfect();
                                }
                            }
                        }
                    }
                }
            }
        }

        // 캐싱된 번들이 없다면 기본 타이틀 출력.
        if (useDefaultTitle)
        {
            var go = Instantiate(Resources.Load<GameObject>("UIPrefab/DefaultTitle"), this.gameObject.transform, true);

            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;

            audioDelay = 0.2f;
        }

        Debug.Log( "================= Manager Position 0 : " + ManagerUI._instance.gameObject.transform.position + " ================= " );
        {
            CameraEffect staticCamera = CameraEffect.MakeScreenEffect();
            staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.4f, true);
        }

        //앱 버전 표시.
        if (NetworkSettings.Instance.buildPhase != NetworkSettings.eBuildPhases.RELEASE)
        {
            _versionText.text = "v" + NetworkSettings.Instance.gameAppVersion;
            if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.DevServer)
                _versionText.text += " d";
            else if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LevelTeamServer)
                _versionText.text += " L";
            else if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.LiveQAServer ||
                NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.Pub_QAServer)
                _versionText.text += " a";
            else if (NetworkSettings.Instance.serverTarget == NetworkSettings.ServerTargets.CustomDevServer)
                _versionText.text += " c";

            if( NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                Global._instance.ShowIngameInfo = true;
            }
        }
        else
        {
            _versionText.gameObject.SetActive(false);
        }

        StartCoroutine(this.AudioPlay(audioDelay, audio));
        if (Global._optionBGM )
        {
            _bgmAudioSource = gameObject.AddMissingComponent<AudioSource>();

            _bgmAudioSource.clip = bgm;
            _bgmAudioSource.loop = true;
            _bgmAudioSource.Play();
        }

        yield return new WaitForSeconds(0.7f);
        resourceProgress = 0;
        ManagerData._instance.StartNetwork();


        yield return new WaitForSeconds(0.5f);
        

        // 데이타 완전이 받을때까지 대기 
        while (true)    // 로딩중 Reboot시  ManagerData가 NULL이어서 처리
        {
            if (Global._pendingReboot)
            {
                yield break; // 
            }

            if (ManagerData._instance._state == DataLoadState.eComplete)
                break;
            yield return null;
        }

        string sceneName = "Lobby";
        if (Global.join) {
            sceneName = "Intro";
            Debug.Log("** GAME Global.join (SceneTitle) : " + ServerRepos.User.loginTs + "  " + Time.time);
        }
        yield return null;
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

        yield return new WaitForSeconds(0.1f);
        async.allowSceneActivation = true;
        if (!Global.join)
        {
            yield return new WaitWhile (() => LobbyMakeWaitFlag);
        }
        
        Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update () {

        string text = "LOADING";

        if (ManagerData._instance == null)
            return;

        bool showDownloadText = false;

        if (ManagerData._instance._state == DataLoadState.eLineInitialize || ManagerData._instance._state == DataLoadState.eStart)
        {
            //INITIALIZING
            text = Global._instance.GetString("title_ld_1"); 
            
        }
        else if (ManagerData._instance._state == DataLoadState.eLineLogin)
        {
            //LOGIN
            text = Global._instance.GetString("title_ld_2");
            loadingProgress = 0.05f;
        }
        else if (ManagerData._instance._state == DataLoadState.eLineUserData)
        {
            //LOADING USERINFO
            text = Global._instance.GetString("title_ld_3");
            loadingProgress = 0.10f;
        }
        else if (ManagerData._instance._state == DataLoadState.eLineFriendsData || ManagerData._instance._state == DataLoadState.eLineInviteFriendsData || ManagerData._instance._state == DataLoadState.eLineRankingData || ManagerData._instance._state == DataLoadState.eLineProfileData)
        {
            //LOADING FRIENDS
            text = Global._instance.GetString("title_ld_4");
            loadingProgress = 0.2f;
        }
        else if (ManagerData._instance._state == DataLoadState.eUserLogin || ManagerData._instance._state == DataLoadState.eGameData)
        {
            //LOADING GAMEINFO
            text = Global._instance.GetString("title_ld_5");
            loadingProgress = 0.3f;
        }
        else if (ManagerData._instance._state == DataLoadState.eResourceData)
        {
            showDownloadText = SceneTitle.resourceTotal > 0;
            if( showDownloadText)
            {
                _downloadLengthText.text = string.Format("({0:n0} / {1:n0}Kbytes)", SceneTitle.resourceDownloaded / 1024, SceneTitle.resourceTotal / 1024);
                loadingProgress = 0.3f + ((float)SceneTitle.resourceDownloaded / (float)SceneTitle.resourceTotal) * 0.4f;
            }
            else
            {
                loadingProgress = 0.3f;
            }

            //LOADING RESOURCE
            text = Global._instance.GetString("title_ld_6");
        }
        else if (ManagerData._instance._state == DataLoadState.eComplete)
        {
            //MAKE WORLD
            text = Global._instance.GetString("title_ld_7");
            completeTimeElapsed += Time.deltaTime;
            completeTimeElapsed = completeTimeElapsed > 5.0f ? 5.0f : completeTimeElapsed;
            loadingProgress = 0.7f + 0.3f * (completeTimeElapsed / 5.0f);
        }

        if (this._loadingProgress.value != loadingProgress)
        {
            _loadingProgress.Set(loadingProgress);
            _loadingProgressLabel.text = $"{(100*loadingProgress):F1}%";
        }


        for (int i = 0; i < (int)Mathf.PingPong(Time.time * 7f, 4); i++)
            text += ".";

        if (_downloadLengthText.gameObject.activeInHierarchy != showDownloadText)
            _downloadLengthText.gameObject.SetActive(showDownloadText);


        _labelLoad.text = text;
        _labelLoadS.text = text;
	}

    private IEnumerator AudioPlay(float audioDelay, AudioClip audioStart)
    {
        yield return new WaitForSeconds(audioDelay);
        ManagerSound.AudioPlay(audioStart);
    }
}
