using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class ImageRequestableResult
{
	 
	public Texture texture;
	public SpriteDesc spriteDesc;
}

public interface IImageRequestable
{
	void OnLoadComplete(ImageRequestableResult result);
    void OnLoadFailed();
}

// {"cnt": 3, "size": 100, 120, "ints": [100, 120, 10] } //millisec
public class SpriteDesc
{
	[JsonProperty("cnt")]
	public int count;
	[JsonProperty("sz")]
	public List<int> size;
	[JsonProperty("ints")]
	public List<int> intervals;
}

public class LoadingContext
{
	enum ELoadingStage
	{
		Pending,
		Completed,
		Failed
	}

	public LoadingContext(IImageRequestable requester, string path, string fileName) {
		Url = path + fileName;
		Callback = requester;
	}

	
	ELoadingStage stage = ELoadingStage.Pending;
	private IImageRequestable requester;
	private string url;
	private SpriteDesc spriteInfo;
 
	public void SetComplete(bool bSuccess) {
		stage = (bSuccess) ? ELoadingStage.Completed : ELoadingStage.Failed;
	}
	
	public bool IsPending { get { return stage == ELoadingStage.Pending; }}
	
	public bool IsSuccess { get { return stage == ELoadingStage.Completed; }}
	
	public string Url { get; private set; }
	
	public IImageRequestable Callback { get; private set; }

	public void Dispose() {
		Callback = null;
		stage = ELoadingStage.Pending;
	}
}

/*
타임 아웃 관리도 되지만 콜백후 꼭 Destory해야만 한다면 
OnDestory() {
	UIImageLoader.Instance.CancelCallback(this);
}
이렇게 해서 콜백에서 빼내면 예외처리를 하지 않고 제거 된다
*/

public class UIImageLoader : MonoBehaviour
{
	public const float ThisTimeOut = 7.0f;
	

	#region Singleton
	private static UIImageLoader _instance;
	
	private bool isQuitting = false;

	public static UIImageLoader Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<UIImageLoader>();
				if (_instance == null)
				{
					GameObject go = new GameObject(typeof(UIImageLoader).Name);

					//아래 코드때문에 Scene에 등록하지 않은 MonoSingleton객체는 게임이 실행되는 동안 계속 살아있게 됩니다.
					DontDestroyOnLoad(go);
					_instance = go.AddComponent<UIImageLoader>();
				}
			}
			return _instance;
		}
	}
	void OnApplicationQuit () {
		isQuitting = true;		
		DestroySelf();
	}
    public static bool IsNull()
    {
        return _instance == null;
    }
	
	public void OnDestroy () {
		
		_instance = null;
	}

    public static void DestroySelf()
	{
		if (_instance == null || _instance.textureDict == null) {
			return;
		}
		
		foreach (KeyValuePair<string, TextureRef> pair in _instance.textureDict)
		{
			Texture texture = pair.Value.tex;
			if (texture != null) {
				DestroyImmediate(texture, true);
			}
		}
		_instance.textureDict.Clear();

		Destroy(_instance.gameObject);
		_instance = null;
	}
	#endregion

	#region 상수
	private const int MAX_CONCURRENT_REQUEST = 3;
	private const long CACHE_EXPIRE_TIME_IN_SEC = 60 * 60;  //60분
    private const string ENDS_WITH = ".png";
    #endregion

    #region private 멤버 변수

	class TextureRef
	{
		public Texture tex = null;
		public int refCount = 0;
		public string spriteDesc;
	}
	[System.NonSerialized]
    private readonly Dictionary<string, TextureRef> textureDict = new Dictionary<string, TextureRef>(); 
	[System.NonSerialized]
	private HashSet<LoadingContext> loadingContexts = new HashSet<LoadingContext>();
	private int pendingJob = 0;

    Dictionary<string, int> forceReloadRequests = new Dictionary<string, int>();
	
	#endregion

	 
	class ImageLoadRequest
	{
		public string localPath;
		public string cdnPath;
		public string filename;
		public string spriteDesc;
		public bool canSprite;

		public IImageRequestable callback;
		public LoadingContext context;

		public string Key {
			get { return cdnPath + filename; }
		}
	}
	
	private Queue<ImageLoadRequest> requestQueue = new Queue<ImageLoadRequest>();
	 
	private int loadingWebTicks = 0;
	private int loadingLocalTicks = 0;
	
	public void Start() {
		StartCoroutine(LoopImageLoading());
	}
	
	// 큐를 거쳐 다운로드
