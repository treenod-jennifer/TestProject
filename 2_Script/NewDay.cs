using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NewDay : MonoBehaviour {

    public Animation animationCamera = null;

    public AudioSource BGM;
    public Transform _transformGround;
    public UILabel _textNewDay1;
    public UILabel _textNewDay2;
    public UILabel _textLoad;

    static public int resourceProgress = 0;
    
    public List<UIWidget> iphoneXWidgetY = new List<UIWidget>();

    bool _complete = false;
    
    void Awake()
    {
        _textLoad.gameObject.SetActive(false);
    }
	// Use this for initialization
    IEnumerator Start()
    {
        resourceProgress = 0;
        {
            CameraEffect staticCamera = CameraEffect.MakeScreenEffect();
            staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f);
        }

        if ( Application.platform != RuntimePlatform.Android )
        {
            //// Debug.Log(" 아이폰X 대응 " + (float)Screen.height / (float)Screen.width);
            if ( ( float ) Screen.height / ( float ) Screen.width > 2f || ( float ) Screen.width / ( float ) Screen.height > 2f )
            {

                //  Debug.Log(" 아이폰X 대응 " + iphoneXWidget.Count);
                for ( int i = 0; i < iphoneXWidgetY.Count; i++ )
                {
                    iphoneXWidgetY[i].topAnchor.absolute = 30;
                }
            }
        }

        animationCamera.Play();

        if(Global._optionBGM)
            BGM.Play();

        if (Global._systemLanguage == CurrLang.eJap)
        {
            _textNewDay1.text = Global.day + "日目の朝の体操";
            _textNewDay2.text = _textNewDay1.text;
        }
        else
        {
            _textNewDay1.text = Global.day + "Day";
            _textNewDay2.text = _textNewDay1.text;
        }

        //   if (CameraEffect._lastCameraEffect != null)

        //CameraEffect._lastCameraEffect.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.4f, true);
        float screenZoom = (720f / 1280f) / ((float)Screen.width / (float)Screen.height);

        int count = 2;
        float skipX = 0.25f / screenZoom;
        float skipY = 0.2f / screenZoom;


        if(Global.day>=3)
            count = 3;

        for (int i = 0; i < count; i++)
        {
            string name = "Character/" + (TypeCharacterType)i;
            GameObject obj = Resources.Load(name) as GameObject;
            Character ch = Instantiate(obj).GetComponent<Character>();
            ch._transform.localScale = Vector3.one * 0.05f / screenZoom;
            
            ch._transform.position = _transformGround.position;
            ch._model.transform.rotation = Quaternion.identity;
            ch._transform.rotation = Quaternion.Euler(3f, -18f, 5f);
            ch._rendererShadow.transform.rotation = Quaternion.Euler(-13f, 0, 0f);
            ch._rendererShadow.transform.localPosition = Vector3.zero;
            yield return null;
            ch.ChangeState(AIStateID.eEvent, "stretching");
            ch._transform.position += new Vector3(((-skipX / 2f) * (count - 1)) + (skipX * i), -skipY, 0f);
        }
        // Play
        //
        yield return new WaitForSeconds(3.5f);
        StartCoroutine(LoadResource());
        yield return new WaitForSeconds(3.5f);
        // End

        // 리소스 다 받는거 기다리기
        while (true)
        {
            if (_complete)
                break;
            yield return null;
        }


        SceneLoading.MakeSceneLoading("Lobby");

        if (Global._optionBGM)
            BGM.Stop();
/*        AsyncOperation async = SceneManager.LoadSceneAsync("Lobby");
        async.allowSceneActivation = false;
        float progress = 0f;
        while (!async.isDone)
        {
            progress = async.progress * 100.0f;
            if (progress >= 0.9f)
                break;
            yield return null;
        }


        {
            CameraEffect staticCamera = CameraEffect.MakeScreenEffect();
            staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0f), Color.black, 0.1f);
        }
        yield return new WaitForSeconds(0.1f);
        async.allowSceneActivation = true;*/
    }
	
	// Update is called once per frame
    void Update()
    {
        _textNewDay1.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
        _textNewDay2.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);

        _textLoad.text = "Loading New Day " + resourceProgress + "%";
    }

    IEnumerator LoadResource()
    {
        yield return new WaitForSeconds(0.5f);
        _textLoad.gameObject.SetActive(true);


       /* List<string> areaNameList = ServerContents.Day.GetString();

        int resourceCount = 0;
        float snapResourceProgress = 0f;
        float resourceProgress = 0f;
        {
            //resourceCount += _housingSelectData.Count;
            resourceCount += areaNameList.Count;
            snapResourceProgress = 100f / resourceCount;
        }


        ManagerAssetLoader.assetDataList.Clear();

        
        for (int i = 0; i < areaNameList.Count; i++)
        {
            //if (ManagerLobby._assetBank.ContainsKey(areaNameList[i]))
            if (ManagerLobby._assetBankBundle.ContainsKey(areaNameList[i]))
            {
                resourceProgress += snapResourceProgress;
                NewDay.resourceProgress = (int)resourceProgress;
            }
            else
            {
                if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
                {

                }
                else
                {
                    //  string url = ManagerAssetBundle.GetInstance().GetVariantPackageUri(areaNameList[i], "");

                    float startPro = resourceProgress;
                    IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(areaNameList[i], (float in_progress) => { resourceProgress = Mathf.Lerp(startPro, startPro + snapResourceProgress, in_progress); NewDay.resourceProgress = (int)resourceProgress; });
                    //IEnumerator e = ManagerAssetBundle.GetInstance().InstanceLoadSync(url, (float in_progress) => { resourceProgress = Mathf.Lerp(startPro, startPro + snapResourceProgress, in_progress); NewDay.resourceProgress = (int)resourceProgress; });
                    while (e.MoveNext())
                        yield return e.Current;
                    if (e.Current != null)
                    {
                        AssetBundle assetBundle = e.Current as AssetBundle;
                        if (assetBundle != null)
                        {
                            ManagerLobby._assetBankBundle.Add(areaNameList[i], assetBundle);
                            if (areaNameList[i].Contains("_p"))
                            {
                                GameObject obj = assetBundle.LoadAsset<GameObject>(areaNameList[i].Replace("_p", ""));
                                ManagerLobby._assetBank.Add(areaNameList[i], obj);
                            }
                            else
                                assetBundle.LoadAllAssets();
                        }
                    }
                }
            }
            yield return null;

        }
        */
        NewDay.resourceProgress = 100;
        _complete = true;
    }
}
