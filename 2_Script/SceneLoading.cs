using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{
    [System.Serializable]
    private struct BGColorSet
    {
        public Color backColor;
        public Color frontColor;
        public Color pokotaColor;
    }

    private static bool _sceneLoading = false;
    private static bool _plaeaseWait = false;

    private const float CLOSE_POS = 10.0f;
    private const float OPEN_POS = 0.0f;

    [SerializeField] private NewDayLoader newDayLoader;
    [SerializeField] private BGColorSet[] bgColorSets;
    [SerializeField] private SpriteRenderer backBG;
    [SerializeField] private SpriteRenderer frontBG;
    [SerializeField] private SpriteRenderer pokotaBG;
    [SerializeField] private UIPanel loadingObjectRoot;

    [SerializeField] private AnimationCurve lineMoveCurve;
    [SerializeField] private Transform[] lines;
    [SerializeField] private UILabel textLoad;

#if UNITY_EDITOR
    [SerializeField] private int debugTipIndex = 0;
#endif

    public static bool IsSceneLoading
    { 
        get{ return _sceneLoading; }  
    }

    private static void Lock()
    {
        _plaeaseWait = true;
    }

    public static void Release()
    {
        _plaeaseWait = false;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var bgColor = bgColorSets[Random.Range(0, bgColorSets.Length)];

        backBG.color = bgColor.backColor;
        frontBG.color = bgColor.frontColor;
        pokotaBG.color = bgColor.pokotaColor;

        loadingObjectRoot.alpha = 0.0f;
    }

    private IEnumerator LineMove(float startPos, float endPos, float time)
    {
        float totalTime = 0.0f;

        while (totalTime < time)
        {
            totalTime += Global.deltaTime;

            float posX = Mathf.Lerp(startPos, endPos, totalTime / time);

            for (int i = 0; i < lines.Length; i++)
            {
                int direction = i % 2 == 0 ? 1 : -1;

                Vector3 pos = lines[i].localPosition;
                pos.x = posX * direction;
                lines[i].localPosition = pos;
            }

            yield return null;
        }
    }

    private IEnumerator LineMove(float startPos, float endPos, AnimationCurve animationCurve, bool curveReversal = false)
    {
        float totalTime = 0.0f;
        float endTime = animationCurve.keys[animationCurve.length - 1].time;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTime;

            float aniTime;

            if (curveReversal)
            {
                aniTime = (animationCurve.Evaluate(endTime - Mathf.Min(endTime, totalTime)) - 1.0f) * -1.0f;
            }
            else
            {
                aniTime = animationCurve.Evaluate(Mathf.Min(endTime, totalTime));
            }

            float posX = Mathf.Lerp(startPos, endPos, aniTime);

            for (int i = 0; i < lines.Length; i++)
            {
                int direction = i % 2 == 0 ? 1 : -1;

                Vector3 pos = lines[i].localPosition;
                pos.x = posX * direction;
                lines[i].localPosition = pos;
            }

            yield return null;
        }
    }

    private UIItemLoadingTip MakeLoadingObject(string sceneName, Transform parent)
    {
#if UNITY_EDITOR
        if(debugTipIndex != 0)
        {
            return UIItemLoadingTip.Make(TipInfoContainer.Instance.GetTip($"UIItemLoadingTip_{debugTipIndex}"), parent);
        }
#endif

        TipInfoContainer.TipInfo[] tips = null;

        switch (sceneName)
        {
            case "InGame":
                switch (Global.GameType)
                {
                    case GameType.ADVENTURE:
                    case GameType.ADVENTURE_EVENT:
                        tips = TipInfoContainer.Instance.GetTips("Adventure");
                        break;
                    
                    case GameType.NORMAL:
                    case GameType.EVENT:
                    case GameType.MOLE_CATCH:
                    case GameType.COIN_BONUS_STAGE:
                    default:

                        if (Global.GameType == GameType.NORMAL &&
                            ManagerData._instance._stageData != null &&
                            ManagerData._instance._stageData.Count > Global.stageIndex - 1 &&
                            ManagerData._instance._stageData[Global.stageIndex - 1]._flowerLevel == 0)
                        {
                            tips = TipInfoContainer.Instance.GetTips($"NormalGame_{Global.stageIndex}");
                        }

                        if (tips == null || tips.Length == 0) 
                        {
                            tips = TipInfoContainer.Instance.GetTips("NormalGame");
                        }

                        break;
                }
                break;

            case "Lobby":
            default:
                tips = TipInfoContainer.Instance.GetTips();
                break;
        }

        if (tips == null || tips.Length == 0) return null;

        return UIItemLoadingTip.Make(tips[Random.Range(0, tips.Length)], parent);
    }

    private IEnumerator CoActive(UIPanel target, bool isActive, float time = 0.25f, float delay = 0.0f)
    {
        if (!Mathf.Approximately(delay, 0.0f))
        {
            yield return new WaitForSeconds(delay);
        }

        float startAlpha = target.alpha;
        float endAlpha = isActive ? 1.0f : 0.0f;

        if (Mathf.Approximately(startAlpha, endAlpha)) yield break;

        float totalTime = 0.0f;

        while (totalTime < time)
        {
            totalTime += Global.deltaTime;
            target.alpha = Mathf.Lerp(startAlpha, endAlpha, totalTime / time);
            yield return null;
        }
    }

    private IEnumerator CoSceneLoding(string sceneName)
    {
        Lock();

        StartCoroutine(LoadingTextAni());

        MakeLoadingObject(sceneName, loadingObjectRoot.transform);
        StartCoroutine(CoActive(loadingObjectRoot, true, delay: 0.7f));

        yield return LineMove(OPEN_POS, CLOSE_POS, lineMoveCurve);

        //유저 데이터 변경된것이 있으면 그로씨로 전송
        ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();

        ManagerUI._instance.ImmediatelyCloseAllPopUp();

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            ManagerLobby._instance.StopRemainStartPostProc();
            ManagerUI._instance.AnchorTopDestroy();
            UILobbyChat_Base.RemoveAll();
        }

        _sceneLoading = true;
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        yield return new WaitUntil(() => async.isDone || async.progress >= 0.9f);

        async.allowSceneActivation = true;
        _sceneLoading = false;

        Scene loadScene = SceneManager.GetSceneByName(sceneName);
        yield return new WaitUntil(() => loadScene.isLoaded);
        yield return new WaitWhile(() => _plaeaseWait);

        StartCoroutine(CoActive(loadingObjectRoot, false, delay: 0.1f));

        yield return LineMove(CLOSE_POS, OPEN_POS, lineMoveCurve);

        Destroy(gameObject);
    }

    private IEnumerator CoNewDay()
    {
        Lock();

        newDayLoader.downLoadEvent += SetDownLoadText;
        StartCoroutine(LoadingTextAni());

        var newDay = UIItemNewDay.Make(Global.day, loadingObjectRoot.transform);
        StartCoroutine(CoActive(loadingObjectRoot, true, delay: 1.0f));

        yield return LineMove(OPEN_POS, CLOSE_POS, lineMoveCurve);

        yield return newDay.ImageIn();

        yield return newDayLoader.LoadNewDayResource();

        //유저 데이터 변경된것이 있으면 그로씨로 전송
        ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();

        ManagerUI._instance.ImmediatelyCloseAllPopUp();
        ManagerLobby._instance.StopRemainStartPostProc();
        ManagerUI._instance.AnchorTopDestroy();
        UILobbyChat_Base.RemoveAll();

        _sceneLoading = true;
        AsyncOperation async = SceneManager.LoadSceneAsync("Lobby");
        async.allowSceneActivation = false;

        yield return new WaitUntil(() => async.isDone || async.progress >= 0.9f);

        async.allowSceneActivation = true;
        _sceneLoading = false;

        Scene loadScene = SceneManager.GetSceneByName("Lobby");
        yield return new WaitUntil(() => loadScene.isLoaded);
        yield return new WaitWhile(() => _plaeaseWait);

        yield return new WaitUntil(() => 
        ManagerLobby._instance?._state != TypeLobbyState.None && 
        ManagerLobby._instance?._state != TypeLobbyState.Preparing);

        yield return newDay.ImageOut();

        StartCoroutine(CoActive(loadingObjectRoot, false, delay: 0.1f));

        yield return LineMove(CLOSE_POS, OPEN_POS, lineMoveCurve);

        newDayLoader.downLoadEvent -= SetDownLoadText;

        Destroy(gameObject);
    }

    private IEnumerator CoLandMove(int outlandIndex)
    {
        Lock();

        DontDestroyOnLoad(gameObject);
        landDownLoadEvent += SetDownLoadText;
        StartCoroutine(LoadingTextAni());

        MakeLoadingObject("Lobby", loadingObjectRoot.transform);
        StartCoroutine(CoActive(loadingObjectRoot, true, delay: 0.7f));

        yield return LineMove(OPEN_POS, CLOSE_POS, lineMoveCurve);

        yield return LoadLandResource(outlandIndex);

        //유저 데이터 변경된것이 있으면 그로씨로 전송
        ServiceSDK.ServiceSDKManager.instance.SendGrowthyInfo();

        ManagerUI._instance.ImmediatelyCloseAllPopUp();
        ManagerLobby._instance.StopRemainStartPostProc();
        ManagerUI._instance.AnchorTopDestroy();
        UILobbyChat_Base.RemoveAll();

        _sceneLoading = true;
        AsyncOperation async = SceneManager.LoadSceneAsync("Lobby");
        async.allowSceneActivation = false;

        yield return new WaitUntil(() => async.isDone || async.progress >= 0.9f);

        async.allowSceneActivation = true;
        _sceneLoading = false;

        Scene loadScene = SceneManager.GetSceneByName("Lobby");
        yield return new WaitUntil(() => loadScene.isLoaded);
        yield return new WaitWhile(() => _plaeaseWait);

        yield return new WaitUntil(() => 
        ManagerLobby._instance?._state != TypeLobbyState.None && 
        ManagerLobby._instance?._state != TypeLobbyState.Preparing);

        StartCoroutine(CoActive(loadingObjectRoot, false, delay: 0.1f));

        yield return LineMove(CLOSE_POS, OPEN_POS, lineMoveCurve);

        landDownLoadEvent -= SetDownLoadText;

        Destroy(gameObject);
    }

    public static SceneLoading MakeSceneLoading(string in_name)
    {
        SceneLoading obj = Instantiate(Global._instance._objSceneLoading).gameObject.GetComponent<SceneLoading>();

        obj.StartCoroutine(obj.CoSceneLoding(in_name));

        return obj;
    }

    public static SceneLoading MakeNewDayLoading()
    {
        SceneLoading obj = Instantiate(Global._instance._objSceneLoading).gameObject.GetComponent<SceneLoading>();

        obj.StartCoroutine(obj.CoNewDay());

        return obj;
    }

    public static SceneLoading MoveLand(int landId)
    {
        SceneLoading obj = Instantiate(Global._instance._objSceneLoading).gameObject.GetComponent<SceneLoading>();

        obj.StartCoroutine(obj.CoLandMove(landId));

        return obj;
    }

    private string downLoadText = string.Empty;
    private IEnumerator LoadingTextAni()
    {
        const int dotCount = 4;
        const float interval = 0.25f;
        float totalTime = 0.0f;

        while (true)
        {
            totalTime += Global.deltaTime;

            int count = (int)(totalTime / interval);
            count = count % dotCount;

            textLoad.text = "Loading";

            for(int i=0; i<count; i++)
            {
                textLoad.text += ".";
            }

            textLoad.text += downLoadText;

            yield return null;
        }
    }

    private void SetDownLoadText(long size, long totalSize)
    {
        downLoadText = $"\n({size}/{totalSize}Kbytes)";
    }

    public event System.Action<long, long> landDownLoadEvent;
    public IEnumerator LoadLandResource(int landIndex)
    {
        List<string> areaNameList = landIndex == 0 ? ServerContents.Day.GetString() : ServerContents.Day.outlands[landIndex];
        long resourceDownloaded = 0;
        long resourceTotal = 0;

        List<ManagerAssetLoader.BundleRequest> bundleLoadList = new List<ManagerAssetLoader.BundleRequest>();
        ManagerAssetLoader.EstimatedLoadResult loadEstimate = new ManagerAssetLoader.EstimatedLoadResult();


        ManagerAssetLoader.assetDataList.Clear();


        for (int i = 0; i < areaNameList.Count; i++)
        {
            if (Global.LoadFromInternal)
            {
            }
            else
            {
                string araName = areaNameList[i];
                ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
                {
                    uri = araName,
                    successCallback = null,
                    failCallback = (r) =>
                    {
                        Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                        ErrorController.ShowResourceDownloadFailed("Download Resource");
                    }
                };
                bundleLoadList.Add(bundleReq);
            }

            yield return null;

        }

        yield return ManagerAssetLoader._instance.EstimateLoad(bundleLoadList, loadEstimate);

        Debug.Log("Estimated: " + loadEstimate.totalDownloadLength);
        resourceTotal = loadEstimate.totalDownloadLength;

        yield return ManagerAssetLoader._instance.ExecuteLoad
        (
            loadEstimate,
            bundleLoadList,
            (f) =>
            {
                resourceDownloaded = (long)(f * resourceTotal);
                landDownLoadEvent?.Invoke(resourceDownloaded / 1024, resourceTotal / 1024);
            }
        );
    }
}
