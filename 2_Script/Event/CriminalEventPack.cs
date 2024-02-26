using Spine.Unity;
using UnityEngine;

public class CriminalEventPack : MonoBehaviour
{
    [Header("Image")]
    [SerializeField]
    private NGUIAtlas atlasUI;
    
    public NGUIAtlas AtlasUI
    { get { return atlasUI; } }
}
