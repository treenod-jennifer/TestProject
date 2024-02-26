using UnityEngine;

public class DiaStashPack : MonoBehaviour
{
    [Header("Atlas")] 
    [SerializeField] private NGUIAtlas atlasUI;

    public NGUIAtlas AtlasUI
    { get { return atlasUI; } }
}
