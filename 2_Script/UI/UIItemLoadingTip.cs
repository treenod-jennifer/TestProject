using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLoadingTip : MonoBehaviour
{
    public static UIItemLoadingTip Make(TipInfoContainer.TipInfo tipInfo, Transform parent = null)
    {
        var origin = Resources.Load(tipInfo.GetFilePath());

        GameObject tipObject;

        if (parent != null)
        {
            tipObject = Instantiate(origin, parent) as GameObject;
        }
        else
        {
            tipObject = Instantiate(origin) as GameObject;
        }

        return tipObject.GetComponent<UIItemLoadingTip>();
    }
}

public class TipInfoContainer
{
    private static TipInfoContainer instance;
    public static TipInfoContainer Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new TipInfoContainer();
            }

            return instance;
        }
    }

    public class TipInfo
    {
        public string FileName { get; private set; }

        public string[] Tags 
        {
            get
            {
                string[] temp = new string[tags.Count];

                tags.CopyTo(temp);

                return temp;
            }
            private set
            {
                tags.Clear();

                foreach (var tag in value)
                {
                    tags.Add(tag);
                }
            }
        }

        public bool IsLock
        {
            get
            {
                if (lockChecker == null)
                {
                    return false;
                }
                else
                {
                    return lockChecker();
                }
            }
        }

        private HashSet<string> tags = new HashSet<string>();

        private System.Func<bool> lockChecker = null;

        public TipInfo(string fileName, params string[] tags)
        {
            FileName = fileName;
            Tags = tags;
        }

        public TipInfo(string fileName, System.Func<bool> lockChecker, params string[] tags)
        {
            FileName = fileName;
            this.lockChecker = lockChecker;
            Tags = tags;
        }

        public string GetFilePath()
        {
            return $"UIPrefab/Tip/{FileName}";
        }

        public bool ContainsTag(string tag)
        {
            return tags.Contains(tag);
        }
    }

    private TipInfo[] tipData = new TipInfo[] 
    {
        new TipInfo("UIItemLoadingTip_1", "NormalGame"),
        new TipInfo("UIItemLoadingTip_2", () => !IsStageOpen(41), "NormalGame", "NormalGame_41"),
        new TipInfo("UIItemLoadingTip_3", () => !IsStageOpen(171), "NormalGame", "NormalGame_171"),
        new TipInfo("UIItemLoadingTip_4", () => !IsStageOpen(486), "NormalGame", "NormalGame_486"),
        new TipInfo("UIItemLoadingTip_5", "NormalGame"),
        new TipInfo("UIItemLoadingTip_6", () => !IsStageOpen(6), "NormalGame"),
        new TipInfo("UIItemLoadingTip_7", () => !IsStageOpen(8), "NormalGame"),

        new TipInfo("UIItemLoadingTip_8", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_9", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_10", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_11", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_12", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_13", () => !IsAdventureOpen(), "Adventure"),
        new TipInfo("UIItemLoadingTip_14", () => !IsAdventureOpen(), "Adventure"),
    };

    private Dictionary<string, List<TipInfo>> tipDictionary = new Dictionary<string, List<TipInfo>>();

    private TipInfoContainer()
    {
        foreach(var tip in tipData)
        {
            tipDictionary.Add(tip.FileName, new List<TipInfo>() { tip });

            foreach(var tag in tip.Tags)
            {
                if (tipDictionary.TryGetValue(tag, out List<TipInfo> tipList))
                {
                    tipList.Add(tip);
                }
                else
                {
                    tipDictionary.Add(tag, new List<TipInfo>() { tip });
                }
            }
        }
    }

    public TipInfo GetTip(string fileName)
    {
        if(tipDictionary.TryGetValue(fileName, out List<TipInfo> tipList) && tipList.Count != 0)
        {
            return tipList[0];
        }

        return null;
    }

    public TipInfo[] GetTips(string tag, bool includeLockObject = false)
    {
        List<TipInfo> tempTips = new List<TipInfo>();

        if (tipDictionary.TryGetValue(tag, out List<TipInfo> tipList))
        {
            tempTips.AddRange(tipList);
        }

        if (!includeLockObject)
        {
            tempTips.RemoveAll((tip) => { return tip.IsLock; });
        }

        return tempTips.ToArray();
    }

    public TipInfo[] GetTips(bool includeLockObject = false)
    {
        List<TipInfo> tempTips = new List<TipInfo>();

        tempTips.AddRange(tipData);

        if (!includeLockObject)
        {
            tempTips.RemoveAll((tip) => { return tip.IsLock; });
        }

        return tempTips.ToArray();
    }

    private static bool IsStageOpen(int stageIndex)
    {
        if (ManagerData._instance._stageData == null) return false;

        if (ManagerData._instance._stageData.Count <= stageIndex) return false;

        if (stageIndex == 1) return true;

        if (ManagerData._instance._stageData[stageIndex - 1]._flowerLevel != 0) return true;

        if (ManagerData._instance._stageData[stageIndex - 2]._flowerLevel != 0) return true;

        return false;
    }

    private static bool IsAdventureOpen()
    {
        return ManagerAdventure.CheckStartable();
    }
}
