using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

public static class ProjectBrowserHelper
{
	static ProjectBrowserHelper()
	{
		Assembly editorAssembly = Assembly.GetAssembly( typeof( EditorWindow ) );
		foreach( string typeName in projecBrowserTypeNames )
		{
			projectBrowserType = editorAssembly.GetType( typeName );

			if( projectBrowserType != null )
			{
				setSearchMethod = projectBrowserType.GetMethod( "SetSearch", new System.Type[] { typeof( string ) } );

				if( setSearchMethod != null )
				{
					return;
				}
			}
		}
	}

	public static bool HasImplementation { get { return projectBrowserType != null && setSearchMethod != null; } }

	public static void SetSearch( string filter )
	{
		if( HasImplementation )
		{
			EditorWindow projectBrowserInstance = EditorWindow.GetWindow( projectBrowserType );

			if( projectBrowserInstance != null )
			{
				setSearchMethodArgs[0] = filter;
				setSearchMethod.Invoke( projectBrowserInstance, setSearchMethodArgs );
			}
		}
	}

	private static System.Type projectBrowserType;
	private static MethodInfo setSearchMethod;
	private static object[] setSearchMethodArgs = new object[] { null };

	private static string[] projecBrowserTypeNames = new string[] {
		"UnityEditor.ProjectBrowser",
		"UnityEditor.ProjectWindow",
		"UnityEditor.ObjectBrowser"
	};
}
