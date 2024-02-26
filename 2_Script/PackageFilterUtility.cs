using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class PackageFilterUtility
{
    public enum FilterType
    {
        Stage,
        Mission,
        Version
    }

    public enum FilterCondition
    {
        Over,   //초과
        Under,  //미만
        Equal   //같음
    }

    public class PackageFilterInfo
    {
        public FilterType type;
        public string value;
        public FilterCondition condition;
    }

    private abstract class PackageFilter
    {
        protected bool Check(string filterData, string targetData, FilterType filterType)
        {
            var filters = JsonToFilterInfo(filterData);
            if (filters == null) return true;

            foreach (var filter in filters)
            {
                if (filter.type == filterType)
                {
                    if (!ConditionCheck(filter.value, targetData, filter.condition))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public abstract bool Check(string filterData);

        protected virtual bool ConditionCheck(string valueData, string targetValueData, FilterCondition condition)
        {
            int value = int.Parse(valueData);
            int targetValue = int.Parse(targetValueData);

            switch (condition)
            {
                case FilterCondition.Over:  return targetValue > value;
                case FilterCondition.Under: return targetValue < value;
                case FilterCondition.Equal: return targetValue == value;
                default:                    return true;
            }
        }
    }

    private class PackageFilter_Mission : PackageFilter
    {
        public override bool Check(string filterData)
        {
            return Check(filterData, Mission.ToString(), FilterType.Mission);
        }

        private int Mission
        {
            get
            {
                int missionCount = 0;

                foreach (var mission in ServerRepos.OpenMission)
                {
                    if (mission.state == 2) missionCount++;
                }

                return missionCount;
            }
        }
    }

    private class PackageFilter_Stage : PackageFilter
    {
        public override bool Check(string filterData)
        {
            return Check(filterData, ServerRepos.User.stage.ToString(), FilterType.Stage);
        }
    }

    private class PackageFilter_Version : PackageFilter
    {
        public override bool Check(string filterData)
        {
            return Check(filterData, Application.version, FilterType.Version);
        }

        protected override bool ConditionCheck(string valueData, string targetValueData, FilterCondition condition)
        {
            if (condition != FilterCondition.Equal) return true;

            return targetValueData.StartsWith(valueData);
        }
    }

    private static List<PackageFilter> packageFilters = new List<PackageFilter>()
    {
        new PackageFilter_Mission(),
        new PackageFilter_Stage(),
        new PackageFilter_Version()
    };

    public static void PackageFiltering(List<ManagerUI.PackageShowData> packageDatas)
    {
        //test data
        //packageDatas[0].packageData.filter = "[{\"type\":0,\"value\":\"300\",\"condition\":0},{\"type\":0,\"value\":\"302\",\"condition\":1}]";
        //packageDatas[0].packageData.filter = "[{\"type\":1,\"value\":\"202\",\"condition\":0},{\"type\":1,\"value\":\"204\",\"condition\":1}]";
        //packageDatas[0].packageData.filter = "[{\"type\":2,\"value\":\"2.8\",\"condition\":2}]";

        for (int i=0; i<packageDatas.Count; i++)
        {
            foreach (var filter in packageFilters)
            {
                if (!filter.Check(packageDatas[i].packageData.filter))
                {
                    packageDatas.Remove(packageDatas[i]);
                    i--;
                    break;
                }
            }
        }
    }

    #region Json Utility
    private static List<PackageFilterInfo> JsonToFilterInfo(string filterData)
    {
        if (filterData == null || filterData == string.Empty) return null;

        return JsonConvert.DeserializeObject<List<PackageFilterInfo>>(filterData);
    }

    public static string FilterInfoToJson(List<PackageFilterInfo> data)
    {
        return JsonConvert.SerializeObject(data);
    }
    #endregion
}
