using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 향후 작업 사항

// https://spphire9.wordpress.com/2016/08/06/unitywebrequest%E3%81%A7%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB%E3%82%92%E3%83%80%E3%82%A6%E3%83%B3%E3%83%AD%E3%83%BC%E3%83%89%E3%81%97%E3%81%A6%E4%BF%9D%E5%AD%98%E3%81%99%E3%82%8B/
// UnityWebRequest 퍼버을 이용해서 캐싱을 해야 순간적인 메모리를 아낄수있다.


// 각각 다른 사람이 다른 자리에서 각각 에셋을 만들었을때 자동으로 연결이 되는지 어떤지..
// 유니티 버전이 다른때 다른 구버전에서 만든 에셋도 자동으로 연결시켜 주는지 실험


 
public class ManagerBundle : MonoBehaviour {

	// Use this for initialization
    IEnumerator Start()
    {

   /*     // if(false)
        {
            string path = GetRelativePath() + "/" + Global.assetBundleDirectory + "/" + "common1";

            WWW www = new WWW(path);
            yield return www;
            Debug.Log(path);


            if (www.error != null)
                Debug.Log(www.error);

            AssetBundle bundle = www.assetBundle;
            //foreach (string name in bundle.GetAllAssetNames())
                //Debug.Log("loade------ " + name);

            // bundle.LoadAllAssets();
       //     GameObject Obj = bundle.LoadAsset<GameObject>("assets/4_outresource/common1/par_dust1.prefab");
       //     NewObject(Obj);

        //    Debug.Log(path);
        }

       {
            string path = GetRelativePath() + "/" + Global.assetBundleDirectory + "/" + "chapter1/active";

            WWW www = new WWW(path);
            yield return www;
            Debug.Log(path);


            if (www.error != null)
                Debug.Log(www.error);

            AssetBundle bundle = www.assetBundle;
        //    foreach (string name in bundle.GetAllAssetNames())
         //       Debug.Log("loade------ " + name);

            // bundle.LoadAllAssets();

            {
                GameObject Obj = bundle.LoadAsset<GameObject>("ChapterActive.prefab");
                NewObject(Obj);
            }
       

        }*/
     /*  {
           string path = GetRelativePath() + "/" + Global.assetBundleDirectory + "/" + "area1/common_prefab";

            WWW www = new WWW(path);
            yield return www;
            Debug.Log(path);


            if (www.error != null)
                Debug.Log(www.error);

            AssetBundle bundle = www.assetBundle;
            foreach (string name in bundle.GetAllAssetNames())
                Debug.Log("loade------ " + name);

            // bundle.LoadAllAssets();
            GameObject Obj = bundle.LoadAsset<GameObject>("AreaCommon.prefab");
            NewObject(Obj);

        }*/
        
        yield return null;
    }

    // 쉐이더가 깨지는 이유,,번들을 안드로이드 타켓으로 만들고 에디터에서 읽어버려서, 플렛폼에 맞게 모두 에셋을 만들어야 한다.
    // http://smilejsu.tistory.com/1020
    void NewObject(Object obj)
    {
        GameObject tmp = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;

#if UNITY_EDITOR
        foreach (Transform t in tmp.GetComponentsInChildren<Transform>())
            if (t.gameObject.GetComponent<Renderer>() != null)
                t.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(t.gameObject.GetComponent<Renderer>().material.shader.name);
#endif
    }

	// Update is called once per frame
	void Update () {
		
	}

    public string GetRelativePath()
    {
#if UNITY_2018_1_OR_NEWER
        if (Application.isEditor)
            return Global.FileUri + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
            return Application.streamingAssetsPath;
        else // For standalone player.
            return "file://" + Application.streamingAssetsPath;
#else
        if (Application.isEditor)
            return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
        else if (Application.isWebPlayer)
            return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
            return Application.streamingAssetsPath;
        else // For standalone player.
            return "file://" + Application.streamingAssetsPath;
#endif
    }
}
