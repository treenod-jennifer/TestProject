using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PreDownloader
{
    private static HashSet<string> preDownloadList = new HashSet<string>();

    private static bool isDownloading = false;

    public static void AddItem(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            preDownloadList.Add(path);
        }
    }

    public static HashSet<string> GetPreDownloadList()
    {
        return preDownloadList;
    }

    public static IEnumerator Download()
    {
        if (isDownloading)
        {
            yield break;
        }

        isDownloading = true;
        int downloadCount = 0;
        int downloadCompleteCount = 0;

        foreach (var preDownloadItem in preDownloadList)
        {
            var cdnPath = StringHelper.GetCDNPath(preDownloadItem);

            if (cdnPath.isSuccess)
            {
                downloadCount++;

                ResourceManager.LoadCDN(cdnPath.folderName, cdnPath.fileName, (Texture2D texture) =>
                {
                    ResourceManager.UnLoad(texture);
                    downloadCompleteCount++;
                });
            }
        }

        yield return new WaitUntil(() =>
        {
            if(downloadCount == downloadCompleteCount)
            {
                preDownloadList.Clear();
                isDownloading = false;
                return true;
            }
            else
            {
                return false;
            }
        });
    }
}
