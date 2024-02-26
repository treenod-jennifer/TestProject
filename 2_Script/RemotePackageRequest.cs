using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RemotePackageRequest.
///
/// Helper object to handle package load requests.
///
/// You never have to instantiate it manually as an instance to it
/// is already returned from RemotePackageManager's "Load" method!
///
/// <example>
/// See RemotePackageManager example to see this in use since this
/// class depends on it!
/// </example>
/// </summary>
///
/// <seealso cref="RemotePackageManager"/>
public class RemotePackageRequest
{
	private System.Action<Object, ICollection<Object>, string[]> requestCallback;
	private System.Action requestErrorCallback;
	private System.Action<float> downloadProgressCallback;

	public bool HasDownloadProgressCallback { get { return downloadProgressCallback != null; } }

	public string PackageUri { get; private set; }

	public RemotePackageRequest( string packageUri )
	{
		PackageUri = packageUri;
	}

	/// <summary>
	/// Filter the downloaded Objects and get the first of type "T".
	/// Then it calls the callback delegate passing it as the parameter.
	/// </summary>
	/// <param name='callback'>
	/// Callback to be called with the filtered Object after the download.
	/// </param>
	/// <typeparam name='T'>
	/// Filter the downloaded Objects by type (GameObject, Texture, AudioClip, etc.)
	/// </typeparam>
	///
	/// <seealso cref="GetAll"/>
	public RemotePackageRequest Get<T>( System.Action<T> callback ) where T : Object
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			if( callback == null )
				return;

			if( mainAsset is T )
				callback( mainAsset as T );
			else if( remotePackage != null )
				callback( remotePackage.OfType<T>().FirstOrDefault() );
		};

		return this;
	}

	/// <summary>
	/// Filter the downloaded Objects and get the first of type "T" and if name "objectName".
	/// Then it calls the callback delegate passing it as the parameter.
	/// </summary>
	/// <param name='objectName'>
	/// Filter the downloaded Objects by its name.
	/// </param>
	/// <param name='callback'>
	/// Callback to be called with the filtered Object after the download.
	/// </param>
	/// <typeparam name='T'>
	/// Filter the downloaded Objects by type (GameObject, Texture, AudioClip, etc.)
	/// </typeparam>
	///
	/// <seealso cref="GetAll"/>
	public RemotePackageRequest Get<T>( string objectName, System.Action<T> callback ) where T : Object
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			if( callback == null )
				return;

			if( mainAsset is T && mainAsset.name == objectName )
				callback( mainAsset as T );
			else if( remotePackage != null )
				callback( remotePackage.OfType<T>().FirstOrDefault( o => o.name == objectName ) );
		};

		return this;
	}

	/// <summary>
	/// Filter the downloaded Objects and get all of type "T".
	/// Then it calls the callback delegate passing them as the parameter.
	///
	/// Don't forget to import "System.Collections.Generic"!
	/// <code>
	/// using System.Collections.Generic;
	/// </code>
	/// </summary>
	/// <param name='callback'>
	/// Callback to be called with the filtered Objects after the download.
	/// </param>
	/// <typeparam name='T'>
	/// Filter the downloaded Objects by type (GameObject, Texture, AudioClip, etc.)
	/// </typeparam>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest GetAll<T>( System.Action<ICollection<T>> callback ) where T : Object
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			if( callback != null && remotePackage != null )
				callback( remotePackage.OfType<T>().ToArray() );
		};

		return this;
	}

	/// <summary>
	/// Filter the downloaded Objects and get the first of type GameObject (Prefab) and then instantiates it.
	/// Then it calls the callback delegate passing the instantiated GameObject as the parameter.
	/// </summary>
	/// <param name='parent'>
	/// Parent transform to instantiate the downloaded Prefab as its child.
	/// </param>
	/// <param name='callback'>
	/// Callback to be called with the instantiated GameObject after the download and instantiation.
	/// </param>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest GetAndInstantiate( Transform parent = null, System.Action<GameObject> callback = null )
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			GameObject prefab = null;

			if( mainAsset is GameObject )
				prefab = mainAsset as GameObject;
			else if( remotePackage != null )
				prefab = remotePackage.OfType<GameObject>().FirstOrDefault();

			if( prefab != null )
			{
				GameObject go = Object.Instantiate( prefab ) as GameObject;

				go.transform.parent = parent;
				go.transform.localPosition = Vector3.zero;

				if( callback != null )
					callback( go );
			}
		};

		return this;
	}

	/// <summary>
	/// Filter the downloaded Objects and get the first of type GameObject (Prefab) and name "objectName"
	/// and then instantiates it. Then it calls the callback delegate passing the instantiated GameObject
	/// as the parameter.
	/// </summary>
	/// <param name='objectName'>
	/// Filter the downloaded Objects by its name.
	/// </param>
	/// <param name='parent'>
	/// Parent transform to instantiate the downloaded Prefab as its child.
	/// </param>
	/// <param name='callback'>
	/// Callback to be called with the instantiated GameObject after the download and instantiation.
	/// </param>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest GetAndInstantiate( string objectName, Transform parent = null, System.Action<GameObject> callback = null )
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			GameObject prefab = null;

			if( mainAsset is GameObject && mainAsset.name == objectName )
				prefab = mainAsset as GameObject;
			else if( remotePackage != null )
				prefab = remotePackage.OfType<GameObject>().FirstOrDefault( o => o.name == objectName );

			if( prefab != null )
			{
				GameObject go = Object.Instantiate( prefab ) as GameObject;

				go.transform.parent = parent;
				go.transform.localPosition = Vector3.zero;

				if( callback != null )
					callback( go );
			}
		};

		return this;
	}

