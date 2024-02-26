using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SceneIntro : MonoBehaviour
{
	public static SceneIntro _instance = null;
	// 여기 텍스처는 리소스 폴더에서 읽어서 출력,, 씬 벗어날때 읽은 텍스처 꼭 해제
	// 추가로 이벤트에 따라 텍스처를 로딩해서 타이틀씬 바뀌게도..
	// 
	public Camera camera;
    public string defaultMovieURL = "https://s3.ap-northeast-2.amazonaws.com/pokopuzzle/movie/intro/intro_low.mp4";
	public string defaultCDNAddr = "https://s3.ap-northeast-2.amazonaws.com/pokopuzzle";
	public string defaultMoviePath = "/movie/intro/intro_mid.mp4";

    public GameObject _skipButton;
    public UILabel _loadText;
     
	private const int MaxSkipCount = 5;
	private VideoPlayer mPlayer = null;
	private int mSkipCount = 0;
	private bool mPreparedMovieURL = false;
	
	private AudioSource mAudioSource;
	// Use this for initialization

    public ProcAnimIntroSubTitle procAnimIntro;

    public List<UIWidget> iphoneXWidget = new List<UIWidget>();

	void PrepareWithDefaultURL() {
		Debug.Log("[movie] Play Intro Movie wuth default URL " + defaultMoviePath );
		DoPrepareMoviePlayerUrl(defaultMoviePath);
	}

	void DoPrepareMoviePlayerUrl(string subUrl) {
		mPlayer.url = MakeMovieURL(subUrl);
		mPlayer.Prepare();
		mPreparedMovieURL = true;
	}

	string MakeMovieURL(string subUrl) {
		var cdnUrl = NetworkSettings.Instance.GetCDN_URL();	//Trident.TridentSDK.getInstance().getCdnServerUrl();
		 
		if (string.IsNullOrEmpty(cdnUrl)) {
			cdnUrl = defaultCDNAddr;
		}
		return cdnUrl + subUrl;	
	}

 

	void PrepareMovie() {
        PrepareWithDefaultURL();
    }

	void Awake()
	{
		_instance = this;
		_skipButton.SetActive(false);
	}
	
	IEnumerator Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

		if (!Trident.TridentSDK.getInstance().isInitialized()) {
			Debug.LogWarning("[movie] TridentSDK Not isInitialized ");
		}

		mPlayer = gameObject.AddComponent<VideoPlayer>();
		mPlayer.playOnAwake = false;
		mPlayer.renderMode = VideoRenderMode.CameraFarPlane;
		mPlayer.targetCamera = camera;
		mPlayer.waitForFirstFrame = true;
		mAudioSource = gameObject.AddComponent<AudioSource>();
		mAudioSource.playOnAwake = false;
		mAudioSource.Pause();
	 	
		mPlayer.source = VideoSource.Url;
		mPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		
		//Assign the Audio from Video to AudioSource to be played
		mPlayer.EnableAudioTrack(1, true);
		mPlayer.SetTargetAudioSource(1, mAudioSource);
		 
		PrepareMovie();
		 
		while (!mPlayer.isPrepared)
		{
			//Prepare/Wait for 5 sceonds only
			yield return new WaitForSeconds(1);
			//Break out of the while loop after 5 seconds wait
			break;
		}


		mPlayer.Play();
		mAudioSource.Play();
        this.procAnimIntro.InitData(mPlayer);
	 
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.INTRO_S);
        Extension.PokoLog.Log( "==========growthy InsertGrowthySequentialEvent(enable)2 " );

        yield return null;
        _loadText.gameObject.SetActive(false);
		yield return new WaitForSeconds(6f);
		_skipButton.SetActive(true);

        if ( Application.platform != RuntimePlatform.Android )
        {
            if ( ( float ) Screen.height / ( float ) Screen.width > 2f || ( float ) Screen.width / ( float ) Screen.height > 2f )
            {
                for ( int i = 0; i < iphoneXWidget.Count; i++ )
                {
                    iphoneXWidget[i].bottomAnchor.absolute = 750;
                    iphoneXWidget[i].UpdateAnchors();
                }
            }
        }

		while (mPlayer.isPlaying) {
			yield return null;
		}


        ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.INTRO_E);
        Extension.PokoLog.Log( "==========growthy InsertGrowthySequentialEvent(enable)3");
    
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Lobby");
	}

    private void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus == false)
        {
            if( mPlayer.isPlaying )
                mPlayer.Pause();
        }
        else
        {
            mPlayer.Play();
        }
        
    }


    public void OnSkip()
    {
        mPlayer.Stop();
        // 동영상 끝남과 함께, 자막도 끝남
        this.procAnimIntro.EndOfSubstring();
    }

	// Update is called once per frame
	void Update () {

        string text = "Loading";
        for (int i = 0; i < (int)Mathf.PingPong(Time.time * 7f, 4); i++)
            text += ".";
        _loadText.text = text;

       /* if (Input.GetKeyDown(KeyCode.Escape) && mPlayer != null)
            mPlayer.Stop();

	    if (mSkipCount > MaxSkipCount) {
	        mPlayer.Stop();
	    }

        
	    if (Input.anyKeyDown) {
	        if (Input.GetKeyDown(KeyCode.A)) {
	            this.procAnimIntro.SetDataOfSubString("SubStringData");
	        }
	        else {
	            mSkipCount++;
	        }
	    }*/
	}
}
