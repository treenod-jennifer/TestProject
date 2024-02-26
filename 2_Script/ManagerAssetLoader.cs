using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class BundleCompareData
{
    public UInt32 crc;
    public string hash;
    public long size;
}

public class ManagerAssetLoader : MonoBehaviour
{
    public class BundleRequest
    {
        public string uri;
        internal bool cached;
        internal long length;
        public bool error;
        public Action<AssetBundle> successCallback;
        public Action<ResultCode> failCallback;
    }

    public class EstimatedLoadResult
    {
        public int needLoadCount;
        public long totalDownloadLength;
    }

    // @philip 현재 타임아웃 0으로 무한 대기라 기본 값 설정
    private const int DefaultTimeout = 180;
    private const int DefaultRetry = 10;
    
    public static ManagerAssetLoader _instance = null;
    public static string _cdnURL = "";
    public static Dictionary<string, BundleCompareData> assetDataList = new Dictionary<string,BundleCompareData>();
    public static Dictionary<string, AssetBundle> loadedAssetBundle = new Dictionary<string, AssetBundle>();

    public enum ResultCode
    {
        OK,
        FILELIST_DOWNLOAD_FAILED = -1,
        ASSET_NOT_EXIST = -2,
        ASSET_DOWNLOAD_FAILED = -3,
    }

    public const string BUNDLE_EXTENSION = "townBundle";

    private string BundleNameAddExtension(string bundleURI)
    {
        if (bundleURI.EndsWith(BUNDLE_EXTENSION))
        {
            return bundleURI;
        }
        else
        {
            return $"{bundleURI}.{BUNDLE_EXTENSION}";
        }
    }

    void Awake() {
     
        _instance = this;
    }
    
    public void ClearCache()
    {
        bool result = Caching.ClearCache();
        Debug.Log("ClearCache : " + result.ToString());
    }
    public bool IsCached(string packageUri)
    {
        string bundleName = BundleNameAddExtension(packageUri);

        if( assetDataList.ContainsKey(bundleName) == false)
        {
            return false;
        }

        return Caching.IsVersionCached(_cdnURL + bundleName, Hash128.Parse(assetDataList[bundleName].hash));
    }

    bool IsError( UnityWebRequest req )
    {
        return req.isNetworkError;
    }

    static public string GetBundlePath()
    {
        string platform;
#if UNITY_IOS
        platform = "ios";
#else
        platform = "android";
#endif

        return $"AssetBundles/{platform}_V4";
    }

    public bool CheckSize(BundleRequest req)
    {
        BundleCompareData data;

        string key = BundleNameAddExtension(req.uri);

        if (loadedAssetBundle.ContainsKey(key))
        {
            return false;
        }

        Debug.Log($"Check Size : {req.uri}");

        assetDataList.TryGetValue(key, out data);
        
        if (data == null)
        {
            Debug.LogWarning("Not Find BundleCompareData - " + key);
            return false;
        }
        else
        {
            req.length = assetDataList[key].size;
            return true;
        }
    }

    /// <summary>
    /// 메모리에 없는 번들 파일의 크기를 가져옵니다.
    /// </summary>
    /// <param name="reqList"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerator EstimateLoad(List<BundleRequest> reqList, EstimatedLoadResult result)
    {
        while (!Caching.ready)
            yield return null;

        if (assetDataList.Count == 0)
        {
            yield return LoadFilelist();
        }

        for (int i = 0; i < reqList.Count; ++i)
        {
            var req = reqList[i];

            req.cached = IsCached(req.uri);
            if (req.cached)
                continue;

            CheckSize(req);

            if( req.error == false)
            {
                result.totalDownloadLength += req.length;
                result.needLoadCount++;
            }
        }
    }

    public IEnumerator ExecuteLoad(EstimatedLoadResult est, List<BundleRequest> reqList, Action<float> progressCallback)
    {
        //if (est.totalDownloadLength == 0)
        //    yield break;

        long downloaded = 0;
        for(int i = 0; i < reqList.Count; ++i)
        {
            if (reqList[i].error == true)
                continue;

            if (reqList[i].cached == false)
            {
                yield return AssetBundleLoader(reqList[i].uri, (f) => { progressCallback((downloaded + (reqList[i].length * f)) / (float)est.totalDownloadLength); }, reqList[i].successCallback, reqList[i].failCallback);
                yield return null;
                downloaded += reqList[i].length;
            }
                
            else
            {
                yield return AssetBundleLoader(reqList[i].uri, null, reqList[i].successCallback, reqList[i].failCallback);
                yield return null;
                downloaded += reqList[i].length;
            }
        }

        yield break;
    }

