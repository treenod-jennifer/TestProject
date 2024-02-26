using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JsonEditor))]
public class JsonEditorCustom : Editor
{
    private JsonEditor jsonEditor;

    private void OnEnable()
    {
        jsonEditor = target as JsonEditor;

        MakeExpTable();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Make Json"))
        {
            string jsonData = JsonConvert.SerializeObject(jsonEditor.expTable);

            Debug.Log(jsonData);
        }
    }

    private void MakeExpTable()
    {
        MakeExpTableTest(jsonEditor.expTable.grade1, 100, 10, 20);
        MakeExpTableTest(jsonEditor.expTable.grade2, 100, 20, 30);
        MakeExpTableTest(jsonEditor.expTable.grade3, 100, 30, 40);
        MakeExpTableTest(jsonEditor.expTable.grade4, 100, 40, 50);
        MakeExpTableTest(jsonEditor.expTable.grade5, 100, 50, 60);
    }

    private void MakeExpTableTest(Dictionary<int, AnimalExpInfo> table, int tableStartExp, int interval, int maxLevel)
    {
        table.Clear();

        int nextExp = 0;

        int startExp = 0;
        int endExp = 0;

        for (int i = 1; i <= maxLevel; i++)
        {
            nextExp = (i == maxLevel) ? 0 : tableStartExp + (interval * (i - 1));

            startExp = endExp;
            endExp = startExp + nextExp;

            table.Add(i, new AnimalExpInfo(startExp, endExp));
        }
    }
}
