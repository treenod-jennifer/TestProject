using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EditorAssetBundlesWindow : EditorWindow
{
    public  EditorAssetBundlesWindow window = null;

  /*  [MenuItem("Window/Package Tool")]
    public static void OpenWindow()
    {
        EditorAssetBundlesWindow window = EditorWindow.GetWindow<EditorAssetBundlesWindow>("Package Tool", true);
    }*/

   /* [MenuItem("Poko/Build Asset Bundles")]
    static void BuildAllAssetBundles()
    {
        // 나중에는 타켓 플렛폼마다 폴더를 만들고 에셋번들을 각각 올려야함 , aos,ios
        string assetBundleDirectory = Global.assetBundleDirectory;
        if (!System.IO.Directory.Exists(assetBundleDirectory))
        {
            System.IO.Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, 0, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("에셋번들 만들기 성공");
    }
    */

       
#if UNITY_EDITOR
	public static string GetPlatformFolderForAssetBundles(BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
#if (!UNITY_2018_1_OR_NEWER)
		case BuildTarget.WebPlayer:
			return "WebPlayer";
#endif
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return "Windows";
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
#if (!UNITY_2018_1_OR_NEWER)
		case BuildTarget.StandaloneOSXUniversal:
			return "OSX";
#endif
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
            default:
			return null;
		}
	}
