using System.Collections;
using UnityEngine;

public partial class ManagerDiaStash : MonoBehaviour
{
    public class DiaStashResource
    {
        private DiaStashPack diaStashPack = null;

        private IEnumerator CoLoadAssetBundle()
        {
            string assetName = $"diastash_event_{ServerContents.DiaStashEvent.resourceIndex}";

            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path =
                    $"Assets/5_OutResource/DiaStash/{assetName}/DiaStashPack.prefab";
                GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (bundleObj.GetComponent<DiaStashPack>() != null)
                    diaStashPack = bundleObj.GetComponent<DiaStashPack>();
#endif
            }
            else
            {
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
                        GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"DiaStashPack");
                        
                        if (bundleObj.GetComponent<DiaStashPack>() != null)
                            diaStashPack = bundleObj.GetComponent<DiaStashPack>();
                    }
                }
            }
            yield return null;
        }

        public IEnumerator CoSetDiaStashResource()
        {
            if (diaStashPack != null) yield break;

            NetworkLoading.MakeNetworkLoading(0.5f);

            yield return CoLoadAssetBundle();
            
            NetworkLoading.EndNetworkLoading();
        }

        public DiaStashPack GetDiaStashPack()
        {
            return diaStashPack;
        }
    }
}
