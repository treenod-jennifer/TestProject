using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerBingoEvent : MonoBehaviour
{
    public class BingoEventResource
    {
        public BingoEventPack bingoEventPack = null;

        public IEnumerator CoLoadAssetBundle()
        {
            string assetName = $"bingo_event_ingame_{ManagerBingoEvent.instance.resourceIndex}";

            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path =
                    $"Assets/5_OutResource/BingoEvent/{assetName}/BingoEventPack.prefab";
                GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (bundleObj.GetComponent<BingoEventPack>() != null)
                    bingoEventPack = bundleObj.GetComponent<BingoEventPack>();
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
                        GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"BingoEventPack");
                        
                        if (bundleObj.GetComponent<BingoEventPack>() != null)
                            bingoEventPack = bundleObj.GetComponent<BingoEventPack>();
                    }
                }
                
                NetworkLoading.EndNetworkLoading();
            }
            yield return null;
        }
    }
}
