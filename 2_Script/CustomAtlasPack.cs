using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAtlasPack : MonoBehaviour
{
    [SerializeField] Object[] atlasObjects = new Object[(int)ManagerUIAtlas.AtlasType.END];

    public INGUIAtlas GetAtlas(ManagerUIAtlas.AtlasType t)
    {
        if( (int)t < atlasObjects.Length)
        {
            return atlasObjects[(int)t] as INGUIAtlas;
        }
        
        return null;
    }
}
