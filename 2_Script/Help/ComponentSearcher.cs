using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public static class Extensions
{
    public static bool CaseInsensitiveContains(this string text, string value,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        return text.IndexOf(value, stringComparison) >= 0;
    }
}

public class ComponentSearcher : EditorWindow
{
    static ComponentSearcher window;

    //static string scriptValue = "";
    //static string oldScriptValue;

    static List<Type> components;
    static List<string> componentNames;
    static List<UnityEngine.Object> sceneObjects;
    static int selectedIndex = 0;
    static int prevIndex = 0;
    static Vector2 scrollValue = Vector2.zero;

    static string strSearch;

    static GUIStyle guiStyle;

    [MenuItem("EditorUtility/ComponentSearcher")]
    static void OpenComponentSearcher()
    {
        window = (ComponentSearcher)EditorWindow.GetWindow(typeof(ComponentSearcher));

        Assembly _assembly = Assembly.Load("Assembly-CSharp");

        components = new List<Type>();
        componentNames = new List<string>();

        foreach (Type type in _assembly.GetTypes())
        {
            if (type.IsClass)
            {
                if (type.BaseType.FullName.Contains("MonoBehaviour"))
                {
                    if(type.Name.Contains("Trigger"))
                    {
                        components.Add(type.UnderlyingSystemType);
                        componentNames.Add(type.Name);
                    }
                }
                else
                {
                    if (!type.BaseType.FullName.Contains("System"))
                    {
                        if (type.Name.Contains("Trigger"))
                        {
                            //Type _type = type.BaseType;
                            components.Add(type.UnderlyingSystemType);
                            componentNames.Add(type.Name);
                        }
                    }
                }
            }
        }

        prevIndex = selectedIndex;
        SelectScript(components[selectedIndex]);

        guiStyle = new GUIStyle();
        guiStyle.margin.left = 10;
        guiStyle.padding.bottom = 5;
        guiStyle.normal.textColor = Color.white;
    }

    static void SelectScript(Type _type)
    {
        //Debug.Log("Selected Type: " + _type);

        sceneObjects = new List<UnityEngine.Object>();

        UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll(_type);
        //Debug.Log(allObjects.Length);
        foreach (UnityEngine.Object _obj in allObjects)
        {
            if (sceneObjects.Find(x => (x.name == _obj.name)) == null)
                sceneObjects.Add(_obj);
        }

        sceneObjects = sceneObjects.OrderBy(o => o.name.Substring(1, 3)).ToList();

        Selection.objects = sceneObjects.ToArray();
    }

    bool onceInit = false;
    void OnGUI()
    {
        if (componentNames != null)
        {
            if (onceInit == false)
            {
                selectedIndex = EditorGUILayout.Popup(selectedIndex, componentNames.ToArray());

                if (selectedIndex != prevIndex)
                {
                    SelectScript(components[selectedIndex]);
                }

                strSearch = EditorGUILayout.TextField("Search: ", strSearch);

                scrollValue = EditorGUILayout.BeginScrollView(scrollValue);

                if (strSearch == null || strSearch == "")
                {
                    foreach (UnityEngine.Object _obj in sceneObjects)
                    {
                        if (_obj != null)
                        {
                            if (GUILayout.Button(_obj.name, guiStyle))
                            {
                                Selection.activeObject = _obj;
                                EditorGUIUtility.PingObject(_obj);

                                GUILayout.Label("Mouse over!");
                            }
                        }

                    }
                }
                else
                {
                    List<UnityEngine.Object> searched = sceneObjects.FindAll(x => x.name.CaseInsensitiveContains(strSearch));

                    foreach (UnityEngine.Object _obj in searched)
                    {
                        if (_obj != null)
                        {
                            if (GUILayout.Button(_obj.name, guiStyle))
                            {
                                Selection.activeObject = _obj;
                                EditorGUIUtility.PingObject(_obj);
                            }
                        }

                    }
                }

                EditorGUILayout.EndScrollView();

                prevIndex = selectedIndex;
            }

            if (onceInit == true)
            {
                onceInit = false;
            }
        }
        else
        {
            if (onceInit == false)
            {
                onceInit = true;
                OpenComponentSearcher();
            }
        }
    }
}
#endif