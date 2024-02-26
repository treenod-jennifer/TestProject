using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ManagerAssetBundle))]
public class EditorManagerAssetBundle : Editor
{
#if !UNITY_4

    public static void OnUpdatePackageVariants()
    {
        variantSmartPopup.SetOptions( AssetBundleHelper.AllAssetBundleVariants );
    }

#endif

    public override void OnInspectorGUI()
    {
#if !UNITY_4
        if( !variantSmartPopup.HasOptions )
        {
            OnUpdatePackageVariants();
        }
#endif

        EditorGUI.BeginChangeCheck();

        GUI.tooltip = "Load packages from the StreamingAssets local folder instead of the web.";
        Manager.useStreamingAssets = EditorGUILayout.Toggle( EditorHelper.Label( "Use Streaming Assets" ), Manager.useStreamingAssets );

#if !UNITY_4
        variantSmartPopup.Show( "Default Variant", Manager.defaultVariant, OnChangeDefaultVariant );
#endif

        baseUrlList.DoLayoutList();

        if( EditorGUI.EndChangeCheck() )
        {
            EditorUtility.SetDirty( Manager );
        }

        if( GUILayout.Button( "Open Package Manager" ) )
        {
            RemotePackageManagerWindow.OpenWindow();
        }
    }

    public void OnEnable()
    {
        baseUrlList = new ReorderableList( Manager.baseUris, typeof( string ) );

        baseUrlList.drawHeaderCallback = BaseUrlDrawHeaderCallback;
        baseUrlList.drawElementCallback = BaseUrlDrawElementCallback;

        baseUrlList.onChangedCallback = BaseUrlOnChange;
        baseUrlList.onCanRemoveCallback = l => l.list.Count > 1;
        baseUrlList.onAddCallback = l => { l.list.Add( "" ); EditorUtility.SetDirty( Manager ); };
    }

#if !UNITY_4

    private void OnChangeDefaultVariant( string newVariant, bool isNew )
    {
        Manager.defaultVariant = newVariant;
        EditorUtility.SetDirty( Manager );
    }

#endif

    private void BaseUrlDrawHeaderCallback( Rect position )
    {
        GUI.tooltip = "Base uris to compose the final package uri in runtime.";
        EditorGUI.LabelField( position, EditorHelper.Label( "Base Uris" ) );
    }

    private void BaseUrlDrawElementCallback( Rect position, int index, bool isActive, bool isFocused )
    {
        float leftWidth = 16.0f;
        Rect leftRect = new Rect( position.x, position.y, leftWidth, position.height );
        Rect rightRect = new Rect( position.x + leftRect.width, position.y, position.width - leftWidth, position.height );

        EditorGUI.BeginChangeCheck();

        if( EditorGUI.Toggle( leftRect, Manager.selectedBaseUriIndex == index ) )
        {
            Manager.selectedBaseUriIndex = index;
        }

        Manager.baseUris[index] = EditorGUI.TextField( rightRect, Manager.baseUris[index] );

        if( EditorGUI.EndChangeCheck() )
        {
            EditorUtility.SetDirty( Manager );
        }
    }

    private void BaseUrlOnChange( ReorderableList list )
    {
        if( Manager.baseUris.Count > 0 )
        {
            Manager.selectedBaseUriIndex = Mathf.Clamp( Manager.selectedBaseUriIndex, 0, Manager.baseUris.Count - 1 );
        }
        else
        {
            Manager.selectedBaseUriIndex = -1;
        }

        EditorUtility.SetDirty( Manager );
    }

    private ManagerAssetBundle Manager
    { get { return target as ManagerAssetBundle; } }

    private ReorderableList baseUrlList = null;

#if !UNITY_4
    private static SmartPopupHelper variantSmartPopup = new SmartPopupHelper( "The default AssetBundle Variant used when downloading packages." );
#endif
}
