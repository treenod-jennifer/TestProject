using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

//public enum TextureHashLoadType
//{
//    Banner = 1,     //1
//    Costume,
//    IconEvent,
//    IconMaterial,
//    IconMission,    //5
//    IconQuest,
//    Invite,
//    Notice,
//    Pokoyura,
//    Shop,           //10
//}

//public class fileHashData
//{
//    public string fileName = "";
//    public string hash = "";
//}

//public class fileHashDataList
//{
//    public List<fileHashData> hashDataList = new List<fileHashData>();
//}

//public class ManagerTextureHashLoader : MonoBehaviour
//{
//    // 현재 타임아웃 0으로 무한 대기라 기본 값 설정
//    private const int DefaultTimeout = 180;
//    private const int DefaultRetry = 10;

//    public static ManagerTextureHashLoader _instance = null;
//    public Dictionary<string, fileHashDataList> dicHashData = new Dictionary<string, fileHashDataList>();

//    public enum ResultCode
//    {
//        OK,
//        FILELIST_DOWNLOAD_FAILED = -1,
//        ASSET_NOT_EXIST = -2,
//        ASSET_DOWNLOAD_FAILED = -3,
//    }

//    private void Awake()
//    {
//        _instance = this;
//    }

//    private void OnDestroy()
//    {
//        if (_instance == this)
//            _instance = null;
//    }

//    bool IsError(UnityWebRequest req)
//    {
//#if UNITY_2018_1_OR_NEWER
//        return req.isNetworkError;
//#else
//        return req.isError;
//#endif
//    }

//    public IEnumerator TextureHashLoader(System.Action<ResultCode> failCallback = null)
//    {   
//        if(dicHashData.Count == 0)
//        {
//            string filePath = FileHelper.MergePath(NetworkSettings.Instance.GetCDN_URL(), "fileList.json");
//            for (int i = 0; i < DefaultRetry; i++)
//            {
//                UnityWebRequest request = new UnityWebRequest(filePath, UnityWebRequest.kHttpVerbGET);
//                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
//                request.SetRequestHeader("Content-Type", "application/json");
//                request.SetRequestHeader("Accept", "application/json");
//                request.timeout = DefaultTimeout;
//                yield return request.Send();

//                if (IsError(request))
//                {
//                    Debug.LogError("request error and retry: " + request.error);
//                    request.Dispose();

//                    if (failCallback != null)
//                        failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
//                    yield break;
//                }
//                else
//                {
//                    MemoryStream stream = new MemoryStream(request.downloadHandler.data);
//                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);

//                    string readToEnd = reader.ReadToEnd();
//                    reader.Close();
//                    stream.Close();
                    
//                    dicHashData = JsonFx.Json.JsonReader.Deserialize<Dictionary<string, fileHashDataList>>(readToEnd);
//                    break;
//                }
//            }
//            if (dicHashData.Count == 0)
//            {
//                if(failCallback != null)
//                    failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
//                yield break;
//            }
//        }
//        yield return null;
//    }

//    public static void ClearTextureHashData()
//    {
//        if( ManagerTextureHashLoader._instance != null)
//            ManagerTextureHashLoader._instance.dicHashData.Clear();
//    }
//}
