using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ManagerGallery : MonoBehaviour 
{
	public static ManagerGallery _instance = null;

	public delegate void ReturnGalleryImageHandler ( Texture2D imageFile );

#if UNITY_IOS && !UNITY_EDITOR
    /// <summary>
    /// iOS용 카메라롤 등록 플러그인 함수
    /// </summary>
    /// <param name="fileName">File name.</param>
    [DllImport("__Internal")]
    public extern static void CaptureToCameraRoll(String fileName);
    /// <summary>
    /// AOS용 카메라 이미지 처리 플러그인 함수
    /// </summary>
    [DllImport("__Internal")]
    private extern static void RequestCameraImage();
#endif

	void Awake ()
    {
        _instance = this;
    }

	/// <summary>
	/// Launchs the AOS or IOS Gallery.
	/// </summary>
	public void LaunchGallery ()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass ajc = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
		AndroidJavaObject ajo = new AndroidJavaObject( "lgpkpk.linecorp.com.photogalleryplugin.UnityBinder" );
		ajo.CallStatic( "OpenGallery", ajc.GetStatic<AndroidJavaObject>( "currentActivity" ) );

#elif UNITY_IOS && !UNITY_EDITOR
		RequestCameraImage ();
#endif
	}

	/// <summary>
	/// Loads the Image to Gallery.
	/// </summary>
	/// <returns>The image.</returns>
	/// <param name="fileUri">File URI.</param>
	public void LoadImage ( string fileUri, ReturnGalleryImageHandler loadHandler )
	{
		this.StartCoroutine ( this.LoadGalleryPickImage( fileUri, loadHandler ) );
	}
	 

	/// <summary>
	/// Save the imageFile(Byte[]) to AOS or IOS Gallery
	/// </summary>
	/// <param name="imageFile">Image file.</param>
	/// <param name="fileName">File name.</param>
	public void SaveImage ( byte[] imageFile , string fileName )
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		File.WriteAllBytes(  this.GetAndroidInternalFilesDir( fileName ), imageFile );

        AndroidJavaClass classPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );

        AndroidJavaClass classUri = new AndroidJavaClass( "android.net.Uri" );
        AndroidJavaObject objIntent = new AndroidJavaObject( "android.content.Intent", new object[2] { "android.intent.action.MEDIA_SCANNER_SCAN_FILE", classUri.CallStatic<AndroidJavaObject>( "parse", "file://" + this.GetAndroidInternalFilesDir( fileName ) ) } );
        objActivity.Call( "sendBroadcast", objIntent );
        //Application.OpenURL( this.GetAndroidInternalFilesDir( fileName ) );
#elif UNITY_IOS && !UNITY_EDITOR
		File.WriteAllBytes( this.GetDocumentsPathIPhone(fileName), imageFile );
		Application.OpenURL(this.GetDocumentsPathIPhone(fileName) );
		CaptureToCameraRoll(String.Format ("/{0}", fileName ));
#endif
    }

	/// <summary>
	/// Gets the android documents path.
	/// </summary>
	/// <returns>The android documents path.</returns>
	/// <param name="isDefaultLocation">If set to <c>true</c> is default location.</param>
	/// <param name="fileName">File name.</param>
    public string GetAndroidInternalFilesDir ( string fileName = "MyStamp" )
    {
        string fileDir = "";
        string[] potentialDirectories = new string[]
        {
            "/mnt/sdcard",
            "/sdcard",
            "/storage/sdcard0",
            "/storage/sdcard1",
            "/storage/emulated/0"
        };

        if ( Application.platform == RuntimePlatform.Android )
        {
            bool isFindPath = false;
            for ( int i = 0; i < potentialDirectories.Length; i++ )
            {
                if ( Directory.Exists( potentialDirectories[i] ) )
                {
                    fileDir = potentialDirectories[i] + "/DCIM/" + fileName + ".png";
                    isFindPath = true;
                    break;
                }
            }

            if ( isFindPath == false )
            {
                AndroidJavaClass ajc = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
                AndroidJavaObject ajo = new AndroidJavaObject( "lgpkpk.linecorp.com.photogalleryplugin.UnityBinder" );
                fileDir = ajo.CallStatic<String>( "GetInternalStorageRoot" );

                if ( Directory.Exists( fileDir ) )
                {
                    fileDir = string.Concat( fileDir, "/DCIM/", fileName, ".png" );
                }
            }
        }
        return fileDir;
    }

	/// <summary>
	/// 아이폰 파일 저장경로를 가져온다 
	/// </summary>
	/// <returns>The I phone documents path.</returns>
    private string GetDocumentsPathIPhone (string fileName)
	{
		return (Application.persistentDataPath + "/" + fileName + ".png");
	}

	private IEnumerator LoadGalleryPickImage ( string fileUri, ReturnGalleryImageHandler loadHandler )
	{
		//Debug.Log( fileUri );

		WWW www = new WWW( Global.FileUri + fileUri );

		yield return www;

		if ( www != null && www.isDone )
		{
			Texture2D galleryImage = new Texture2D( www.texture.width, www.texture.height );
			galleryImage.SetPixels32( www.texture.GetPixels32() );
			galleryImage.Apply();

			www.LoadImageIntoTexture( galleryImage );

			loadHandler(galleryImage);
			www = null;
		}

		yield break;
	}
}