    public IEnumerator LoadFilelist(Action<ResultCode> failCallback = null)
    {
        string fileListUri = FileHelper.MergePath(_cdnURL, "fileList.text");

        for (int i = 0; i < DefaultRetry; i++)
        {
            UnityWebRequest request = new UnityWebRequest(fileListUri, UnityWebRequest.kHttpVerbGET); // must enter the BOARD_ID, START_ARTICLE_ID, LIMIT and PROFILE_TEMPLATE_ID.
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            request.SetRequestHeader("Pragma", "no-cache");

            request.timeout = DefaultTimeout;
            yield return request.SendWebRequest();

            if (IsError(request))
            {
                Debug.LogError("request error and retry: " + request.error);
                request.Dispose();

                bool wait = true;
                ErrorController.ShowNetworkErrorDialogAndRetry("LIST", () => wait = false);
                while (wait)
                {
                    yield return null;
                }
            }
            else
            {
                //handling the result in here
                MemoryStream stream = new MemoryStream(request.downloadHandler.data);
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                string readToEnd = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                assetDataList = JsonFx.Json.JsonReader.Deserialize<Dictionary<string, BundleCompareData>>(readToEnd);
                break; // MUST
            }
        }
        if (assetDataList.Count == 0)
        {
            if (failCallback != null)
                failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
            yield break;
        }
    }

    public IEnumerator AssetBundleLoader(string packageUri,System.Action<float> reportProgress = null, System.Action<AssetBundle> callback = null, Action<ResultCode> failCallback = null)
    {
        while (!Caching.ready)
            yield return null;

        packageUri = BundleNameAddExtension(packageUri);

        Debug.Log( "cdnUri : " + packageUri );
      
        if ( assetDataList.Count == 0 )
        {
            yield return LoadFilelist(failCallback);
        }

        if (assetDataList.Count > 0) {
            for (int i = 0; i < DefaultRetry; i++) {
                // string text2 =  _cdnURL + packageUri + Hash128.Parse( assetDataList[packageUri].hash ) +  assetDataList[packageUri].crc;
                if( !assetDataList.ContainsKey(packageUri) )
                {
                    Debug.LogError("Asset Not found: " + packageUri + " (not in assetDataList)");
                    if( failCallback != null )
                        failCallback(ResultCode.ASSET_NOT_EXIST);
                    yield break;
                }

                
                if( loadedAssetBundle.ContainsKey(packageUri))
                {
                    yield return loadedAssetBundle[packageUri];
                    yield break;
                }

                string path = _cdnURL + packageUri;

                var requestData = UnityWebRequestAssetBundle.GetAssetBundle(path,
                    Hash128.Parse(assetDataList[packageUri].hash), 0); // assetDataList[packageUri].crc );
                requestData.timeout = DefaultTimeout;
                //requestData.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                //requestData.SetRequestHeader("Pragma", "no-cache");

                yield return requestData.SendWebRequest();


                if (IsError(requestData)) {
                    Debug.LogError("request error and retry: " + requestData.error);
                    requestData.Dispose();

                    bool wait = true;
                    ErrorController.ShowNetworkErrorDialogAndRetry("", () => wait = false);
                    while (wait) {
                        yield return null;
                    }
                }
                else {
                    var assetBundle = DownloadHandlerAssetBundle.GetContent(requestData);
                    if (assetBundle != null) {
                        if (reportProgress != null)
                            reportProgress(1f);

                        loadedAssetBundle.Add(packageUri, assetBundle);
                        if (callback != null)
                            callback(assetBundle);

                        yield return assetBundle;
                    }
                    else
                    {
                        failCallback(ResultCode.ASSET_DOWNLOAD_FAILED);
                        yield break;
                    }
                    yield break;
                }
            }
            if( failCallback != null)
                failCallback(ResultCode.ASSET_DOWNLOAD_FAILED);
            yield break;
        }
    }
    
