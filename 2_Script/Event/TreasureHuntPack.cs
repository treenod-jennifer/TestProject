using UnityEngine;

public class TreasureHuntPack : MonoBehaviour
{
    [Header("Atlas")]
    [SerializeField]
    private NGUIAtlas uiAtlas;
    [SerializeField]
    private NGUIAtlas ingameAtlas;
    
    [Header("Spine")]
    [SerializeField]
    private GameObject resultSpineObj;
    
    public NGUIAtlas UIAtlas
    { get { return uiAtlas; } }
    public NGUIAtlas IngameAtlas
    { get { return ingameAtlas; } }
    public GameObject ResultSpineObj
    { get { return resultSpineObj; } }
}
