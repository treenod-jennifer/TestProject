using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class BundleCompareData
{
    public UInt32 crc;
    public string hash;
}

public class ManagerAssetLoader : MonoBehaviour
{
    
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

  /*  public IEnumerator InstanceLoadSync(string packageUri, System.Action<AssetBundle> callback = null)
    {
        yield return null;
        bool bLocalLoad = false;
        string localPath = pathForDocumentsFile(packageUri);

        // Debug.Log(localPath);

        AssetBundle assetBundle = null;
        if (File.Exists(localPath))
        {
            //assetBundle = AssetBundle.LoadFromFile(localPath);
            var bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
            yield return bundleRequest;
            assetBundle = bundleRequest.assetBundle;
        }

        if (assetBundle != null)
            bLocalLoad = true;
        if (!bLocalLoad)
        {
            WWW assetWWW = new WWW(GetPackageUrl(packageUri));
            yield return assetWWW;
            if (CheckError(packageUri, assetWWW))
            {
                assetWWW.Dispose();
                yield break;
            }
            assetBundle = assetWWW.assetBundle;

            string strLocal = pathForDocumentsFile(packageUri);
            //  Debug.Log(strLocal);
            //string strLocalPath = strLocal.Substring(0, strLocal.LastIndexOf('/'));
            //if (!Directory.Exists(strLocalPath))
            //  Directory.CreateDirectory(strLocalPath);
            byte[] bytes = assetWWW.bytes;
            File.WriteAllBytes(strLocal, bytes);
            Debug.Log(strLocal);
            assetWWW.Dispose();
            if (assetBundle == null)
            {
                Error(packageUri, "AssetBundle is null.");
                yield break;
            }
        }

        if (errorSet.Contains(packageUri))
        {
            errorSet.Remove(packageUri);
            yield break;
        }
        if (callback != null)
            callback(assetBundle);
        yield return assetBundle;
    }*/
    void Awake() {
     
        _instance = this;
    }
    //private void ReadAssetBundleList ()
    //{
    //    Dictionary<string, BundleCompareData> bundleData = new Dictionary<string, BundleCompareData>();

    //    string path = Application.dataPath.Replace( "/Assets", "/AssetBundles/" + EditorUserBuildSettings.activeBuildTarget.ToString().ToLower() );
    //    FileInfo[] fileInfo = new DirectoryInfo( path ).GetFiles();
    //    int length = fileInfo.Length;
    //    for ( int i = 0; i < length; i++ )
    //    {
    //        FileInfo file = fileInfo[i];
    //        if ( file.Name.IndexOf( ".manifest" ) != -1f )
    //        {
    //            string text = file.OpenText().ReadToEnd();
    //            BundleCompareData data = new BundleCompareData();
    //            data.crc = this.GetCRC( text );
    //            data.hash = this.GetHash( text ).ToString();

    //            bundleData.Add( file.Name.Replace( ".manifest", "" ), data );
    //        }
    //    }

    //    File.WriteAllText( path + "/fileList.text", JsonFx.Json.JsonWriter.Serialize( bundleData ), System.Text.Encoding.UTF8 );
    //}

    public IEnumerator AssetBundleLoader(string packageUri,System.Action<float> reportProgress = null, System.Action<AssetBundle> callback = null, Action<ResultCode> failCallback = null)
    {
        while (!Caching.ready)
            yield return null;

        Debug.Log( "cdnUri : " + packageUri );
        //Debug.Log(packageUri);
        //Debug.Log(Caching.IsVersionCached(_cdnURL + packageUri, hash) ? "캐싱O" : "캐싱X");
      ///  assetDataList.Clear();

        if ( assetDataList.Count == 0 )
        {
            string text = _cdnURL + "/fileList.text";

            for (int i = 0; i < DefaultRetry; i++) {
                UnityWebRequest request = new UnityWebRequest(_cdnURL + "/fileList.text", UnityWebRequest .kHttpVerbGET); // must enter the BOARD_ID, START_ARTICLE_ID, LIMIT and PROFILE_TEMPLATE_ID.
                request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = DefaultTimeout;
                yield return request.Send();

                if (request.isNetworkError) {
                    Debug.LogError("request error and retry: " + request.error);
                    request.Dispose();

                    bool wait = true;
                   /* ErrorController.ShowNetworkErrorDialogAndRetry("LIST", () => wait = false);
                    while (wait) {
                        yield return null;
                    }*/
                }
                else {
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
                failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
                yield break;
            }
        }

        if (assetDataList.Count > 0) {
            for (int i = 0; i < DefaultRetry; i++) {
                // string text2 =  _cdnURL + packageUri + Hash128.Parse( assetDataList[packageUri].hash ) +  assetDataList[packageUri].crc;
                if( !assetDataList.ContainsKey(packageUri) )
                {
                    Debug.LogError("Asset Not found: " + packageUri + " (not in assetDataList)");
                    failCallback(ResultCode.ASSET_NOT_EXIST);
                    break;
                }

                
                if( loadedAssetBundle.ContainsKey(packageUri))
                {
                    yield return loadedAssetBundle[packageUri];
                    yield break;
                }

                var requestData = UnityWebRequestAssetBundle.GetAssetBundle(_cdnURL + packageUri,
                    Hash128.Parse(assetDataList[packageUri].hash), 0); // assetDataList[packageUri].crc );
                requestData.timeout = DefaultTimeout;

                yield return requestData.Send();

                if (requestData.isNetworkError) {
                    Debug.LogError("request error and retry: " + requestData.error);
                    requestData.Dispose();

                    bool wait = true;
                  /*  ErrorController.ShowNetworkErrorDialogAndRetry("BUNDLE", () => wait = false);
                    while (wait) {
                        yield return null;
                    }*/
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
                    yield break;
                }
            }
            failCallback(ResultCode.ASSET_DOWNLOAD_FAILED);
            yield break;
        }
    }

    private IEnumerator ManifestDownload(string uri)
    {

        UInt32 crc;
        Hash128 hash;

        const string endWith = ".manifest";

        var request = UnityWebRequest.Get(uri + endWith);
        request.timeout = DefaultTimeout;
        yield return request.Send();

        if (request.isNetworkError)
            Debug.LogError("request error: " + request.error);
        else
        {
            crc = GetCRC(request.downloadHandler.text);
            hash = GetHash(request.downloadHandler.text);
        }

        request.Dispose();
    }
   /* public IEnumerator AssetBundleLoader(string uri, Hash128 in_hash, UInt32 in_crc)
    {
        var request = UnityWebRequest.GetAssetBundle(uri, in_hash, in_crc);
        yield return request.Send();

        if (request.isError)
            Debug.LogError("request error: " + request.error);
        else
        {
            var assetBundle = DownloadHandlerAssetBundle.GetContent(request);
          //  var go = assetBundle.LoadAsset<GameObject>(ANIMAL_FILE_NAME);

            //ResourcesManager.Instance.Set(ANIMAL_FILE_NAME, go);
            assetBundle.Unload(false);
        }
    }
    */
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

}