#endif
    private void OnSelectionChange()
    {
        Repaint();
    }
    private void OnGUI()
    {
        ShowToolbarMenu();
        EditorGUILayout.Space();
        ShowBuildPackages();
      //  ShowBuildTargets();

        ShowUploadTab();
        PackageSelectorHelper.ShowSelectablePackages();


        GUILayout.FlexibleSpace();
    }
    private void OnFocus()
    {
        LoadPrefs();
    }
    private void LoadPrefs()
    {
        PackageSelectorHelper.BuildPackageSelectorHierarchy();
      //  AssetBundleHelper.UpdateMenuOptions();
    }
    private void ShowToolbarMenu()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Package Manager", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }
    private static Color32 buildPackagesButtonColor = new Color32(166, 191, 225, 255);

    private void ShowBuildPackages()
    {
        GUI.tooltip = "Incrementally build all packages.";
        GUI.backgroundColor = buildPackagesButtonColor;
        if (GUILayout.Button(EditorHelper.Label("Build Packages"), GUILayout.Height(32.0f)))
        {
         //   for (int i = 0; i < SettingAssetBundles.Instance.targets.Count; i++)
            {
              //  if (SettingAssetBundles.Instance.targets[i].enabled)
                SettingAssetBundles.Target target = new SettingAssetBundles.Target();
                target.buildTarget = BuildTarget.Android;
                target.buildTarget = BuildTarget.StandaloneWindows64;
                BuildAssetBundles(target, false);
            }

      /*      {
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(Global.assetBundleDirectory, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
                AssetDatabase.Refresh();
            }
            {
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(Global.assetBundleDirectory + "/android", BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.Android);
                AssetDatabase.Refresh();
            }

            */
         //   AssetDatabase.RemoveAssetBundleName("common1",true);
        //    AssetImporter.GetAtPath("Assets/4_OutResource/Common_2").assetBundleName = "common_2";
          
        //    BuildAssetBundles(false);
        }
        GUI.backgroundColor = Color.white;

        GUI.tooltip = "Force all packages to be rebuilt.";
        if (GUILayout.Button(EditorHelper.Label("Force Rebuild"), GUILayout.Height(24.0f)))
        {
            string dialog = "Are you sure you want to rebuild all packages?\nIt may take a while";
            if (EditorUtility.DisplayDialog("Force Rebuild", dialog, "Rebuild", "Cancel"))
            {
                SettingAssetBundles.Target target = new SettingAssetBundles.Target();
                target.buildTarget = BuildTarget.Android; //BuildTarget.Android;
                target.buildTarget = BuildTarget.StandaloneWindows64;
                BuildAssetBundles(target, true);

             /*   if (CheckDifferentSelectedBuildTarget())
                {
                    AssetDatabase.DeleteAsset(BuilderHelper.GetRecentlyBuiltFolderPath(true));
                    AssetDatabase.DeleteAsset(BuilderHelper.GetRecentlyBuiltFolderPath(false));
                    AssetDatabase.Refresh();

                    ForEachSelectedBuildTarget(target => PackageSettingsHelper.BuildAssetBundles(target, true));
                    AssetDatabase.Refresh();
                }*/
            }
        }
    }
    public static void BuildAssetBundles(SettingAssetBundles.Target target, bool forceRebuild)
    {
        string buildFolder = Global.assetBundleDirectory;// +"/" + GetPlatformFolderForAssetBundles(target.buildTarget);
        if (!System.IO.Directory.Exists(buildFolder))
            System.IO.Directory.CreateDirectory(buildFolder);

      //  buildFolder = "Assets/Resources/Cdn/AssetBundles";



        BuildAssetBundleOptions options = forceRebuild ? BuildAssetBundleOptions.ForceRebuildAssetBundle : BuildAssetBundleOptions.None;
        options |= BuildAssetBundleOptions.UncompressedAssetBundle;    // for File Loading Speed
        
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildFolder, options, target.buildTarget);
        
        AssetDatabase.Refresh();

        if (manifest != null)
        {
            int assetBundleCount = manifest.GetAllAssetBundles().Length;
            string isLocalText = target.useStreamingAssets ? " (<b>using StreamingAssets</b>)" : "";
            Debug.LogFormat("<b>{0}</b> AssetBundles are ready for the <b>{1}</b> platform{2}.", assetBundleCount, target.buildTarget, isLocalText);
        }
    }
    private static ReorderableList targetList;
    private static bool showTargetList = true;
    private void ShowBuildTargets()
    {
        if (targetList == null)
        {
            targetList = new ReorderableList(SettingAssetBundles.Instance.targets, typeof(SettingAssetBundles.Target));
            targetList.drawHeaderCallback += DrawTargetListHeader;
            targetList.drawElementCallback += DrawTargetListElement;
        }

        if (targetList != null)
        {
            if (showTargetList)
            {
                targetList.DoLayoutList();
            }
            else
            {
                GUI.tooltip = "Choose in which platform the packages are going to be used.";
                showTargetList = GUILayout.Toggle(showTargetList, EditorHelper.Label("Build Targets"), EditorHelper.Styles.Minimized);
            }
        }
        
    }
    private static void DrawTargetListHeader(Rect rect)
    {
        GUI.tooltip = "Choose in which platform the packages are going to be used.";
        showTargetList = GUI.Toggle(rect, showTargetList, EditorHelper.Label("Build Targets"), EditorStyles.label);
    }

    private static void DrawTargetListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        float enabledToggleWidth = 16.0f;
        float localToggleWidth = 48.0f;

        Rect enabledToggleRect = new Rect(rect);
        enabledToggleRect.width = enabledToggleWidth;
        Rect popupRect = new Rect(rect);
        popupRect.xMin = enabledToggleRect.xMax;
        popupRect.xMax -= localToggleWidth;
        Rect localToggleRect = new Rect(rect);
        localToggleRect.xMin = popupRect.xMax;
        localToggleRect.height = 16.0f;

        SettingAssetBundles.Target target = SettingAssetBundles.Instance.targets[index];

        EditorGUI.BeginChangeCheck();

        target.enabled = EditorGUI.Toggle(enabledToggleRect, target.enabled);
        target.buildTarget = (BuildTarget)EditorGUI.EnumPopup(popupRect, target.buildTarget);
        target.useStreamingAssets = GUI.Toggle(localToggleRect, target.useStreamingAssets, "Local", EditorStyles.miniButton);

        if (EditorGUI.EndChangeCheck())
        {
            SettingAssetBundles.Instance.Save();
        }
    }
    private static Vector2 scroll = Vector2.zero;
    private static PackageSelectorNode packageSelectorRoot = new PackageSelectorNode();
    public static void ShowSelectAllButton(SettingPackageSelector selector, string[] allAssetBundles, string label, bool isSelected)
    {
        if (GUILayout.Button(label, EditorStyles.toolbarButton))
        {
            foreach (string assetBundle in allAssetBundles)
            {
                selector[assetBundle] = isSelected;
            }
        }
    }
    private void ShowUploadTab()
    {
   //     Uploader.Get<AmazonS3Uploader, RemotePackageManagerWindow>().ShowSettingsInspector(UploaderSettingsHelper.UploadRecentlyBuiltButton);
   //     Uploader.Get<FtpUploader, RemotePackageManagerWindow>().ShowSettingsInspector(UploaderSettingsHelper.UploadRecentlyBuiltButton);
    }
  /*  private void ShowPackages()
    {
        SettingPackageSelector selector = SettingPackageSelector.GetInstance();

        string[] allAssetBundles = AssetDatabase.GetAllAssetBundleNames();


        EditorHelper.BeginBoxHeader();

        EditorGUILayout.LabelField("Packages", GUILayout.Width(64.0f));
        GUILayout.FlexibleSpace();


        
        ShowSelectAllButton(selector, allAssetBundles, "Select All", true);
        ShowSelectAllButton(selector, allAssetBundles, "Clear", false);
        scroll = EditorHelper.EndBoxHeaderBeginContent(scroll);
        // 
        foreach (PackageSelectorNode node in packageSelectorRoot.children)
        {
            PackageSelectorNode.Show(selector, node, 0);
        }


        if (EditorHelper.EndBox())
        {
            selector.Save();
        }
    }*/
}