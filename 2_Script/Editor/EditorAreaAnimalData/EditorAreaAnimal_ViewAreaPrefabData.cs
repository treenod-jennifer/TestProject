using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorAreaAnimal_ViewAreaPrefabData : EditorAreaAnimal_ViewBase
{
    [SerializeField]
    private List<AreaBase> listData = new List<AreaBase>();

    private List<AreaBase> listPrevData = new List<AreaBase>();
    private int prevAreaIndex = -1;

    public override void InitView()
    {
        viewSize.x = 300;
        titleName = "에리어 프리팹 데이터";
        dataBoxHeight_normal = 127;
        dataBoxHeight_fold = 35;
    }

    protected override void DrawDataView()
    {
        int newCount = EditorGUI.IntField(new Rect(viewPos.x, viewPos.y, viewSize.x, 15f), "size", listData.Count);

        if (newCount != listData.Count)
        {
            while (newCount < listData.Count)
                listData.RemoveAt(listData.Count - 1);

            while (newCount > listData.Count)
                listData.Add(null);
        }
        viewPos.y += 20f;

        for (int i = 0; i < listData.Count; i++)
        {
            GUILayout.BeginArea(new Rect(viewPos.x, viewPos.y, viewSize.x, 15f));
            listData[i] = (AreaBase)EditorGUILayout.ObjectField("Prefab", listData[i], typeof(AreaBase), false);
            GUILayout.EndArea();
            viewPos.y += 20f;
        }
    }

    protected override bool IsUseAddDataButton()
    {
        return false;
    }

    #region 저장
    public void SaveData(int areaIndex)
    {
        bool bDataChange = listData.Count != listPrevData.Count;
        if (bDataChange == false)
        {
            for (int i = 0; i < listData.Count; i++)
            {
                if (listData[i] != listPrevData[i])
                {
                    bDataChange = true;
                    break;
                }       
            }
        }

        if (areaIndex != prevAreaIndex)
            bDataChange = true;

        //변경된 데이터가 없으면 반환
        if (bDataChange == false)
            return;

        //이제 사용안하는 에리어의 오픈 인덱스를 -1로 변경해줌.
        for (int i = 0; i < listPrevData.Count; i++)
        {
            int findIndex = listData.FindIndex(x => x == listPrevData[i]);
            if (findIndex > -1)
                continue;

            listPrevData[i].openAreaIndex = -1;
            PrefabUtility.SavePrefabAsset(listPrevData[i].gameObject);
        }

        //현재 등록된 에리어들의 오픈 인덱스를 변경시켜줌.
        for (int i = 0; i < listData.Count; i++)
        {
            if (listData[i] == null || listData[i].openAreaIndex == areaIndex)
                continue;
            listData[i].openAreaIndex = areaIndex;
            PrefabUtility.SavePrefabAsset(listData[i].gameObject);
        }

        //과거 데이터를 현재 데이터와 같게 만들어줌.
        listPrevData = new List<AreaBase>(listData);
    }
    #endregion

    #region 로드
    public void LoadData(int areaIndex)
    {
        listData.Clear();
        listPrevData.Clear();

        string[] arr = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Assets/5_OutResource/", "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = arr[i].Replace(Directory.GetCurrentDirectory() + "/", "");
            
            AreaBase areaBase = AssetDatabase.LoadAssetAtPath<AreaBase>(arr[i]);
            if (areaBase == null || areaBase.openAreaIndex != areaIndex)
                continue;

            listData.Add(areaBase);
            listPrevData.Add(areaBase);
        }
    }
    #endregion
}