//	public void LazyLoad(string localPath, string cdnPath, string fileName, IImageRequestable requester)
//	{
//		var req = new ImageLoadRequest() {
//			callback = requester,
//			localPath = localPath,
//			cdnPath =  cdnPath,
//			filename = fileName,
//		};
//		requestQueue.Enqueue(req);
//	}

	public LoadingContext LoadWithSprite(string localPath, string cdnPath, string fileName, IImageRequestable requester) {
		return doLoad(localPath, cdnPath, fileName, requester, true);
	}

	// 병렬 로드나, 큐에 삽입
	public LoadingContext Load(string localPath, string cdnPath, string fileName, IImageRequestable requester) {
		return doLoad(localPath, cdnPath, fileName, requester, false);
	}

    public LoadingContext ForceLoad(string localPath, string cdnPath, string fileName, IImageRequestable requester)
    {
        string reloadKey = cdnPath + fileName;
        if ( !this.forceReloadRequests.ContainsKey(reloadKey) )
        {
            forceReloadRequests.Add(reloadKey, 0);
        }

        forceReloadRequests[reloadKey] = forceReloadRequests[reloadKey] + 1;

        return doLoad(localPath, cdnPath, fileName, requester, false);
    }

    LoadingContext doLoad(string localPath, string cdnPath, string fileName, IImageRequestable requester, bool isSrite) {
		
		var context = new LoadingContext(requester, cdnPath, fileName);
		
		string key = cdnPath + fileName;
		string url = Global._cdnAddress + cdnPath + fileName + ENDS_WITH;
       
		var req = new ImageLoadRequest() {
			callback = requester,
			localPath = localPath,
			cdnPath = cdnPath,
			filename = fileName,
			context = context,
			canSprite = isSrite,
		};

		loadingContexts.Add(context);
		Request(req);
		return context;
	}
	
	bool CancelCallback(IImageRequestable requester) {
		bool bFound = false;
		foreach (var r in requestQueue.ToArray()) {
			if (r.callback == requester) {
				r.callback = null;
				bFound = true;
			}
		}
		return bFound;
	}
 
	public void CancelAndUnload(LoadingContext context) {
		if (isQuitting) {
			return;
		}
		 
		if (context != null) {
			if (context.IsPending) {
				bool canceled = CancelCallback(context.Callback);
			}
			else if (context.IsSuccess) {
				Unload(context.Url, context.Callback);
			}
			context.Dispose();
		}
	}
	
	void Unload(string url, IImageRequestable requester)
	{
		 
		try {
			if (textureDict != null && textureDict.ContainsKey(url)) {
				textureDict[url].refCount--;
				if (textureDict[url].refCount <= 0) {
					Destroy(textureDict[url].tex);
					textureDict[url].tex = null;
					textureDict.Remove(url);
				}
			}
		}
		catch (System.Exception e) {
			Debug.LogError("[exeception] Unload Image : " + e.ToString());
		}
		// CancelCallback(requester);
	}

	IEnumerator LoopImageLoading() {
		while (!isQuitting) {

			if (loadingLocalTicks > 0 || loadingWebTicks > 0) {
				//Debug.Log("** Ticks: " + loadingLocalTicks + " , " + loadingWebTicks);
			}
			 
			if (requestQueue.Count == 0) {
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			
			var req = requestQueue.Dequeue();
			RunImmdediately(req);
		}
	}
	
	private void Request(ImageLoadRequest req)
    {
       
	    if (pendingJob > MAX_CONCURRENT_REQUEST) {
		    requestQueue.Enqueue(req);
	    }
	    else {
		    RunImmdediately(req);
	    }
    }

	void RunImmdediately(ImageLoadRequest req) {
		pendingJob++;
		
		if (IsCachedToMemory(req.Key)) {
			StartCoroutine(LoadFromMemory(req));
		}
		else {
			string filePath = GetFilePath(req.localPath, req.cdnPath + req.filename) + ENDS_WITH;
			FileInfo fileInfo = new FileInfo(filePath);

            bool forceReload = false;
            string reloadKey = req.cdnPath + req.filename;
            if( forceReloadRequests.ContainsKey(reloadKey) )
            {
                forceReload = true;
                forceReloadRequests[reloadKey] = forceReloadRequests[reloadKey] - 1;
                if (forceReloadRequests[reloadKey] <= 0)
                    forceReloadRequests.Remove(reloadKey);
            }
			
			 	
			if (!fileInfo.Exists || IsExpired(fileInfo) || forceReload) {
				StartCoroutine(LoadFromServer(req));
			}
			else {
				StartCoroutine(LoadFromFile(req));
			}
		}
	}

	bool IsExpired(FileInfo fileInfo) {
		//10000000 Ticks == 1 Sec
		long elapsedTimeInSec = ((DateTime.Now.Ticks - fileInfo.LastWriteTime.Ticks) / 10000000);
		return elapsedTimeInSec >= CACHE_EXPIRE_TIME_IN_SEC;
	}

	private IEnumerator LoadSpriteDesc(string url, ImageLoadRequest req, System.Action<string> cb) {
		
		WWW www = new WWW(url + ".txt");
		float timer = 0;
		bool bTimeout = false;
		while (!www.isDone) {
			if (timer > ThisTimeOut) {
				bTimeout = true;
				break;
			}
			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		if (!string.IsNullOrEmpty(www.error) || bTimeout) {
			www.Dispose();
			cb(string.Empty);
		}
		else {
			cb(www.text);
		}
	}

	private IEnumerator LoadFromServer(ImageLoadRequest req) {
		var st = System.Environment.TickCount;
        string url = Global._cdnAddress + req.cdnPath + req.filename + ENDS_WITH;
       	 
		if (req.canSprite) {
			bool bLoadedSpriteDesc = false;
			StartCoroutine(LoadSpriteDesc(url, req, (text) =>
			{
				req.spriteDesc = text;
				bLoadedSpriteDesc = true;
				
			}));
			while (!bLoadedSpriteDesc) yield return null;
		}
		
		WWW www = new WWW(url);
		//WWW www = new WWW(url + "?_tag=" + UnityEngine.Random.Range(1000000,8000000).ToString()); // DISABLE CACHE
		 
		float timer = 0;
		bool bTimeout = false;
		while (!www.isDone) {
			if (timer > ThisTimeOut) {
				bTimeout = true;
				break;
			}
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	
		req.context.SetComplete(false);
		
		if (!string.IsNullOrEmpty(www.error) || bTimeout ) {
			Debug.LogWarning(string.Format("[download] FAILED: {0} - {1}  timeout:{2} ", url, www.error, bTimeout));
			www.Dispose();  // Must IOS
			if (req.callback != null) {
				req.callback.OnLoadFailed();
			}
		}
		else {
			
			try {
				Texture2D texture = www.texture;
				string name = Global.GetHashfromText(req.Key) + ENDS_WITH;
				name = req.localPath + name;
				name = name.Replace(Application.persistentDataPath, "");
			
				if (SafeCallback(req.callback, texture, req.spriteDesc)) {
					NGUITools.Save(name, texture.EncodeToPNG());
					 
					if (!string.IsNullOrEmpty(req.spriteDesc)) {
						NGUITools.Save(name + ".txt", ASCIIEncoding.ASCII.GetBytes(req.spriteDesc));
					}
					
					CacheToMemory(req.Key, texture, req.spriteDesc);
					req.context.SetComplete(true);
				}
			}
			catch (System.Exception e) {
				Debug.LogWarning("UIImageLoader: " + e.ToString());
			}
		}
		pendingJob--;
		
		loadingWebTicks += (System.Environment.TickCount-st);
	}
 
	private IEnumerator LoadFromFile(ImageLoadRequest req) {
		var st = System.Environment.TickCount;
        string url = req.cdnPath + req.filename;
        string filePath = Global.FileUri + GetFilePath(req.localPath, url) + ENDS_WITH;
 
		if (req.canSprite) {
			bool bLoadedSpriteDesc = false;
			StartCoroutine(LoadSpriteDesc(filePath, req, (text) =>
			{
				req.spriteDesc = text;
				bLoadedSpriteDesc = true;
				
			}));
			while (!bLoadedSpriteDesc) yield return null;
		}
		
        WWW www = new WWW(filePath);
		yield return www;

		req.context.SetComplete(false);
		
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.LogWarning("LoadFromFile : " + filePath + www.error);
			www.Dispose();
		}
		else {
			 
			Texture2D texture = www.texture;
			try {
				SafeCallback(req.callback, texture, req.spriteDesc);
				CacheToMemory(req.Key, texture, req.spriteDesc);
				req.context.SetComplete(true);
			}
			catch (SystemException e) {
				Debug.LogException(e);
			}

		}
		pendingJob--;
		loadingLocalTicks += (System.Environment.TickCount-st);
	}
	 
	private IEnumerator LoadFromMemory(ImageLoadRequest req)
	{
		if (textureDict[req.Key].tex != null) {
		
			SafeCallback(req.callback, textureDict[req.Key].tex, textureDict[req.Key].spriteDesc);
			textureDict[req.Key].refCount++;
			req.context.SetComplete(true);
		}
		else {
			req.context.SetComplete(false);
		}
		pendingJob--;
		yield return null;
	}

	private bool IsCachedToMemory(string url)
	{
		if (!textureDict.ContainsKey(url))
			return false;

		return textureDict[url].tex != null;
	}

	private void CacheToMemory(string url, Texture texture, string desc) {
		
		if (texture == null) {
			Debug.LogWarning("[download] Texture Is null: " + url);
			return;
		}
		if (textureDict.ContainsKey(url))
		{
			textureDict[url].refCount ++;
		}
		else {
			texture.hideFlags = 0; // HideFlags.HideAndDontSave;
			var texRef = new TextureRef()
			{
				tex = texture,
				refCount = 1,
				spriteDesc = desc,
			};
			textureDict.Add(url, texRef);
		}	 
	}
	
	#region File
	private string GetFilePath(string localPath, string url)
	{
		string filePath = CreateDataFilePath(localPath, url);
		return filePath;
	}

	static string CreateDataFilePath(string localPath, string fileName)
	{
		if (String.IsNullOrEmpty(fileName))
			return null;
		
		return localPath + Global.GetHashfromText(fileName);
	}

	static void Save(string path, byte[] bytes)
	{
		CreateDirectoryFromFullFilePath(path);
		File.WriteAllBytes(path, bytes);
	}

	static bool CreateDirectoryFromFullFilePath(string filePath)
	{
		string folder = Path.GetDirectoryName(filePath);
		if (String.IsNullOrEmpty(folder))
			return true;

		DirectoryInfo directoryInfo = new DirectoryInfo(folder);
		if (directoryInfo.Exists)
			return true;

		directoryInfo = Directory.CreateDirectory(folder);
		return directoryInfo.Exists;
	}
	#endregion

	 

	bool SafeCallback(IImageRequestable cb, Texture tex, string spriteDesc) {
		try {
			if (cb != null) {
				SpriteDesc sd = null;
				if (!string.IsNullOrEmpty(spriteDesc)) {
					try {
						sd = Newtonsoft.Json.JsonConvert.DeserializeObject<SpriteDesc>(spriteDesc);
					}
					catch (System.Exception) {
					}
				}
				cb.OnLoadComplete(new ImageRequestableResult
				{
					texture = tex,
					spriteDesc = sd,
				});
				return true;
			}
		}
		catch (System.Exception ex) {
			Debug.LogWarning("[download] UIImageLoader Callback Exception: " + ex);
		}
		return false;
	}
}
