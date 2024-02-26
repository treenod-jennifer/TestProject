using System.Collections;
using UnityEngine;

public partial class ManagerAntiqueStore : MonoBehaviour
{
    public class AntiqueStoreResource
    {
        public AntiqueStorePack antiqueStorePack = null;

        public IEnumerator CoLoadAssetBundle()
        {
            string assetName = $"antique_store_{ServerContents.AntiqueStore.resourceIndex}";

            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path =
                    $"Assets/5_OutResource/antiqueStore/{assetName}/AntiqueStorePack.prefab";
                GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (bundleObj.GetComponent<AntiqueStorePack>() != null)
                    antiqueStorePack = bundleObj.GetComponent<AntiqueStorePack>();
#endif
            }
            else
            {
                NetworkLoading.MakeNetworkLoading(0.5f);
                
                IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
                while (e.MoveNext())
                {
                    yield return e.Current;
                }
                
                if (e.Current != null)
                {
                    AssetBundle assetBundle = e.Current as AssetBundle;
                    if (assetBundle != null)
                    {
                        GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"AntiqueStorePack");
                        
                        if (bundleObj.GetComponent<AntiqueStorePack>() != null)
                            antiqueStorePack = bundleObj.GetComponent<AntiqueStorePack>();
                    }
                }
                
                NetworkLoading.EndNetworkLoading();
            }
            yield return null;
        }
    }
}
