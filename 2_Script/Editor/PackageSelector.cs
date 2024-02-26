using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !UNITY_4

public class PackageSelector : ScriptableObject
{
    public bool this[string packageName]
    {
        get
        {
            bool isSelected = false;
            Registry.TryGetValue( packageName, out isSelected );
            return isSelected;
        }

        set
        {
            Registry[packageName] = value;
        }
    }

    public void Save()
    {
        selectedPackages.Clear();
        foreach( var e in Registry )
        {
            selectedPackages.Add( new SelectedPackage { name = e.Key, selected = e.Value } );
        }

        EditorUtility.SetDirty( this );
        AssetDatabase.SaveAssets();
    }

    private void TryLoad()
    {
        if( Registry == null )
        {
            Registry = new Dictionary<string, bool>();
            foreach( SelectedPackage p in selectedPackages )
            {
                Registry.Add( p.name, p.selected );
            }
        }
    }

    private const string packageSelectorPath = "Assets/2_Script/Editor/SelectedPackages.asset";
    private static PackageSelector instance = null;

    public static PackageSelector GetInstance()
    {
        if( instance == null )
        {
            instance = EditorCompatibilityHelper.LoadAssetAtPath<PackageSelector>( packageSelectorPath );

            if( instance == null )
            {
                instance = CreateInstance<PackageSelector>();
                AssetDatabase.CreateAsset( instance, packageSelectorPath );
                AssetDatabase.Refresh();
            }
        }

        instance.TryLoad();
        return instance;
    }

    [System.Serializable]
    public class SelectedPackage
    {
        public string name;
        public bool selected;
    }

    [SerializeField]
    private List<SelectedPackage> selectedPackages = new List<SelectedPackage>();

    public Dictionary<string, bool> Registry
    { get; private set; }
}

#endif
