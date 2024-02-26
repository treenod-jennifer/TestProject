using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ResourceType의 경우 번들 네이밍(카멜)을 대문자 + 언더바로 변환하여 선언
/// 임의로 선언할 경우 매개변수를 추가해야 해서 번거로울 수 있음
/// </summary>
public enum ResourceType
{
    NONE,
    SPOT_DIA_SPINE,
};

public class ManagerResourceLoader : MonoBehaviour
{
    public static ManagerResourceLoader instance = null;
    public Dictionary<ResourceType, GameObject> resourceDic = new Dictionary<ResourceType, GameObject>();

    private void Awake() => instance = this;
    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
        NetworkLoading.EndNetworkLoading();

        resourceDic.Clear();
        
        if (instance == this)
        {
            instance = null;
        }
    }
    
    public static void Init()
    {
        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerResourceLoader>();
    }

    public static void OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    private CancellationTokenSource _cts = new CancellationTokenSource();

    public async UniTask<GameObject> AsyncLoadResource(ResourceType resourceType, string bundleName = null, string editorPath = null)
    {
        // 에디터 환경에서 리소스 로드
        if (Global.LoadFromInternal)
        {
            var path = editorPath;
            if (string.IsNullOrEmpty(editorPath))
            {
                path = ToCamel(resourceType.ToString()) + "/" + ToCamel(resourceType.ToString()) + ".prefab";
            }
#if UNITY_EDITOR
            var bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/5_OutResource/{path}");

            if (bundleObj != null)
            {
                if (bundleObj != null)
                {
                    if (resourceDic.ContainsKey(resourceType))
                    {
                        resourceDic[resourceType] = bundleObj;
                    }
                    else
                    {
                        resourceDic.Add(resourceType, bundleObj);
                    }
                }
            }
#endif
        }
        // CDN 환경에서 리소스 로드
        else
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
            var loadName = bundleName;
            if (string.IsNullOrEmpty(bundleName))
            {
                loadName = resourceType.ToString();
            }
            var assetBundle = await ManagerAssetLoader._instance.AsyncAssetBundleLoader(loadName.ToLower(), _cts.Token);
            if (assetBundle != null)
            {
                var bundleObj = assetBundle.LoadAsset<GameObject>(ToCamel(loadName));
                if (bundleObj != null)
                {
                    if (resourceDic.ContainsKey(resourceType))
                    {
                        resourceDic[resourceType] = bundleObj;
                    }
                    else
                    {
                        resourceDic.Add(resourceType, bundleObj);
                    }
                }
            }
            NetworkLoading.EndNetworkLoading();
        }

        GameObject obj = null;
        resourceDic.TryGetValue(resourceType, out obj);
        return obj;
    }

    /// <summary>
    /// 팝업 OFF 후 리소스 할당 해제를 위한 함수
    /// isRemove : 빌드가 켜져있는 동안 수시로 로드가 필요한 리소스의 경우 false, 드물게 로드하는 리소스의 경우 true (ex. 스팟다이아)
    /// </summary>
    public void UnLoadResource(ResourceType resourceType, bool isRemove = false)
    {
        if (resourceDic.ContainsKey(resourceType))
        {
            if (isRemove)
            {
                resourceDic.Remove(resourceType);
            }
            else
            {
                resourceDic[resourceType] = null;
            }
        }
    }

    private string ToCamel(string resourceName)
    {
        var changeStr = "";
        var strArray = resourceName.ToLower().Split('_');
        
        foreach (var str in strArray)
        {
            var tempStr = char.ToUpper(str[0]) + str.Substring(1);
            changeStr += tempStr;
        }
        
        return changeStr;
    }
}
