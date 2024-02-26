using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerWorldRanking : MonoBehaviour
{
    public class Resource
    {
        public WorldRankingPack worldRankingPack = null;

        private int ResourceIndex { 
            get 
            {
                if (contentsData != null && contentsData.ResourceIndex > 0)
                {
                    return contentsData.ResourceIndex;
                }
                else
                {
                    const int DEFAULT_RESOURCE_INDEX = 1;

                    return DEFAULT_RESOURCE_INDEX;
                }
            }
        }

        #region 월드랭킹 리소스 다운로드
        public IEnumerator LoadWorldRankingResource()
        {
            if (Global.LoadFromInternal)
                LoadFromInternal_WorldRankingPack();
            else
                yield return LoadFromBundle_WorldRankingPack();
        }

        private void LoadFromInternal_WorldRankingPack()
        {
            #if UNITY_EDITOR
            string bundleName = string.Format("worldRanking_{0}", ResourceIndex);
            string path = "Assets/5_OutResource/worldRankings/" + bundleName + "/WorldRankingPack.prefab";
            GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            worldRankingPack = bundleObj.GetComponent<WorldRankingPack>();
            #endif
        }

        private IEnumerator LoadFromBundle_WorldRankingPack()
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
            string bundleName = string.Format("w_rank_{0}", ResourceIndex);
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject bundleObj = assetBundle.LoadAsset<GameObject>("WorldRankingPack");
                    worldRankingPack = bundleObj.GetComponent<WorldRankingPack>();
                }
            }
            NetworkLoading.EndNetworkLoading();
        }
        #endregion
    }
}
