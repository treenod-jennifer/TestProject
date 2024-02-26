using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ManagerObjectPool))]
[CanEditMultipleObjects]
public class EditorObjectPool : Editor
{

    public class CategoryEditData
    {
        public bool isVisible;

        public bool isEditingCategory;
        public string editingCategoryName;

        public GameObject newPrefab;
        public int newSize;
    }

    private const string CONTROL_NAME_FOCUS_OUT = "focusOut";
    private const string CONTROL_NAME_NEW_CATEGORY = "newCategory";
    private const string CONTROL_NAME_EDIT_CATEGORY_TEXTFIELD = "editCategoryTextField";

    private ManagerObjectPool objectPool;
    private int objectPoolCount;

    private Dictionary<ManagerObjectPool.CategoryData, CategoryEditData> editDataDict = new Dictionary<ManagerObjectPool.CategoryData, CategoryEditData>();
    private string newCategoryName;

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        objectPool = target as ManagerObjectPool;
        objectPoolCount = objectPool.startupPools.Count;

        for (int i = 0; i < objectPoolCount; i++)
        {
            var category = objectPool.startupPools[i];
            if (!editDataDict.ContainsKey(category))
                editDataDict.Add(category, new CategoryEditData { isVisible = true, isEditingCategory = false });
        }
    }

    public override void OnInspectorGUI()
    {
        if (objectPoolCount != objectPool.startupPools.Count)
            Init();

        serializedObject.Update();

        

        DrawTopMenu();
        DrawFocusOutObject();
        DrawNewCategory();

        for (int i = 0; i < objectPool.startupPools.Count; i++)
        {
            ManagerObjectPool.CategoryData categoryData = objectPool.startupPools[i];
            CategoryEditData editData = editDataDict[categoryData];

            string textFieldControlName = CONTROL_NAME_EDIT_CATEGORY_TEXTFIELD + i;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(editData.isVisible ? "-" : "+", GUILayout.Width(20)))
                {
                    editData.isVisible = !editData.isVisible;
                }

                if (editData.isEditingCategory)
                {
                    GUI.SetNextControlName(textFieldControlName);
                    editData.editingCategoryName = EditorGUILayout.TextField(editData.editingCategoryName);
                }
                else
                {
                    EditorGUILayout.LabelField(categoryData.category + " (" + categoryData.dataList.Count + ")", EditorStyles.boldLabel);
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            {
                if (editData.isEditingCategory)
                {
                    if (GUILayout.Button("Save") || IsReturnPressedOn(textFieldControlName))
                    {
                        if (categoryData.category.ToLower().Equals(editData.editingCategoryName.ToLower()))
                        {
                        }
                        else if (CheckValidCategoryName(editData.editingCategoryName))
                        {
                            categoryData.category = editData.editingCategoryName;
                        }
                        else
                        {
                            editData.editingCategoryName = categoryData.category;
                        }

                        editData.isEditingCategory = false;

                        ResetFocus();
                        EditorUtility.SetDirty(target);
                        //                        EditorSceneManager.MarkSceneDirty();
                        EditorApplication.MarkSceneDirty();
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit"))
                    {
                        editData.editingCategoryName = categoryData.category;
                        editData.isEditingCategory = true;

                        EditorGUI.FocusTextInControl(textFieldControlName);
                        EditorUtility.SetDirty(target);
                    }
                }

                if (GUILayout.Button("Sort"))
                {
                    categoryData.dataList.Sort((first, second) => { return first.prefab.name.CompareTo(second.prefab.name); });
                    EditorApplication.MarkSceneDirty();
                }

                SetGUIColor(Color.red);
                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "카테고리에 포함되어 있는 프리팹 리스트도 사라집니다. 정말 삭제하시겠습니까?", "Yes", "No"))
                    {
                        objectPool.startupPools.RemoveAt(i);
                        i--;
                        editDataDict.Remove(categoryData);

                        EditorApplication.MarkSceneDirty();
                    }
                }
                UnsetGUIColor();

                if (GUILayout.Button("▲", GUILayout.Width(30)))
                {
                    if (SwapCategoryData(objectPool.startupPools, i - 1, i))
                    {
                        EditorApplication.MarkSceneDirty();
                    }
                }

                if (GUILayout.Button("▼", GUILayout.Width(30)))
                {
                    if (SwapCategoryData(objectPool.startupPools, i + 1, i))
                    {
                        EditorApplication.MarkSceneDirty();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space();

            if (editData.isVisible)
            {
                EditorGUI.indentLevel += 1;
                {
                    DrawCategoryDataList(categoryData, editData);
                    DrawNewPrefab(categoryData, editData);
                }
                EditorGUI.indentLevel -= 1;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool SwapCategoryData(List<ManagerObjectPool.CategoryData> list, int firstIndex, int secondIndex)
    {
        if (firstIndex < 0 || firstIndex >= list.Count ||
           secondIndex < 0 || secondIndex >= list.Count)
            return false;

        ManagerObjectPool.CategoryData firstData = list[firstIndex];
        ManagerObjectPool.CategoryData secondData = list[secondIndex];
        list[firstIndex] = secondData;
        list[secondIndex] = firstData;

        return true;
    }

    private void DrawTopMenu()
    {
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("+ Expand All"))
            {
                foreach (var pair in editDataDict)
                {
                    pair.Value.isVisible = true;
                }
            }

            if (GUILayout.Button("- Collapse All"))
            {
                foreach (var pair in editDataDict)
                {
                    pair.Value.isVisible = false;
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawFocusOutObject()
    {
        GUI.SetNextControlName(CONTROL_NAME_FOCUS_OUT);
        GUI.Label(new Rect(-100, -100, 1, 1), "");
    }

    private void ResetFocus()
    {
        EditorGUI.FocusTextInControl(CONTROL_NAME_FOCUS_OUT);
    }

    private void DrawNewCategory()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            GUI.SetNextControlName(CONTROL_NAME_NEW_CATEGORY);
            newCategoryName = EditorGUILayout.TextField("New Category", newCategoryName);

            SetGUIColor(Color.green);
            if (GUILayout.Button("Add", GUILayout.Width(50)) || IsReturnPressedOn(CONTROL_NAME_NEW_CATEGORY))
            {
                if (CheckValidCategoryName(newCategoryName))
                {
                    AddNewCategory(newCategoryName);
                }

                newCategoryName = "";
                ResetFocus();
                EditorUtility.SetDirty(target);
                EditorApplication.MarkSceneDirty();
            }
            UnsetGUIColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        Rect rect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width, 1), Color.black);

        EditorGUILayout.Space();
    }

    private void DrawCategoryDataList(ManagerObjectPool.CategoryData categoryData, CategoryEditData editData)
    {
        //draw items
        for (int i = 0; i < categoryData.dataList.Count; i++)
        {
            ManagerObjectPool.StartupPool data = categoryData.dataList[i];

            //Contents
            EditorGUILayout.BeginHorizontal();
            {
                data.prefab = EditorGUILayout.ObjectField(data.prefab, typeof(GameObject), false) as GameObject;
                data.size = EditorGUILayout.IntField(data.size, GUILayout.Width(50));

                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    categoryData.dataList.RemoveAt(i);
                    i--;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawNewPrefab(ManagerObjectPool.CategoryData categoryData, CategoryEditData editData)
    {
        //Add new prefab
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("New", GUILayout.Width(50));
            editData.newPrefab = EditorGUILayout.ObjectField(editData.newPrefab, typeof(GameObject), false) as GameObject;

            string newSizeControlName = "newSize" + categoryData.category;
            GUI.SetNextControlName(newSizeControlName);
            editData.newSize = EditorGUILayout.IntField(editData.newSize, GUILayout.Width(50));

            SetGUIColor(Color.green);
            string addButtonControlName = "addButton" + categoryData.category;
            GUI.SetNextControlName(addButtonControlName);
            if (GUILayout.Button("Add", GUILayout.Width(40)) || IsReturnPressedOn(newSizeControlName))
            {
                if (editData.newPrefab != null && editData.newSize > 0)
                {
                    if (IsSamePrefabExists(editData.newPrefab))
                    {
                        editData.newPrefab = null;
                        EditorUtility.DisplayDialog("Warning!", "이미 등록된 Prefab입니다.", "Ok");
                        ResetFocus();

                        EditorUtility.SetDirty(target);
                    }
                    else
                    {
                        categoryData.dataList.Add(new ManagerObjectPool.StartupPool() { prefab = editData.newPrefab, size = editData.newSize });
                        editData.newPrefab = null;
                        editData.newSize = 0;
                    }
                }
                else
                {
                    string msg = editData.newPrefab == null ? "Prefab이 없습니다!\n" : "";
                    msg += editData.newSize <= 0 ? "Size에 0보다 큰 수를 입력해주세요!" : "";
                    EditorUtility.DisplayDialog("Warning!", msg, "Ok");
                    ResetFocus();
                }
            }
            UnsetGUIColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        Rect rect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width, 1), Color.grey);

        EditorGUILayout.Space();
    }

    private void AddNewCategory(string newCategoryName)
    {
        if (objectPool.startupPools.Find((category) => category.category.Equals(newCategoryName)) == null)
        {
            ManagerObjectPool.CategoryData data = new ManagerObjectPool.CategoryData();
            data.category = newCategoryName;
            data.dataList = new List<ManagerObjectPool.StartupPool>();

            objectPool.startupPools.Add(data);
            editDataDict.Add(data, new CategoryEditData { isVisible = true, isEditingCategory = false });

            EditorApplication.MarkSceneDirty();
        }
    }

    private bool IsReturnPressedOn(string controlName)
    {
        return Event.current.isKey &&
                Event.current.keyCode == KeyCode.Return &&
                GUI.GetNameOfFocusedControl() == controlName;
    }

    #region Check Valid
    private bool CheckValidCategoryName(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            EditorUtility.DisplayDialog("Warning!", "카테고리 이름을 입력해주세요!", "Ok");
            return false;
        }

        categoryName = categoryName.Trim();
        if (string.IsNullOrEmpty(categoryName))
        {
            EditorUtility.DisplayDialog("Warning!", "카테고리 이름을 입력해주세요!", "Ok");
            return false;
        }

        if (IsSameCategoryNameExists(categoryName))
        {
            EditorUtility.DisplayDialog("Warning!", "같은 카테고리 이름이 이미 사용 중입니다!", "Ok");
            return false;
        }

        return true;
    }

    private bool IsSameCategoryNameExists(string categoryName)
    {
        string lowerCategoryName = categoryName.ToLower();
        return objectPool.startupPools.Find(pool => pool.category.ToLower().Equals(lowerCategoryName)) != null;
    }

    private bool IsSamePrefabExists(GameObject prefab)
    {
        var pools = objectPool.startupPools;
        for (int i = 0; i < pools.Count; i++)
        {
            var pool = pools[i];
            if (pool.dataList.Find(data => data.prefab == prefab) != null)
                return true;
        }

        return false;
    }
    #endregion

    #region GUI Color
    private void SetGUIColor(Color color)
    {
        GUI.color = color;
    }

    private void UnsetGUIColor()
    {
        GUI.color = Color.white;
    }
    #endregion
}
