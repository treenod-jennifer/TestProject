using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class StageUtility : MonoBehaviour
{
    private static StageUtility instance;
    private static StageUtility _instance
    {
        get
        {
            if(instance == null)
            {
                var tempObject = new GameObject();
                tempObject.name = "StageUtility";
                DontDestroyOnLoad(tempObject);

                instance = tempObject.AddComponent<StageUtility>();
            }

            return instance;
        }
    }

    public static StageMapData lastLoadedStageMapData { get; private set; } = null;

    public static void StageMapDataLoad(string stageName, System.Action<StageMapData> completeCallback)
    {
        _instance.StartCoroutine(CoStageMapDataLoad(stageName, completeCallback));
    }

    private static IEnumerator CoStageMapDataLoad(string stageName, System.Action<StageMapData> completeCallback)
    {
        yield return ManagerUI._instance.CoCheckStageData(stageName);

        UnityWebRequest request = new UnityWebRequest(Global.FileUri + Global.StageDirectory + stageName)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        if (!request.isNetworkError)
        {
            MemoryStream stream = new MemoryStream(request.downloadHandler.data);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StageMapData mapData = null;

            try
            {
                var serializer = new XmlSerializer(typeof(StageMapData));
                mapData = serializer.Deserialize(reader) as StageMapData;
            }
            catch (System.Exception e)
            {
#if UNITY_EDITOR
                Debug.LogWarning("암호화 되지 않은 맵을 읽는중입니다! - 실제 기기에서는 에러가 날 수 있습니다");
                string key = StageHelper.filekey;
                key = key.Insert(4, stageName);
                key = key.Substring(0, StageHelper.filekey.Length);

                string encrypted = StageHelper.AESEncrypt256(reader.ReadToEnd(), key);

                MemoryStream memoryStream = new MemoryStream(request.downloadHandler.data);
                var stageInfoSerializer = new XmlSerializer(typeof(StageInfo));
                request.Dispose();
                memoryStream.Position = 0;
                var openStage = stageInfoSerializer.Deserialize(memoryStream) as StageInfo;

                StageMapData stageData = new StageMapData();
                stageData.version = 0;
                stageData.moveCount = openStage.turnCount;
                stageData.collectCount = openStage.collectCount;
                stageData.collectType = openStage.collectType;
                stageData.collectColorCount = openStage.collectColorCount;
                stageData.gameMode = openStage.gameMode;
                stageData.isHardStage = openStage.isHardStage;
                stageData.data = encrypted;

                if (openStage.gameMode == (int)GameMode.ADVENTURE)
                {
                    stageData.bossIdx = openStage.bossInfo.idx;
                    stageData.bossAttr = openStage.bossInfo.attribute;
                }
                mapData = stageData;
#endif
            }

            lastLoadedStageMapData = mapData;

            if(completeCallback != null)
                completeCallback(mapData);
        }
        else
        {
            if (completeCallback != null)
                completeCallback(null);
        }
    }

    public static StageInfo StageInfoDecryption(StageMapData stage)
    {
        string key = StageHelper.filekey;
        key = key.Insert(4, Global.GameInstance.GetStageKey());
        key = key.Substring(0, StageHelper.filekey.Length);
        string sdeData = StageHelper.Decrypt256(stage.data, key);

        StringReader readerStage = new StringReader(sdeData);
        var serializerStage = new XmlSerializer(typeof(StageInfo));

        return serializerStage.Deserialize(readerStage) as StageInfo;
    }
}
