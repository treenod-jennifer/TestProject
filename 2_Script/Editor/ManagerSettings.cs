using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ManagerSettings : ScriptableObject
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

#if !UNITY_4
//	public string exportedAssetBundlesFolder = "Assets/RemotePackageManager/AssetBundles_RecentlyBuilt";
    public string exportedAssetBundlesFolder = "AssetBundles";
#endif

	public List<Target> targets = new List<Target>();

	public void Save()
	{
		EditorUtility.SetDirty( this );
		AssetDatabase.SaveAssets();
	}

	public static ManagerSettings Instance
	{
		get
		{
			if( instance == null )
			{
                instance = EditorCompatibilityHelper.LoadAssetAtPath<ManagerSettings>( path );

				if( instance == null )
				{
					instance = CreateInstance<ManagerSettings>();
					AssetDatabase.CreateAsset( instance, path );
					AssetDatabase.Refresh();
				}
			}

			return instance;
		}
	}

    private const string path = "Assets/2_Script/Editor/ManagerSettings.asset";
	private static ManagerSettings instance = null;
}
