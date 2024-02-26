using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class UIPopupVideo : UIPopupBase
{
    public static UIPopupVideo _instance = null;
    public UILabel textLoad;

    [System.Serializable]
    internal class VideoData
    {
        public string filename;

        public VideoPlayer player;

        public RenderTexture renderTexture = null;
        [SerializeField]
        public AudioSource audioSrc;

        public void Play()
        {
            player.Play();
            audioSrc.Play();
        }
    }

    VideoData videoData = null;
    Coroutine videoCoroutine = null;
    [SerializeField]
    UIPokoButton videoReplayBtn;

    [SerializeField]
    public UITexture videoTexture;

    int movieIndex = 1;
    bool loadFailed = false;
    

    private void Awake()
    {
        _instance = this;
    }
    new void OnDestroy()
    {
        if (videoData != null)
        {
            if (this.videoCoroutine != null)
            {
                StopCoroutine(videoCoroutine);
                videoData.player.Stop();
                videoData.audioSrc.Stop();
            }
            if (ManagerSound._instance != null)
                ManagerSound._instance.UnPauseBGM();
        }

        base.OnDestroy();
        _instance = null;
    }

    public void InitPopUp(int movieIndex)
    {
        this.movieIndex = movieIndex;

        StartCoroutine(StartVideo());
    }

    private IEnumerator StartVideo()
    {
        yield return null;

        videoReplayBtn.gameObject.SetActive(false);
        ManagerSound._instance.PauseBGM();

        this.videoData = new VideoData()
        {
            filename = string.Format("mv_{0}.mp4", this.movieIndex),
            renderTexture = new RenderTexture(1024, 1024, 24),
            player = gameObject.AddComponent<VideoPlayer>(),
            audioSrc = gameObject.AddComponent<AudioSource>(),
        };

        videoData.renderTexture.DiscardContents();

        string remoteURL = MakeMovieURL(this.videoData.filename);
        string localURL = Global.movieDirectory + videoData.filename;

        if (!File.Exists(localURL))
        {
            bool downloadRet = false;
            yield return DownloadVideo(remoteURL, videoData.filename, (bool ret) => { downloadRet = ret; });

            if( !downloadRet )
            {
                textLoad.text = "Load Failed";
                loadFailed = true;

                yield break;
            }
        }

        //this.videoTexture.SetRect(0, 0, _data.video[0].size[0], _data.video[0].size[1]);
        //this.videoTexture.transform.position = _data.video[0].GetPosition();

        this.videoData.player.playOnAwake = false;
        this.videoData.player.renderMode = VideoRenderMode.RenderTexture;
        this.videoData.player.targetTexture = this.videoData.renderTexture;

        this.videoData.player.waitForFirstFrame = true;
        this.videoData.audioSrc.playOnAwake = false;
        this.videoData.audioSrc.Pause();

        videoTexture.mainTexture = this.videoData.renderTexture;

        this.videoData.player.source = VideoSource.Url;
        this.videoData.player.audioOutputMode = VideoAudioOutputMode.AudioSource;

        //Assign the Audio from Video to AudioSource to be played
        this.videoData.player.EnableAudioTrack(0, true);
        this.videoData.player.SetTargetAudioSource(0, this.videoData.audioSrc);

        this.videoData.player.url = Global.FileUri + localURL;


        this.videoCoroutine = StartCoroutine(VideoPlayCoroutine());
    }

    IEnumerator VideoPlayCoroutine()
    {
        if (!videoData.player.isPrepared)
            this.videoData.player.Prepare();

        while (!this.videoData.player.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }

        videoData.Play();

        while (videoData.player.isPlaying)
        {
            yield return new WaitForSeconds(0.5f);
        }

        this.videoData.player.Stop();
        this.videoData.audioSrc.Stop();

        videoReplayBtn.gameObject.SetActive(true);

        this.videoCoroutine = null;
        yield break;
    }

    IEnumerator DownloadVideo(string remoteURL, string saveFilename, System.Action<bool> callback)
    {
        using (var www = UnityWebRequest.Get(remoteURL))
        {
            www.timeout = 7;
            yield return www.SendWebRequest();

            if (www.IsError() || www.downloadHandler == null )
            {
                callback(false);
            }
            else
            {
                string saveDir = Application.persistentDataPath + "/movie/";
                string savePath = saveDir + saveFilename;

                try
                {
                    if (!Directory.Exists(saveDir))
                        Directory.CreateDirectory(saveDir);

                    if (File.Exists(savePath))
                        File.Delete(savePath);

                    FileStream stream = new FileStream(savePath, FileMode.Create);
                    stream.Write(www.downloadHandler.data, 0, www.downloadHandler.data.Length);
                    stream.Flush();
                    stream.Close();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[ERROR] Save bundle FAIL : " + ex.Message);
                    callback(false);
                    yield break;
                }

                callback(true);
                yield break;
            }

        }
        

       
    }

    void OnClickReplay()
    {
        if (this.videoData == null)
            return;

        if (this.videoData.player.isPlaying)
        {
            this.videoData.player.Stop();
            this.videoData.audioSrc.Stop();
        }

        videoReplayBtn.gameObject.SetActive(false);

        this.videoCoroutine = StartCoroutine(VideoPlayCoroutine());
    }

    string MakeMovieURL(string filename)
    {
        var cdnUrl = NetworkSettings.Instance.GetCDN_URL(); //Trident.TridentSDK.getInstance().getCdnServerUrl();

        if (string.IsNullOrEmpty(cdnUrl))
        {
            cdnUrl = "https://s3.ap-northeast-2.amazonaws.com/pokopuzzle";
        }
        return cdnUrl + "/movie/" + filename;
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }    

    void Update()
    {
        if(!loadFailed)
            textLoad.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }
}
