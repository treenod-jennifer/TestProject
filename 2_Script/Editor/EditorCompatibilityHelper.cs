#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define OLD_UNITY
#endif

using UnityEditor;
using UnityEngine;

public static class EditorCompatibilityHelper
{
	public static T LoadAssetAtPath<T>( string path ) where T : Object
	{
#if OLD_UNITY
		return AssetDatabase.LoadAssetAtPath( path, typeof( T ) ) as T;
#else
		return AssetDatabase.LoadAssetAtPath<T>( path );
#endif
	}

	public static GUIStyle CenteredGreyMiniLabel()
	{
#if OLD_UNITY
		return EditorStyles.miniLabel;
#else
		return EditorStyles.centeredGreyMiniLabel;
#endif
	}

	public static bool IsWebPlayer( BuildTarget savedBuildTarget )
	{
#if OLD_UNITY
		return savedBuildTarget == BuildTarget.WebPlayer || savedBuildTarget == BuildTarget.WebPlayerStreamed;
#else
		return false;
#endif
	}
}
