using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class UploaderList
{
    private const float checkboxWidth = 16.0f;

    private System.Type uploaderType;

    private System.Type context;

    private List<Uploader> uploaders = new List<Uploader>();

    private bool showList = true;

    private ReorderableList editorList = null;

    private int selectedIndex = -1;

    private System.Action<Uploader> onUpload = null;

    public UploaderList( System.Type uploaderType, System.Type context )
    {
        this.uploaderType = uploaderType;
        this.context = context;

        this.editorList = new ReorderableList( this.uploaders, this.uploaderType );
        this.editorList.drawHeaderCallback = DrawListHeader;
        this.editorList.drawElementCallback = DrawListElementInspector;
        this.editorList.elementHeight = EditorHeightAttribute.GetHeight( uploaderType );
    }

    public string[] AsStrings()
    {
        return uploaders.Select( u => u.ToString().Replace( "/", "\\" ) ).ToArray();
    }

    public Uploader Get( int index )
    {
        if( index >= 0 && index < uploaders.Count )
        {
            return uploaders[index];
        }

        return null;
    }

    public void ShowSettingsInspector( System.Action<Uploader> onUpload = null )
    {
        this.onUpload = onUpload;

        RetrieveSettings();
        EditorGUI.BeginChangeCheck();

        if( showList )
        {
            editorList.DoLayoutList();
        }
        else
        {
            showList = GUILayout.Toggle( showList, uploaderType.Name, EditorHelper.Styles.Minimized );
        }

        if( EditorGUI.EndChangeCheck() )
        {
            SaveSettings();
        }

        this.onUpload = null;
    }

    private void DrawListHeader( Rect rect )
    {
        float rightWidth = 128.0f;
        Rect leftRect = new Rect( rect.x, rect.y, rect.width - rightWidth, rect.height );
        Rect rightRect = new Rect( rect.x + leftRect.width, rect.y, rightWidth, rect.height );

        showList = GUI.Toggle( leftRect, showList, uploaderType.Name, EditorStyles.label );

        if( onUpload != null )
        {
            bool disabled = selectedIndex < 0 || !uploaders[selectedIndex].HasSettingsNoRetrieve();

            EditorGUI.BeginDisabledGroup( disabled );
            if( GUI.Button( rightRect, "Upload Selected" ) )
            {
                onUpload( uploaders[selectedIndex] );
            }
            EditorGUI.EndDisabledGroup();
        }
    }

    private void DrawListElementInspector( Rect rect, int index, bool isActive, bool isFocused )
    {
        Rect checkboxRect = new Rect( rect.x, rect.y, checkboxWidth, checkboxWidth );
        if( EditorGUI.Toggle( checkboxRect, selectedIndex == index ) )
        {
            selectedIndex = index;
        }

        rect.xMin += checkboxWidth;

        Uploader uploader = uploaders[index];
        uploader.OnShowSettingsInspector( rect );
    }

    private void RetrieveSettings()
    {
        showList = EditorPrefs.GetBool( GetPrefsKey( "ShowList" ), showList );

        uploaders.Clear();
        string text = EditorPrefs.GetString( GetPrefsKey( "Uploaders" ), "" );

        string[] uploaderTexts;
        SerializationHelper.Deserialize( text, ';' ).ReadStringArray( out uploaderTexts );

        foreach( string serialized in uploaderTexts )
        {
            Uploader uploader = System.Activator.CreateInstance( uploaderType ) as Uploader;
            uploader.RetrieveSettings( serialized );
            uploaders.Add( uploader );
        }

        if( selectedIndex < 0 && uploaders.Count > 0 )
        {
            selectedIndex = 0;
        }
        else if( selectedIndex >= uploaders.Count )
        {
            selectedIndex = uploaders.Count - 1;
        }
    }

    private void SaveSettings()
    {
        EditorPrefs.SetBool( GetPrefsKey( "ShowList" ), showList );

        string[] serializeds = uploaders.Select( u => u.SaveSettings() ).ToArray();
        string json = SerializationHelper.Serialize( ';' ).WriteArray( serializeds ).ToString();
        EditorPrefs.SetString( GetPrefsKey( "Uploaders" ), json );
    }

    private string GetPrefsKey( string key )
    {
        return string.Format( "{0}_{1}_{2}", context.Name, uploaderType.Name, key );
    }
}
