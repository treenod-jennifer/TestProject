using UnityEngine;

public class LuckyRoulettePack : MonoBehaviour
{
    [Header("Image")] 
    [SerializeField] private NGUIAtlas atlasUI;
    
    [Header("Spine")] 
    [SerializeField] private GameObject objSpine;
    public NGUIAtlas AtlasUI
    { get { return atlasUI; } }
    
    public GameObject ObjSpine
    { get { return objSpine; } }
}
