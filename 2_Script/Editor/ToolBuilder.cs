using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

public class ToolBuilder : MonoBehaviour {


    #region 툴빌드메뉴

    //const string BUILD_FILE_NAME = "PokoLevelTool";
    const string KEYSTORE_NAME = "pokopuzzle.keystore";
    const string KEYSTORE_PASSWORD = "xmflshem";
    const string ALIAS_NAME = "pokopuzzle";
    const string ALIAS_PASSWORD = "xmflshem";

    static string toolVersion = "0.0.1 ";
    //private const string DEVELOP_BUILD_TARGET_PATH = "C:/Build"; // "C:/Users/Vic/Documents/pokopuzzle/";   //C:\Users\Vic\Documents\pokopuzzle

    public static string DEVELOP_BUILD_TARGET_PATH{
        get{ return Path.GetFullPath(".") + Path.DirectorySeparatorChar; }
    }

    [MenuItem("Poko/Stage/Real_Tool")]
    private static void PerformAndroidDevelopBuild()
    {
        /*
        UpdateKeyStoreData();
        PlayerSettings.productName = "VilligeOfFlower";
        PlayerSettings.applicationIdentifier = "com.linecorp.LGPKV";

        string[] scenes = GetEnabledScenes();
        string buildFileName = "villigeOfFlower.apk";

        Texture2D icon = Resources.Load("Icon/BuildIcon") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);


        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + "/" + buildFileName, BuildTarget.Android, BuildOptions.None);*/

        UpdateKeyStoreData();
        PlayerSettings.productName = DateTime.Now.ToString("MMdd_HHmm") + "_리얼툴";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.treenod.RealTool");

        ToolSettings.Instance.toolType = ToolSettings.ToolType.TOOL_REAL;

        Texture2D icon = Resources.Load("ToolIcon/RealTool") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

        string[] scenes = GetEnabledScenesTool();
        //string buildFileName = "PKT_RealTool" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".apk";
        string buildFileName = "PKT_RealTool.apk";
        Debug.Log("BuildPath: " + DEVELOP_BUILD_TARGET_PATH + buildFileName);
        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + buildFileName, BuildTarget.Android, BuildOptions.None);

    }


    [MenuItem("Poko/Stage/Dev_Tool")]
    private static void PerformAndroidToolBuild()
    {
        UpdateKeyStoreData();
        PlayerSettings.productName = DateTime.Now.ToString("MMdd_HHmm") + "_개발툴";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.treenod.DevTool");

        ToolSettings.Instance.toolType = ToolSettings.ToolType.TOOL_DEV;

        Texture2D icon = Resources.Load("ToolIcon/DevTool") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

        string[] scenes = GetEnabledScenesTool();
        //string buildFileName = "PKT_DevTool" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".apk";
        string buildFileName = "PKT_DevTool.apk";
        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + buildFileName, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Poko/Stage/Test_Tool")]
    private static void PerformAndroidToolBuildTest()
    {
        UpdateKeyStoreData();
        PlayerSettings.productName = DateTime.Now.ToString("MMdd_HHmm") + "_기믹툴";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.treenod.TestTool");

        ToolSettings.Instance.toolType = ToolSettings.ToolType.TOOL_TEST;

        Texture2D icon = Resources.Load("ToolIcon/TestTool") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

        string[] scenes = GetEnabledScenesTool();
        //string buildFileName = "PKT_TestTool" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".apk";
        string buildFileName = "PKT_TestTool.apk";
        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + buildFileName, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Poko/Stage/Ext_Tool")]
    private static void PerformAndroidToolBuildExternal()
    {
        UpdateKeyStoreData();
        PlayerSettings.productName = DateTime.Now.ToString("MMdd_HHmm") + "_외부툴";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.treenod.ExtTool");

        ToolSettings.Instance.toolType = ToolSettings.ToolType.TOOL_EXT;

        Texture2D icon = Resources.Load("ToolIcon/ExtTool") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

        string[] scenes = GetEnabledScenesTool();
        //string buildFileName = "PKT_ExtTool" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".apk";
        string buildFileName = "PKT_ExtTool.apk";
        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + buildFileName, BuildTarget.Android, BuildOptions.None);
    }
    
    [MenuItem("Poko/Window/Encryption_Tool")]
    private static void PerformWindowBuildEncryptionTool()
    {
        var name = DateTime.Now.ToString("MMdd_HHmm") + "_EncryptionTool";
        PlayerSettings.productName = name;
        
        PlayerSettings.fullScreenMode          = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth      = 640;
        PlayerSettings.defaultScreenHeight     = 480;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
        PlayerSettings.usePlayerLog            = false;

        Texture2D   icon  = Resources.Load<Texture2D>("ToolIcon/EncryptionTool");
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
        
        string   path   = DEVELOP_BUILD_TARGET_PATH + "/build";
        string[] scenes = { "Assets/1_Scene/MapTool.unity" };
        
        CreateFolder(path);
        
        if (Directory.Exists(path))
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.GetFiles().ToList().ForEach(f => f.Delete());
            directory.GetDirectories().ToList().ForEach(d => d.Delete(true));
        }

        GenericBuild(scenes, path + $"/{name}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

        CreateFolder(path + "/Stage");

        void CreateFolder(string path)
        {
            var sDirPath = path;
            var di       = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }
        }
    }


    static void GenericAndroidBuild(string[] scenes, string buildFileName, BuildTarget buildTarget, BuildOptions buildOptions, string path = "")
    {
        /*
        Texture2D icon = Resources.Load("ToolIcon/ToolIcon") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
        */
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget);

#if UNITY_2018_1_OR_NEWER
        var res = BuildPipeline.BuildPlayer(scenes, buildFileName, buildTarget, buildOptions);
        if (res.summary.result != BuildResult.Succeeded)
        {
            Debug.Log("Debug BuildPlayer failure : " + res);
            throw new Exception("BuildPlayer failure: " + res);
        }
#else
        string res = BuildPipeline.BuildPlayer(scenes, buildFileName, buildTarget, buildOptions);

        if (res.Length > 0)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
#endif
    }

    private static void GenericBuild(string[] scenes, string buildFileName, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
        
        var report = BuildPipeline.BuildPlayer(scenes, buildFileName, buildTarget, buildOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("BuildPlayer failure: " + report.summary.totalErrors);
        }
    }
    
    static void UpdateKeyStoreData()
    {
        PlayerSettings.Android.keystoreName = KEYSTORE_NAME;
        PlayerSettings.Android.keystorePass = KEYSTORE_PASSWORD;
        PlayerSettings.Android.keyaliasName = ALIAS_NAME;
        PlayerSettings.Android.keyaliasPass = ALIAS_PASSWORD;
    }
    
    private static string[] GetEnabledScenes()
    {
        var editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
            {
                continue;
            }
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }

    //꺼진 툴씬만 추가
    private static string[] GetEnabledScenesTool()
    {
        var editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            /*
            if (scene.enabled)
            {
                continue;
            }*/
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }


    #endregion
}
