using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class Editor_AreaRedirect : EditorWindow
{
    private Vector2 scrollPos;

    static public bool redirect = false;
    static public Dictionary<string, string> rules = new Dictionary<string, string>();

    string orgString = "";
    string newString = "";

    [MenuItem("Tools/AreaRedirect")]
    public static void CreateDevConsole()
    {
        EditorWindow.GetWindow<Editor_AreaRedirect>(false, "AreaRedirect", true);
    }

    public static void Init()
    {
        if( EditorPrefs.HasKey("UseAreaRedirect") )
        {
            redirect = EditorPrefs.GetBool("UseAreaRedirect");
            string pref = EditorPrefs.GetString("AreaRedirectInfo");
            rules = JsonConvert.DeserializeObject<Dictionary<string, string>>(pref);
        }
    }

    public virtual void OnGUI()
    {
        bool r = redirect;
        bool rulesChanged = false;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.BeginVertical();
        GUILayout.Height(20f);

        GUILayout.BeginHorizontal();
        r = GUILayout.Toggle(redirect, "다음 에리어로 전환 (day에 정의된 번들이름을 바꿔줍니다)");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        orgString = EditorGUILayout.TextField(orgString, GUILayout.Width(200f));
        GUILayout.Label("->", "BoldLabel");
        newString = EditorGUILayout.TextField(newString, GUILayout.Width(200f));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Add", GUILayout.Height(40f)))
        {
            rules[orgString] = newString;
            rulesChanged = true;
        }

        if (GUILayout.Button("Reset", GUILayout.Height(40f)))
        {
            rules.Clear();
            rulesChanged = true;
        }
        GUILayout.EndVertical();

        if ( r != redirect)
        {
            redirect = r;
            EditorPrefs.SetBool("UseAreaRedirect", redirect);
            EditorPrefs.SetString("AreaRedirectInfo", JsonConvert.SerializeObject(rules));
        }
            

        if (rulesChanged)
        {
            EditorPrefs.SetBool("UseAreaRedirect", redirect);
            EditorPrefs.SetString("AreaRedirectInfo", JsonConvert.SerializeObject(rules));
        }

        foreach(var i in rules)
        {
            GUILayout.Label(i.Key + " -> " + i.Value, "BoldLabel");
        }
            

        GUI.backgroundColor = Color.white;
        

        EditorGUILayout.EndScrollView();
    }


}
