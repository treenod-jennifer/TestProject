using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
    private const string DEVELOP_BUILD_TARGET_PATH = "C:/Build"; // "C:/Users/Vic/Documents/pokopuzzle/";   //C:\Users\Vic\Documents\pokopuzzle

    [MenuItem("Poko/Stage/Android_Build")]
    private static void PerformAndroidDevelopBuild()
    {
        UpdateKeyStoreData();
        PlayerSettings.productName = "VilligeOfFlower";
        PlayerSettings.applicationIdentifier = "com.linecorp.LGPKV";

        string[] scenes = GetEnabledScenes();
        string buildFileName = "villigeOfFlower.apk";

        Texture2D icon = Resources.Load("Icon/BuildIcon") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);


        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + "/" + buildFileName, BuildTarget.Android, BuildOptions.None);
    }


    [MenuItem("Poko/Stage/Android_Tool")]
    private static void PerformAndroidToolBuild()
    {
        UpdateKeyStoreData();
        PlayerSettings.productName = "레벨툴";
        PlayerSettings.applicationIdentifier = "com.treenod.puzzleTool";

        Texture2D icon = Resources.Load("Icon/ToolIcon") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);

        string[] scenes = GetEnabledScenesTool();
        string buildFileName = "puzzleTool" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".apk";
        GenericAndroidBuild(scenes, DEVELOP_BUILD_TARGET_PATH + "/" + buildFileName, BuildTarget.Android, BuildOptions.None);
    }

    static void GenericAndroidBuild(string[] scenes, string buildFileName, BuildTarget buildTarget, BuildOptions buildOptions, string path = "")
    {
        Texture2D icon = Resources.Load("Icon/BuildIcon") as Texture2D;
        Texture2D[] icons = new Texture2D[] { icon, icon, icon, icon };
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);

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
            if (scene.enabled)
            {
                continue;
            }
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }


    #endregion
}
