using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public class PackageInfo
{
    [PackageSelector]
    public string package = "";
    [PackageVariantSelector]
    public string variant = "";
}

public class SampleAssetBundle2 : MonoBehaviour
{
    public PackageInfo[] _packageInfos;

    private bool loaded = false;

    private IEnumerator Load()
    {
        yield return null;
        if (!loaded)
        {
            loaded = true;
            int nCount = _packageInfos.Length;
            for (int i = 0; i < nCount; i++)
            {
                if (!string.IsNullOrEmpty(_packageInfos[i].package))
                {
                    string url = ManagerAssetBundle.GetInstance().GetVariantPackageUri(_packageInfos[i].package, _packageInfos[i].variant);
                    IEnumerator e = ManagerAssetBundle.GetInstance().InstanceLoadSync(url);
                    while (e.MoveNext())
                        yield return e.Current;
                    if (e.Current != null)
                    {
                        AssetBundle assetBundle = e.Current as AssetBundle;
                        if (assetBundle != null)
                            OnLoad(assetBundle);
                    }
                }
            }
        }
        yield return null;
    }

    private void Start()
    {
        StartCoroutine(Load());
    }

    private void OnEnabled()
    {
        StartCoroutine(Load());
    }

    private void OnLoad(AssetBundle assetBundle)
    {
        Object[] assets = null;
        if (!assetBundle.isStreamedSceneAssetBundle)
            assets = assetBundle.LoadAllAssets();
        int nCount = assets.Length;
        Debug.Log("------------------------- AssetBundle OnLoad : " + assetBundle.name + " -------------------------");
        for (int i = 0; i < nCount; i++)
        {
            Debug.Log("Asset Name : " + assets[i].name);
        }
        /*
        GameObject prefab = null;
        prefab = assets.OfType<GameObject>().FirstOrDefault();

        if (prefab != null)
        {
            GameObject go = Object.Instantiate(prefab) as GameObject;

            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
        }
         * */
    }
}
