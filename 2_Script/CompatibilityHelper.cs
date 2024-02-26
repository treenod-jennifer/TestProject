#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define OLD_UNITY
#endif

using UnityEngine;

#if !OLD_UNITY
using UnityEngine.SceneManagement;
#endif

public static class CompatibilityHelper
{
	public static void LoadScene( string sceneName )
	{
#if OLD_UNITY
		Application.LoadLevel( sceneName );
#else
		SceneManager.LoadScene( sceneName );
#endif
	}

	public static void LoadSceneAdditive( string sceneName )
	{
#if OLD_UNITY
		Application.LoadLevelAdditive( sceneName );
#else
		SceneManager.LoadScene( sceneName, LoadSceneMode.Additive );
#endif
	}
}
