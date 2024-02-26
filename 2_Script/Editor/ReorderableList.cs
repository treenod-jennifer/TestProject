#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ReorderableList
{
    public ReorderableList( IList list, System.Type elementType )
    {
        this.list = list;
        this.elementType = elementType;
    }

    public void DoLayoutList()
    {
        Rect headerRect = GUILayoutUtility.GetRect( GUIContent.none, GUI.skin.button );
        TriggerAction( drawHeaderCallback, headerRect );

        int removeElementAt = -1;
        for( int i = 0; i < list.Count; i++ )
        {
            Rect elementRect = GUILayoutUtility.GetRect( GUIContent.none, GUI.skin.button, GUILayout.Height( elementHeight + space ) );
            elementRect.xMin += indentation;
            elementRect.yMax -= space;

            if( ShowRemoveButton( ref elementRect ) )
            {
                removeElementAt = i;
            }

            GUI.Box( elementRect, GUIContent.none );

            if( drawElementCallback != null )
            {
                drawElementCallback( elementRect, i, true, true );
            }
        }

        if( removeElementAt >= 0 )
        {
            list.RemoveAt( removeElementAt );
            TriggerAction( onChangedCallback, this );
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if( GUILayout.Button( "+" ) )
        {
            TriggerAction( onAddCallback, this );
            TriggerAction( onChangedCallback, this );
        }
        EditorGUILayout.EndHorizontal();
    }

    private bool ShowRemoveButton( ref Rect rect )
    {
        EditorGUI.BeginDisabledGroup( !TriggerPredicate( onCanRemoveCallback, this ) );

        rect.width -= removeButtonWidth + space;
        Rect buttonRect = new Rect( rect.xMax + space, rect.y, removeButtonWidth, removeButtonHeight );
        bool button = GUI.Button( buttonRect, "-" );

        EditorGUI.EndDisabledGroup();

        return button;
    }

    private static void TriggerAction<T>( System.Action<T> callback, T param )
    {
        if( callback != null )
        {
            callback( param );
        }
    }

    private static bool TriggerPredicate<T>( System.Predicate<T> callback, T param )
    {
        if( callback != null )
        {
            return callback( param );
        }

        return false;
    }

    private const float indentation = 16.0f;
    private const float removeButtonWidth = 16.0f;
    private const float removeButtonHeight = 16.0f;
    private const float space = 4.0f;

    private static void DefaultAddElement( ReorderableList self )
    {
        self.list.Add( System.Activator.CreateInstance( self.elementType ) );
    }

    private static bool DefaultCanRemoveElement( ReorderableList self )
    {
        return true;
    }

    public System.Action<Rect> drawHeaderCallback = null;
    public System.Action<Rect, int, bool, bool> drawElementCallback = null;

    public System.Predicate<ReorderableList> onCanRemoveCallback = DefaultCanRemoveElement;
    public System.Action<ReorderableList> onAddCallback = DefaultAddElement;
    public System.Action<ReorderableList> onChangedCallback = null;

    public float elementHeight = EditorHeightAttribute.SingleLineHeight;

    public IList list;
    private System.Type elementType;
}

#endif
