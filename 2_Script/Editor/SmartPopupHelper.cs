using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SmartPopupHelper
{
    public SmartPopupHelper( string popupTooltip )
    {
        this.popupTooltip = popupTooltip;
        this.canCreate = false;
    }

    public SmartPopupHelper( string controlName, string popupTooltip, string createNewTooltip )
    {
        this.controlName = controlName;
        this.popupTooltip = popupTooltip;
        this.createNewTooltip = createNewTooltip;
        this.canCreate = true;
    }

    public bool HasOptions { get { return options != null; } }

    public void SetOptions( string[] newOptions )
    {
        if( canCreate )
        {
            options = new string[defaultOptionsFirst.Length + newOptions.Length + defaultOptionsSecond.Length];
            defaultOptionsFirst.CopyTo( options, 0 );
            newOptions.CopyTo( options, defaultOptionsFirst.Length );
            defaultOptionsSecond.CopyTo( options, defaultOptionsFirst.Length + newOptions.Length );
        }
        else
        {
            options = new string[defaultOptionsFirst.Length + newOptions.Length];
            defaultOptionsFirst.CopyTo( options, 0 );
            newOptions.CopyTo( options, defaultOptionsFirst.Length );
        }

        optionsGUIContent = options.Select( o => new GUIContent( o ) ).ToArray();
        SetSelectMode( true );
    }

    public bool Show( string value, System.Action<string, bool> onChangeCallback = null )
    {
        GUIContent label = null;
        return Show( GetRect(), label, value, onChangeCallback );
    }

    public bool Show( string label, string value, System.Action<string, bool> onChangeCallback = null )
    {
        GUI.tooltip = selectMode ? popupTooltip : createNewTooltip;
        return Show( GetRect(), EditorHelper.Label( label ), value, onChangeCallback );
    }

    public bool Show( Rect position, GUIContent label, string value, System.Action<string, bool> onChangeCallback = null )
    {
        if( selectMode )
        {
            Popup( position, label, value, onChangeCallback );
        }
        else
        {
            return CreateNew( position, label, onChangeCallback );
        }

        return false;
    }

    private Rect GetRect()
    {
        return EditorHelper.Rect( selectMode ? EditorStyles.popup : EditorStyles.textField );
    }

    private void Popup( Rect position, GUIContent label, string value, System.Action<string, bool> onChangeCallback )
    {
        value = string.IsNullOrEmpty( value ) ? NoneOption : value;
        int index = System.Array.IndexOf( options, value );

        if( index < 0 )
        {
            index = 0;
            TriggerCallback( onChangeCallback, null, false );
        }

        EditorGUI.BeginChangeCheck();

        if( label == null )
        {
            index = EditorGUI.Popup( position, index, optionsGUIContent );
        }
        else
        {
            index = EditorGUI.Popup( position, label, index, optionsGUIContent );
        }

        if( EditorGUI.EndChangeCheck() )
        {
            if( !canCreate || index < options.Length - 1 )
            {
                value = index != 0 ? options[index] : null;
                TriggerCallback( onChangeCallback, value, false );
            }
            else
            {
                SetSelectMode( false );
            }
        }
    }

    private bool CreateNew( Rect position, GUIContent label, System.Action<string, bool> onChangeCallback )
    {
        GUI.SetNextControlName( controlName );

        if( label == null )
        {
            newEntry = EditorGUI.TextField( position, newEntry );
        }
        else
        {
            newEntry = EditorGUI.TextField( position, label, newEntry );
        }

        Event e = Event.current;
        if( e != null && e.isKey )
        {
            if( e.keyCode == KeyCode.Return )
            {
                TriggerCallback( onChangeCallback, newEntry, true );

                SetSelectMode( true );
                e.Use();

                return true;
            }
            else if( e.keyCode == KeyCode.Escape )
            {
                SetSelectMode( true );
                e.Use();
            }
        }

        return false;
    }

    private static void TriggerCallback<T1, T2>( System.Action<T1, T2> callback, T1 arg1, T2 arg2 )
    {
        if( callback != null )
        {
            callback( arg1, arg2 );
        }
    }

    private void SetSelectMode( bool select )
    {
        selectMode = select;
        newEntry = "";

        if( !select )
        {
#if !UNITY_4
            EditorGUI.FocusTextInControl( controlName );
#endif
        }
    }

    private string controlName;
    private string popupTooltip;
    private string createNewTooltip;

    private string[] options;
    private GUIContent[] optionsGUIContent;

    private bool canCreate;
    private bool selectMode;
    private string newEntry;

    public const string NoneOption = "None";
    private static string[] defaultOptionsFirst = new string[] { NoneOption, "" };
    private static string[] defaultOptionsSecond = new string[] { "", "New..." };
}
