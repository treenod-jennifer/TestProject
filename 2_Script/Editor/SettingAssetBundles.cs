using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SettingAssetBundles : ScriptableObject
{
    [System.Serializable]
    public class Target
    {
        public bool enabled = true;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool useStreamingAssets = false;
    }

    public bool checkForDifferentSelectedBuildTarget = true;
    public bool checkForUpgrade = true;

#if UNITY_5
//    public string exportedAssetBundlesFolder = "Assets/RemotePackageManager/AssetBundles_RecentlyBuilt";
    public string exportedAssetBundlesFolder = "AssetBundles";
#endif

    public List<Target> targets = new List<Target>();

    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public static SettingAssetBundles Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<SettingAssetBundles>(path);

                if (instance == null)
                {
                    instance = CreateInstance<SettingAssetBundles>();
                    AssetDatabase.CreateAsset(instance, path);
                    AssetDatabase.Refresh();
                }
            }

            return instance;
        }
    }

    private const string path = "Assets/2_Script/Editor/SettingAssetBundles.asset";
    private static SettingAssetBundles instance = null;
}
