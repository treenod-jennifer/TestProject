using UnityEngine;

public class AntiqueStorePack : MonoBehaviour
{
    [Header("Image")] 
    [SerializeField] private NGUIAtlas atlasUI;
    [SerializeField] private Texture texDecoInfo;

    [Header("Spine")] 
    [SerializeField] private GameObject objGlassSpine;
    [SerializeField] private GameObject objTwinkleSpine;

    public NGUIAtlas AtlasUI
    { get { return atlasUI; } }

    public Texture TexDecoInfo
    { get { return texDecoInfo; } }

    public GameObject ObjGlassSpine
    { get { return objGlassSpine; } }

    public GameObject ObjTwinkleSpine
    { get { return objTwinkleSpine; } }
}
