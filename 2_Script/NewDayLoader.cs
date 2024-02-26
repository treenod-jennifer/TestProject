using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDayLoader : MonoBehaviour
{
    public event System.Action<long, long> downLoadEvent;

    public IEnumerator LoadNewDayResource()
    {
        int minMission = int.MaxValue;
        foreach (var item in ManagerData._instance._missionData)
        {
            if (item.Value.day == Global.day)
            {
                if (minMission > item.Key)
                    minMission = item.Key;
            }
        }

        MissionData mission = ManagerData._instance._missionData[minMission];
        int landIndex = 0;
        var landMap = ServerContents.Day.CreateAreaToLandMap();
        if( landMap.TryGetValue(mission.sceneArea, out landIndex))
        {
            ManagerLobby.landIndex = landIndex;
            ManagerLobby.transLand = true;
        }

        List<string> areaNameList = landIndex == 0 ? ServerContents.Day.GetString() : ServerContents.Day.outlands[landIndex];
        long resourceDownloaded = 0;
        long resourceTotal = 0;

        List<ManagerAssetLoader.BundleRequest> bundleLoadList = new List<ManagerAssetLoader.BundleRequest>();
        ManagerAssetLoader.EstimatedLoadResult loadEstimate = new ManagerAssetLoader.EstimatedLoadResult();


        ManagerAssetLoader.assetDataList.Clear();


        for (int i = 0; i < areaNameList.Count; i++)
        {
            if (Global.LoadFromInternal)
            {
            }
            else
            {
                string araName = areaNameList[i];
                ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
                {
                    uri = araName,
                    successCallback = null,
                    failCallback = (r) =>
                    {
                        Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                        ErrorController.ShowResourceDownloadFailed("Download Resource");
                    }
                };
                bundleLoadList.Add(bundleReq);
            }

            yield return null;

        }

        yield return ManagerAssetLoader._instance.EstimateLoad(bundleLoadList, loadEstimate);

        Debug.Log("Estimated: " + loadEstimate.totalDownloadLength);
        resourceTotal = loadEstimate.totalDownloadLength;

        yield return ManagerAssetLoader._instance.ExecuteLoad
        (
            loadEstimate, 
            bundleLoadList,
            (f) =>
            {
                resourceDownloaded = (long)(f * resourceTotal);
                downLoadEvent?.Invoke(resourceDownloaded / 1024, resourceTotal / 1024);
            }
        );
    }
}