#if !UNITY_4

	/// <summary>
	/// Get the first scene inside the downloaded package.
	/// Then it calls the callback delegate passing it as the parameter.
	///
	/// <example>
	/// <code>
	/// RemotePackageManager.Load( "Your/LevelPackage/Name" ).GetLevel( l => l.Load() );
	/// </code>
	/// </example>
	///
	/// </summary>
	/// <param name='callback'>
	/// Callback to be called with the downloaded scene.
	/// </param>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest GetLevel( System.Action<RemoteLevel> callback )
	{
		requestCallback += delegate ( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths ) {
			if( callback != null )
			{
				string scenePath = scenePaths != null ? scenePaths[0] : null;
				callback( new RemoteLevel( scenePath ) );
			}
		};

		return this;
	}

#endif

	/// <summary>
	/// Called if an error occurred while trying to download the package.
	/// </summary>
	/// <param name='callback'>
	/// Callback to be called when there is an error.
	/// </param>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest OnError( System.Action callback )
	{
		if( callback != null )
			requestErrorCallback += callback;

		return this;
	}

	/// <summary>
	/// Get the download progress as a callback. The callback is a float that is in the
	/// range 0..1. 0 is the download just started and 1, the download is completed.
	/// </summary>
	/// <param name='callback'>
	/// Callback to be called with the filtered Object after the download.
	/// </param>
	///
	/// <seealso cref="Get"/>
	public RemotePackageRequest OnDownloadProgress( System.Action<float> callback )
	{
		if( callback != null )
			downloadProgressCallback += callback;

		return this;
	}

	public void SetRemotePackage( Object mainAsset, ICollection<Object> remotePackage, string[] scenePaths )
	{
		if( requestCallback != null )
			requestCallback( mainAsset, remotePackage, scenePaths );
	}

	public void RaiseError()
	{
		if( requestErrorCallback != null )
			requestErrorCallback();
	}

	public void SetDownloadProgress( float progress )
	{
		if( HasDownloadProgressCallback )
			downloadProgressCallback( progress );
	}
}
