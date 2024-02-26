#if !UNITY_4

using System.IO;
using UnityEngine;

/// <summary>
/// RemoteLevel.
///
/// Helps loading levels downloaded from remote packages.
///
/// <seealso cref="RemotePackageRequest"/>
/// </summary>
public class RemoteLevel
{
	/// <summary>
	/// It gets the remote level's name (or path if in the Editor).
	/// </summary>
	public string Name { get; private set; }

	public RemoteLevel( string scenePath )
	{
		Name = scenePath;
	}

	/// <summary>
	/// It loads the remote level just as Application.LoadLevel().
	/// Note that you do *not* need to add the scene to the build settings.
	/// </summary>
	public void Load()
	{
		if( !string.IsNullOrEmpty( Name ) )
		{
#if UNITY_EDITOR
			if( !RemotePackageManager.IsUsingWWW )
				UnityEditor.EditorApplication.LoadLevelInPlayMode( Name );
			else
#endif
				CompatibilityHelper.LoadScene( Path.GetFileNameWithoutExtension( Name ) );
		}
	}

	/// <summary>
	/// It loads the remote level additively just as Application.LoadLevelAdditive().
	/// Note that you do *not* need to add the scene to the build settings.
	/// </summary>
	public void LoadAdditive()
	{
		if( !string.IsNullOrEmpty( Name ) )
		{
#if UNITY_EDITOR
			if( !RemotePackageManager.IsUsingWWW )
				UnityEditor.EditorApplication.LoadLevelAdditiveInPlayMode( Name );
			else
#endif
				CompatibilityHelper.LoadSceneAdditive( Path.GetFileNameWithoutExtension( Name ) );
		}
	}
}

#endif
