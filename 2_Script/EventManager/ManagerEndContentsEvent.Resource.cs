using System.Collections;
using UnityEngine;

public partial class ManagerEndContentsEvent : MonoBehaviour
{
    public EndContentsAreaBase endContentsPack_Object = null;
    public EndContentsPack_Ingame endContentsPack_Ingame = null;
    
    public IEnumerator LoadEndContentsResource(string prefabType)
    {
        if (Global.LoadFromInternal)
            LoadFromInternal_EndContentsPack(prefabType);
        else
            yield return LoadFromBundle_EndContentsPack(prefabType);
    }

    private void LoadFromInternal_EndContentsPack(string prefabType)
    {
#if UNITY_EDITOR
        string bundleName = string.Format($"endContents_{prefabType}");
        string path = "Assets/5_OutResource/endContentsEvent/" + bundleName + $"/EndContentsPack.prefab";
        GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (bundleObj.GetComponent<EndContentsAreaBase>() != null)
            endContentsPack_Object = bundleObj.GetComponent<EndContentsAreaBase>();
        if (bundleObj.GetComponent<EndContentsPack_Ingame>() != null)
            endContentsPack_Ingame = bundleObj.GetComponent<EndContentsPack_Ingame>();
#endif
    }

    private IEnumerator LoadFromBundle_EndContentsPack(string prefabType)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string bundleName = string.Format($"e_contents_{prefabType.ToLower()}");
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"EndContentsPack");
                if (bundleObj.GetComponent<EndContentsAreaBase>() != null)
                    endContentsPack_Object = bundleObj.GetComponent<EndContentsAreaBase>();
                if (bundleObj.GetComponent<EndContentsPack_Ingame>() != null)
                    endContentsPack_Ingame = bundleObj.GetComponent<EndContentsPack_Ingame>();
            }
        }
        NetworkLoading.EndNetworkLoading();
    }
}
