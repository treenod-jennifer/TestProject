using UnityEditor;
using UnityEngine;

public static class EditorHelper
{
#if !UNITY_4

    public static void BeginChangeLabelWidth( float labelWidth )
    {
        savedLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = labelWidth;
    }

    public static void EndChangeLabelWidth()
    {
        EditorGUIUtility.labelWidth = savedLabelWidth;
    }

#endif

    public static void BeginBoxHeader()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical( BoxStyle );
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
    }

    public static void EndBoxHeaderBeginContent()
    {
        EndBoxHeaderBeginContent( Vector2.zero );
    }

    public static Vector2 EndBoxHeaderBeginContent( Vector2 scroll )
    {
        EditorGUILayout.EndHorizontal();
        GUILayout.Space( 1.0f );
        return EditorGUILayout.BeginScrollView( scroll );
    }

    public static bool EndBox()
    {
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        return EditorGUI.EndChangeCheck();
    }

    public static Rect Rect( GUIStyle style )
    {
        return GUILayoutUtility.GetRect( GUIContent.none, style );
    }

    public static Rect Rect( float height )
    {
        return GUILayoutUtility.GetRect( 0.0f, height, GUILayout.ExpandWidth( true ) );
    }

    public static GUIContent Label( string label )
    {
        return new GUIContent( label, GUI.tooltip );
    }

    public static bool Button( string label )
    {
        return GUILayout.Button( Label( label ) );
    }

    public static string SearchField( string search )
    {
        EditorGUILayout.BeginHorizontal();

        search = GUILayout.TextField( search, GUI.skin.GetStyle( "SearchTextField" ) );

        GUIStyle buttonStyle = GUI.skin.GetStyle( "SearchCancelButtonEmpty" );
        if( !string.IsNullOrEmpty( search ) )
        {
            buttonStyle = GUI.skin.GetStyle( "SearchCancelButton" );
        }

        if( GUILayout.Button( "", buttonStyle ) )
        {
            search = "";
        }

        EditorGUILayout.EndHorizontal();

        return search;
    }

    public static class Styles
    {
        public static GUIStyle Selection { get { return GUI.skin.FindStyle( "MeTransitionSelectHead" ) ?? GUIStyle.none; } }
        public static GUIStyle PreDrop { get { return GUI.skin.FindStyle( "TL SelectionButton PreDropGlow" ) ?? GUIStyle.none; } }
        public static GUIStyle Minimized { get { return GUI.skin.FindStyle( "ShurikenModuleTitle" ) ?? GUIStyle.none; } }
    }

    public static void DrawAllStyles()
    {
        searchField = SearchField( searchField );

        string searchLower = searchField.ToLower();
        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView( scroll );
        foreach( GUIStyle style in GUI.skin.customStyles )
        {
            if( string.IsNullOrEmpty( searchField ) || style.name.ToLower().Contains( searchLower ) )
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.TextField( style.name, EditorStyles.label );
                GUILayout.Label( style.name, style );

                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private static float savedLabelWidth;

    private static string searchField = "";
    private static Vector2 scroll = Vector2.zero;

    private static GUIStyle boxStyle = null;

    private static GUIStyle BoxStyle
    {
        get
        {
            if( boxStyle == null )
            {
#if !UNITY_4
                boxStyle = EditorStyles.helpBox;
#else
                boxStyle = GUI.skin.box;
#endif

                boxStyle.padding.left = 1;
                boxStyle.padding.right = 1;
                boxStyle.padding.top = 4;
                boxStyle.padding.bottom = 8;

                boxStyle.margin.left = 16;
                boxStyle.margin.right = 16;
            }

            return boxStyle;
        }
    }
}
