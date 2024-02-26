using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TitleBundle : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static AssetBundle loadedBundle = null;

    public static IEnumerator CheckBundleAndDownload(string cdnURL, int ver)
    {
        // ver == 0 이면 그냥 기본모드로 돌아간다는 의미
        if (ver == 0)
        {
            PlayerPrefs.DeleteKey("Title_Ver");
            yield break;
        }
        
        // 이전에 받아둔 번들이 없었던 경우
        AssetBundle bundle = GetLocalBundle();
        if (!bundle)
        {
            bool ret = false;
            yield return DownloadBundle(cdnURL, (r) => { ret = r; });

            if( ret)
            {
                // 다운로드에 성공했으면 일단 밑에 버전체크하는 부분을 공통적으로 거쳐야하고, 만약 다운실패면
                // 어짜피 전에 받은번들이 없었으니까 바로 리네임하고 그런 처리도 해버림
                bundle = GetLocalBundle(true);
                if( !bundle)
                {
                    PlayerPrefs.DeleteKey("Title_Ver");
                    yield break;
                }
            }
            else
            {
                PlayerPrefs.DeleteKey("Title_Ver");
                yield break;
            }
        }
            

        GameObject go = bundle.LoadAsset<GameObject>("TitlePrefab");
        if (!go)
            yield break;
        var titleVerData = go.GetComponent<TitleData>();
        if (!titleVerData)
            yield break;

        if ( titleVerData.version != ver)
        {
            bool ret = false;
            yield return DownloadBundle(cdnURL, (r) => { ret = r; });

            if (ret)
                PlayerPrefs.SetInt("Title_Ver", ver);
            else
                PlayerPrefs.DeleteKey("Title_Ver");

            yield break;
        }
        else
        {
            PlayerPrefs.SetInt("Title_Ver", ver);
        }
    }

    static public AssetBundle GetLocalBundle(bool scanNew = false)
    {
        string bundleFileName = string.Format("{0}/Title/{1}", Application.persistentDataPath, "title");

        if ( scanNew)
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

    static public IEnumerator DownloadBundle(string cdnURL, Action<bool> callback)
    {
        string bundleUrl = cdnURL + "title";

        using (WWW www = new WWW(bundleUrl))
        {
            while (!www.isDone && string.IsNullOrEmpty(www.error))
            {
                yield return null;
            }

            if (!string.IsNullOrEmpty(www.error))
                Debug.LogError("[ERROR] Downloading bundles FAIL : " + www.error + " FILE : " + bundleUrl);
            else
            {
                if( SaveBundle("title", www.bytes) )
                {
                    callback(true);
                    yield break;
                }
            }
        }

        callback(false);
        yield break;
    }

    static bool SaveBundle(string fileName, byte[] bundleStream)
    {
        string savePath = string.Format("{0}/Title/{1}_new", Application.persistentDataPath, fileName);

        string saveDir = Application.persistentDataPath + "/Title/";

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
