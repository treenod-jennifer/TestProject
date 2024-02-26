using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialGetInfo
{
    public bool bFiled = false;
    public bool bBox1 = false;
    public bool bBox2 = false;
    public bool bBox3 = false;
}

public class QuestAlarmData
{
    public int preProg1 = 0;
    public int curProg1 = 0;
    public bool bNew = false;

    public QuestAlarmData(int preP, int curP, bool isNew)
    {
        this.preProg1 = preP;
        this.curProg1 = curP;
        this.bNew = isNew;
    }
}

public class UIDiaryData
{
    public List<QuestGameData> QuestGetData { get ; set; }
    public List<PlusHousingModelData> ProgressHousingData { get; set; }
    public List<CdnCostume> CostumeData { get; set; }
    public Dictionary<int, QuestAlarmData> DicQuestAlarmData { get; set; }
    public Dictionary<int, int> DicHousingCateroryAlarmData { get; set; }
    public Dictionary<int, List<int>> DicProgressHousingNewData { get; set; }
    public Dictionary<int, bool> DicProgressHousingPreview { get; set; }
    public Dictionary<int, MaterialGetInfo> DicMaterialGetInfoData { get; set; }
    public Dictionary<int, List<int>> DicCostumeNewData { get; set; }
    public Dictionary<int, int> QuestPlayerPrefsKey { get; set; }
    public List<string> HousingPlayerPrefsKey { get; set; }
    public List<string> CostumePlayerPrefsKey { get; set; }

    public bool MissionAlarm { get; set; }
    public int QuestRewardCount { get; set; }
    public int EventQuestRewardCount { get; set; }
    public int HousingGetCount { get; set; }
    public bool StampAlarm { get; set; }
}
