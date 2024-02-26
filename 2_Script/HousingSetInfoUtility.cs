using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class HousingSetInfoUtility
{
    public struct HousingInfo
    {
        public int housingIndex;
        public int modelIndex;

        public HousingInfo(int housingIndex, int modelIndex)
        {
            this.housingIndex = housingIndex;
            this.modelIndex = modelIndex;
        }
    }

    private class SetInfo
    {
        public string setImage;
        public float camPosX;
        public float camPosY;
        public List<HousingInfo> housing;

        public SetInfo(string setImage, float camPosX, float camPosY, List<HousingInfo> housing)
        {
            this.setImage = setImage;
            this.camPosX = camPosX;
            this.camPosY = camPosY;
            this.housing = housing;
        }
    }

    /// <summary>
    /// Json으로 관리 되는 자료 형태
    /// </summary>
    private static Dictionary<int, SetInfo> originData;

    /// <summary>
    /// 하우징 정보, 세트 번호(하나의 오브젝트가 여러 세트에 포함 될 경우를 대비하여 List로 관리)
    /// </summary>
    private static Dictionary<HousingInfo, List<int>> housingToSetIndex;

    /// <summary>
    /// 세트 인덱스 정보를 관리
    /// </summary>
    private static List<int> listSetIndex = new List<int>();

    public static bool IsLoading { get; private set; } = false;

    public static void LoadData(Action complete = null)
    {
        IsLoading = true;

        originData = null;
        housingToSetIndex = null;

        //ResourceManager.LoadStreaming<string>("HousingSetInfo.json", (jsonText) =>
        ResourceManager.LoadCDN<string>("CachedScript", "HousingSetInfo.jsone", (jsonText) =>
        {
            IsLoading = false;

            string test = jsonText;

            if (string.IsNullOrEmpty(jsonText)) return;
            
            UnityEngine.Debug.Log(jsonText);

            
            originData = JsonConvert.DeserializeObject<Dictionary<int, SetInfo>>(jsonText);

            housingToSetIndex = new Dictionary<HousingInfo, List<int>>();

            foreach (var item in originData)
            {
                foreach (var housing in item.Value.housing)
                {
                    if (housingToSetIndex.TryGetValue(housing, out List<int> setList))
                    {
                        setList.Add(item.Key);
                    }
                    else
                    {
                        housingToSetIndex.Add(housing, new List<int>() { item.Key });
                    }
                }

                if(listSetIndex.Contains(item.Key) == false)
                {
                    listSetIndex.Add(item.Key);
                }
            }

            complete?.Invoke();
        });
    }

    public static bool TryGetSetIndex(HousingInfo housingInfo, out int[] setIndexArray)
    {
        setIndexArray = null;

        if (IsLoading) return false;

        if (housingToSetIndex == null || housingToSetIndex.Count == 0) return false;

        if (housingToSetIndex.TryGetValue(housingInfo, out List<int> setIndexList))
        {
            setIndexArray = setIndexList.ToArray();
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TryGetSetIndex(int housingIndex, int modelIndex, out int[] setIndexArray)
    {
        return TryGetSetIndex(new HousingInfo(housingIndex, modelIndex), out setIndexArray);
    }

    public static HousingInfo[] GetSetHousingList(int setIndex)
    {
        if (IsLoading) return null;

        if (originData == null || originData.Count == 0) return null;

        List<HousingInfo> housingInfos = new List<HousingInfo>();

        if(IsLoading && originData.TryGetValue(setIndex, out SetInfo Infos))
        {
            housingInfos.AddRange(Infos.housing);
            return housingInfos.ToArray();
        }
        else
        {
            return null;
        }
    }

    public static bool TryGetSetImage(int setIndex, out string ImageName)
    {
        ImageName = null;

        if (IsLoading) return false;

        if (originData == null || originData.Count == 0) return false;

        if (originData.TryGetValue(setIndex, out SetInfo info))
        {
            ImageName = info.setImage;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static List<int> GetSetIndexList()
    {
        if (listSetIndex == null) return new List<int>();

        return listSetIndex;
    }

    public static bool TryGetLandCamPostion(int housingIdx, int modelIdx, out Vector3 camPos)
    {
        camPos = new Vector3();

        int setIndex = 0;

        if (IsLoading) return false;

        if (originData == null || originData.Count == 0) return false;


        if(housingToSetIndex.TryGetValue(new HousingInfo(housingIdx, modelIdx), out List<int> value))
        {
            setIndex = value[0];
        }

        if(originData.TryGetValue(setIndex, out SetInfo info))
        {
            camPos = new Vector3(info.camPosX, 0, info.camPosY);
            return true;
        }
        else
        {
            return false;
        }
    }
}