    private IEnumerator AssetBundleLoader(string packageUri, Action<AssetBundle> callback)
    {
        while (!Caching.ready)
        {
            yield return null;
        }

        packageUri = BundleNameAddExtension(packageUri);

        Debug.Log("cdnUri : " + packageUri);

        if (assetDataList.Count > 0)
        {
            for (int i = 0; i < DefaultRetry; i++)
            {
                if (!assetDataList.ContainsKey(packageUri))
                {
                    Debug.LogError("Asset Not found: " + packageUri + " (not in assetDataList)");
                    yield break;
                }

                if (loadedAssetBundle.ContainsKey(packageUri))
                {
                    callback?.Invoke(loadedAssetBundle[packageUri]);
                    yield break;
                }

                string path = _cdnURL + packageUri;
                var requestData = UnityWebRequestAssetBundle.GetAssetBundle(path, Hash128.Parse(assetDataList[packageUri].hash), 0);
                requestData.timeout = DefaultTimeout;

                yield return requestData.SendWebRequest();

                if (IsError(requestData))
                {
                    Debug.LogError("request error and retry: " + requestData.error);
                    requestData.Dispose();

                    bool wait = true;
                    ErrorController.ShowNetworkErrorDialogAndRetry("", () => wait = false);
                    while (wait)
                    {
                        yield return null;
                    }
                }
                else
                {
                    var assetBundle = DownloadHandlerAssetBundle.GetContent(requestData);
                    if (assetBundle != null)
                    {
                        loadedAssetBundle.Add(packageUri, assetBundle);
                        if (callback != null)
                        {
                            callback(assetBundle);
                        }

                        yield return assetBundle;
                    }
                    else
                    {
                        yield break;
                    }

                    yield break;
                }
            }
        }
    }

    private IEnumerator ManifestDownload(string uri)
    {

        UInt32 crc;
        Hash128 hash;

        const string endWith = ".manifest";

        var request = UnityWebRequest.Get(uri + endWith);
        request.timeout = DefaultTimeout;
        request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
        request.SetRequestHeader("Pragma", "no-cache");

        yield return request.SendWebRequest();

        if (IsError(request))
            Debug.LogError("request error: " + request.error);
        else
        {
            crc = GetCRC(request.downloadHandler.text);
            hash = GetHash(request.downloadHandler.text);
        }

        request.Dispose();
    }
   
    private UInt32 GetCRC(string manifest)
    {
        return Convert.ToUInt32(GetValue(manifest, 1));
    }

    private Hash128 GetHash(string manifest)
    {
        return Hash128.Parse(GetValue(manifest, 5));
    }
    private string GetValue(string manifest, int index)
    {
        return manifest.Split("\n".ToCharArray())[index].Split(':')[1].Trim();
    }

    public static void UnloadBundles()
    {
        assetDataList.Clear();

        foreach(var b in loadedAssetBundle)
        {
            b.Value.Unload(false);
        }
        loadedAssetBundle.Clear();
    }

    public static string GetNewBundleName(string oldName, string versionCode)
    {
        if (string.IsNullOrEmpty(oldName))
        {
            return string.Empty;
        }

        for(int i = oldName.Length - 1; i >= 0; i--)
        {
            if(oldName[i] == '_')
            {
                return oldName.Insert(i, versionCode);
            }
        }

        return string.Empty;
    }
    
    /// <summary>
    /// 번들 로드 완료 후 콜백을 통해서 사용 처리하는 함수
    /// 대기하는 처리를 하지 않기 때문에 CoPreBundleLoad를 사용해서 미리 로드 후 사용 권장
    /// </summary>
    /// <param name="bundleNames">번들 파일 이름</param>
    /// <param name="assetName">번들에서 추출할 파일 이름</param>
    /// <param name="onComplete">로드한 번들 사용 처리</param>
    /// <typeparam name="T">번들에서 추출할 파일의 타입(NGUIAtlas, Texture, GameObject..)</typeparam>
    public void BundleLoad<T>(string bundleNames, string assetName, Action<T> onComplete = null) where T : UnityEngine.Object
    {
        Debug.Log(bundleNames + "/" + assetName);
        LoadAssetAsync(bundleNames, assetBundle =>
        {
            var result = assetBundle.LoadAsset<T>(assetName);
            onComplete?.Invoke(result);
        });
    }
    
    private void LoadAssetAsync(string bundleNames, Action<AssetBundle> onCompleted)
    {
        StartCoroutine(AssetBundleLoader(bundleNames, onCompleted));
    }
    
    /// <summary>
    /// 미리 번들을 로드해서 loadedAssetBundle 딕셔너리에 캐싱
    /// (Ex. 팝업에서 번들을 사용할 경우 아이콘 클릭 시 CoPreBundleLoad 함수를 호출하고, 호출이 완료된 후 팝업 출력)
    /// </summary>
    /// <param name="bundleNames">미리 로드 할 번들 이름 리스트</param>
    /// <returns></returns>
    public IEnumerator CoPreBundleLoad(string[] bundleNames)
    {
        foreach (var bundleName in bundleNames)
        {
            yield return StartCoroutine(AssetBundleLoader(bundleName));
        }

        yield return null;
    }
    
