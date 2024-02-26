using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using UnityEditor;

public class ToolSettings : ScriptableObject{

    public enum ToolType
    {
        TOOL_REAL,
        TOOL_DEV,
        TOOL_TEST,
        TOOL_EXT,
    }


    public string buildedTimeString = "";

    private static ToolSettings s_Instance;

	public static ToolSettings Instance {
		get {
			if (s_Instance == null) {
				s_Instance = (ToolSettings)Resources.Load("ToolSettings", typeof(ToolSettings));
				
			}
			return s_Instance;
		}
	}

    public ToolType toolType = ToolType.TOOL_DEV;

    //[MenuItem("Assets/Create/ToolSettings")]
    //public static void CreateMyAsset()
    //{
    //    ToolSettings asset = ScriptableObject.CreateInstance<ToolSettings>();

    //    AssetDatabase.CreateAsset(asset, "Assets/Resources/ToolSettings.asset");
    //    AssetDatabase.SaveAssets();

    //    EditorUtility.FocusProjectWindow();

    //    Selection.activeObject = asset;
    //}
}
