using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 지원하는 데이터의 형태

//              CDN    Web    R      S
//byte[]        o      o             o
//Texture2D     o      o      o      o
//AudioClip     o(E,W) o      o      o(W)
//string        o(E)                 o
//apng          o                    o

//(E)는 암호화 된 파일 입니다.
//(W)는 WAV 파일만 지원 합니다.

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance;
    private static ResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject resourceManager = new GameObject();
                resourceManager.hideFlags = HideFlags.HideAndDontSave;
                resourceManager.name = nameof(ResourceManager);
                DontDestroyOnLoad(resourceManager);

                instance = resourceManager.AddComponent<ResourceManager>();
            }

            return instance;
        }
    }

    private string unloadedSceneName;

    private void Awake()
    {
        SceneManager.sceneUnloaded += (unLoadedScene) =>
        {
            unloadedSceneName = unLoadedScene.name;
        };

        SceneManager.sceneLoaded += (loadedScene, mode) =>
        {
            if( (unloadedSceneName == "Lobby" && loadedScene.name == "InGame") ||
                (unloadedSceneName == "InGame" && loadedScene.name == "Lobby") ||
                Global._pendingReboot)
            {
                UnLoadAll();
            }
        };
    }



    /// <summary>
    /// 비동기 다운로드를 실행하고, 완료되는 액션에 대한 처리를 담당합니다.
    /// LoadSchedule하나에 여러 LoadAction이 존재할 수 있습니다.
    /// (3001.png 파일을 A, B, C 3개의 오브젝트에서 로드를 실행 한다면, 3001.png를 다운로드 하는 하나의 LoadSchedule을 가지고, 로드 완료시 A, B, C각각의 완료 콜백(LoadAction이)이 호출 됩니다.
    /// </summary>
    public class LoadAction
    {
        /// <summary>
        /// 다운로가 취소처리 되면 실행하지 않는 액션
        /// </summary>
        private Action<object> action;

        /// <summary>
        /// 취소 여부와 상관없이 완료시 무조건 호출되는 액션
        /// </summary>
        private Action<object> hiddenComplete;

        /// <summary>
        /// 비동기 다운로드의 취소 여부
        /// </summary>
        private bool isCancel = false;

        /// <summary>
        /// action의 실행이 이루어 졌는지에 대한 플래그
        /// </summary>
        private bool isExecute = false;

        /// <summary>
        /// 유효성 검사. 다운로드가 취소 처리 되지 않고, 한번도 실행한적이 없으면 유효
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return !isCancel && !isExecute;
            }
        }

        public LoadAction(Action<object> action, Action<object> hiddenComplete)
        {
            this.action = action;
            this.hiddenComplete = hiddenComplete;
        }

        public void Cancel()
        {
            if (IsAvailable)
            {
                DebugLog($"로딩 취소");
                isCancel = true;
            }
        }

        public bool Invoke(object resource)
        {
            if (!isCancel)
            {
                action?.Invoke(resource);
                isExecute = true;
            }

            hiddenComplete?.Invoke(resource);

            return !isCancel;
        }
    }

    /// <summary>
    /// 실제 비동기 다운로드를 하는 작업을 칭합니다.
    /// </summary>
    private class LoadSchedule
    {
        private readonly string key;
        private readonly Coroutine coroutine;
        private readonly List<LoadAction> completeEvent = new List<LoadAction>();
        public int CompleteEventCount 
        {
            get
            {
                return completeEvent.Count;
            } 
        }

        private LoadSchedule(string key, Coroutine coroutine, LoadAction complete)
        {
            this.key = key;
            this.coroutine = coroutine;
            completeEvent.Add(complete);
        }

        /// <summary>
        /// 스케쥴에 등록된 작업을 실행 합니다.
        /// </summary>
        /// <returns>취소된 작업 수</returns>
        public int Complete<T>(T resource)
        {
            int cancelCount = 0;

            if (Instance.loadScheduleDictionary.Remove(key))
            {
                foreach (var loadAction in completeEvent)
                {
                    if (!loadAction.Invoke(resource))
                    {
                        cancelCount++;
                    }
                }
            }

            return cancelCount;
        }

        public void Cancel()
        {
            if (Instance.loadScheduleDictionary.Remove(key))
            {
                DebugLog($"로딩 취소(coroutine stop) : {key}");
                Instance.StopCoroutine(coroutine);
            }
        }

        public static LoadSchedule Make(string key, Coroutine coroutine, LoadAction complete)
        {
            var (isSuccess, schedule) = Add(key, complete);

            if (!isSuccess)
            {
                LoadSchedule makeSchedule = new LoadSchedule(key, coroutine, complete);
                Instance.loadScheduleDictionary.Add(key, makeSchedule);
                return makeSchedule;
            }
            else
            {
                return schedule;
            }
        }

        /// <summary>
        /// 이미 로드중인 스케줄이 있는 경우 스케줄 완료 complete action을 추가합니다.
        /// </summary>
        public static (bool isSuccess, LoadSchedule schedule) Add(string key, LoadAction complete)
        {
            if (Instance.loadScheduleDictionary.TryGetValue(key, out LoadSchedule schedule))
            {
                schedule.completeEvent.Add(complete);
                return (true, schedule);
            }
            else
            {
                return (false, null);
            }
        }
    }

    private readonly Dictionary<string, LoadSchedule> loadScheduleDictionary = new Dictionary<string, LoadSchedule>();



    /// <summary>
    /// 메니져에서 관리되는 리소스의 형태 입니다. 레퍼런스 카운팅으로 리소스를 관리할 수 있습니다.
    /// </summary>
    private class ResourceObject
    {
        public readonly object resource;

        public event Action<object> UnLoadEvent;

        private int refCount = 0;

        public ResourceObject(object resource)
        {
            this.resource = resource;
        }

        public void RefAdd(int count = 1)
        {
            refCount += count;
            DebugLog($"레퍼런스 증가 : {refCount}(+{count}) / {Instance.resourceDictionary.DebugGetKey(resource)}");
        }

        public bool RefSub(int count = 1)
        {
            refCount -= count;
            DebugLog($"레퍼런스 감소 : {refCount}(-{count}) / {Instance.resourceDictionary.DebugGetKey(resource)}");

            if (refCount == 0)
            {
                UnLoad();
                return true;
            }

            return false;
        }

        public void UnLoad()
        {
            UnLoadEvent?.Invoke(resource);
            ResourceManager.UnLoadEvent?.Invoke(resource);
        }
    }

    /// <summary>
    /// ResourceObject를 관리하는 Dictionary입니다.
    /// </summary>
    private class ResourceDictionary : IEnumerable<KeyValuePair<string, ResourceObject>>
    {
        private Dictionary<string, ResourceObject> resources = new Dictionary<string, ResourceObject>();
        private Dictionary<object, string> resourcesMirror = new Dictionary<object, string>();

        public void AddResource(string key, ResourceObject resourceObject)
        {
            if (!resources.ContainsKey(key))
            {
                resources.Add(key, resourceObject);
                resourcesMirror.Add(resourceObject.resource, key);
            }
        }

        public void RemoveResource(object resource)
        {
            if (resourcesMirror.TryGetValue(resource, out string key))
            {
                resources.Remove(key);
                resourcesMirror.Remove(resource);

                DebugLog($"남은 리소스 : {resources.Count}");
            }
        }

        public bool TryWithKey(string key, out ResourceObject resourceObject)
        {
            return resources.TryGetValue(key, out resourceObject);
        }

        public bool TryWithResource(object resource, out ResourceObject resourceObject)
        {
            if (resourcesMirror.TryGetValue(resource, out string keystring))
            {
                return resources.TryGetValue(keystring, out resourceObject);
            }
            else
            {
                resourceObject = null;
                return false;
            }
        }

        public void Clear()
        {
            resources.Clear();
            resourcesMirror.Clear();
        }

        public IEnumerator<KeyValuePair<string, ResourceObject>> GetEnumerator()
        {
            return resources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return resources.GetEnumerator();
        }

        public string DebugGetKey(object resource)
        {
            return resourcesMirror[resource];
        }
    }

    private readonly ResourceDictionary resourceDictionary = new ResourceDictionary();

    public static string GetCDNKey(string localFolderName, string cdnFolderName, string fileName, Type resourceType)
    {
        const string CDN_HEAD = "CDN";
        return $"{CDN_HEAD}/{localFolderName}/{cdnFolderName}/{fileName}({resourceType.Name})";
    }

    public static string GetWebKey(string url, Type resourceType)
    {
        const string WEB_HEAD = "Web";
        return $"{WEB_HEAD}/{url}({resourceType.Name})";
    }

    public static string GetResourceKey(string path, Type resourceType)
    {
        const string RESOURCE_HEAD = "Resource";
        return $"{RESOURCE_HEAD}/{path}({resourceType.Name})";
    }

    public static string GetStreamingKey(string path, Type resourceType)
    {
        const string STREAMING_HEAD = "Streaming";
        return $"{STREAMING_HEAD}/{path}({resourceType.Name})";
    }



    /// <summary>
    /// 메모리에 리소스가 있는 경우 사용됩니다.
    /// </summary>
    /// <typeparam name="T">로드한 리소스 타입</typeparam>
    /// <param name="key">리소스관리에 필요한 키값</param>
    /// <param name="complete">로드 완료시 실행되는 액션(다운로드 과정을 거친다면 취소 처리가 될 수 있고, 이 경우 호출되지 않음)</param>
    /// <param name="hiddenComplete">로드 완료시 무조건 실행되는 액션(다운로드 과정의 취소 처리랑 상관없이 호출됨)</param>
    /// <returns>메모리에서 리소스 로드의 성공 여부</returns>
    private static bool LoadCache<T>(string key, Action<T> complete, Action<T> hiddenComplete = null) where T : class
    {
        if (Instance.resourceDictionary.TryWithKey(key, out ResourceObject obj))
        {
            DebugLog($"메모리에 있음 {key}");

            obj.RefAdd();

            hiddenComplete?.Invoke(obj.resource as T);

            complete(obj.resource as T);

            return true;
        }
        else
        {
            DebugLog($"메모리에 없음 {key}");

            return false;
        }
    }

    /// <summary>
    /// 리소스를 로드하고 마지막 처리를 위한 프로세스 입니다. (외부에서 다운로드, 로컬에서 로드 하는 경우 사용합니다)
    /// </summary>
    /// <typeparam name="T">로드한 리소스 타입</typeparam>
    /// <param name="key">리소스관리에 필요한 키값</param>
    /// <param name="resource">로드된 리소스</param>
    /// <param name="complete">로드 완료시 실행되는 액션(다운로드 과정을 거친다면 취소 처리가 될 수 있고, 이 경우 호출되지 않음)</param>
    /// <param name="hiddenComplete">로드 완료시 무조건 실행되는 액션(다운로드 과정의 취소 처리랑 상관없이 호출됨)</param>
    /// <param name="unLoadAction">리로스를 언로드 하는 방법을 지정</param>
    private static void LoadEnd<T>(string key, T resource, Action<T> complete, Action<T> hiddenComplete = null, Action<object> unLoadAction = null)
    {
        try
        {
            ResourceObject resourceObject = null;

            if (resource != null)
            {
                if (!Instance.resourceDictionary.TryWithKey(key, out resourceObject))
                {
                    resourceObject = new ResourceObject(resource);
                    resourceObject.UnLoadEvent += unLoadAction;
                    Instance.resourceDictionary.AddResource(key, resourceObject);
                }
            }

            if (Instance.loadScheduleDictionary.TryGetValue(key, out LoadSchedule schedule))
            {
                resourceObject?.RefAdd(schedule.CompleteEventCount);
                int cancelCount = schedule.Complete(resource);
                UnLoad(resource, cancelCount);
            }
            else
            {
                resourceObject?.RefAdd();
                complete?.Invoke(resource);
                hiddenComplete?.Invoke(resource);
            }
        }
        catch(MissingReferenceException)
        {
            Debug.Log("test");

            if(resource != null)
            {
                UnLoad(resource);
            }
        }
    }

    /// <summary>
    /// 리소스가 메모리에 없는 경우 외부에서 로드하기 위해 사용됩니다.(비동기 방식으로 로드 하는 모든 방식을 지칭 합니다. 예-로컬에 저장된 리소스를 로드 하는 것도 포함 됩니다.)
    /// </summary>
    /// <typeparam name="T">로드한 리소스 타입</typeparam>
    /// <param name="key">리소스관리에 필요한 키값</param>
    /// <param name="complete">로드 완료시 실행되는 액션(다운로드 과정을 거친다면 취소 처리가 될 수 있고, 이 경우 호출되지 않음)</param>
    /// <param name="downloader">다운로드를 하기위한 추상화된 방법</param>
    /// <param name="unLoadAction">리로스를 언로드 하는 방법을 지정</param>
    /// <param name="hiddenComplete">로드 완료시 무조건 실행되는 액션(다운로드 과정의 취소 처리랑 상관없이 호출됨)</param>
    /// <returns>비동기 다운로드가 실행되면 LoadAction을 넘겨 주며, 이를 통해 다운로드를 취소 처리 할 수 있음</returns>
    private static LoadAction DownLoad<T>(string key, Action<T> complete, Func<Action<T>, Coroutine> downloader, Action<object> unLoadAction = null, Action<T> hiddenComplete = null) where T : class
    {
        LoadAction loadAction = new LoadAction((resource) => complete(resource as T), (resource) => hiddenComplete?.Invoke(resource as T));

        (bool isSuccess, LoadSchedule schedule) = LoadSchedule.Add(key, loadAction);

        if (isSuccess)
        {
            return loadAction;
        }
        else
        {
            Coroutine coroutine = downloader((T resource) => LoadEnd(key, resource, complete, hiddenComplete, unLoadAction));

            if (coroutine != null)
            {
                LoadSchedule.Make(key, coroutine, loadAction);

                return loadAction;
            }
        }

        return null;
    }



    public static LoadAction LoadCDN<T>(string cdnFolderName, string fileName, Action<T> complete, Action<T> hiddenComplete = null) where T : class
    {
        return LoadCDN(cdnFolderName, cdnFolderName, fileName, complete, hiddenComplete);
    }

    public static LoadAction LoadCDN<T>(string localFolderName, string cdnFolderName, string fileName, Action<T> complete, Action<T> hiddenComplete = null) where T : class
    {
        var path = CDNLoadUtility.LegacyPathConvert(localFolderName, cdnFolderName, fileName);

        string key = GetCDNKey(path.localFolderName, path.cdnFolderName, path.fileName, typeof(T));

        if (!LoadCache(key, complete, hiddenComplete))
        {
            return DownLoad(
                key,
                complete,
                (downloadComplete) => LoadFuncCDN(path.localFolderName, path.cdnFolderName, path.fileName, downloadComplete),
                UnLoadDestroy,
                hiddenComplete
            );
        }

        return null;
    }

    public static LoadAction LoadWeb<T>(string url, Action<T> complete, Action<T> hiddenComplete = null) where T : class
    {
        string key = GetWebKey(url, typeof(T));

        if (!LoadCache(key, complete, hiddenComplete))
        {
            return DownLoad(
                key,
                complete,
                (downloadComplete) => LoadFuncWeb(url, downloadComplete),
                UnLoadDestroy,
                hiddenComplete
            );
        }

        return null;
    }

    public static void LoadResource<T>(string path, Action<T> complete) where T : class
    {
        string key = GetResourceKey(path, typeof(T));

        if (!LoadCache(key, complete))
        {
            LoadFuncResource(path, (T resource) =>
            {
                LoadEnd(
                    key: key, 
                    resource: resource, 
                    complete: complete, 
                    unLoadAction: UnLoadResource);
            });
        }
    }

    public static LoadAction LoadStreaming<T>(string path, Action<T> complete, Action<T> hiddenComplete = null) where T : class
    {
        string key = GetStreamingKey(path, typeof(T));

        if (!LoadCache(key, complete, hiddenComplete))
        {
            return DownLoad(
                key,
                complete,
                (downloadComplete) => LoadFuncStreaming(path, downloadComplete),
                UnLoadDestroy,
                hiddenComplete
            );
        }

        return null;
    }



    public static event Action<object> UnLoadEvent;

    public static void UnLoad(object resource)
    {
        UnLoad(resource, 1);
    }

    private static void UnLoad(object resource, int count)
    {
        if (count < 1) return;

        if (resource == null) return;

        if (Instance.resourceDictionary.TryWithResource(resource, out ResourceObject resourceObject))
        {
            if (resourceObject.RefSub(count))
            {
                Instance.resourceDictionary.RemoveResource(resource);
            }
        }
    }

    public static void UnLoadAll()
    {
        DebugLog("<color=green><size=21><b>UnLoadAll!!</b></size></color>");

        foreach(var loadSchedule in Instance.loadScheduleDictionary)
        {
            loadSchedule.Value.Cancel();
        }

        Instance.loadScheduleDictionary.Clear();

        foreach (var resourceObject in Instance.resourceDictionary)
        {
            resourceObject.Value.UnLoad();
        }

        Instance.resourceDictionary.Clear();

        Resources.UnloadUnusedAssets();
    }



    /// <summary>
    /// 일반적인 리소스의 언로드 방법 입니다. Destroy 함수를 이용합니다.
    /// </summary>
    /// <param name="resource">언로드할 리소스</param>
    private static void UnLoadDestroy(object resource)
    {
        switch (resource)
        {
            case UnityEngine.Object unityResource:
                DebugLog($"{unityResource} <color=red>UnLoadDestroy</color>");
                Destroy(unityResource);
                break;
            case APNGInfo apng:
                DebugLog($"{apng} <color=red>UnLoadDestroy</color>");
                apng.UnLoad();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Resources.Load를 통해 로드한 리소스를 언로드 하는 방법 입니다.
    /// </summary>
    /// <param name="resource">언로드할 리소스</param>
    private static void UnLoadResource(object resource)
    {
        if (resource is UnityEngine.Object unityResource)
        {
            DebugLog($"{resource} <color=red>UnLoadResource</color>");
            Resources.UnloadAsset(unityResource);
        }
    }



    #region 외부에서 리소스를 로드하는 방법에 대한 정의 입니다.
    private static Coroutine LoadFuncCDN<T>(string localFolderName, string cdnFolderName, string fileName, Action<T> complete) where T : class
    {
        switch (complete)
        {
            case Action<byte[]> dataComplete:
                return CDNLoadUtility.Load(localFolderName, cdnFolderName, fileName, dataComplete);
            case Action<Texture2D> textureComplete:
                return CDNLoadUtility.Load(localFolderName, cdnFolderName, fileName, textureComplete);
            case Action<AudioClip> audioClipComplete:
                return CDNLoadUtility.Load(localFolderName, cdnFolderName, fileName, audioClipComplete);
            case Action<string> textComplete:
                return CDNLoadUtility.Load(localFolderName, cdnFolderName, fileName, textComplete);
            case Action<APNGInfo> apngComplete:
                return CDNLoadUtility.Load(localFolderName, cdnFolderName, fileName, apngComplete);
            default:
                complete(null);
                return null;
        }
    }

    private static Coroutine LoadFuncWeb<T>(string url, Action<T> complete) where T : class
    {
        switch (complete)
        {
            case Action<byte[]> dataComplete:
                return WebLoadUtility.Load(url, dataComplete);
            case Action<Texture2D> textureComplete:
                return WebLoadUtility.Load(url, textureComplete);
            case Action<AudioClip> audioClipComplete:
                return WebLoadUtility.Load(url, AudioType.WAV, audioClipComplete);
            default:
                complete(null);
                return null;
        }
    }

    private static void LoadFuncResource<T>(string path, Action<T> complete) where T : class
    {
        switch (complete)
        {
            case Action<Texture2D> _:
            case Action<AudioClip> _:
                complete(Resources.Load(path) as T);
                break;
            default:
                complete(null);
                break;
        }
    }

    private static Coroutine LoadFuncStreaming<T>(string path, Action<T> complete) where T : class
    {
        switch (complete)
        {
            case Action<byte[]> dataComplete:
                return StreamingAssetsUtility.Load(path, dataComplete);
            case Action<Texture2D> textureComplete:
                return StreamingAssetsUtility.Load(path, textureComplete);
            case Action<AudioClip> audioClipComplete:
                return StreamingAssetsUtility.Load(path, audioClipComplete);
            case Action<string> textClipComplete:
                return StreamingAssetsUtility.Load(path, textClipComplete);
            case Action<APNGInfo> apngComplete:
                return StreamingAssetsUtility.Load(path, apngComplete);
            default:
                complete(null);
                return null;
        }
    }
    #endregion



    /// <summary>
    /// 테스트를 위한 로그입니다.
    /// </summary>
    [System.Diagnostics.Conditional("SANDBOX")]
    private static void DebugLog(string log)
    {
        Debug.Log($"프레임 : {Time.frameCount} / {log}");
    }
}