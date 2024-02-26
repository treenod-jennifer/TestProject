using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{

    static public SceneLoading _lastSceneLoading = null;
    static public bool _sceneLoading = false;

    public UISprite _spriteBoni;
    public UISprite _spriteBird;
    public UISprite _spriteBlind;
    public UILabel _textLoading;
    public UILabel _textTip;
    public UIPanel _mainPanel;
    static public bool _plaeaseWait = false;

    string sceneName = "InGame";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _sceneLoading = true;
        _plaeaseWait = false;

    }
    // Use this for initialization
    IEnumerator Start()
    {
        //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        //watch.Start();
        SettingTipText();
        StartCoroutine(CoAnimation());

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimeNoScale * 8f;
            _mainPanel.alpha = timer;
            yield return null;
        }
        _mainPanel.alpha = 1f;
        yield return new WaitForSeconds(0.5f);

        //유저 데이터 변경된것이 있으면 그로씨로 전송
        //ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();

        ManagerUI._instance.ImmediatelyCloseAllPopUp();

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            UIButtonPackage.RemoveAll();
            ManagerUI._instance.AnchorTopRightDestroy();
            UILobbyChat.RemoveAll();
        }

        yield return null;
     //   watch.Stop();
      //  Global.Log("1. " + (float)watch.ElapsedMilliseconds / 1000f);
      //  watch.Reset();
      //  watch.Start();


        {
            AsyncOperation async = SceneManager.LoadSceneAsync("Empty");
            async.allowSceneActivation = false;
            float progress = 0f;
            while (!async.isDone)
            {
                progress = async.progress * 100.0f;

                //Debug.Log(string.Format("empty[{0}] ==> {1}%\n", Time.time, progress));

                if (progress >= 0.9f)
                    break;
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            async.allowSceneActivation = true;
        }

        //Application.LoadLevel("Empty");

       // watch.Stop();
       // Global.Log("Empty. " + (float)watch.ElapsedMilliseconds / 1000f);
       // watch.Reset();
       // watch.Start();

        yield return null;
        yield return null;
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            float progress = 0f;
            while (!async.isDone)
            {
                progress = async.progress * 100.0f;

                //Debug.Log(string.Format("sceneName[{0}] ==> {1}%\n", Time.time, progress));

                if (progress >= 0.9f)
                    break;
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            _sceneLoading = false;
            async.allowSceneActivation = true;
        }
       // watch.Stop();
       // Global.Log("3. " + (float)watch.ElapsedMilliseconds / 1000f);

        yield return new WaitForSeconds(0.2f);

        if (_plaeaseWait)
        {
            while (true)
            {
                if (_plaeaseWait == false)
                    break;
                yield return null;
            }
        }
        

        timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimeNoScale * 10f;
            _mainPanel.alpha = 1f - timer;
            yield return null;
        }
        Destroy(gameObject);
        yield return null;
    }
    IEnumerator CoAnimation()
    {
        float boniFrame = 0f;
        float birdFrame = 0f;
        float textFrame = 0f;

        while (true)
        {
            boniFrame += Global.deltaTimeNoScale * 9f;
            birdFrame += Global.deltaTimeNoScale * 11f;
            textFrame += Global.deltaTimeNoScale * 13f;

            _spriteBoni.spriteName = "roading_boni0" + (int)Mathf.Repeat(boniFrame,4);
            _spriteBird.spriteName = "roading_bluebird0" + (int)Mathf.Repeat(birdFrame, 3);
            _spriteBoni.MakePixelPerfect();
            _spriteBird.MakePixelPerfect();

            _spriteBoni.cachedTransform.localScale = Vector3.one * 1.3f;
            _spriteBird.cachedTransform.localScale = Vector3.one * 1.3f;

            _textLoading.spacingX = (int)Mathf.PingPong(textFrame, 3);

            yield return null;
        }
    }
    static public SceneLoading MakeSceneLoading(string in_name)
    {
        


        SceneLoading obj = Instantiate(Global._instance._objSceneLoading).gameObject.GetComponent<SceneLoading>();
        obj.sceneName = in_name;

        if ("Lobby" == in_name)
            SceneLoading._plaeaseWait = true;
        //obj.GetComponent<Camera>().depth = in_Depth;
        return obj;
    }

    private void SettingTipText()
    {
        int randomKey = Random.Range(1, 11);
        string key = string.Format("tip_{0}", randomKey);
        _textTip.text = "Tip : " + Global._instance.GetString(key);
    }
}
