﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

#if !UNITY_4
[CustomPropertyDrawer( typeof( PackageVariantSelectorAttribute ) )]
public class PackageVariantSelectorDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty prop, GUIContent label )
    {

        if( !variantSmartPopup.HasOptions )
        {
            variantSmartPopup.SetOptions( AssetBundleHelper.AllAssetBundleVariants );
        }

        variantSmartPopup.Show( position, label, prop.stringValue, (value, isNew) => prop.stringValue = value );
    }

    public static void OnUpdatePackageVariants()
    {
        variantSmartPopup.SetOptions( AssetBundleHelper.AllAssetBundleVariants );
    }

    private static SmartPopupHelper variantSmartPopup = new SmartPopupHelper( "" );
}
#endif