    private async UniTask AsyncLoadFileList(CancellationToken ct, Action<ResultCode> failCallback = null)
    {
        string fileListUri = FileHelper.MergePath(_cdnURL, "fileList.text");

        for (int i = 0; i < DefaultRetry; i++)
        {
            UnityWebRequest request = new UnityWebRequest(fileListUri, UnityWebRequest.kHttpVerbGET); // must enter the BOARD_ID, START_ARTICLE_ID, LIMIT and PROFILE_TEMPLATE_ID.
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            request.SetRequestHeader("Pragma", "no-cache");
            request.timeout = DefaultTimeout;
            await request.SendWebRequest().WithCancellation(cancellationToken:ct);;

            if (IsError(request))
            {
                Debug.LogError("request error and retry: " + request.error);
                request.Dispose();

                bool wait = true;
                ErrorController.ShowNetworkErrorDialogAndRetry("LIST", () => wait = false);
                await UniTask.WaitUntil(() => wait == false, cancellationToken:ct);
            }
            else
            {
                MemoryStream stream = new MemoryStream(request.downloadHandler.data);
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                string readToEnd = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                assetDataList = JsonFx.Json.JsonReader.Deserialize<Dictionary<string, BundleCompareData>>(readToEnd);
                break;
            }
        }
        if (assetDataList.Count == 0)
        {
            if (failCallback != null)
                failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
        }
    }
    
    public async UniTask<AssetBundle> AsyncAssetBundleLoader(string packageUri, CancellationToken ct, Action<float> reportProgress = null, Action<AssetBundle> callback = null, Action<ResultCode> failCallback = null)
    {
        await UniTask.WaitUntil(() => Caching.ready, cancellationToken:ct);

        packageUri = BundleNameAddExtension(packageUri);

        Debug.Log( "cdnUri : " + packageUri );
      
        if ( assetDataList.Count == 0 )
        {
            await AsyncLoadFileList(ct, failCallback);
        }

        if (assetDataList.Count > 0)
        {
            for (int i = 0; i < DefaultRetry; i++)
            {
                if( !assetDataList.ContainsKey(packageUri) )
                {
                    Debug.LogError("Asset Not found: " + packageUri + " (not in assetDataList)");
                    if( failCallback != null )
                        failCallback(ResultCode.ASSET_NOT_EXIST);
                    return null;
                }

                loadedAssetBundle.TryGetValue(packageUri, out AssetBundle bundle);
                if( bundle != null ) return bundle;

                string path = _cdnURL + packageUri;

                var requestData = UnityWebRequestAssetBundle.GetAssetBundle(path, Hash128.Parse(assetDataList[packageUri].hash), 0);
                requestData.timeout = DefaultTimeout;

                await requestData.SendWebRequest().WithCancellation(cancellationToken:ct);

                if (IsError(requestData)) 
                {
                    Debug.LogError("request error and retry: " + requestData.error);
                    requestData.Dispose();

                    bool wait = true;
                    ErrorController.ShowNetworkErrorDialogAndRetry("", () => wait = false);
                    await UniTask.WaitUntil(() => !wait, cancellationToken:ct);
                }
                else 
                {
                    var assetBundle = DownloadHandlerAssetBundle.GetContent(requestData);
                    if (assetBundle != null) {
                        if (reportProgress != null)
                            reportProgress(1f);

                        loadedAssetBundle.Add(packageUri, assetBundle);
                        if (callback != null)
                            callback(assetBundle);

                        return assetBundle;
                    }
                    else
                    {
                        failCallback(ResultCode.ASSET_DOWNLOAD_FAILED);
                        return null;
                    }
                }
            }
            if( failCallback != null)
                failCallback(ResultCode.ASSET_DOWNLOAD_FAILED);
        }

        return null;
    }
}

public static class AssetBundleExtend
{
    public static GameObject Instantiate_ShaderFix(this GameObject o)
    {
        GameObject obj = MonoBehaviour.Instantiate(o);

        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            var materials = rend.sharedMaterials;
            if (materials == null)
                continue;
        
            for (int i = 0; i < materials.Length; i++)
            {
                if( materials[i] != null )
                    materials[i].shader = Shader.Find(materials[i].shader.name);
            }
        }
        return obj;
    }
    
}