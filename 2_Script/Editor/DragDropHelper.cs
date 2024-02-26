using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class DragDropHelper
{
    public static void ShowSelection( Rect selectionArea, RemotePackageSettings settings )
    {
        if( settings != null && Selection.Contains( settings ) )
        {
            ShowSelection( selectionArea );
        }
    }

    public static void ShowSelection( Rect selectionArea )
    {
        Rect boxRect = new Rect( selectionArea );
        boxRect.x -= 3.0f;
        boxRect.y -= 1.0f;
        boxRect.width += 6.0f;
        boxRect.height += 2.0f;
        GUI.Box( boxRect, "", EditorHelper.Styles.Selection );
    }

    public static void DragAndDropArea( Rect dropArea, RemotePackageSettings settings )
    {
        Event currentEvent = Event.current;
        EventType currentEventType = currentEvent.type;

        ShowSelection( dropArea, settings );

        if( currentEventType == EventType.DragExited )
        {
            DragAndDrop.PrepareStartDrag();
        }

        if( !dropArea.Contains( currentEvent.mousePosition ) )
        {
            if( currentEventType == EventType.MouseDown && !currentEvent.control && settings != null )
            {
                SelectSettings( settings, false );
            }

            return;
        }

        switch( currentEventType )
        {
        case EventType.MouseDown:
            if( settings == null ) break;

            DragAndDrop.PrepareStartDrag();

            DragAndDrop.SetGenericData( dragDropIdentifier, true );
            DragAndDrop.objectReferences = new Object[1] { settings };

            SelectSettings( settings, true );
            break;

        case EventType.MouseDrag:
            if( DragAndDrop.GetGenericData( dragDropIdentifier ) != null )
            {
                DragAndDrop.StartDrag( dragDropTitle );
                currentEvent.Use();
            }

            break;

        case EventType.DragUpdated:
            if( IsDragTargetValid() )
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            currentEvent.Use();
            break;

        case EventType.Repaint:
            if( DragAndDrop.visualMode == DragAndDropVisualMode.Link )
            {
                GUI.Box( dropArea, "", EditorHelper.Styles.PreDrop );
            }

            break;

        case EventType.DragPerform:
            DragAndDrop.AcceptDrag();

            foreach( Object objectReference in DragAndDrop.objectReferences )
            {
                RemotePackageSettings settingsReference = objectReference as RemotePackageSettings;

                if( !settingsReference.Equals( settings ) && settingsReference.CheckParentCicle( settings ) )
                {
                    settingsReference.parent = settings;
                    EditorUtility.SetDirty( settingsReference );
                }
            }

            currentEvent.Use();
            break;

        case EventType.MouseUp:
            DragAndDrop.PrepareStartDrag();
            break;
        }
    }

    public static bool IsDragTargetValid()
    {
        return DragAndDrop.objectReferences.All( o => o is RemotePackageSettings );
    }

    public static void SelectSettings( RemotePackageSettings settings, bool selected )
    {
        if( !selected )
        {
            Object[] objects = Selection.objects;
            ArrayUtility.Remove( ref objects, settings );
            Selection.objects = objects;
            return;
        }

        if( Event.current.control )
        {
            Object[] objects = Selection.objects;

            if( Selection.Contains( settings ) )
            {
                ArrayUtility.Remove( ref objects, settings );
            }
            else
            {
                ArrayUtility.Add( ref objects, settings );
            }

            Selection.objects = objects;
        }
        else
        {
            Selection.activeObject = settings;

            if( Event.current.clickCount > 1 )
            {
                EditorGUIUtility.PingObject( settings );
            }
        }
    }

    private const string dragDropIdentifier = "RemotePackageHierarchyDragAndDrop";
    private const string dragDropTitle = "RemotePackageHierarchyDragAndDrop";
}
