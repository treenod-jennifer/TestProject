using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAtlasPack : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("blockAtlas")]
    [SerializeField] private Object blockAtlasObj;
    [UnityEngine.Serialization.FormerlySerializedAs("block2Atlas")]
    [SerializeField] private Object block2AtlasObj;
    [UnityEngine.Serialization.FormerlySerializedAs("effectAtlas")]
    [SerializeField] private Object effectAtlasObj;
    [UnityEngine.Serialization.FormerlySerializedAs("ingameUIAtlas")]
    [SerializeField] private Object ingameUIAtlasObj;

    public INGUIAtlas blockAtlas
    {
        get
        {
            return blockAtlasObj as INGUIAtlas;
        }
    }
    public INGUIAtlas block2Atlas
    {
        get
        {
            return block2AtlasObj as INGUIAtlas;
        }
    }
    public INGUIAtlas effectAtlas
    {
        get
        {
            return effectAtlasObj as INGUIAtlas;
        }
    }
    public INGUIAtlas ingameUIAtlas
    {
        get
        {
            return ingameUIAtlasObj as INGUIAtlas;
        }
    }
}
