using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SceneIntro : MonoBehaviour
{
    // 여기 텍스처는 리소스 폴더에서 읽어서 출력,, 씬 벗어날때 읽은 텍스처 꼭 해제
    // 추가로 이벤트에 따라 텍스처를 로딩해서 타이틀씬 바뀌게도..
    // 
	public Camera camera;
    public string defaultMovieURL = "https://s3.ap-northeast-2.amazonaws.com/pokopuzzle/movie/intro/intro_low.mp4";
	public string defaultCDNAddr = "https://s3.ap-northeast-2.amazonaws.com/pokopuzzle";
	public string defaultMoviePath = "/movie/intro/intro_mid.mp4";
	public bool useSampleLatency = false;

    public GameObject _skipButton;
    public UILabel _loadText;
     
	private const int MaxSkipCount = 5;
	private VideoPlayer mPlayer = null;
	private int mSkipCount = 0;
	private bool mPreparedMovieURL = false;
	
	private AudioSource mAudioSource;
	// Use this for initialization

    public List<UIWidget> iphoneXWidget = new List<UIWidget>();

	void PrepareWithDefaultURL() {
		Debug.Log("[movie] Play Intro Movie wuth default URL " + defaultMoviePath );
		//DoPrepareMoviePlayerUrl(defaultMoviePath);
	}
    /*
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


   void PreparWithSampling() {
       var checker = GetComponent<ServerLatency>();
       if (checker != null) {
           checker.StartLatencyCheking((url) => {
               try {
                   var subUrl = url;
                   if (String.IsNullOrEmpty(subUrl)) {
                       // 네트워크 연결 안됨 무비 SKIP
                       Debug.Log("[movie] Network Not Connected!!!");
                       // Next Scene ???
                   }
                   else {
                       DoPrepareMoviePlayerUrl(subUrl);
                   }
               }
               catch (SystemException e) {
                   Debug.LogWarning("[movie] excepiont: " + e);
                   PrepareWithDefaultURL();
               }
           });
       }
   }
*/

    void PrepareMovie() {
		if (useSampleLatency) {
		//	PreparWithSampling();
		}
		else {
			PrepareWithDefaultURL();
		}
	}

	void Awake()
	{
		_skipButton.SetActive(false);
	}
	
	IEnumerator Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;


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


    
        Application.LoadLevel("Lobby");
	}


    public void OnSkip()
    {
        mPlayer.Stop();

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
