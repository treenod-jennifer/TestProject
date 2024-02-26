using UnityEngine;

public class PackageSelectorAttribute : PropertyAttribute
{
}

public class PackageVariantSelectorAttribute : PropertyAttribute
{
}

public class SampleAssetBundle : MonoBehaviour
{
    /// <summary>
    /// The URI of the package to be downloaded.
    /// Unity 4: This is the relative path from the "Assets/RemotePackageManager/AssetBundles/" folder.
    /// Unity 5: This is the AssetBundle name.
    /// </summary>
    [PackageSelector]
    public string package = "";

#if !UNITY_4
    /// <summary>
    /// The AssetBundle Variant of the package to be downloaded.
    /// You can find more info at http://docs.unity3d.com/Manual/BuildingAssetBundles5x.html
    /// </summary>
    [PackageVariantSelector]
    public string variant = "";
#endif

    private bool loaded = false;

    public void Load()
    {
        if( !string.IsNullOrEmpty( package ) && !loaded )
        {
            loaded = true;
            ManagerAssetBundle.Load(package, variant, OnLoad);
        }
    }

    private void Start()
    {
        Load();
    }

    private void OnEnabled()
    {
        Load();
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
