using UnityEngine;

public class NoyBoostPack_Ingame : MonoBehaviour
{
    [Header("Atlas")]
    [SerializeField]
    private NGUIAtlas ingameAtlas;
    
    [Header("Spine")]
    [SerializeField]
    private GameObject noySpineObj;
    [SerializeField]
    private GameObject bombSpineObj;
    
    public NGUIAtlas IngameAtlas
    { get { return ingameAtlas; } }
    public GameObject NoySpineObj
    { get { return noySpineObj; } }
    public GameObject BombSpineObj
    { get { return bombSpineObj; } }
}
