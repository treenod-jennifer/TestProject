using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerTurnRelay : MonoBehaviour
{
    public class TurnRelayResource
    {
        internal const string TURNRELAY_CUTSCENE_PLAYED = "TR_CUTSCENE_PLAYED";
        public TurnRelayPack turnRelayPack = null;

        private int ResourceIndex
        {
            get
            {
                if (ManagerTurnRelay.instance.ResourceIndex > 0)
                {
                    return ManagerTurnRelay.instance.ResourceIndex;
                }
                else
                {
                    const int DEFAULT_RESOURCE_INDEX = 1;

                    return DEFAULT_RESOURCE_INDEX;
                }
            }
        }

        #region 턴 릴레이 리소스 다운로드
        public IEnumerator LoadTurnRelayResource()
        {
            if (Global.LoadFromInternal)
                LoadFromInternal_TurnRelayPack();
            else
                yield return LoadFromBundle_TurnRelayPack();
        }

        private void LoadFromInternal_TurnRelayPack()
        {
            #if UNITY_EDITOR
            string bundleName = string.Format("turnrelay_v2_{0}", ResourceIndex);
            string path = "Assets/5_OutResource/turnrelay/" + bundleName + "/TurnRelayPack.prefab";
            GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            turnRelayPack = bundleObj.GetComponent<TurnRelayPack>();
            #endif
        }

        private IEnumerator LoadFromBundle_TurnRelayPack()
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
            string bundleName = string.Format("turnrelay_v2_{0}", ResourceIndex);
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject bundleObj = assetBundle.LoadAsset<GameObject>("TurnRelayPack");
                    turnRelayPack = bundleObj.GetComponent<TurnRelayPack>();
                }
            }
            NetworkLoading.EndNetworkLoading();
        }
        #endregion

        public GameObject GetAreaObject()
        {
            return turnRelayPack?.AreaActiveObj;
        }
    }
}
