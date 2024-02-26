using UnityEngine;

public class NoyBoostPack_UI : MonoBehaviour
{
    [Header("Atlas")]
    [SerializeField]
    private NGUIAtlas uiAtlas;
    
    [Header("Spine")]
    [SerializeField]
    private GameObject noySpineObj;
    
    public NGUIAtlas UIAtlas
    { get { return uiAtlas; } }
    public GameObject NoySpineObj
    { get { return noySpineObj; } }
}
