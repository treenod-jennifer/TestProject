//#define UNITY_IOS
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices; // 제거 X: 제거시 빌드에서 The type or namespace name `DllImport' 발생
using System.Text;
using UnityEngine.Networking;

// 타이틀 화면은 서버와 통신하기 전 셋팅이 필요하기 때문에
// 캐싱된 번들이 있을 경우 해당 번들로 출력, 캐싱된 번들이 없다면 기본 타이틀을 사용하고 
// 서버 통신 시점에 다운로드 받을 수 있는 번들이 있다면 다운로드 후 캐싱.
public class TitleBundle : MonoBehaviour {
    
    static AssetBundle loadedBundle = null;

    private static void ResetToDefault()
    {
        PlayerPrefs.DeleteKey("Title_Ver");
        PlayerPrefs.DeleteKey("Title_Hash");

        LocalNotification.customNotificationEndTs = 0;
        LocalNotification.notificationCustomFile = "";
    }

    /// <summary>
    /// 로컬에 캐싱되어있는 타이틀 번들 로드.
    /// </summary>
    public static AssetBundle GetLocalBundle(int ver, bool scanNew = false)
    {
        string bundleFileName = $"{Application.persistentDataPath}/Title/title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}";
        
        if (scanNew)
        {
            if (loadedBundle != null)
            {
                loadedBundle.Unload(true);
                loadedBundle = null;
            }

            string newFilename = bundleFileName + "_new";
            if (File.Exists(newFilename))
            {
                if (File.Exists(bundleFileName))
                    File.Delete(bundleFileName);

                File.Copy(newFilename, bundleFileName);
                File.Delete(newFilename);
            }
        }

        if (loadedBundle != null)
            return loadedBundle;

        if (!File.Exists(bundleFileName))
        {
            return null;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(bundleFileName);
        if (null == bundle)
        {
            Debug.LogError("[ERROR] Asset bundle is NOT EXIST! : " + bundleFileName);
            return null;
        }

        loadedBundle = bundle;

        return bundle;
    }
    
    /// <summary>
    /// CDN에서 타이틀 번들을 다운받아야하는지 체크 및 다운로드.(서버와 데이터 통신하는 시점)
    /// </summary>
    public static IEnumerator CheckBundleAndDownload(string cdnURL, int ver)
    {
        // ver == 0 이면 그냥 기본모드로 돌아간다는 의미.
        if (ver == 0)
        {
            ResetToDefault();

            yield break;
        }

        if (Global.LoadFromInternal)
        {
            PlayerPrefs.SetInt("Title_Ver", ver);
            yield break;
        }

        string bundleFileName = $"{Application.persistentDataPath}/Title/title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}";
        FileInfo fileInfo = new FileInfo(bundleFileName);

        // 해당 버전의 캐싱된 파일이 없거나, 해시값이 변경되었을 경우 CDN에서 타이틀 번들 다운로드.
        if (!fileInfo.Exists || HashChecker.IsHashChanged(ManagerAssetLoader.GetBundlePath(), $"title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}", fileInfo))
        {
            bool ret = false;
            yield return DownloadBundle(ver, cdnURL, (r) => { ret = r; });

            if (ret)
            {
                ManagerData._instance.StartCoroutine(DownloadBundleSetting(ver));
            }
            else
            {
                ResetToDefault();
                yield break;
            }
        }
        else
        {
            PlayerPrefs.SetInt("Title_Ver", ver);
        }
    }

    /// <summary>
    /// 다운로드된 타이틀 번들을 바로 교체하지 않고 타이틀씬이 언로드 되고 나면 교체한다.
    /// </summary>
    private static IEnumerator DownloadBundleSetting(int ver)
    {
        yield return new WaitUntil(() => SceneTitle.instance == null);

        var bundle = GetLocalBundle(ver, true);
        if (!bundle)
        {
            ResetToDefault();
            yield break;
        }
        else
        {
            GameObject go = bundle.LoadAsset<GameObject>("TitlePrefab");
            if (!go)
            {
                ResetToDefault();
                yield break;
            }

            var titleVerData = go.GetComponent<TitleData>();
            if (!titleVerData)
            {
                ResetToDefault();
                yield break;
            }
            else //정상 타이틀 번들 다운로드 완료
            {
                PlayerPrefs.SetInt("Title_Ver", ver);
                PlayerPrefs.SetString("Title_Hash", GetCurrentBundleHash(ver));
            }
        }
    }

    /// <summary>
    /// 캐싱되어있는 번들 파일의 해시값을 리턴.
    /// </summary>
    private static string GetCurrentBundleHash(int ver)
    {
        string bundleFileName = $"{Application.persistentDataPath}/Title/title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}";
        if (File.Exists(bundleFileName) == false)
            return "";

        return HashChecker.GetHash(new FileInfo(bundleFileName));
    }
    
    /// <summary>
    /// CDN 서버에서 타이틀 번들 다운로드.
    /// </summary>
    private static IEnumerator DownloadBundle(int ver, string cdnURL, Action<bool> callback)
    {
        string bundleUrl = cdnURL + $"title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}.{ManagerAssetLoader.BUNDLE_EXTENSION}";
        
        UnityWebRequest request = new UnityWebRequest(bundleUrl, UnityWebRequest.kHttpVerbGET);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
        request.SetRequestHeader("Pragma", "no-cache");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.timeout = 180;
        yield return request.SendWebRequest();


        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("[ERROR] Downloading bundles FAIL : " + request.error + " FILE : " + bundleUrl);
        }
        else
        {
            // Show results as text
            Debug.Log(request.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = request.downloadHandler.data;

            if (SaveBundle($"title_v2_{ver.ToString()}_{LanguageUtility.SystemCountryCodeForAssetBundle}", results))
            {
                callback(true);
                yield break;
            }
        }

        callback(false);
        yield break;
    }

    /// <summary>
    /// 다운로드 받은 번들 파일 캐싱.
    /// </summary>
    private static bool SaveBundle(string fileName, byte[] bundleStream)
    {
        string saveDir = Application.persistentDataPath + "/Title/";
        string savePath = $"{saveDir}{fileName}_new";
        
        try
        {
            if (!System.IO.Directory.Exists(saveDir))
                System.IO.Directory.CreateDirectory(saveDir);

            if (File.Exists(savePath))
                File.Delete(savePath);

            FileStream stream = new FileStream(savePath, FileMode.Create);
            stream.Write(bundleStream, 0, bundleStream.Length);
            stream.Flush();
            stream.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ERROR] Save bundle FAIL : " + ex.Message);
            return false;
        }

        return true;
    }
}
